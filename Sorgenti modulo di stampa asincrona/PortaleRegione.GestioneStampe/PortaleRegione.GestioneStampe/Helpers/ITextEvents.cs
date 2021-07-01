using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.GestioneStampe.Helpers
{
    internal class ITextEvents : PdfPageEventHelper
    {
        // this is the BaseFont we are going to use for the header / footer
        private BaseFont bf;

        // This is the contentbyte object of the writer
        private PdfContentByte cb;

        // we will put the final number of pages in a template
        private PdfTemplate headerTemplate, footerTemplate, footerTemplate2;

        // This keeps track of the creation time
        private DateTime PrintTime = DateTime.Now;

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                PrintTime = DateTime.Now;
                bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                cb = writer.DirectContent;
                headerTemplate = cb.CreateTemplate(100, 50);
                footerTemplate = cb.CreateTemplate(200, 50);
                footerTemplate2 = cb.CreateTemplate(500, 50);
            }
            catch (DocumentException docEx)
            {
                throw docEx;
            }
            catch (IOException ioEx)
            {
                throw ioEx;
            }
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            try
            {
                base.OnEndPage(writer, document);
                var text = String.Empty;
                switch ((StatiEnum)EM.IDStato)
                {
                    case StatiEnum.Bozza_Riservata:
                        text = nameof(StatiEnum.Bozza_Riservata).Replace("_", " ");
                        break;
                    case StatiEnum.Bozza:
                        text = nameof(StatiEnum.Bozza);
                        break;
                    case StatiEnum.Depositato:
                        break;
                    case StatiEnum.Approvato:
                        text = nameof(StatiEnum.Approvato);
                        break;
                    case StatiEnum.Non_Approvato:
                        text = nameof(StatiEnum.Non_Approvato).Replace("_", " ");
                        break;
                    case StatiEnum.Ritirato:
                        text = nameof(StatiEnum.Ritirato);
                        break;
                    case StatiEnum.Decaduto:
                        text = nameof(StatiEnum.Decaduto);
                        break;
                    case StatiEnum.Inammissibile:
                        text = nameof(StatiEnum.Inammissibile);
                        break;
                    case StatiEnum.Approvato_Con_Modifiche:
                        text = nameof(StatiEnum.Approvato_Con_Modifiche).Replace("_", " ");
                        break;
                }

                var dtdeposito = "--"; //MAX: cambio il nome della variabile da dtscadenza in dtdeposito
                if (EM.IDStato >= (int) StatiEnum.Depositato)
                    dtdeposito = EM.DataDeposito;

                //Add paging to header
                {
                    cb.BeginText();
                    cb.SetFontAndSize(bf, 10);
                    cb.SetTextMatrix(document.PageSize.GetRight(170), document.PageSize.GetTop(20));
                    cb.ShowText(text);
                    cb.EndText();
                    var len = bf.GetWidthPoint(text, 12);
                    cb.AddTemplate(headerTemplate, document.PageSize.GetRight(120) + len, document.PageSize.GetTop(20));
                }
                //Add paging to footer
                {
                    cb.BeginText();
                    cb.SetFontAndSize(bf, 12);
                    cb.SetTextMatrix(document.PageSize.GetRight(200), document.PageSize.GetBottom(20));

                    cb.EndText();
                    cb.AddTemplate(footerTemplate, document.PageSize.GetRight(200) + 50, document.PageSize.GetBottom(20));
                }
                //Add paging to footer
                {
                    cb.BeginText();
                    cb.SetFontAndSize(bf, 12);
                    cb.SetTextMatrix(document.PageSize.GetRight(500), document.PageSize.GetBottom(20));

                    if (EM.Firma_da_ufficio)
                        cb.ShowText($"{EM.N_EM} depositato d'ufficio");
                    else if (dtdeposito != "--")
                        cb.ShowText($"{EM.N_EM} depositato il {dtdeposito}");

                    cb.EndText();
                    var len = bf.GetWidthPoint(text, 12);
                    cb.AddTemplate(footerTemplate2, document.PageSize.GetRight(500) + 300, document.PageSize.GetBottom(20));
                }

                cb.MoveTo(40, document.PageSize.GetBottom(50));
                cb.LineTo(document.PageSize.Width - 40, document.PageSize.GetBottom(50));
                cb.Stroke();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            try
            {
                base.OnCloseDocument(writer, document);

                footerTemplate.BeginText();
                footerTemplate.SetFontAndSize(bf, 12);
                footerTemplate.SetTextMatrix(0, 15);
                footerTemplate.ShowText($"Totale pagine: {writer.PageNumber}");
                footerTemplate.EndText();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #region Properties

        public string Header { get; set; }

        public EmendamentiDto EM { get; set; }

        #endregion
    }
}