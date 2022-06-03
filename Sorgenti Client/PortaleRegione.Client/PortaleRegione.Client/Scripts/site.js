var baseUrl = "";

document.addEventListener("DOMContentLoaded",
    function() {
        // INITIALIZE MATERIALIZE v1.0.0 - https://materializecss.com/
        M.AutoInit();

        var mode = getClientMode();
        if (mode == null)
            setClientMode(1);

        setTimeout(function() {
                $("body").addClass("loaded");
            },
            200);
    });

function waiting(enable, message) {
    var instance = M.Modal.getInstance($("#waiting"));
    if (enable) {
        $("#waiting_info_message").text(message);
        instance.options.dismissible = false;
        instance.options.preventScrolling = true;
        instance.open();
    } else {
        instance.close();
    }
}

var gruppi_theme = "";
var trattazione_theme = "";

function AggiornaPosizioneTemi(baseUrl) {
    gruppi_theme = baseUrl + "/Content/gruppi-theme.css";
    trattazione_theme = baseUrl + "/Content/trattazione-theme.css";
}

function Reset_ClientMode() {
    setClientMode(1);
}

// Gestione apertura navigazione laterale DX (Ricerche/Filtri)
function openSearch() {
    var elems = document.querySelector("#slide-out-DX");
    var instances = M.Sidenav.init(elems,
        {
            edge: "right",
            draggable: true,
            onOpenStart: function() {
                // Menu laterale SX sotto a layer opaco
                $("#slide-out").css("z-index", 997);
            },
            onCloseEnd: function() {
                // Ripristino menu laterale SX
                $("#slide-out").css("z-index", 999);
            }
        });

    instances.open();
}

async function openMetaDati(emendamentoUId) {
    var em = await GetEM(emendamentoUId);
    await LoadMetaDatiEM(em);
    $("#modalMetaDati").modal("open");

    $("#btnSposta_MetaDatiPartial").on("click", function() {
        Sposta_EMTrattazione(em);
    });
    $("#btnSpostaUP_MetaDatiPartial").on("click", function () {
        SpostaUP_EMTrattazione(em);
    });
    $("#btnSpostaDOWN_MetaDatiPartial").on("click", function () {
        SpostaDOWN_EMTrattazione(em);
    });
}

function Sposta_EMTrattazione(em) {
    
    swal("Sposta emendamento selezionato in una posizione precisa",
            {
                content: {
                    element: "input",
                    attributes: { type: "number" }
                },
                buttons: { cancel: "Annulla", confirm: "Ok" }
            })
        .then((value) => {
            console.log('response', value)
            if (value == null || value == "") {
                $('#modalMetaDati').modal("open");
                return;
            }

            $.ajax({
                url: baseUrl + "/emendamenti/sposta?id=" + em.UIDEM + "&pos=" + value,
                type: "GET",
                contentType: "application/json; charset=utf-8",
                dataType: "json"
            }).done(function (data) {
                if (data.message) {
                    ErrorAlert(data.message);
                } else {
                    location.reload();
                }
            }).fail(function (err) {
                console.log("error", err);
                ErrorAlert(err.message);
            });
        });
}

function SpostaUP_EMTrattazione(em) {
    
    $.ajax({
        url: baseUrl + "/emendamenti/ordina-up?id=" + em.UIDEM,
        type: "GET",
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    }).done(function (data) {
        if (data.message) {
            ErrorAlert(data.message);
        } else {
            location.reload();
        }
    }).fail(function (err) {
        console.log("error", err);
        ErrorAlert(err.message);
    });
}

function SpostaDOWN_EMTrattazione(em) {
    
    $.ajax({
        url: baseUrl + "/emendamenti/ordina-down?id=" + em.UIDEM,
        type: "GET",
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    }).done(function (data) {
        if (data.message) {
            ErrorAlert(data.message);
        } else {
            location.reload();
        }
    }).fail(function (err) {
        console.log("error", err);
        ErrorAlert(err.message);
    });
}

async function GetEM(emUId) {
    return new Promise(async function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/emendamenti/" + emUId + "/meta-data",
            type: "GET"
        }).done(function(result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            ErrorAlert(err.message);
        });
    });
}


function go(link, switchMode) {
    var mode = getClientMode();
    if (switchMode == true) {
        if (!link) return;
        if (link.includes("mode=1")) {
            link = link.replace("mode=1", "mode=2");
        } else if (link.includes("mode=2")) {
            link = link.replace("mode=2", "mode=1");
        } else if (link.includes("?")) {
            link = link + "&mode=" + mode;
        } else {
            link = link + "?mode=" + mode;
        }
    } else {
        if (!link) return;
        if (link.includes("mode")) {
            //esco
        } else if (link.includes("?")) {
            link = link + "&mode=" + mode;
        } else {
            link = link + "?mode=" + mode;
        }
    }

    setTimeout(function() {
            $("body").removeClass("loaded");
        },
        200);
    document.location = link;
}

function AbilitaTrattazione(url) {
    var mode = getClientMode();
    if (mode == 1) {
        setClientMode(2);
        go('/attitrattazione/archivio');
    } else {
        setClientMode(1);
        go('/home');
    }
}

//EVENTI EMENDAMENTO

function DeselectALLEM() {
    $("#checkAll").prop("checked", false);
    $('input[id^="chk_EM_"]').not(this).prop("checked", false);
    setSelezionaTutti(false);
    setListaEmendamenti([]);
    AbilitaComandiMassivi(null);
}

function AbilitaComandiMassivi(uidEM) {
    if (uidEM) {
        var chk = $("#chk_EM_" + uidEM);
        var selezionaTutti = getSelezionaTutti();
        if (chk[0].checked) {
            if (selezionaTutti) {
                removeEM(uidEM); //listaEsclusiva
            } else {
                addEM(uidEM); //listaInsclusiva
            }
        } else {
            if (selezionaTutti) {
                addEM(uidEM); //listaEsclusiva
            } else {
                removeEM(uidEM); //listaInsclusiva
            }
        }
    }

    var lchk = getListaEmendamenti();

    if (lchk.length > 0 || $("#checkAll")[0].checked) {
        $("#btnComandiMassiviAdmin").show();
        $("#btnComandiMassivi").show();
        $("#btnNuovoEM").hide();
    } else {
        $("#btnComandiMassiviAdmin").hide();
        $("#btnComandiMassivi").hide();
        $("#btnNuovoEM").show();
    }
}

function checkSelectedEM() {
    var selezionaTutti = getSelezionaTutti();
    var lista = getListaEmendamenti();

    $("#checkAll").prop("checked", selezionaTutti);
    $('input[id^="chk_EM_"]').not(this).prop("checked", selezionaTutti);
    $.each(lista,
        function(index, item) {
            $("#chk_EM_" + item).prop("checked", selezionaTutti ? false : true);
        });

    AbilitaComandiMassivi(null);
}

function checkSelectedDASI() {
    var selezionaTutti = getSelezionaTutti();
    var lista = getListaAtti();

    $("#checkAll").prop("checked", selezionaTutti);
    $('input[id^="chk_Atto_"]').not(this).prop("checked", selezionaTutti);
    $.each(lista,
        function(index, item) {
            $("#chk_Atto_" + item).prop("checked", selezionaTutti ? false : true);
        });

    AbilitaComandiMassivi_DASI(null);
}

function addEM(uidEM) {
    var lista = getListaEmendamenti();
    lista.push(uidEM);
    setListaEmendamenti(lista);
}

function removeEM(uidEM) {
    var lista = getListaEmendamenti();
    var findIndex = jQuery.inArray(uidEM, lista);
    lista.splice(findIndex, 1);
    setListaEmendamenti(lista);
}

function addAtto(uidAtto) {
    var lista = getListaAtti();
    lista.push(uidAtto);
    setListaAtti(lista);
}

function removeAtto(uidAtto) {
    var lista = getListaAtti();
    var findIndex = jQuery.inArray(uidAtto, lista);
    lista.splice(findIndex, 1);
    setListaAtti(lista);
}



function ConfirmAction(id, name, action) {
    $("#emActionDisplayName").empty();
    $("#emActionDisplayName").append(name);
    $("#emActionMessage").empty();

    if (action == 1) {
        $("#btnConfermaAction").text("ELIMINA");
        $("#emActionMessage").append("Stai per eliminare l'emendamento selezionato. Sei sicuro?");
    } else if (action == 2) {
        $("#btnConfermaAction").text("RITIRA");
        $("#emActionMessage").append("Stai per ritirare l'emendamento selezionato. Sei sicuro?");
    }
    $("#btnConfermaAction").on("click",
        function() {
            $.ajax({
                url: baseUrl + "/emendamenti/azioni?id=" + id + "&azione=" + action,
                method: "GET"
            }).done(function(data) {
                $("#modalAction").modal("close");
                $("#btnConfermaAction").off("click");
                console.log(data.message);
                if (data.message) {
                    ErrorAlert(data.message);
                } else {
                    go(data);
                }
            }).fail(function(err) {
                console.log("error", err);
                ErrorAlert(err.message);
            });
        });
    $("#modalAction").modal("open");
}

function ConfirmActionDASI(id, name, action) {
    $("#attoActionDisplayName").empty();
    $("#attoActionDisplayName").append(name);
    $("#attoActionMessage").empty();

    if (action == 1) {
        $("#btnConfermaActionDASI").text("ELIMINA");
        $("#attoActionMessage").append("Stai per eliminare l'atto selezionato. Sei sicuro?");
    } else if (action == 2) {
        $("#btnConfermaActionDASI").text("RITIRA");
        $("#attoActionMessage").append("Stai per ritirare l'atto selezionato. Sei sicuro?");
    }
    $("#btnConfermaActionDASI").on("click",
        function() {
            $.ajax({
                url: baseUrl + "/dasi/azioni?id=" + id + "&azione=" + action,
                method: "GET"
            }).done(function(data) {
                $("#modalActionDASI").modal("close");
                $("#btnConfermaActionDASI").off("click");
                console.log(data.message)
                if (data.message) {
                    ErrorAlert(data.message);
                } else {
                    go(data);
                }
            }).fail(function(err) {
                console.log("error", err);
                ErrorAlert(err.message);
            });
        });
    $("#modalActionDASI").modal("open");
}

function RitiraFirma(id) {

    swal("Inserisci il pin per ritirare la firma",
            {
                content: {
                    element: "input",
                    attributes: { placeholder: "******", className: "password" }
                },
                buttons: { cancel: "Annulla", confirm: "Si" }
            })
        .then((value) => {
            if (value == null || value == "")
                return;

            $.ajax({
                url: baseUrl + "/emendamenti/ritiro-firma?id=" + id + "&pin=" + value,
                method: "GET"
            }).done(function(data) {
                swal({
                    title: "Esito ritiro firma",
                    text: data.message,
                    icon: "info",
                    button: "OK"
                }).then(() => {
                    location.reload();
                });
            }).fail(function(err) {
                console.log("error", err);
                ErrorAlert(err.message);
            });
        });
}

function EliminaFirma(id) {

    swal("Inserisci il pin per eliminare la firma",
            {
                content: {
                    element: "input",
                    attributes: { placeholder: "******", className: "password" }
                },
                buttons: { cancel: "Annulla", confirm: "Si" }
            })
        .then((value) => {
            if (value == null || value == "")
                return;

            $.ajax({
                url: baseUrl + "/emendamenti/elimina-firma?id=" + id + "&pin=" + value,
                method: "GET"
            }).done(function(data) {
                if (data.message) {
                    swal({
                        title: "Esito ritiro firma",
                        text: data.message,
                        icon: "info",
                        button: "OK"
                    }).then(() => {
                        location.reload();
                    });
                } else {
                    go(data);
                }
            }).fail(function(err) {
                console.log("error", err);
                ErrorAlert(err.message);
            });
        });
}

function RevealFirmatari(uidem) {
    var panel = $("#reveal_" + uidem + " #dataItems");
    panel.show();
    var panelArea = $("#reveal_" + uidem + " #PinArea");
    panelArea.hide();
    $("#titleReveal").text("Firmatari");
    return new Promise(function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/emendamenti/firmatari",
            data: { id: uidem, tipo: 3, tag: true },
            type: "GET"
        }).done(function(data) {
            panel.empty();
            if (data.length > 0) {
                panel.append(data);
            } else {
                //Pannello di cortesia
                panel.append(
                    "<div>L'emendamento non è stato ancora firmato</div>");
            }
        }).fail(function(err) {
            console.log("error", err);
            ErrorAlert(err.message);
        });
    });
}

function RevealFirmaDeposito(id, action) {
    var text = "";
    var button = "";
    if (action == 3) {
        text = "Inserisci il PIN per firmare";
        button = "Firma";
    } else if (action == 4) {
        text = "Inserisci il PIN per depositare";
        button = "Deposita";
    }

    swal(text,
            {
                content: {
                    element: "input",
                    attributes: { placeholder: "******", className: "password" }
                },
                buttons: { cancel: "Annulla", confirm: button }
            })
        .then((value) => {
            if (value == null || value == "")
                return;

            $.ajax({
                url: baseUrl + "/emendamenti/azioni?id=" + id + "&azione=" + action + "&pin=" + value,
                method: "GET"
            }).done(function(data) {
                if (data.message) {
                    swal({
                        title: "Esito " + button,
                        text: data.message,
                        icon: "info",
                        button: "OK"
                    }).then(() => {
                        location.reload();
                    });
                } else {
                    go(data);
                }
            }).fail(function(err) {
                console.log("error", err);
                ErrorAlert(err.message);
            });
        });
}

function RevealFirmaDepositoDASI(id, action) {
    var text = "";
    var button = "";
    if (action == 3) {
        text = "Inserisci il PIN per firmare";
        button = "Firma";
    } else if (action == 4) {
        text = "Inserisci il PIN per presentare";
        button = "Presenta";
    }

    swal(text,
            {
                content: {
                    element: "input",
                    attributes: { placeholder: "******", className: "password" }
                },
                buttons: { cancel: "Annulla", confirm: button }
            })
        .then((value) => {
            if (value == null || value == "")
                return;

            $.ajax({
                url: baseUrl + "/dasi/azioni?id=" + id + "&azione=" + action + "&pin=" + value,
                method: "GET"
            }).done(function(data) {
                if (data.message) {
                    swal({
                        title: "Esito " + button,
                        text: data.message,
                        icon: "info",
                        button: "OK"
                    }).then(() => {
                        if (data.message.includes("OK")) {
                            location.reload();
                        } 
                    });
                } else {
                    go(data);
                }
            }).fail(function(err) {
                console.log("error", err);
                ErrorAlert(err.message);
            });
        });
}

function NTitolo_OnChange(item) {
    TestoEmendamento_ParteEM(item.value, "Il titolo");
}

function NCapo_OnChange(item) {
    TestoEmendamento_ParteEM(item.value, "Il capo");
}

function TestoEmendamento_ParteEM(value, text) {
    if ($("#Emendamento_TestoEM_originale_ifr").contents().find("#trumbowyg").text().length < 200) {
        var tipoEMList = $('input[name="Emendamento.IDTipo_EM"]');
        $.each(tipoEMList,
            function(index, itemTipoEM) {
                if (itemTipoEM.checked) {
                    var testo = text + " " + value;
                    if (itemTipoEM.value == 1) {
                        $("#Emendamento_TestoEM_originale_ifr").contents().find("#trumbowyg")
                            .text(testo + " è soppresso.");
                    } else if (itemTipoEM.value == 2) {
                        $("#Emendamento_TestoEM_originale_ifr").contents().find("#trumbowyg")
                            .text(testo + " è modificato come segue: ");
                    } else if (itemTipoEM.value == 3) {
                        $("#Emendamento_TestoEM_originale_ifr").contents().find("#trumbowyg")
                            .text(testo + " viene emendato aggiungendo: ");
                    } else if (itemTipoEM.value == 4) {
                        $("#Emendamento_TestoEM_originale_ifr").contents().find("#trumbowyg")
                            .text(testo + " è sostituito.");
                    }
                }
            });
    }
}

async function Articoli_OnChange(value, valueCommaSelected, valueLetteraSelected) {
    console.log('Articoli_OnChange', value, valueCommaSelected, valueLetteraSelected);
    set_ListaCommiEM([]);
    $("#ArticoliList").val(value);
    var elemsArt = document.querySelectorAll("#ArticoliList");
    M.FormSelect.init(elemsArt, null);

    var commi = await GetCommi(value);
    if (commi.length > 0) {
        $("#pnlCommi").show();
        var commiSelect = $("#CommiList");
        commiSelect.empty();

        if (valueCommaSelected)
            commiSelect.append('<option value="0">Seleziona comma</option>');
        else
            commiSelect.append('<option selected="selected" value="0">Seleziona comma</option>');

        $.each(commi,
            function(index, item) {
                var template = "";
                if (item.UIDComma == valueCommaSelected)
                    template = "<option selected='selected'></option>";
                else
                    template = "<option></option>";
                commiSelect.append($(template).val(item.UIDComma).html(item.Comma));
            });

        var elems = document.querySelectorAll("#CommiList");
        M.FormSelect.init(elems, null);

        if (valueLetteraSelected) {
            await Commi_OnChange(valueCommaSelected, valueLetteraSelected);
        }
    } else
        $("#pnlCommi").hide();
}

async function Commi_OnChange(value, valueLetteraSelected) {
    set_ListaLettereEM([]);
    var lettere = await GetLettere(value);

    if (lettere.length > 0) {
        $("#pnlLettereOLD").hide();
        $("#pnlLettere").show();
        var lettereSelect = $("#LettereList");
        lettereSelect.empty();

        if (valueLetteraSelected)
            lettereSelect.append('<option value="0">Seleziona lettera</option>');
        else
            lettereSelect.append('<option selected="selected" value="0">Seleziona lettera</option>');

        $.each(lettere,
            function(index, item) {
                var template = "";
                if (item.UIDLettera == valueLetteraSelected)
                    template = "<option selected='selected'></option>";
                else
                    template = "<option></option>";
                lettereSelect.append($(template).val(item.UIDLettera).html(item.Lettera));
            });

        var elems = document.querySelectorAll("#LettereList");
        M.FormSelect.init(elems, null);
    } else {
        $("#pnlLettere").hide();

        var letteraOLD = $("#txtLetteraOLD").val();
        if (letteraOLD != null && letteraOLD != "")
            $("#pnlLettereOLD").show();
    }
}

function EsportaXLS(attoUId) {
    swal("In che ordine devo esportare gli emendamenti?",
            {
                buttons: {
                    cancel: "Annulla",
                    presentazione: {
                        text: "Presentazione",
                        value: "1",
                    },
                    votazione: {
                        text: "Votazione",
                        value: "2",
                    }
                }
            })
        .then((value) => {

            if (value == null || value == "")
                return;

            go("emendamenti/esportaXLS?id=" + attoUId + "&ordine=" + value);
        });
}

function EsportaXLS_Segreteria(attoUId) {
    go("emendamenti/esportaXLS?id=" + attoUId + "&is_report=true");
}

function EsportaDOC(attoUId) {
    swal("In che ordine devo esportare gli emendamenti?",
            {
                buttons: {
                    cancel: "Annulla",
                    presentazione: {
                        text: "Presentazione",
                        value: "0",
                    },
                    votazione: {
                        text: "Votazione",
                        value: "1",
                    }
                }
            })
        .then((value) => {

            if (value == null || value == "")
                return;
            window.open("emendamenti/esportaDOC?id=" + attoUId + "&ordine=" + value, "_blank");
        });
}

function DownloadStampa(stampaUId) {
    go("/stampe/" + stampaUId);
}

function ResetStampa(stampaUId, url) {
    $.ajax({
        url: baseUrl + "/stampe/reset",
        data: { id: stampaUId },
        type: "GET"
    }).done(function(result) {
        console.log("RESET COMPLETE", result);
        go(url);
    }).fail(function(err) {
        console.log("error", err);
        ErrorAlert(err.message);
    });
}

function CambioStato(uidem, stato) {
    var obj = {};
    obj.Stato = stato;
    obj.Lista = [];
    obj.Lista.push(uidem);

    $.ajax({
        url: baseUrl + "/emendamenti/modifica-stato",
        type: "POST",
        data: JSON.stringify(obj),
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    }).done(function(data) {
        if (data.message) {
            ErrorAlert(data.message);
        } else {
            var label = $("#tdStato_" + uidem + ">label");
            var textStato = "";
            var cssStato = "";
            if (stato == 1) {
                textStato = "Depositato";
                cssStato = "depositatoT";
            } else if (stato == 2) {
                textStato = "Approvato";
                cssStato = "approvatoT";
            } else if (stato == 3) {
                textStato = "Non approvato";
                cssStato = "NOapprovatoT";
            } else if (stato == 4) {
                textStato = "Ritirato";
                cssStato = "ritiratoT";
            } else if (stato == 5) {
                textStato = "Decaduto";
                cssStato = "decadutoT";
            } else if (stato == 6) {
                textStato = "Inammissibile";
                cssStato = "inammissibileT";
            } else if (stato == 7) {
                textStato = "Approvato con modifiche";
                cssStato = "approvatomodT";
            }
            $(label).removeClass();
            $(label).text(textStato);
            $(label).addClass(cssStato);
        }
    }).fail(function(err) {
        console.log("error", err);
        ErrorAlert(err.message);
    });
}

function CambioStatoDASI(uidatto, stato) {
    console.log('uidatto', uidatto)
    var obj = {};
    obj.Stato = stato;
    obj.Lista = [];
    obj.Lista.push(uidatto);

    $.ajax({
        url: baseUrl + "/dasi/modifica-stato",
        type: "POST",
        data: JSON.stringify(obj),
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    }).done(function(data) {
        if (data.message) {
            ErrorAlert(data.message);
        } else {
            go(data);
        }
    }).fail(function(err) {
        console.log("error", err);
        ErrorAlert(err.message);
    });
}

function CambioStatoMassivo(stato, descr) {
    var text = "";
    var listaEM = getListaEmendamenti();
    var selezionaTutti = getSelezionaTutti();
    var text_counter = "";
    if (selezionaTutti && listaEM.length == 0) {
        text_counter = $("#hdTotaleDocumenti").val();
    } else if (selezionaTutti && listaEM.length > 0) {
        text_counter = $("#hdTotaleDocumenti").val() - listaEM.length;
    } else {
        text_counter = listaEM.length;
    }
    text = "Cambia stato di " + text_counter + " emendamenti in " + descr;

    swal(text,
            {
                buttons: { cancel: "Annulla", confirm: "Ok" }
            })
        .then((value) => {
            if (value == null || value == "")
                return;

            var obj = {};
            obj.Stato = stato;
            obj.Lista = listaEM;
            obj.All = selezionaTutti;
            obj.AttoUId = $("#hdUIdAtto").val();

            $.ajax({
                url: baseUrl + "/emendamenti/modifica-stato",
                type: "POST",
                data: JSON.stringify(obj),
                contentType: "application/json; charset=utf-8",
                dataType: "json"
            }).done(function(data) {
                DeselectALLEM();
                location.reload();
            }).fail(function(err) {
                console.log("error", err);
                ErrorAlert(err.message);
            });
        });
}

function GetPersoneFromDB() {
    return new Promise(function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/persone/all",
            type: "GET"
        }).done(function(result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            ErrorAlert(err.message);
        });
    });
}

function GetPersonePerInviti(attoUId, tipo) {
    return new Promise(function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/notifiche/destinatari?atto=" + attoUId + "&tipo=" + tipo,
            type: "GET"
        }).done(function(result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            ErrorAlert(err.message);
        });
    });
}

function GetPersonePerInviti(tipo) {
    return new Promise(function(resolve, reject) {
        $.ajax({
            url: baseUrl + "/notifiche/destinatari-dasi?tipo=" + tipo,
            type: "GET"
        }).done(function(result) {
            resolve(result);
        }).fail(function(err) {
            console.log("error", err);
            ErrorAlert(err.message);
        });
    });
}

//SEGRETERIA
function Ordina_EMTrattazione(attoUId) {
    $.ajax({
        url: baseUrl + "/emendamenti/ordina?id=" + attoUId,
        type: "GET",
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    }).done(function(data) {
        if (data.message) {
            ErrorAlert(data.message);
        } else {
            swal({
                    title: "Ordinamento automatico",
                    text: "L'operazione di ordinamento automatico è stata effettuata correttamente",
                    icon: "success",
                    buttons: true,
                })
                .then((willDelete) => {
                    location.reload();
                });

        }
    }).fail(function(err) {
        console.log("error", err);
        ErrorAlert(err.message);
    });
}


function OrdinamentoConcluso(attoUId) {
    $.ajax({
        url: baseUrl + "/emendamenti/ordinamento-concluso?id=" + attoUId,
        type: "GET",
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    }).done(function(data) {
        if (data.message) {
            ErrorAlert(data.message);
        } else {
            swal("Email inviata con successo!").then((val) => {
                location.reload();
            });
        }
    }).fail(function(err) {
        console.log("error", err);
        ErrorAlert(err.message);
    });
}

function Proponenti_OnChange(tipo) {
    var pnlConsiglieri = $("#pnlProponentiConsiglieri");
    var pnlAssessori = $("#pnlProponentiAssessori");
    if (tipo == 1) {
        //CONSIGLIERI
        pnlAssessori.hide();
        pnlConsiglieri.show();
    } else if (tipo == 2) {
        pnlConsiglieri.hide();
        pnlAssessori.show();
    }
}

//EVENTI ATTI

async function GetArticoliAtto(attoUId) {
    var tableCommi = $("#tableCommi");
    var tableLettere = $("#tableLettere");
    tableCommi.empty();
    tableLettere.empty();
    set_ListaCommiEM([]);
    set_ListaLettereEM([]);
    tableCommi.append("<li class='collection-item'>Crea da un articolo</li>");
    tableLettere.append("<li class='collection-item'>Crea da un comma</li>");

    var articoli = await GetArticoli(attoUId);
    var table = $("#tableArticoli");
    table.empty();

    table.append('<li class="collection-item"><div>Nuovo articolo <a onclick="CreaArticolo(\'' +
        attoUId +
        '\')" class="secondary-content blue-text"><i class="material-icons">add</i></a></div></li>');

    $.each(articoli,
        function(index, item) {
            table.append('<li onclick="GetCommiArticolo(\'' +
                item.UIDArticolo +
                '\')" class="collection-item" uid="' +
                item.UIDArticolo +
                '"><div>' +
                item.Articolo +
                " " +
                '<a onclick="EliminaArticolo(\'' +
                item.UIDArticolo +
                '\')" class="secondary-content"><i class="material-icons red-text">delete</i></a>' +
                "</div></li>");
        });
}

async function GetCommiArticolo(articoloUId) {

    set_ListaCommiEM([]);

    $("#tableArticoli").find("li").removeClass("active");
    $("#tableArticoli").find("li[uid='" + articoloUId + "']").addClass("active");

    var tableLettere = $("#tableLettere");
    tableLettere.empty();
    tableLettere.append("<li class='collection-item'>Crea da un comma</li>");

    var commi = await GetCommi(articoloUId);

    var table = $("#tableCommi");
    table.empty();

    table.append('<li class="collection-item"><div>Nuovo comma <a onclick="CreaComma(\'' +
        articoloUId +
        '\')" class="secondary-content blue-text"><i class="material-icons">add</i></a></div></li>');

    $.each(commi,
        function(index, item) {
            table.append('<li onclick="GetLettereComma(\'' +
                item.UIDComma +
                '\')" class="collection-item" uid="' +
                item.UIDComma +
                '"><div>' +
                item.Comma +
                " " +
                '<a onclick="EliminaComma(\'' +
                item.UIDComma +
                '\')" class="secondary-content"><i class="material-icons red-text">delete</i></a>' +
                "</div></li>");
        });
}

async function GetLettereComma(commaUId) {

    set_ListaLettereEM([]);

    $("#tableCommi").find("li").removeClass("active");
    $("#tableCommi").find("li[uid='" + commaUId + "']").addClass("active");

    var lettere = await GetLettere(commaUId);
    var table = $("#tableLettere");
    table.empty();

    table.append('<li class="collection-item"><div>Nuova lettera <a onclick="CreaLettera(\'' +
        commaUId +
        '\')" class="secondary-content blue-text"><i class="material-icons">add</i></a></div></li>');

    $.each(lettere,
        function(index, item) {
            table.append("<li class='collection-item' uid='" +
                item.UIDLettera +
                "'><div>" +
                item.Lettera +
                " " +
                '<a onclick="EliminaLettera(\'' +
                item.UIDLettera +
                '\')" class="secondary-content"><i class="material-icons red-text">delete</i></a>' +
                "</div></li>");
        });
}

function CreaArticolo(attoUId) {

    swal("Inserisci articolo",
            {
                content: { element: "input", attributes: { placeholder: "1-5, 10, 22-34" } },
                buttons: { cancel: "Annulla", confirm: "Aggiungi" }
            })
        .then((value) => {
            if (value == null || value == "")
                return;

            $.ajax({
                url: baseUrl + "/atti/crea-articoli?id=" + attoUId + "&articoli=" + value,
                method: "GET"
            }).done(function(data) {
                if (data.message) {
                    ErrorAlert(data.message);
                } else {
                    GetArticoliAtto(attoUId);
                }
            }).fail(function(err) {
                console.log("error", err);
                ErrorAlert(err.message);
            });
        });
}

function CreaComma(articoloUId) {

    swal("Inserisci comma",
            {
                content: { element: "input", attributes: { placeholder: "1-5, 10, 22-34" } },
                buttons: { cancel: "Annulla", confirm: "Aggiungi" }
            })
        .then((value) => {
            if (value == null || value == "")
                return;

            $.ajax({
                url: baseUrl + "/atti/crea-commi?id=" + articoloUId + "&commi=" + value,
                method: "GET"
            }).done(function(data) {
                if (data.message) {
                    ErrorAlert(data.message);
                } else {
                    GetCommiArticolo(articoloUId);
                }
            }).fail(function(err) {
                console.log("error", err);
                ErrorAlert(err.message);
            });
        });
}

function CreaLettera(commaUId) {

    swal("Inserisci lettera",
            {
                content: { element: "input", attributes: { placeholder: "1-5, 10, 22-34" } },
                buttons: { cancel: "Annulla", confirm: "Aggiungi" }
            })
        .then((value) => {
            if (value == null || value == "")
                return;

            $.ajax({
                url: baseUrl + "/atti/crea-lettere?id=" + commaUId + "&lettere=" + value,
                method: "GET"
            }).done(function(data) {
                if (data.message) {
                    ErrorAlert(data.message);
                } else {
                    GetLettereComma(commaUId);
                }
            }).fail(function(err) {
                console.log("error", err);
                ErrorAlert(err.message);
            });
        });
}

function EliminaArticolo(articoloUId) {
    swal({
            title: "Sei sicuro?",
            text: "Verranno eliminati tutti i commi e le loro lettere associate",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        })
        .then((willDelete) => {
            if (!willDelete) return;

            $.ajax({
                url: baseUrl + "/atti/elimina-articolo?id=" + articoloUId,
                method: "GET"
            }).done(function(data) {
                if (data.message) {
                    ErrorAlert(data.message);
                } else {
                    GetArticoliAtto($("#Atto_UIDAtto").val());
                }
            }).fail(function(err) {
                console.log("error", err);
                ErrorAlert(err.message);
            });
        });
}

function EliminaComma(commaUId) {
    swal({
            title: "Sei sicuro?",
            text: "Verranno eliminate anche tutte le lettere associate",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        })
        .then((willDelete) => {
            if (!willDelete) return;

            $.ajax({
                url: baseUrl + "/atti/elimina-comma?id=" + commaUId,
                method: "GET"
            }).done(function(data) {
                if (data.message) {
                    ErrorAlert(data.message);
                } else {
                    var uidArticoloAttivo = $("#tableArticoli").find("li.active").attr("uid");
                    GetCommiArticolo(uidArticoloAttivo);
                }
            }).fail(function(err) {
                console.log("error", err);
                ErrorAlert(err.message);
            });
        });
}

function EliminaLettera(letteraUId) {
    swal({
            title: "Sei sicuro?",
            text: "Verrà eliminata la lettera selezionata",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        })
        .then((willDelete) => {
            if (!willDelete) return;

            $.ajax({
                url: baseUrl + "/atti/elimina-lettera?id=" + letteraUId,
                method: "GET"
            }).done(function(data) {
                if (data.message) {
                    ErrorAlert(data.message);
                } else {
                    var uidCommaAttivo = $("#tableCommi").find("li.active").attr("uid");
                    GetLettereComma(uidCommaAttivo);
                }
            }).fail(function(err) {
                console.log("error", err);
                ErrorAlert(err.message);
            });
        });
}

function PubblicaFascicolo(attoUId, ordine) {
    var obj = {};
    obj.Id = attoUId;
    obj.Ordinamento = ordine;

    if (ordine == 1) {
        obj.Abilita = $("#chkAbilitaFascicoloPresentazione_" + attoUId)[0].checked;
    } else {
        obj.Abilita = $("#chkAbilitaFascicoloVotazione_" + attoUId)[0].checked;
    }

    $.ajax({
        url: baseUrl + "/atti/abilita-fascicolazione",
        type: "POST",
        data: JSON.stringify(obj),
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    }).done(function(data) {
        if (data.message) {
            ErrorAlert(data.message);
        }
        if (obj.Abilita == true) {
            SuccessAlert("Fascicolo pubblicato");
        } else {
            SuccessAlert("Fascicolo rimosso dalla pubblicazione");
        }
    }).fail(function(err) {
        console.log("error", err);
        ErrorAlert(err.message);
    });
}

//NOTIFICHE

function GetDestinatariNotifica(notificaId) {
    var panel = $("#pnlDestinatariNotifica_" + notificaId);
    panel.empty();
    $.ajax({
        url: baseUrl + "/notifiche/" + notificaId + "/destinatari",
        type: "GET",
    }).done(function(data) {
        panel.append(data);
    }).fail(function(err) {
        console.log("error", err);
        ErrorAlert(err.message);
    });
}

function GetFormattedDate(value) {
    var splitted_date_arr = value.split('(');
    var splitted_date = splitted_date_arr[splitted_date_arr.length - 1]
        .replace("/", "")
        .replace(";", "")
        .replace(')', '');
    var dateX = new Date(parseInt(splitted_date));
    var date = moment(dateX);
    return date.format("DD/MM/YYYY HH:mm");
}

// NOTIFICATION SWEETALERT.JS

function SuccessModal(message, ctrl) {
    swal({
        title: "Bel lavoro!",
        text: message,
        icon: "success",
        button: "OK"
    }).then((value) => {
        $(ctrl).modal("close");
    });
}

function SuccessAlert(message) {
    swal({
        title: "Bel lavoro!",
        text: message,
        icon: "success",
        button: "OK"
    });
}

function SuccessAlert(message, url) {
    swal({
        title: "Bel lavoro!",
        text: message,
        icon: "success",
        button: "OK"
    }).then((value) => {
        if (value == null || value == "")
            return;
        go(url);
    });
}

function ErrorAlert(message) {
    swal({
        title: "Attenzione!",
        text: message,
        icon: "error",
        button: "Ooops!"
    });
    setTimeout(function() {
            $("body").addClass("loaded");
        },
        200);
}

$.fn.serializeObject = function() {
    var o = {};
    var a = this.serializeArray();
    $.each(a,
        function() {
            if (o[this.name] !== undefined) {
                if (!o[this.name].push) {
                    o[this.name] = [o[this.name]];
                }
                o[this.name].push(this.value || "");
            } else {
                o[this.name] = this.value || "";
            }
        });
    return o;
};
