// RecoverPasswordViewController.js - Controlador JS separado para recuperación y restablecimiento de contraseña
document.addEventListener("DOMContentLoaded", function () {
    const recoverForm = document.getElementById("recoverForm");
    if (recoverForm) {
        recoverForm.addEventListener("submit", function (e) {
            e.preventDefault();
            const email = document.getElementById("recEmail")?.value.trim();

            if (!email) {
                notify.warning("Por favor ingrese su correo electrónico.");
                return;
            }

            const btnSubmit = recoverForm.querySelector("button[type='submit']");
            const originalText = btnSubmit ? btnSubmit.innerHTML : "";
            if (btnSubmit) {
                btnSubmit.disabled = true;
                btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Enviando código...';
            }

            apiClient.post("Users/RecoverPassword", { email: email })
                .done(function (res) {
                    notify.success(res?.message || res?.Message || "Código de recuperación enviado a su correo electrónico.");
                    sessionStorage.setItem("sgde_reset_email", email);
                    setTimeout(function () {
                        window.location.href = "/ResetPassword";
                    }, 1500);
                })
                .fail(function (xhr) {
                    if (btnSubmit) {
                        btnSubmit.disabled = false;
                        btnSubmit.innerHTML = originalText;
                    }
                    handleApiError(xhr);
                });
        });
    }

    const resetForm = document.getElementById("resetForm");
    if (resetForm) {
        const resEmailEl = document.getElementById("resEmail");
        const savedResEmail = sessionStorage.getItem("sgde_reset_email");
        if (resEmailEl && savedResEmail && !resEmailEl.value) {
            resEmailEl.value = savedResEmail;
        }

        resetForm.addEventListener("submit", function (e) {
            e.preventDefault();
            const email = document.getElementById("resEmail")?.value.trim();
            const otpCode = document.getElementById("resOtp")?.value.trim();
            const newPassword = document.getElementById("resNewPassword")?.value;
            const confirmPassword = document.getElementById("resConfirmPassword")?.value;

            if (!email || !otpCode || !newPassword) {
                notify.warning("Por favor complete todos los campos requeridos.");
                return;
            }

            if (newPassword !== confirmPassword) {
                notify.error("Las nuevas contraseñas ingresadas no coinciden.");
                return;
            }

            const btnSubmit = resetForm.querySelector("button[type='submit']");
            const originalText = btnSubmit ? btnSubmit.innerHTML : "";
            if (btnSubmit) {
                btnSubmit.disabled = true;
                btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Restableciendo...';
            }

            apiClient.post("Users/ResetPassword", { email: email, otpCode: otpCode, newPassword: newPassword, confirmPassword: confirmPassword })
                .done(function (res) {
                    notify.success(res?.message || res?.Message || "Contraseña restablecida con éxito. Inicie sesión con sus nuevas credenciales.");
                    sessionStorage.removeItem("sgde_reset_email");
                    setTimeout(function () {
                        window.location.href = "/Login";
                    }, 1500);
                })
                .fail(function (xhr) {
                    if (btnSubmit) {
                        btnSubmit.disabled = false;
                        btnSubmit.innerHTML = originalText;
                    }
                    handleApiError(xhr);
                });
        });
    }
});