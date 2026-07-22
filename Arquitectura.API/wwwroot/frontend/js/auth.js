const AUTH_API_BASE = window.location.origin + "/api";

function guardarSesion(data) {
    localStorage.setItem("cega_token", data.token);
    localStorage.setItem("cega_usuario_id", data.usuarioId);
    localStorage.setItem("cega_nombre", data.nombre);
    localStorage.setItem("cega_email", data.email);
    localStorage.setItem("cega_rol", data.rol);
    localStorage.setItem("cega_es_admin", data.esAdministrador ? "true" : "false");
}

function obtenerSesion() {
    return {
        token: localStorage.getItem("cega_token"),
        usuarioId: parseInt(localStorage.getItem("cega_usuario_id") || "0"),
        nombre: localStorage.getItem("cega_nombre"),
        email: localStorage.getItem("cega_email"),
        rol: localStorage.getItem("cega_rol"),
        esAdmin: localStorage.getItem("cega_es_admin") === "true"
    };
}

function cerrarSesion() {
    localStorage.removeItem("cega_token");
    localStorage.removeItem("cega_usuario_id");
    localStorage.removeItem("cega_nombre");
    localStorage.removeItem("cega_email");
    localStorage.removeItem("cega_rol");
    localStorage.removeItem("cega_es_admin");

    window.location.href = "/frontend/login.html";
}

function validarSesion() {
    const sesion = obtenerSesion();

    if (!sesion.token || !sesion.usuarioId) {
        window.location.href = "/frontend/login.html";
        return false;
    }

    return true;
}

function usuarioEsAdmin() {
    return localStorage.getItem("cega_es_admin") === "true";
}

function obtenerUsuarioIdActual() {
    return parseInt(localStorage.getItem("cega_usuario_id") || "0");
}

function obtenerHeadersAuth() {
    const token = localStorage.getItem("cega_token");

    return {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`
    };
}

async function iniciarSesion() {
    const email = document.getElementById("email").value.trim();
    const password = document.getElementById("password").value.trim();
    const mensaje = document.getElementById("mensajeLogin");

    if (!email || !password) {
        mensaje.textContent = "Debe ingresar correo y contraseña.";
        mensaje.className = "message error";
        return;
    }

    try {
        const respuesta = await fetch(`${AUTH_API_BASE}/Auth/login`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                email: email,
                password: password
            })
        });

        if (!respuesta.ok) {
            throw new Error("Credenciales inválidas.");
        }

        const data = await respuesta.json();

        guardarSesion(data);

        window.location.href = "/frontend/index.html";

    } catch (error) {
        mensaje.textContent = error.message;
        mensaje.className = "message error";
    }
}

function pintarUsuarioEnNavbar() {
    const sesion = obtenerSesion();
    const contenedor = document.getElementById("usuarioSesion");

    if (!contenedor) return;

    contenedor.innerHTML = `
        <span><strong>${sesion.nombre}</strong> | ${sesion.rol}</span>
        <button class="btn-danger" onclick="cerrarSesion()">Cerrar sesión</button>
    `;
}
