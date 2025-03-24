using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Docs.Projects;

namespace Volo.Docs.Common.Documents;

[Authorize(DocsCommonPermissions.Documents.PdfGeneration)]
public class DocumentPdfGeneratorAppService : ApplicationService, IDocumentPdfGeneratorAppService
{
    private readonly IDocumentPdfGenerator _documentPdfGenerator;
    private readonly IProjectRepository _projectRepository;

    public DocumentPdfGeneratorAppService(
        IDocumentPdfGenerator documentPdfGenerator,
        IProjectRepository projectRepository)
    {
        _documentPdfGenerator = documentPdfGenerator;
        _projectRepository = projectRepository;
    }

    public async Task<IRemoteStreamContent> GeneratePdfAsync(DocumentPdfGeneratorInput input)
    {
        var project = await _projectRepository.GetAsync(input.ProjectId);
        return await _documentPdfGenerator.GenerateAsync(project, input.Version, input.LanguageCode);
    }
}