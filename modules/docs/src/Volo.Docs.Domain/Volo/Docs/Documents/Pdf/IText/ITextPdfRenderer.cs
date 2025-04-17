using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using ITextDocument = iText.Layout.Document;

namespace Volo.Docs.Documents.Pdf.IText;

public class ITextPdfRenderer : IPdfRenderer ,ITransientDependency
{
    protected IOptions<DocsDocumentPdfGeneratorOptions> Options { get; }
    
    public ITextPdfRenderer(IOptions<DocsDocumentPdfGeneratorOptions> options)
    {
        Options = options;
    }
    
    public virtual Task<MemoryStream> GeneratePdfAsync(string htmlContent, List<PdfDocumentNode> pdfDocumentNodes)
    {
        var pdfStream = new MemoryStream();
        var pdfWrite = new PdfWriter(pdfStream);
        var pdfDocument = new PdfDocument(pdfWrite);
        var textDocument = new ITextDocument(pdfDocument);
        pdfWrite.SetCloseStream(false);
        
        var htmlBuilder = new StringBuilder();
        htmlBuilder.Append(Options.Value.HtmlLayout);
        htmlBuilder.Replace(DocsDocumentPdfGeneratorOptions.StylePlaceholder, Options.Value.HtmlStyles);
        htmlBuilder.Replace(DocsDocumentPdfGeneratorOptions.ContentPlaceholder, htmlContent);
        
        var converter = new ConverterProperties();
        var tagWorkerFactory = new HtmlIdTagWorkerFactory(pdfDocument);
        converter.SetTagWorkerFactory(tagWorkerFactory);
        
        HtmlConverter.ConvertToDocument(htmlBuilder.ToString(), pdfDocument, converter);
        
        tagWorkerFactory.AddNamedDestinations();
        var pdfOutlines = pdfDocument.GetOutlines(false);
        BuildPdfOutlines(pdfOutlines, pdfDocumentNodes);
                
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
}