using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronPdf;

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
                if (!string.IsNullOrEmpty(nome_documento))
                    Renderer.PrintOptions.Footer.RightText = $"{nome_documento}" + " Pagina {page} di {total-pages}";
                using (var pdf = await Renderer.RenderHtmlAsPdfAsync(body))
                {
                    if (attachments != null)
                        if (attachments.Any())
                            foreach (var attachment in attachments)
                            {
                                if (!File.Exists(attachment))
                                    continue;

                                if (Path.GetExtension(attachment)
                                    .Equals(".pdf", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    var attach = PdfDocument.FromFile(attachment);
                                    pdf.AppendPdf(attach);
                                }
                            }

                    return pdf.Stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                //Log.Error("CreaPDFInMemory Error-->", ex);
                throw ex;
            }
        }

        public async Task CreaPDFAsync(string path, string body, string nome_documento, List<string> attachments = null)
        {
            try
            {
                var Renderer = SetupRender();
                if (!string.IsNullOrEmpty(nome_documento))
                    Renderer.PrintOptions.Footer.RightText = $"{nome_documento}" + " Pagina {page} di {total-pages}";
                using (var pdf = await Renderer.RenderHtmlAsPdfAsync(body))
                {
                    if (attachments != null)
                        if (attachments.Any())
                            foreach (var attachment in attachments)
                            {
                                if (!File.Exists(attachment))
                                    continue;

                                if (Path.GetExtension(attachment)
                                    .Equals(".pdf", StringComparison.InvariantCultureIgnoreCase))
                                    using (var attach = PdfDocument.FromFile(attachment))
                                    {
                                        pdf.AppendPdf(attach);
                                    }
                            }

                    pdf.SaveAs(path);
                }
            }
            catch (Exception ex)
            {
                //Log.Error("CreaPDFInMemory Error-->", ex);
                throw ex;
            }
        }

        public void CreaPDF(string path, string body, string nome_documento, List<string> attachments = null)
        {
            try
            {
                var Renderer = SetupRender();
                if (!string.IsNullOrEmpty(nome_documento))
                    Renderer.PrintOptions.Footer.RightText = $"{nome_documento}" + " Pagina {page} di {total-pages}";
                using (var pdf = Renderer.RenderHtmlAsPdf(body))
                {
                    if (attachments != null)
                        if (attachments.Any())
                            foreach (var attachment in attachments)
                            {
                                if (!File.Exists(attachment))
                                    continue;

                                if (Path.GetExtension(attachment)
                                    .Equals(".pdf", StringComparison.InvariantCultureIgnoreCase))
                                    using (var attach = PdfDocument.FromFile(attachment))
                                    {
                                        pdf.AppendPdf(attach);
                                    }
                            }

                    pdf.SaveAs(path);
                }
            }
            catch (Exception ex)
            {
                //Log.Error("CreaPDFInMemory Error-->", ex);
                throw ex;
            }
        }

        public async Task<byte[]> CreaPDFInMemory(string body)
        {
            return await CreaPDFInMemory(body, string.Empty);
        }

        public void CreaPDF(string txtHTML, string path)
        {
            var Renderer = SetupRender();
            Renderer.PrintOptions.Footer.RightText = "Pagina {page} di {total-pages}";
            using (var pdf = Renderer.RenderHtmlAsPdf(txtHTML))
            {
                pdf.SaveAs(path);
            }
        }

        public async Task<object> CreaPDFObject(string txtHTML, bool abilitaPaginazione = true,
            List<string> attachments = null)
        {
            var Renderer = SetupRender();
            if (abilitaPaginazione)
                Renderer.PrintOptions.Footer.RightText = "Pagina {page} di {total-pages}";
            using (var pdf = await Renderer.RenderHtmlAsPdfAsync(txtHTML))
            {
                if (attachments == null) return pdf;
                if (!attachments.Any()) return pdf;

                foreach (var attach in from attachment in attachments
                         where File.Exists(attachment)
                         where Path.GetExtension(attachment)
                             .Equals(".pdf", StringComparison.InvariantCultureIgnoreCase)
                         select PdfDocument.FromFile(attachment))
                    pdf.AppendPdf(attach);

                return pdf;
            }
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
            var batchSize = 100;
            for (var i = 0; i < docs.Count; i += batchSize)
            {
                var batch = docs.Skip(i).Take(batchSize).ToList();
                var listPdf = batch.Select(doc => (PdfDocument)doc);
                PdfDocument.Merge(listPdf).SaveAs(path);
            }
        }

        public void MergedPDF(string path, List<string> docs)
        {
            var batchSize = 100;

            for (var i = 0; i < docs.Count; i += batchSize)
                try
                {
                    var fascicolo = new PdfDocument(path);
                    var batch = docs.Skip(i).Take(batchSize).ToList();
                    var listPdf = batch.Select(p => new PdfDocument(p)).ToList();
                    listPdf.Insert(0, fascicolo);
                    PdfDocument.Merge(listPdf).SaveAs(path);
                    foreach (var doc in listPdf) doc.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
        }

        public void MergedPDFWithRetry(string path, List<string> docs)
        {
            var batchSize = 1000;
            var maxRetryAttempts = 3;
            var mergedBatchList = new List<PdfDocument> { new PdfDocument(path) };

            for (var i = 0; i < docs.Count; i += batchSize)
            {
                var retryCount = 0;
                var success = false;
                while (retryCount < maxRetryAttempts && success == false)
                    try
                    {
                        var batch = docs.Skip(i).Take(batchSize).ToList();
                        var listPdf = batch.Select(p => new PdfDocument(p)).ToList();
                        mergedBatchList.Add(PdfDocument.Merge(listPdf));
                        foreach (var doc in listPdf) doc.Dispose();

                        success = true;
                    }
                    catch (Exception)
                    {
                        retryCount++;
                    }

                if (!success) throw new Exception("Max retry attempts reached. Unable to process the batch.");
            }

            try
            {
                PdfDocument.Merge(mergedBatchList).SaveAs(path);
                foreach (var doc in mergedBatchList) doc.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void MergedPDF(string path, List<byte[]> docs)
        {
            var batchSize = 100;
            for (var i = 0; i < docs.Count; i += batchSize)
            {
                var batch = docs.Skip(i).Take(batchSize).ToList();
                var listPdf = batch.Select(doc => new PdfDocument(doc)).ToList();
                if (File.Exists(path)) listPdf.Insert(0, new PdfDocument(path));

                PdfDocument.Merge(listPdf).SaveAs(path);
            }
        }

        public void MergedPDF(string pathFascicolo, byte[] fileToAppend, ref PdfDocument fascicolo)
        {
            var listPdf = new List<PdfDocument>();
            if (File.Exists(pathFascicolo)) listPdf.Insert(0, fascicolo);

            listPdf.Add(new PdfDocument(fileToAppend));

            fascicolo = PdfDocument.Merge(listPdf).SaveAs(pathFascicolo);
        }

        public void MergedPDF(string pathFascicolo, byte[] fileToAppend)
        {
            var listPdf = new List<PdfDocument>();
            if (File.Exists(pathFascicolo)) listPdf.Insert(0, new PdfDocument(pathFascicolo));

            listPdf.Add(new PdfDocument(fileToAppend));

            PdfDocument.Merge(listPdf).SaveAs(pathFascicolo);
        }

        public byte[] MergedPDFInMemory(string path, List<object> docs)
        {
            var listPdf = docs.Select(i => (PdfDocument)i);
            var Renderer = SetupRender();
            Renderer.PrintOptions.Footer.RightText = "Pagina {page} di {total-pages}";
            Renderer.PrintOptions.Footer.DrawDividerLine = true;
            return PdfDocument.Merge(listPdf).Stream.ToArray();
        }
    }
}