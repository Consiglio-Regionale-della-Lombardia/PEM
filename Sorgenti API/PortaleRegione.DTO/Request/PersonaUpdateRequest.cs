using PortaleRegione.DTO.Model;
using System;
using System.Collections.Generic;

namespace PortaleRegione.DTO.Request
{
    public class PersonaUpdateRequest
    {
        public Guid UID_persona { get; set; }
        public int id_persona { get; set; }
        public string nome { get; set; }
        public string cognome { get; set; }
        public string email { get; set; }
        public string foto { get; set; }
        public string userAD { get; set; }
        public int no_Cons { get; set; } = 0;
        public bool notifica_firma { get; set; } = false;
        public bool notifica_deposito { get; set; } = false;
        public bool deleted { get; set; } = false;
        public bool attivo { get; set; } = false;

        public List<AD_ObjectModel> gruppiAd { get; set; }
    }
}