using System.IO;
using System.Threading.Tasks;
using Volo.Docs.Projects;

namespace Volo.Docs.Documents.Pdf;

public interface IDocumentPdfFileStore
{
    Task SetAsync(Project project, string version, string languageCode, Stream stream);
    
    Task<Stream> GetOrNullAsync(Project project, string version, string languageCode);
}