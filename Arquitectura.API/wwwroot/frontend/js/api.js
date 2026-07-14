const API_BASE = window.location.origin + "/api";
window.API_BASE = API_BASE;

function obtenerParametro(nombre) {
    const params = new URLSearchParams(window.location.search);
    return params.get(nombre);
}

function formatearFecha(fecha) {
    if (!fecha) return "";
    return new Date(fecha).toLocaleDateString("es-CR");
}

function mostrarMensaje(idElemento, mensaje, tipo = "success") {
    const elemento = document.getElementById(idElemento);
    if (!elemento) return;

    elemento.textContent = mensaje;
    elemento.className = `message ${tipo}`;
}
