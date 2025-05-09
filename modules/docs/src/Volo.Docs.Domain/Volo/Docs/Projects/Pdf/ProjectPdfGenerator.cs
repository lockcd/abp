using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;
using Volo.Docs.Documents;
using Volo.Docs.Documents.Rendering;
using Volo.Docs.HtmlConverting;
using Volo.Extensions;

namespace Volo.Docs.Projects.Pdf;

public class ProjectPdfGenerator : IProjectPdfGenerator, ITransientDependency
{
    protected IDocumentSourceFactory DocumentStoreFactory { get; }
    protected IDocumentToHtmlConverterFactory DocumentToHtmlConverterFactory { get; }
    protected IDocumentRepository DocumentRepository { get; }
    protected IDocumentSectionRenderer DocumentSectionRenderer { get; }
    protected IOptions<DocsProjectPdfGeneratorOptions> Options { get; }  
    protected IProjectPdfFileStore ProjectPdfFileStore { get; }
    protected IHtmlToPdfRenderer HtmlToPdfRenderer { get; }
    protected ILogger<ProjectPdfGenerator> Logger { get; set; }
    protected IDocumentSource DocumentSource { get; set; }
    protected DocumentParams DocumentParams { get; set; }
    protected Project Project { get; set; }
    protected List<PdfDocument> AllPdfDocuments { get; } = [];

    public ProjectPdfGenerator(
        IDocumentSourceFactory documentStoreFactory, 
        IDocumentRepository documentRepository,
        IOptions<DocsProjectPdfGeneratorOptions> options,
        IDocumentSectionRenderer documentSectionRenderer,
        IProjectPdfFileStore projectPdfFileStore, 
        IHtmlToPdfRenderer htmlToPdfRenderer, 
        IDocumentToHtmlConverterFactory documentToHtmlConverterFactory)
    {
        DocumentStoreFactory = documentStoreFactory;
        DocumentRepository = documentRepository;
        Options = options;
        DocumentSectionRenderer = documentSectionRenderer;
        ProjectPdfFileStore = projectPdfFileStore;
        HtmlToPdfRenderer = htmlToPdfRenderer;
        DocumentToHtmlConverterFactory = documentToHtmlConverterFactory;
        Logger = NullLogger<ProjectPdfGenerator>.Instance;
    }
    
    public virtual async Task<IRemoteStreamContent> GenerateAsync(Project project, string version, string languageCode)
    {
        var fileName = Options.Value.CalculatePdfFileName(project, version, languageCode);
        var fileStream = await ProjectPdfFileStore.GetOrNullAsync(project, version, languageCode);
        
        if (fileStream != null)
        {
            return new RemoteStreamContent(fileStream, fileName, "application/pdf");
        }
        
        Project = project;
        DocumentSource = DocumentStoreFactory.Create(project.DocumentStoreType);
        DocumentParams = await GetDocumentParamsAsync(project, version, languageCode);
        
        var navigation = await GetNavigationAsync(project, version, languageCode);
        await SetAllPdfDocumentsAsync(navigation.Items, project, version, languageCode);
        
        var html = await BuildHtmlAsync();
        var title = Options.Value.CalculatePdfFileTitle?.Invoke(project) ?? project.Name;
        var pdfStream = await HtmlToPdfRenderer.RenderAsync(title, html, AllPdfDocuments);
        
        await ProjectPdfFileStore.SetAsync(project, version, languageCode, pdfStream);
    
        return new RemoteStreamContent(pdfStream, fileName, "application/pdf");
    }
    
    protected virtual async Task<string> BuildHtmlAsync()
    {
        var htmlContent = await ConvertDocumentsToHtmlAsync(AllPdfDocuments);
        
        var htmlBuilder = new StringBuilder();
        htmlBuilder.Append(Options.Value.HtmlLayout);
        htmlBuilder.Replace(DocsProjectPdfGeneratorOptions.StylePlaceholder, Options.Value.HtmlStyle);
        htmlBuilder.Replace(DocsProjectPdfGeneratorOptions.ContentPlaceholder, htmlContent);
        
        return htmlBuilder.ToString();
    }
    
    protected virtual async Task<string> ConvertDocumentsToHtmlAsync(List<PdfDocument> pdfDocuments)
    {
        var contentBuilder = new StringBuilder();

        foreach (var pdfDocument in pdfDocuments)
        {
            if (pdfDocument.Document != null)
            {
                var renderedDocument = await RenderDocumentAsync(pdfDocument);
                var documentToHtmlConverter = GetDocumentToHtmlConverter(Project, pdfDocument);
                var htmlContent = documentToHtmlConverter.Convert(new PdfDocumentToHtmlConverterContext(renderedDocument, pdfDocument, DocumentParams));
                contentBuilder.AppendLine(htmlContent);
            }

            if (pdfDocument.HasChildren)
            {
                contentBuilder.AppendLine(await ConvertDocumentsToHtmlAsync(pdfDocument.Children));
            }
        }
        
        return contentBuilder.ToString();
    }

    protected virtual IDocumentToHtmlConverter<PdfDocumentToHtmlConverterContext> GetDocumentToHtmlConverter(Project project, PdfDocument pdfDocument)
    {
        return DocumentToHtmlConverterFactory.Create<PdfDocumentToHtmlConverterContext>(DocsDomainConsts.PdfDocumentToHtmlConverterPrefix +(pdfDocument.Document.Format ?? project.Format));
    }
    
    protected virtual async Task<string> RenderDocumentAsync(PdfDocument pdfDocument)
    {
        return await DocumentSectionRenderer.RenderAsync(pdfDocument.Document.Content, pdfDocument.RenderParameters);
    }
    
    protected virtual async Task SetAllPdfDocumentsAsync(
        List<NavigationNode> navigations,
        Project project,
        string version,
        string languageCode,
        PdfDocument parentPdfDocument = null)
    {
        var groupedCombinationsDocuments = new Dictionary<string, List<PdfDocument>>();
        foreach (var navigation in navigations)
        {
            if (navigation.IgnoreOnDownload)
            {
                continue;
            }
            try
            {
                var pdfDocument = new PdfDocument
                { 
                    Title = navigation.Text,
                    IgnoreOnOutline = navigation.Path == Options.Value.IndexPagePath
                };
         
                if (!navigation.Path.IsNullOrWhiteSpace() && !navigation.HasChildItems)
                {
                    var document = await GetDocumentAsync(project, navigation.Path, version, languageCode);
                    var parameters = await DocumentSectionRenderer.GetAvailableParametersAsync(document.Content);
                    var parameterCombinations = GenerateAllParameterCombinations(parameters.Keys.ToList(), parameters);
                    var firstParameterCombination = parameterCombinations.FirstOrDefault();
                    
                    pdfDocument.Document = document;
                    pdfDocument.RenderParameters = firstParameterCombination;
                    pdfDocument.Id = GetDocumentId(document.Name, document.Format ?? project.Format, firstParameterCombination, true);
                    pdfDocument.Title = GetDocumentTitle(navigation.Text, firstParameterCombination, DocumentParams);
                    
                    if(parameters.Count <= 1)
                    {
                        AddParameterCombinationsDocuments(parentPdfDocument, groupedCombinationsDocuments);
                    }
                    else
                    {
                        for (var i = 1; i < parameterCombinations.Count; i++)
                        {
                            var parameterCombination = parameterCombinations[i];
                            var key = parameterCombination.Select(x => $"{x.Key}_{x.Value}").JoinAsString("-");
                            if (!groupedCombinationsDocuments.ContainsKey(key))
                            {
                                groupedCombinationsDocuments[key] = [];
                            }
                            
                            groupedCombinationsDocuments[key].Add(new PdfDocument
                            {
                                Document = document,
                                Id = GetDocumentId(document.Name, document.Format ?? project.Format, parameterCombination, false),
                                Title = GetDocumentTitle(navigation.Text, parameterCombination, DocumentParams),
                                RenderParameters = parameterCombination
                            });
                        }
                    }
                }
                
                if (parentPdfDocument == null)
                {
                    AllPdfDocuments.AddIfNotContains(pdfDocument);
                }
                else
                {
                    parentPdfDocument.Children.AddIfNotContains(pdfDocument);
                }
                
                if (navigation.HasChildItems)
                {
                    await SetAllPdfDocumentsAsync(navigation.Items, project, version, languageCode, pdfDocument);
                }

                if (navigation == navigations.Last())
                {
                    AddParameterCombinationsDocuments(parentPdfDocument, groupedCombinationsDocuments);
                }
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, $"Cannot get document for the path '{navigation.Path}' in the project {project.Name}.");
            }
        }
    }
    
    private void AddParameterCombinationsDocuments(PdfDocument parentPdfDocument, Dictionary<string,List<PdfDocument>> groupedCombinationsDocuments)
    {
        foreach (var combinations in groupedCombinationsDocuments)
        { 
            if (parentPdfDocument == null)
            {
                AllPdfDocuments.AddIfNotContains(combinations.Value);
            }
            else
            {
                parentPdfDocument.Children.AddIfNotContains(combinations.Value);
            }
        }
    }
    
    private async Task<NavigationNode> GetNavigationAsync(
        Project project,
        string version,
        string languageCode)
    {
        var navigationDocument = await GetDocumentAsync(project, project.NavigationDocumentName, version, languageCode);
       
        if (!DocsJsonSerializerHelper.TryDeserialize<NavigationNode>(navigationDocument.Content, out var navigation))
        {
            throw new UserFriendlyException($"Cannot validate navigation file '{project.NavigationDocumentName}' for the project {project.Name}.");
        }

        if (!Options.Value.IndexPagePath.IsNullOrWhiteSpace())
        {
            navigation.Items.Insert(0, new NavigationNode
            {
                Path = Options.Value.IndexPagePath
            });
        }
        
        return navigation;
    }
    
    private async Task<DocumentParams> GetDocumentParamsAsync(
        Project project,
        string version,
        string languageCode)
    {
        if (project.ParametersDocumentName.IsNullOrWhiteSpace())
        {
            return new DocumentParams();
        }

        try
        {
            var documentParamsDocument = await GetDocumentAsync(project, project.ParametersDocumentName, version, languageCode);
        
            if (!DocsJsonSerializerHelper.TryDeserialize<DocumentParams>(documentParamsDocument.Content, out var documentParams))
            {
                throw new UserFriendlyException($"Cannot validate document params file '{project.ParametersDocumentName}' for the project {project.Name}.");
            }
        
            return documentParams;
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"Cannot get document params for the project {project.Name}.", e);
            return new DocumentParams();
        }
    }
    
    private async Task<Document> GetDocumentAsync(
        Project project, 
        string documentName,
        string version,
        string languageCode)
    {
        version = string.IsNullOrWhiteSpace(version) ? project.LatestVersionBranchName : version;
        
        Document document = null;

        Exception firstException = null;
        foreach (var name in GetPossibleNames(documentName, project.Format))
        {
            try
            {
                document = await DocumentRepository.FindAsync(project.Id, documentName, version, languageCode);
                if (document != null)
                {
                    break;
                }
                
                document = await DocumentSource.GetDocumentAsync(project, name, languageCode, version);
                break;
            }
            catch (Exception ex)
            {
                firstException ??= ex;
            }
        }

        if(document == null)
        {
            throw firstException!;
        }

        return document;
    }
    
    private List<DocumentRenderParameters> GenerateAllParameterCombinations(List<string> parameterKeys, Dictionary<string, List<string>> parameters)
    {
        var parameterCombinations = new List<DocumentRenderParameters>();

        if (parameters.Count <= 0)
        {
            return parameterCombinations;
        }
        
        GenerateCombinations(0, new DocumentRenderParameters());
        return parameterCombinations;

        void GenerateCombinations(int keyIndex, DocumentRenderParameters currentCombination)
        {
            if (keyIndex == parameterKeys.Count)
            {
                parameterCombinations.Add(new DocumentRenderParameters(currentCombination));
                return;
            }

            var currentKey = parameterKeys[keyIndex];
            foreach (var value in parameters[currentKey])
            {
                currentCombination[currentKey] = value;
                GenerateCombinations(keyIndex + 1, currentCombination);
            }
        }
    }

    private static string GetDocumentId(string documentName, string format, DocumentRenderParameters parameters, bool isFirstCombinationDocument)
    {
       var id = documentName.Replace("." + format, string.Empty).Replace("/","-").Replace(" ", "-").ToLower();
       if (parameters != null && !isFirstCombinationDocument)
       {
           id = $"{id}{parameters.Select(x => $"{x.Key}_{x.Value}").JoinAsString("-")}";
       }
       
       return id;
    }
    
    private static string GetDocumentTitle(string title, DocumentRenderParameters parameters, DocumentParams documentParams)
    {
        if (parameters == null || parameters.Count <= 0)
        {
            return title;
        }
        
        var paramValues = parameters.Select(x =>
        {
            var documentParam = documentParams.Parameters.FirstOrDefault(p => p.Name == x.Key);
            return $"{documentParam?.DisplayName ?? x.Key} : {documentParam?.Values[x.Value] ?? x.Value}";
        });
        
        return title.Trim() + $" ({string.Join(", ", paramValues)})";
    }
    
    private static List<string> GetPossibleNames(string originalDocumentName, string format)
    {
        var extension = Path.GetExtension(originalDocumentName);
        if (extension.IsNullOrWhiteSpace())
        {
            extension = "." + format;
        }
        
        if (!extension.Equals("." + format, StringComparison.OrdinalIgnoreCase))
        {
            return [originalDocumentName];
        }

        var lowerCaseIndex = "index." + format;
        var titleCaseIndex = "Index." + format;
        var indexLength = lowerCaseIndex.Length;

        var possibleNames = new List<string> {originalDocumentName};
        if (originalDocumentName.EndsWith("/" + lowerCaseIndex, StringComparison.OrdinalIgnoreCase) || originalDocumentName.Equals(lowerCaseIndex, StringComparison.OrdinalIgnoreCase))
        {
            var indexPart = originalDocumentName.Right(indexLength);

            var documentNameWithoutIndex = originalDocumentName.Left(originalDocumentName.Length - lowerCaseIndex.Length);

            if(indexPart != lowerCaseIndex)
            {
                possibleNames.Add(documentNameWithoutIndex + lowerCaseIndex);
            }

            if(indexPart != titleCaseIndex)
            {
                possibleNames.Add(documentNameWithoutIndex + titleCaseIndex);
            }
        }
        else
        {
            var documentNameWithoutExtension = RemoveFileExtensionFromPath(originalDocumentName, format).EnsureEndsWith('/');
            possibleNames.Add(documentNameWithoutExtension + lowerCaseIndex);
            possibleNames.Add(documentNameWithoutExtension + titleCaseIndex);
        }

        return possibleNames;
    }
    
    private static string RemoveFileExtensionFromPath(string path, string format)
    {
        if (path == null)
        {
            return null;
        }

        return path.EndsWith("." + format)
            ? path.Left(path.Length - format.Length - 1)
            : path;
    }
}