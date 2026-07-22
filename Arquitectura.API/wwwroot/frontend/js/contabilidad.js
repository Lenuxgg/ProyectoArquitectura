const CONTABILIDAD_API_BASE = (typeof API_BASE !== "undefined") ? API_BASE : `${window.location.origin}/api`;

let proyectosPermitidosContabilidad = [];

// ============================================================
// SESIÓN / TOKEN
// ============================================================

function obtenerTokenContabilidad() {
    return localStorage.getItem("cega_token") || localStorage.getItem("cega_token_jwt") || "";
}

function guardarTokenContabilidad() {
    const input = document.getElementById("tokenJwt");

    if (!input) return;

    const token = input.value.trim();

    if (!token) {
        mostrarMensaje("mensajeToken", "Debe ingresar un token válido.", "error");
        return;
    }

    const tokenLimpio = token.replace(/^Bearer\s+/i, "");
    localStorage.setItem("cega_token_jwt", tokenLimpio);
    input.value = tokenLimpio;

    mostrarMensaje("mensajeToken", "Token guardado correctamente.", "success");
}

function limpiarTokenContabilidad() {
    localStorage.removeItem("cega_token_jwt");

    const input = document.getElementById("tokenJwt");

    if (input) {
        input.value = "";
    }

    mostrarMensaje("mensajeToken", "Token eliminado.", "success");
}

function obtenerHeadersJson(requiereToken = false) {
    const headers = {
        "Content-Type": "application/json"
    };

    if (requiereToken) {
        const token = obtenerTokenContabilidad();

        if (token) {
            headers["Authorization"] = `Bearer ${token}`;
        }
    }

    return headers;
}

// ============================================================
// HELPERS API
// ============================================================

async function leerContenidoRespuesta(respuesta) {
    const texto = await respuesta.text();

    if (!texto) {
        return null;
    }

    try {
        return JSON.parse(texto);
    } catch {
        return texto;
    }
}

async function leerRespuestaError(respuesta) {
    const data = await leerContenidoRespuesta(respuesta);

    if (!data) {
        return "Ocurrió un error al procesar la solicitud.";
    }

    if (typeof data === "string") {
        return data;
    }

    if (data.mensaje) return data.mensaje;
    if (data.message) return data.message;

    if (data.errors) {
        return Object.values(data.errors).flat().join(" ");
    }

    return "Ocurrió un error al procesar la solicitud.";
}

async function apiGetContabilidad(ruta) {
    const token = obtenerTokenContabilidad();
    const headers = {};

    if (token) {
        headers["Authorization"] = `Bearer ${token}`;
    }

    const respuesta = await fetch(`${CONTABILIDAD_API_BASE}${ruta}`, {
        headers
    });

    if (!respuesta.ok) {
        throw new Error(await leerRespuestaError(respuesta));
    }

    return await leerContenidoRespuesta(respuesta);
}

async function apiEnviarContabilidad(ruta, metodo, datos, requiereToken = true) {
    const token = obtenerTokenContabilidad();

    if (requiereToken && !token) {
        throw new Error("Debe iniciar sesión antes de realizar esta acción.");
    }

    const respuesta = await fetch(`${CONTABILIDAD_API_BASE}${ruta}`, {
        method: metodo,
        headers: obtenerHeadersJson(requiereToken),
        body: JSON.stringify(datos)
    });

    if (!respuesta.ok) {
        throw new Error(await leerRespuestaError(respuesta));
    }

    return await leerContenidoRespuesta(respuesta);
}

// ============================================================
// HELPERS GENERALES
// ============================================================

function setTextoSeguro(id, valor) {
    const elemento = document.getElementById(id);

    if (elemento) {
        elemento.textContent = valor;
    }
}

function setValorSeguro(id, valor) {
    const elemento = document.getElementById(id);

    if (elemento) {
        elemento.value = valor;
    }
}

function formatearMoneda(valor) {
    const numero = Number(valor || 0);

    return new Intl.NumberFormat("es-CR", {
        style: "currency",
        currency: "CRC",
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(numero).replace(/\s/g, "\u00A0");
}

function formatearFechaContabilidad(fecha) {
    if (!fecha) return "";
    return new Date(fecha).toLocaleDateString("es-CR");
}

function fechaHoyInput() {
    return new Date().toISOString().substring(0, 10);
}

function obtenerValor(objeto, ...nombres) {
    if (!objeto) {
        return null;
    }

    for (const nombre of nombres) {
        if (objeto[nombre] !== undefined && objeto[nombre] !== null) {
            return objeto[nombre];
        }
    }

    return null;
}

function obtenerNumero(objeto, ...nombres) {
    const valor = obtenerValor(objeto, ...nombres);
    const numero = Number(valor);
    return Number.isFinite(numero) ? numero : 0;
}

function obtenerTexto(objeto, ...nombres) {
    const valor = obtenerValor(objeto, ...nombres);
    return valor === null || valor === undefined ? "" : String(valor);
}

function obtenerProyectoIdDesdeSelect(idSelect) {
    const valor = document.getElementById(idSelect)?.value;

    if (!valor) {
        return null;
    }

    return parseInt(valor);
}

function proyectoPermitidoParaEmpleado(proyectoId) {
    if (usuarioEsAdmin()) {
        return true;
    }

    if (!proyectoId) {
        return false;
    }

    return proyectosPermitidosContabilidad.some(p => p.id === proyectoId);
}

// ============================================================
// PERMISOS DE VISTA
// ============================================================

function aplicarPermisosContabilidad() {
    if (usuarioEsAdmin()) {
        return;
    }

    const seccionesAdmin = [
        "seccionResumenAdmin",
        "seccionCierreAdmin",
        "seccionCategoriasAdmin",
        "seccionNominaAdmin"
    ];

    seccionesAdmin.forEach(id => {
        const elemento = document.getElementById(id);

        if (elemento) {
            elemento.style.display = "none";
        }
    });

    const titulo = document.getElementById("tituloContabilidad");

    if (titulo) {
        titulo.textContent = "Contabilidad de mis proyectos";
    }

    const descripcion = document.getElementById("descripcionContabilidad");

    if (descripcion) {
        descripcion.textContent = "Consulta y registra movimientos financieros relacionados únicamente con los proyectos asignados.";
    }
}

// ============================================================
// RESUMEN FINANCIERO
// ============================================================

function actualizarResumenFinanciero(totalIngresos, totalEgresos, cantidadIngresos, cantidadEgresos) {
    const balance = totalIngresos - totalEgresos;

    setTextoSeguro("totalIngresos", formatearMoneda(totalIngresos));
    setTextoSeguro("totalEgresos", formatearMoneda(totalEgresos));
    setTextoSeguro("balance", formatearMoneda(balance));
    setTextoSeguro("cantidadIngresos", cantidadIngresos);
    setTextoSeguro("cantidadEgresos", cantidadEgresos);
}

function actualizarResumenDesdeReporte(reporte) {
    actualizarResumenFinanciero(
        obtenerNumero(reporte, "totalIngresos", "TotalIngresos"),
        obtenerNumero(reporte, "totalEgresos", "TotalEgresos"),
        obtenerNumero(reporte, "cantidadIngresos", "CantidadIngresos"),
        obtenerNumero(reporte, "cantidadEgresos", "CantidadEgresos")
    );
}

function actualizarResumenDesdeTransacciones(transacciones) {
    let totalIngresos = 0;
    let totalEgresos = 0;
    let cantidadIngresos = 0;
    let cantidadEgresos = 0;

    (transacciones || []).forEach(t => {
        const tipo = obtenerTexto(t, "tipo", "Tipo");
        const monto = obtenerNumero(t, "monto", "Monto");

        if (tipo === "Ingreso") {
            totalIngresos += monto;
            cantidadIngresos++;
        }

        if (tipo === "Egreso") {
            totalEgresos += monto;
            cantidadEgresos++;
        }
    });

    actualizarResumenFinanciero(totalIngresos, totalEgresos, cantidadIngresos, cantidadEgresos);
}

function reporteTieneDatos(reporte) {
    return obtenerNumero(reporte, "totalIngresos", "TotalIngresos") > 0 ||
        obtenerNumero(reporte, "totalEgresos", "TotalEgresos") > 0 ||
        obtenerNumero(reporte, "cantidadIngresos", "CantidadIngresos") > 0 ||
        obtenerNumero(reporte, "cantidadEgresos", "CantidadEgresos") > 0;
}

// ============================================================
// PROYECTOS PARA CONTABILIDAD
// ============================================================

async function cargarProyectosContabilidad() {
    try {
        validarSesion();

        const esAdmin = usuarioEsAdmin();
        const usuarioId = obtenerUsuarioIdActual();

        let ruta = "/Proyectos";

        if (!esAdmin) {
            ruta = `/Proyectos/usuario/${usuarioId}`;
        }

        const proyectos = await apiGetContabilidad(ruta);
        proyectosPermitidosContabilidad = proyectos || [];

        if (esAdmin) {
            llenarSelectProyectos("ingresoProyectoId", proyectosPermitidosContabilidad, "Sin proyecto");
            llenarSelectProyectos("egresoProyectoId", proyectosPermitidosContabilidad, "Sin proyecto");
        } else {
            llenarSelectProyectos("ingresoProyectoId", proyectosPermitidosContabilidad, "Seleccione un proyecto asignado");
            llenarSelectProyectos("egresoProyectoId", proyectosPermitidosContabilidad, "Seleccione un proyecto asignado");
        }

        llenarSelectProyectos("proyectoReporteId", proyectosPermitidosContabilidad, "Seleccione un proyecto");

    } catch (error) {
        console.error("Error cargando proyectos:", error);
        mostrarMensaje("mensajeReporteProyecto", "No se pudieron cargar los proyectos.", "error");
    }
}

function llenarSelectProyectos(idSelect, proyectos, textoInicial) {
    const select = document.getElementById(idSelect);

    if (!select) {
        return;
    }

    select.innerHTML = `<option value="">${textoInicial}</option>`;

    if (!proyectos || proyectos.length === 0) {
        return;
    }

    proyectos.forEach(proyecto => {
        select.innerHTML += `
            <option value="${proyecto.id}">${proyecto.nombre}</option>
        `;
    });
}

// ============================================================
// INICIALIZACIÓN
// ============================================================

async function inicializarContabilidad() {
    validarSesion();
    pintarUsuarioEnNavbar();
    aplicarPermisosContabilidad();

    const hoy = fechaHoyInput();
    const anioActual = new Date().getFullYear();
    const mesActual = new Date().getMonth() + 1;

    setValorSeguro("fechaIngreso", hoy);
    setValorSeguro("fechaEgreso", hoy);
    setValorSeguro("fechaInicioCierre", hoy);
    setValorSeguro("fechaFinCierre", hoy);
    setValorSeguro("anioCierre", anioActual);
    setValorSeguro("mesCierre", mesActual);
    setValorSeguro("anioNomina", anioActual);
    setValorSeguro("mesNomina", mesActual);

    const tokenJwt = document.getElementById("tokenJwt");

    if (tokenJwt) {
        tokenJwt.value = obtenerTokenContabilidad();
    }

    cambiarCamposCierre();

    await cargarProyectosContabilidad();
    await cargarPanelContabilidad();
}

async function cargarPanelContabilidad() {
    const transacciones = await cargarTransaccionesFrontend();

    if (usuarioEsAdmin()) {
        await Promise.allSettled([
            cargarReporteFinanciero(transacciones),
            cargarDesgloseFinanciero(),
            cargarNominasFrontend()
        ]);
    } else {
        actualizarResumenDesdeTransacciones(transacciones);
    }
}

// ============================================================
// REPORTE GENERAL / CATEGORÍAS
// ============================================================

async function cargarReporteFinanciero(transaccionesLocales = []) {
    if (!usuarioEsAdmin()) {
        actualizarResumenDesdeTransacciones(transaccionesLocales);
        return;
    }

    try {
        const reporte = await apiGetContabilidad("/Contabilidad/reporte");

        if (reporteTieneDatos(reporte) || !transaccionesLocales || transaccionesLocales.length === 0) {
            actualizarResumenDesdeReporte(reporte);
        }

    } catch (error) {
        console.error("No se pudo cargar /Contabilidad/reporte. Se mantiene el resumen calculado desde transacciones.", error);

        if (!transaccionesLocales || transaccionesLocales.length === 0) {
            actualizarResumenFinanciero(0, 0, 0, 0);
        }
    }
}

async function cargarDesgloseFinanciero() {
    if (!usuarioEsAdmin()) {
        return;
    }

    try {
        const desglose = await apiGetContabilidad("/Contabilidad/informe/desglose");
        renderCategorias("tablaIngresosCategoria", desglose.ingresosPorCategoria);
        renderCategorias("tablaEgresosCategoria", desglose.egresosPorCategoria);

    } catch (error) {
        const ingresos = document.getElementById("tablaIngresosCategoria");
        const egresos = document.getElementById("tablaEgresosCategoria");

        if (ingresos) {
            ingresos.innerHTML = `<tr><td colspan="3">Error: ${error.message}</td></tr>`;
        }

        if (egresos) {
            egresos.innerHTML = `<tr><td colspan="3">Error: ${error.message}</td></tr>`;
        }
    }
}

function renderCategorias(idTabla, categorias) {
    const tabla = document.getElementById(idTabla);

    if (!tabla) return;

    if (!categorias || categorias.length === 0) {
        tabla.innerHTML = `<tr><td colspan="3">No hay datos registrados.</td></tr>`;
        return;
    }

    tabla.innerHTML = "";

    categorias.forEach(c => {
        tabla.innerHTML += `
            <tr>
                <td>${c.categoria}</td>
                <td class="text-right">${formatearMoneda(c.total)}</td>
                <td>${c.cantidad}</td>
            </tr>
        `;
    });
}

// ============================================================
// TRANSACCIONES
// ============================================================

async function cargarTransaccionesFrontend() {
    const tabla = document.getElementById("tablaTransacciones");

    if (!tabla) return [];

    try {
        const transacciones = await apiGetContabilidad("/Contabilidad");

        let transaccionesFiltradas = transacciones || [];

        if (!usuarioEsAdmin()) {
            const proyectosPermitidosIds = proyectosPermitidosContabilidad.map(p => p.id);

            transaccionesFiltradas = transaccionesFiltradas.filter(t => {
                const proyectoId = obtenerValor(t, "proyectoId", "ProyectoId");
                return proyectosPermitidosIds.includes(proyectoId);
            });
        }

        if (!transaccionesFiltradas || transaccionesFiltradas.length === 0) {
            tabla.innerHTML = `<tr><td colspan="8">No hay transacciones registradas para sus proyectos asignados.</td></tr>`;
            actualizarResumenFinanciero(0, 0, 0, 0);
            return [];
        }

        tabla.innerHTML = "";

        transaccionesFiltradas.forEach(t => {
            const id = obtenerValor(t, "id", "Id");
            const tipo = obtenerTexto(t, "tipo", "Tipo");
            const categoria = obtenerTexto(t, "categoria", "Categoria");
            const proyectoNombre = obtenerValor(t, "proyectoNombre", "ProyectoNombre") || "Sin proyecto";
            const descripcion = obtenerValor(t, "descripcion", "Descripcion") || "";
            const fecha = obtenerValor(t, "fecha", "Fecha");
            const monto = obtenerNumero(t, "monto", "Monto");
            const usuarioId = obtenerValor(t, "usuarioId", "UsuarioId") || "";
            const badge = tipo === "Ingreso" ? "badge-ingreso" : "badge-egreso";

            tabla.innerHTML += `
                <tr>
                    <td>${id}</td>
                    <td><span class="badge ${badge}">${tipo}</span></td>
                    <td>${categoria}</td>
                    <td>${proyectoNombre}</td>
                    <td>${descripcion}</td>
                    <td>${formatearFechaContabilidad(fecha)}</td>
                    <td class="text-right">${formatearMoneda(monto)}</td>
                    <td>${usuarioId}</td>
                </tr>
            `;
        });

        actualizarResumenDesdeTransacciones(transaccionesFiltradas);
        return transaccionesFiltradas;

    } catch (error) {
        tabla.innerHTML = `<tr><td colspan="8">Error: ${error.message}</td></tr>`;
        actualizarResumenFinanciero(0, 0, 0, 0);
        return [];
    }
}

async function registrarIngresoFrontend() {
    const dto = {
        categoriaId: parseInt(document.getElementById("categoriaIngreso").value),
        monto: parseFloat(document.getElementById("montoIngreso").value),
        descripcion: document.getElementById("descripcionIngreso").value.trim(),
        fecha: document.getElementById("fechaIngreso").value,
        proyectoId: obtenerProyectoIdDesdeSelect("ingresoProyectoId")
    };

    if (!dto.monto || dto.monto <= 0) {
        mostrarMensaje("mensajeIngreso", "El monto debe ser mayor a 0.", "error");
        return;
    }

    if (!usuarioEsAdmin() && !dto.proyectoId) {
        mostrarMensaje("mensajeIngreso", "Debe seleccionar uno de sus proyectos asignados.", "error");
        return;
    }

    if (!usuarioEsAdmin() && !proyectoPermitidoParaEmpleado(dto.proyectoId)) {
        mostrarMensaje("mensajeIngreso", "No tiene permiso para registrar ingresos en ese proyecto.", "error");
        return;
    }

    try {
        await apiEnviarContabilidad("/Contabilidad/ingresos", "POST", dto, true);

        mostrarMensaje("mensajeIngreso", "Ingreso registrado correctamente.", "success");

        document.getElementById("montoIngreso").value = "";
        document.getElementById("descripcionIngreso").value = "";

        if (usuarioEsAdmin()) {
            document.getElementById("ingresoProyectoId").value = "";
        }

        await cargarPanelContabilidad();

    } catch (error) {
        mostrarMensaje("mensajeIngreso", error.message, "error");
    }
}

async function registrarEgresoFrontend() {
    const dto = {
        categoriaId: parseInt(document.getElementById("categoriaEgreso").value),
        monto: parseFloat(document.getElementById("montoEgreso").value),
        descripcion: document.getElementById("descripcionEgreso").value.trim(),
        fecha: document.getElementById("fechaEgreso").value,
        proyectoId: obtenerProyectoIdDesdeSelect("egresoProyectoId")
    };

    if (!dto.monto || dto.monto <= 0) {
        mostrarMensaje("mensajeEgreso", "El monto debe ser mayor a 0.", "error");
        return;
    }

    if (!usuarioEsAdmin() && !dto.proyectoId) {
        mostrarMensaje("mensajeEgreso", "Debe seleccionar uno de sus proyectos asignados.", "error");
        return;
    }

    if (!usuarioEsAdmin() && !proyectoPermitidoParaEmpleado(dto.proyectoId)) {
        mostrarMensaje("mensajeEgreso", "No tiene permiso para registrar egresos en ese proyecto.", "error");
        return;
    }

    try {
        await apiEnviarContabilidad("/Contabilidad/egresos", "POST", dto, true);

        mostrarMensaje("mensajeEgreso", "Egreso registrado correctamente.", "success");

        document.getElementById("montoEgreso").value = "";
        document.getElementById("descripcionEgreso").value = "";

        if (usuarioEsAdmin()) {
            document.getElementById("egresoProyectoId").value = "";
        }

        await cargarPanelContabilidad();

    } catch (error) {
        mostrarMensaje("mensajeEgreso", error.message, "error");
    }
}

// ============================================================
// REPORTE FINANCIERO POR PROYECTO
// ============================================================

async function consultarReporteProyecto() {
    const proyectoId = obtenerProyectoIdDesdeSelect("proyectoReporteId");

    if (!proyectoId) {
        mostrarMensaje("mensajeReporteProyecto", "Debe seleccionar un proyecto.", "error");
        return;
    }

    if (!proyectoPermitidoParaEmpleado(proyectoId)) {
        mostrarMensaje("mensajeReporteProyecto", "No tiene permiso para consultar ese proyecto.", "error");
        return;
    }

    try {
        const reporte = await apiGetContabilidad(`/Contabilidad/proyecto/${proyectoId}`);

        document.getElementById("proyectoTotalIngresos").textContent =
            formatearMoneda(obtenerNumero(reporte, "totalIngresos", "TotalIngresos"));

        document.getElementById("proyectoTotalEgresos").textContent =
            formatearMoneda(obtenerNumero(reporte, "totalEgresos", "TotalEgresos"));

        document.getElementById("proyectoBalance").textContent =
            formatearMoneda(obtenerNumero(reporte, "balance", "Balance"));

        renderTransaccionesProyecto(obtenerValor(reporte, "transacciones", "Transacciones") || []);

        mostrarMensaje(
            "mensajeReporteProyecto",
            `Reporte consultado para ${obtenerTexto(reporte, "proyectoNombre", "ProyectoNombre")}.`,
            "success"
        );

    } catch (error) {
        mostrarMensaje("mensajeReporteProyecto", error.message, "error");
    }
}

function renderTransaccionesProyecto(transacciones) {
    const tabla = document.getElementById("tablaTransaccionesProyecto");

    if (!tabla) return;

    if (!transacciones || transacciones.length === 0) {
        tabla.innerHTML = `
            <tr>
                <td colspan="6">Este proyecto no tiene transacciones registradas.</td>
            </tr>
        `;
        return;
    }

    tabla.innerHTML = "";

    transacciones.forEach(t => {
        const tipo = obtenerTexto(t, "tipo", "Tipo");
        const badge = tipo === "Ingreso" ? "badge-ingreso" : "badge-egreso";

        tabla.innerHTML += `
            <tr>
                <td>${obtenerValor(t, "id", "Id")}</td>
                <td><span class="badge ${badge}">${tipo}</span></td>
                <td>${obtenerTexto(t, "categoria", "Categoria")}</td>
                <td>${obtenerValor(t, "descripcion", "Descripcion") || ""}</td>
                <td>${formatearFechaContabilidad(obtenerValor(t, "fecha", "Fecha"))}</td>
                <td class="text-right">${formatearMoneda(obtenerNumero(t, "monto", "Monto"))}</td>
            </tr>
        `;
    });
}

// ============================================================
// CIERRES DE CAJA
// ============================================================

function cambiarCamposCierre() {
    const tipo = document.getElementById("tipoCierre")?.value;

    if (!tipo) return;

    const campos = document.querySelectorAll(".campo-cierre");

    campos.forEach(campo => campo.style.display = "none");

    document.querySelectorAll(`.campo-${tipo}`).forEach(campo => {
        campo.style.display = "block";
    });
}

async function consultarCierreCaja() {
    if (!usuarioEsAdmin()) {
        mostrarMensaje("mensajeCierre", "Solo el administrador puede consultar cierres de caja.", "error");
        return;
    }

    const tipo = document.getElementById("tipoCierre").value;
    let ruta = "";

    if (tipo === "diario") {
        const fecha = document.getElementById("fechaInicioCierre").value;
        ruta = `/Contabilidad/cierre/diario?fecha=${fecha}`;
    }

    if (tipo === "mensual") {
        const anio = document.getElementById("anioCierre").value;
        const mes = document.getElementById("mesCierre").value;
        ruta = `/Contabilidad/cierre/mensual?anio=${anio}&mes=${mes}`;
    }

    if (tipo === "anual") {
        const anio = document.getElementById("anioCierre").value;
        ruta = `/Contabilidad/cierre/anual?anio=${anio}`;
    }

    if (tipo === "rango") {
        const inicio = document.getElementById("fechaInicioCierre").value;
        const fin = document.getElementById("fechaFinCierre").value;
        ruta = `/Contabilidad/cierre/rango?fechaInicio=${inicio}&fechaFin=${fin}`;
    }

    try {
        const cierre = await apiGetContabilidad(ruta);
        mostrarMensaje("mensajeCierre", "Cierre consultado correctamente.", "success");
        renderCierre(cierre);

    } catch (error) {
        mostrarMensaje("mensajeCierre", error.message, "error");
    }
}

function renderCierre(cierre) {
    const tabla = document.getElementById("resultadoCierre");

    if (!tabla) return;

    tabla.innerHTML = `
        <tr><th>Tipo cierre</th><td>${cierre.tipoCierre}</td></tr>
        <tr><th>Fecha inicio</th><td>${formatearFechaContabilidad(cierre.fechaInicio)}</td></tr>
        <tr><th>Fecha fin</th><td>${formatearFechaContabilidad(cierre.fechaFin)}</td></tr>
        <tr><th>Total ingresos</th><td>${formatearMoneda(cierre.totalIngresos)}</td></tr>
        <tr><th>Total egresos</th><td>${formatearMoneda(cierre.totalEgresos)}</td></tr>
        <tr><th>Balance</th><td>${formatearMoneda(cierre.balance)}</td></tr>
        <tr><th>Cantidad ingresos</th><td>${cierre.cantidadIngresos}</td></tr>
        <tr><th>Cantidad egresos</th><td>${cierre.cantidadEgresos}</td></tr>
    `;
}

// ============================================================
// NÓMINA
// ============================================================

async function revisarInconsistenciasNominaFrontend() {
    if (!usuarioEsAdmin()) {
        mostrarMensaje("mensajeNomina", "Solo el administrador puede revisar inconsistencias de nómina.", "error");
        return;
    }

    const anio = document.getElementById("anioNomina").value;
    const mes = document.getElementById("mesNomina").value;

    try {
        const resultado = await apiGetContabilidad(`/Contabilidad/nomina/inconsistencias?anio=${anio}&mes=${mes}`);
        renderInconsistencias(resultado);

        const tieneInconsistencias = obtenerValor(resultado, "tieneInconsistencias", "TieneInconsistencias");
        const totalInconsistencias = obtenerValor(resultado, "totalInconsistencias", "TotalInconsistencias") || 0;

        const mensaje = tieneInconsistencias
            ? `Se encontraron ${totalInconsistencias} inconsistencias.`
            : "No se encontraron inconsistencias.";

        mostrarMensaje("mensajeNomina", mensaje, tieneInconsistencias ? "error" : "success");

    } catch (error) {
        mostrarMensaje("mensajeNomina", error.message, "error");
    }
}

function renderInconsistencias(resultado) {
    const tabla = document.getElementById("tablaInconsistencias");

    if (!tabla) return;

    const inconsistencias = obtenerValor(resultado, "inconsistencias", "Inconsistencias") || [];

    if (inconsistencias.length === 0) {
        tabla.innerHTML = `<tr><td colspan="3">No hay inconsistencias.</td></tr>`;
        return;
    }

    tabla.innerHTML = "";

    inconsistencias.forEach(i => {
        tabla.innerHTML += `
            <tr>
                <td>${obtenerTexto(i, "tipo", "Tipo")}</td>
                <td>${obtenerTexto(i, "nombreEmpleado", "NombreEmpleado") || obtenerValor(i, "usuarioId", "UsuarioId") || "General"}</td>
                <td>${obtenerTexto(i, "detalle", "Detalle")}</td>
            </tr>
        `;
    });
}

async function procesarNominaFrontend() {
    if (!usuarioEsAdmin()) {
        mostrarMensaje("mensajeNomina", "Solo el administrador puede procesar nómina.", "error");
        return;
    }

    const confirmar = confirm("¿Desea procesar la nómina del periodo seleccionado?");
    if (!confirmar) return;

    const dto = {
        anio: parseInt(document.getElementById("anioNomina").value),
        mes: parseInt(document.getElementById("mesNomina").value),
        porcentajeDeduccion: parseFloat(document.getElementById("deduccionNomina").value || "0"),
        bonificacionGeneral: parseFloat(document.getElementById("bonificacionNomina").value || "0")
    };

    try {
        await apiEnviarContabilidad("/Contabilidad/nomina/procesar", "POST", dto, true);
        mostrarMensaje("mensajeNomina", "Nómina procesada correctamente.", "success");
        await cargarNominasFrontend();

    } catch (error) {
        mostrarMensaje("mensajeNomina", error.message, "error");
    }
}

async function cargarNominasFrontend() {
    const tabla = document.getElementById("tablaNominas");

    if (!tabla) return;

    if (!usuarioEsAdmin()) {
        tabla.innerHTML = `<tr><td colspan="6">No disponible para empleados.</td></tr>`;
        return;
    }

    try {
        const nominas = await apiGetContabilidad("/Contabilidad/nomina");

        if (!nominas || nominas.length === 0) {
            tabla.innerHTML = `<tr><td colspan="6">No hay nóminas procesadas.</td></tr>`;
            return;
        }

        tabla.innerHTML = "";

        nominas.forEach(n => {
            tabla.innerHTML += `
                <tr>
                    <td>${obtenerValor(n, "id", "Id")}</td>
                    <td>${formatearFechaContabilidad(obtenerValor(n, "periodoInicio", "PeriodoInicio"))} - ${formatearFechaContabilidad(obtenerValor(n, "periodoFin", "PeriodoFin"))}</td>
                    <td><span class="badge badge-neutral">${obtenerTexto(n, "estado", "Estado")}</span></td>
                    <td class="text-right">${formatearMoneda(obtenerNumero(n, "totalBruto", "TotalBruto"))}</td>
                    <td class="text-right">${formatearMoneda(obtenerNumero(n, "totalDeducciones", "TotalDeducciones"))}</td>
                    <td class="text-right">${formatearMoneda(obtenerNumero(n, "totalNeto", "TotalNeto"))}</td>
                </tr>
            `;
        });

    } catch (error) {
        tabla.innerHTML = `<tr><td colspan="6">Error: ${error.message}</td></tr>`;
    }
}