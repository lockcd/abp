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
      importConfigModuleToDefaultProjectAppModule(workspace, packageName, options),
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
  workspace: WorkspaceDefinition,
  packageName: string,
  options: GenerateLibSchema,
) {
  return (tree: Tree) => {
    // const projectName = readWorkspaceSchema(tree).defaultProject || getFirstApplication(tree).name!;
    const projectName = getFirstApplication(tree).name!;
    const project = workspace.projects.get(projectName);
    const sourceRoot = project?.sourceRoot || 'src';

    const isSourceStandalone = isStandaloneApp(tree, `${sourceRoot}/main.ts`);
    console.log('isStandalone --->>>>', isSourceStandalone);
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

    // const appModulePath = `${project?.sourceRoot}/app/app.module.ts`;
    // const appModule = tree.read(appModulePath);
    // if (!appModule) {
    //  return;
    // }
    // const appModuleContent = appModule.toString();
    /*    if (
      appModuleContent.includes(
        options.isStandaloneTemplate
          ? `provide${pascal(packageName)}Config`
          : `${camel(packageName)}ConfigModule`,
      )
    ) {
      return;
    }*/

    /*    const rootConfigStatement = options.isStandaloneTemplate
      ? `provide${pascal(packageName)}Config()`
      : `${pascal(packageName)}ConfigModule.forRoot()`;
    const text = tree.read(appModulePath);
    if (!text) {
      return;
    }
    const sourceText = text.toString();
    if (sourceText.includes(rootConfigStatement)) {
      return;
    }
    const source = ts.createSourceFile(appModulePath, sourceText, ts.ScriptTarget.Latest, true);

    const changes = addImportToModule(
      source,
      appModulePath,
      rootConfigStatement,
      `${kebab(packageName)}/config`,
    );
    const recorder = tree.beginUpdate(appModulePath);
    for (const change of changes) {
      if (change instanceof InsertChange) {
        recorder.insertLeft(change.pos, change.toAdd);
      }
    }
    tree.commitUpdate(recorder);*/

    return chain(rules);
  };
}

export function addRoutingToAppRoutingModule(
  workspace: WorkspaceDefinition,
  packageName: string,
  options: GenerateLibSchema,
): Rule {
  return (tree: Tree) => {
    // const projectName = readWorkspaceSchema(tree).defaultProject || getFirstApplication(tree).name!;
    const projectName = getFirstApplication(tree).name!;
    const project = workspace.projects.get(projectName);
    const sourceRoot = project?.sourceRoot || 'src';

    const mainPath = `${sourceRoot}/main.ts`;
    const isSourceStandalone = isStandaloneApp(tree, mainPath);
    const pascalName = pascal(packageName);
    const routePath = `${kebab(packageName)}`;

    if (isSourceStandalone) {
      return addRootProvider(projectName, code => {
        const routeExpr =
          options.templateType === 'standalone'
            ? `() => import('${routePath}').then(m => m.${pascalName}Routes)`
            : `() => import('${routePath}').then(m => m.${moduleName}.forLazy())`;

        return code.code`provideRouter([
            { path: '${routePath}', loadChildren: ${routeExpr} }
        ])`;
      });
    }

    const appRoutingModulePath = `${project?.sourceRoot}/app/app-routing.module.ts`;
    const appRoutingModule = tree.read(appRoutingModulePath);
    if (!appRoutingModule) {
      return;
    }
    const appRoutingModuleContent = appRoutingModule.toString();
    const moduleName = `${pascalName}Module`;
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

    return;
  };
}
