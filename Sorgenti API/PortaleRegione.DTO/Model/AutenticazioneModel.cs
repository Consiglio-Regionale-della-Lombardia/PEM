using PortaleRegione.DTO.Autenticazione;

namespace PortaleRegione.DTO.Model
{
    public class AutenticazioneModel
    {
        public AutenticazioneModel()
        {
        }

        public LoginRequest LoginRequest { get; set; } = new LoginRequest();

        public string versione { get; set; }

        public AutenticazioneModel(string _versione)
        {
            versione = _versione;
        }
    }
}