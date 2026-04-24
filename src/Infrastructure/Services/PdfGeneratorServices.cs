using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    public class PdfGeneratorServices(ITemplateServices templateServices): IPdfGeneratorServices
    {
        public async Task<byte[]> GenerateAsync<T>(string templateName, object data)
        {
            string templateContent = templateServices.Render(templateName, data);

            // 🔥 Siempre descargar Chromium (clave para Docker)
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            var options = new LaunchOptions
            {
                Headless = true,
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-gpu",
                    "--no-zygote",
                    "--single-process"
                }
            };

            await using var browser = await Puppeteer.LaunchAsync(options);
            await using var page = await browser.NewPageAsync();

            await page.SetContentAsync(templateContent, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            });

            var pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true
            });

            await browser.CloseAsync();
            return pdfBytes;
        }
    }
}