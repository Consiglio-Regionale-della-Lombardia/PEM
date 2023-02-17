using IronPdf;
using System;
using System.Collections.Generic;
using System.IO;
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

        public async Task<byte[]> CreaPDFInMemory(string body, string nome_documento, List<string> attachments = null)
        {
            try
            {
                var Renderer = SetupRender();
                var pdf = await Renderer.RenderHtmlAsPdfAsync(body);
                Renderer.PrintOptions.TextFooter.RightText = $"{nome_documento} " + "Pagina {page} di {total-pages}";
                if (attachments != null)
                {
                    if (attachments.Any())
                    {
                        foreach (var attachment in attachments)
                        {
                            if (!File.Exists(attachment))
                                continue;

                            if (Path.GetExtension(attachment)
                                .Equals(".pdf", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var attach = PdfDocument.FromFile(attachment);
                                pdf.AppendPdf(attach);
                                continue;
                            }

                            pdf.Attachments.AddAttachment(
                                $"Allegato_{pdf.Attachments.Count() + 1}{Path.GetExtension(attachment)}",
                                File.ReadAllBytes(attachment));
                        }
                    }
                }

                return pdf.Stream.ToArray();
            }
            catch (Exception ex)
            {
                ////Log.Error("CreaPDFInMemory DASI Error-->", ex);
                throw ex;
            }
        }

        public async void CreaPDF(string txtHTML, string path)
        {
            var Renderer = SetupRender();
            var pdf = await Renderer.RenderHtmlAsPdfAsync(txtHTML);
            Renderer.PrintOptions.TextFooter.RightText = "Pagina {page} di {total-pages}";
            pdf.SaveAs(path);
        }

        public async Task<object> CreaPDFObject(string txtHTML, List<string> attachments = null)
        {
            var Renderer = SetupRender();
            var pdf = await Renderer.RenderHtmlAsPdfAsync(txtHTML);
            Renderer.PrintOptions.TextFooter.RightText = "Pagina {page} di {total-pages}";
            if (attachments == null) return pdf;
            if (!attachments.Any()) return pdf;

            foreach (var attach in from attachment in attachments
                                   where File.Exists(attachment)
                                   where Path.GetExtension(attachment)
.Equals(".pdf", StringComparison.InvariantCultureIgnoreCase)
                                   select PdfDocument.FromFile(attachment))
            {
                pdf.AppendPdf(attach);
            }

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
            Renderer.PrintOptions.TextFooter.RightText = "Pagina {page} di {total-pages}";
            Renderer.PrintOptions.TextFooter.DrawDividerLine = true;
            PdfDocument.Merge(listPdf).SaveAs(path);
        }
    }
}