using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Model;
using System.Collections.Generic;

namespace PortaleRegione.Client.Models
{
    public class ViewUtenteModel
    {
        public ViewUtenteModel()
        {
            GruppiAD = new List<AD_ObjectModel>();
        }
        public PersonaDto Persona { get; set; }
        public ICollection<AD_ObjectModel> GruppiAD { get; set; }
        public ICollection<KeyValueDto> GruppiInDB { get; set; }

        public PersonaDto CurrentUser { get; set; }
    }
}