using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using Volo.Abp.DependencyInjection;

namespace Volo.Docs.Projects.Pdf.Puppeteer;

public class PuppeteerHtmlToPdfRenderer : IHtmlToPdfRenderer, ITransientDependency
{
    public async Task<Stream> RenderAsync(string title, string html, List<PdfDocument> documents)
    {
        await new BrowserFetcher()
        {
            Browser = SupportedBrowser.Chromium
        }.DownloadAsync();

        await using var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(new LaunchOptions { Headless = true, Timeout = 600000, Browser = SupportedBrowser.Chromium });
        await using var page = await browser.NewPageAsync();

        await page.SetContentAsync(html, new NavigationOptions
        {
            Timeout = 600000,
        });

        var pdfOptions = new PdfOptions
        {
            Format = PaperFormat.A4,
            MarginOptions = new MarginOptions { Top = "20mm", Bottom = "20mm", Left = "15mm", Right = "15mm" },
        };

        var stream = await page.PdfStreamAsync(pdfOptions);
        await page.CloseAsync();
        await browser.CloseAsync();
        return stream;
    }
}