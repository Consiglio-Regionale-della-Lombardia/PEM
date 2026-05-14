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

using System.Web.Optimization;

namespace PortaleRegione.Client
{
    public class BundleConfig
    {
        // Per altre informazioni sulla creazione di bundle, vedere  https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/lib").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery.validate*",
                "~/Scripts/jquery-hex-picker.js",
                "~/Scripts/modernizr-*",
                "~/Scripts/materialize.js",
                "~/Scripts/sweetalert.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/logic")
                .Include("~/Scripts/loader.js", "~/Scripts/site.js",
                    "~/Scripts/SessionManager.js",
                    "~/Scripts/FiltriManager.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/filevalidation").Include(
                "~/Scripts/file-upload-validation.js"
            ));
            
            bundles.Add(new ScriptBundle("~/bundles/editor").Include(
                "~/Content/editor/trumbowyg.js",
                "~/Content/editor/plugins/cleanpaste/trumbowyg.cleanpaste.js",
                "~/Scripts/trumbowyg-secure-config.js"
            ));

            // Utilizzare la versione di sviluppo di Modernizr per eseguire attività di sviluppo e formazione. Successivamente, quando si è
            // pronti per passare alla produzione, usare lo strumento di compilazione disponibile all'indirizzo https://modernizr.com per selezionare solo i test necessari.

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/site.css",
                "~/Content/jquery-hex-picker.css",
                "~/Content/editor/ui/trumbowyg.min.css"));
        }
    }
}