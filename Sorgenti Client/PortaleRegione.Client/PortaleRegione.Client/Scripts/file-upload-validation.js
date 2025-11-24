// Configurazione validazione file
var fileValidationConfig = {
    // Dimensione massima: 50 MB
    maxFileSize: 50 * 1024 * 1024,

    // Estensioni consentite
    allowedExtensions: ['.pdf'],

    // Estensioni BLOCCATE (blacklist)
    blacklistedExtensions: [
        '.exe', '.dll', '.bat', '.cmd', '.com', '.pif', '.scr', '.vbs', '.js', '.jse',
        '.ws', '.wsf', '.wsc', '.wsh', '.ps1', '.ps1xml', '.ps2', '.ps2xml', '.psc1',
        '.psc2', '.msh', '.msh1', '.msh2', '.mshxml', '.msh1xml', '.msh2xml',
        '.scf', '.lnk', '.inf', '.reg', '.app', '.application', '.gadget', '.msi',
        '.msp', '.mst', '.cpl', '.msc', '.jar',
        // ARCHIVI COMPRESSI (incluso ZIP)
        '.zip', '.rar', '.7z', '.tar', '.gz', '.bz2', '.xz', '.iso', '.img',
        '.dmg', '.pkg', '.deb', '.rpm', '.cab', '.ace', '.arj', '.lzh', '.sit'
    ],

    // MIME types consentiti
    allowedMimeTypes: ['application/pdf'],

    // Messaggi di errore
    messages: {
        noFile: 'Nessun file selezionato.',
        fileTooBig: 'Il file supera la dimensione massima consentita di 50 MB.',
        invalidExtension: 'Il tipo di file non è consentito. Solo file PDF sono accettati.',
        blacklistedExtension: 'Il tipo di file è nella blacklist per motivi di sicurezza.',
        zipFile: 'I file ZIP e altri archivi compressi non sono consentiti per motivi di sicurezza.',
        invalidMimeType: 'Il tipo MIME del file non è valido. Solo file PDF sono accettati.',
        multipleExtensions: 'Il nome del file contiene estensioni multiple. Possibile tentativo di mascheramento.'
    }
};

/**
 * Valida un file prima dell'upload
 * @param {File} file - File da validare
 * @returns {Object} - { valid: boolean, error: string }
 */
function validateFile(file) {
    // Verifica che ci sia un file
    if (!file) {
        return {
            valid: false,
            error: fileValidationConfig.messages.noFile
        };
    }

    // Verifica dimensione
    if (file.size > fileValidationConfig.maxFileSize) {
        return {
            valid: false,
            error: fileValidationConfig.messages.fileTooBig
        };
    }

    // Verifica estensione vuota
    if (!file.name || file.name.indexOf('.') === -1) {
        return {
            valid: false,
            error: fileValidationConfig.messages.invalidExtension
        };
    }

    // Estrai estensione
    var extension = '.' + file.name.split('.').pop().toLowerCase();

    // Verifica estensioni multiple (double extension attack)
    // Es: documento.pdf.exe
    var nameParts = file.name.split('.');
    if (nameParts.length > 2) {
        // Controlla se una delle estensioni intermedie è nella blacklist
        for (var i = 1; i < nameParts.length - 1; i++) {
            var intermediateExt = '.' + nameParts[i].toLowerCase();
            if (fileValidationConfig.blacklistedExtensions.indexOf(intermediateExt) !== -1) {
                return {
                    valid: false,
                    error: fileValidationConfig.messages.multipleExtensions
                };
            }
        }
    }

    // PRIORITÀ 1: Verifica se è nella BLACKLIST
    if (fileValidationConfig.blacklistedExtensions.indexOf(extension) !== -1) {
        // Messaggio specifico per ZIP
        if (['.zip', '.rar', '.7z', '.tar', '.gz'].indexOf(extension) !== -1) {
            return {
                valid: false,
                error: fileValidationConfig.messages.zipFile
            };
        }
        return {
            valid: false,
            error: fileValidationConfig.messages.blacklistedExtension + ' (' + extension + ')'
        };
    }

    // PRIORITÀ 2: Verifica se è nella whitelist delle estensioni consentite
    if (fileValidationConfig.allowedExtensions.indexOf(extension) === -1) {
        return {
            valid: false,
            error: fileValidationConfig.messages.invalidExtension + ' (' + extension + ')'
        };
    }

    // Verifica MIME type
    if (fileValidationConfig.allowedMimeTypes.indexOf(file.type) === -1) {
        return {
            valid: false,
            error: fileValidationConfig.messages.invalidMimeType + ' (ricevuto: ' + file.type + ')'
        };
    }

    return { valid: true };
}

/**
 * Valida file con alert user-friendly
 * @param {HTMLInputElement} inputElement - Input file element
 * @returns {boolean} - true se valido, false altrimenti
 */
function validateFileWithAlert(inputElement) {
    if (!inputElement || !inputElement.files || inputElement.files.length === 0) {
        alert(fileValidationConfig.messages.noFile);
        return false;
    }

    var file = inputElement.files[0];
    var validation = validateFile(file);

    if (!validation.valid) {
        alert('ERRORE DI VALIDAZIONE FILE:\n\n' + validation.error);
        // Reset input
        inputElement.value = '';
        return false;
    }

    return true;
}

/**
 * Aggiunge listener di validazione a un input file
 * @param {string} selector - Selettore CSS dell'input file
 */
function attachFileValidation(selector) {
    var inputs = document.querySelectorAll(selector);
    inputs.forEach(function(input) {
        input.addEventListener('change', function(e) {
            if (this.files && this.files.length > 0) {
                var validation = validateFile(this.files[0]);
                if (!validation.valid) {
                    alert('ERRORE DI VALIDAZIONE FILE:\n\n' + validation.error);
                    this.value = '';
                    // Reset anche il campo visual path se presente
                    var filePath = this.parentElement.parentElement.querySelector('.file-path');
                    if (filePath) {
                        filePath.value = '';
                    }
                }
            }
        });
    });
}

/**
 * Verifica se un file è un archivio ZIP basandosi sul contenuto
 * (verifica aggiuntiva per double extension attacks)
 * @param {File} file - File da verificare
 * @returns {Promise<boolean>}
 */
function isZipFileByContent(file) {
    return new Promise(function(resolve, reject) {
        var reader = new FileReader();
        reader.onload = function(e) {
            var bytes = new Uint8Array(e.target.result);

            // Signatures ZIP comuni
            var isZip = (
                // ZIP standard
                (bytes[0] === 0x50 && bytes[1] === 0x4B && bytes[2] === 0x03 && bytes[3] === 0x04) ||
                // ZIP empty
                (bytes[0] === 0x50 && bytes[1] === 0x4B && bytes[2] === 0x05 && bytes[3] === 0x06) ||
                // ZIP spanned
                (bytes[0] === 0x50 && bytes[1] === 0x4B && bytes[2] === 0x07 && bytes[3] === 0x08) ||
                // RAR
                (bytes[0] === 0x52 && bytes[1] === 0x61 && bytes[2] === 0x72 && bytes[3] === 0x21) ||
                // 7Z
                (bytes[0] === 0x37 && bytes[1] === 0x7A && bytes[2] === 0xBC && bytes[3] === 0xAF) ||
                // GZIP
                (bytes[0] === 0x1F && bytes[1] === 0x8B)
            );

            resolve(isZip);
        };
        reader.onerror = function() {
            reject(new Error('Errore lettura file'));
        };

        // Leggi solo i primi 4 bytes
        reader.readAsArrayBuffer(file.slice(0, 4));
    });
}

/**
 * Validazione avanzata con controllo signature
 * @param {File} file - File da validare
 * @returns {Promise<Object>} - { valid: boolean, error: string }
 */
async function validateFileAdvanced(file) {
    // Validazione base
    var basicValidation = validateFile(file);
    if (!basicValidation.valid) {
        return basicValidation;
    }

    // Verifica signature per rilevare ZIP mascherati
    try {
        var isZip = await isZipFileByContent(file);
        if (isZip) {
            return {
                valid: false,
                error: 'ATTENZIONE: Il file risulta essere un archivio compresso mascherato. ' +
                    'Questo tipo di file non è consentito per motivi di sicurezza.'
            };
        }
    } catch (e) {
        console.error('Errore durante la verifica signature:', e);
        // In caso di errore nella lettura, procedi comunque con la validazione server-side
    }

    return { valid: true };
}

// Auto-inizializzazione se jQuery è disponibile
if (typeof jQuery !== 'undefined') {
    jQuery(document).ready(function() {
        // Attach validazione a tutti gli input file con accept="application/pdf"
        attachFileValidation('input[type="file"][accept="application/pdf"]');
        attachFileValidation('input[name="DocAllegatoGenerico"]');
        attachFileValidation('input[name="DocEffettiFinanziari"]');
        attachFileValidation('input[name="document_object"]');
    });
}