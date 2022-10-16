using PortaleRegione.Common;
using PortaleRegione.DTO.Domain;
using PortaleRegione.Logger;
using System;
using System.Threading.Tasks;

namespace PortaleRegione.GestioneStampe
{
    public class PdfStamper_IronPDF
    {
        public PdfStamper_IronPDF(string license)
        {
            IronPdf.License.LicenseKey = license;
        }

        public async Task<byte[]> CreaPDFInMemory(string txtHTML, AttoDASIDto atto)
        {
            try
            {
                var Renderer = new IronPdf.HtmlToPdf();
                Renderer.PrintOptions.RenderDelay = 200;
                Renderer.PrintOptions.MarginLeft = 10f;
                Renderer.PrintOptions.MarginRight = 10f;
                Renderer.PrintOptions.MarginTop = 10f;
                Renderer.PrintOptions.MarginBottom = 10f;

                Renderer.PrintOptions.Footer.RightText = $"{Utility.GetText_Tipo(atto.Tipo)} {atto.NAtto}" +
                                                         " Pagina {page} di {total-pages}";
                Renderer.PrintOptions.Footer.LeftText = "{date} {time}";
                Renderer.PrintOptions.Footer.DrawDividerLine = true;

                var pdf = await Renderer.RenderHtmlAsPdfAsync(txtHTML);
                return pdf.Stream.ToArray();
            }
            catch (Exception ex)
            {
                Log.Error("CreaPDFInMemory DASI Error-->", ex);
                throw ex;
            }
        }
    }
}