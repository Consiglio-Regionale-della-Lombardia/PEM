function getPaginationTabsVisibility() {
    var session_raw = localStorage.getItem("PaginationTabsVisibility");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function setPaginationTabsVisibility(visibility) {
    localStorage.setItem("PaginationTabsVisibility", JSON.stringify(visibility));
}

function getListaEmendamenti() {
    var session_raw = localStorage.getItem("listaEmendamenti");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function setListaEmendamenti(lista) {
    localStorage.setItem("listaEmendamenti", JSON.stringify(lista));
}

function getListaAtti() {
    var session_raw = localStorage.getItem("listaDASI");
    if (session_raw == null)
        return [];
    return JSON.parse(session_raw);
}

function setListaAtti(lista) {
    localStorage.setItem("listaDASI", JSON.stringify(lista));
}

function getSelezionaTutti() {
    var session_raw = localStorage.getItem("SelezionaTutti");
    return JSON.parse(session_raw);
}

function setSelezionaTutti(seleziona) {
	localStorage.setItem("SelezionaTutti", JSON.stringify(seleziona));
}

function getSelezionaTutti_DASI() {
    var session_raw = localStorage.getItem("SelezionaTutti_DASI");
    return JSON.parse(session_raw);
}

function setSelezionaTutti_DASI(seleziona) {
	localStorage.setItem("SelezionaTutti_DASI", JSON.stringify(seleziona));
}

function getListaPersone() {
    var session_raw = localStorage.getItem("listaPersone");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function setListaPersone(lista) {
    localStorage.setItem("listaPersone", JSON.stringify(lista));
}

function getClientMode() {
    var session_raw = localStorage.getItem("CLIENT_MODE");
    return JSON.parse(session_raw);
}

function setClientMode(mode) {
    $('#hdMode').val(mode);
    localStorage.setItem("CLIENT_MODE", JSON.stringify(mode));
}

function get_ListaLegislature() {
    var session_raw = localStorage.getItem("ListaLegislature");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_ListaLegislature(obj) {
    localStorage.setItem("ListaLegislature", JSON.stringify(obj));
}

function get_ListaProponenti() {
    var session_raw = localStorage.getItem("ListaProponenti");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_ListaProponenti(obj) {
    localStorage.setItem("ListaProponenti", JSON.stringify(obj));
}

function get_ListaStatiEM() {
    var session_raw = localStorage.getItem("ListaStatiEM");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_ListaStatiEM(obj) {
    localStorage.setItem("ListaStatiEM", JSON.stringify(obj));
}

function get_ListaStatiDASI() {
    var session_raw = localStorage.getItem("ListaStatiDASI");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function get_ListaTipiDASI() {
    var session_raw = localStorage.getItem("ListaTipiDASI");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function get_ListaTipiPEM() {
    var session_raw = localStorage.getItem("ListaTipiPEM");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function get_ListaTipiMOZDASI() {
    var session_raw = localStorage.getItem("ListaTipiMOZDASI");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function get_ListaSoggettiIterrogabiliDASI() {
    var session_raw = localStorage.getItem("ListaSoggettiInterrogabiliDASI");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_ListaStatiDASI(obj) {
    localStorage.setItem("ListaStatiDASI", JSON.stringify(obj));
}

function set_ListaTipiPEM(obj) {
    localStorage.setItem("ListaTipiPEM", JSON.stringify(obj));
}

function set_ListaTipiDASI(obj) {
    localStorage.setItem("ListaTipiDASI", JSON.stringify(obj));
}

function set_ListaTipiMOZDASI(obj) {
    localStorage.setItem("ListaTipiMOZDASI", JSON.stringify(obj));
}

function set_ListaSoggettiInterrogabiliDASI(obj) {
    localStorage.setItem("ListaSoggettiInterrogabiliDASI", JSON.stringify(obj));
}

function get_ListaTipiEM() {
    var session_raw = localStorage.getItem("ListaTipiEM");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_ListaTipiEM(obj) {
    localStorage.setItem("ListaTipiEM", JSON.stringify(obj));
}

function get_ListaPartiEM() {
    var session_raw = localStorage.getItem("ListaPartiEM");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_ListaPartiEM(obj) {
    localStorage.setItem("ListaPartiEM", JSON.stringify(obj));
}

function get_ListaMissioniEM() {
    var session_raw = localStorage.getItem("ListaMissioniEM");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function get_ListaTitoliMissioniEM() {
    var session_raw = localStorage.getItem("ListaTitoliMissioniEM");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_ListaMissioniEM(obj) {
    localStorage.setItem("ListaMissioniEM", JSON.stringify(obj));
}

function set_ListaTitoliMissioniEM(obj) {
    localStorage.setItem("ListaTitoliMissioniEM", JSON.stringify(obj));
}

function get_Gruppi() {
    var session_raw = localStorage.getItem("GruppiPoliticiLegislatura");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_Gruppi(obj) {
    localStorage.setItem("GruppiPoliticiLegislatura", JSON.stringify(obj));
}

/////////////////////////////////////////////////////////////
////////
////////            FILTRI
////////
/////////////////////////////////////////////////////////////

//SEDUTE
function get_Filtri_Sedute() {
    var session_raw = localStorage.getItem("Filtri_Sedute");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_Filtri_Sedute(obj) {
    localStorage.setItem("Filtri_Sedute", JSON.stringify(obj));
}

//EM
function get_Filtri_EM() {
    var session_raw = localStorage.getItem("Filtri_EM");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_Filtri_EM(obj) {
    localStorage.setItem("Filtri_EM", JSON.stringify(obj));
}

//DASI
function get_Filtri_DASI() {
    var session_raw = localStorage.getItem("Filtri_DASI");
    if (session_raw == null)
        return {}
    return JSON.parse(session_raw);
}

function set_Filtri_DASI(obj) {
    localStorage.setItem("Filtri_DASI", JSON.stringify(obj));
}