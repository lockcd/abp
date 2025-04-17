using System.Collections.Generic;
using System.Linq;
using Volo.Docs.Documents.Rendering;

namespace Volo.Docs.Documents.Pdf;

public class PdfDocumentNode
{
    public Document Document { get; set; }
    public string Title { get; set; }
    public string Id { get; set; }
    public List<PdfDocumentNode> Children { get; set; } = [];
    public DocumentRenderParameters RenderParameters { get; set; }
    public bool HasChildren => Children.Any();
    public bool IgnoreOnOutline { get; set; }
}