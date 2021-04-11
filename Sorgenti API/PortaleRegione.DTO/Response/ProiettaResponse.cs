using PortaleRegione.DTO.Domain;

namespace PortaleRegione.DTO.Response
{
    public class ProiettaResponse
    {
        public EmendamentiDto EM { get; set; }
        public int next { get; set; }
        public int prev { get; set; }
    }
}