using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    public class PdfGeneratorServices(ITemplateServices templateServices, IErrorManager errorManager): IPdfGeneratorServices
    {
       public async Task<byte[]> GenerateAsync<T>(string templateName, object data)
        {
            string templateContent = templateServices.Render(templateName, data);
            
            var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

            var options = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = isDocker ? "/usr/bin/chromium" : null,
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

            // Solo descargamos en local
            if (!isDocker)
            {
                await new BrowserFetcher().DownloadAsync();
            }

            try 
            {
                await using var browser = await Puppeteer.LaunchAsync(options);
                await using var page = await browser.NewPageAsync();
                
                // Networkidle0 es clave para esperar que carguen estilos/imágenes
                await page.SetContentAsync(templateContent, new NavigationOptions 
                { 
                    WaitUntil = [WaitUntilNavigation.Networkidle0] 
                });

                return await page.PdfDataAsync(new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true
                });
            }
            catch (Exception ex)
            {
                // Esto te permitirá ver el error real en los logs de Render
                Console.WriteLine($"PUPPETEER ERROR: {ex.Message}");
                throw;
            }
        }
    }
}