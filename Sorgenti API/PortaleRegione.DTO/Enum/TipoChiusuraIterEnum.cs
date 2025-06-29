﻿/*
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
    public enum TipoChiusuraIterEnum
    {
        RESPINTO = 13,
        APPROVATO = 14,
        RITIRATO = 15,
        DECADUTO = 16,
        DECADENZA_PER_FINE_LEGISLATURA = 22,
        INAMMISSIBILE= 23,
        CHIUSURA_PER_MOTIVI_DIVERSI = 24,
        DECADENZA_PER_FINE_MANDATO_CONSIGLIERE = 25,
        COMUNICAZIONE_ASSEMBLEA = 26,
        TRATTAZIONE_ASSEMBLEA = 27,
    }
}