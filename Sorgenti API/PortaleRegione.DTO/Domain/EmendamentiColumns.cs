using System;
using System.ComponentModel;

namespace PortaleRegione.DTO.Domain
{
    public class EmendamentiColumns
    {
        [DisplayName("Numero EM")] public string N_EM { get; set; }
        [DisplayName("Data Deposito")] public string DataDeposito { get; set; }
        [DisplayName("Stato")] public int IDStato { get; set; }
        [DisplayName("Tipo")] public int IDTipo_EM { get; set; }
        [DisplayName("Parte")] public int IDParte { get; set; }
        [DisplayName("Articolo")] public Guid? UIDArticolo { get; set; }
        [DisplayName("Comma")] public Guid? UIDComma { get; set; }
        [DisplayName("Lettera")] public Guid? UIDLettera { get; set; }
        [DisplayName("Titolo")] public string NTitolo { get; set; }
        [DisplayName("Capo")] public string NCapo { get; set; }
        [DisplayName("Missione")] public int? NMissione { get; set; }
        [DisplayName("Programma")] public int? NProgramma { get; set; }
        [DisplayName("TitoloM")] public int? NTitoloB { get; set; }
        [DisplayName("Area politica")] public int? AreaPolitica { get; set; }
        [DisplayName("Firmatari")] public string Firme { get; set; }
        [DisplayName("Proponente")] public Guid UIDPersonaProponente { get; set; }
        [DisplayName("Gruppi")] public int id_gruppo { get; set; }
        [DisplayName("Effetti finanziari")] public int EffettiFinanziari { get; set; }
        [DisplayName("Tags")] public string Tags { get; set; }
    }
}