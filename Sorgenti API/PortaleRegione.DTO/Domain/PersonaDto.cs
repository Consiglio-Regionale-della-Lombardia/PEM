/*
 * Copyright (C) 2019 Consiglio Regionale della Lombardia
 * SPDX-License-Identifier: AGPL-3.0-or-later
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using PortaleRegione.DTO.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PortaleRegione.DTO.Domain
{
    public class PersonaDto
    {
        //Parametro valorizzato solo da pannello amministrativo
        public string GruppiAD;

        //Preso in considerazione solo quando richiesto dal pannello amministrativo
        [Display(Name = "Attivo")] public bool? attivo { get; set; }

        public string DisplayName => $"{cognome.Replace("'", "’")} {nome.Replace("'", "’")}";

        public string DisplayName_GruppoCode =>
            $"{DisplayName} ({(Gruppo != null ? Gruppo.codice_gruppo : "N.D.")})";

        public string DisplayName_GruppoCode_EX =>
            $"{DisplayName} ({(Gruppo != null ? Gruppo.nome_gruppo : "N.D.")})";

        [Display(Name = "GUID")] public Guid UID_persona { get; set; }
        public int id_persona { get; set; }

        [Display(Name = "Cognome")] public string cognome { get; set; }
        [Display(Name = "Nome")] public string nome { get; set; }
        [Display(Name = "Email")] public string email { get; set; }
        [Display(Name = "Foto")] public string foto { get; set; }
        [Display(Name = "Login di rete")] public string userAD { get; set; }

        [Display(Name = "Consigliere/Assessore")]
        public int No_Cons { get; set; }

        [Display(Name = "Notifica alla firma")]
        public bool? notifica_firma { get; set; }

        [Display(Name = "Notifica al deposito")]
        public bool? notifica_deposito { get; set; }

        [Display(Name = "Eliminato")] public bool? deleted { get; set; }

        public string Carica { get; set; }

        public IEnumerable<RuoliDto> Ruoli { get; set; }

        [Display(Name = "Gruppo di riferimento")]
        public GruppiDto Gruppo { get; set; }

        public RuoliIntEnum CurrentRole { get; set; }

        //Parametro valorizzato solo da pannello amministratore
        [Display(Name = "Gruppi A.D. PEM")] public string Gruppi { get; set; }

        //Parametro valorizzato solo da pannello amministratore
        public StatoPinEnum Stato_Pin { get; set; }

        public bool IsGiunta()
        {
            switch (CurrentRole)
            {
                case RuoliIntEnum.Presidente_Regione:
                case RuoliIntEnum.Assessore_Sottosegretario_Giunta:
                case RuoliIntEnum.Amministratore_Giunta:
                case RuoliIntEnum.Responsabile_Segreteria_Giunta:
                case RuoliIntEnum.Segreteria_Giunta_Regionale:
                    return true;
                default:
                    return false;
            }
        }
    }
}