using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Docs.Documents.Pdf;
using Volo.Docs.Projects;

namespace Volo.Docs.Common.Documents;

[Authorize(DocsCommonPermissions.Documents.PdfGeneration)]
public class DocumentPdfGeneratorAppService : ApplicationService, IDocumentPdfGeneratorAppService
{
    protected IDocumentPdfGenerator DocumentPdfGenerator { get; }
    protected IProjectRepository ProjectRepository { get; }
    
    public DocumentPdfGeneratorAppService(
        IDocumentPdfGenerator documentPdfGenerator,
        IProjectRepository projectRepository)
    {
        DocumentPdfGenerator = documentPdfGenerator;
        ProjectRepository = projectRepository;
    }

    public virtual async Task<IRemoteStreamContent> GeneratePdfAsync(DocumentPdfGeneratorInput input)
    {
        var project = await ProjectRepository.GetAsync(input.ProjectId);
        return await DocumentPdfGenerator.GenerateAsync(project, input.Version, input.LanguageCode);
    }
}