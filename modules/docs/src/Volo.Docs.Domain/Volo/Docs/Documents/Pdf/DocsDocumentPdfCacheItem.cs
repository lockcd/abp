using System;

namespace Volo.Docs.Documents.Pdf;

public class DocsDocumentPdfCacheItem
{
    public static string CalculateCacheKey(Guid projectId, string version, string languageCode)
    {
        return $"{projectId}_{version}_{languageCode}";
    }
}