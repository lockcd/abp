using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;

namespace Volo.Docs.Documents.Pdf.Markdig;

public class AnchorLinkResolverExtension : IMarkdownExtension
{
    private readonly PdfDocumentNode _documentNode;
    
    public AnchorLinkResolverExtension(PdfDocumentNode documentNode)
    {
        _documentNode = documentNode;
    }
    
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer)
        {
            htmlRenderer.ObjectRenderers.Replace<LinkInlineRenderer>(new AnchorLinkRenderer(_documentNode));
        }
    }
}