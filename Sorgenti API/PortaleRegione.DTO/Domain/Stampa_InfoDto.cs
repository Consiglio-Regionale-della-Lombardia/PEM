using System;

namespace PortaleRegione.DTO.Domain
{
    public class Stampa_InfoDto
    {
        public Guid Id { get; set; }
        public Guid UIDStampa { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
    }
}