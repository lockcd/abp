using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.Localization;
using Volo.Docs.Documents;
using Volo.Docs.HtmlConverting;
using Volo.Docs.Models;
using Volo.Docs.Projects;

namespace Volo.Docs.Pages.Documents.Project
{
    public class IndexModel : AbpPageModel
    {
        [BindProperty(SupportsGet = true)]
        public string ProjectName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Version { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string DocumentName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string LanguageCode { get; set; }

        public ProjectDto Project { get; set; }

        public List<SelectListItem> LanguageSelectListItems { get; set; }

        public string DocumentNameWithExtension { get; private set; }

        public DocumentWithDetailsDto Document { get; private set; }

        public List<SelectListItem> VersionSelectItems { get; private set; }

        public List<SelectListItem> ProjectSelectItems { get; private set; }

        public NavigationWithDetailsDto Navigation { get; private set; }

        public VersionInfoViewModel LatestVersionInfo { get; private set; }

        private readonly IDocumentAppService _documentAppService;
        private readonly IDocumentToHtmlConverterFactory _documentToHtmlConverterFactory;
        private readonly IProjectAppService _projectAppService;

        public IndexModel(
            IDocumentAppService documentAppService,
            IDocumentToHtmlConverterFactory documentToHtmlConverterFactory,
            IProjectAppService projectAppService)
        {
            _documentAppService = documentAppService;
            _documentToHtmlConverterFactory = documentToHtmlConverterFactory;
            _projectAppService = projectAppService;
        }

        public async Task OnGetAsync()
        {
            await SetProjectAsync();
            await SetProjectsAsync();
            await SetVersionAsync();
            await SetDocumentAsync();
            await SetNavigationAsync();
            SetLanguageSelectListItems();
            AddLanguageCodePrefixToLinks();
        }

        private async Task SetProjectAsync()
        {
            Project = await _projectAppService.GetAsync(ProjectName);
        }

        private async Task SetProjectsAsync()
        {
            var projects = await _projectAppService.GetListAsync();

            ProjectSelectItems = projects.Items.Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.Id != Project.Id ? "/documents/" + LanguageCode + "/" + p.ShortName + "/" + DocsAppConsts.Latest : null,
                Selected = p.Id == Project.Id
            }).ToList();
        }

        private async Task SetVersionAsync()
        {
            //TODO: Needs refactoring
            if (string.IsNullOrWhiteSpace(Version))
            {
                Version = DocsAppConsts.Latest;
            }

            var output = await _projectAppService.GetVersionsAsync(Project.ShortName);
            var versions = output.Items
                .Select(v => new VersionInfoViewModel(v.DisplayName, v.Name))
                .ToList();

            if (versions.Any())
            {
                LatestVersionInfo = versions.First();
                LatestVersionInfo.DisplayText = $"{LatestVersionInfo.DisplayText} ({DocsAppConsts.Latest})";
                LatestVersionInfo.Version = LatestVersionInfo.Version;

                if (string.Equals(Version, DocsAppConsts.Latest, StringComparison.OrdinalIgnoreCase))
                {
                    LatestVersionInfo.IsSelected = true;
                    Version = LatestVersionInfo.Version;
                }
                else
                {
                    var versionFromUrl = versions.FirstOrDefault(v => v.Version == Version);
                    if (versionFromUrl != null)
                    {
                        versionFromUrl.IsSelected = true;
                        Version = versionFromUrl.Version;
                    }
                    else
                    {
                        versions.First().IsSelected = true;
                        Version = versions.First().Version;
                    }
                }
            }

            VersionSelectItems = versions.Select(v => new SelectListItem
            {
                Text = v.DisplayText,
                Value = CreateVersionLink(LatestVersionInfo, v.Version, DocumentName),
                Selected = v.IsSelected
            }).ToList();
        }

        private async Task SetNavigationAsync()
        {
            try
            {
                var document = await _documentAppService.GetNavigationAsync(
                    new GetNavigationDocumentInput
                    {
                        ProjectId = Project.Id,
                        LanguageCode = LanguageCode,
                        Version = Version
                    }
                );

                Navigation = ObjectMapper.Map<DocumentWithDetailsDto, NavigationWithDetailsDto>(document);
            }
            catch (DocumentNotFoundException) //TODO: What if called on a remote service which may return 404
            {
                return;
            }

            LanguageCode = Document.CurrentLanguageCode;

            Navigation.ConvertItems();
        }

        public string CreateVersionLink(VersionInfoViewModel latestVersion, string version, string documentName = null)
        {
            if (latestVersion == null || latestVersion.Version == version)
            {
                version = DocsAppConsts.Latest;
            }

            var link = "/documents/" + LanguageCode + "/" + ProjectName + "/" + version;

            if (documentName != null)
            {
                link += "/" + DocumentName;
            }

            return link;
        }

        public string GetSpecificVersionOrLatest()
        {
            if (Document?.Version == null)
            {
                return DocsAppConsts.Latest;
            }

            return Document.Version == LatestVersionInfo.Version ?
                DocsAppConsts.Latest :
                Document.Version;
        }

        private async Task SetDocumentAsync()
        {
            if (DocumentName.IsNullOrWhiteSpace())
            {
                DocumentName = Project.DefaultDocumentName;
            }

            DocumentNameWithExtension = DocumentName + "." + Project.Format;

            try
            {
                if (DocumentNameWithExtension.IsNullOrWhiteSpace())
                {
                    Document = await _documentAppService.GetDefaultAsync(
                        new GetDefaultDocumentInput
                        {
                            ProjectId = Project.Id,
                            LanguageCode = LanguageCode,
                            Version = Version
                        }
                    );
                }
                else
                {
                    Document = await _documentAppService.GetAsync(
                        new GetDocumentInput
                        {
                            ProjectId = Project.Id,
                            Name = DocumentNameWithExtension,
                            LanguageCode = LanguageCode,
                            Version = Version
                        }
                    );
                }

            }
            catch (DocumentNotFoundException)
            {
                Document = await _documentAppService.GetDefaultAsync(
                    new GetDefaultDocumentInput
                    {
                        ProjectId = Project.Id,
                        LanguageCode = LanguageCode,
                        Version = Version
                    }
                );
            }

            LanguageCode = Document.CurrentLanguageCode;

            ConvertDocumentContentToHtml();
        }

        private void SetLanguageSelectListItems()
        {
            LanguageSelectListItems = new List<SelectListItem>();

            foreach (var language in Document.Project.Languages)
            {
                LanguageSelectListItems.Add(
                    new SelectListItem(
                        language.Value,
                        "/documents/" + language.Key + "/" + Project.ShortName + "/" + Version + "/" + DocumentName,
                        language.Key == LanguageCode
                        )
                    );
            }
        }

        private void ConvertDocumentContentToHtml()
        {
            var converter = _documentToHtmlConverterFactory.Create(Document.Format ?? Project.Format);
            var content = converter.Convert(Project, Document, GetSpecificVersionOrLatest());

            content = HtmlNormalizer.ReplaceImageSources(
                content,
                Document.RawRootUrl,
                Document.LocalDirectory
            );

            //todo find a way to make it on client in prismJS configuration (eg: map C# => csharp)
            content = HtmlNormalizer.ReplaceCodeBlocksLanguage(
                content,
                "language-C#",
                "language-csharp"
            );

            Document.Content = content;
        }

        private void AddLanguageCodePrefixToLinks()
        {
            Document.Content = Document.Content.Replace("href=\"/documents", "href=\"/documents/" + LanguageCode);
        }
    }
}