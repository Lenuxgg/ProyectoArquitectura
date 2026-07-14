(function () {
    const STORAGE_KEY = "cega_theme";

    function obtenerTemaGuardado() {
        const tema = localStorage.getItem(STORAGE_KEY);
        return tema === "dark" ? "dark" : "light";
    }

    function aplicarTema(tema) {
        const temaFinal = tema === "dark" ? "dark" : "light";
        document.documentElement.setAttribute("data-theme", temaFinal);
        localStorage.setItem(STORAGE_KEY, temaFinal);
        actualizarBotonesTema(temaFinal);
    }

    function alternarTema() {
        const temaActual = document.documentElement.getAttribute("data-theme") || obtenerTemaGuardado();
        aplicarTema(temaActual === "dark" ? "light" : "dark");
    }

    function actualizarBotonesTema(tema) {
        const esOscuro = tema === "dark";
        const botones = document.querySelectorAll("[data-theme-toggle]");

        botones.forEach((boton) => {
            boton.classList.add("theme-toggle");
            boton.dataset.themeMode = tema;
            boton.innerHTML = `
                <span class="theme-toggle__icon theme-toggle__icon--sun" aria-hidden="true">
                    <svg viewBox="0 0 24 24" focusable="false">
                        <circle cx="12" cy="12" r="4"></circle>
                        <path d="M12 2v2"></path>
                        <path d="M12 20v2"></path>
                        <path d="M4.93 4.93l1.41 1.41"></path>
                        <path d="M17.66 17.66l1.41 1.41"></path>
                        <path d="M2 12h2"></path>
                        <path d="M20 12h2"></path>
                        <path d="M4.93 19.07l1.41-1.41"></path>
                        <path d="M17.66 6.34l1.41-1.41"></path>
                    </svg>
                </span>
                <span class="theme-toggle__icon theme-toggle__icon--moon" aria-hidden="true">
                    <svg viewBox="0 0 24 24" focusable="false">
                        <path d="M21 12.8A8.5 8.5 0 1 1 11.2 3a6.7 6.7 0 0 0 9.8 9.8z"></path>
                    </svg>
                </span>
                <span class="theme-toggle__knob" aria-hidden="true"></span>
            `;
            boton.setAttribute("aria-label", esOscuro ? "Cambiar a modo claro" : "Cambiar a modo oscuro");
            boton.setAttribute("title", esOscuro ? "Cambiar a modo claro" : "Cambiar a modo oscuro");
            boton.setAttribute("aria-pressed", esOscuro ? "true" : "false");
        });
    }

    aplicarTema(obtenerTemaGuardado());

    document.addEventListener("DOMContentLoaded", () => {
        actualizarBotonesTema(document.documentElement.getAttribute("data-theme") || "light");
        document.querySelectorAll("[data-theme-toggle]").forEach((boton) => {
            if (!boton.dataset.themeReady) {
                boton.addEventListener("click", alternarTema);
                boton.dataset.themeReady = "true";
            }
        });
    });
})();
