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
        public PersonaDto()
        {
            Gruppo = new GruppiDto();
        }

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

        [Display(Name = "Cognome")] public string cognome { get; set; } = "";
        [Display(Name = "Nome")] public string nome { get; set; } = "";
        [Display(Name = "Email")] public string email { get; set; }
        [Display(Name = "Foto")] public string foto { get; set; }
        [Display(Name = "Login di rete")] public string userAD { get; set; }

        [Display(Name = "Non consigliere")]
        public int No_Cons { get; set; } = 0;

        [Display(Name = "Notifica alla firma")]
        public bool? notifica_firma { get; set; }

        [Display(Name = "Notifica al deposito")]
        public bool? notifica_deposito { get; set; }

        public string legislature { get; set; }
        public bool? legislatura_attuale { get; set; } = false;

        [Display(Name = "Eliminato")] public bool? deleted { get; set; }

        public string Carica { get; set; }

        public IEnumerable<RuoliDto> Ruoli { get; set; }

        [Display(Name = "Gruppo di riferimento")]

        public int id_gruppo_politico_rif { get; set; }


        public GruppiDto Gruppo { get; set; }

        public RuoliIntEnum CurrentRole { get; set; }

        //Parametro valorizzato solo da pannello amministratore
        [Display(Name = "Gruppi A.D. PEM")] public string Gruppi { get; set; }

        //Parametro valorizzato solo da pannello amministratore
        public StatoPinEnum Stato_Pin { get; set; }
        public bool IsSegreteriaAssemblea => IsAmministratorePEM || CurrentRole == RuoliIntEnum.Segreteria_Assemblea;
        public bool IsSegreteriaAssemblea_Read => CurrentRole == RuoliIntEnum.Segreteria_Assemblea_Read;
        public bool IsSoloSegreteriaAssemblea => CurrentRole == RuoliIntEnum.Segreteria_Assemblea;
        public bool IsConsigliereRegionale => CurrentRole == RuoliIntEnum.Consigliere_Regionale;
        public bool IsAssessore => CurrentRole == RuoliIntEnum.Assessore_Sottosegretario_Giunta;
        public bool IsPresidente => CurrentRole == RuoliIntEnum.Presidente_Regione;
        public bool IsSegreteriaPolitica => CurrentRole == RuoliIntEnum.Segreteria_Politica || CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica;

        public bool IsSegreteriaGiunta => CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta || CurrentRole == RuoliIntEnum.Segreteria_Giunta_Regionale;
        public bool IsResponsabileSegreteriaPolitica => CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica;
        public bool IsResponsabileSegreteriaGiunta => CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta;

        public bool IsGiunta => IsPresidente
                                || IsAmministratoreGiunta
                                || CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta
                                || CurrentRole == RuoliIntEnum.Segreteria_Giunta_Regionale
                                || IsAssessore;

        public bool IsAmministratoreGiunta => CurrentRole == RuoliIntEnum.Amministratore_Giunta;
        public bool IsAmministratorePEM => CurrentRole == RuoliIntEnum.Amministratore_PEM;

        public bool IsCapoGruppo { get; set; }
    }
    
    public class PersonaCookieDto
    {
        public PersonaCookieDto()
        {
            Gruppo = new GruppiDto();
        }
        
        public string DisplayName => $"{cognome.Replace("'", "’")} {nome.Replace("'", "’")}";

        public string DisplayName_GruppoCode =>
            $"{DisplayName} ({(Gruppo != null ? Gruppo.codice_gruppo : "N.D.")})";

        public string DisplayName_GruppoCode_EX =>
            $"{DisplayName} ({(Gruppo != null ? Gruppo.nome_gruppo : "N.D.")})";

        [Display(Name = "GUID")] public Guid UID_persona { get; set; }
        public int id_persona { get; set; }

        [Display(Name = "Cognome")] public string cognome { get; set; } = "";
        [Display(Name = "Nome")] public string nome { get; set; } = "";
        [Display(Name = "Email")] public string email { get; set; }
        [Display(Name = "Foto")] public string foto { get; set; }

        public string Carica { get; set; }

        public IEnumerable<RuoliDto> Ruoli { get; set; }

        [Display(Name = "Gruppo di riferimento")]

        public int id_gruppo_politico_rif { get; set; }

        public GruppiDto Gruppo { get; set; }

        public RuoliIntEnum CurrentRole { get; set; }
        public bool IsSegreteriaAssemblea => IsAmministratorePEM || CurrentRole == RuoliIntEnum.Segreteria_Assemblea;
        public bool IsSegreteriaAssemblea_Read => CurrentRole == RuoliIntEnum.Segreteria_Assemblea_Read;
        public bool IsSoloSegreteriaAssemblea => CurrentRole == RuoliIntEnum.Segreteria_Assemblea;
        public bool IsConsigliereRegionale => CurrentRole == RuoliIntEnum.Consigliere_Regionale;
        public bool IsAssessore => CurrentRole == RuoliIntEnum.Assessore_Sottosegretario_Giunta;
        public bool IsPresidente => CurrentRole == RuoliIntEnum.Presidente_Regione;
        public bool IsSegreteriaPolitica => CurrentRole == RuoliIntEnum.Segreteria_Politica || CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica;

        public bool IsSegreteriaGiunta => CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta || CurrentRole == RuoliIntEnum.Segreteria_Giunta_Regionale;
        public bool IsResponsabileSegreteriaPolitica => CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Politica;
        public bool IsResponsabileSegreteriaGiunta => CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta;

        public bool IsGiunta => IsPresidente
                                || IsAmministratoreGiunta
                                || CurrentRole == RuoliIntEnum.Responsabile_Segreteria_Giunta
                                || CurrentRole == RuoliIntEnum.Segreteria_Giunta_Regionale
                                || IsAssessore;

        public bool IsAmministratoreGiunta => CurrentRole == RuoliIntEnum.Amministratore_Giunta;
        public bool IsAmministratorePEM => CurrentRole == RuoliIntEnum.Amministratore_PEM;

        public bool IsCapoGruppo { get; set; }
        
        public static implicit operator PersonaCookieDto(PersonaDto dto)
        {
            if (dto == null) return null;

            return new PersonaCookieDto
            {
                UID_persona = dto.UID_persona,
                id_persona = dto.id_persona,
                cognome = dto.cognome,
                nome = dto.nome,
                email = dto.email,
                foto = dto.foto,
                Carica = dto.Carica,
                Ruoli = dto.Ruoli,
                id_gruppo_politico_rif = dto.id_gruppo_politico_rif,
                Gruppo = dto.Gruppo,
                CurrentRole = dto.CurrentRole,
                IsCapoGruppo = dto.IsCapoGruppo
            };
        }
    }
}