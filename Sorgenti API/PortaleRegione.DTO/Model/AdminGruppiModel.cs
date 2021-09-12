using PortaleRegione.DTO.Domain;

namespace PortaleRegione.DTO.Model
{
    public class AdminGruppiModel
    {
        public GruppiDto Gruppo { get; set; }
        public bool Error_AD { get; set; } = false;
        public string Error_AD_Message { get; set; } = string.Empty;
    }
}