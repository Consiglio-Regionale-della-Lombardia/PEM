using System;
using System.ComponentModel.DataAnnotations;

namespace PortaleRegione.DTO.Model
{
    public class ResetPinModel
    {       
        [Required]
        [Display(Name = "Nuovo PIN")]
        public string nuovo_pin { get; set; }

        public Guid PersonaUId { get; set; }
    }
}