// LoginViewController.js (§22.1, §23, §24) - Controlador JS para autenticación, registro y recuperación
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando LoginViewController...");

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

            apiClient.post("Users/LoginStep1", { email: email, password: password })
                .done(function (res) {
                    sessionStorage.setItem("sgde_login_email", email);

                    // En modo dev local, el backend omite OTP.
                    // Intentamos LoginStep2 directamente con un código dummy.
                    apiClient.post("Users/LoginStep2", { email: email, otpCode: "000000" })
                        .done(function (res2) {
                            const loginData = res2?.data || res2?.Data || (res2?.token || res2?.Token ? res2 : null);
                            if (loginData) {
                                session.save(loginData);
                                sessionStorage.removeItem("sgde_login_email");
                                notify.success("¡Inicio de sesión exitoso! Redirigiendo...");
                                setTimeout(function () {
                                    window.location.href = dashboardUrlForRole(session.getRole()) || "/";
                                }, 800);
                            }
                        })
                        .fail(function () {
                            // Si LoginStep2 directo falla, es porque el backend SÍ requiere OTP real.
                            // Redirigir al flujo OTP normal.
                            notify.success(res?.message || res?.Message || "Código OTP enviado a su correo.");
                            setTimeout(function () {
                                window.location.href = "/LoginOtp";
                            }, 1200);
                        });
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
        if (!savedEmail) {
            notify.warning("Por favor inicie sesión desde el paso 1.");
            setTimeout(function () {
                window.location.href = "/Login";
            }, 1500);
        }

        otpForm.addEventListener("submit", function (e) {
            e.preventDefault();
            const otpCode = document.getElementById("otpCode")?.value.trim();

            if (!otpCode || otpCode.length !== 6) {
                notify.warning("El código OTP debe tener 6 dígitos numéricos.");
                return;
            }

            const btnSubmit = otpForm.querySelector("button[type='submit']");
            const originalText = btnSubmit ? btnSubmit.innerHTML : "";
            if (btnSubmit) {
                btnSubmit.disabled = true;
                btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Verificando OTP...';
            }

            apiClient.post("Users/LoginStep2", { email: savedEmail, otpCode: otpCode })
                .done(function (res) {
                    const loginData = res?.data || res?.Data || (res?.token || res?.Token ? res : null);
                    if (loginData) {
                        session.save(loginData);
                        sessionStorage.removeItem("sgde_login_email");
                        notify.success("¡Inicio de sesión exitoso! Redirigiendo...");

                        setTimeout(function () {
                            window.location.href = dashboardUrlForRole(session.getRole()) || "/";
                        }, 1000);
                    } else {
                        notify.error("No se recibieron datos de sesión válidos.");
                        if (btnSubmit) {
                            btnSubmit.disabled = false;
                            btnSubmit.innerHTML = originalText;
                        }
                    }
                })
                .fail(function (xhr) {
                    if (btnSubmit) {
                        btnSubmit.disabled = false;
                        btnSubmit.innerHTML = originalText;
                    }
                    handleApiError(xhr);
                });
        });

        const resendBtn = document.getElementById("resendOtpBtn");
        if (resendBtn) {
            resendBtn.addEventListener("click", function () {
                notify.info("Por seguridad, regrese a la pantalla de inicio de sesión e ingrese sus credenciales para generar un nuevo código.");
                setTimeout(function () {
                    window.location.href = "/Login";
                }, 2000);
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

            if (password !== confirmPassword) {
                notify.error("Las contraseñas ingresadas no coinciden.");
                return;
            }

            const fn1 = document.getElementById("regFirstName1")?.value.trim() || document.getElementById("regFirstName")?.value.trim() || "";
            const fn2 = document.getElementById("regFirstName2")?.value.trim() || "";
            const ln1 = document.getElementById("regLastName1")?.value.trim() || document.getElementById("regLastName")?.value.trim() || "";
            const ln2 = document.getElementById("regLastName2")?.value.trim() || "";

            const dto = {
                identification: document.getElementById("regId")?.value.trim(),
                email: document.getElementById("regEmail")?.value.trim(),
                firstName: fn2 ? `${fn1} ${fn2}` : fn1,
                lastName: ln2 ? `${ln1} ${ln2}` : ln1,
                phone: document.getElementById("regPhone")?.value.trim(),
                birthDate: document.getElementById("regBirthDate")?.value,
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
        if (actEmailEl && savedActEmail && !actEmailEl.value) {
            actEmailEl.value = savedActEmail;
        }

        activateForm.addEventListener("submit", function (e) {
            e.preventDefault();
            const email = document.getElementById("actEmail")?.value.trim();
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

            apiClient.post("Users/Activate", { email: email, otpCode: otpCode })
                .done(function (res) {
                    notify.success(res?.message || res?.Message || "Cuenta activada correctamente. Ahora puede iniciar sesión.");
                    sessionStorage.removeItem("sgde_activate_email");
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

        // Reenvío del OTP de activación: usa el endpoint ResendOtp existente. Permite obtener un
        // código nuevo si el anterior expiró, sin tener que volver a llenar el formulario de registro.
        const resendActivationBtn = document.getElementById("resendActivationBtn");
        if (resendActivationBtn) {
            resendActivationBtn.addEventListener("click", function () {
                const email = document.getElementById("actEmail")?.value.trim();
                if (!email) {
                    notify.warning("Ingrese su correo electrónico para reenviar el código.");
                    return;
                }

                const original = resendActivationBtn.innerHTML;
                resendActivationBtn.disabled = true;
                resendActivationBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Reenviando...';

                apiClient.post("Users/ResendOtp", { email: email, usageType: "Activation" })
                    .done(function (res) {
                        notify.success(res?.message || res?.Message || "Código de activación reenviado. Revise su correo.");
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

    // ==========================================
    // 5. FLUJO DE RECUPERACIÓN (/RecoverPassword)
    // ==========================================
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

    // ==========================================
    // 6. FLUJO DE RESTABLECIMIENTO (/ResetPassword)
    // ==========================================
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

            apiClient.post("Users/ResetPassword", { email: email, otpCode: otpCode, newPassword: newPassword })
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
