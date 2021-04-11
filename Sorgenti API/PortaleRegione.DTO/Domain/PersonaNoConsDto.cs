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

using System;

namespace PortaleRegione.DTO.Domain
{
    public class PersonaNoConsDto
    {
        public string DisplayName => $"{cognome.Replace("'", "’")} {nome.Replace("'", "’")}";

        public Guid UID_persona { get; set; }

        public int id_persona { get; set; }

        public string cognome { get; set; }

        public string nome { get; set; }

        public string email { get; set; }

        public string foto { get; set; }

        public string UserAD { get; set; }

        public int? id_gruppo_politico_rif { get; set; }

        public bool notifica_firma { get; set; }

        public bool notifica_deposito { get; set; }

        public bool RichiediModificaPWD { get; set; }

        public DateTime? Data_ultima_modifica_PWD { get; set; }

        public string pass_locale_crypt { get; set; }

        public bool attivo { get; set; } = true;

        public bool deleted { get; set; } = false;
    }
}