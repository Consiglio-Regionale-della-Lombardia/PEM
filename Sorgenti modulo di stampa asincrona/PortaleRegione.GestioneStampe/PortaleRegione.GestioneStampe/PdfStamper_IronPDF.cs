using IronPdf;
using PortaleRegione.Common;
using PortaleRegione.DTO.Domain;
using PortaleRegione.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortaleRegione.GestioneStampe
{
    public class PdfStamper_IronPDF
    {
        public PdfStamper_IronPDF(string license)
        {
            License.LicenseKey = license;
        }

        public async Task<byte[]> CreaPDFInMemory(string txtHTML, AttoDASIDto atto)
        {
            try
            {
                var Renderer = SetupRender();
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

        public async void CreaPDF(string txtHTML, string path)
        {
            var Renderer = SetupRender();
            var pdf = await Renderer.RenderHtmlAsPdfAsync(txtHTML);
            pdf.SaveAs(path);
        }

        public async Task<object> CreaPDFObject(string txtHTML)
        {
            var Renderer = SetupRender();
            var pdf = await Renderer.RenderHtmlAsPdfAsync(txtHTML);
            return pdf;
        }

        private HtmlToPdf SetupRender()
        {
            var Renderer = new HtmlToPdf();
            Renderer.PrintOptions.RenderDelay = 200;
            Renderer.PrintOptions.MarginLeft = 10f;
            Renderer.PrintOptions.MarginRight = 10f;
            Renderer.PrintOptions.MarginTop = 10f;
            Renderer.PrintOptions.MarginBottom = 10f;

            return Renderer;
        }

        public void MergedPDF(string path, List<object> docs)
        {
            var listPdf = docs.Select(i => (PdfDocument)i);
            var Renderer = SetupRender();
            Renderer.PrintOptions.Footer.RightText = "Pagina {page} di {total-pages}";
            Renderer.PrintOptions.Footer.DrawDividerLine = true;
            PdfDocument.Merge(listPdf).SaveAs(path);
        }
    }
}