using System.Collections.Generic;
using PortaleRegione.DTO.Domain;

namespace GeneraStampeJob
{
    public class BodyModel
    {
        public string Path { get; set; }
        public string Body { get; set; }
        public EmendamentiDto EM { get; set; }
        public AttoDASIDto Atto { get; set; }
        public object Content { get; set; }
        public List<string> Attachments { get; set; }
    }
}