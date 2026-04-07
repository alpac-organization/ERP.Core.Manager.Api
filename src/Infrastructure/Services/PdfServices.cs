using DinkToPdf;
using DinkToPdf.Contracts;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    public class PdfServices(IConverter _converter) : IPdfServices
    {
        public byte[] GeneratePdfFromHtml(string htmlContent)
        {
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Landscape,
                    PaperSize = PaperKind.Letter,
                    Margins = new MarginSettings { Top = 0, Bottom = 0, Left = 0, Right = 0 }
                },
                Objects = {
                    new ObjectSettings() {
                        HtmlContent = htmlContent,
                        WebSettings = { DefaultEncoding = "utf-8", Background = true }
                    }
                }
            };
            
            return _converter.Convert(doc);
        }
    }
}