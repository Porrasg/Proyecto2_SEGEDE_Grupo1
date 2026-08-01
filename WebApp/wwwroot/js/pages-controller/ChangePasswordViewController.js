// ChangePasswordViewController.js - Cambio de contraseña CON sesión activa.
// Distinto de RecoverPassword/ResetPassword (esos sí son para "olvidé mi
// contraseña", sin sesión, con OTP). Aquí el usuario ya está autenticado:
// se verifica identidad con la contraseña actual, sin OTP adicional.
document.addEventListener("DOMContentLoaded", function () {
    // Guard de sesión: esta pantalla ya no es pública.
    if (!session.isAuthenticated() || session.isExpired()) {
        window.location.href = "/Login?returnUrl=" + encodeURIComponent("/ChangePassword");
        return;
    }

    const changePassForm = document.getElementById("changePassForm");
    if (!changePassForm) return;

    const currentPassInput = document.getElementById("chgCurrentPassword");
    const newPassInput = document.getElementById("chgNewPassword");
    const confirmPassInput = document.getElementById("chgConfirmPassword");

    changePassForm.addEventListener("submit", function (e) {
        e.preventDefault();

        const currentPassword = currentPassInput?.value || "";
        const newPassword = newPassInput?.value || "";
        const confirmPassword = confirmPassInput?.value || "";

        if (!currentPassword || !newPassword || !confirmPassword) {
            notify.warning("Complete todos los campos.");
            return;
        }

        const btnSubmit = changePassForm.querySelector("button[type='submit']");
        const originalText = btnSubmit ? btnSubmit.innerHTML : "";
        if (btnSubmit) {
            btnSubmit.disabled = true;
            btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Guardando...';
        }

        apiClient.post("Users/ChangePassword", {
            userId: session.getUserId(),
            currentPassword: currentPassword,
            newPassword: newPassword,
            confirmPassword: confirmPassword
        })
            .done(function (res) {
                notify.success(res?.message || res?.Message || "Contraseña actualizada correctamente.");
                changePassForm.reset();
            })
            .fail(function (xhr) {
                handleApiError(xhr);
            })
            .always(function () {
                if (btnSubmit) {
                    btnSubmit.disabled = false;
                    btnSubmit.innerHTML = originalText;
                }
            });
    });
});

// CONTROL DEL OJITO PARA VER CONTRASEÑA (REUTILIZABLE)
function togglePassword(inputId, buttonElement) {
    const passwordInput = document.getElementById(inputId);

    if (passwordInput) {
        const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
        passwordInput.setAttribute('type', type);

        const icon = buttonElement.querySelector('i');
        if (icon) {
            icon.classList.toggle('bi-eye');
            icon.classList.toggle('bi-eye-slash');
        }
    }
};
