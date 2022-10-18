namespace PortaleRegione.DTO.Model
{
    public class AllegatoMail
    {
        public AllegatoMail()
        {

        }

        public AllegatoMail(byte[] _content, string _nomeFile)
        {
            this.content = _content;
            this.nomeFile = _nomeFile;
        }

        public byte[] content { get; set; }
        public string nomeFile { get; set; }
    }
}