using PortaleRegione.DTO.Model;
using System.Collections.Generic;

namespace PortaleRegione.DTO.Request
{
    public class FilterRequest
    {
        public List<FilterItem> filters { get; set; } = new List<FilterItem>();
        public int page { get; set; }
        public int size { get; set; }
    }
}