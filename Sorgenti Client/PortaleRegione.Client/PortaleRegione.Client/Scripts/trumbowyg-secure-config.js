/*
 * CONFIGURAZIONE SICURA TRUMBOWYG - ACT32
 * 
 * Questo file contiene la configurazione dell'editor Trumbowyg
 * con whitelist di tag HTML permessi e protezione XSS
 * 
 * DA INCLUDERE IN: 
 * - Views/DASI/_ScriptFormDASI.cshtml
 * - Views/Emendamenti/_EMFormScript.cshtml  
 * - Views/Template/TemplateForm.cshtml
 * - Qualsiasi altra view che utilizza Trumbowyg
 */

// Configurazione globale sicura per Trumbowyg
var trumbowygSecureConfig = {
    lang: 'it',

    // Rimuovi formattazioni pericolose durante il paste
    removeformatPasted: true,

    // Usa tag semantici validi
    semantic: true,

    // Plugin cleanpaste per rimuovere formattazioni non sicure
    plugins: {
        fontfamily: {
            fontList: [
                { name: 'Arial', family: 'Arial, Helvetica, sans-serif' },
                { name: 'Open Sans', family: '\'Open Sans\', sans-serif' }
            ]
        }
    },

    // Pulsanti consentiti nella toolbar
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

    // Tag da rimuovere automaticamente
    tagsToRemove: ['script', 'style', 'iframe', 'object', 'embed', 'applet', 'form', 'input', 'button'],

    // Eventi per validazione aggiuntiva
    events: {
        // Validazione prima di inserire contenuto
        tbwpaste: function() {
            // Il plugin cleanpaste gestirà la pulizia
            return true;
        },

        // Validazione dopo il cambio di contenuto
        tbwchange: function() {
            var editor = $(this);
            var content = editor.trumbowyg('html');

            // Rimuovi tag pericolosi se presenti
            content = sanitizeEditorContent(content);

            // Aggiorna il contenuto se è stato modificato
            if (content !== editor.trumbowyg('html')) {
                editor.trumbowyg('html', content);
            }
        }
    }
};

// Funzione di sanitizzazione lato client
function sanitizeEditorContent(html) {
    if (!html) return html;

    // Pattern pericolosi da rimuovere
    var dangerousPatterns = [
        /<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi,
        /javascript:/gi,
        /on\w+\s*=/gi,
        /<iframe/gi,
        /<object/gi,
        /<embed/gi,
        /vbscript:/gi,
        /data:text\/html/gi
    ];

    var cleaned = html;
    dangerousPatterns.forEach(function(pattern) {
        cleaned = cleaned.replace(pattern, '');
    });

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

    // Verifica presenza di contenuto pericoloso
    var dangerousFound = false;
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
            dangerousFound = true;
            break;
        }
    }

    return !dangerousFound;
}