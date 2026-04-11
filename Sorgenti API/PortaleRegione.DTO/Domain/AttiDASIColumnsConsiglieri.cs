using System;
using System.ComponentModel;

namespace PortaleRegione.DTO.Domain
{
    public class AttiDASIColumnsConsiglieri
    {
        [DisplayName("Tipo atto")] public int Tipo { get; set; }
        [DisplayName("Etichetta")] public string Etichetta { get; set; }
        [DisplayName("Oggetto")] public string Oggetto { get; set; }
        [DisplayName("Proponente")] public Guid? UIDPersonaProponente { get; set; }
        [DisplayName("Firmatari")] public string Firme { get; set; }
        [DisplayName("Data presentazione")] public DateTime Timestamp { get; set; }
        [DisplayName("Gruppo politico")] public int id_gruppo { get; set; }
        [DisplayName("Area politica")] public int AreaPolitica { get; set; }
        [DisplayName("Stato dell'atto")] public int IDStato { get; set; }
    }
}