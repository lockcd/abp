using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;
using Volo.Docs.Projects;
using Volo.Docs.Projects.Pdf;

namespace Volo.Docs.Admin.BackgroundJobs;

public class DocumentPdfGenerateJob : AsyncBackgroundJob<DocumentPdfGenerateJobArgs>, ITransientDependency
{
    protected IProjectPdfGenerator ProjectPdfGenerator { get; }
    protected IProjectRepository ProjectRepository { get; }

    public DocumentPdfGenerateJob(IProjectPdfGenerator projectPdfGenerator, IProjectRepository projectRepository)
    {
        ProjectPdfGenerator = projectPdfGenerator;
        ProjectRepository = projectRepository;
    }

    [UnitOfWork]
    public async override Task ExecuteAsync(DocumentPdfGenerateJobArgs args)
    {
        try
        {
            var project = await ProjectRepository.GetAsync(args.ProjectId, includeDetails: true);
            await ProjectPdfGenerator.GenerateAsync(project, args.Version, args.LanguageCode);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error while generating PDF for project {ProjectId}, version {Version}, language {LanguageCode}", args.ProjectId, args.Version, args.LanguageCode);
        }
    }
}