/*
 * CONFIGURAZIONE SICURA TRUMBOWYG - ACT32 (FIXED)
 * 
 * Mantiene formattazioni sicure (grassetto, corsivo, tabelle, liste)
 * Rimuove solo contenuti pericolosi (script, iframe, event handlers)
 */

// Configurazione globale sicura per Trumbowyg
var trumbowygSecureConfig = {
    lang: 'it',

    // NON rimuovere tutte le formattazioni - solo quelle pericolose
    removeformatPasted: false,  // ← CAMBIATO DA true A false

    // Usa tag semantici validi
    semantic: true,

    // Plugin per gestione paste da Word
    plugins: {
        fontfamily: {
            fontList: [
                { name: 'Arial', family: 'Arial, Helvetica, sans-serif' },
                { name: 'Open Sans', family: '\'Open Sans\', sans-serif' }
            ]
        }
    },

    btns: [
        ['viewHTML'],
        ['formatting'],
        ['strong', 'em', 'del'],
        ['justifyLeft', 'justifyCenter', 'justifyRight', 'justifyFull'],
        ['unorderedList', 'orderedList'],
        ['indent', 'outdent'],
        ['table'],
        ['link'],
        ['fontfamily'],
        ['fontsize'],
        ['removeformat'],
        ['fullscreen']
    ],

    // Tag HTML consentiti - WHITELIST
    tagsToKeep: [
        'p', 'br', 'strong', 'b', 'em', 'i', 'u', 'del', 's',
        'ul', 'ol', 'li',
        'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
        'table', 'thead', 'tbody', 'tr', 'th', 'td',
        'a', 'blockquote', 'pre', 'code',
        'div', 'span', 'hr', 'sup', 'sub'
    ],

    // Tag da rimuovere automaticamente (SOLO pericolosi)
    tagsToRemove: ['script', 'style', 'iframe', 'object', 'embed', 'applet', 'form', 'input', 'button'],

    // Eventi per validazione aggiuntiva
    events: {
        // Pulizia selettiva durante paste
        tbwpaste: function(e) {
            var $this = $(this);

            // Ritarda leggermente per permettere il paste
            setTimeout(function() {
                var content = $this.trumbowyg('html');

                // Pulisci SOLO contenuti pericolosi, mantieni formattazioni
                content = cleanWordPasteKeepFormatting(content);

                $this.trumbowyg('html', content);
            }, 10);
        },

        // Validazione dopo il cambio di contenuto
        tbwchange: function() {
            var editor = $(this);
            var content = editor.trumbowyg('html');

            // Rimuovi SOLO tag pericolosi
            content = sanitizeEditorContent(content);

            if (content !== editor.trumbowyg('html')) {
                editor.trumbowyg('html', content);
            }
        }
    }
};

// Funzione per pulire paste da Word mantenendo formattazioni utili
function cleanWordPasteKeepFormatting(html) {
    if (!html) return html;

    var cleaned = html;

    // Rimuovi commenti condizionali Word (<!--[if ...]>...<![endif]-->)
    cleaned = cleaned.replace(/<!--\[if[\s\S]*?<!\[endif\]-->/gi, '');

    // Rimuovi tag XML di Word (<w:*, <o:*, <m:*)
    cleaned = cleaned.replace(/<\/?w:[^>]*>/gi, '');
    cleaned = cleaned.replace(/<\/?o:[^>]*>/gi, '');
    cleaned = cleaned.replace(/<\/?m:[^>]*>/gi, '');

    // Rimuovi attributi Word (class="Mso*", style con mso-*)
    cleaned = cleaned.replace(/\s*class="Mso[^"]*"/gi, '');
    cleaned = cleaned.replace(/\s*class='Mso[^']*'/gi, '');
    cleaned = cleaned.replace(/\s*style="[^"]*mso-[^"]*"/gi, '');
    cleaned = cleaned.replace(/\s*style='[^']*mso-[^']*'/gi, '');

    // Rimuovi <meta>, <link>, <xml> tags
    cleaned = cleaned.replace(/<meta[^>]*>/gi, '');
    cleaned = cleaned.replace(/<link[^>]*>/gi, '');
    cleaned = cleaned.replace(/<xml[^>]*>[\s\S]*?<\/xml>/gi, '');

    // Rimuovi span vuoti o inutili
    cleaned = cleaned.replace(/<span[^>]*>\s*<\/span>/gi, '');

    // Rimuovi &nbsp; multipli (sostituisci con singolo spazio)
    cleaned = cleaned.replace(/(&nbsp;\s*){2,}/gi, ' ');

    // MANTIENI: strong, b, em, i, u, del, ul, ol, li, table, tr, td, th
    // Questi NON vengono toccati dalla pulizia

    // Rimuovi SOLO tag pericolosi
    cleaned = sanitizeEditorContent(cleaned);

    return cleaned;
}

// Funzione di sanitizzazione lato client (SOLO pericolosi)
function sanitizeEditorContent(html) {
    if (!html) return html;

    var cleaned = html;

    // Rimuovi tag pericolosi
    cleaned = cleaned.replace(/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, '');
    cleaned = cleaned.replace(/<iframe[^>]*>[\s\S]*?<\/iframe>/gi, '');
    cleaned = cleaned.replace(/<object[^>]*>[\s\S]*?<\/object>/gi, '');
    cleaned = cleaned.replace(/<embed[^>]*>/gi, '');
    cleaned = cleaned.replace(/<applet[^>]*>[\s\S]*?<\/applet>/gi, '');
    cleaned = cleaned.replace(/<form[^>]*>[\s\S]*?<\/form>/gi, '');

    // Rimuovi event handlers inline
    cleaned = cleaned.replace(/\s*on\w+\s*=\s*["'][^"']*["']/gi, '');
    cleaned = cleaned.replace(/\s*on\w+\s*=\s*[^\s>]*/gi, '');

    // Rimuovi javascript: e vbscript: da href/src
    cleaned = cleaned.replace(/href\s*=\s*["']?\s*javascript:/gi, 'href="#"');
    cleaned = cleaned.replace(/src\s*=\s*["']?\s*javascript:/gi, 'src="#"');
    cleaned = cleaned.replace(/href\s*=\s*["']?\s*vbscript:/gi, 'href="#"');

    // Rimuovi data:text/html
    cleaned = cleaned.replace(/href\s*=\s*["']?\s*data:text\/html/gi, 'href="#"');

    return cleaned;
}

// Funzione helper per inizializzare editor con configurazione sicura
function initSecureTrumbowyg(selector) {
    $(selector).trumbowyg(trumbowygSecureConfig);
}

// Validazione prima del submit del form
function validateEditorContent(editorSelector) {
    var editor = $(editorSelector).parent().find('.trumbowyg-editor');
    var content = editor.html();

    var dangerousChecks = [
        { pattern: /<script/i, message: 'Tag script non sono permessi' },
        { pattern: /javascript:/i, message: 'JavaScript inline non è permesso' },
        { pattern: /on\w+\s*=/i, message: 'Event handler inline non sono permessi' },
        { pattern: /<iframe/i, message: 'Tag iframe non sono permessi' },
        { pattern: /<object/i, message: 'Tag object non sono permessi' },
        { pattern: /<embed/i, message: 'Tag embed non sono permessi' }
    ];

    for (var i = 0; i < dangerousChecks.length; i++) {
        if (dangerousChecks[i].pattern.test(content)) {
            alert('ATTENZIONE: ' + dangerousChecks[i].message);
            return false;
        }
    }

    return true;
}