using System.Threading.Tasks;
using Volo.Abp.Content;
using Volo.Docs.Projects;

namespace Volo.Docs.Documents.Pdf;

public interface IDocumentPdfGenerator
{
    Task<IRemoteStreamContent> GenerateAsync(Project project, string version, string languageCode);
}