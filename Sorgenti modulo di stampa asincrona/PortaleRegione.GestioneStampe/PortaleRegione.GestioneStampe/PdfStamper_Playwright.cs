using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PortaleRegione.GestioneStampe;

/// <summary>
///     Sostituto open-source di IronPDF per: HTML→PDF (con footer paginato) + merge.
///     Rendering: Playwright/Chromium; Merge: PdfSharp.
/// </summary>
public class PdfStamper_Playwright
{
    private static readonly object s_installLock = new();
    
    // --- Browser condiviso per tutto il processo (thread-safe) ------------
    private static readonly Lazy<Task<(IPlaywright pw, IBrowser browser)>> s_runtime =
        new(() => InitAsync(), LazyThreadSafetyMode.ExecutionAndPublication);
    
    // Pool di pagine riutilizzabili
    private static readonly Lazy<Task<PagePool>> s_pagePool = 
        new(() => InitPagePoolAsync(), LazyThreadSafetyMode.ExecutionAndPublication);

    // ----------------------------------------------------------------------

    private static async Task<(IPlaywright, IBrowser)> InitAsync()
    {
        // installa i browser se mancanti (chiama il CLI interno di Playwright)
        lock (s_installLock)
        {
            try
            {
                Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
            }
            catch { /* se già installato, ignora */ }
        }

        var pw = await Playwright.CreateAsync();
        var browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        });
        return (pw, browser);
    }
    
    private static async Task<PagePool> InitPagePoolAsync()
    {
        var (_, browser) = await s_runtime.Value;
        return new PagePool(browser, poolSize: 5);
    }

    // ========== HTML → PDF =================================================

    public async Task<byte[]> CreaPDFInMemory(string body, string nome_documento, List<string> attachments = null)
    {
        try
        {
            var pdf = await RenderHtmlToPdfAsync(body, BuildFooter(nome_documento));
            if (attachments != null && attachments.Any())
            {
                var attachBytes = attachments
                    .Where(File.Exists)
                    .Where(p => Path.GetExtension(p).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    .Select(File.ReadAllBytes);
                pdf = MergeBytes(new[] { pdf }.Concat(attachBytes));
            }

            return pdf;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<byte[]> CreaPDFInMemory(string body)
    {
        return await CreaPDFInMemory(body, string.Empty);
    }

    public async Task CreaPDFAsync(string path, string body, string nome_documento, List<string> attachments = null)
    {
        var bytes = await CreaPDFInMemory(body, nome_documento, attachments);
        EnsureDirectory(path);

        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
        {
            await fs.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }
    }

    public void CreaPDF(string path, string body, string nome_documento, List<string> attachments = null)
    {
        CreaPDFAsync(path, body, nome_documento, attachments).GetAwaiter().GetResult();
    }

    public void CreaPDF(string txtHTML, string path)
    {
        var bytes = CreaPDFInMemory(txtHTML, string.Empty).GetAwaiter().GetResult();
        EnsureDirectory(path);
        File.WriteAllBytes(path, bytes);
    }

    /// <summary>
    ///     Sostituisce CreaPDFObject: ritorna byte[] invece dell'oggetto IronPdf.
    /// </summary>
    public async Task<byte[]> CreaPDFObject(string txtHTML, bool abilitaPaginazione = true,
        List<string> attachments = null)
    {
        var footer = abilitaPaginazione ? BuildFooter(null) : null;
        var pdf = await RenderHtmlToPdfAsync(txtHTML, footer);
        if (attachments != null && attachments.Any())
        {
            var attachBytes = attachments
                .Where(File.Exists)
                .Where(p => Path.GetExtension(p).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                .Select(File.ReadAllBytes);
            pdf = MergeBytes(new[] { pdf }.Concat(attachBytes));
        }

        return pdf;
    }

    // ========== MERGE ======================================================

    public void MergedPDF(string path, List<string> docs)
    {
        // Batch per non saturare memoria
        const int batchSize = 100;
        EnsureDirectory(path);

        using var outDoc = new PdfDocument();

        // Se esiste già un fascicolo, importalo prima (come facevi tu)
        if (File.Exists(path))
        {
            using var existing = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            AppendAllPages(existing, outDoc);
        }

        for (var i = 0; i < docs.Count; i += batchSize)
            foreach (var p in docs.Skip(i).Take(batchSize))
            {
                if (!File.Exists(p) || !p.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) continue;
                using var input = PdfReader.Open(p, PdfDocumentOpenMode.Import);
                AppendAllPages(input, outDoc);
            }

        outDoc.Save(path);
    }

    public void MergedPDF(string path, List<byte[]> docs)
    {
        const int batchSize = 100;
        EnsureDirectory(path);

        using var outDoc = new PdfDocument();

        if (File.Exists(path))
        {
            using var existing = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            AppendAllPages(existing, outDoc);
        }

        for (var i = 0; i < docs.Count; i += batchSize)
            foreach (var bytes in docs.Skip(i).Take(batchSize))
            {
                using var ms = new MemoryStream(bytes);
                using var input = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
                AppendAllPages(input, outDoc);
            }

        outDoc.Save(path);
    }

    public void MergedPDF(string pathFascicolo, byte[] fileToAppend)
    {
        EnsureDirectory(pathFascicolo);

        using var outDoc = new PdfDocument();

        if (File.Exists(pathFascicolo))
        {
            using var existing = PdfReader.Open(pathFascicolo, PdfDocumentOpenMode.Import);
            AppendAllPages(existing, outDoc);
        }

        using (var ms = new MemoryStream(fileToAppend))
        using (var input = PdfReader.Open(ms, PdfDocumentOpenMode.Import))
        {
            AppendAllPages(input, outDoc);
        }

        outDoc.Save(pathFascicolo);
    }

    public byte[] MergedPDFInMemory(List<byte[]> docs)
    {
        return MergeBytes(docs);
    }

    public byte[] MergedPDFInMemoryFromFiles(List<string> paths)
    {
        var pdfs = paths
            .Where(File.Exists)
            .Where(p => p.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            .Select(File.ReadAllBytes);
        return MergeBytes(pdfs);
    }

    // ========== INTERNALS ==================================================

    private async Task<byte[]> RenderHtmlToPdfAsync(
        string html,
        string footerRightText = null
    )
    {
        var pool = await s_pagePool.Value;
        var page = await pool.AcquirePageAsync();

        try
        {
            await page.SetContentAsync(html, new PageSetContentOptions 
            { 
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 60000  // Timeout di sicurezza
            });

            var displayHeaderFooter = !string.IsNullOrEmpty(footerRightText);

            var bytes = await page.PdfAsync(new PagePdfOptions
            {
                Format = "A4",
                PrintBackground = true,
                Margin = new Margin { Top = "10mm", Right = "10mm", Bottom = "10mm", Left = "10mm" },
                DisplayHeaderFooter = displayHeaderFooter,
                HeaderTemplate = "<div></div>",
                FooterTemplate = displayHeaderFooter ? footerRightText : "<div></div>",
                PreferCSSPageSize = false,  // Forza A4
                Scale = 1.0f
            });

            return bytes;
        }
        finally
        {
            await pool.ReleasePageAsync(page);
        }
    }

    private string BuildFooter(string nome_documento)
    {
        // Token Playwright: pageNumber / totalPages
        var label = string.IsNullOrEmpty(nome_documento) ? "" : $"{nome_documento} ";
        return $@"<div style='font-size:10px;width:100%;text-align:right;padding-right:8mm;'>
                        {label}Pagina <span class=""pageNumber""></span> di <span class=""totalPages""></span>
                      </div>";
    }

    private void EnsureDirectory(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    private void AppendAllPages(PdfDocument input, PdfDocument output)
    {
        for (var i = 0; i < input.PageCount; i++)
            output.AddPage(input.Pages[i]);
    }

    private byte[] MergeBytes(IEnumerable<byte[]> pdfs)
    {
        using var outDoc = new PdfDocument();
        foreach (var b in pdfs)
        {
            using var ms = new MemoryStream(b);
            using var input = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
            AppendAllPages(input, outDoc);
        }

        using var outMs = new MemoryStream();
        outDoc.Save(outMs);
        return outMs.ToArray();
    }
}