using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Docs.Documents;
using Volo.Docs.Projects;
using Volo.Extensions;

namespace Volo.Docs.Common.Documents;

public class DocumentPdfGeneratorAppService : ApplicationService, IDocumentPdfGeneratorAppService
{
    protected IBlobContainer<DocsDocumentPdfContainer> BlobContainer { get; }
    protected IProjectRepository ProjectRepository { get; }
    protected IDocumentSourceFactory DocumentStoreFactory { get; }
    protected IDocumentRepository DocumentRepository { get; }
    
    public DocumentPdfGeneratorAppService(
        IBlobContainer<DocsDocumentPdfContainer> blobContainer,
        IProjectRepository projectRepository,
        IDocumentSourceFactory documentStoreFactory,
        IDocumentRepository documentRepository)
    {
        BlobContainer = blobContainer;
        ProjectRepository = projectRepository;
        DocumentStoreFactory = documentStoreFactory;
        DocumentRepository = documentRepository;
    }
    
    public virtual async Task<IRemoteStreamContent> GeneratePdfAsync(DocumentPdfGeneratorInput input)
    {
       var project = await ProjectRepository.GetAsync(input.ProjectId);
       var fileName = CalculateDocumentPdfFileName(project.Name, input.Version, input.LanguageCode);
       var stream = await BlobContainer.GetOrNullAsync(fileName);

       if(stream != null)
       {
          return new RemoteStreamContent(stream, fileName , "application/pdf");
       }

       var documentStore = DocumentStoreFactory.Create(project.DocumentStoreType);
       var navigation = await GetNavigationAsync(documentStore, project, input.LanguageCode, input.Version);
       
       
    }
    
    protected virtual string CalculateDocumentPdfFileName(string projectName, string version, string languageCode)
    {
        return $"{projectName.ToLower()}-{version.ToLower()}--{languageCode.ToLower()}";
    }
    
    private async Task<NavigationNode> GetNavigationAsync(
        IDocumentSource documentStore,
        Project project,
        string languageCode,
        string version)
    {
        var navigationDocument = await GetDocumentAsync(documentStore, project, project.NavigationDocumentName, languageCode, version);
       
        if (!DocsJsonSerializerHelper.TryDeserialize<NavigationNode>(navigationDocument.Content, out var navigation))
        {
            throw new UserFriendlyException($"Cannot validate navigation file '{project.NavigationDocumentName}' for the project {project.Name}.");
        }
        
        return navigation;
    }
    
    private async Task<Document> GetDocumentAsync(
        IDocumentSource documentStore,
        Project project, 
        string documentName, 
        string languageCode,
        string version)
    {
        version = string.IsNullOrWhiteSpace(version) ? project.LatestVersionBranchName : version;
        var document = await DocumentRepository.FindAsync(project.Id, documentName, version, languageCode);

        if (document != null)
        {
            return document;
        }

        document = await documentStore.GetDocumentAsync(project, documentName, languageCode, version);

        return document;
    }
}