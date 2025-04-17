export interface GenerateLibSchema {
  /**
   * Angular package name will create
   */
  packageName: string;

  /**
   * İs the package a library or a library module
   */
  isSecondaryEntrypoint: boolean;
  /**
   * İs the package has standalone template
   */
  templateType: 'standalone' | 'module';

  override: boolean;

  target: string;
}
