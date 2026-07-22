// theme.js (§29.4) - Gestión de modo oscuro / claro (monochrome alto contraste)
document.addEventListener("DOMContentLoaded", function () {
    const btnToggle = document.getElementById("btnThemeToggle");
    
    // Cargar tema persistido en localStorage o preferido por sistema
    let currentTheme = localStorage.getItem("sgde_theme") || "light";
    applyTheme(currentTheme);

    if (btnToggle) {
        btnToggle.addEventListener("click", function () {
            currentTheme = currentTheme === "light" ? "dark" : "light";
            applyTheme(currentTheme);
            localStorage.setItem("sgde_theme", currentTheme);
        });
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute("data-bs-theme", theme);
        if (btnToggle) {
            const icon = btnToggle.querySelector("i");
            if (icon) {
                icon.className = theme === "dark"
                    ? "bi bi-sun-fill fs-6"
                    : "bi bi-moon-stars-fill fs-6";
            }
            btnToggle.title = theme === "dark" ? "Cambiar a modo claro" : "Cambiar a modo oscuro";
        }
        applyChartJsTheme(theme);
    }

    // Chart.js no hereda el tema de Bootstrap: sus textos de leyenda/ejes usan un gris casi
    // negro por defecto, ilegible sobre las cards oscuras del modo oscuro. Se fija el color
    // global aquí y se refrescan los gráficos ya creados (los dashboards los crean después
    // de este listener, así que además quedan bien desde la primera carga).
    function applyChartJsTheme(theme) {
        if (typeof Chart === "undefined") return;
        const styles = getComputedStyle(document.documentElement);
        const textColor = styles.getPropertyValue("--sgde-body-secondary").trim() || (theme === "dark" ? "#A3A1AC" : "#5C5A66");
        const gridColor = styles.getPropertyValue("--sgde-border").trim() || (theme === "dark" ? "#383546" : "#DDDCE4");

        Chart.defaults.color = textColor;
        Chart.defaults.borderColor = gridColor;

        Object.values(Chart.instances || {}).forEach(function (chart) {
            chart.update();
        });
    }
});
