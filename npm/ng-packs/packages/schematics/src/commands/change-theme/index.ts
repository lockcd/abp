import { JsonArray, JsonValue } from '@angular-devkit/core';
import { Rule, SchematicsException, Tree, chain } from '@angular-devkit/schematics';
import { ProjectDefinition } from '@angular-devkit/core/src/workspace';
import * as ts from 'typescript';
import { allStyles, importMap, styleMap } from './style-map';
import { ChangeThemeOptions } from './model';
import {
  addRootImport,
  addRootProvider,
  getAppModulePath,
  isLibrary,
  isStandaloneApp,
  updateWorkspace,
  WorkspaceDefinition,
} from '../../utils';
import { ThemeOptionsEnum } from './theme-options.enum';
import { findNodes, getDecoratorMetadata, getMetadataField } from '../../utils/angular/ast-utils';
import { findBootstrapApplicationCall, getMainFilePath } from '../../utils/angular/standalone/util';
import { findAppConfig } from '../../utils/angular/standalone/app_config';

export default function (_options: ChangeThemeOptions): Rule {
  return async () => {
    const targetThemeName = _options.name;
    const selectedProject = _options.targetProject;

    if (!targetThemeName) {
      throw new SchematicsException('The theme name does not selected');
    }

    return chain([
      updateWorkspace(storedWorkspace => {
        updateProjectStyle(selectedProject, storedWorkspace, targetThemeName);
      }),
      updateAppModule(selectedProject, targetThemeName),
    ]);
  };
}

function updateProjectStyle(
  projectName: string,
  workspace: WorkspaceDefinition,
  targetThemeName: ThemeOptionsEnum,
) {
  const project = workspace.projects.get(projectName);

  if (!project) {
    throw new SchematicsException('The target project does not selected');
  }

  if (isLibrary(project)) {
    throw new SchematicsException('The library project does not supported');
  }

  const targetOption = getProjectTargetOptions(project, 'build');
  const styles = targetOption.styles as (string | { input: string })[];

  const sanitizedStyles = removeThemeBasedStyles(styles);

  const newStyles = styleMap.get(targetThemeName);
  if (!newStyles) {
    throw new SchematicsException('The theme does not found');
  }
  targetOption.styles = [...newStyles, ...sanitizedStyles] as JsonArray;
}

function updateAppModule(selectedProject: string, targetThemeName: ThemeOptionsEnum): Rule {
  return async (host: Tree) => {
    const mainFilePath = await getMainFilePath(host, selectedProject);
    const isStandalone = isStandaloneApp(host, mainFilePath);
    const appModulePath = isStandalone
      ? getAppConfigPath(host, mainFilePath)
      : getAppModulePath(host, mainFilePath);

    return chain([
      removeImportPath(appModulePath, targetThemeName),
      ...(!isStandalone ? [removeImportFromNgModuleMetadata(appModulePath, targetThemeName)] : []),
      isStandalone
        ? removeImportsFromStandaloneProviders(appModulePath, targetThemeName)
        : removeProviderFromNgModuleMetadata(appModulePath, targetThemeName),
      insertImports(selectedProject, targetThemeName),
      insertProviders(selectedProject, targetThemeName),
    ]);
  };
}

export function removeImportPath(filePath: string, selectedTheme: ThemeOptionsEnum): Rule {
  return (host: Tree) => {
    const buffer = host.read(filePath);
    if (!buffer) return host;

    const sourceText = buffer.toString('utf-8');
    const source = ts.createSourceFile(filePath, sourceText, ts.ScriptTarget.Latest, true);
    const recorder = host.beginUpdate(filePath);

    const impMap = getImportPaths(selectedTheme, true); // all theme-related imports

    const nodes = findNodes(source, ts.isImportDeclaration);

    const filteredNodes = nodes.filter(node => {
      const importPath = (node.moduleSpecifier as ts.StringLiteral).text;
      const namedBindings = node.importClause?.namedBindings;

      return impMap.some(({ path, importName }) => {
        const symbol = importName.split('.')[0]; // Ex: ThemeXModule from ThemeXModule.forRoot()
        const matchesPath = !!path && importPath === path;

        const matchesSymbol =
          !!namedBindings &&
          ts.isNamedImports(namedBindings) &&
          namedBindings.elements.some(e => e.name.text === symbol);

        return matchesPath || matchesSymbol;
      });
    });

    for (const node of filteredNodes) {
      recorder.remove(node.getStart(), node.getWidth());
    }

    host.commitUpdate(recorder);
    return host;
  };
}

export function removeImportFromNgModuleMetadata(
  appModulePath: string,
  selectedTheme: ThemeOptionsEnum,
): Rule {
  return (host: Tree) => {
    const recorder = host.beginUpdate(appModulePath);
    const source = createSourceFile(host, appModulePath);
    const impMap = getImportPaths(selectedTheme, true);

    const node = getDecoratorMetadata(source, 'NgModule', '@angular/core')[0] || {};
    if (!node) {
      throw new SchematicsException('The app module does not found');
    }

    const matchingProperties = getMetadataField(node as ts.ObjectLiteralExpression, 'imports');
    const assignment = matchingProperties[0] as ts.PropertyAssignment;
    const assignmentInit = assignment.initializer as ts.ArrayLiteralExpression;

    const elements = assignmentInit.elements;
    if (!elements || elements.length < 1) {
      throw new SchematicsException(`Elements could not found: ${elements}`);
    }

    const filteredElements = elements.filter(f =>
      impMap.some(s => f.getText().match(s.importName)),
    );

    if (!filteredElements || filteredElements.length < 1) {
      return;
    }

    filteredElements.map(willRemoveModule =>
      recorder.remove(willRemoveModule.getStart(), willRemoveModule.getWidth() + 1),
    );
    host.commitUpdate(recorder);
    return host;
  };
}

export function removeImportsFromStandaloneProviders(
  mainPath: string,
  selectedTheme: ThemeOptionsEnum,
): Rule {
  return (host: Tree) => {
    const buffer = host.read(mainPath);
    if (!buffer) return host;

    const sourceText = buffer.toString('utf-8');
    const source = ts.createSourceFile(mainPath, sourceText, ts.ScriptTarget.Latest, true);
    console.log('remove from standalone main path --->>>>', mainPath);
    const recorder = host.beginUpdate(mainPath);

    const impMap = getImportPaths(selectedTheme, true);

    const callExpressions = findNodes(source, ts.isCallExpression);

    for (const expr of callExpressions) {
      const text = expr.getText();

      const match = impMap.find(({ importName }) => text.includes(importName.split('.')[0]));

      if (match) {
        recorder.remove(expr.getStart(), expr.getWidth() + 1);
      }
    }

    host.commitUpdate(recorder);
    return host;
  };
}

export function removeProviderFromNgModuleMetadata(
  appModulePath: string,
  selectedTheme: ThemeOptionsEnum,
): Rule {
  return (host: Tree) => {
    const recorder = host.beginUpdate(appModulePath);
    const source = createSourceFile(host, appModulePath);
    const impMap = getImportPaths(selectedTheme, true);

    const node = getDecoratorMetadata(source, 'NgModule', '@angular/core')[0] || {};
    if (!node) {
      throw new SchematicsException('The app module does not found');
    }

    const matchingProperties = getMetadataField(node as ts.ObjectLiteralExpression, 'providers');
    const assignment = matchingProperties[0] as ts.PropertyAssignment;
    const assignmentInit = assignment.initializer as ts.ArrayLiteralExpression;

    const elements = assignmentInit.elements;
    if (!elements || elements.length < 1) {
      throw new SchematicsException(`Elements could not found: ${elements}`);
    }

    const filteredElements = elements.filter(f =>
      impMap.filter(f => !!f.provider).some(s => f.getText().match(s.provider!)),
    );

    if (!filteredElements || filteredElements.length < 1) {
      return;
    }

    filteredElements.map(willRemoveModule => {
      recorder.remove(willRemoveModule.getStart(), willRemoveModule.getWidth());
    });
    host.commitUpdate(recorder);
    return host;
  };
}

export function insertImports(projectName: string, selectedTheme: ThemeOptionsEnum): Rule {
  return addRootImport(projectName, code => {
    const selected = importMap.get(selectedTheme);
    if (!selected || selected.length === 0) return code.code``;

    const expressions: string[] = [];

    for (const { importName, path, expression } of selected) {
      const imported = code.external(importName, path);
      expressions.push(expression ?? imported); // default fallback
    }

    return code.code`${expressions.join(',\n')}`;
  });
}

export function insertProviders(projectName: string, selectedTheme: ThemeOptionsEnum): Rule {
  return addRootProvider(projectName, code => {
    const selected = importMap.get(selectedTheme);
    if (!selected || selected.length === 0) return code.code``;

    const expressions: string[] = [];

    for (const { path, provider } of selected) {
      if (!provider) {
        continue;
      }
      const imported = code.external(provider, path);
      expressions.push(`${imported}()`);
    }

    return code.code`${expressions.join(',\n')}`;
  });
}

export function createSourceFile(host: Tree, appModulePath: string): ts.SourceFile {
  const buffer = host.read(appModulePath);
  if (!buffer || buffer.length === 0) {
    throw new SchematicsException(`${appModulePath} file could not be read.`);
  }

  const sourceText = buffer.toString('utf-8');

  return ts.createSourceFile(
    appModulePath,
    sourceText,
    ts.ScriptTarget.Latest,
    true,
    ts.ScriptKind.TS,
  );
}

/**
 * Returns all import paths except the selected theme
 * @param selectedTheme The selected theme
 * @param getAll If true, returns all import paths
 */
export function getImportPaths(selectedTheme: ThemeOptionsEnum, getAll = false) {
  if (getAll) {
    return Array.from(importMap.values()).reduce((acc, val) => [...acc, ...val], []);
  }

  return Array.from(importMap.values())
    .filter(f => f !== importMap.get(selectedTheme))
    .reduce((acc, val) => [...acc, ...val], []);
}

export function getProjectTargetOptions(
  project: ProjectDefinition,
  buildTarget: string,
): Record<string, JsonValue | undefined> {
  const options = project.targets?.get(buildTarget)?.options;

  if (!options) {
    throw new SchematicsException(
      `Cannot determine project target configuration for: ${buildTarget}.`,
    );
  }

  return options;
}

export function removeThemeBasedStyles(styles: (string | object)[]) {
  return styles.filter(s => !allStyles.some(x => styleCompareFn(s, x)));
}

export const styleCompareFn = (item1: string | object, item2: string | object) => {
  const type1 = typeof item1;
  const type2 = typeof item1;

  if (type1 !== type2) {
    return false;
  }

  if (type1 === 'string') {
    return item1 === item2;
  }
  const o1 = item1 as { bundleName?: string };
  const o2 = item2 as { bundleName?: string };

  return o1.bundleName && o2.bundleName && o1.bundleName == o2.bundleName;
};

export const getAppConfigPath = (host: Tree, mainFilePath: string): string => {
  const bootstrapCall = findBootstrapApplicationCall(host, mainFilePath);
  const appConfig = findAppConfig(bootstrapCall, host, mainFilePath);
  return appConfig?.filePath || '';
};
