namespace Volo.Docs.Projects.Pdf;

public class PdfDocumentToHtmlConverterContext
{
    public string Content { get; set; }
    public PdfDocument PdfDocument { get; set; }
    
    public PdfDocumentToHtmlConverterContext(string content, PdfDocument pdfDocument)
    {
        Content = content;
        PdfDocument = pdfDocument;
    }
}