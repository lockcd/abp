using System;

namespace Volo.Docs.Admin.Projects;

public class DeletePdfFileInput
{
    public Guid Id { get; set; }
    
    public string Version { get; set; }
    
    public string LanguageCode { get; set; }
}