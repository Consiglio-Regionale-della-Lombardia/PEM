using System;

namespace PortaleRegione.DTO.Request
{
    public class ResetRequest
    {
        public string new_value { get; set; }
        public Guid persona_UId { get; set; }
    }
}