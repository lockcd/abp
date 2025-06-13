using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Volo.Docs.Projects.Pdf;

public interface IHtmlToPdfRenderer
{
    Stream Render(string title, string html, List<PdfDocument> documents);
}