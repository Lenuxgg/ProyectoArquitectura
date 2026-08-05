function esAdminFrontend() {
    return typeof usuarioEsAdmin === "function" && usuarioEsAdmin();
}

function obtenerHeadersAuthTareas() {
    if (typeof obtenerHeadersAuth === "function") {
        return obtenerHeadersAuth();
    }

    const token = localStorage.getItem("cega_token");

    return {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`
    };
}

function obtenerHeadersFormDataTareas() {
    const token = localStorage.getItem("cega_token");

    if (!token) {
        return {};
    }

    return {
        "Authorization": `Bearer ${token}`
    };
}

async function leerErrorTareas(respuesta, mensajePorDefecto) {
    const texto = await respuesta.text();

    if (!texto) {
        return mensajePorDefecto;
    }

    try {
        const json = JSON.parse(texto);
        return json.message || json.title || texto;
    } catch {
        return texto;
    }
}

function convertirFechaInput(fecha) {
    if (!fecha) return "";
    return fecha.split("T")[0];
}

async function cargarTareasDelProyecto(proyectoId) {
    const tabla = document.getElementById("tablaTareasProyecto");

    if (!tabla) return;

    try {
        const respuesta = await fetch(`${API_BASE}/Tareas/proyecto/${proyectoId}`, {
            headers: obtenerHeadersAuthTareas()
        });

        if (!respuesta.ok) {
            throw new Error(await leerErrorTareas(respuesta, "No se pudieron cargar las tareas."));
        }

        const tareas = await respuesta.json();

        if (!tareas || tareas.length === 0) {
            tabla.innerHTML = `<tr><td colspan="4">Este proyecto no tiene tareas registradas.</td></tr>`;
            return;
        }

        tabla.innerHTML = "";

        tareas.forEach(t => {
            const acciones = esAdminFrontend()
                ? `
                    <a class="btn-warning" href="editar-tarea.html?id=${t.id}&proyectoId=${t.proyectoId}">Editar</a>
                    <button class="btn-secondary" onclick="terminarTarea(${t.id}, ${t.proyectoId})">Terminar</button>
                `
                : `
                    <a class="btn-secondary" href="editar-tarea.html?id=${t.id}&proyectoId=${t.proyectoId}">Ver documentos</a>
                `;

            tabla.innerHTML += `
                <tr>
                    <td>${t.id}</td>
                    <td>${t.titulo}</td>
                    <td>${t.estado}</td>
                    <td>${acciones}</td>
                </tr>
            `;
        });

    } catch (error) {
        tabla.innerHTML = `<tr><td colspan="4">Error: ${error.message}</td></tr>`;
    }
}

function prepararFormularioCrearTarea() {
    const proyectoId = obtenerParametro("proyectoId");

    if (!proyectoId) {
        alert("No se recibió el ID del proyecto.");
        window.location.href = "proyectos.html";
        return;
    }

    document.getElementById("proyectoId").value = proyectoId;
    document.getElementById("proyectoIdVisible").value = proyectoId;

    document.getElementById("volverDetalle").href = `detalle-proyecto.html?id=${proyectoId}`;
    document.getElementById("btnCancelar").href = `detalle-proyecto.html?id=${proyectoId}`;
}

async function crearTarea() {
    const proyectoId = document.getElementById("proyectoId").value;

    const tarea = {
        titulo: document.getElementById("titulo").value.trim(),
        proyectoId: parseInt(proyectoId),
        descripcion: document.getElementById("descripcion").value.trim(),
        fechaInicio: document.getElementById("fechaInicio").value || null,
        fechaFin: document.getElementById("fechaFin").value || null
    };

    if (!tarea.titulo || !tarea.proyectoId) {
        mostrarMensaje("mensajeTarea", "Debe ingresar título y proyecto.", "error");
        return;
    }

    try {
        const respuesta = await fetch(`${API_BASE}/Tareas`, {
            method: "POST",
            headers: obtenerHeadersAuthTareas(),
            body: JSON.stringify(tarea)
        });

        if (!respuesta.ok) {
            throw new Error(await leerErrorTareas(respuesta, "No se pudo crear la tarea. Verifique que el proyecto esté asignado al usuario."));
        }

        mostrarMensaje("mensajeTarea", "Tarea creada correctamente.", "success");

        setTimeout(() => {
            window.location.href = `detalle-proyecto.html?id=${proyectoId}`;
        }, 800);

    } catch (error) {
        mostrarMensaje("mensajeTarea", error.message, "error");
    }
}

async function cargarTareaParaEditar() {
    const tareaId = obtenerParametro("id");
    const proyectoIdParametro = obtenerParametro("proyectoId");

    if (!tareaId) {
        alert("No se recibió el ID de la tarea.");
        window.location.href = "proyectos.html";
        return;
    }

    try {
        const respuesta = await fetch(`${API_BASE}/Tareas/${tareaId}`, {
            headers: obtenerHeadersAuthTareas()
        });

        if (!respuesta.ok) {
            throw new Error(await leerErrorTareas(respuesta, "No se encontró la tarea."));
        }

        const t = await respuesta.json();
        const proyectoId = proyectoIdParametro || t.proyectoId;

        document.getElementById("tareaId").value = t.id;
        document.getElementById("proyectoId").value = proyectoId;
        document.getElementById("titulo").value = t.titulo;
        document.getElementById("estado").value = t.estado;
        document.getElementById("descripcion").value = t.descripcion ?? "";
        document.getElementById("fechaInicio").value = t.fechaInicio ? convertirFechaInput(t.fechaInicio) : "";
        document.getElementById("fechaFin").value = t.fechaFin ? convertirFechaInput(t.fechaFin) : "";

        document.getElementById("volverDetalle").href = `detalle-proyecto.html?id=${proyectoId}`;
        document.getElementById("btnCancelar").href = `detalle-proyecto.html?id=${proyectoId}`;

        aplicarPermisosVisualesEditarTarea();
        await cargarDocumentosTarea(tareaId);

    } catch (error) {
        mostrarMensaje("mensajeTarea", error.message, "error");
    }
}

function aplicarPermisosVisualesEditarTarea() {
    if (esAdminFrontend()) {
        return;
    }

    const campos = ["titulo", "estado", "descripcion", "fechaInicio", "fechaFin"];

    campos.forEach(id => {
        const campo = document.getElementById(id);
        if (campo) campo.disabled = true;
    });

    const botones = document.querySelectorAll("button[onclick='editarTarea()']");
    botones.forEach(boton => boton.style.display = "none");

    const titulo = document.querySelector(".form-card h2");
    if (titulo) titulo.textContent = "Detalle de la tarea";
}

async function editarTarea() {
    if (!esAdminFrontend()) {
        mostrarMensaje("mensajeTarea", "Solo un administrador puede editar tareas.", "error");
        return;
    }

    const tareaId = document.getElementById("tareaId").value;
    const proyectoId = document.getElementById("proyectoId").value;

    const tarea = {
        titulo: document.getElementById("titulo").value.trim(),
        estado: document.getElementById("estado").value,
        descripcion: document.getElementById("descripcion").value.trim(),
        fechaInicio: document.getElementById("fechaInicio").value || null,
        fechaFin: document.getElementById("fechaFin").value || null
    };

    if (!tarea.titulo) {
        mostrarMensaje("mensajeTarea", "Debe ingresar el título de la tarea.", "error");
        return;
    }

    try {
        const respuesta = await fetch(`${API_BASE}/Tareas/${tareaId}`, {
            method: "PUT",
            headers: obtenerHeadersAuthTareas(),
            body: JSON.stringify(tarea)
        });

        if (!respuesta.ok) {
            throw new Error(await leerErrorTareas(respuesta, "No se pudo editar la tarea. Revise que el estado sea válido."));
        }

        mostrarMensaje("mensajeTarea", "Tarea actualizada correctamente.", "success");

        setTimeout(() => {
            window.location.href = `detalle-proyecto.html?id=${proyectoId}`;
        }, 800);

    } catch (error) {
        mostrarMensaje("mensajeTarea", error.message, "error");
    }
}

async function terminarTarea(tareaId, proyectoId) {
    if (!esAdminFrontend()) {
        alert("Solo un administrador puede terminar tareas.");
        return;
    }

    const confirmar = confirm("¿Desea marcar esta tarea como terminada?");

    if (!confirmar) return;

    try {
        const respuesta = await fetch(`${API_BASE}/Tareas/${tareaId}/terminar`, {
            method: "PUT",
            headers: obtenerHeadersAuthTareas()
        });

        if (!respuesta.ok) {
            throw new Error(await leerErrorTareas(respuesta, "No se pudo terminar la tarea."));
        }

        cargarTareasDelProyecto(proyectoId);

    } catch (error) {
        alert("Error: " + error.message);
    }
}

async function subirDocumentoTarea() {
    const tareaId = document.getElementById("tareaId").value;
    const archivoInput = document.getElementById("archivoTarea");

    if (!archivoInput.files || archivoInput.files.length === 0) {
        mostrarMensaje("mensajeDocumentoTarea", "Debe seleccionar un archivo.", "error");
        return;
    }

    const formData = new FormData();
    formData.append("archivo", archivoInput.files[0]);

    try {
        const respuesta = await fetch(`${API_BASE}/Tareas/${tareaId}/documentos`, {
            method: "POST",
            headers: obtenerHeadersFormDataTareas(),
            body: formData
        });

        if (!respuesta.ok) {
            throw new Error(await leerErrorTareas(respuesta, "No se pudo subir el documento."));
        }

        mostrarMensaje("mensajeDocumentoTarea", "Documento subido correctamente.", "success");
        archivoInput.value = "";
        await cargarDocumentosTarea(tareaId);

    } catch (error) {
        mostrarMensaje("mensajeDocumentoTarea", error.message, "error");
    }
}

async function cargarDocumentosTarea(tareaId) {
    const tabla = document.getElementById("tablaDocumentosTarea");

    if (!tabla || !tareaId) {
        return;
    }

    try {
        const respuesta = await fetch(`${API_BASE}/Tareas/${tareaId}/documentos`, {
            headers: obtenerHeadersAuthTareas()
        });

        if (!respuesta.ok) {
            throw new Error(await leerErrorTareas(respuesta, "No se pudieron cargar los documentos de la tarea."));
        }

        const documentos = await respuesta.json();

        if (!documentos || documentos.length === 0) {
            tabla.innerHTML = `
                <tr>
                    <td colspan="4">Esta tarea no tiene documentos registrados.</td>
                </tr>
            `;
            return;
        }

        tabla.innerHTML = "";

        documentos.forEach(doc => {
            const acciones = esAdminFrontend()
                ? `<button class="btn-danger" onclick="eliminarDocumentoTarea(${doc.id})">Eliminar</button>`
                : `<span class="text-muted">Sin acciones</span>`;

            tabla.innerHTML += `
                <tr>
                    <td>${doc.nombre ?? "Documento"}</td>
                    <td>${formatearFecha(doc.fecha)}</td>
                    <td><a href="${doc.rutaArchivo}" target="_blank">Abrir</a></td>
                    <td>${acciones}</td>
                </tr>
            `;
        });
    } catch (error) {
        tabla.innerHTML = `
            <tr>
                <td colspan="4">Error: ${error.message}</td>
            </tr>
        `;
    }
}

async function eliminarDocumentoTarea(documentoId) {
    if (!esAdminFrontend()) {
        mostrarMensaje("mensajeDocumentoTarea", "Solo un administrador puede eliminar documentos.", "error");
        return;
    }

    const tareaId = document.getElementById("tareaId").value;
    const confirmar = confirm("¿Desea eliminar este documento?");

    if (!confirmar) {
        return;
    }

    try {
        const respuesta = await fetch(`${API_BASE}/Tareas/documentos/${documentoId}`, {
            method: "DELETE",
            headers: obtenerHeadersAuthTareas()
        });

        if (!respuesta.ok) {
            throw new Error(await leerErrorTareas(respuesta, "No se pudo eliminar el documento."));
        }

        mostrarMensaje("mensajeDocumentoTarea", "Documento eliminado correctamente.", "success");
        await cargarDocumentosTarea(tareaId);
    } catch (error) {
        mostrarMensaje("mensajeDocumentoTarea", error.message, "error");
    }
}


document.addEventListener("DOMContentLoaded", function () {
    if (!esAdminFrontend()) {
        const btnCrearTarea = document.getElementById("btnCrearTarea");
        if (btnCrearTarea) btnCrearTarea.style.display = "none";
    }
});
