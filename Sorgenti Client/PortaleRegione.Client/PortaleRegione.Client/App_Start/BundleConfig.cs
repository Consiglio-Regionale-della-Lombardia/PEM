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