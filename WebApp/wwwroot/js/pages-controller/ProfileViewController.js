//ProfileViewController.js - Controlador JS para el Panel de Perfil del usuario
document.addEventListener("DOMContentLoaded", function () {
    // Primero validamos que la sesión exista y siga siendo válida
    const hasSession = window.session && typeof window.session.isAuthenticated === "function"
        ? window.session.isAuthenticated() && !window.session.isExpired()
        : false;

    // Si no hay sesión, se redirige al Login con returnUrl.
    if (!hasSession) {
        window.location.href = `/Login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
        return;
    }

    // Obtener datos desde la sesión:
    // - Se intenta usar window.session.get() si está disponible
    // - Como respaldo, se lee "sgde_session" desde sessionStorage
    let s = null;
    try {
        s = typeof window.session.get === "function"
            ? window.session.get()
            : (sessionStorage.getItem("sgde_session") ? JSON.parse(sessionStorage.getItem("sgde_session")) : null);
    } catch (e) {
        s = null;
    }

    // Si no hay sesión, se redirige al Login con returnUrl
    if (!s) {
        window.location.href = `/Login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
        return;
    }

    // Obtener datos de email, rol telefono y id, se prueban en mayúscula y minúscula porque la sesión podría venir de JavaScript o del backend
    const email = s.email || s.Email || "";
    const role = s.role || s.Role || "";
    const phone = s.phone || s.Phone || s.phoneNumber || s.PhoneNumber || "";
    const userIdValue = s.userId || s.UserId || s.id || s.Id || null;

    // Obtener nombre a partir de campos individuales
    const firstName = s.firstName || s.FirstName || s.firstname || s.Firstname || "";
    const firstLastName = s.firstLastName || s.FirstLastName || s.firstLast || s.first_last || "";
    const secondLastName = s.secondLastName || s.SecondLastName || s.secondLast || s.second_last || "";
    const identificationValue = s.identification || s.Identification || s.cedula || s.Cedula || s.nationalId || s.NationalId || s.identificationNumber || s.IdentificationNumber || null;
    // Juntar los campos del nombre que existan
    const name = [];
    if (firstName) name.push(firstName);
    if (firstLastName) name.push(firstLastName);
    if (secondLastName) name.push(secondLastName);
    const fullName = name.length > 0 ? name.join(" ") : email || "Usuario SGDE"; // si no obtiene el nombre completo usa email o Usuario SGDE

    // Helper
    function setText(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = value;
    }

    // Poblar los campos principales del perfil
    setText("profUserId", userIdValue || "-");
    setText("profIdentification", identificationValue || "-");
    setText("profName", fullName || "-");
    setText("profEmail", email || "-");
    setText("profRole", role || "-");
    setText("profDate", (s.createdAt || s.CreatedAt) ? new Date(s.createdAt || s.CreatedAt).toLocaleDateString("es-CR") : "-");

    // Poblar los campos de la sección de detalles
    setText("detUserId", userIdValue || "-");
    setText("detIdentification", identificationValue || "-");
    setText("detName", fullName || "-");
    setText("detEmail", email || "-");
    setText("detRole", role || "-");
    setText("detPhone", phone || "-");

    // Asignar foto de perfil si la sesión trae una URL.
    const imgEl = document.getElementById("profilePhoto");
    const photo = s.photoUrl || s.profilePhoto || s.PhotoUrl || s.ProfilePhoto || null;
    if (imgEl && photo) imgEl.src = photo;

    // ---------------- EDITAR PERFIL ----------------
    const editButton = document.getElementById("editProfileBtn");
    const modal = document.getElementById("editProfileModal");
    const form = document.getElementById("editProfileForm");
    const saveButton = document.getElementById("saveProfileBtn");
    let emailToConfirm = "";

    // Evita errores si Hot Reload ejecuta este archivo fuera de la página Perfil.
    if (!editButton || !modal || !form || !saveButton) return;

    editButton.addEventListener("click", function () {
        document.getElementById("editFirstName").value = firstName;
        document.getElementById("editFirstLastName").value = firstLastName;
        document.getElementById("editSecondLastName").value = secondLastName;
        document.getElementById("editProfileEmail").value = email;
        document.getElementById("editProfilePhone").value = phone;
        emailToConfirm = "";
        document.getElementById("profileEditFields").classList.remove("d-none");
        document.getElementById("emailOtpFields").classList.add("d-none");
        saveButton.innerHTML = '<i class="bi bi-check-lg me-1"></i>Guardar cambios';
    });

    form.addEventListener("submit", function (event) {
        event.preventDefault();

        if (!userIdValue || Number(userIdValue) <= 0) {
            notify.error("No es posible editar este perfil con la sesión actual.");
            return;
        }

        // Si hay un correo pendiente, este botón confirma el OTP recibido.
        if (emailToConfirm) {
            const otpCode = document.getElementById("emailChangeOtp").value.trim();
            if (!/^\d{6}$/.test(otpCode)) {
                notify.warning("Ingrese el código OTP de 6 dígitos.");
                return;
            }

            apiClient.post("Users/Profile/ConfirmEmailChange", { userId: Number(userIdValue), newEmail: emailToConfirm, otpCode: otpCode })
                .done(function (updatedUser) {
                    session.save(Object.assign(s, updatedUser));
                    notify.success("Correo electrónico actualizado correctamente.");
                    window.location.reload();
                })
                .fail(handleApiError);
            return;
        }

        const newFirstName = document.getElementById("editFirstName").value.trim();
        const newFirstLastName = document.getElementById("editFirstLastName").value.trim();
        const newSecondLastName = document.getElementById("editSecondLastName").value.trim();
        const newEmail = document.getElementById("editProfileEmail").value.trim();
        const newPhone = document.getElementById("editProfilePhone").value.trim();

        if (!newFirstName || !newFirstLastName || !newEmail || !/^\d{8,}$/.test(newPhone)) {
            notify.warning("Complete los campos obligatorios y use un teléfono de al menos 8 dígitos.");
            return;
        }

        apiClient.put("Users/Profile", {
            userId: Number(userIdValue),
            firstName: newFirstName,
            firstLastName: newFirstLastName,
            secondLastName: newSecondLastName,
            phoneNumber: newPhone
        })
            .done(function (updatedUser) {
                session.save(Object.assign(s, updatedUser));

                // Si el correo no cambió, el proceso termina aquí.
                if (newEmail.toLowerCase() === email.toLowerCase()) {
                    notify.success("Perfil actualizado correctamente.");
                    window.location.reload();
                    return;
                }

                // Si cambió, se envía un OTP al correo nuevo para confirmarlo.
                apiClient.post("Users/Profile/RequestEmailChange", { userId: Number(userIdValue), newEmail: newEmail })
                    .done(function () {
                        emailToConfirm = newEmail;
                        document.getElementById("profileEditFields").classList.add("d-none");
                        document.getElementById("emailOtpFields").classList.remove("d-none");
                        document.getElementById("pendingEmailLabel").textContent = newEmail;
                        saveButton.innerHTML = '<i class="bi bi-check-lg me-1"></i>Confirmar correo';
                        notify.success("Código de confirmación enviado al correo nuevo.");
                    })
                    .fail(handleApiError);
            })
            .fail(handleApiError);
    });
});