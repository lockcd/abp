using System.Threading.Tasks;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;
using Volo.Docs.Projects;

namespace Volo.Docs.Common.Documents;

public interface IDocumentPdfGenerator : ITransientDependency
{
    Task<IRemoteStreamContent> GenerateAsync(Project project, string version, string languageCode);
}