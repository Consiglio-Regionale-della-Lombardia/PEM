using System;
using System.ComponentModel;

namespace PortaleRegione.DTO.Domain
{
    public class AttiDASISortingConsiglieri
    {
        [DisplayName("Tipo atto")] public int Tipo { get; set; }
        [DisplayName("Numero atto")] public string NAtto_search { get; set; }
        [DisplayName("Data presentazione")] public DateTime Timestamp { get; set; }
    }
}