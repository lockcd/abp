using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Volo.Docs.Projects.Pdf;

public interface IHtmlToPdfRenderer
{
    Task<MemoryStream> RenderAsync(string html, List<PdfDocument> documents);
}