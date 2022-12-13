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

namespace PortaleRegione.DTO.Routes
{
    public static partial class ApiRoutes
    {
        public static partial class PEM
        {
            public static partial class Atti
            {
                // api/pem/atti
                private const string Base = ApiRoutes.PEM.Base + "/atti";

                public const string GetAll = Base + "/all";
                public const string Get = Base + "/{id}";
                public const string Delete = Base + "/{id}";
                public const string Create = Base + "/create";
                public const string Edit = Base + "/edit";
                public const string ModificaFascicoli = Base + "/modifica-fascicoli";
                public const string DownloadDoc = Base + "/scarica-documento/{path}";
                public const string GrigliaTesti = Base + "/{id}/griglia-testi/{view}";
                public const string AggiornaRelatori = Base + "/aggiorna-relatori";
                public const string AggiornaTesto = Base + "/aggiorna-testo";
                public const string AbilitaFascicolo = Base + "/abilita-fascicolo";
                public const string BloccoODG = Base + "/odg/blocca";
                public const string JollyODG = Base + "/odg/jolly";
                public const string SpostaUp = Base + "/{id}/sposta/up";
                public const string SpostaDown = Base + "/{id}/sposta/down";
                public const string GetTipi = Base + "/tipi/{dasi}";
            }
        }
    }
}