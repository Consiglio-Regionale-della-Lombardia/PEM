namespace PortaleRegione.DTO.Model
{
    public class FilterItem
    {
        public string property { get; set; }
        public string value { get; set; }
        public bool not_empty { get; set; } = false;
    }
}