// LoginViewController.js (§22.1, §23, §24) - Controlador JS para autenticación, registro y recuperación
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando LoginViewController...");

    //datos estaticos
    const staticLoginEmails = ["admin@segede.local", "engineer@segede.local", "buyer@segede.local"];
    function isStaticLoginEmail(email) {
        return !!email && staticLoginEmails.includes(String(email).trim().toLowerCase());
    }

    // ==========================================
    // 1. FLUJO DE LOGIN - PASO 1 (/Login)
    // ==========================================
    const loginForm = document.getElementById("loginForm");
    if (loginForm) {
        loginForm.addEventListener("submit", function (e) {
            e.preventDefault();
            const email = document.getElementById("identification")?.value.trim();
            const password = document.getElementById("password")?.value;

            if (!email || !password) {
                notify.warning("Por favor ingrese su correo y contraseña.");
                return;
            }

            const btnSubmit = loginForm.querySelector("button[type='submit']");
            const originalText = btnSubmit ? btnSubmit.innerHTML : "";
            if (btnSubmit) {
                btnSubmit.disabled = true;
                btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Verificando...';
            }

            apiClient.post("Users/Login", { email: email, password: password })
                .done(function (res) {
                    if (isStaticLoginEmail(email)) {
                        sessionStorage.removeItem("sgde_login_email");
                        sessionStorage.removeItem("sgde_login_userId");
                        sessionStorage.removeItem("sgde_login_otp_expires_at");
                        session.save(res);
                        notify.success("Inicio de sesión exitoso.");
                        setTimeout(function () {
                            window.location.href = dashboardUrlForRole(session.getRole()) || "/";
                        }, 1000);
                        return;
                    }

                    sessionStorage.setItem("sgde_login_email", email);
                    sessionStorage.setItem("sgde_login_userId", res?.userId ?? res?.UserId ?? res?.id ?? res?.Id ?? "");
                    sessionStorage.setItem("sgde_login_otp_expires_at", String(Date.now() + (3 * 60 * 1000)));
                    notify.success(res?.message || res?.Message || "Código OTP enviado a su correo.");
                    setTimeout(function () {
                        window.location.href = "/LoginOtp";
                    }, 1200);
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

    // ==========================================
    // 2. FLUJO DE LOGIN - PASO 2 OTP (/LoginOtp)
    // ==========================================
    const otpForm = document.getElementById("otpForm");
    if (otpForm) {
        const savedEmail = sessionStorage.getItem("sgde_login_email");
        const loginOtpExpiryKey = "sgde_login_otp_expires_at";
        const countdownEl = document.getElementById("loginOtpCountdown");
        const otpInput = document.getElementById("otpCode");
        const otpSubmitBtn = otpForm.querySelector("button[type='submit']");
        const resendBtn = document.getElementById("resendOtpBtn");

        function setLoginOtpExpiry(minutes = 3) {
            const expiresAt = Date.now() + (minutes * 60 * 1000);
            sessionStorage.setItem(loginOtpExpiryKey, String(expiresAt));
            return expiresAt;
        }

        function startLoginOtpCountdown() {
            const stored = Number(sessionStorage.getItem(loginOtpExpiryKey) || 0);
            const expiry = stored > 0 ? stored : setLoginOtpExpiry(3);

            const update = function () {
                const remaining = Math.max(0, expiry - Date.now());
                const totalSeconds = Math.floor(remaining / 1000);
                const minutes = String(Math.floor(totalSeconds / 60)).padStart(2, "0");
                const seconds = String(totalSeconds % 60).padStart(2, "0");

                if (countdownEl) countdownEl.textContent = `${minutes}:${seconds}`;

                if (remaining <= 0) {
                    if (otpSubmitBtn) otpSubmitBtn.disabled = true;
                    if (resendBtn) resendBtn.disabled = false;
                    if (otpInput) otpInput.disabled = true;
                    clearInterval(window.sgdeLoginOtpTimer);
                    notify.warning("El código OTP expiró. Reenvíe uno nuevo.");
                } else {
                    if (otpInput) otpInput.disabled = false;
                    if (otpSubmitBtn) otpSubmitBtn.disabled = false;
                }
            };

            update();
            clearInterval(window.sgdeLoginOtpTimer);
            window.sgdeLoginOtpTimer = setInterval(update, 1000);
        }

        if (!savedEmail) {
            notify.warning("Por favor inicie sesión desde el paso 1.");
            setTimeout(function () {
                window.location.href = "/Login";
            }, 1500);
        } else {
            startLoginOtpCountdown();
        }

        otpForm.addEventListener("submit", function (e) {
            e.preventDefault();
            const storedExpiry = Number(sessionStorage.getItem(loginOtpExpiryKey) || 0);
            if (storedExpiry > 0 && Date.now() > storedExpiry) {
                notify.warning("El código OTP expiró. Reenvíe uno nuevo.");
                return;
            }

            const otpCode = document.getElementById("otpCode")?.value.trim();

            if (!otpCode || otpCode.length !== 6) {
                notify.warning("El código OTP debe tener 6 dígitos numéricos.");
                return;
            }

            const originalText = otpSubmitBtn ? otpSubmitBtn.innerHTML : "";
            if (otpSubmitBtn) {
                otpSubmitBtn.disabled = true;
                otpSubmitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Verificando OTP...';
            }

            apiClient.post("Users/ValidateLoginOtp", { email: savedEmail, otpCode: otpCode })
                .done(function (res) {
                    const loginData = res?.data || res?.Data || res;
                    if (loginData) {
                        session.save(loginData);
                        sessionStorage.removeItem("sgde_login_email");
                        sessionStorage.removeItem("sgde_login_userId");
                        sessionStorage.removeItem(loginOtpExpiryKey);
                        clearInterval(window.sgdeLoginOtpTimer);
                        notify.success("¡Inicio de sesión exitoso! Redirigiendo...");

                        setTimeout(function () {
                            window.location.href = dashboardUrlForRole(session.getRole()) || "/";
                        }, 1000);
                    }
                })
                .fail(function (xhr) {
                    if (otpSubmitBtn) {
                        otpSubmitBtn.disabled = false;
                        otpSubmitBtn.innerHTML = originalText;
                    }
                    handleApiError(xhr);
                });
        });

        if (resendBtn) {
            resendBtn.addEventListener("click", function () {
                if (!savedEmail) {
                    notify.warning("Por favor inicie sesión desde el paso 1.");
                    return;
                }

                const original = resendBtn.innerHTML;
                resendBtn.disabled = true;
                resendBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Reenviando...';

                apiClient.post("Users/ResendOtp?usageType=Login", { email: savedEmail })
                    .done(function (res) {
                        notify.success(res?.message || res?.Message || "Código OTP reenviado correctamente.");
                        sessionStorage.removeItem(loginOtpExpiryKey);
                        setLoginOtpExpiry(3);
                        startLoginOtpCountdown();
                        if (otpInput) {
                            otpInput.value = "";
                            otpInput.disabled = false;
                            otpInput.focus();
                        }
                    })
                    .fail(function (xhr) {
                        handleApiError(xhr);
                    })
                    .always(function () {
                        resendBtn.disabled = false;
                        resendBtn.innerHTML = original;
                    });
            });
        }
    }

    // ==========================================
    // 3. FLUJO DE REGISTRO (/Register)
    // ==========================================
    const registerForm = document.getElementById("registerForm");
    if (registerForm) {
        // Edad calculada automáticamente a partir de la fecha de nacimiento (RF de registro).
        const birthInput = document.getElementById("regBirthDate");
        const ageInput = document.getElementById("regAge");
        if (birthInput && ageInput) {
            birthInput.addEventListener("change", function () {
                const bd = new Date(this.value);
                if (isNaN(bd)) { ageInput.value = ""; return; }
                const today = new Date();
                let age = today.getFullYear() - bd.getFullYear();
                const m = today.getMonth() - bd.getMonth();
                if (m < 0 || (m === 0 && today.getDate() < bd.getDate())) age--;
                ageInput.value = age >= 0 ? `${age} años` : "";
            });
        }

        // Fotografía de perfil: se redimensiona en el cliente (máx. 256px) y se envía como data-URL base64.
        let photoDataUrl = null;
        const photoInput = document.getElementById("regPhoto");
        const photoPreview = document.getElementById("regPhotoPreview");
        const photoPlaceholder = document.getElementById("regPhotoPlaceholder");
        if (photoInput) {
            photoInput.addEventListener("change", function () {
                const file = this.files?.[0];
                if (!file) { photoDataUrl = null; return; }
                if (!/^image\/(jpeg|png|webp)$/.test(file.type)) {
                    notify.warning("Formato no soportado. Use JPG, PNG o WebP.");
                    this.value = "";
                    return;
                }
                const img = new Image();
                img.onload = function () {
                    const MAX = 256;
                    const scale = Math.min(1, MAX / Math.max(img.width, img.height));
                    const canvas = document.createElement("canvas");
                    canvas.width = Math.round(img.width * scale);
                    canvas.height = Math.round(img.height * scale);
                    canvas.getContext("2d").drawImage(img, 0, 0, canvas.width, canvas.height);
                    photoDataUrl = canvas.toDataURL("image/jpeg", 0.82);
                    URL.revokeObjectURL(img.src);
                    if (photoPreview && photoPlaceholder) {
                        photoPreview.src = photoDataUrl;
                        photoPreview.classList.remove("d-none");
                        photoPlaceholder.classList.add("d-none");
                    }
                };
                img.onerror = function () {
                    URL.revokeObjectURL(img.src);
                    notify.error("No se pudo leer la imagen seleccionada.");
                };
                img.src = URL.createObjectURL(file);
            });
        }

        registerForm.addEventListener("submit", function (e) {
            e.preventDefault();

            const password = document.getElementById("regPassword")?.value;
            const confirmPassword = document.getElementById("regConfirmPassword")?.value;
            const fullName = document.getElementById("regFirstName")?.value.trim() || "";
            const ln1 = document.getElementById("regLastName1")?.value.trim() || document.getElementById("regLastName")?.value.trim() || "";
            const ln2 = document.getElementById("regLastName2")?.value.trim() || "";
            const email = document.getElementById("regEmail")?.value.trim();
            const identification = document.getElementById("regId")?.value.trim();
            const phone = document.getElementById("regPhone")?.value.trim();
            const birthDate = document.getElementById("regBirthDate")?.value;

            if (!identification || !email || !fullName || !ln1 || !phone || !birthDate || !password || !confirmPassword) {
                notify.warning("Complete todos los campos obligatorios.");
                return;
            }

            if (password !== confirmPassword) {
                notify.error("Las contraseñas ingresadas no coinciden.");
                return;
            }

            if (!/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$/.test(password)) {
                notify.warning("La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial.");
                return;
            }

            const dto = {
                identification: identification,
                email: email,
                firstName: fullName,
                firstLastName: ln1,
                secondLastName: ln2 || null,
                phone: phone,
                birthDate: birthDate,
                photoUrl: photoDataUrl,
                password: password
            };

            const btnSubmit = registerForm.querySelector("button[type='submit']");
            const originalText = btnSubmit ? btnSubmit.innerHTML : "";
            if (btnSubmit) {
                btnSubmit.disabled = true;
                btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Registrando...';
            }

            apiClient.post("Users/Register", dto)
                .done(function (res) {
                    notify.success(res?.message || res?.Message || "Comprador registrado con éxito. Active su cuenta con el código enviado a su correo.");
                    sessionStorage.setItem("sgde_activate_email", dto.email);
                    setTimeout(function () {
                        window.location.href = "/Activate";
                    }, 1500);
                })
                .fail(function (xhr) {
                    if (btnSubmit) {
                        btnSubmit.disabled = false;
                        btnSubmit.innerHTML = originalText;
                    }
                    const msg = xhr?.responseJSON?.message || xhr?.responseJSON?.Message || xhr?.responseText || "No se pudo registrar el usuario.";
                    notify.error(msg);
                    handleApiError(xhr);
                });
        });
    }

    // ==========================================
    // 4. FLUJO DE ACTIVACIÓN (/Activate)
    // ==========================================
    const activateForm = document.getElementById("activateForm");
    if (activateForm) {
        const actEmailEl = document.getElementById("actEmail");
        const savedActEmail = sessionStorage.getItem("sgde_activate_email");
        if (actEmailEl && savedActEmail) {
            actEmailEl.value = savedActEmail;
            actEmailEl.readOnly = true;
        }

        const countdownEl = document.getElementById("actCountdown");
        const activateBtn = activateForm.querySelector("button[type='submit']");
        const resendActivationBtn = document.getElementById("resendActivationBtn");
        const activationExpiryKey = "sgde_activation_expires_at";

        function setActivationExpiry(minutes = 3) {
            const expiresAt = Date.now() + (minutes * 60 * 1000);
            sessionStorage.setItem(activationExpiryKey, String(expiresAt));
            return expiresAt;
        }

        function startActivationCountdown() {
            const stored = Number(sessionStorage.getItem(activationExpiryKey) || 0);
            const expiry = stored > 0 ? stored : setActivationExpiry(3);

            const update = function () {
                const remaining = Math.max(0, expiry - Date.now());
                const totalSeconds = Math.floor(remaining / 1000);
                const minutes = String(Math.floor(totalSeconds / 60)).padStart(2, "0");
                const seconds = String(totalSeconds % 60).padStart(2, "0");

                if (countdownEl) countdownEl.textContent = `${minutes}:${seconds}`;

                if (remaining <= 0) {
                    if (activateBtn) activateBtn.disabled = true;
                    if (resendActivationBtn) resendActivationBtn.disabled = false;
                    clearInterval(window.sgdeActivationTimer);
                    notify.warning("El código de activación expiró. Reenvíe uno nuevo.");
                } else if (activateBtn) {
                    activateBtn.disabled = false;
                }
            };

            update();
            clearInterval(window.sgdeActivationTimer);
            window.sgdeActivationTimer = setInterval(update, 1000);
        }

        startActivationCountdown();

        activateForm.addEventListener("submit", function (e) {
            e.preventDefault();
            const email = (savedActEmail || document.getElementById("actEmail")?.value || "").trim();
            const otpCode = document.getElementById("actOtp")?.value.trim();

            if (!email || !otpCode || otpCode.length !== 6) {
                notify.warning("Por favor ingrese su correo y el código de activación de 6 dígitos.");
                return;
            }

            const btnSubmit = activateForm.querySelector("button[type='submit']");
            const originalText = btnSubmit ? btnSubmit.innerHTML : "";
            if (btnSubmit) {
                btnSubmit.disabled = true;
                btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Activando...';
            }

            apiClient.post("Users/Activate", { email: email, tokenCode: otpCode })
                .done(function (res) {
                    notify.success(res?.message || res?.Message || "Cuenta activada correctamente. Ahora puede iniciar sesión.");
                    sessionStorage.removeItem("sgde_activate_email");
                    sessionStorage.removeItem(activationExpiryKey);
                    setTimeout(function () {
                        window.location.href = "/Login";
                    }, 1500);
                })
                .fail(function (xhr) {
                    if (btnSubmit) {
                        btnSubmit.disabled = false;
                        btnSubmit.innerHTML = originalText;
                    }
                    const msg = xhr?.responseJSON?.message
                        || xhr?.responseJSON?.Message
                        || xhr?.responseJSON?.error
                        || xhr?.statusText
                        || xhr?.responseText
                        || "No se pudo activar la cuenta.";
                    notify.error(msg);
                    if (!xhr?.responseJSON?.message && !xhr?.responseJSON?.Message && !xhr?.responseJSON?.error && !xhr?.responseText) {
                        handleApiError(xhr);
                    }
                });
        });

        // Reenvío del OTP de activación: usa el endpoint ResendOtp existente. Permite obtener un
        // código nuevo si el anterior expiró, sin tener que volver a llenar el formulario de registro.
        if (resendActivationBtn) {
            resendActivationBtn.addEventListener("click", function () {
                const email = (savedActEmail || document.getElementById("actEmail")?.value || "").trim();
                if (!email) {
                    notify.warning("Ingrese su correo electrónico para reenviar el código.");
                    return;
                }

                const original = resendActivationBtn.innerHTML;
                resendActivationBtn.disabled = true;
                resendActivationBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Reenviando...';

                apiClient.post("Users/ResendOtp?usageType=Activation", { email: email })
                    .done(function (res) {
                        notify.success(res?.message || res?.Message || "Código de activación reenviado. Revise su correo.");
                        setActivationExpiry(3);
                        startActivationCountdown();
                    })
                    .fail(function (xhr) {
                        handleApiError(xhr);
                    })
                    .always(function () {
                        resendActivationBtn.disabled = false;
                        resendActivationBtn.innerHTML = original;
                    });
            });
        }
    }

    // CONTROL DEL OJITO PARA VER CONTRASEÑA (Aporte de Josué)
    const togglePassword = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('password');

    if (togglePassword && passwordInput) {
        togglePassword.addEventListener('click', function () {
            // Alternar el tipo de input de password a text y viceversa
            const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
            passwordInput.setAttribute('type', type);

            // Alternar el icono del ojito (ojo abierto / ojo tachado)
            const icon = this.querySelector('i');
            if (icon) {
                icon.classList.toggle('bi-eye');
                icon.classList.toggle('bi-eye-slash');
            }
        });
    }
});
