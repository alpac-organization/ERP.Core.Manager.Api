using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    public class PdfGeneratorServices(ITemplateServices templateServices) : IPdfGeneratorServices
    {
        private static IBrowser? _browser;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        private static async Task<IBrowser> GetBrowserAsync()
        {
            if (_browser != null)
                return _browser;

            await _semaphore.WaitAsync();
            try
            {
                if (_browser == null)
                {
                    // 🔥 Descargar Chromium UNA sola vez
                    var browserFetcher = new BrowserFetcher();
                    await browserFetcher.DownloadAsync();

                    // 🚀 Lanzar browser
                    _browser = await Puppeteer.LaunchAsync(new LaunchOptions
                    {
                        Headless = true,
                        Args = new[]
                        {
                            "--no-sandbox",
                            "--disable-setuid-sandbox",
                            "--disable-dev-shm-usage",
                            "--disable-gpu",
                            "--no-zygote"
                        }
                    });
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return _browser!;
        }

        public async Task<byte[]> GenerateAsync<T>(string templateName, object data)
        {
            string templateContent = templateServices.Render(templateName, data);

            var browser = await GetBrowserAsync();

            await using var page = await browser.NewPageAsync();

            await page.SetContentAsync(templateContent, new NavigationOptions
            {
                WaitUntil = [WaitUntilNavigation.Networkidle0]
            });

            var pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true
            });

            await page.CloseAsync();

            return pdfBytes;
        }
    }
}