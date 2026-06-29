async function cargarTareasDelProyecto(proyectoId) {
    const tabla = document.getElementById("tablaTareasProyecto");

    if (!tabla) return;

    try {
        const respuesta = await fetch(`${API_BASE}/Tareas/proyecto/${proyectoId}`);
        const tareas = await respuesta.json();

        if (!respuesta.ok) {
            throw new Error("No se pudieron cargar las tareas.");
        }

        if (tareas.length === 0) {
            tabla.innerHTML = `<tr><td colspan="4">Este proyecto no tiene tareas registradas.</td></tr>`;
            return;
        }

        tabla.innerHTML = "";

        tareas.forEach(t => {
            tabla.innerHTML += `
                <tr>
                    <td>${t.id}</td>
                    <td>${t.titulo}</td>
                    <td>${t.estado}</td>
                    <td>
                        <a class="btn-warning" href="editar-tarea.html?id=${t.id}&proyectoId=${t.proyectoId}">Editar</a>
                        <button class="btn-secondary" onclick="terminarTarea(${t.id}, ${t.proyectoId})">Terminar</button>
                    </td>
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
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(tarea)
        });

        if (!respuesta.ok) {
            throw new Error("No se pudo crear la tarea.");
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
        const respuesta = await fetch(`${API_BASE}/Tareas/${tareaId}`);
        const t = await respuesta.json();

        if (!respuesta.ok) {
            throw new Error("No se encontró la tarea.");
        }

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

    } catch (error) {
        mostrarMensaje("mensajeTarea", error.message, "error");
    }
}

async function editarTarea() {
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
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(tarea)
        });

        if (!respuesta.ok) {
            throw new Error("No se pudo editar la tarea. Revise que el estado sea válido.");
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
    const confirmar = confirm("¿Desea marcar esta tarea como terminada?");

    if (!confirmar) return;

    try {
        const respuesta = await fetch(`${API_BASE}/Tareas/${tareaId}/terminar`, {
            method: "PUT"
        });

        if (!respuesta.ok) {
            throw new Error("No se pudo terminar la tarea.");
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
            body: formData
        });

        if (!respuesta.ok) {
            throw new Error("No se pudo subir el documento. Verifique que el endpoint exista en Swagger.");
        }

        mostrarMensaje("mensajeDocumentoTarea", "Documento subido correctamente.", "success");
        archivoInput.value = "";

    } catch (error) {
        mostrarMensaje("mensajeDocumentoTarea", error.message, "error");
    }
}
