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
            // Issue #1685: con le ottimizzazioni spente ASP.NET emette un tag per file e senza
            // versione (<script src="/Scripts/site.js">). IIS serve gli statici senza
            // Cache-Control, quindi il browser applica la freschezza euristica e dopo un
            // rilascio puo' continuare a usare per giorni il JS che ha in cache: e' cosi' che
            // su Chrome sparivano i filtri preferiti mentre su Edge funzionavano. Con le
            // ottimizzazioni attive l'URL diventa /bundles/logic?v=<hash del contenuto> e
            // cambia da solo a ogni modifica. Va forzato qui perche' in produzione
            // <compilation debug> resta "true".
            BundleTable.EnableOptimizations = true;

            bundles.Add(SenzaMinificazione(new ScriptBundle("~/bundles/lib")).Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery.validate*",
                "~/Scripts/jquery-hex-picker.js",
                "~/Scripts/modernizr-*",
                "~/Scripts/materialize.js",
                "~/Scripts/sweetalert.min.js"));
            bundles.Add(SenzaMinificazione(new ScriptBundle("~/bundles/logic"))
                .Include("~/Scripts/loader.js", "~/Scripts/site.js",
                    "~/Scripts/SessionManager.js",
                    "~/Scripts/FiltriManager.js"));

            bundles.Add(SenzaMinificazione(new ScriptBundle("~/bundles/filevalidation")).Include(
                "~/Scripts/file-upload-validation.js"
            ));

            bundles.Add(SenzaMinificazione(new ScriptBundle("~/bundles/editor")).Include(
                "~/Content/editor/trumbowyg.js",
                "~/Content/editor/plugins/cleanpaste/trumbowyg.cleanpaste.js",
                "~/Scripts/trumbowyg-secure-config.js"
            ));

            // Utilizzare la versione di sviluppo di Modernizr per eseguire attività di sviluppo e formazione. Successivamente, quando si è
            // pronti per passare alla produzione, usare lo strumento di compilazione disponibile all'indirizzo https://modernizr.com per selezionare solo i test necessari.

            // Gli URL relativi dentro site.css ("../images/", "../fonts/") continuano a
            // risolvere perche' il bundle vive in ~/Content/, la stessa cartella del foglio;
            // gli altri due CSS non ne hanno. Un file CSS incluso qui da una sottocartella
            // e che usi url() va aggiunto con un CssRewriteUrlTransform.
            bundles.Add(SenzaMinificazione(new StyleBundle("~/Content/css")).Include(
                "~/Content/site.css",
                "~/Content/jquery-hex-picker.css",
                "~/Content/editor/ui/trumbowyg.min.css"));
        }

        // I bundle concatenano ma non minificano: il minificatore di WebGrease e' fermo al
        // 2013 e sui costrutti moderni dei nostri script (async/await, arrow function,
        // template literal) e' capace di produrre codice rotto. Quello che ci serve e' la
        // versione nell'URL, non i byte risparmiati.
        private static T SenzaMinificazione<T>(T bundle) where T : Bundle
        {
            bundle.Transforms.Clear();
            return bundle;
        }
    }
}