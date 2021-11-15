function getListaEmendamenti() {
    var session_raw = sessionStorage.getItem("listaEmendamenti");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function setListaEmendamenti(lista) {
    sessionStorage.setItem("listaEmendamenti", JSON.stringify(lista));
}

function getSelezionaTutti() {
    var session_raw = sessionStorage.getItem("SelezionaTutti");
    return JSON.parse(session_raw);
}

function setSelezionaTutti(seleziona) {
    sessionStorage.setItem("SelezionaTutti", JSON.stringify(seleziona));
}

function getListaPersone() {
    var session_raw = sessionStorage.getItem("listaPersone");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function setListaPersone(lista) {
    sessionStorage.setItem("listaPersone", JSON.stringify(lista));
}

function getClientMode() {
    var session_raw = sessionStorage.getItem("CLIENT_MODE");
    return JSON.parse(session_raw);
}

function setClientMode(mode) {
    sessionStorage.setItem("CLIENT_MODE", JSON.stringify(mode));
}

function get_ListaLegislature() {
    var session_raw = sessionStorage.getItem("ListaLegislature");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_ListaLegislature(obj) {
    sessionStorage.setItem("ListaLegislature", JSON.stringify(obj));
}

function get_ListaStatiEM() {
    var session_raw = sessionStorage.getItem("ListaStatiEM");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_ListaStatiEM(obj) {
    sessionStorage.setItem("ListaStatiEM", JSON.stringify(obj));
}

function get_ListaTipiEM() {
    var session_raw = sessionStorage.getItem("ListaTipiEM");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_ListaTipiEM(obj) {
    sessionStorage.setItem("ListaTipiEM", JSON.stringify(obj));
}

function get_ListaPartiEM() {
    var session_raw = sessionStorage.getItem("ListaPartiEM");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_ListaPartiEM(obj) {
    sessionStorage.setItem("ListaPartiEM", JSON.stringify(obj));
}

function get_ListaMissioniEM() {
    var session_raw = sessionStorage.getItem("ListaMissioniEM");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function get_ListaTitoliMissioniEM() {
    var session_raw = sessionStorage.getItem("ListaTitoliMissioniEM");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_ListaMissioniEM(obj) {
    sessionStorage.setItem("ListaMissioniEM", JSON.stringify(obj));
}

function set_ListaTitoliMissioniEM(obj) {
    sessionStorage.setItem("ListaTitoliMissioniEM", JSON.stringify(obj));
}

function get_ListaArticoliEM() {
    var session_raw = sessionStorage.getItem("ListaArticoliEM");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_ListaArticoliEM(obj) {
    sessionStorage.setItem("ListaArticoliEM", JSON.stringify(obj));
}

function get_ListaCommiEM() {
    var session_raw = sessionStorage.getItem("ListaCommiEM");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_ListaCommiEM(obj) {
    sessionStorage.setItem("ListaCommiEM", JSON.stringify(obj));
}

function get_ListaLettereEM() {
    var session_raw = sessionStorage.getItem("ListaLettereEM");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_ListaLettereEM(obj) {
    sessionStorage.setItem("ListaLettereEM", JSON.stringify(obj));
}

function get_Gruppi() {
    var session_raw = sessionStorage.getItem("GruppiPoliticiLegislatura");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_Gruppi(obj) {
    sessionStorage.setItem("GruppiPoliticiLegislatura", JSON.stringify(obj));
}

/////////////////////////////////////////////////////////////
////////
////////            FILTRI
////////
/////////////////////////////////////////////////////////////

//SEDUTE
function get_Filtri_Sedute() {
    var session_raw = sessionStorage.getItem("Filtri_Sedute");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_Filtri_Sedute(obj) {
    sessionStorage.setItem("Filtri_Sedute", JSON.stringify(obj));
}

//EM
function get_Filtri_EM() {
    var session_raw = sessionStorage.getItem("Filtri_EM");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_Filtri_EM(obj) {
    sessionStorage.setItem("Filtri_EM", JSON.stringify(obj));
}