using System.ComponentModel.DataAnnotations;
using System;

namespace PortaleRegione.Domain
{
    public class View_consiglieri
    {
        [Key] public Guid UID_persona { get; set; }
        public int id_persona { get; set; }
        public int id_legislatura { get; set; }
        public string DisplayName { get; set; }
    }
}