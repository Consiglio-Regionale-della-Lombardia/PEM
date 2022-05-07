
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
            url: baseUrl + "/sedute/legislature",
            type: "GET"
        }).done(function(result) {
            set_ListaLegislature(result);
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            ErrorAlert(err.message);
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
            ErrorAlert(err.message);
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
            ErrorAlert(err.message);
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
            ErrorAlert(err.message);
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

    var parti_commi = await GetCommi(filterArticolo);
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
            ErrorAlert(err.message);
        });
    });
}

function GetArticoli(attoUId) {
    var articoli = get_ListaArticoliEM();
    if (articoli.length > 0) {
        console.log("Articoli", articoli);
        return articoli;
    }

    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/atti/articoli",
            data: { id: attoUId },
            type: "GET"
        }).done(function(result) {
            set_ListaArticoliEM(result);
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            ErrorAlert(err.message);
        });
    });
}

function GetCommi(articoloUId) {
    var commi = get_ListaCommiEM();
    if (commi.length > 0) {
        console.log("Commi", commi);

        return commi;
    }

    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/atti/commi",
            data: { id: articoloUId },
            type: "GET"
        }).done(function(result) {
            set_ListaCommiEM(result);
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            ErrorAlert(err.message);
        });
    });
}

function GetLettere(commaUId) {
    var lettere = get_ListaLettereEM();
    if (lettere.length > 0) {
        console.log("Lettere", lettere);

        return lettere;
    }

    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/atti/lettere",
            data: { id: commaUId },
            type: "GET"
        }).done(function(result) {
            set_ListaLettereEM(result);
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            ErrorAlert(err.message);
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
            ErrorAlert(err.message);
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
            ErrorAlert(err.message);
        });
    });
}

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
    set_ListaCommiEM([]);
    set_ListaLettereEM([]);
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
    set_ListaLettereEM([]);
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
    set_ListaArticoliEM([]);
    set_ListaCommiEM([]);
    set_ListaLettereEM([]);
    var value = $("#filter_em_parte_titolo").val();
    var filtri_em = get_Filtri_EM();
    filtri_em.parte_titolo = value;
    set_Filtri_EM(filtri_em);
}

function filter_em_parte_capo_OnChange() {
    set_ListaArticoliEM([]);
    set_ListaCommiEM([]);
    set_ListaLettereEM([]);
    var value = $("#filter_em_parte_capo").val();
    var filtri_em = get_Filtri_EM();
    filtri_em.parte_capo = value;
    set_Filtri_EM(filtri_em);
}

function filter_em_parte_missione_OnChange() {
    set_ListaArticoliEM([]);
    set_ListaCommiEM([]);
    set_ListaLettereEM([]);
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