let dashboardCharts = [];

function obtenerHeadersDashboard() {
    if (typeof obtenerHeadersAuth === "function") {
        return obtenerHeadersAuth();
    }

    return {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("cega_token")}`
    };
}

async function apiGetDashboard(ruta) {
    const respuesta = await fetch(`${API_BASE}${ruta}`, {
        headers: obtenerHeadersDashboard()
    });

    if (!respuesta.ok) {
        const texto = await respuesta.text();
        throw new Error(texto || "No se pudo consultar la información del dashboard.");
    }

    return await respuesta.json();
}

function textoProp(objeto, ...nombres) {
    for (const nombre of nombres) {
        if (objeto && objeto[nombre] !== undefined && objeto[nombre] !== null) {
            return objeto[nombre];
        }
    }
    return "";
}

function numeroProp(objeto, ...nombres) {
    const valor = textoProp(objeto, ...nombres);
    const numero = Number(valor);
    return Number.isFinite(numero) ? numero : 0;
}

function formatearMonedaDashboard(valor) {
    return new Intl.NumberFormat("es-CR", {
        style: "currency",
        currency: "CRC",
        maximumFractionDigits: 0
    }).format(Number(valor) || 0);
}

function formatearFechaDashboard(fecha) {
    if (!fecha) return "-";
    try {
        return new Date(fecha).toLocaleDateString("es-CR");
    } catch {
        return "-";
    }
}

function actualizarTextoDashboard(id, valor) {
    const elemento = document.getElementById(id);
    if (elemento) elemento.textContent = valor;
}

function obtenerColorChart(nombre) {
    const estilos = getComputedStyle(document.documentElement);
    const mapa = {
        accent: estilos.getPropertyValue("--color-accent").trim() || "#E8CA4E",
        success: estilos.getPropertyValue("--color-success").trim() || "#16A34A",
        danger: estilos.getPropertyValue("--color-danger").trim() || "#DC2626",
        text: estilos.getPropertyValue("--color-text").trim() || "#111827",
        border: estilos.getPropertyValue("--color-border").trim() || "#D1D5DB",
        surface: estilos.getPropertyValue("--color-surface").trim() || "#FFFFFF"
    };
    return mapa[nombre] || mapa.accent;
}

function destruirGraficosDashboard() {
    dashboardCharts.forEach(chart => chart.destroy());
    dashboardCharts = [];
}

function opcionesChartBase() {
    return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                labels: {
                    color: obtenerColorChart("text"),
                    boxWidth: 12,
                    usePointStyle: true
                }
            }
        },
        scales: {
            x: {
                ticks: { color: obtenerColorChart("text") },
                grid: { color: "rgba(148, 163, 184, 0.16)" }
            },
            y: {
                ticks: { color: obtenerColorChart("text") },
                grid: { color: "rgba(148, 163, 184, 0.16)" }
            }
        }
    };
}


function crearLeyendaConChecks(chart, idContenedor) {
    const contenedor = document.getElementById(idContenedor);

    if (!contenedor || !chart) {
        return;
    }

    contenedor.innerHTML = "";

    chart.data.datasets.forEach((dataset, index) => {
        const boton = document.createElement("button");
        boton.type = "button";
        boton.className = "chart-toggle-item is-active";
        boton.setAttribute("aria-pressed", "true");

        boton.innerHTML = `
            <span class="chart-toggle-check" style="--legend-color: ${dataset.borderColor}">
                <svg viewBox="0 0 24 24" aria-hidden="true">
                    <path d="M5 12.5l4.2 4.2L19 7"></path>
                </svg>
            </span>
            <span>${dataset.label}</span>
        `;

        boton.addEventListener("click", () => {
            const visible = chart.isDatasetVisible(index);

            chart.setDatasetVisibility(index, !visible);
            chart.update();

            boton.classList.toggle("is-active", !visible);
            boton.setAttribute("aria-pressed", String(!visible));
        });

        contenedor.appendChild(boton);
    });
}


function crearGraficoLinea(idCanvas, labels, ingresos, egresos) {
    const canvas = document.getElementById(idCanvas);
    if (!canvas || !window.Chart) return;

    const chart = new Chart(canvas, {
        type: "line",
        data: {
            labels,
            datasets: [
                {
                    label: "Ingresos",
                    data: ingresos,
                    borderColor: obtenerColorChart("success"),
                    backgroundColor: "rgba(22, 163, 74, 0.12)",
                    tension: 0.35,
                    fill: true
                },
                {
                    label: "Egresos",
                    data: egresos,
                    borderColor: obtenerColorChart("danger"),
                    backgroundColor: "rgba(220, 38, 38, 0.10)",
                    tension: 0.35,
                    fill: true
                }
            ]
        },
        options: {
            ...opcionesChartBase(),
            plugins: {
                ...opcionesChartBase().plugins,
                legend: {
                    display: false
                }
            }
        }
    });

    crearLeyendaConChecks(chart, "legendIngresosEgresos");
    dashboardCharts.push(chart);
}

function crearGraficoDona(idCanvas, labels, datos) {
    const canvas = document.getElementById(idCanvas);
    if (!canvas || !window.Chart) return;

    const chart = new Chart(canvas, {
        type: "doughnut",
        data: {
            labels,
            datasets: [{
                data: datos,
                backgroundColor: [
                    obtenerColorChart("accent"),
                    "#3B82F6",
                    "#22C55E",
                    "#EF4444",
                    "#A855F7",
                    "#14B8A6"
                ],
                borderColor: obtenerColorChart("surface"),
                borderWidth: 3
            }]
        },
        options: {
            ...opcionesChartBase(),
            cutout: "68%",
            scales: {}
        }
    });

    dashboardCharts.push(chart);
}

async function inicializarDashboardAnalitico() {
    validarSesion();

    const esAdmin = usuarioEsAdmin();
    const usuarioId = obtenerUsuarioIdActual();

    actualizarTextoDashboard(
        "dashboardSubtitulo",
        esAdmin
            ? "Resumen ejecutivo global de proyectos, tareas y movimientos financieros."
            : "Resumen ejecutivo de sus proyectos asignados y movimientos financieros relacionados."
    );

    try {
        const [proyectos, transacciones] = await Promise.all([
            cargarProyectosDashboard(esAdmin, usuarioId),
            cargarTransaccionesDashboard()
        ]);

        const tareas = await cargarTareasDashboard(proyectos);
        renderDashboard(proyectos, tareas, transacciones);

    } catch (error) {
        console.error(error);
        actualizarTextoDashboard("kpiProyectosTexto", "No se pudo cargar el dashboard.");
    }
}

async function cargarProyectosDashboard(esAdmin, usuarioId) {
    const ruta = esAdmin ? "/Proyectos" : `/Proyectos/usuario/${usuarioId}`;
    return await apiGetDashboard(ruta);
}

async function cargarTransaccionesDashboard() {
    try {
        return await apiGetDashboard("/Contabilidad");
    } catch (error) {
        console.error("No se pudieron cargar transacciones.", error);
        return [];
    }
}

async function cargarTareasDashboard(proyectos) {
    const tareas = [];

    for (const proyecto of proyectos || []) {
        const proyectoId = numeroProp(proyecto, "id", "Id");
        if (!proyectoId) continue;

        try {
            const tareasProyecto = await apiGetDashboard(`/Tareas/proyecto/${proyectoId}`);
            (tareasProyecto || []).forEach(t => {
                tareas.push({
                    ...t,
                    proyectoNombre: textoProp(proyecto, "nombre", "Nombre")
                });
            });
        } catch (error) {
            console.warn("No se pudieron cargar tareas del proyecto", proyectoId, error);
        }
    }

    return tareas;
}

function renderDashboard(proyectos, tareas, transacciones) {
    destruirGraficosDashboard();
    renderKpis(proyectos, tareas, transacciones);
    renderTablaProyectos(proyectos);
    renderMovimientos(transacciones);
    renderEstadosTareas(tareas);
    renderGraficos(proyectos, tareas, transacciones);
}

function renderKpis(proyectos, tareas, transacciones) {
    const proyectosActivos = (proyectos || []).filter(p => {
        const estado = String(textoProp(p, "estado", "Estado")).toLowerCase();
        return estado !== "terminado" && estado !== "cancelado" && estado !== "finalizado";
    }).length;

    const tareasPendientes = (tareas || []).filter(t => {
        const estado = String(textoProp(t, "estado", "Estado")).toLowerCase();
        return estado !== "terminada" && estado !== "completada" && estado !== "cancelada";
    }).length;

    const ingresos = totalPorTipo(transacciones, "Ingreso");
    const egresos = totalPorTipo(transacciones, "Egreso");
    const balance = ingresos - egresos;

    actualizarTextoDashboard("kpiProyectosActivos", proyectosActivos);
    actualizarTextoDashboard("kpiProyectosTexto", `${(proyectos || []).length} proyectos visibles`);
    actualizarTextoDashboard("kpiTareasPendientes", tareasPendientes);
    actualizarTextoDashboard("heroTareasPendientes", `${tareasPendientes} tareas pendientes`);
    actualizarTextoDashboard("kpiTareasTexto", `${(tareas || []).length} tareas consultadas`);
    actualizarTextoDashboard("kpiIngresos", formatearMonedaDashboard(ingresos));
    actualizarTextoDashboard("kpiEgresos", formatearMonedaDashboard(egresos));
    actualizarTextoDashboard("kpiBalance", formatearMonedaDashboard(balance));
    actualizarTextoDashboard("kpiBalanceTexto", balance >= 0 ? "Resultado positivo" : "Resultado negativo");
}

function totalPorTipo(transacciones, tipoEsperado) {
    return (transacciones || []).reduce((total, t) => {
        const tipo = textoProp(t, "tipo", "Tipo");
        if (tipo !== tipoEsperado) return total;
        return total + numeroProp(t, "monto", "Monto");
    }, 0);
}

function renderTablaProyectos(proyectos) {
    const tabla = document.getElementById("tablaDashboardProyectos");
    if (!tabla) return;

    if (!proyectos || proyectos.length === 0) {
        tabla.innerHTML = `<tr><td colspan="4">No hay proyectos visibles.</td></tr>`;
        return;
    }

    tabla.innerHTML = "";

    proyectos.slice(0, 6).forEach(p => {
        tabla.innerHTML += `
            <tr>
                <td>
                    <strong>${textoProp(p, "nombre", "Nombre")}</strong>
                    <small>ID ${textoProp(p, "id", "Id")}</small>
                </td>
                <td><span class="analytics-status">${textoProp(p, "estado", "Estado") || "Sin estado"}</span></td>
                <td>${formatearFechaDashboard(textoProp(p, "fechaInicio", "FechaInicio"))}</td>
                <td>${formatearFechaDashboard(textoProp(p, "fechaFin", "FechaFin"))}</td>
            </tr>
        `;
    });
}

function renderMovimientos(transacciones) {
    const contenedor = document.getElementById("listaMovimientosRecientes");
    if (!contenedor) return;

    if (!transacciones || transacciones.length === 0) {
        contenedor.innerHTML = `<p>No hay movimientos registrados.</p>`;
        return;
    }

    const ordenadas = [...transacciones].sort((a, b) => {
        return new Date(textoProp(b, "fecha", "Fecha")) - new Date(textoProp(a, "fecha", "Fecha"));
    });

    contenedor.innerHTML = "";

    ordenadas.slice(0, 6).forEach(t => {
        const tipo = textoProp(t, "tipo", "Tipo");
        const clase = tipo === "Ingreso" ? "activity-income" : "activity-expense";

        contenedor.innerHTML += `
            <div class="activity-item">
                <span class="activity-dot ${clase}"></span>
                <div>
                    <strong>${tipo}</strong>
                    <p>${textoProp(t, "descripcion", "Descripcion") || textoProp(t, "categoria", "Categoria") || "Movimiento contable"}</p>
                    <small>${textoProp(t, "proyectoNombre", "ProyectoNombre") || "Sin proyecto"} · ${formatearFechaDashboard(textoProp(t, "fecha", "Fecha"))}</small>
                </div>
                <b>${formatearMonedaDashboard(numeroProp(t, "monto", "Monto"))}</b>
            </div>
        `;
    });
}

function renderEstadosTareas(tareas) {
    const contenedor = document.getElementById("listaEstadosTareas");
    if (!contenedor) return;

    const grupos = agruparPor(tareas || [], t => textoProp(t, "estado", "Estado") || "Sin estado");

    if (Object.keys(grupos).length === 0) {
        contenedor.innerHTML = `<p>No hay tareas disponibles.</p>`;
        return;
    }

    contenedor.innerHTML = "";

    Object.entries(grupos).forEach(([estado, lista]) => {
        contenedor.innerHTML += `
            <div class="status-row">
                <span>${estado}</span>
                <strong>${lista.length}</strong>
            </div>
        `;
    });
}

function renderGraficos(proyectos, tareas, transacciones) {
    const serie = construirSerieMensual(transacciones || []);
    crearGraficoLinea("chartIngresosEgresos", serie.labels, serie.ingresos, serie.egresos);

    const gastos = agruparMontosPorCategoria((transacciones || []).filter(t => textoProp(t, "tipo", "Tipo") === "Egreso"));
    crearGraficoDona(
        "chartGastos",
        Object.keys(gastos).length ? Object.keys(gastos) : ["Sin egresos"],
        Object.keys(gastos).length ? Object.values(gastos) : [1]
    );

    const estadosProyecto = agruparPor(proyectos || [], p => textoProp(p, "estado", "Estado") || "Sin estado");
    crearGraficoDona(
        "chartProyectosEstado",
        Object.keys(estadosProyecto).length ? Object.keys(estadosProyecto) : ["Sin proyectos"],
        Object.keys(estadosProyecto).length ? Object.values(estadosProyecto).map(x => x.length) : [1]
    );
}

function construirSerieMensual(transacciones) {
    const meses = {};

    (transacciones || []).forEach(t => {
        const fecha = new Date(textoProp(t, "fecha", "Fecha"));
        if (Number.isNaN(fecha.getTime())) return;

        const clave = fecha.toLocaleDateString("es-CR", { month: "short", year: "2-digit" });

        if (!meses[clave]) {
            meses[clave] = {
                ingresos: 0,
                egresos: 0,
                orden: fecha.getFullYear() * 100 + fecha.getMonth()
            };
        }

        const tipo = textoProp(t, "tipo", "Tipo");
        const monto = numeroProp(t, "monto", "Monto");

        if (tipo === "Ingreso") meses[clave].ingresos += monto;
        if (tipo === "Egreso") meses[clave].egresos += monto;
    });

    const entradas = Object.entries(meses).sort((a, b) => a[1].orden - b[1].orden).slice(-6);

    if (entradas.length === 0) {
        return { labels: ["Sin datos"], ingresos: [0], egresos: [0] };
    }

    return {
        labels: entradas.map(([label]) => label),
        ingresos: entradas.map(([, valor]) => valor.ingresos),
        egresos: entradas.map(([, valor]) => valor.egresos)
    };
}

function agruparMontosPorCategoria(transacciones) {
    const grupos = {};

    (transacciones || []).forEach(t => {
        const categoria = textoProp(t, "categoria", "Categoria") || "Sin categoría";
        grupos[categoria] = (grupos[categoria] || 0) + numeroProp(t, "monto", "Monto");
    });

    return grupos;
}

function agruparPor(lista, selector) {
    return (lista || []).reduce((grupos, item) => {
        const clave = selector(item);
        grupos[clave] = grupos[clave] || [];
        grupos[clave].push(item);
        return grupos;
    }, {});
}
