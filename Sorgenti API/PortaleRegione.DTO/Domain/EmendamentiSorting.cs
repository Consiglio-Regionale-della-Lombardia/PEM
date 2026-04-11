using System;
using System.ComponentModel;

namespace PortaleRegione.DTO.Domain
{
    public class EmendamentiSorting
    {
        [DisplayName("Stato")] public int IDStato { get; set; }
        [DisplayName("Ordine Presentazione")] public int OrdinePresentazione { get; set; }
        [DisplayName("Ordine Votazione")] public int OrdineVotazione { get; set; }
        [DisplayName("Data presentazione")] public DateTime? Timestamp { get; set; }
        [DisplayName("Progressivo")] public int Progressivo { get; set; }
        [DisplayName("Sub-Progressivo")] public int? SubProgressivo { get; set; }
    }
}