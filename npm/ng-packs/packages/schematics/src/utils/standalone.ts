import { SchematicsException, Tree } from '@angular-devkit/schematics';
import { findBootstrapApplicationCall, getMainFilePath } from './angular/standalone/util';
import { findAppConfig } from './angular/standalone/app_config';
import * as ts from 'typescript';
import { normalize, Path } from '@angular-devkit/core';
import * as path from 'path';

export const getAppConfigPath = (host: Tree, mainFilePath: string): string => {
  const bootstrapCall = findBootstrapApplicationCall(host, mainFilePath);
  const appConfig = findAppConfig(bootstrapCall, host, mainFilePath);
  return appConfig?.filePath || '';
};

export function findAppRoutesPath(tree: Tree, mainFilePath: string): Path | null {
  const appConfigPath = getAppConfigPath(tree, mainFilePath);
  if (!appConfigPath || !tree.exists(appConfigPath)) return null;

  const buffer = tree.read(appConfigPath);
  if (!buffer) return null;

  const source = ts.createSourceFile(
    appConfigPath,
    buffer.toString('utf-8'),
    ts.ScriptTarget.Latest,
    true,
  );

  for (const stmt of source.statements) {
    if (!ts.isImportDeclaration(stmt)) continue;

    const importClause = stmt.importClause;
    if (!importClause?.namedBindings || !ts.isNamedImports(importClause.namedBindings)) continue;

    const isRoutesImport = importClause.namedBindings.elements.some(
      el => el.name.getText() === 'routes',
    );
    if (!isRoutesImport || !ts.isStringLiteral(stmt.moduleSpecifier)) continue;

    let importPath = stmt.moduleSpecifier.text;

    if (!importPath.endsWith('.ts')) {
      importPath += '.ts';
    }

    const configDir = path.dirname(appConfigPath);
    const resolvedFsPath = path.resolve(configDir, importPath);
    const workspaceRelativePath = path.relative(process.cwd(), resolvedFsPath).replace(/\\/g, '/');

    const normalizedPath = normalize(workspaceRelativePath);

    if (!tree.exists(normalizedPath)) {
      throw new Error(`Cannot find routes file: ${normalizedPath}`);
    }

    return normalizedPath;
  }

  return null;
}

export const hasProviderInStandaloneAppConfig = async (
  host: Tree,
  projectName: string,
  providerName: string,
): Promise<boolean> => {
  const mainFilePath = await getMainFilePath(host, projectName);
  const appConfigPath = getAppConfigPath(host, mainFilePath);
  const buffer = host.read(appConfigPath);

  if (!buffer) {
    throw new SchematicsException(`Could not read file: ${appConfigPath}`);
  }

  const source = ts.createSourceFile(
    appConfigPath,
    buffer.toString('utf-8'),
    ts.ScriptTarget.Latest,
    true,
  );
  const callExpressions = source.statements
    .flatMap(stmt => (ts.isVariableStatement(stmt) ? stmt.declarationList.declarations : []))
    .flatMap(decl =>
      decl.initializer && ts.isObjectLiteralExpression(decl.initializer)
        ? decl.initializer.properties
        : [],
    )
    .filter(ts.isPropertyAssignment)
    .filter(prop => prop.name.getText() === 'providers');

  if (callExpressions.length === 0) return false;

  const providersArray = callExpressions[0].initializer as ts.ArrayLiteralExpression;
  return providersArray.elements.some(el => el.getText().includes(providerName));
};
