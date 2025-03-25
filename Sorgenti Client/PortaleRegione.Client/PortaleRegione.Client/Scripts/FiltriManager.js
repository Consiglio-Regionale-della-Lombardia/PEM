
/////////////////////////////////////////////////////////////
////////
////////            SEDUTE
////////
/////////////////////////////////////////////////////////////

async function Filtri_Sedute_CaricaLegislature(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_Sedute();
    if (filtri != null) {
        filterSelect = filtri.legislatura;
    }

    var legislature = await GetLegislature();
    if (legislature.length > 0) {
        var select = $("#" + ctrlSelect);
        select.empty();
        select.append('<option value="0">Seleziona</option>');

        $.each(legislature,
            function(index, item) {
                var template = "";
                if (item.id_legislatura == filterSelect)
                    template = "<option selected='selected'></option>";
                else
                    template = "<option></option>";
                select.append($(template).val(item.id_legislatura).html(item.num_legislatura));
            });

        var elems = document.querySelectorAll("#" + ctrlSelect);
        M.FormSelect.init(elems, null);
    }
}

async function Filtri_Sedute_CaricaAnni(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_Sedute();
    if (filtri != null) {
        filterSelect = filtri.anno;
    }
    var currentYear = new Date().getFullYear();
    var select = $("#" + ctrlSelect);
    select.empty();
    select.append('<option value="0">Seleziona</option>');

    for (var i = 5; i >= 0; i--) {
        var template = "";
        if (currentYear == filterSelect)
            template = "<option selected='selected'></option>";
        else
            template = "<option></option>";
        select.append($(template).val(currentYear).html(currentYear));
        currentYear--;
    }

    var elems = document.querySelectorAll("#" + ctrlSelect);
    M.FormSelect.init(elems, null);
}

async function Filtri_Sedute_CaricaDa(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_Sedute();
    if (filtri != null) {
        filterSelect = filtri.da;
    }

    var select = $("#" + ctrlSelect);
    select.empty();
    select.val(filterSelect);
}

async function Filtri_Sedute_CaricaA(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_Sedute();
    if (filtri != null) {
        filterSelect = filtri.a;
    }

    var select = $("#" + ctrlSelect);
    select.empty();
    select.val(filterSelect);
}

function GetLegislature() {
    var legislature = get_ListaLegislature();
    if (legislature.length > 0) {
        return legislature;
    }

    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/pem/legislature",
            type: "GET"
        }).done(function(result) {
            set_ListaLegislature(result);
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetProponenti(idLegislatura) {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/persone/proponenti-firmatari?legislaturaId=" + idLegislatura,
            type: "GET"
        }).done(function(result) {
            set_ListaProponenti(result);
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

let cacheAbbinamentiDisponibili = {
    timestamp: null,
    data: null,
    legislatura: null
};

function GetAbbinamentiDisponibili(legislaturaId) {
    return new Promise(async function(resolve, reject) {
        const now = new Date().getTime();
        const oneHour = 60 * 60 * 1000;

        // Verifica se i dati sono in cache e non sono scaduti
        if (cacheAbbinamentiDisponibili.timestamp && (now - cacheAbbinamentiDisponibili.timestamp < oneHour) && cacheAbbinamentiDisponibili.legislatura == legislaturaId) {
            resolve(cacheAbbinamentiDisponibili.data);
            return;
        }

        let page = 1;
        const size = 20; // possiamo impostare una dimensione di pagina standard
        let allResults = [];

        try {
            while (true) {
                const result = await $.ajax({
                    url: `${baseUrl}/dasi/view-abbinamenti-disponibili`,
                    type: "GET",
                    data: {
                        legislaturaId: legislaturaId,
                        page: page,
                        size: size
                    }
                });

                if (result && result.length > 0) {
                    allResults = allResults.concat(result);
                    page++;
                } else {
                    break;
                }
            }

            // Memorizza i dati nella cache e aggiorna il timestamp
            cacheAbbinamentiDisponibili.data = allResults;
            cacheAbbinamentiDisponibili.timestamp = now;
            cacheAbbinamentiDisponibili.legislatura = legislaturaId;

            resolve(allResults);
        } catch (err) {
            console.log("error", err);
            reject(err);
        }
    });
}

let cacheGruppiByLegislatura = {
    timestamp: null,
    data: null,
    legislatura: null
};

function GetGruppiByLegislatura(legislaturaId) {
    return new Promise(async function (resolve, reject) {
        const now = new Date().getTime();
        const oneHour = 60 * 60 * 1000;

        // Verifica se i dati sono in cache e non sono scaduti
        if (cacheGruppiByLegislatura.timestamp && (now - cacheGruppiByLegislatura.timestamp < oneHour) && cacheGruppiByLegislatura.legislatura == legislaturaId) {
            resolve(cacheGruppiByLegislatura.data);
            return;
        }

        let page = 1;
        const size = 20; // possiamo impostare una dimensione di pagina standard
        let allResults = [];

        try {
            while (true) {
                const result = await $.ajax({
                    url: `${baseUrl}/dasi/view-gruppi-disponibili`,
                    type: "GET",
                    data: {
                        legislaturaId: legislaturaId,
                        page: page,
                        size: size
                    }
                });

                if (result && result.length > 0) {
                    allResults = allResults.concat(result);
                    page++;
                } else {
                    break;
                }
            }

            // Memorizza i dati nella cache e aggiorna il timestamp
            cacheGruppiByLegislatura.data = allResults;
            cacheGruppiByLegislatura.timestamp = now;
            cacheGruppiByLegislatura.legislatura = legislaturaId;

            resolve(allResults);
        } catch (err) {
            console.log("error", err);
            reject(err);
        }
    });
}

function GetOrganiDisponibili(legislaturaId) {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/dasi/view-organi-disponibili?legislaturaId=" + legislaturaId,
            type: "GET"
        }).done(function(result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetSedutaByData(dataSeduta) {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/pem/seduta-by-data?data=" + dataSeduta,
            type: "GET"
        }).done(function(result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetSeduteAttive() {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/pem/sedute-attive",
            type: "GET"
        }).done(function (result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetSeduteAttiveMOZU() {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/pem/sedute-attive-mozu",
            type: "GET"
        }).done(function (result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetSeduteAttiveDashboard() {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/pem/sedute-attive-dashboard",
            type: "GET"
        }).done(function (result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetTags() {
    
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/emendamenti/tags",
            type: "GET"
        }).done(function (result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetRiepilogoFirmeAtto() {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/dasi/sedute-attive",
            type: "GET"
        }).done(function (result) {
            set_ListaSeduteAttive(result);
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetGruppiInDb() {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/adminpanel/gruppi-in-db",
            type: "GET"
        }).done(function(result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function filter_sedute_legislature_OnChange() {
    var value = $("#filter_sedute_legislature").children("option:selected").val();
    var filtri_sedute = get_Filtri_Sedute();
    filtri_sedute.legislatura = value;
    set_Filtri_Sedute(filtri_sedute);
}

function filter_sedute_anno_OnChange() {
    var value = $("#filter_sedute_anno").children("option:selected").val();
    var filtri_sedute = get_Filtri_Sedute();
    filtri_sedute.anno = value;
    set_Filtri_Sedute(filtri_sedute);
}

function filter_sedute_a_OnChange() {
    var value = $("#filter_sedute_a").val();
    var filtri_sedute = get_Filtri_Sedute();
    filtri_sedute.a = value;
    set_Filtri_Sedute(filtri_sedute);
}

function filter_sedute_da_OnChange() {
    var value = $("#filter_sedute_da").val();
    var filtri_sedute = get_Filtri_Sedute();
    filtri_sedute.da = value;
    set_Filtri_Sedute(filtri_sedute);
}

/////////////////////////////////////////////////////////////
////////
////////            EMENDAMENTI
////////
/////////////////////////////////////////////////////////////

function Filtri_EM_CaricaText1(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_EM();
    if (filtri != null) {
        filterSelect = filtri.text1;
        highlight(filterSelect);
    }

    var select = $("#" + ctrlSelect);
    select.empty();
    select.val(filterSelect);
}

function Filtri_EM_CaricaText2(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_EM();
    if (filtri != null) {
        filterSelect = filtri.text2;
        highlight(filterSelect);
    }

    var select = $("#" + ctrlSelect);
    select.empty();
    select.val(filterSelect);
}

function Filtri_EM_CaricaTextConnector(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_EM();
    if (filtri != null) {
        filterSelect = filtri.text_connector;
    }

    var select = $("#" + ctrlSelect);
    select.empty();
    select.append("<option value='1'>AND</option>");
    select.append("<option value='2'>OR</option>");
    select.val(filterSelect);
    var elems = document.querySelectorAll("#" + ctrlSelect);
    M.FormSelect.init(elems, null);
}

function Filtri_EM_CaricaNumeroEM(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_EM();
    if (filtri != null) {
        filterSelect = filtri.n_em;
    }

    var select = $("#" + ctrlSelect);
    select.empty();
    select.val(filterSelect);
}

function Filtri_EM_CaricaMy(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_EM();
    if (filtri != null) {
        filterSelect = filtri.my;
    }
    var check = false;
    if (filterSelect == "1")
        check = true;
    $("#" + ctrlSelect).prop("checked", check);
}

function Filtri_EM_CaricaFinancials(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_EM();
    if (filtri != null) {
        filterSelect = filtri.financials;
    }
    var check = false;
    if (filterSelect == "1")
        check = true;
    $("#" + ctrlSelect).prop("checked", check);
}

async function Filtri_EM_CaricaStatiEM(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_EM();
    if (filtri != null) {
        filterSelect = filtri.stato;
    }

    var stati = await GetStatiEM();
    if (stati.length > 0) {
        var select = $("#" + ctrlSelect);
        select.empty();
        select.append('<option value="">Seleziona</option>');
        $.each(stati,
            function(index, item) {
                var template = "";
                if (item.IDStato == filterSelect)
                    template = "<option selected='selected'></option>";
                else
                    template = "<option></option>";
                select.append($(template).val(item.IDStato).html(item.Stato));
            });

        var elems = document.querySelectorAll("#" + ctrlSelect);
        M.FormSelect.init(elems, null);
    }
}

function GetStatiEM() {
    var stati = get_ListaStatiEM();
    if (stati.length > 0) {
        return stati;
    }

    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/emendamenti/stati-em",
            type: "GET"
        }).done(function(result) {
            set_ListaStatiEM(result);
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetStatiDASI() {
    var stati = get_ListaStatiDASI();
    if (stati.length > 0) {
        return stati;
    }

    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/dasi/stati",
            type: "GET"
        }).done(function(result) {
            set_ListaStatiDASI(result);
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetFiltriPreferitiDASI() {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/dasi/gruppo-filtri",
            type: "GET"
        }).done(function(result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetReportsDASI() {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/dasi/get-reports",
            type: "GET"
        }).done(function(result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetTipiDASI() {
    var tipi = get_ListaTipiDASI();
    if (tipi.length > 0) {
        return tipi;
    }

    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/atti/tipi",
            type: "GET"
        }).done(function(result) {
            set_ListaTipiDASI(result);
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetTipiPEM() {
    var tipi = get_ListaTipiPEM();
    if (tipi.length > 0) {
        return tipi;
    }

    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/atti/tipi?dasi=false",
            type: "GET"
        }).done(function(result) {
            set_ListaTipiPEM(result);
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetTipiMOZDASI() {
    var tipi = get_ListaTipiMOZDASI();
    if (tipi.length > 0) {
        return tipi;
    }

    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/dasi/tipi-moz",
            type: "GET"
        }).done(function(result) {
            set_ListaTipiMOZDASI(result);
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetAttiSeduteAttive() {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/dasi/odg/atti-sedute-attive",
            type: "GET"
        }).done(function(result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetTipiMOZAbbinabiliDASI() {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/dasi/moz-abbinabili",
            type: "GET"
        }).done(function(result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetSoggettiInterrogabiliDASI() {
    var soggetti = get_ListaSoggettiIterrogabiliDASI();
    if (soggetti.length > 0) {
        return soggetti;
    }

    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/dasi/soggetti-interrogabili",
            type: "GET"
        }).done(function(result) {
            set_ListaSoggettiInterrogabiliDASI(result);
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

async function Filtri_EM_CaricaTipiEM(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_EM();
    if (filtri != null) {
        filterSelect = filtri.tipo;
    }

    var tipi = await GetTipiEM();
    if (tipi.length > 0) {
        var select = $("#" + ctrlSelect);
        select.empty();
        select.append('<option value="">Seleziona</option>');

        $.each(tipi,
            function(index, item) {
                var template = "";
                if (item.IDTipo_EM == filterSelect)
                    template = "<option selected='selected'></option>";
                else
                    template = "<option></option>";
                select.append($(template).val(item.IDTipo_EM).html(item.Tipo_EM));
            });

        var elems = document.querySelectorAll("#" + ctrlSelect);
        M.FormSelect.init(elems, null);
    }
}

function GetTipiEM() {
    var tipi = get_ListaTipiEM();
    if (tipi.length > 0) {
        return tipi;
    }

    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/emendamenti/tipi-em",
            type: "GET"
        }).done(function(result) {
            set_ListaTipiEM(result);
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

async function Filtri_EM_CaricaPartiEM(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_EM();
    if (filtri != null) {
        filterSelect = filtri.parte;
    }

    var parti = await GetPartiEM();
    if (parti.length > 0) {
        var select = $("#" + ctrlSelect);
        select.empty();
        select.append('<option value="">Seleziona</option>');

        $.each(parti,
            function(index, item) {
                var template = "";
                if (item.IDParte == filterSelect) {
                    template = "<option selected='selected'></option>";
                    if (filterSelect == 4) {
                        $("#pnlFiltroArticolo").show();
                        Filtri_EM_CaricaPartiArticoloEM("filter_em_parte_articolo");
                    } else if (filterSelect == 5) {
                        $("#pnlFiltroMissione").show();
                        Filtri_EM_CaricaPartiMissioneEM("filter_em_parte_missione");
                    } else if (filterSelect == 2) {
                        $("#pnlFiltroTitolo").show();
                        Filtri_EM_CaricaPartiTitoloEM("filter_em_parte_titolo");
                    } else if (filterSelect == 3) {
                        $("#pnlFiltroCapo").show();
                        Filtri_EM_CaricaPartiCapoEM("filter_em_parte_capo");
                    }
                } else
                    template = "<option></option>";
                select.append($(template).val(item.IDParte).html(item.Parte));
            });

        var elems = document.querySelectorAll("#" + ctrlSelect);
        M.FormSelect.init(elems, null);
    }
}

function Filtri_EM_CaricaPartiTitoloEM(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_EM();
    if (filtri != null) {
        filterSelect = filtri.parte_titolo;
    }

    var select = $("#" + ctrlSelect);
    select.empty();
    select.val(filterSelect);
}

function Filtri_EM_CaricaPartiCapoEM(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_EM();
    if (filtri != null) {
        filterSelect = filtri.parte_capo;
    }

    var select = $("#" + ctrlSelect);
    select.empty();
    select.val(filterSelect);
}

async function Filtri_EM_CaricaPartiArticoloEM(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_EM();
    if (filtri != null) {
        filterSelect = filtri.parte_articolo;
    }
    $("#pnlFiltroComma").hide();

    var parti_articoli = await GetArticoli($("#inputFiltroAtto").val());
    if (parti_articoli.length > 0) {
        var select = $("#" + ctrlSelect);
        select.empty();
        select.append('<option value="">Seleziona</option>');

        $.each(parti_articoli,
            function(index, item) {
                var template = "";
                if (item.UIDArticolo == filterSelect) {
                    template = "<option selected='selected'></option>";
                    $("#pnlFiltroComma").show();
                } else
                    template = "<option></option>";
                select.append($(template).val(item.UIDArticolo).html(item.Articolo));
            });

        var elems = document.querySelectorAll("#" + ctrlSelect);
        M.FormSelect.init(elems, null);

        await Filtri_EM_CaricaPartiCommaEM("filter_em_parte_comma");
    }
}

async function Filtri_EM_CaricaPartiCommaEM(ctrlSelect) {
    var filterSelect = 0;
    var filterArticolo = 0;
    var filtri = get_Filtri_EM();
    if (filtri != null) {
        filterSelect = filtri.parte_comma;
        filterArticolo = filtri.parte_articolo;
    }

    $("#pnlFiltroLettera").hide();
    $("#pnlFiltroLetteraOLD").hide();

    if (!filterArticolo || filterArticolo == 0) return;

    var parti_commi = await GetCommi(filterArticolo, false);
    if (parti_commi.length > 0) {
        var select = $("#" + ctrlSelect);
        select.empty();
        select.append('<option value="">Seleziona</option>');

        $.each(parti_commi,
            function(index, item) {
                var template = "";
                if (item.UIDComma == filterSelect) {
                    template = "<option selected='selected'></option>";
                    $("#pnlFiltroLettera").show();
                } else
                    template = "<option></option>";
                select.append($(template).val(item.UIDComma).html(item.Comma));
            });

        var elems = document.querySelectorAll("#" + ctrlSelect);
        M.FormSelect.init(elems, null);

        await Filtri_EM_CaricaPartiLetteraEM("filter_em_parte_lettera");
    }
}

async function Filtri_EM_CaricaPartiLetteraEM(ctrlSelect) {
    var filterSelect = 0;
    var filterSelectOLD = 0;
    var filterComma = 0;
    var filtri = get_Filtri_EM();
    if (filtri != null) {
        filterSelect = filtri.parte_lettera;
        filterSelectOLD = filtri.parte_letteraOLD;
        filterComma = filtri.parte_comma;
    }

    $("#pnlFiltroLetteraOLD").hide();
    if (!filterComma || filterComma == 0) return;

    var parti_lettere = await GetLettere(filterComma);
    if (parti_lettere.length > 0) {
        var select = $("#" + ctrlSelect);
        select.empty();
        select.append('<option value="">Seleziona</option>');

        $.each(parti_lettere,
            function(index, item) {
                var template = "";
                if (item.UIDLettera == filterSelect) {
                    template = "<option selected='selected'></option>";
                } else
                    template = "<option></option>";
                select.append($(template).val(item.UIDLettera).html(item.Lettera));
            });

        var elems = document.querySelectorAll("#" + ctrlSelect);
        M.FormSelect.init(elems, null);
    } else {
        $("#pnlFiltroLettera").hide();
        $("#pnlFiltroLetteraOLD").show();

        if (filterSelectOLD) {
            $("#filter_em_parte_letteraOLD").val(filterSelectOLD);
        }
    }
}

async function Filtri_EM_CaricaPartiMissioneEM(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_EM();
    if (filtri != null) {
        filterSelect = filtri.parte_missione;
    }

    var missioni = await GetMissioni();
    if (missioni.length > 0) {
        var select = $("#" + ctrlSelect);
        select.empty();
        select.append('<option value="">Seleziona</option>');

        $.each(missioni,
            function(index, item) {
                var template = "";
                if (item.NMissione == filterSelect) {
                    template = "<option selected='selected'></option>";
                    $("#pnlFiltroProgramma").show();
                } else
                    template = "<option></option>";
                select.append($(template).val(item.NMissione).html(item.Display));
            });

        var elems = document.querySelectorAll("#" + ctrlSelect);
        M.FormSelect.init(elems, null);

        Filtri_EM_CaricaPartiProgrammaEM("filter_em_parte_programma");
    }
}

function Filtri_EM_CaricaPartiProgrammaEM(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_EM();
    if (filtri != null) {
        filterSelect = filtri.parte_programma;
    }

    var select = $("#" + ctrlSelect);
    select.empty();
    select.val(filterSelect);
}

function GetPartiEM() {
    var parti = get_ListaPartiEM();
    if (parti.length > 0) {
        return parti;
    }

    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/emendamenti/parti-em",
            type: "GET"
        }).done(function(result) {
            set_ListaPartiEM(result);
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetArticoli(attoUId) {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/atti/articoli",
            data: { id: attoUId },
            type: "GET"
        }).done(function(result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetGrigliaTesto(attoUId) {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/atti/griglia-testi",
            data: { id: attoUId, viewEm: false },
            type: "GET"
        }).done(function(result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetGrigliaTestoEM(attoUId) {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/atti/griglia-testi",
            data: { id: attoUId, viewEm: true },
            type: "GET"
        }).done(function(result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetGrigliaOrdinamentoEM(attoUId) {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/atti/griglia-ordinamento",
            data: { id: attoUId },
            type: "GET"
        }).done(function(result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}
function GetCommi(articoloUId, expanded) {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/atti/commi",
            data: { id: articoloUId, expanded: expanded },
            type: "GET"
        }).done(function(result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetLettere(commaUId) {
    
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/atti/lettere",
            data: { id: commaUId },
            type: "GET"
        }).done(function(result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetMissioni() {
    var missioni = get_ListaMissioniEM();
    if (missioni.length > 0) {
        return missioni;
    }

    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/emendamenti/missioni-em",
            type: "GET"
        }).done(function(result) {
            set_ListaMissioniEM(result);
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

function GetTitoliMissioni() {
    var missioni = get_ListaTitoliMissioniEM();
    if (missioni.length > 0) {
        return missioni;
    }

    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/emendamenti/titoli-missioni-em",
            type: "GET"
        }).done(function(result) {
            set_ListaTitoliMissioniEM(result);
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            Error(err);
        });
    });
}

//DASI
function Filtri_DASI_CaricaNAtto(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_DASI();
    if (filtri != null) {
        filterSelect = filtri.natto;
    }

    var select = $("#" + ctrlSelect);
    select.empty();
    select.val(filterSelect);
}

function Filtri_DASI_CaricaNAtto2(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_DASI();
    if (filtri != null) {
        filterSelect = filtri.natto2;
    }

    var select = $("#" + ctrlSelect);
    select.empty();
    select.val(filterSelect);
}

function Filtri_DASI_CaricaDataPresentazioneDA(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_DASI();
    if (filtri != null) {
        filterSelect = filtri.da;
    }

    var select = $("#" + ctrlSelect);
    select.empty();
    select.val(filterSelect);
}

function Filtri_DASI_CaricaDataPresentazioneA(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_DASI();
    if (filtri != null) {
        filterSelect = filtri.a;
    }

    var select = $("#" + ctrlSelect);
    select.empty();
    select.val(filterSelect);
}

function Filtri_DASI_CaricaDataSeduta(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_DASI();
    if (filtri != null) {
        filterSelect = filtri.data_seduta;
    }

    var select = $("#" + ctrlSelect);
    select.empty();
    select.val(filterSelect);
}

function Filtri_DASI_CaricaDataIscrizioneSeduta(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_DASI();
    if (filtri != null) {
        filterSelect = filtri.data_iscrizione_seduta;
    }

    var select = $("#" + ctrlSelect);
    select.empty();
    select.val(filterSelect);
}

function Filtri_DASI_CaricaOggetto(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_DASI();
    if (filtri != null) {
        filterSelect = filtri.oggetto;
        highlight(filterSelect);
    }

    var select = $("#" + ctrlSelect);
    select.empty();
    select.val(filterSelect);
}

async function Filtri_DASI_CaricaStato(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_DASI();
    if (filtri != null) {
        filterSelect = filtri.stato;
    }

    var stati = await GetStatiDASI();
    if (stati.length > 0) {
        var select = $("#" + ctrlSelect);
        select.empty();
        $.each(stati,
            function(index, item) {
                var template = "";
                if (item.IDStato == filterSelect)
                    template = "<option selected='selected'></option>";
                else
                    template = "<option></option>";
                select.append($(template).val(item.IDStato).html(item.Stato));
            });

        var elems = document.querySelectorAll("#" + ctrlSelect);
        M.FormSelect.init(elems, null);
    }
}

async function Filtri_DASI_CaricaTipo(ctrlSelect) {
    $('#pnl_tipo_mozione_urgente').hide();
    var filterSelect = 0;
    var filtri = get_Filtri_DASI();
    if (filtri != null) {
        filterSelect = filtri.tipo;
    }

    ShowHideSoloUrgenti(filterSelect);
    ShowHideProvvedimenti(filterSelect);

    var tipi = await GetTipiDASI();
    if (tipi.length > 0) {
        var select = $("#" + ctrlSelect);
        select.empty();
        $.each(tipi,
            function(index, item) {
                var template = "";
                if (item.IDTipoAtto == filterSelect) {
                    template = "<option selected='selected'></option>";
                }
                else
                    template = "<option></option>";
                select.append($(template).val(item.IDTipoAtto).html(item.Tipo_Atto));
            });

        var elems = document.querySelectorAll("#" + ctrlSelect);
        M.FormSelect.init(elems, null);
    }
}

function Filtri_DASI_CaricaTipoRisposta(ctrlSelect) {
    var filtri = get_Filtri_DASI();
    if (filtri != null) {
        $('#' + ctrlSelect).val(filtri.tipo_risposta);
        var elems = document.querySelectorAll("#" + ctrlSelect);
        M.FormSelect.init(elems, null);
    }
}

function Filtri_DASI_CaricaSoloMozioniUrgenti(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_DASI();
    if (filtri != null) {
        filterSelect = filtri.solo_urgenza;
    }
    var check = false;
    if (filterSelect == "1")
        check = true;
    $("#" + ctrlSelect).prop("checked", check);
}

async function Filtri_DASI_CaricaSoggetti(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_DASI();
    if (filtri != null) {
        filterSelect = filtri.tipo;
    }

    var tipi = await GetTipiDASI();
    if (tipi.length > 0) {
        var select = $("#" + ctrlSelect);
        select.empty();
        $.each(tipi,
            function(index, item) {
                var template = "";
                if (item.IDTipoAtto == filterSelect)
                    template = "<option selected='selected'></option>";
                else
                    template = "<option></option>";
                select.append($(template).val(item.IDTipoAtto).html(item.Tipo_Atto));
            });

        var elems = document.querySelectorAll("#" + ctrlSelect);
        M.FormSelect.init(elems, null);
    }
}

async function SetupFiltriSoggettiDestinatari() {
    var filterSelect = [];
    var filtri = get_Filtri_DASI();
    if (filtri != null) {
        if (filtri.soggetti_dest)
            filterSelect = filtri.soggetti_dest;
    }
    var soggetti = await GetSoggettiInterrogabiliDASI();
    var select = $("#qSoggettoDestinatarioDasi");
    select.empty();

    $.each(soggetti,
        function(index, item) {
            var template = "";
            var find_user = false;
            for (var i = 0; i < filterSelect.length; i++) {
                if (filterSelect[i].toString() == item.id_carica.toString()) {
                    find_user = true;
                    break;
                }
            }
            if (find_user) {
                template = "<option selected='selected'></option>";
            } else
                template = "<option></option>";
            select.append($(template).val(item.id_carica).html(item.nome_carica));
        });
    var elems = document.querySelectorAll("#qSoggettoDestinatarioDasi");
    M.FormSelect.init(elems, null);
}

async function Filtri_DASI_CaricaLegislature(ctrlSelect) {
    var filterSelect = 0;
    var filtri = get_Filtri_DASI();
    if (filtri != null) {
        filterSelect = filtri.legislatura;
    }

    var legislature = await GetLegislature();
    if (legislature.length > 0) {
        var select = $("#" + ctrlSelect);
        select.empty();
        select.append($("<option selected='selected'></option>").val("").html(""));
        $.each(legislature,
            function (index, item) {
                var template = "";
                if (item.id_legislatura == filterSelect)
                    template = "<option selected='selected'></option>";
                else
                    template = "<option></option>";
                select.append($(template).val(item.id_legislatura).html(item.num_legislatura));
            });

        var elems = document.querySelectorAll("#" + ctrlSelect);
        M.FormSelect.init(elems, null);
    }
}

async function SetupFiltriProponentiDASI() {
	var filterSelect = [];
	var filtri = get_Filtri_DASI();
	if (filtri != null) {
		if (filtri.proponenti)
			filterSelect = filtri.proponenti;
	}
	var persone = await GetPersoneFromDB();
	var select = $("#filter_proponente");
	select.empty();

	$.each(persone,
		function(index, item) {
			var template = "";
			var find_user = false;
			for (var i = 0; i < filterSelect.length; i++) {
				if (filterSelect[i].toString() == item.UID_persona.toString()) {
					find_user = true;
					break;
				}
			}
			if (find_user) {
				template = "<option selected='selected'></option>";
			}
			else
				template = "<option></option>";
			select.append($(template).val(item.UID_persona).html(item.DisplayName));
		});
	var elems = document.querySelectorAll("#filter_proponente");
	M.FormSelect.init(elems, null);
}


function filter_dasi_da_OnChange() {
    var value = $("#qDataPresentazioneDA").val();
    var filtri = get_Filtri_DASI();
    filtri.da = value;
    set_Filtri_DASI(filtri);
}

function filter_dasi_a_OnChange() {
    var value = $("#qDataPresentazioneA").val();
    var filtri = get_Filtri_DASI();
    filtri.a = value;
    set_Filtri_DASI(filtri);
}

function filter_dasi_data_seduta_OnChange() {
    var value = $("#qDataSeduta").val();
    var filtri = get_Filtri_DASI();
    filtri.data_seduta = value;
    set_Filtri_DASI(filtri);
}

function filter_dasi_data_iscrizione_seduta_OnChange() {
    var value = $("#qDataIscrizioneSeduta").val();
    var filtri = get_Filtri_DASI();
    filtri.data_iscrizione_seduta = value;
    set_Filtri_DASI(filtri);
}

function filter_dasi_oggetto_OnChange() {
    var value = $("#qOggetto").val();
    var filtri = get_Filtri_DASI();
    filtri.oggetto = value;
    set_Filtri_DASI(filtri);
}

function filter_dasi_stato_OnChange() {
    var value = $("#qStato").val();
    var filtri = get_Filtri_DASI();
    filtri.stato = value;
    set_Filtri_DASI(filtri);
}

function filter_dasi_natto_OnChange() {
    var value = $("#qNAtto").val();
    var filtri = get_Filtri_DASI();
    filtri.natto = value;
    set_Filtri_DASI(filtri);
}

function filter_dasi_natto2_OnChange() {
    var value = $("#qNAtto2").val();
    var filtri = get_Filtri_DASI();
    filtri.natto2 = value;
    set_Filtri_DASI(filtri);
}

function filter_dasi_legislatura_OnChange() {
    var value = $("#qLegislatura").val();
    var filtri = get_Filtri_DASI();
    filtri.legislatura = value;
    set_Filtri_DASI(filtri);
}

function filter_dasi_tipo_risposta_OnChange() {
    var value = $("#qTipoRisposta").val();
    var filtri = get_Filtri_DASI();
    filtri.tipo_risposta = value;
    set_Filtri_DASI(filtri);
}

function filter_dasi_tipo_OnChange() {
    var value = $("#qTipo").val();
    var filtri = get_Filtri_DASI();
    filtri.tipo = value;
    set_Filtri_DASI(filtri);

    ShowHideSoloUrgenti(value);
    ShowHideProvvedimenti(value);
}

function ShowHideSoloUrgenti(tipo) {
    if (tipo == 6) {
        $('#pnl_tipo_mozione_urgente').show();
    } else {
        $('#pnl_tipo_mozione_urgente').hide();
        $('#qTipo_Mozione_Urgente').prop("checked", false);
    }
}

function ShowHideProvvedimenti(tipo) {
    if (tipo == 7) {
        $('#pnl_provvedimenti_odg').show();
        CaricaProvvedimenti();
    } else {
        $('#pnl_provvedimenti_odg').hide();
        var filtri = get_Filtri_DASI();
        if (filtri != null) {
	        if (filtri.provvedimenti) {
		        filtri.provvedimenti = [];
		        set_Filtri_DASI(filtri);
		        CaricaProvvedimenti();
	        }
        }
    }
}

async function CaricaProvvedimenti() {
	var filterSelect = [];
	var filtri = get_Filtri_DASI();
	if (filtri != null) {
		if (filtri.provvedimenti)
			filterSelect = filtri.provvedimenti;
	}
	var provvedimenti = await GetAttiSeduteAttive();
	var select = $("#filter_provvedimenti");
	select.empty();

	$.each(provvedimenti,
		function(index, item) {
			var template = "";
			var find = false;
			for (var i = 0; i < filterSelect.length; i++) {
				if (filterSelect[i].toString() == item.UIDAtto.toString()) {
					find = true;
					break;
				}
			}
			if (find) {
				template = "<option selected='selected'></option>";
			}
			else
				template = "<option></option>";
			select.append($(template).val(item.UIDAtto).html(item.NAtto));
		});
	var elems = document.querySelectorAll("#filter_provvedimenti");
	M.FormSelect.init(elems, null);
}

function filter_dasi_tipo_mozione_urgente_OnChange() {
    var value = $("#qTipo_Mozione_Urgente").is(":checked");
    var filtri = get_Filtri_DASI();
    filtri.solo_urgenza = value;
    set_Filtri_DASI(filtri);
}

function filter_dasi_soggetti_dest_OnChange() {
    var filtri = get_Filtri_DASI();
    if (filtri.soggetti_dest == null)
        filtri.soggetti_dest = [];
    var nuovi_soggetti = [];
    if ($("#qSoggettoDestinatarioDasi option").length != 0) {
        $("#qSoggettoDestinatarioDasi option").each(function(index, opt) {
            if ($(opt).is(":checked")) {
                console.log("SOGGETTO SELEZIONATO - ", $(opt).val());
                nuovi_soggetti.push($(opt).val());
            }
        });
        filtri.soggetti_dest = nuovi_soggetti;
    }
    set_Filtri_DASI(filtri);
}


//EM
function filter_em_text1_OnChange() {
    var value = $("#filter_em_text1").val();
    var filtri_em = get_Filtri_EM();
    filtri_em.text1 = value;
    set_Filtri_EM(filtri_em);
}

function filter_em_text2_OnChange() {
    var value = $("#filter_em_text2").val();
    var filtri_em = get_Filtri_EM();
    filtri_em.text2 = value;
    set_Filtri_EM(filtri_em);
}

function filter_em_text_connector_OnChange() {
    var value = $("#filter_em_text_connector").val();
    var filtri_em = get_Filtri_EM();
    filtri_em.text_connector = value;
    set_Filtri_EM(filtri_em);
}

function filter_em_n_em_OnChange() {
    var value = $("#filter_em_n_em").val();
    var filtri_em = get_Filtri_EM();
    filtri_em.n_em = value;
    set_Filtri_EM(filtri_em);
}

function filter_em_stato_OnChange() {
    var value = $("#filter_em_stato").val();
    var filtri_em = get_Filtri_EM();
    filtri_em.stato = value;
    set_Filtri_EM(filtri_em);
}

function filter_em_tipo_OnChange() {
    var value = $("#filter_em_tipo").val();
    var filtri_em = get_Filtri_EM();
    filtri_em.tipo = value;
    set_Filtri_EM(filtri_em);
}

async function filter_em_parte_OnChange() {
    var value = $("#filter_em_parte").val();
    var filtri_em = get_Filtri_EM();
    filtri_em.parte = value;
    filtri_em.parte_articolo = "";
    filtri_em.parte_comma = "";
    filtri_em.parte_lettera = "";
    filtri_em.parte_letteraOLD = "";
    filtri_em.parte_letteraOLD = "";
    filtri_em.parte_missione = "";
    filtri_em.parte_programma = "";
    filtri_em.parte_titolo = "";
    filtri_em.parte_capo = "";
    set_Filtri_EM(filtri_em);

    $("#pnlFiltroArticolo").hide();
    $("#pnlFiltroComma").hide();
    $("#pnlFiltroLettera").hide();
    $("#pnlFiltroLetteraOLD").hide();
    $("#pnlFiltroMissione").hide();
    $("#pnlFiltroProgramma").hide();
    $("#pnlFiltroTitolo").hide();
    $("#pnlFiltroCapo").hide();

    if (value == 4) {
        $("#pnlFiltroArticolo").show();
        await Filtri_EM_CaricaPartiArticoloEM("filter_em_parte_articolo");
    } else if (value == 5) {
        $("#pnlFiltroMissione").show();
        await Filtri_EM_CaricaPartiMissioneEM("filter_em_parte_missione");
    } else if (value == 2) {
        $("#pnlFiltroTitolo").show();
        Filtri_EM_CaricaPartiTitoloEM("filter_em_parte_titolo");
    } else if (value == 3) {
        $("#pnlFiltroCapo").show();
        Filtri_EM_CaricaPartiCapoEM("filter_em_parte_capo");
    }
}

async function filter_em_parte_articolo_OnChange() {
    var value = $("#filter_em_parte_articolo").val();
    var filtri_em = get_Filtri_EM();
    filtri_em.parte_articolo = value;
    filtri_em.parte_comma = "";
    filtri_em.parte_lettera = "";
    filtri_em.parte_letteraOLD = "";
    set_Filtri_EM(filtri_em);
    $("#pnlFiltroComma").show();
    $("#pnlFiltroLettera").hide();
    await Filtri_EM_CaricaPartiCommaEM("filter_em_parte_comma");
}

async function filter_em_parte_comma_OnChange() {
    var value = $("#filter_em_parte_comma").val();
    var filtri_em = get_Filtri_EM();
    filtri_em.parte_comma = value;
    filtri_em.parte_lettera = "";
    filtri_em.parte_letteraOLD = "";
    set_Filtri_EM(filtri_em);
    $("#pnlFiltroLettera").show();
    await Filtri_EM_CaricaPartiLetteraEM("filter_em_parte_lettera");
}

function filter_em_parte_lettera_OnChange() {
    var value = $("#filter_em_parte_lettera").val();
    var filtri_em = get_Filtri_EM();
    filtri_em.parte_lettera = value;
    set_Filtri_EM(filtri_em);
}

function filter_em_parte_letteraOLD_OnChange() {
    var value = $("#filter_em_parte_letteraOLD").val();
    var filtri_em = get_Filtri_EM();
    filtri_em.parte_letteraOLD = value;
    set_Filtri_EM(filtri_em);
}

function filter_em_parte_titolo_OnChange() {
    var value = $("#filter_em_parte_titolo").val();
    var filtri_em = get_Filtri_EM();
    filtri_em.parte_titolo = value;
    set_Filtri_EM(filtri_em);
}

function filter_em_parte_capo_OnChange() {
    var value = $("#filter_em_parte_capo").val();
    var filtri_em = get_Filtri_EM();
    filtri_em.parte_capo = value;
    set_Filtri_EM(filtri_em);
}

function filter_em_parte_missione_OnChange() {
    var value = $("#filter_em_parte_missione").val();
    var filtri_em = get_Filtri_EM();
    filtri_em.parte_missione = value;
    filtri_em.parte_programma = "";
    set_Filtri_EM(filtri_em);
    $("#pnlFiltroProgramma").show();
    Filtri_EM_CaricaPartiProgrammaEM("filter_em_parte_programma");
}

function filter_em_parte_programma_OnChange() {
    var value = $("#filter_em_parte_programma").val();
    var filtri_em = get_Filtri_EM();
    filtri_em.parte_programma = value;
    set_Filtri_EM(filtri_em);
}

function filter_em_my_OnChange() {
    var value = $("#filter_em_my").is(":checked");
    var filtri_em = get_Filtri_EM();
    filtri_em.my = value;
    set_Filtri_EM(filtri_em);
}

function filter_em_financials_OnChange() {
    var value = $("#filter_em_effetti_finanziari").is(":checked");
    var filtri_em = get_Filtri_EM();
    filtri_em.financials = value;
    set_Filtri_EM(filtri_em);
}

function filter_em_gruppi_OnChange() {
    var filtri_em = get_Filtri_EM();
    if (filtri_em.gruppi == null)
        filtri_em.gruppi = [];
    var nuovi_gruppi = [];
    if ($("#filter_em_gruppi option").length != 0) {
        $("#filter_em_gruppi option").each(function(index, opt) {
            if ($(opt).is(":checked")) {
                console.log("GRUPPO ATTIVO - ", $(opt).val());
                nuovi_gruppi.push($(opt).val());
            }
        });
        filtri_em.gruppi = nuovi_gruppi;
    }
    set_Filtri_EM(filtri_em);
}

function filter_em_proponenti_OnChange() {
    var filtri_em = get_Filtri_EM();
    if (filtri_em.proponenti == null)
        filtri_em.proponenti = [];
    var nuovi_proponenti = [];
    if ($("#filter_em_proponente option").length != 0) {
        $("#filter_em_proponente option").each(function(index, opt) {
            if ($(opt).is(":checked")) {
                console.log("PROPONENTE ATTIVO - ", $(opt).val());
                nuovi_proponenti.push($(opt).val());
            }
        });
        filtri_em.proponenti = nuovi_proponenti;
    }
    set_Filtri_EM(filtri_em);
}

function filter_dasi_proponenti_OnChange() {
    var filtri = get_Filtri_DASI();
    if (filtri.proponenti == null)
        filtri.proponenti = [];
    var nuovi_proponenti = [];
    if ($("#filter_proponente option").length != 0) {
        $("#filter_proponente option").each(function(index, opt) {
            if ($(opt).is(":checked")) {
                console.log("PROPONENTE ATTIVO - ", $(opt).val());
                nuovi_proponenti.push($(opt).val());
            }
        });
        filtri.proponenti = nuovi_proponenti;
    }
    set_Filtri_DASI(filtri);
}

function filter_dasi_provvedimenti_OnChange() {
    var filtri = get_Filtri_DASI();
    if (filtri.provvedimenti == null)
        filtri.provvedimenti = [];
    var nuovi_provvedimenti = [];
    if ($("#filter_provvedimenti option").length != 0) {
        $("#filter_provvedimenti option").each(function(index, opt) {
            if ($(opt).is(":checked")) {
                nuovi_provvedimenti.push($(opt).val());
            }
        });
        filtri.provvedimenti = nuovi_provvedimenti;
    }
    set_Filtri_DASI(filtri);
}

function filter_em_firmatari_OnChange() {
    var filtri_em = get_Filtri_EM();
    if (filtri_em.firmatari == null)
        filtri_em.firmatari = [];
    var nuovi_firmatari = [];
    if ($("#filter_em_firmatari option").length != 0) {
        $("#filter_em_firmatari option").each(function(index, opt) {
            if ($(opt).is(":checked")) {
                console.log("FIRMATARIO ATTIVO - ", $(opt).val());
                nuovi_firmatari.push($(opt).val());
            }
        });
        filtri_em.firmatari = nuovi_firmatari;
    }
    set_Filtri_EM(filtri_em);
}