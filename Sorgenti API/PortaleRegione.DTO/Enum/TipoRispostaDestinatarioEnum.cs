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

namespace PortaleRegione.DTO.Enum
{
    public enum TipoRispostaDestinatarioEnum
    {
        IL_PRESIDENTE_DELLA_REGIONE = 1,
        IL_PRESIDENTE_DELLA_REGIONE_E_ASSESSORE_AUTONOMIA_E_CULTURA = 2,
        IL_PRESIDENTE_DELLA_GIUNTA_REGIONALE_E_ASSESSORE_COMPETENTE = 3,
        IL_PRESIDENTE_DELLA_GIUNTA_REGIONALE_E_GLI_ASSESSORI_COMPETENTI = 4,
        IL_PRESIDENTE_E_LA_GIUNTA_REGIONALE = 5,
        IL_PRESIDENTE_LA_GIUNTA_REGIONALE_E_ASSESSORE_COMPETENTE =6,
        IL_PRESIDENTE_LA_GIUNTA_REGIONALE_E_GLI_ASSESSORI_COMPETENTI=7,
        LA_GIUNTA_REGIONALE = 8,
        LA_GIUNTA_REGIONALE_E_ASSESSORE_COMPETENTE = 9,
        ASSESSORE_COMPETENTE = 10
    }
}