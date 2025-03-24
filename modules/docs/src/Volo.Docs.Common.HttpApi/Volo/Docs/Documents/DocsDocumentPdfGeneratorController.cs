using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Docs.Common;
using Volo.Docs.Common.Documents;

namespace Volo.Docs.Documents;

[RemoteService(Name = DocsCommonRemoteServiceConsts.RemoteServiceName)]
[Area(DocsCommonRemoteServiceConsts.ModuleName)]
[ControllerName("Document")]
[Route("api/docs/documents")]
public class DocsDocumentPdfGeneratorController : DocsControllerBase, IDocumentPdfGeneratorAppService
{
    protected IDocumentPdfGeneratorAppService DocumentPdfGeneratorAppService { get; }
    
    public DocsDocumentPdfGeneratorController(IDocumentPdfGeneratorAppService documentPdfGeneratorAppService)
    {
        DocumentPdfGeneratorAppService = documentPdfGeneratorAppService;
    }
    
    [HttpGet]
    [Route("pdf")]
    public Task<IRemoteStreamContent> GeneratePdfAsync(DocumentPdfGeneratorInput input)
    {
        return DocumentPdfGeneratorAppService.GeneratePdfAsync(input);
    }
}