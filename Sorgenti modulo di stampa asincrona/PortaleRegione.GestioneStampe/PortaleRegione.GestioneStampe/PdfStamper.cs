﻿using System;
using System.Collections.Generic;
using System.IO;
using HtmlAgilityPack;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using PortaleRegione.DTO.Domain;
using PortaleRegione.GestioneStampe.Helpers;
using PortaleRegione.Logger;

namespace PortaleRegione.GestioneStampe
{
    public class PdfStamper
    {
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
                            var ev = new ITextEvents {EM = em};
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
                            catch (Exception ex)
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
                                    var linkPemError = $"{urlPEM}/{em.UIDEM}";
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


                //**********************************************************************************
                //MAX: ELIMINO LA FUFFA
                //**********************************************************************************
                //try
                //{
                //    if (File.Exists(path)) File.Delete(path);
                //}
                //catch (Exception ex)
                //{
                //    Log.Error("[CreaPDF]: Impossibile eliminare il file temporaneo " + path + " -->", ex);
                //}

                //**********************************************************************************
            }
            catch (Exception ex)
            {
                Log.Error("CreaPDF Error-->", ex);
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
                Log.Error("CreaPDFCopertina Error-->", ex);
                throw ex;
            }
        }

        public static void CreateMergedPDF(string targetPDF, string pathcopertina,
            Dictionary<Guid, string> listaEmendamentiGenerati)
        {
            try
            {
                using (var stream = new FileStream(targetPDF, FileMode.Create))
                {
                    var pdfDoc = new Document(PageSize.A4);
                    var pdf = new PdfCopy(pdfDoc, stream);
                    pdfDoc.Open();
                    //Aggiungo la copertina
                    pdf.AddDocument(new PdfReader(pathcopertina));

                    foreach (var item in listaEmendamentiGenerati) pdf.AddDocument(new PdfReader(item.Value));

                    if (pdfDoc != null)
                        pdfDoc.Close();

                    Log.Debug("PDF merge complete.");
                }
            }
            catch (Exception ex)
            {
                Log.Error("CreateMergedPDF Error-->", ex);
                throw ex;
            }
        }
    }
}