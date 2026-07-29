// ChangePasswordViewController.js - Cambio de contraseña sin sesión, con OTP de confirmación
document.addEventListener("DOMContentLoaded", function () {
    const changePassForm = document.getElementById("changePassForm");
    const changePassOtpForm = document.getElementById("changePassOtpForm");
    if (!changePassForm || !changePassOtpForm) return;

    const changeOtpExpiryKey = "sgde_change_otp_expires_at";
    const pendingChangeKey = "sgde_change_password_pending";
    const changeCountdownContainer = document.getElementById("chgCountdownContainer");
    const changeCountdownEl = document.getElementById("chgCountdown");
    const changeOtpInput = document.getElementById("chgOtpCode");
    const resendChangeOtpBtn = document.getElementById("resendChangeOtpBtn");
    const emailInput = document.getElementById("chgEmail");
    const currentPassInput = document.getElementById("chgCurrentPassword");
    const newPassInput = document.getElementById("chgNewPassword");
    const confirmPassInput = document.getElementById("chgConfirmPassword");

    function setChangeOtpExpiry(minutes = 3) {
        const expiresAt = Date.now() + (minutes * 60 * 1000);
        sessionStorage.setItem(changeOtpExpiryKey, String(expiresAt));
        return expiresAt;
    }

    function savePendingChange(payload) {
        sessionStorage.setItem(pendingChangeKey, JSON.stringify(payload));
    }

    function getPendingChange() {
        const raw = sessionStorage.getItem(pendingChangeKey);
        if (!raw) return null;
        try { return JSON.parse(raw); } catch { return null; }
    }

    function clearPendingChange() {
        sessionStorage.removeItem(pendingChangeKey);
        sessionStorage.removeItem(changeOtpExpiryKey);
    }

    function startChangeCountdown() {
        const stored = Number(sessionStorage.getItem(changeOtpExpiryKey) || 0);
        const expiry = stored > 0 ? stored : setChangeOtpExpiry(3);

        const update = function () {
            const remaining = Math.max(0, expiry - Date.now());
            const totalSeconds = Math.floor(remaining / 1000);
            const minutes = String(Math.floor(totalSeconds / 60)).padStart(2, "0");
            const seconds = String(totalSeconds % 60).padStart(2, "0");

            if (changeCountdownEl) changeCountdownEl.textContent = `${minutes}:${seconds}`;
            if (changeCountdownContainer) changeCountdownContainer.classList.remove("d-none");

            if (remaining <= 0) {
                if (changeOtpInput) changeOtpInput.disabled = true;
                if (resendChangeOtpBtn) resendChangeOtpBtn.disabled = false;
                clearInterval(window.sgdeChangeOtpTimer);
                notify.warning("El código OTP para el cambio de contraseña expiró. Reenvíe uno nuevo.");
            } else {
                if (changeOtpInput) changeOtpInput.disabled = false;
            }
        };

        update();
        clearInterval(window.sgdeChangeOtpTimer);
        window.sgdeChangeOtpTimer = setInterval(update, 1000);
    }

    const pending = getPendingChange();
    if (pending) {
        changePassForm.classList.add("d-none");
        changePassOtpForm.classList.remove("d-none");
        if (emailInput && pending.email) emailInput.value = pending.email;
        if (currentPassInput && pending.currentPassword) currentPassInput.value = pending.currentPassword;
        if (newPassInput && pending.newPassword) newPassInput.value = pending.newPassword;
        if (confirmPassInput && pending.confirmPassword) confirmPassInput.value = pending.confirmPassword;
        startChangeCountdown();
    }

    changePassForm.addEventListener("submit", function (e) {
        e.preventDefault();

        const email = emailInput?.value.trim();
        const currentPassword = currentPassInput?.value || "";
        const newPassword = newPassInput?.value || "";
        const confirmPassword = confirmPassInput?.value || "";

        if (!email || !currentPassword || !newPassword || !confirmPassword) {
            notify.warning("Complete todos los campos.");
            return;
        }

        const btnSubmit = changePassForm.querySelector("button[type='submit']");
        const originalText = btnSubmit ? btnSubmit.innerHTML : "";
        if (btnSubmit) {
            btnSubmit.disabled = true;
            btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Enviando código...';
        }

        apiClient.post("Users/ChangePasswordByEmail", {
            email: email,
            currentPassword: currentPassword,
            newPassword: newPassword,
            confirmPassword: confirmPassword
        })
            .done(function (res) {
                savePendingChange({
                    email: email,
                    currentPassword: currentPassword,
                    newPassword: newPassword,
                    confirmPassword: confirmPassword
                });
                setChangeOtpExpiry(3);
                changePassForm.classList.add("d-none");
                changePassOtpForm.classList.remove("d-none");
                if (changeOtpInput) {
                    changeOtpInput.value = "";
                    changeOtpInput.disabled = false;
                }
                startChangeCountdown();
                notify.success(res?.message || res?.Message || "Se ha enviado un código OTP a su correo.");
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

    changePassOtpForm.addEventListener("submit", function (e) {
        e.preventDefault();

        const storedExpiry = Number(sessionStorage.getItem(changeOtpExpiryKey) || 0);
        if (storedExpiry > 0 && Date.now() > storedExpiry) {
            notify.warning("El código OTP expiró. Reenvíe uno nuevo.");
            return;
        }

        const otpCode = changeOtpInput?.value.trim();
        const pendingChange = getPendingChange();

        if (!pendingChange || !otpCode || otpCode.length !== 6) {
            notify.warning("Ingrese el código OTP de 6 dígitos.");
            return;
        }

        const btnSubmit = changePassOtpForm.querySelector("button[type='submit']");
        const originalText = btnSubmit ? btnSubmit.innerHTML : "";
        if (btnSubmit) {
            btnSubmit.disabled = true;
            btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Confirmando...';
        }

        apiClient.post(`Users/ConfirmChangePasswordByEmail?tokenCode=${encodeURIComponent(otpCode)}`, {
            email: pendingChange.email,
            newPassword: pendingChange.newPassword,
            confirmPassword: pendingChange.confirmPassword
        })
            .done(function (res) {
                notify.success(res?.message || res?.Message || "Contraseña actualizada correctamente.");
                clearPendingChange();
                if (changeCountdownContainer) changeCountdownContainer.classList.add("d-none");
                if (changeOtpInput) changeOtpInput.value = "";
                if (emailInput) emailInput.value = "";
                if (currentPassInput) currentPassInput.value = "";
                if (newPassInput) newPassInput.value = "";
                if (confirmPassInput) confirmPassInput.value = "";
                clearInterval(window.sgdeChangeOtpTimer);
                setTimeout(function () {
                    window.location.href = "/Login";
                }, 1500);
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

    if (resendChangeOtpBtn) {
        resendChangeOtpBtn.addEventListener("click", function () {
            const pendingChange = getPendingChange();
            if (!pendingChange) {
                notify.warning("Primero envíe la solicitud de cambio de contraseña.");
                return;
            }

            const original = resendChangeOtpBtn.innerHTML;
            resendChangeOtpBtn.disabled = true;
            resendChangeOtpBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Reenviando...';

            apiClient.post("Users/ChangePasswordByEmail", {
                email: pendingChange.email,
                currentPassword: pendingChange.currentPassword,
                newPassword: pendingChange.newPassword,
                confirmPassword: pendingChange.confirmPassword
            })
                .done(function (res) {
                    setChangeOtpExpiry(3);
                    startChangeCountdown();
                    if (changeCountdownContainer) changeCountdownContainer.classList.remove("d-none");
                    if (changeOtpInput) {
                        changeOtpInput.value = "";
                        changeOtpInput.disabled = false;
                        changeOtpInput.focus();
                    }
                    notify.success(res?.message || res?.Message || "Código OTP reenviado correctamente.");
                })
                .fail(function (xhr) {
                    handleApiError(xhr);
                })
                .always(function () {
                    resendChangeOtpBtn.disabled = false;
                    resendChangeOtpBtn.innerHTML = original;
                });
        });
    }

 


});

// CONTROL DEL OJITO PARA VER CONTRASEÑA esta funcion puede ser REUTILIZABLE)
function togglePassword(inputId, buttonElement) {
    const passwordInput = document.getElementById(inputId);

    if (passwordInput) {
        // Alternar el tipo de input
        const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
        passwordInput.setAttribute('type', type);

        // Alternar el icono del ojito
        const icon = buttonElement.querySelector('i');
        if (icon) {
            icon.classList.toggle('bi-eye');
            icon.classList.toggle('bi-eye-slash');
        }
    }
};