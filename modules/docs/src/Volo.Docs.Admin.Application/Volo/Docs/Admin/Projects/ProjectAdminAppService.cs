using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Guids;
using Volo.Docs.Documents;
using Volo.Docs.Documents.FullSearch.Elastic;
using Volo.Docs.Localization;
using Volo.Docs.Projects;
using Volo.Docs.Projects.Pdf;

namespace Volo.Docs.Admin.Projects
{
    [Authorize(DocsAdminPermissions.Projects.Default)]
    public class ProjectAdminAppService : ApplicationService, IProjectAdminAppService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentFullSearch _elasticSearchService;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IOptions<DocsProjectPdfGeneratorOptions> _pdfGeneratorOptions;

        public ProjectAdminAppService(
            IProjectRepository projectRepository,
            IDocumentRepository documentRepository,
            IDocumentFullSearch elasticSearchService,
            IGuidGenerator guidGenerator,
            IOptions<DocsProjectPdfGeneratorOptions> pdfGeneratorOptions)
        {
            ObjectMapperContext = typeof(DocsAdminApplicationModule);
            LocalizationResource = typeof(DocsResource);

            _projectRepository = projectRepository;
            _documentRepository = documentRepository;
            _elasticSearchService = elasticSearchService;
            _guidGenerator = guidGenerator;
            _pdfGeneratorOptions = pdfGeneratorOptions;
        }

        public virtual async Task<PagedResultDto<ProjectDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var projects = await _projectRepository.GetListAsync(input.Sorting, input.MaxResultCount, input.SkipCount);

            var totalCount = await _projectRepository.GetCountAsync();

            return new PagedResultDto<ProjectDto>(
                totalCount,
                ObjectMapper.Map<List<Project>, List<ProjectDto>>(projects)
                );
        }

        public virtual async Task<ProjectDto> GetAsync(Guid id)
        {
            var project = await _projectRepository.GetAsync(id);

            return ObjectMapper.Map<Project, ProjectDto>(project);
        }

        [Authorize(DocsAdminPermissions.Projects.Create)]
        public virtual async Task<ProjectDto> CreateAsync(CreateProjectDto input)
        {
            if (await _projectRepository.ShortNameExistsAsync(input.ShortName))
            {
                throw new ProjectShortNameAlreadyExistsException(input.ShortName);
            }

            var project = new Project(_guidGenerator.Create(),
                input.Name,
                input.ShortName,
                input.DocumentStoreType,
                input.Format,
                input.DefaultDocumentName,
                input.NavigationDocumentName,
                input.ParametersDocumentName
            )
            {
                MinimumVersion = input.MinimumVersion,
                MainWebsiteUrl = input.MainWebsiteUrl,
                LatestVersionBranchName = input.LatestVersionBranchName
            };

            foreach (var extraProperty in input.ExtraProperties)
            {
                project.SetProperty(extraProperty.Key, extraProperty.Value);
            }

            project = await _projectRepository.InsertAsync(project);

            return ObjectMapper.Map<Project, ProjectDto>(project);
        }

        [Authorize(DocsAdminPermissions.Projects.Update)]
        public virtual async Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectDto input)
        {
            var project = await _projectRepository.GetAsync(id);

            project.SetName(input.Name);
            project.SetFormat(input.Format);
            project.SetNavigationDocumentName(input.NavigationDocumentName);
            project.SetDefaultDocumentName(input.DefaultDocumentName);
            project.SetConcurrencyStampIfNotNull(input.ConcurrencyStamp);

            project.MinimumVersion = input.MinimumVersion;
            project.MainWebsiteUrl = input.MainWebsiteUrl;
            project.LatestVersionBranchName = input.LatestVersionBranchName;

            foreach (var extraProperty in input.ExtraProperties)
            {
                project.SetProperty(extraProperty.Key, extraProperty.Value);
            }

            project = await _projectRepository.UpdateAsync(project);

            return ObjectMapper.Map<Project, ProjectDto>(project);
        }

        [Authorize(DocsAdminPermissions.Projects.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            await _projectRepository.DeleteAsync(id);
        }

        public virtual async Task ReindexAsync(ReindexInput input)
        {
            _elasticSearchService.ValidateElasticSearchEnabled();

            await ReindexProjectAsync(input.ProjectId);
        }

        private async Task ReindexProjectAsync(Guid projectId)
        {
            var project = await _projectRepository.FindAsync(projectId);
            if (project == null)
            {
                throw new Exception("Cannot find the project with the Id " + projectId);
            }

            await _elasticSearchService.DeleteAllByProjectIdAsync(project.Id);

            var docsCount = await _documentRepository.GetUniqueDocumentCountByProjectIdAsync(projectId);

            if (docsCount == 0)
            {
                return;
            }

            const int maxResultCount = 1000;

            var skipCount = 0;
            while(skipCount < docsCount)
            {
                var docs = await _documentRepository.GetUniqueDocumentsByProjectIdPagedAsync(projectId, skipCount, maxResultCount);
                docs = docs.Where(doc => doc.FileName != project.NavigationDocumentName && doc.FileName != project.ParametersDocumentName).ToList();
                if (!docs.Any())
                {
                    return;
                }
                await _elasticSearchService.AddOrUpdateManyAsync(docs);
                skipCount += maxResultCount;
            }
        }

        public virtual async Task ReindexAllAsync()
        {
            _elasticSearchService.ValidateElasticSearchEnabled();
            var projects = await _projectRepository.GetListAsync();

            foreach (var project in projects)
            {
                await ReindexProjectAsync(project.Id);
            }
        }

        public virtual async Task<List<ProjectWithoutDetailsDto>> GetListWithoutDetailsAsync()
        {
            var projects = await _projectRepository.GetListWithoutDetailsAsync();
            return ObjectMapper.Map<List<ProjectWithoutDetails>, List<ProjectWithoutDetailsDto>>(projects);
        }

        [Authorize(DocsAdminPermissions.Projects.ManagePdfFiles)]
        public virtual async Task<PagedResultDto<ProjectPdfFileDto>> GetPdfFilesAsync(GetPdfFilesInput input)
        {
            var project = await _projectRepository.GetAsync(input.ProjectId, includeDetails: true);

            var pdfFiles = project.PdfFiles.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

            return new PagedResultDto<ProjectPdfFileDto>(
                project.PdfFiles.Count,
                ObjectMapper.Map<List<ProjectPdfFile>, List<ProjectPdfFileDto>>(pdfFiles)
            );
        }

        [Authorize(DocsAdminPermissions.Projects.ManagePdfFiles)]
        public virtual async Task DeletePdfFileAsync(DeletePdfFileInput input)
        {
            var project = await _projectRepository.GetAsync(input.ProjectId, includeDetails: true);
            project.RemovePdfFile(_pdfGeneratorOptions.Value.CalculatePdfFileName(project, input.Version, input.LanguageCode));
            
            await _projectRepository.UpdateAsync(project);
        }
    }
}
