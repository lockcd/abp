using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Volo.Abp.BlobStoring;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Docs.Projects;

namespace Volo.Docs.Documents.Pdf;

public class BlobDocumentPdfFileStore : IDocumentPdfFileStore, ITransientDependency
{
    protected IBlobContainer<DocsDocumentPdfContainer> BlobContainer { get; }
    protected IDistributedCache<DocsDocumentPdfCacheItem> Cache { get; }
    protected IOptions<DocsDocumentPdfGeneratorOptions> Options { get; }
    
    public BlobDocumentPdfFileStore(
        IBlobContainer<DocsDocumentPdfContainer> blobContainer, 
        IDistributedCache<DocsDocumentPdfCacheItem> cache,
        IOptions<DocsDocumentPdfGeneratorOptions> options)
    {
        BlobContainer = blobContainer;
        Cache = cache;
        Options = options;
    }
    
    public virtual async Task SetAsync(Project project, string version, string languageCode, Stream stream)
    {
        await BlobContainer.SaveAsync(Options.Value.CalculatePdfFileName(project, version, languageCode), stream);
      
        await Cache.SetAsync(DocsDocumentPdfCacheItem.CalculateCacheKey(project.Id, version, languageCode), new DocsDocumentPdfCacheItem(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.Add(Options.Value.PdfFileCacheExpiration)
            });
    }

    public virtual async Task<Stream> GetOrNullAsync(Project project, string version, string languageCode)
    {
        var cache = await Cache.GetAsync(DocsDocumentPdfCacheItem.CalculateCacheKey(project.Id, version, languageCode));
        if (cache == null)
        {
            return null;
        }

        return await BlobContainer.GetOrNullAsync(Options.Value.CalculatePdfFileName(project, version, languageCode));
    }
}