using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Docs.Utils;
using ITextDocument = iText.Layout.Document;

namespace Volo.Docs.Projects.Pdf.IText;

public class ITextHtmlToPdfRenderer : IHtmlToPdfRenderer
{
    protected IOptions<DocsProjectPdfGeneratorOptions> Options { get; }
    
    public ITextHtmlToPdfRenderer(IOptions<DocsProjectPdfGeneratorOptions> options)
    {
        Options = options;
    }
    
    public virtual Task<Stream> RenderAsync(string title, string html, List<PdfDocument> documents)
    {
        var pdfStream = new MemoryStream();
        var pdfWrite = new PdfWriter(pdfStream);
        var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfWrite);
        pdfDocument.GetDocumentInfo().SetTitle(title);
        var textDocument = new ITextDocument(pdfDocument);
        pdfWrite.SetCloseStream(false);
        
        var converter = new ConverterProperties();
        var tagWorkerFactory = new HtmlIdTagWorkerFactory(pdfDocument);
        converter.SetTagWorkerFactory(tagWorkerFactory);
        
        HtmlConverter.ConvertToDocument(html, pdfDocument, converter);
        
        tagWorkerFactory.AddNamedDestinations();
        var pdfOutlines = pdfDocument.GetOutlines(false);
        BuildPdfOutlines(pdfOutlines, documents);
                
        textDocument.Close();
        pdfStream.Position = 0;
        return Task.FromResult<Stream>(pdfStream);
    }

    public virtual async Task<MemoryStream> MergePdfFilesAsync(List<MemoryStream> pdfFiles, string title, bool disposeStreams = true)
    {
        var mergedStream = new MemoryStream();
        var mergedPdfWriter = new PdfWriter(mergedStream);
        var mergedPdfDocument = new iText.Kernel.Pdf.PdfDocument(mergedPdfWriter);
        mergedPdfDocument.GetDocumentInfo().SetTitle(title);
        mergedPdfWriter.SetCloseStream(false);
        
        foreach (var pdfFile in pdfFiles)
        {
            try
            {
                using var reader = new PdfReader(pdfFile);
                using var sourcePdf = new iText.Kernel.Pdf.PdfDocument(reader);

                var pageCount = sourcePdf.GetNumberOfPages();

                for (var i = 1; i <= pageCount; i++)
                {
                    var page = sourcePdf.GetPage(i);
                    mergedPdfDocument.AddPage(page.CopyTo(mergedPdfDocument));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error merging PDF file {pdfFile}: {ex.Message}", ex);
            }
            finally
            {
                try
                {
                    await pdfFile.DisposeAsync();
                }
                catch
                {
                    // Ignore any exceptions during disposal
                }
            }
        }
        
        mergedPdfDocument.Close();
        mergedStream.Position = 0;
        return mergedStream;
    }

    private void BuildPdfOutlines(PdfOutline parentOutline, List<PdfDocument> pdfDocumentNodes)
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
                outline.AddAction(UrlHelper.IsExternalLink(pdfDocumentNode.Id) ? PdfAction.CreateURI(pdfDocumentNode.Id) : PdfAction.CreateGoTo(pdfDocumentNode.Id));
            }

            if (pdfDocumentNode.HasChildren)
            {
                BuildPdfOutlines(outline, pdfDocumentNode.Children);
            }
        }
    }
}