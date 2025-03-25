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
    public enum TemplateTypeEnum
    {
        PDF = 1,
        PDF_COPERTINA = 2,
        MAIL = 3,
        HTML = 4,
        HTML_MODIFICABILE = 5,
        FIRMA = 6,
        INDICE_DASI = 7,
        REPORT_HEADER_DEFAULT = 8,
        REPORT_ITEM_CARD = 9,
        REPORT_COVER = 10,
        REPORT_ITEM_GRID = 11,
        HTML_PDF = 12
    }
}