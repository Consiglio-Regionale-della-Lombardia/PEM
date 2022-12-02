using HtmlAgilityPack;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using PortaleRegione.DTO.Domain;
using PortaleRegione.GestioneStampe.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace PortaleRegione.GestioneStampe
{
    public sealed class PdfStamper
    {
        static PdfStamper _instance;

        public static PdfStamper Instance => _instance ?? (_instance = new PdfStamper());

        public PdfStamper()
        {
        }

        public static void CreaPDF(string txtHTML, string path, EmendamentiDto em, string urlPEM)
        {
            try
            {
                if (string.IsNullOrEmpty(txtHTML))
                    throw new Exception("Nessun testo da inserire nel PDF.");
                if (string.IsNullOrEmpty(path))
                    throw new Exception("Percorso del PDF non valido.");
                //Create a byte array that will eventually hold our final PDF
                byte[] bytes;

                //Boilerplate iTextSharp setup here
                //Create a stream that we can write to, in this case a MemoryStream
                using (var ms = new MemoryStream())
                {
                    //Create an iTextSharp Document which is an abstraction of a PDF but **NOT** a PDF
                    using (var doc = new Document(new Rectangle(600, 800), 20, 20, 20, 60))
                    {
                        //Create a writer that's bound to our PDF abstraction and our stream
                        using (var writer = PdfWriter.GetInstance(doc, ms))
                        {
                            var ev = new ITextEvents { EM = em };
                            writer.PageEvent = ev;
                            //Open the document for writing
                            doc.Open();

                            //XMLWorker also reads from a TextReader and not directly from a string
                            var hDocument = new HtmlDocument
                            {
                                OptionWriteEmptyNodes = true,
                                OptionAutoCloseOnEnd = true
                            };
                            hDocument.LoadHtml(txtHTML);
                            txtHTML = hDocument.DocumentNode.WriteTo();

                            try
                            {
                                //Parse the HTML
                                using (var srHtml = new StringReader(txtHTML))
                                {
                                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml);
                                }
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    using (var srHtml =
                                        new StringReader(txtHTML.Replace("<ol", "<div").Replace("</ol>", "</div>")))
                                    {
                                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml);
                                    }
                                }
                                catch (Exception ex2)
                                {
                                    var linkPemError = urlPEM;
                                    using (var srHtml_ERR = new StringReader(
                                        $"<html><body>ATTENZIONE, Si è verificato un problema durante la generazione del pdf di questo EM/SUBEM: {ex2.Message} <br/> L'emendamento/subemendamento è stato comunque correttamente acquisito dal sistema ed è visualizzabile attraverso la piattaforma PEM all'indirizzo <a href='{linkPemError}'>{linkPemError}</a></body></html>")
                                    )
                                    {
                                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml_ERR);
                                    }
                                }
                            }

                            doc.Close();
                        }
                    }

                    //After all of the PDF "stuff" above is done and closed but **before** we
                    //close the MemoryStream, grab all of the active bytes from the stream
                    bytes = ms.ToArray();
                }

                //Now we just need to do something with those bytes.
                //Here I'm writing them to disk but if you were in ASP.Net you might Response.BinaryWrite() them.
                //You could also write the bytes to a database in a varbinary() column (but please don't) or you
                //could pass them to another function for further PDF processing.
                //var testFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test.pdf");
                File.WriteAllBytes(path, bytes);
            }
            catch (Exception ex)
            {
                ////Log.Error("CreaPDF Error-->", ex);
                throw ex;
            }
        }

        public static byte[] CreaPDFInMemory(string txtHTML, EmendamentiDto em, string urlPEM)
        {
            byte[] bytes;
            try
            {
                if (string.IsNullOrEmpty(txtHTML))
                    throw new Exception("Nessun testo da inserire nel PDF.");

                //Boilerplate iTextSharp setup here
                //Create a stream that we can write to, in this case a MemoryStream
                using (var ms = new MemoryStream())
                {
                    //Create an iTextSharp Document which is an abstraction of a PDF but **NOT** a PDF
                    using (var doc = new Document(new Rectangle(600, 800), 20, 20, 20, 60))
                    {
                        //Create a writer that's bound to our PDF abstraction and our stream
                        using (var writer = PdfWriter.GetInstance(doc, ms))
                        {
                            var ev = new ITextEvents { EM = em };
                            writer.PageEvent = ev;
                            //Open the document for writing
                            doc.Open();

                            //XMLWorker also reads from a TextReader and not directly from a string
                            var hDocument = new HtmlDocument
                            {
                                OptionWriteEmptyNodes = true,
                                OptionAutoCloseOnEnd = true
                            };
                            hDocument.LoadHtml(txtHTML);
                            txtHTML = hDocument.DocumentNode.WriteTo();

                            try
                            {
                                //Parse the HTML
                                using (var srHtml = new StringReader(txtHTML))
                                {
                                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml);
                                }
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    using (var srHtml =
                                        new StringReader(txtHTML.Replace("<ol", "<div").Replace("</ol>", "</div>")))
                                    {
                                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml);
                                    }
                                }
                                catch (Exception ex2)
                                {
                                    var linkPemError = urlPEM;
                                    using (var srHtml_ERR = new StringReader(
                                        $"<html><body>ATTENZIONE, Si è verificato un problema durante la generazione del pdf di questo EM/SUBEM: {ex2.Message} <br/> L'emendamento/subemendamento è stato comunque correttamente acquisito dal sistema ed è visualizzabile attraverso la piattaforma PEM all'indirizzo <a href='{linkPemError}'>{linkPemError}</a></body></html>")
                                    )
                                    {
                                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml_ERR);
                                    }
                                }
                            }

                            doc.Close();
                        }
                    }
                    bytes = ms.ToArray();
                }

                return bytes;
            }
            catch (Exception ex)
            {
                //Log.Error("CreaPDFInMemory Error-->", ex);
                throw ex;
            }
        }

        public static void CreaPDFCopertina(string txtHTML, string pathcopertina)
        {
            try
            {
                if (string.IsNullOrEmpty(txtHTML))
                    throw new Exception("Nessun testo da inserire nel PDF Copertina.");
                if (string.IsNullOrEmpty(pathcopertina))
                    throw new Exception("Percorso del PDF Copertina non valido.");

                //Create a byte array that will eventually hold our final PDF
                byte[] bytes;

                //Boilerplate iTextSharp setup here
                //Create a stream that we can write to, in this case a MemoryStream
                using (var ms = new MemoryStream())
                {
                    //Create an iTextSharp Document which is an abstraction of a PDF but **NOT** a PDF
                    using (var doc = new Document())
                    {
                        //Create a writer that's bound to our PDF abstraction and our stream
                        using (var writer = PdfWriter.GetInstance(doc, ms))
                        {
                            //Open the document for writing
                            doc.Open();

                            //XMLWorker also reads from a TextReader and not directly from a string
                            using (var srHtml = new StringReader(txtHTML))
                            {
                                //Parse the HTML
                                XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml);
                            }

                            doc.Close();
                        }
                    }

                    //After all of the PDF "stuff" above is done and closed but **before** we
                    //close the MemoryStream, grab all of the active bytes from the stream
                    bytes = ms.ToArray();
                }

                //Now we just need to do something with those bytes.
                //Here I'm writing them to disk but if you were in ASP.Net you might Response.BinaryWrite() them.
                //You could also write the bytes to a database in a varbinary() column (but please don't) or you
                //could pass them to another function for further PDF processing.
                File.WriteAllBytes(pathcopertina, bytes);
            }
            catch (Exception ex)
            {
                //Log.Error("CreaPDFCopertina Error-->", ex);
                throw ex;
            }
        }

        public static void CreateMergedPDF(string targetPDF, string pathcopertina,
            Dictionary<Guid, string> lista)
        {
            try
            {
                using (var stream = new FileStream(targetPDF, FileMode.Create))
                {
                    var pdfDoc = new Document(PageSize.A4);
                    var pdf = new PdfCopy(pdfDoc, stream);
                    pdfDoc.Open();
                    if (!string.IsNullOrEmpty(pathcopertina))
                    {
                        if (File.Exists(pathcopertina))
                        {
                            //Aggiungo la copertina
                            pdf.AddDocument(new PdfReader(pathcopertina));
                        }
                    }

                    foreach (var item in lista)
                    {
                        pdf.AddDocument(new PdfReader(item.Value));
                    }

                    pdfDoc.Close();

                    //Log.Debug("PDF merge complete.");
                }
            }
            catch (Exception ex)
            {
                //Log.Error("CreateMergedPDF Error-->", ex);
                throw ex;
            }
        }

        public static void CreaPDF(string txtHTML, string path, AttoDASIDto atto, string url)
        {
            try
            {
                if (string.IsNullOrEmpty(txtHTML))
                    throw new Exception("Nessun testo da inserire nel PDF.");
                if (string.IsNullOrEmpty(path))
                    throw new Exception("Percorso del PDF non valido.");
                //Create a byte array that will eventually hold our final PDF
                byte[] bytes;

                //Boilerplate iTextSharp setup here
                //Create a stream that we can write to, in this case a MemoryStream
                using (var ms = new MemoryStream())
                {
                    //Create an iTextSharp Document which is an abstraction of a PDF but **NOT** a PDF
                    using (var doc = new Document(new Rectangle(600, 800), 20, 20, 20, 60))
                    {
                        //Create a writer that's bound to our PDF abstraction and our stream
                        using (var writer = PdfWriter.GetInstance(doc, ms))
                        {
                            var ev = new ITextEvents { Atto = atto };
                            writer.PageEvent = ev;
                            //Open the document for writing
                            doc.Open();

                            //XMLWorker also reads from a TextReader and not directly from a string
                            var hDocument = new HtmlDocument
                            {
                                OptionWriteEmptyNodes = true,
                                OptionAutoCloseOnEnd = true
                            };
                            hDocument.LoadHtml(txtHTML);
                            txtHTML = hDocument.DocumentNode.WriteTo();

                            try
                            {
                                //Parse the HTML
                                using (var srHtml = new StringReader(txtHTML))
                                {
                                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml);
                                }
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    using (var srHtml =
                                        new StringReader(txtHTML.Replace("<ol", "<div").Replace("</ol>", "</div>")))
                                    {
                                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml);
                                    }
                                }
                                catch (Exception ex2)
                                {
                                    var linkPemError = url;
                                    using (var srHtml_ERR = new StringReader(
                                        $"<html><body>ATTENZIONE, Si è verificato un problema durante la generazione del pdf di questo atto: {ex2.Message} <br/> L'atto è stato comunque correttamente acquisito dal sistema ed è visualizzabile attraverso la piattaforma all'indirizzo <a href='{linkPemError}'>{linkPemError}</a></body></html>")
                                    )
                                    {
                                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml_ERR);
                                    }
                                }
                            }

                            doc.Close();
                        }
                    }

                    //After all of the PDF "stuff" above is done and closed but **before** we
                    //close the MemoryStream, grab all of the active bytes from the stream
                    bytes = ms.ToArray();
                }

                //Now we just need to do something with those bytes.
                //Here I'm writing them to disk but if you were in ASP.Net you might Response.BinaryWrite() them.
                //You could also write the bytes to a database in a varbinary() column (but please don't) or you
                //could pass them to another function for further PDF processing.
                //var testFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test.pdf");
                File.WriteAllBytes(path, bytes);

            }
            catch (Exception ex)
            {
                //Log.Error("CreaPDF DASI Error-->", ex);
                throw ex;
            }

        }

        public static byte[] CreaPDFInMemory(string txtHTML, AttoDASIDto atto, string url)
        {
            byte[] bytes;
            try
            {
                if (string.IsNullOrEmpty(txtHTML))
                    throw new Exception("Nessun testo da inserire nel PDF.");

                //Boilerplate iTextSharp setup here
                //Create a stream that we can write to, in this case a MemoryStream
                using (var ms = new MemoryStream())
                {
                    //Create an iTextSharp Document which is an abstraction of a PDF but **NOT** a PDF
                    using (var doc = new Document(new Rectangle(600, 800), 20, 20, 20, 60))
                    {
                        //Create a writer that's bound to our PDF abstraction and our stream
                        using (var writer = PdfWriter.GetInstance(doc, ms))
                        {
                            var ev = new ITextEvents { Atto = atto };
                            writer.PageEvent = ev;
                            //Open the document for writing
                            doc.Open();

                            //XMLWorker also reads from a TextReader and not directly from a string
                            var hDocument = new HtmlDocument
                            {
                                OptionWriteEmptyNodes = true,
                                OptionAutoCloseOnEnd = true
                            };
                            hDocument.LoadHtml(txtHTML);
                            txtHTML = hDocument.DocumentNode.WriteTo();

                            try
                            {
                                //Parse the HTML
                                using (var srHtml = new StringReader(txtHTML))
                                {
                                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml);
                                }
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    using (var srHtml =
                                        new StringReader(txtHTML.Replace("<ol", "<div").Replace("</ol>", "</div>")))
                                    {
                                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml);
                                    }
                                }
                                catch (Exception ex2)
                                {
                                    var linkPemError = url;
                                    using (var srHtml_ERR = new StringReader(
                                        $"<html><body>ATTENZIONE, Si è verificato un problema durante la generazione del pdf di questo atto: {ex2.Message} <br/> L'atto è stato comunque correttamente acquisito dal sistema ed è visualizzabile attraverso la piattaforma all'indirizzo <a href='{linkPemError}'>{linkPemError}</a></body></html>")
                                    )
                                    {
                                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml_ERR);
                                    }
                                }
                            }

                            doc.Close();
                        }
                    }

                    bytes = ms.ToArray();
                }

                return bytes;
            }
            catch (Exception ex)
            {
                //Log.Error("CreaPDFInMemory DASI Error-->", ex);
                throw ex;
            }

        }


    }
}