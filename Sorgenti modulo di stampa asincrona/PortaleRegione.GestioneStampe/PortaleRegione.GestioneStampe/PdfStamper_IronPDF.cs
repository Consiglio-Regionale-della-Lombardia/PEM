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

        public async Task<byte[]> CreaPDFInMemory(string txtHTML, AttoDASIDto atto, string url)
        {
            try
            {
                var Renderer = new IronPdf.ChromePdfRenderer();
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