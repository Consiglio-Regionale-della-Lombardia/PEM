using System;
using System.ComponentModel.DataAnnotations;

namespace PortaleRegione.DTO.Domain
{
    public class GruppoAD_Dto
    {
        public Guid UID_Gruppo { get; set; }

        public int id_gruppo { get; set; }

        [StringLength(50)]
        public string GruppoAD { get; set; }

        public bool GiuntaRegionale { get; set; }

        public bool AbilitaEMPrivati { get; set; }

        public int? id_AreaPolitica { get; set; }

        public int id_legislatura { get; set; }

        public virtual GruppiDto gruppi_politici { get; set; }
    }
}