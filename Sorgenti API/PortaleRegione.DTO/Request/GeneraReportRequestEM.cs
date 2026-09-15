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

using System.Collections.Generic;

namespace PortaleRegione.DTO.Request;

/// <summary>
///     Payload inviato dal modale "Genera report" del modulo Emendamenti verso
///     l'endpoint MVC client unificato GeneraReport.
///     Incapsula i filtri/ordinamento correnti (FilterRequestEM) piu' le opzioni
///     di presentazione (copertina, template, formato di esportazione, colonne).
/// </summary>
public class GeneraReportRequestEM
{
    public FilterRequestEM Filter { get; set; } = new();

    public string ReportName { get; set; }
    public string CoverType { get; set; }
    public string DataViewType { get; set; }
    public string DataViewType_Template { get; set; }
    public List<string> Columns { get; set; } = new();

    /// <summary>
    ///     Formato richiesto: "XLS", "PDF" oppure "WORD".
    /// </summary>
    public string ExportFormat { get; set; }
}
