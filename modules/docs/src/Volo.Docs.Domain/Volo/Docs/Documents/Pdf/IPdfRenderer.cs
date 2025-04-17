using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Volo.Docs.Documents.Pdf;

public interface IPdfRenderer
{
    Task<MemoryStream> GeneratePdfAsync(string htmlContent, List<PdfDocumentNode> pdfDocumentNodes);
}