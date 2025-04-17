using Volo.Docs.Common.Projects;
using Volo.Docs.Documents;

namespace Volo.Docs.HtmlConverting
{
    public interface IDocumentToHtmlConverter
    {
        string Convert(ProjectDto project, DocumentWithDetailsDto document, string version, string languageCode, string projectShortName = null);
    }
}