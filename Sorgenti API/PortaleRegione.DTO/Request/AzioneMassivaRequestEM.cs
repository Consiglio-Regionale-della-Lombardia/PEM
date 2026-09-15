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

using PortaleRegione.DTO.Model;

namespace PortaleRegione.DTO.Request;

/// <summary>
///     Wrapper inviato dal client al controller MVC per le azioni massive del modulo Emendamenti.
///     Contiene la richiesta filtro corrente (per risolvere gli identificatori quando Comando.Tutti
///     vale true) e il comando di azione vera e propria.
/// </summary>
public class AzioneMassivaRequestEM
{
    public FilterRequestEM Filter { get; set; } = new();
    public ComandiAzioneModel Comando { get; set; } = new();
}
