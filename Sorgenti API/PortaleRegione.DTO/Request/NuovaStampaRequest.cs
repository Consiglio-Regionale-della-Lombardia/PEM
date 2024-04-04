using System;
using System.Collections.Generic;
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.DTO.Request
{
    public class NuovaStampaRequest
    {
        public List<Guid> Lista { get; set; }
        public int Da { get; set; }
        public int A { get; set; }
        public ModuloStampaEnum Modulo { get; set; }
        public OrdinamentoEnum Ordinamento { get; set; } = OrdinamentoEnum.Default;
        public Guid UIDAtto { get; set; }
    }
}