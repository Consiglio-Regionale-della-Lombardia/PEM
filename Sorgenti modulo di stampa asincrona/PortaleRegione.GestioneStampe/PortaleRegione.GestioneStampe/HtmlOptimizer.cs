using System.Text.RegularExpressions;

namespace PortaleRegione.GestioneStampe;

public static class HtmlOptimizer
{
    private static readonly Regex WhitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

    private static readonly Regex CommentRegex =
        new Regex(@"<!--.*?-->", RegexOptions.Compiled | RegexOptions.Singleline);
    
    public static string OptimizeForPdf(string html)
    {
        // Rimuovi commenti HTML
        html = CommentRegex.Replace(html, "");

        // Riduci whitespace multipli
        html = WhitespaceRegex.Replace(html, " ");

        // Rimuovi lazy loading dalle immagini
        html = html.Replace("loading=\"lazy\"", "");

        return html;
    }
}