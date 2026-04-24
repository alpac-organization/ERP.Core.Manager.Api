using ERP.Core.Application.Commons.Interfaces;
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
            
            var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
            var dockerPath = Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH");

            var options = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = isDocker ? dockerPath : null,
                Args = isDocker 
                    ? [
                        "--no-sandbox", 
                        "--disable-setuid-sandbox", 
                        "--disable-dev-shm-usage", 
                        "--disable-gpu",
                        "--no-zygote",
                        "--single-process" 
                    ]
                    : ["--no-sandbox"]
            };

            if (!isDocker)
            {
                await new BrowserFetcher().DownloadAsync();
            }

            await using var browser = await Puppeteer.LaunchAsync(options);
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

            await browser.CloseAsync();
            return pdfBytes;
        }
    }
}