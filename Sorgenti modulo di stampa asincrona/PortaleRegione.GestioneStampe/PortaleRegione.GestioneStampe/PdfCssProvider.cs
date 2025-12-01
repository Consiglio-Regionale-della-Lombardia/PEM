using System;
using System.IO;
using System.Reflection;

namespace PortaleRegione.GestioneStampe;

public static class PdfCssProvider
{
    // Cache statica - caricato una sola volta
    private static readonly Lazy<string> CachedCss = new Lazy<string>(BuildInlineCss);
    private static readonly Assembly CurrentAssembly = typeof(PdfCssProvider).Assembly;
    private static readonly Lazy<string> CachedLogoBase64 = new Lazy<string>(LoadLogoAsBase64);
    
    // Namespace base per le risorse embedded
    private const string ResourcePrefix = "PortaleRegione.GestioneStampe.Styles.";
    
    public static string GetInlineCss()
    {
        return CachedCss.Value;
    }
    
    /// <summary>
    /// Ritorna il logo come data URI base64, pronto per essere usato in src=""
    /// </summary>
    public static string GetLogoBase64()
    {
        return CachedLogoBase64.Value;
    }
    
    private static string LoadLogoAsBase64()
    {
        try
        {
            var logoBytes = LoadEmbeddedBinaryResource("logo.png");
            if (logoBytes != null)
            {
                var base64 = Convert.ToBase64String(logoBytes);
                return $"data:image/png;base64,{base64}";
            }
        }
        catch
        {
            // Ignora errori
        }
        
        // Fallback: ritorna stringa vuota (nessun logo)
        return string.Empty;
    }
    
    private static string BuildInlineCss()
    {
        var fontCss = LoadFontCss();
        var materializeCss = LoadEmbeddedTextResource("materialize.css");
        var siteCss = LoadEmbeddedTextResource("style.css");
        
        return $"<style>{fontCss}{materializeCss}{siteCss}</style>";
    }
    
    private static string LoadFontCss()
    {
        try
        {
            // Carica il font woff2 come embedded resource e convertilo in base64
            var fontBytes = LoadEmbeddedBinaryResource("flUhRq6tzZclQEJ-Vdg-IuiaDsNc.woff2");
            
            if (fontBytes != null)
            {
                var fontBase64 = Convert.ToBase64String(fontBytes);
                
                return $@"
@font-face {{
    font-family: 'Material Icons';
    font-style: normal;
    font-weight: 400;
    src: url(data:font/woff2;base64,{fontBase64}) format('woff2');
}}
.material-icons {{
    font-family: 'Material Icons';
    font-weight: normal;
    font-style: normal;
    font-size: 24px;
    line-height: 1;
    display: inline-block;
    white-space: nowrap;
    direction: ltr;
    -webkit-font-smoothing: antialiased;
}}";
            }
        }
        catch
        {
            // Ignora errori
        }
        
        // Fallback: nascondi le icone Material
        return ".material-icons { display: none !important; }";
    }
    
    private static string LoadEmbeddedTextResource(string fileName)
    {
        try
        {
            var resourceName = ResourcePrefix + fileName;
            using var stream = CurrentAssembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }
        catch
        {
            // Ignora errori
        }
        return null;
    }
    
    private static byte[] LoadEmbeddedBinaryResource(string fileName)
    {
        try
        {
            var resourceName = ResourcePrefix + fileName;
            using var stream = CurrentAssembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
        catch
        {
            // Ignora errori
        }
        return null;
    }
}