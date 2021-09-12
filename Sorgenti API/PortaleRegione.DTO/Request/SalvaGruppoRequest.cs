using System;

namespace PortaleRegione.DTO.Request
{
    public class SalvaGruppoRequest
    {
        public Guid UID_Gruppo { get; set; }
        public string GruppoAD { get; set; }
        public bool abilita_em_privati { get; set; }
        public bool giunta { get; set; }
        public int Id_Gruppo { get; set; }
    }
}