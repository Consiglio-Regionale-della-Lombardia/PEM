/*
 * Copyright (C) 2019 Consiglio Regionale della Lombardia
 * SPDX-License-Identifier: AGPL-3.0-or-later
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

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
                // Ogni thread crea la propria istanza di Renderer per evitare conflitti.
                var renderer = SetupRender();

                // Configurazione del footer (non condiviso tra thread)
                if (!string.IsNullOrEmpty(nome_documento))
                    renderer.PrintOptions.Footer.RightText = $"{nome_documento}" + " Pagina {page} di {total-pages}";
                else
                    renderer.PrintOptions.Footer.RightText = "Pagina {page} di {total-pages}";

                // Render PDF dal contenuto HTML
                using (var pdf = await renderer.RenderHtmlAsPdfAsync(body))
                {
                    // Gestione degli allegati
                    if (attachments != null && attachments.Any())
                    {
                        foreach (var attachment in attachments)
                        {
                            if (!File.Exists(attachment))
                                continue;

                            if (Path.GetExtension(attachment).Equals(".pdf", StringComparison.InvariantCultureIgnoreCase))
                            {
                                // Evitare conflitti tra thread che accedono allo stesso file
                                using (var attach = PdfDocument.FromFile(attachment))
                                {
                                    pdf.AppendPdf(attach);
                                }
                            }
                        }
                    }

                    // Salvataggio del PDF (ogni thread salva in percorsi separati)
                    pdf.SaveAs(path);
                }
            }
            catch (Exception ex)
            {
                // Log dell'errore
                // Log.Error("CreaPDFAsync Error-->", ex);

                // Rilancio dell'eccezione con il suo stack originale
                throw;
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
            var batchSize = 250;
            var maxRetryAttempts = 3;

            var mergedBatchList = new List<PdfDocument>();
            PdfDocument finalDocument = null;

            try
            {
                // Verifica se il file esiste e lo carica come documento base
                if (File.Exists(path))
                {
                    finalDocument = new PdfDocument(path);
                }

                for (var i = 0; i < docs.Count; i += batchSize)
                {
                    var retryCount = 0;
                    var success = false;
                    while (retryCount < maxRetryAttempts && success == false)
                    {
                        try
                        {
                            var batch = docs.Skip(i).Take(batchSize).ToList();
                            var listPdf = batch.Select(p => new PdfDocument(p)).ToList();

                            var mergedBatch = PdfDocument.Merge(listPdf);
                            mergedBatchList.Add(mergedBatch);

                            // Dispose individual PDFs to free memory
                            foreach (var doc in listPdf) doc.Dispose();

                            success = true;
                        }
                        catch (Exception)
                        {
                            retryCount++;
                        }
                    }

                    if (!success)
                        throw new Exception("Max retry attempts reached. Unable to process the batch.");
                }

                // Se finalDocument esiste già, aggiungi i batch a esso
                if (finalDocument != null)
                {
                    foreach (var batch in mergedBatchList)
                    {
                        finalDocument.AppendPdf(batch);
                    }
                }
                else
                {
                    // Altrimenti, crea un nuovo documento unendo i batch
                    finalDocument = PdfDocument.Merge(mergedBatchList);
                }

                finalDocument.SaveAs(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                // Dispose merged batch documents to free memory
                foreach (var doc in mergedBatchList) doc.Dispose();
                // Dispose the final merged document
                finalDocument?.Dispose();
            }
        }


        public void MergedPDFWithRetryOLD(string path, List<string> docs)
        {
            var batchSize = 250;
            var maxRetryAttempts = 3;

            var mergedBatchList = new List<PdfDocument>();
            PdfDocument finalDocument = null;

            try
            {
                for (var i = 0; i < docs.Count; i += batchSize)
                {
                    var retryCount = 0;
                    var success = false;
                    while (retryCount < maxRetryAttempts && success == false)
                    {
                        try
                        {
                            var batch = docs.Skip(i).Take(batchSize).ToList();
                            var listPdf = batch.Select(p => new PdfDocument(p)).ToList();
                            var mergedBatch = PdfDocument.Merge(listPdf);

                            mergedBatchList.Add(mergedBatch);

                            // Dispose individual PDFs to free memory
                            foreach (var doc in listPdf) doc.Dispose();

                            success = true;
                        }
                        catch (Exception)
                        {
                            retryCount++;
                        }
                    }

                    if (!success)
                        throw new Exception("Max retry attempts reached. Unable to process the batch.");
                }

                // Merge all batches into the final document
                finalDocument = PdfDocument.Merge(mergedBatchList);
                finalDocument.SaveAs(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                // Dispose merged batch documents to free memory
                foreach (var doc in mergedBatchList) doc.Dispose();
                // Dispose the final merged document
                finalDocument?.Dispose();
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