// notifications.js (§24.3, §29.3) - Gestión de alertas y confirmaciones con SweetAlert2, consciente del tema claro/oscuro
function sgdeIsDark() {
    return document.documentElement.getAttribute("data-bs-theme") === "dark";
}

// Toast no bloqueante en la esquina superior derecha, se autocierra (Enmienda 4 — nunca congela la pantalla).
const sgdeToastMixin = function () {
    return Swal.mixin({
        toast: true,
        position: "top-end",
        showConfirmButton: false,
        timer: 4000,
        timerProgressBar: true,
        customClass: { popup: sgdeIsDark() ? "sgde-swal-dark" : "sgde-swal-light" },
        didOpen: (el) => {
            el.addEventListener("mouseenter", Swal.stopTimer);
            el.addEventListener("mouseleave", Swal.resumeTimer);
        }
    });
};

const Notifications = {
    // Compatibilidad con el nombre anterior (§29.3) — ahora respaldado por SweetAlert2.
    showToast: function (message, type = "info") {
        const icon = type === "error" ? "error" : type === "success" ? "success" : type === "warning" ? "warning" : "info";
        sgdeToastMixin().fire({ icon: icon, title: message });
    }
};

const notify = {
    success: m => Notifications.showToast(m, "success"),
    error: m => Notifications.showToast(m, "error"),
    warning: m => Notifications.showToast(m, "warning"),
    info: m => Notifications.showToast(m, "info"),

    // Reemplaza al confirm() nativo del navegador (Enmienda 3: prohibido usar confirm()/alert() nativos).
    // Retorna una Promise<boolean> — uso: notify.confirm("¿Seguro?").then(ok => { if (!ok) return; ... }).
    confirm: function (message, options = {}) {
        return Swal.fire({
            icon: options.icon || "warning",
            title: options.title || "Confirmar acción",
            text: message,
            showCancelButton: true,
            confirmButtonText: options.confirmText || "Confirmar",
            cancelButtonText: options.cancelText || "Cancelar",
            confirmButtonColor: options.dangerous ? "var(--sgde-state-damaged)" : "var(--sgde-primary)",
            reverseButtons: true,
            customClass: { popup: sgdeIsDark() ? "sgde-swal-dark" : "sgde-swal-light" }
        }).then(function (result) {
            return result.isConfirmed;
        });
    }
};

// Evita saturar la pantalla de toasts idénticos cuando varias llamadas fallan casi al mismo tiempo
// (p. ej. un dashboard que dispara 4-5 peticiones en paralelo contra un backend caído).
let sgdeLastErrorMsg = null;
let sgdeLastErrorAt = 0;
function sgdeShowThrottledError(message) {
    const now = Date.now();
    if (message === sgdeLastErrorMsg && (now - sgdeLastErrorAt) < 2000) return;
    sgdeLastErrorMsg = message;
    sgdeLastErrorAt = now;
    notify.error(message);
}

// Manejador global estándar para errores de respuestas AJAX / API (§24.3)
// Además de notificar, degrada con gracia los endpoints que el backend aún no expone:
// mientras WebAPI no tenga los controladores/DTOs implementados, toda llamada responde
// 404 (ruta inexistente) o 0 (servidor caído) — en ambos casos se informa al usuario sin
// romper la navegación. La ruta que falló ya queda registrada en consola por apiClient.js.
function handleApiError(xhr) {
    const res = xhr.responseJSON || (xhr.responseText ? (function () { try { return JSON.parse(xhr.responseText); } catch (e) { return null; } })() : null);

    if (xhr.status === 401) {
        window.session?.clear();
        window.location = '/Login';
        return;
    }
    if (xhr.status === 403) {
        sgdeShowThrottledError('No tiene permisos para realizar esta acción.');
        return;
    }
    if (xhr.status === 0) {
        sgdeShowThrottledError('No se pudo conectar con el servidor. Verifique su conexión o que el servicio esté disponible.');
        return;
    }
    if (xhr.status === 404 || xhr.status === 501) {
        sgdeShowThrottledError('Este módulo está en construcción: la funcionalidad todavía no está disponible en el servidor.');
        return;
    }
    if (res?.errors?.length) {
        sgdeShowThrottledError(res.errors.join(' '));
    } else {
        sgdeShowThrottledError(res?.message ?? res?.Message ?? 'Ocurrió un error inesperado al procesar la solicitud.');
    }
}

// Exponer en el alcance global del navegador
window.Notifications = Notifications;
window.notify = notify;
window.handleApiError = handleApiError;
