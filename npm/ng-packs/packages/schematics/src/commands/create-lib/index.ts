import {
  applyTemplates,
  chain,
  move,
  noop,
  Rule,
  SchematicsException,
  Tree,
  url,
} from '@angular-devkit/schematics';
import * as ts from 'typescript';

import { join, normalize } from '@angular-devkit/core';
import {
  addRootImport,
  addRootProvider,
  addRouteDeclarationToModule,
  applyWithOverwrite,
  getFirstApplication,
  getWorkspace,
  InsertChange,
  interpolate,
  isLibrary,
  isStandaloneApp,
  JSONFile,
  kebab,
  pascal,
  resolveProject,
  updateWorkspace,
} from '../../utils';
import { ProjectDefinition, WorkspaceDefinition } from '../../utils/angular/workspace';
import { addLibToWorkspaceFile } from '../../utils/angular-schematic/generate-lib';
import * as cases from '../../utils/text';
import { Exception } from '../../enums/exception';
import { GenerateLibSchema } from './models/generate-lib-schema';
import { getMainFilePath } from '../../utils/angular/standalone/util';

export default function (schema: GenerateLibSchema) {
  return async (tree: Tree) => {
    if (schema.override || !(await checkLibExist(schema, tree))) {
      return chain([createLibrary(schema)]);
    }
  };
}

async function checkLibExist(options: GenerateLibSchema, tree: Tree) {
  const packageName = kebab(options.packageName);
  if (options.isSecondaryEntrypoint) {
    const lib = await resolveProject(tree, options.target);
    const ngPackagePath = `${lib?.definition.root}/${packageName}/ng-package.json`;
    const packageInfo = tree.read(ngPackagePath);
    if (packageInfo) {
      throw new SchematicsException(
        interpolate(Exception.LibraryAlreadyExists, `${lib.name}/${packageName}`),
      );
    }
    return false;
  }

  const target = await resolveProject(tree, options.packageName, null);
  if (target) {
    throw new SchematicsException(interpolate(Exception.LibraryAlreadyExists, packageName));
  }
  return false;
}

function createLibrary(options: GenerateLibSchema): Rule {
  return async (tree: Tree) => {
    const target = await resolveProject(tree, options.packageName, null);
    if (!target || options.override) {
      if (options.isSecondaryEntrypoint) {
        if (options.templateType === 'standalone') {
          return createLibSecondaryEntryWithStandaloneTemplate(tree, options);
        } else {
          return createLibSecondaryEntry(tree, options);
        }
      }
      if (options.templateType === 'module') {
        return createLibFromModuleTemplate(tree, options);
      } else {
        return createLibFromModuleStandaloneTemplate(tree, options);
      }
    } else {
      throw new SchematicsException(
        interpolate(Exception.LibraryAlreadyExists, options.packageName),
      );
    }
  };
}
async function resolvePackagesDirFromAngularJson(host: Tree) {
  const workspace = await getWorkspace(host);
  const projectFolder = readFirstLibInAngularJson(workspace);
  return projectFolder?.root?.split('/')?.[0] || 'projects';
}

function readFirstLibInAngularJson(workspace: WorkspaceDefinition): ProjectDefinition | undefined {
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const library = <ProjectDefinition | undefined>(
    Array.from(workspace.projects.values()).find((value: ProjectDefinition) => isLibrary(value))
  );

  return library;
}

async function createLibFromModuleTemplate(tree: Tree, options: GenerateLibSchema) {
  const packagesDir = await resolvePackagesDirFromAngularJson(tree);
  const packageJson = JSON.parse(tree.read('./package.json')!.toString());
  const abpVersion = packageJson.dependencies['@abp/ng.core'];

  return chain([
    applyWithOverwrite(url('./files-package'), [
      applyTemplates({
        ...cases,
        libraryName: options.packageName,
        abpVersion,
      }),
      move(normalize(packagesDir)),
    ]),
    addLibToWorkspaceIfNotExist(options, packagesDir),
  ]);
}

async function createLibFromModuleStandaloneTemplate(tree: Tree, options: GenerateLibSchema) {
  const packagesDir = await resolvePackagesDirFromAngularJson(tree);
  const packageJson = JSON.parse(tree.read('./package.json')!.toString());
  const abpVersion = packageJson.dependencies['@abp/ng.core'];

  return chain([
    applyWithOverwrite(url('./files-package-standalone'), [
      applyTemplates({
        ...cases,
        libraryName: options.packageName,
        abpVersion,
      }),
      move(normalize(packagesDir)),
    ]),
    addLibToWorkspaceIfNotExist(options, packagesDir),
  ]);
}

export function addLibToWorkspaceIfNotExist(options: GenerateLibSchema, packagesDir: string): Rule {
  return async (tree: Tree) => {
    const workspace = await getWorkspace(tree);
    const packageName = kebab(options.packageName);
    const isProjectExist = workspace.projects.has(packageName);

    const projectRoot = join(normalize(packagesDir), packageName);
    const pathImportLib = `${packagesDir}/${packageName}`;

    return chain([
      isProjectExist
        ? updateWorkspace(w => {
            w.projects.delete(packageName);
          })
        : noop(),
      addLibToWorkspaceFile(projectRoot, packageName),
      updateTsConfig(packageName, pathImportLib),
      importConfigModuleToDefaultProjectAppModule(packageName, options),
      addRoutingToAppRoutingModule(workspace, packageName, options),
    ]);
  };
}

export function updateTsConfig(packageName: string, path: string) {
  return (host: Tree) => {
    const files = ['tsconfig.json', 'tsconfig.app.json', 'tsconfig.base.json'];
    const tsConfig = files.find(f => host.exists(f));
    if (!tsConfig) {
      return host;
    }

    const file = new JSONFile(host, tsConfig);
    const jsonPath = ['compilerOptions', 'paths', packageName];
    const jsonPathConfig = ['compilerOptions', 'paths', `${packageName}/config`];
    file.modify(jsonPath, [`${path}/src/public-api.ts`]);
    file.modify(jsonPathConfig, [`${path}/config/src/public-api.ts`]);
  };
}

export async function createLibSecondaryEntry(tree: Tree, options: GenerateLibSchema) {
  const targetLib = await resolveProject(tree, options.target);
  const packageName = `${kebab(targetLib.name)}/${kebab(options.packageName)}`;
  const importPath = `${targetLib.definition.root}/${kebab(options.packageName)}`;
  return chain([
    applyWithOverwrite(url('./files-secondary-entrypoint'), [
      applyTemplates({
        ...cases,
        libraryName: options.packageName,
        target: targetLib.name,
      }),
      move(normalize(targetLib.definition.root)),
      updateTsConfig(packageName, importPath),
    ]),
  ]);
}

export async function createLibSecondaryEntryWithStandaloneTemplate(
  tree: Tree,
  options: GenerateLibSchema,
) {
  const targetLib = await resolveProject(tree, options.target);
  const packageName = `${kebab(targetLib.name)}/${kebab(options.packageName)}`;
  const importPath = `${targetLib.definition.root}/${kebab(options.packageName)}`;
  return chain([
    applyWithOverwrite(url('./files-secondary-entrypoint-standalone'), [
      applyTemplates({
        ...cases,
        libraryName: options.packageName,
        target: targetLib.name,
      }),
      move(normalize(targetLib.definition.root)),
      updateTsConfig(packageName, importPath),
    ]),
  ]);
}

export function importConfigModuleToDefaultProjectAppModule(
  packageName: string,
  options: GenerateLibSchema,
) {
  return (tree: Tree) => {
    const projectName = getFirstApplication(tree).name!;
    const rules: Rule[] = [];

    if (options.templateType === 'standalone') {
      rules.push(
        addRootProvider(projectName, code => {
          const configFn = code.external(
            `provide${pascal(packageName)}Config`,
            `${kebab(packageName)}/config`,
          );
          return code.code`${configFn}()`;
        }),
      );
    } else {
      rules.push(
        addRootImport(projectName, code => {
          const configFn = code.external(
            `${pascal(packageName)}ConfigModule`,
            `${kebab(packageName)}/config`,
          );
          return code.code`${configFn}.forRoot()`;
        }),
      );
    }
    return chain(rules);
  };
}

export function addRoutingToAppRoutingModule(
  workspace: WorkspaceDefinition,
  packageName: string,
  options: GenerateLibSchema,
): Rule {
  return async (tree: Tree) => {
    const projectName = getFirstApplication(tree).name!;
    const project = workspace.projects.get(projectName);
    console.log('project --->>>', project);
    const mainFilePath = await getMainFilePath(tree, projectName);
    console.log('main file path ---->>>>', mainFilePath);
    const isSourceStandalone = isStandaloneApp(tree, mainFilePath);
    console.log('isSourceStandalone ---->>>>', isSourceStandalone);

    const pascalName = pascal(packageName);
    const routePath = `${kebab(packageName)}`;
    const moduleName = `${pascalName}Module`;

    if (isSourceStandalone) {
      const appRoutesPath = `${project?.sourceRoot}/app/app.routes.ts`;
      const buffer = tree.read(appRoutesPath);
      if (!buffer) {
        throw new SchematicsException(`Cannot find routes file: ${appRoutesPath}`);
      }

      const content = buffer.toString();
      const source = ts.createSourceFile(appRoutesPath, content, ts.ScriptTarget.Latest, true);
      const routeExpr =
        options.templateType === 'standalone'
          ? `() => import('${routePath}').then(m => m.${pascalName}Routes)`
          : `() => import('${routePath}').then(m => m.${moduleName}.forLazy())`;
      const routeToAdd = `{ path: '${routePath}', loadChildren: ${routeExpr} }`;
      const change = addRouteToRoutesArray(source, 'routes', routeToAdd);

      if (change instanceof InsertChange) {
        const recorder = tree.beginUpdate(appRoutesPath);
        recorder.insertLeft(change.pos, change.toAdd);
        tree.commitUpdate(recorder);
      }
    } else {
      const appRoutingModulePath = `${project?.sourceRoot}/app/app-routing.module.ts`;
      const appRoutingModule = tree.read(appRoutingModulePath);
      if (!appRoutingModule) {
        return;
      }
      const appRoutingModuleContent = appRoutingModule.toString();
      if (appRoutingModuleContent.includes(moduleName)) {
        return;
      }

      const source = ts.createSourceFile(
        appRoutingModulePath,
        appRoutingModuleContent,
        ts.ScriptTarget.Latest,
        true,
      );
      const importStatement =
        options.templateType === 'standalone'
          ? `() => import('${routePath}').then(m => m.${pascalName}Routes)`
          : `() => import('${routePath}').then(m => m.${moduleName}.forLazy())`;
      const routeDefinition = `{ path: '${routePath}', loadChildren: ${importStatement} }`;
      const change = addRouteDeclarationToModule(source, routePath, routeDefinition);

      const recorder = tree.beginUpdate(appRoutingModulePath);
      if (change instanceof InsertChange) {
        recorder.insertLeft(change.pos, change.toAdd);
      }
      tree.commitUpdate(recorder);
    }
    return;
  };
}

export function addRouteToRoutesArray(
  source: ts.SourceFile,
  arrayName: string,
  routeToAdd: string,
): InsertChange | null {
  const routesVar = source.statements.find(
    stmt =>
      ts.isVariableStatement(stmt) &&
      stmt.declarationList.declarations.some(
        decl =>
          ts.isVariableDeclaration(decl) &&
          decl.name.getText() === arrayName &&
          decl.initializer !== undefined &&
          ts.isArrayLiteralExpression(decl.initializer),
      ),
  );

  if (!routesVar || !ts.isVariableStatement(routesVar)) {
    throw new Error(`Could not find routes array named "${arrayName}".`);
  }

  const declaration = routesVar.declarationList.declarations.find(
    decl => decl.name.getText() === arrayName,
  ) as ts.VariableDeclaration;

  const arrayLiteral = declaration.initializer as ts.ArrayLiteralExpression;

  const getPathValue = (routeText: string): string | null => {
    const match = routeText.match(/path:\s*['"`](.+?)['"`]/);
    return match?.[1] ?? null;
  };

  const newPath = getPathValue(routeToAdd);

  const alreadyExists = arrayLiteral.elements.some(el => {
    const existingPath = getPathValue(el.getText());
    return existingPath === newPath;
  });

  if (alreadyExists) {
    return null;
  }

  const insertPos =
    arrayLiteral.elements.hasTrailingComma || arrayLiteral.elements.length === 0
      ? arrayLiteral.getEnd() - 1
      : arrayLiteral.elements[arrayLiteral.elements.length - 1].getEnd();

  const prefix = arrayLiteral.elements.length > 0 ? ',\n  ' : '  ';
  const toAdd = `${prefix}${routeToAdd}`;

  return new InsertChange(source.fileName, insertPos, toAdd);
}
