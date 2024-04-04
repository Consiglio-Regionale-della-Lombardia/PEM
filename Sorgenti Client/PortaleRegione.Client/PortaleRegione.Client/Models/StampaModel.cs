using System;
using System.Collections.Generic;

namespace PortaleRegione.Client.Models
{
    public class StampaModel
    {
        public int da { get; set; }
        public int a { get; set; }
        public string client_mode { get; set; }
        public List<string> filters { get; set; }
        public string uid_atto { get; set; }
        public string ordine { get; set; }

        public ICollection<Guid> Lista { get; set; }
        public bool Tutti { get; set; } = false;
    }
}