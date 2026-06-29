async function cargarProyectos() {
    const tabla = document.getElementById("tablaProyectos");

    try {
        const respuesta = await fetch(`${API_BASE}/Proyectos`);
        const proyectos = await respuesta.json();

        if (!respuesta.ok) {
            throw new Error("No se pudieron cargar los proyectos.");
        }

        if (proyectos.length === 0) {
            tabla.innerHTML = `<tr><td colspan="7">No hay proyectos registrados.</td></tr>`;
            return;
        }

        tabla.innerHTML = "";

        proyectos.forEach(p => {
            tabla.innerHTML += `
                <tr>
                    <td>${p.id}</td>
                    <td>${p.nombre}</td>
                    <td>${p.descripcion ?? ""}</td>
                    <td>${formatearFecha(p.fechaInicio)}</td>
                    <td>${p.fechaFin ? formatearFecha(p.fechaFin) : ""}</td>
                    <td>${p.estado}</td>
                    <td>
                        <a class="btn-primary" href="detalle-proyecto.html?id=${p.id}">Ver Proyecto</a>
                        <a class="btn-warning" href="editar-proyecto.html?id=${p.id}">Editar</a>
                    </td>
                </tr>
            `;
        });

    } catch (error) {
        tabla.innerHTML = `<tr><td colspan="7">Error: ${error.message}</td></tr>`;
    }
}

async function crearProyecto() {
    const nombre = document.getElementById("nombre").value.trim();
    const descripcion = document.getElementById("descripcion").value.trim();
    const fechaInicio = document.getElementById("fechaInicio").value;

    if (!nombre || !fechaInicio) {
        mostrarMensaje("mensajeProyecto", "Debe ingresar nombre y fecha de inicio.", "error");
        return;
    }

    const proyecto = {
        nombre,
        descripcion,
        fechaInicio
    };

    try {
        const respuesta = await fetch(`${API_BASE}/Proyectos`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(proyecto)
        });

        if (!respuesta.ok) {
            throw new Error("No se pudo crear el proyecto.");
        }

        mostrarMensaje("mensajeProyecto", "Proyecto creado correctamente.", "success");

        setTimeout(() => {
            window.location.href = "proyectos.html";
        }, 800);

    } catch (error) {
        mostrarMensaje("mensajeProyecto", error.message, "error");
    }
}

async function cargarProyectoParaEditar() {
    const id = obtenerParametro("id");

    if (!id) {
        mostrarMensaje("mensajeProyecto", "No se recibió el ID del proyecto.", "error");
        return;
    }

    try {
        const respuesta = await fetch(`${API_BASE}/Proyectos/${id}`);
        const p = await respuesta.json();

        if (!respuesta.ok) {
            throw new Error("No se encontró el proyecto.");
        }

        document.getElementById("proyectoId").value = p.id;
        document.getElementById("nombre").value = p.nombre;
        document.getElementById("descripcion").value = p.descripcion ?? "";
        document.getElementById("fechaInicio").value = convertirFechaInput(p.fechaInicio);
        document.getElementById("fechaFin").value = p.fechaFin ? convertirFechaInput(p.fechaFin) : "";
        document.getElementById("estado").value = p.estado;

    } catch (error) {
        mostrarMensaje("mensajeProyecto", error.message, "error");
    }
}

async function editarProyecto() {
    const id = document.getElementById("proyectoId").value;

    const proyecto = {
        nombre: document.getElementById("nombre").value.trim(),
        descripcion: document.getElementById("descripcion").value.trim(),
        fechaInicio: document.getElementById("fechaInicio").value,
        fechaFin: document.getElementById("fechaFin").value || null,
        estado: document.getElementById("estado").value
    };

    if (!proyecto.nombre || !proyecto.fechaInicio) {
        mostrarMensaje("mensajeProyecto", "Debe ingresar nombre y fecha de inicio.", "error");
        return;
    }

    try {
        const respuesta = await fetch(`${API_BASE}/Proyectos/${id}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(proyecto)
        });

        if (!respuesta.ok) {
            throw new Error("No se pudo editar el proyecto.");
        }

        mostrarMensaje("mensajeProyecto", "Proyecto actualizado correctamente.", "success");

        setTimeout(() => {
            window.location.href = `detalle-proyecto.html?id=${id}`;
        }, 800);

    } catch (error) {
        mostrarMensaje("mensajeProyecto", error.message, "error");
    }
}

async function cargarDetalleProyecto() {
    const id = obtenerParametro("id");

    if (!id) {
        alert("No se recibió el ID del proyecto.");
        window.location.href = "proyectos.html";
        return;
    }

    try {
        const respuesta = await fetch(`${API_BASE}/Proyectos/${id}`);
        const p = await respuesta.json();

        if (!respuesta.ok) {
            throw new Error("No se encontró el proyecto.");
        }

        document.getElementById("tituloProyecto").textContent = p.nombre;
        document.getElementById("detalleId").textContent = p.id;
        document.getElementById("detalleDescripcion").textContent = p.descripcion ?? "";
        document.getElementById("detalleEstado").textContent = p.estado;
        document.getElementById("detalleFechaInicio").textContent = formatearFecha(p.fechaInicio);
        document.getElementById("detalleFechaFin").textContent = p.fechaFin ? formatearFecha(p.fechaFin) : "Sin fecha final";

        document.getElementById("btnEditarProyecto").href = `editar-proyecto.html?id=${p.id}`;
        document.getElementById("btnCrearTarea").href = `crear-tarea.html?proyectoId=${p.id}`;

        cargarDocumentosProyecto(p.id);
        cargarTareasDelProyecto(p.id);

    } catch (error) {
        alert(error.message);
        window.location.href = "proyectos.html";
    }
}

async function subirDocumentoProyecto() {
    const proyectoId = obtenerParametro("id");
    const archivoInput = document.getElementById("archivoProyecto");

    if (!archivoInput.files || archivoInput.files.length === 0) {
        mostrarMensaje("mensajeDocumento", "Debe seleccionar un archivo.", "error");
        return;
    }

    const formData = new FormData();
    formData.append("archivo", archivoInput.files[0]);

    try {
        const respuesta = await fetch(`${API_BASE}/Proyectos/${proyectoId}/documentos`, {
            method: "POST",
            body: formData
        });

        if (!respuesta.ok) {
            throw new Error("No se pudo subir el documento.");
        }

        mostrarMensaje("mensajeDocumento", "Documento subido correctamente.", "success");
        archivoInput.value = "";

        cargarDocumentosProyecto(proyectoId);

    } catch (error) {
        mostrarMensaje("mensajeDocumento", error.message, "error");
    }
}

async function cargarDocumentosProyecto(proyectoId) {
    const tabla = document.getElementById("tablaDocumentos");

    try {
        const respuesta = await fetch(`${API_BASE}/Proyectos/${proyectoId}/documentos`);
        const documentos = await respuesta.json();

        if (!respuesta.ok) {
            throw new Error("No se pudieron cargar los documentos.");
        }

        if (documentos.length === 0) {
            tabla.innerHTML = `<tr><td colspan="3">No hay documentos registrados.</td></tr>`;
            return;
        }

        tabla.innerHTML = "";

        documentos.forEach(d => {
            tabla.innerHTML += `
                <tr>
                    <td>${d.nombre}</td>
                    <td><a href="${d.rutaArchivo}" target="_blank">Abrir</a></td>
                    <td>${formatearFecha(d.fechaCarga)}</td>
                </tr>
            `;
        });

    } catch (error) {
        tabla.innerHTML = `<tr><td colspan="3">Error: ${error.message}</td></tr>`;
    }
}

function convertirFechaInput(fecha) {
    if (!fecha) return "";
    return fecha.substring(0, 10);
}
