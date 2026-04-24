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
            if (data is null)
            {
                return errorManager.ThrowBadRequest<byte[]>("No se encontro la informacion", "ERP:01");
            }

            string templateContent = templateServices.Render(templateName, data);

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions 
            { 
                Headless = true,
                Args = ["--no-sandbox", "--disable-setuid-sandbox"] 
            });

            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(templateContent);

            var pdfOptions = new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "10mm",
                    Bottom = "10mm",
                    Left = "10mm",
                    Right = "10mm"
                }
            };

            byte[] pdfBytes = await page.PdfDataAsync(pdfOptions);

            await browser.CloseAsync();

            return pdfBytes;
        }
    }
}