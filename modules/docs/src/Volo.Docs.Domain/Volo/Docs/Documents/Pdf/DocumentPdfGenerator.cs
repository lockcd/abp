using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Markdig;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;
using Volo.Docs.Documents.Pdf.Markdig;
using Volo.Docs.Documents.Rendering;
using Volo.Docs.Projects;
using Volo.Docs.Utils;
using Volo.Extensions;

namespace Volo.Docs.Documents.Pdf;

public class DocumentPdfGenerator : IDocumentPdfGenerator, ITransientDependency
{
    protected IDocumentSourceFactory DocumentStoreFactory { get; }
    protected IDocumentRepository DocumentRepository { get; }
    protected IDocumentSectionRenderer DocumentSectionRenderer { get; }
    protected IOptions<DocsDocumentPdfGeneratorOptions> Options { get; }  
    protected IDocumentPdfFileStore DocumentPdfFileStore { get; }
    protected IPdfRenderer PdfRenderer { get; }
    protected ILogger<DocumentPdfGenerator> Logger { get; set; }
    protected IDocumentSource DocumentSource { get; set; }
    protected DocumentParams DocumentParams { get; set; }
    protected List<PdfDocumentNode> AllPdfDocumentNodes { get; } = [];

    public DocumentPdfGenerator(
        IDocumentSourceFactory documentStoreFactory, 
        IDocumentRepository documentRepository,
        IOptions<DocsDocumentPdfGeneratorOptions> options,
        IDocumentSectionRenderer documentSectionRenderer,
        IDocumentPdfFileStore documentPdfFileStore, 
        IPdfRenderer pdfRenderer)
    {
        DocumentStoreFactory = documentStoreFactory;
        DocumentRepository = documentRepository;
        Options = options;
        DocumentSectionRenderer = documentSectionRenderer;
        DocumentPdfFileStore = documentPdfFileStore;
        PdfRenderer = pdfRenderer;
        Logger = NullLogger<DocumentPdfGenerator>.Instance;
    }
    
    public virtual async Task<IRemoteStreamContent> GenerateAsync(Project project, string version, string languageCode)
    {
        var fileName = Options.Value.CalculatePdfFileName(project, version, languageCode);
        var fileStream = await DocumentPdfFileStore.GetOrNullAsync(project, version, languageCode);
        
        if (fileStream != null)
        {
            return new RemoteStreamContent(fileStream, fileName, "application/pdf");
        }
        
        DocumentSource = DocumentStoreFactory.Create(project.DocumentStoreType);
        DocumentParams = await GetDocumentParamsAsync(project, version, languageCode);
        var navigation = await GetNavigationAsync(project, version, languageCode);
    
        await SetAllPdfDocumentNodesAsync(navigation.Items, project, version, languageCode);
        var htmlContent = await ConvertDocumentsToHtmlAsync(AllPdfDocumentNodes);
        
        var pdfStream = await PdfRenderer.GeneratePdfAsync(htmlContent, AllPdfDocumentNodes);
        await DocumentPdfFileStore.SetAsync(project, version, languageCode, pdfStream);
    
        return new RemoteStreamContent(pdfStream, fileName, "application/pdf");
    }
    
    protected virtual async Task<string> ConvertDocumentsToHtmlAsync(List<PdfDocumentNode> pdfDocumentNodes)
    {
        var contentBuilder = new StringBuilder();

        foreach (var pdfDocumentNode in pdfDocumentNodes)
        {
            if (pdfDocumentNode.Document != null)
            {
                var renderedDocument = await RenderDocumentAsync(pdfDocumentNode);
                renderedDocument = NormalizeHtmlContent(pdfDocumentNode, Markdown.ToHtml(renderedDocument, GetMarkdownPipeline(pdfDocumentNode)));
                contentBuilder.AppendLine(renderedDocument);
            }

            if (pdfDocumentNode.HasChildren)
            {
                contentBuilder.AppendLine(await ConvertDocumentsToHtmlAsync(pdfDocumentNode.Children));
            }
        }
        
        return contentBuilder.ToString();
    }
    
    protected virtual async Task<string> RenderDocumentAsync(PdfDocumentNode pdfDocumentNode)
    {
        var content = NormalizeMarkdownContent(pdfDocumentNode.Document.Content);
        var renderedDocument = await DocumentSectionRenderer.RenderAsync(content, pdfDocumentNode.RenderParameters);
        if (pdfDocumentNode.RenderParameters != null)
        {
            renderedDocument = SetDocumentTitle(renderedDocument, pdfDocumentNode.Title);
        }

        return renderedDocument;
    }
    
    protected virtual async Task SetAllPdfDocumentNodesAsync(
        List<NavigationNode> navigations,
        Project project,
        string version,
        string languageCode,
        PdfDocumentNode parentPdfDocumentNode = null)
    {
        var parameterCombinationsDocuments = new List<PdfDocumentNode>();
        foreach (var navigation in navigations)
        {
            if (navigation.IgnoreOnDownload)
            {
                continue;
            }
            try
            {
                var pdfDocumentNode = new PdfDocumentNode
                { 
                    Title = navigation.Text,
                    IgnoreOnOutline = navigation.Path == Options.Value.CoverPagePath
                };
         
                if (!navigation.Path.IsNullOrWhiteSpace() && !navigation.HasChildItems)
                {
                    var path = NormalizeNavigationPath(navigation.Path);
                    var document = await GetDocumentAsync(project, path, version, languageCode);
                    var parameters = await DocumentSectionRenderer.GetAvailableParametersAsync(document.Content);
                    var parameterCombinations = GenerateAllParameterCombinations(parameters.Keys.ToList(), parameters);
                    var firstParameterCombination = parameterCombinations.FirstOrDefault();
                    
                    pdfDocumentNode.Document = document;
                    pdfDocumentNode.Title = GetDocumentTitle(navigation.Text, document.Content, firstParameterCombination);
                    pdfDocumentNode.Id = GetDocumentId(path, firstParameterCombination, true);
                    pdfDocumentNode.RenderParameters = firstParameterCombination;
                    
                    if(parameters.Count <= 1)
                    {
                        AddParameterCombinationsDocuments(parentPdfDocumentNode, parameterCombinationsDocuments);
                    }
                    else
                    {
                        for (var i = 1; i < parameterCombinations.Count; i++)
                        {
                            var parameterCombination = parameterCombinations[i];
                            parameterCombinationsDocuments.Add(new PdfDocumentNode
                            {
                                Document = document,
                                Title = GetDocumentTitle(navigation.Text, document.Content, parameterCombination),
                                Id = GetDocumentId(path, parameterCombination, false),
                                RenderParameters = parameterCombination
                            });
                        }
                    }
                }
                
                if (parentPdfDocumentNode == null)
                {
                    AllPdfDocumentNodes.Add(pdfDocumentNode);
                }
                else
                {
                    parentPdfDocumentNode.Children.Add(pdfDocumentNode);
                }
                
                if (navigation.HasChildItems)
                {
                    await SetAllPdfDocumentNodesAsync(navigation.Items, project, version, languageCode, pdfDocumentNode);
                }

                if (navigation == navigations.Last())
                {
                    AddParameterCombinationsDocuments(parentPdfDocumentNode, parameterCombinationsDocuments);
                }
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, $"Cannot get document for the path '{navigation.Path}' in the project {project.Name}.");
            }
        }
    }
    
    private void AddParameterCombinationsDocuments(PdfDocumentNode parentPdfDocumentNode, List<PdfDocumentNode> parameterCombinationsDocuments)
    {
        if (parentPdfDocumentNode == null)
        {
            AllPdfDocumentNodes.AddRange(parameterCombinationsDocuments);
        }
        else
        {
            parentPdfDocumentNode.Children.AddRange(parameterCombinationsDocuments);
        }
    }
    
    private string NormalizeNavigationPath(string path)
    {
        return !path.EndsWith(".md") ? Path.Combine(path, "index.md") : path;
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

        if (!Options.Value.CoverPagePath.IsNullOrWhiteSpace())
        {
            navigation.Items.Insert(0, new NavigationNode
            {
                Path = Options.Value.CoverPagePath
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
        var document = await DocumentRepository.FindAsync(project.Id, documentName, version, languageCode);

        if (document != null)
        {
            return document;
        }

        document = await DocumentSource.GetDocumentAsync(project, documentName, languageCode, version);

        return document;
    }
    
    protected virtual List<DocumentRenderParameters> GenerateAllParameterCombinations(List<string> parameterKeys, Dictionary<string, List<string>> parameters)
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

    private string SetDocumentTitle(string content, string title)
    {
        var titleLine = content.Split(Environment.NewLine).FirstOrDefault(x => x.TrimStart().StartsWith("#"));
        if (titleLine == null)
        {
            return content;
        }
        
        var newTitle = $"# {title}";
        return content.Replace(titleLine, newTitle);
    }
    
    private string GetDocumentTitle(string title, string content, DocumentRenderParameters parameters)
    {
        if (parameters == null || parameters.Count <= 0)
        {
            return title;
        }
        
        var titleLine = content.Split(Environment.NewLine).FirstOrDefault(x => x.TrimStart().StartsWith("#"));
        if (titleLine == null)
        {
            return title;
        }
        
        var paramValues = parameters.Select(x => $"{DocumentParams.Parameters.FirstOrDefault(p => p.Name == x.Key)?.DisplayName ?? x.Key}: {x.Value}").ToList();
        return titleLine.TrimStart('#').Trim() + $" ({string.Join(", ", paramValues)})";
    }

    private string GetDocumentId(string path, DocumentRenderParameters parameters, bool isFirstCombinationDocument)
    {
       var id = path.Replace(".md",string.Empty).Replace("/","-").Replace(" ", "-").ToLower();
       if (parameters != null && !isFirstCombinationDocument)
       {
           id = $"{id}{parameters.Select(x => $"{x.Key}_{x.Value}").JoinAsString("-")}";
       }
       
       return id;
    }
    
    private string NormalizeHtmlContent(PdfDocumentNode pdfDocumentNode, string htmlContent)
    {
        htmlContent = $"<div class='page' id='{pdfDocumentNode.Id}'>{htmlContent}</div>";
        htmlContent = ReplaceRelativeImageUrls(pdfDocumentNode, htmlContent);

        return htmlContent;
    }

    private string ReplaceRelativeImageUrls(PdfDocumentNode pdfDocumentNode, string htmlContent)
    {
        return Regex.Replace(htmlContent, @"(<img\s+[^>]*)src=""([^""]*)""([^>]*>)", delegate (Match match)
        {
            if (UrlHelper.IsExternalLink(match.Groups[2].Value))
            {
                return match.Value;
            }

            var rootUrl = UrlHelper.IsExternalLink(pdfDocumentNode.Document.RawRootUrl)
                ? pdfDocumentNode.Document.RawRootUrl.EnsureEndsWith('/')
                : Options.Value.BaseUrl.EnsureEndsWith('/') + pdfDocumentNode.Document.RawRootUrl.TrimStart('/').EnsureEndsWith('/');
            var newImageSource = rootUrl +
                                 (pdfDocumentNode.Document.LocalDirectory.IsNullOrEmpty() ? "" : pdfDocumentNode.Document.LocalDirectory.TrimStart('/').EnsureEndsWith('/')) +
                                 match.Groups[2].Value.TrimStart('/');

            return match.Groups[1] + " src=\"" + HttpUtility.HtmlEncode(newImageSource) + "\" " + match.Groups[3];

        }, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);
    }

    private string NormalizeMarkdownContent(string content)
    {
        var pattern = @"````json\s*//$begin:math:display$doc-nav$end:math:display$[\s\S]*?````";
        return Regex.Replace(content, pattern, string.Empty, RegexOptions.IgnoreCase);
    }

    private MarkdownPipeline GetMarkdownPipeline(PdfDocumentNode pdfDocumentNode)
    {
        return new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Use(new AnchorLinkResolverExtension(pdfDocumentNode))
            .Build(); 
    }
}