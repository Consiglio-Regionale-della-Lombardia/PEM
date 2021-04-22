using PortaleRegione.DTO.Domain;

namespace GeneraStampeJob
{
    public class BodyModel
    {
        public string Path { get; set; }
        public string Body { get; set; }
        public EmendamentiDto EM { get; set; }
    }
}