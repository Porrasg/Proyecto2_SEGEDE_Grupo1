// RecoverPasswordViewController.js
// Maneja la solicitud, reenvío y uso del código OTP para restablecer contraseña

document.addEventListener("DOMContentLoaded", function () {
    "use strict";

    const recoverForm = document.getElementById("recoverForm");
    const resetForm = document.getElementById("resetForm");
    const resEmailInput = document.getElementById("resEmail");

    const STORAGE_EMAIL_KEY = "sgde_reset_email";
    const OTP_TTL_SECONDS = 180; // 3 minutos

    let otpRemaining = OTP_TTL_SECONDS;
    let otpTimerId = null;
    let otpTimerElement = document.getElementById("otpTimer");
    let resendButton = document.getElementById("btnResendOtp");

    // Extrae mensaje útil de la respuesta de la API
    function getApiMessage(response, fallback) {
        try {
            if (!response) return fallback;
            if (response.responseJSON && (response.responseJSON.message || response.responseJSON.Message))
                return response.responseJSON.message || response.responseJSON.Message;
            if (response.message) return response.message;
            if (response.Message) return response.Message;
            if (response.responseText && response.responseText.trim()) return response.responseText;
        } catch (e) {
            // ignore parsing errors
        }
        return fallback;
    }

    // Pone un botón en estado loading y devuelve función para restaurarlo
    function setButtonLoading(btn, text) {
        if (!btn) return function () { };
        const orig = btn.innerHTML;
        btn.disabled = true;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> ' + text;
        return function restore() {
            btn.disabled = false;
            btn.innerHTML = orig;
        };
    }

    // Formatea segundos a MM:SS
    function formatTime(s) {
        const sec = Math.max(0, Math.floor(s));
        const m = Math.floor(sec / 60);
        const r = sec % 60;
        return String(m).padStart(2, '0') + ':' + String(r).padStart(2, '0');
    }

    // Inicia/ reinicia el temporizador OTP
    function startOtpTimer(seconds) {
        if (!otpTimerElement || !resendButton) return;
        clearInterval(otpTimerId);
        otpRemaining = seconds || OTP_TTL_SECONDS;
        otpTimerElement.textContent = formatTime(otpRemaining);
        otpTimerId = setInterval(function () {
            otpRemaining--;
            otpTimerElement.textContent = formatTime(otpRemaining);
            if (otpRemaining <= 0) {
                clearInterval(otpTimerId);
                otpTimerId = null;
            }
        }, 1000);
    }

    // Crea controles OTP si no existen en la vista
    function ensureOtpControls() {
        if (!resetForm) return;
        if (otpTimerElement && resendButton) return;

        const wrapper = document.createElement('div');
        wrapper.className = 'mb-3 d-flex justify-content-between align-items-center';
        wrapper.innerHTML = '\n            <div class="text-muted small">Tiempo restante: <span id="otpTimer" class="fw-bold">--:--</span></div>\n            <button type="button" id="btnResendOtp" class="btn btn-outline-primary btn-sm">Reenviar código</button>\n        ';

        const otpField = document.getElementById('resOtp');
        if (otpField && otpField.parentNode) otpField.parentNode.insertBefore(wrapper, otpField.nextSibling);
        else resetForm.insertBefore(wrapper, resetForm.firstChild);

        otpTimerElement = document.getElementById('otpTimer');
        resendButton = document.getElementById('btnResendOtp');
    }

    // Llama al endpoint para generar/reenviar código
    function requestPasswordCode(email) {
        return apiClient.post('Users/RecoverPassword', { email: email });
    }

    // Preparar controles y estado inicial
    ensureOtpControls();
    const saved = sessionStorage.getItem(STORAGE_EMAIL_KEY);
    if (saved && resEmailInput && !resEmailInput.value) resEmailInput.value = saved;
    if (saved && otpTimerElement && resendButton) startOtpTimer();

    // --------------------
    // Recover: solicitar código
    // --------------------
    if (recoverForm) {
        recoverForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const email = (document.getElementById('recEmail')?.value || '').trim();
            if (!email) { notify.warning('Ingrese su correo.'); return; }
            const btn = recoverForm.querySelector("button[type='submit']");
            const restore = setButtonLoading(btn, 'Enviando...');

            requestPasswordCode(email)
                .done(function (res) {
                    sessionStorage.setItem(STORAGE_EMAIL_KEY, email);
                    notify.success(getApiMessage(res, 'Código enviado. Revise su correo.'));
                    setTimeout(function () { window.location.href = '/ResetPassword'; }, 900);
                })
                .fail(function (xhr) {
                    restore();
                    notify.error(getApiMessage(xhr, 'No se pudo enviar el código.')); 
                });
        });
    }

    // --------------------
    // Resend: reenviar código
    // --------------------
    // Use event delegation so the handler works even if the button is recreated
    document.addEventListener('click', function (ev) {
        const btn = ev.target && (ev.target.id === 'btnResendOtp' ? ev.target : ev.target.closest && ev.target.closest('#btnResendOtp'));
        if (!btn) return;
        // ensure controls are present
        ensureOtpControls();
        const email = (resEmailInput?.value || sessionStorage.getItem(STORAGE_EMAIL_KEY) || '').trim();
        if (!email) { notify.warning('Ingrese el correo asociado.'); return; }
        if (btn.disabled) return;
        const restore = setButtonLoading(btn, 'Enviando...');

        requestPasswordCode(email)
            .done(function (res) {
                sessionStorage.setItem(STORAGE_EMAIL_KEY, email);
                notify.success(getApiMessage(res, 'Código reenviado.'));
                startOtpTimer(OTP_TTL_SECONDS);
                setTimeout(restore, 300);
            })
            .fail(function (xhr) {
                restore();
                notify.error(getApiMessage(xhr, 'No se pudo reenviar el código.'));
            });
    });

    // --------------------
    // Reset: enviar nueva contraseña
    // --------------------
    if (resetForm) {
        resetForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const email = (document.getElementById('resEmail')?.value || '').trim();
            const otp = (document.getElementById('resOtp')?.value || '').trim();
            const np = document.getElementById('resNewPassword')?.value;
            const cp = document.getElementById('resConfirmPassword')?.value;

            if (!email || !otp || !np) { notify.warning('Complete los campos requeridos.'); return; }
            if (np !== cp) { notify.error('Las contraseñas no coinciden.'); return; }

            const btn = resetForm.querySelector("button[type='submit']");
            const restore = setButtonLoading(btn, 'Restableciendo...');

            apiClient.post('Users/ResetPassword', {
                email: email,
                otpCode: otp,
                newPassword: np,
                confirmPassword: cp
            })
                .done(function (res) {
                    clearInterval(otpTimerId);
                    sessionStorage.removeItem(STORAGE_EMAIL_KEY);
                    notify.success(getApiMessage(res, 'Contraseña restablecida. Inicie sesión.'));
                    setTimeout(function () { window.location.href = '/Login'; }, 1200);
                })
                .fail(function (xhr) {
                    restore();
                    notify.error(getApiMessage(xhr, 'No se pudo restablecer la contraseña.'));
                });
        });
    }

});
