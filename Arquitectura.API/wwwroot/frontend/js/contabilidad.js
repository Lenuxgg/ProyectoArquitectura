const CONTABILIDAD_API_BASE = (typeof API_BASE !== "undefined") ? API_BASE : `${window.location.origin}/api`;

function obtenerTokenContabilidad() {
    return localStorage.getItem("cega_token_jwt") || "";
}

function guardarTokenContabilidad() {
    const token = document.getElementById("tokenJwt").value.trim();

    if (!token) {
        mostrarMensaje("mensajeToken", "Debe ingresar un token válido.", "error");
        return;
    }

    const tokenLimpio = token.replace(/^Bearer\s+/i, "");
    localStorage.setItem("cega_token_jwt", tokenLimpio);
    document.getElementById("tokenJwt").value = tokenLimpio;
    mostrarMensaje("mensajeToken", "Token guardado correctamente.", "success");
}

function limpiarTokenContabilidad() {
    localStorage.removeItem("cega_token_jwt");
    document.getElementById("tokenJwt").value = "";
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

async function leerRespuestaError(respuesta) {
    try {
        const data = await respuesta.json();

        if (data?.mensaje) return data.mensaje;
        if (data?.message) return data.message;

        if (data?.errors) {
            return Object.values(data.errors).flat().join(" ");
        }

        if (typeof data === "string") return data;

        return "Ocurrió un error al procesar la solicitud.";
    } catch {
        return await respuesta.text() || "Ocurrió un error al procesar la solicitud.";
    }
}

async function apiGetContabilidad(ruta) {
    const respuesta = await fetch(`${CONTABILIDAD_API_BASE}${ruta}`);

    if (!respuesta.ok) {
        throw new Error(await leerRespuestaError(respuesta));
    }

    return await respuesta.json();
}

async function apiEnviarContabilidad(ruta, metodo, datos, requiereToken = true) {
    const token = obtenerTokenContabilidad();

    if (requiereToken && !token) {
        throw new Error("Debe guardar un token antes de realizar esta acción.");
    }

    const respuesta = await fetch(`${CONTABILIDAD_API_BASE}${ruta}`, {
        method: metodo,
        headers: obtenerHeadersJson(requiereToken),
        body: JSON.stringify(datos)
    });

    if (!respuesta.ok) {
        throw new Error(await leerRespuestaError(respuesta));
    }

    return await respuesta.json();
}

function formatearMoneda(valor) {
    const numero = valor || 0;

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

function inicializarContabilidad() {
    const hoy = fechaHoyInput();
    const anioActual = new Date().getFullYear();
    const mesActual = new Date().getMonth() + 1;

    document.getElementById("fechaIngreso").value = hoy;
    document.getElementById("fechaEgreso").value = hoy;
    document.getElementById("fechaInicioCierre").value = hoy;
    document.getElementById("fechaFinCierre").value = hoy;
    document.getElementById("anioCierre").value = anioActual;
    document.getElementById("mesCierre").value = mesActual;
    document.getElementById("anioNomina").value = anioActual;
    document.getElementById("mesNomina").value = mesActual;
    document.getElementById("tokenJwt").value = obtenerTokenContabilidad();

    cambiarCamposCierre();
    cargarPanelContabilidad();
}

async function cargarPanelContabilidad() {
    await Promise.allSettled([
        cargarReporteFinanciero(),
        cargarDesgloseFinanciero(),
        cargarTransaccionesFrontend(),
        cargarNominasFrontend()
    ]);
}

async function cargarReporteFinanciero() {
    try {
        const reporte = await apiGetContabilidad("/Contabilidad/reporte");

        document.getElementById("totalIngresos").textContent = formatearMoneda(reporte.totalIngresos);
        document.getElementById("totalEgresos").textContent = formatearMoneda(reporte.totalEgresos);
        document.getElementById("balance").textContent = formatearMoneda(reporte.balance);
        document.getElementById("cantidadIngresos").textContent = reporte.cantidadIngresos ?? 0;
        document.getElementById("cantidadEgresos").textContent = reporte.cantidadEgresos ?? 0;
    } catch (error) {
        console.error(error);
    }
}

async function cargarDesgloseFinanciero() {
    try {
        const desglose = await apiGetContabilidad("/Contabilidad/informe/desglose");
        renderCategorias("tablaIngresosCategoria", desglose.ingresosPorCategoria);
        renderCategorias("tablaEgresosCategoria", desglose.egresosPorCategoria);
    } catch (error) {
        document.getElementById("tablaIngresosCategoria").innerHTML = `<tr><td colspan="3">Error: ${error.message}</td></tr>`;
        document.getElementById("tablaEgresosCategoria").innerHTML = `<tr><td colspan="3">Error: ${error.message}</td></tr>`;
    }
}

function renderCategorias(idTabla, categorias) {
    const tabla = document.getElementById(idTabla);

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

async function cargarTransaccionesFrontend() {
    const tabla = document.getElementById("tablaTransacciones");

    try {
        const transacciones = await apiGetContabilidad("/Contabilidad");

        if (!transacciones || transacciones.length === 0) {
            tabla.innerHTML = `<tr><td colspan="7">No hay transacciones registradas.</td></tr>`;
            return;
        }

        tabla.innerHTML = "";

        transacciones.forEach(t => {
            const badge = t.tipo === "Ingreso" ? "badge-ingreso" : "badge-egreso";

            tabla.innerHTML += `
                <tr>
                    <td>${t.id}</td>
                    <td><span class="badge ${badge}">${t.tipo}</span></td>
                    <td>${t.categoria}</td>
                    <td>${t.descripcion ?? ""}</td>
                    <td>${formatearFechaContabilidad(t.fecha)}</td>
                    <td class="text-right">${formatearMoneda(t.monto)}</td>
                    <td>${t.usuarioId}</td>
                </tr>
            `;
        });
    } catch (error) {
        tabla.innerHTML = `<tr><td colspan="7">Error: ${error.message}</td></tr>`;
    }
}

async function registrarIngresoFrontend() {
    const dto = {
        categoriaId: parseInt(document.getElementById("categoriaIngreso").value),
        monto: parseFloat(document.getElementById("montoIngreso").value),
        descripcion: document.getElementById("descripcionIngreso").value.trim(),
        fecha: document.getElementById("fechaIngreso").value
    };

    if (!dto.monto || dto.monto <= 0) {
        mostrarMensaje("mensajeIngreso", "El monto debe ser mayor a 0.", "error");
        return;
    }

    try {
        await apiEnviarContabilidad("/Contabilidad/ingresos", "POST", dto, true);
        mostrarMensaje("mensajeIngreso", "Ingreso registrado correctamente.", "success");
        document.getElementById("montoIngreso").value = "";
        document.getElementById("descripcionIngreso").value = "";
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
        fecha: document.getElementById("fechaEgreso").value
    };

    if (!dto.monto || dto.monto <= 0) {
        mostrarMensaje("mensajeEgreso", "El monto debe ser mayor a 0.", "error");
        return;
    }

    try {
        await apiEnviarContabilidad("/Contabilidad/egresos", "POST", dto, true);
        mostrarMensaje("mensajeEgreso", "Egreso registrado correctamente.", "success");
        document.getElementById("montoEgreso").value = "";
        document.getElementById("descripcionEgreso").value = "";
        await cargarPanelContabilidad();
    } catch (error) {
        mostrarMensaje("mensajeEgreso", error.message, "error");
    }
}

function cambiarCamposCierre() {
    const tipo = document.getElementById("tipoCierre").value;
    const campos = document.querySelectorAll(".campo-cierre");

    campos.forEach(campo => campo.style.display = "none");

    document.querySelectorAll(`.campo-${tipo}`).forEach(campo => {
        campo.style.display = "block";
    });
}

async function consultarCierreCaja() {
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

async function revisarInconsistenciasNominaFrontend() {
    const anio = document.getElementById("anioNomina").value;
    const mes = document.getElementById("mesNomina").value;

    try {
        const resultado = await apiGetContabilidad(`/Contabilidad/nomina/inconsistencias?anio=${anio}&mes=${mes}`);
        renderInconsistencias(resultado);

        const mensaje = resultado.tieneInconsistencias
            ? `Se encontraron ${resultado.totalInconsistencias} inconsistencias.`
            : "No se encontraron inconsistencias.";

        mostrarMensaje("mensajeNomina", mensaje, resultado.tieneInconsistencias ? "error" : "success");
    } catch (error) {
        mostrarMensaje("mensajeNomina", error.message, "error");
    }
}

function renderInconsistencias(resultado) {
    const tabla = document.getElementById("tablaInconsistencias");
    const inconsistencias = resultado.inconsistencias || [];

    if (inconsistencias.length === 0) {
        tabla.innerHTML = `<tr><td colspan="3">No hay inconsistencias.</td></tr>`;
        return;
    }

    tabla.innerHTML = "";

    inconsistencias.forEach(i => {
        tabla.innerHTML += `
            <tr>
                <td>${i.tipo}</td>
                <td>${i.nombreEmpleado ?? i.usuarioId ?? "General"}</td>
                <td>${i.detalle}</td>
            </tr>
        `;
    });
}

async function procesarNominaFrontend() {
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
                    <td>${n.id}</td>
                    <td>${formatearFechaContabilidad(n.periodoInicio)} - ${formatearFechaContabilidad(n.periodoFin)}</td>
                    <td><span class="badge badge-neutral">${n.estado}</span></td>
                    <td class="text-right">${formatearMoneda(n.totalBruto)}</td>
                    <td class="text-right">${formatearMoneda(n.totalDeducciones)}</td>
                    <td class="text-right">${formatearMoneda(n.totalNeto)}</td>
                </tr>
            `;
        });
    } catch (error) {
        tabla.innerHTML = `<tr><td colspan="6">Error: ${error.message}</td></tr>`;
    }
}
