using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using Markdig;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Docs.Documents;
using Volo.Docs.Documents.Rendering;
using Volo.Docs.Projects;
using Volo.Extensions;
using ITextDocument = iText.Layout.Document;

namespace Volo.Docs.Common.Documents;

public class DocumentPdfGenerator : IDocumentPdfGenerator
{
    protected IDocumentSourceFactory DocumentStoreFactory { get; }
    protected IDocumentRepository DocumentRepository { get; }
    protected IDocumentSectionRenderer DocumentSectionRenderer { get; }
    protected IOptions<DocsDocumentPdfGeneratorOptions> Options { get; }
    protected ILogger<DocumentPdfGenerator> Logger { get; set; }
    protected IDocumentSource DocumentSource { get; set; }
    protected DocumentParams DocumentParams { get; set; }
    protected List<PdfDocumentNode> AllPdfDocumentNodes { get; set; } = new();
    protected MarkdownPipeline MarkdownPipeline { get; } = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    public DocumentPdfGenerator(
        IDocumentSourceFactory documentStoreFactory, 
        IDocumentRepository documentRepository,
        IOptions<DocsDocumentPdfGeneratorOptions> options,
        IDocumentSectionRenderer documentSectionRenderer)
    {
        DocumentStoreFactory = documentStoreFactory;
        DocumentRepository = documentRepository;
        Options = options;
        DocumentSectionRenderer = documentSectionRenderer;
        Logger = NullLogger<DocumentPdfGenerator>.Instance;
    }
    
    public virtual async Task<IRemoteStreamContent> GenerateAsync(Project project, string version, string languageCode)
    {
        DocumentSource = DocumentStoreFactory.Create(project.DocumentStoreType);
        DocumentParams = await GetDocumentParamsAsync(project, version, languageCode);
        var navigation = await GetNavigationAsync(project, version, languageCode);
        
        await SetAllPdfDocumentNodesAsync(navigation.Items, project, version, languageCode);
        var htmlContent = await ConvertDocumentsToHtmlAsync(AllPdfDocumentNodes);
        
        var pdfStream = await GeneratePdfAsync(htmlContent);
        
        return new RemoteStreamContent(pdfStream, CalculateDocumentPdfFileName(project.ShortName, version, languageCode), "application/pdf");
    }
    
    protected virtual async Task<string> ConvertDocumentsToHtmlAsync(List<PdfDocumentNode> pdfDocumentNodes)
    {
        var contentBuilder = new StringBuilder();

        foreach (var pdfDocumentNode in pdfDocumentNodes)
        {
            if (pdfDocumentNode.Document != null)
            {
                var renderedDocument = await RenderDocumentAsync(pdfDocumentNode);
                renderedDocument = NormalizeHtmlContent(pdfDocumentNode, Markdown.ToHtml(renderedDocument, MarkdownPipeline));
                contentBuilder.AppendLine(renderedDocument);
            }

            if (pdfDocumentNode.HasChildren)
            {
                contentBuilder.AppendLine(await ConvertDocumentsToHtmlAsync(pdfDocumentNode.Children));
            }
        }
        
        return contentBuilder.ToString();
    }
    
    protected virtual Task<MemoryStream> GeneratePdfAsync(string htmlContent)
    {
        var pdfStream = new MemoryStream();
        var pdfWrite = new PdfWriter(pdfStream);
        var pdfDocument = new PdfDocument(pdfWrite);
        var textDocument = new ITextDocument(pdfDocument);
        pdfWrite.SetCloseStream(false);
        
        var htmlBuilder = new StringBuilder();
        htmlBuilder.Append(Options.Value.HtmlLayout);
        htmlBuilder.Replace("{{style-placeholder}}", Options.Value.HtmlStyles);
        htmlBuilder.Replace("{{content-placeholder}}", htmlContent);
        
        var converter = new ConverterProperties();
        var tagWorkerFactory = new HtmlIdTagWorkerFactory(pdfDocument);
        converter.SetTagWorkerFactory(tagWorkerFactory);
        
        HtmlConverter.ConvertToDocument(htmlBuilder.ToString(), pdfDocument, converter);
        
        tagWorkerFactory.AddNamedDestinations();
        var pdfOutlines = pdfDocument.GetOutlines(false);
        BuildPdfOutlines(pdfOutlines, AllPdfDocumentNodes);
                
        textDocument.Close();
        pdfStream.Position = 0;
        return Task.FromResult(pdfStream);
    }

    private void BuildPdfOutlines(PdfOutline parentOutline, List<PdfDocumentNode> pdfDocumentNodes)
    {
        foreach (var pdfDocumentNode in pdfDocumentNodes)
        {
            if (pdfDocumentNode.IgnoreOnOutline)
            {
                continue;
            }
            
            var outline = parentOutline.AddOutline(pdfDocumentNode.Title);
            if (!pdfDocumentNode.Id.IsNullOrWhiteSpace())
            {
                outline.AddAction(PdfAction.CreateGoTo(pdfDocumentNode.Id));
            }

            if (pdfDocumentNode.HasChildren)
            {
                BuildPdfOutlines(outline, pdfDocumentNode.Children);
            }
        }
    }
    
    protected virtual async Task<string> RenderDocumentAsync(PdfDocumentNode pdfDocumentNode)
    {
        var renderedDocument = await DocumentSectionRenderer.RenderAsync(pdfDocumentNode.Document.Content, pdfDocumentNode.RenderParameters);
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
                    IgnoreOnOutline = navigation.Path == "index.md"
                };
                
                if (!navigation.Path.IsNullOrWhiteSpace())
                {
                    var path = NormalizeNavigationPath(navigation.Path);
                    var document = await GetDocumentAsync(project, path, version, languageCode);
                    var parameters = await DocumentSectionRenderer.GetAvailableParametersAsync(document.Content);
                    var parameterCombinations = GenerateAllParameterCombinations(parameters.Keys.ToList(), parameters);
                    var firstParameterCombination = parameterCombinations.FirstOrDefault();
                    
                    pdfDocumentNode.Document = document;
                    pdfDocumentNode.Title = GetDocumentTitle(navigation.Text, document.Content, firstParameterCombination);
                    pdfDocumentNode.Id = GetDocumentId(navigation.Path, firstParameterCombination);
                    pdfDocumentNode.RenderParameters = firstParameterCombination;
                   
                    if(parameters.Count <= 1)
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
                    else
                    {
                        for (var i = 1; i < parameterCombinations.Count; i++)
                        {
                            var parameterCombination = parameterCombinations[i];
                            parameterCombinationsDocuments.Add(new PdfDocumentNode
                            {
                                Document = document,
                                Title = GetDocumentTitle(navigation.Text, document.Content, parameterCombination),
                                Id = GetDocumentId(navigation.Path, parameterCombination),
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
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, $"Cannot get document for the path '{navigation.Path}' in the project {project.Name}.");
            }
        }
    }
    
    protected virtual string CalculateDocumentPdfFileName(string projectName, string version, string languageCode)
    {
        return $"{projectName.ToLower()}-{languageCode.ToLower()}.pdf";
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
        
        navigation.Items.Insert(0, new NavigationNode
        {
            Text = "Index",
            Path = "index.md"
        });
        
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
        var titleLine = content.Split('\n').FirstOrDefault(x => x.TrimStart().StartsWith("#"));
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
        
        var titleLine = content.Split('\n').FirstOrDefault(x => x.TrimStart().StartsWith("#"));
        if (titleLine == null)
        {
            return title;
        }
        
        var paramValues = parameters.Select(x => $"{DocumentParams.Parameters.FirstOrDefault(p => p.Name == x.Key)?.DisplayName ?? x.Key}: {x.Value}").ToList();
        return titleLine.TrimStart('#').Trim() + $" ({string.Join(", ", paramValues)})";
    }
    

    private string GetDocumentId(string path, DocumentRenderParameters parameters)
    {
       var id = path.Replace(".md",string.Empty).Replace("/","-").Replace(" ", "-").ToLower();
       if (parameters != null)
       {
           id = $"{id}{parameters.Select(x => $"{x.Key}_{x.Value}").JoinAsString("-")}";
       }
       return id;
    }
    
    private string NormalizeHtmlContent(PdfDocumentNode pdfDocumentNode, string htmlContent)
    {
        htmlContent = $"<div class='page' id='{pdfDocumentNode.Id}'>{htmlContent}</div>";
        
        htmlContent = Regex.Replace(htmlContent, @"(<img\s+[^>]*)src=""([^""]*)""([^>]*>)", delegate (Match match)
        {
            if (IsExternalLink(match.Groups[2].Value))
            {
                return match.Value;
            }

            var rootUrl = IsExternalLink(pdfDocumentNode.Document.RawRootUrl)
                ? pdfDocumentNode.Document.RawRootUrl.EnsureEndsWith('/')
                : Options.Value.BaseUrl.EnsureEndsWith('/') + pdfDocumentNode.Document.RawRootUrl.TrimStart('/').EnsureEndsWith('/');
            var newImageSource = rootUrl +
                                 (pdfDocumentNode.Document.LocalDirectory.IsNullOrEmpty() ? "" : pdfDocumentNode.Document.LocalDirectory.TrimStart('/').EnsureEndsWith('/')) +
                                 match.Groups[2].Value.TrimStart('/');

            return match.Groups[1] + " src=\"" + HttpUtility.HtmlEncode(newImageSource) + "\" " + match.Groups[3];

        }, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);

        return htmlContent;
    }
    
    private bool IsExternalLink(string link)
    {
        return link.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               link.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    protected class PdfDocumentNode
    {
        public Document Document { get; set; }
        public string Title { get; set; }
        public string Id { get; set; }
        public List<PdfDocumentNode> Children { get; set; } = [];
        public DocumentRenderParameters RenderParameters { get; set; }
        public bool HasChildren => Children.Any();
        public bool IgnoreOnOutline { get; set; }
    }
}