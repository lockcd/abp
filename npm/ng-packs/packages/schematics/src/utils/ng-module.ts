import { SchematicsException, Tree } from '@angular-devkit/schematics';
import { getMainFilePath } from './angular/standalone/util';
import * as ts from 'typescript';
import { getAppModulePath, getDecoratorMetadata, getMetadataField } from './angular';
import { createSourceFile } from '../commands/change-theme/index';
import { normalize, Path } from '@angular-devkit/core';
import * as path from 'path';

export const hasImportInNgModule = async (
  host: Tree,
  projectName: string,
  metadataFn: string,
  metadataName = 'imports',
): Promise<boolean> => {
  const mainFilePath = await getMainFilePath(host, projectName);
  const appModulePath = getAppModulePath(host, mainFilePath);
  const buffer = host.read(appModulePath);

  if (!buffer) {
    throw new SchematicsException(`Could not read file: ${appModulePath}`);
  }

  const source = createSourceFile(host, appModulePath);

  console.log('AppModule content:\n', buffer.toString('utf-8'));

  // Get the NgModule decorator metadata
  const ngModuleDecorator = getDecoratorMetadata(source, 'NgModule', '@angular/core')[0];
  console.log(
    'Found NgModule decorators:',
    getDecoratorMetadata(source, 'NgModule', '@angular/core'),
  );
  if (!ngModuleDecorator) {
    throw new SchematicsException('The app module does not found');
  }

  const matchingProperties = getMetadataField(
    ngModuleDecorator as ts.ObjectLiteralExpression,
    metadataName,
  );
  const assignment = matchingProperties[0] as ts.PropertyAssignment;
  const assignmentInit = assignment.initializer as ts.ArrayLiteralExpression;

  const elements = assignmentInit.elements;
  if (!elements || elements.length < 1) {
    throw new SchematicsException(`Elements could not found: ${elements}`);
  }

  return elements.some(f => f.getText().match(metadataFn));
};

export async function findAppRoutesModulePath(
  tree: Tree,
  mainFilePath: string,
): Promise<Path | null> {
  const appModulePath = getAppModulePath(tree, mainFilePath);
  if (!appModulePath || !tree.exists(appModulePath)) return null;

  const buffer = tree.read(appModulePath);
  if (!buffer) return null;

  const source = ts.createSourceFile(
    appModulePath,
    buffer.toString('utf-8'),
    ts.ScriptTarget.Latest,
    true,
  );

  for (const stmt of source.statements) {
    if (!ts.isImportDeclaration(stmt)) continue;

    const importClause = stmt.importClause;
    if (!importClause?.namedBindings || !ts.isNamedImports(importClause.namedBindings)) continue;

    const isRoutesImport = importClause.namedBindings.elements.some(
      el => el.name.getText() === 'AppRoutingModule',
    );
    if (!isRoutesImport || !ts.isStringLiteral(stmt.moduleSpecifier)) continue;

    let importPath = stmt.moduleSpecifier.text;

    if (!importPath.endsWith('.ts')) {
      importPath += '.ts';
    }

    const configDir = path.dirname(appModulePath);
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
