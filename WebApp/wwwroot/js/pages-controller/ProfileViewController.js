/**
 * ProfileViewController.js
 * Maneja la lógica de la página de perfil universal (para todos los roles).
 */
document.addEventListener("DOMContentLoaded", function () {
    const userId = session.getUserId();
    if (!userId) {
        window.location.href = "/Login";
        return;
    }

    let cachedUser = null;
    let profilePhotoDataUrl = null;

    if (document.getElementById("profName")) {
        loadUserProfile();

        const profileForm = document.getElementById("profileForm");
        if (profileForm) {
            profileForm.addEventListener("submit", function (e) {
                e.preventDefault();
                updateProfile();
            });
        }

        const confirmDeactBtn = document.getElementById("confirmDeactBtn");
        if (confirmDeactBtn) {
            confirmDeactBtn.addEventListener("click", function () {
                deactivateAccount();
            });
        }
    }

    function getUserField(user, camel, pascal) {
        return user?.[camel] ?? user?.[pascal] ?? "";
    }

    function loadUserProfile() {
        apiClient.get("Users/RetrieveById/" + userId)
            .done(function (res) {
                cachedUser = res?.data || res?.Data || {};
                
                const firstName = getUserField(cachedUser, "firstName", "FirstName");
                const firstLastName = getUserField(cachedUser, "firstLastName", "FirstLastName");
                const secondLastName = getUserField(cachedUser, "secondLastName", "SecondLastName");
                
                const fullName = [firstName, firstLastName, secondLastName].filter(Boolean).join(" ");
                
                // Mostrar info en card
                setText("profName", fullName || "Usuario SGDE");
                setText("profEmail", getUserField(cachedUser, "email", "Email") || "-");
                setText("profId", getUserField(cachedUser, "identification", "Identification") || "-");
                
                const role = getUserField(cachedUser, "role", "Role");
                setText("profRole", role || "-");
                
                // Mostrar sección de riesgo sólo para compradores/clientes
                if (role && (role.toLowerCase() === "buyer" || role.toLowerCase() === "customer")) {
                    const dangerZone = document.getElementById("dangerZone");
                    if (dangerZone) dangerZone.classList.remove("d-none");
                }

                const created = getUserField(cachedUser, "createdAt", "CreatedAt");
                setText("profDate", created ? new Date(created).toLocaleDateString("es-CR") : "-");

                // Llenar formulario
                setInputValue("pFirstName", firstName);
                setInputValue("pFirstLastName", firstLastName);
                setInputValue("pSecondLastName", secondLastName);
                setInputValue("pEmail", getUserField(cachedUser, "email", "Email"));
                setInputValue("pPhone", getUserField(cachedUser, "phone", "Phone") || getUserField(cachedUser, "phoneNumber", "PhoneNumber") || "");

                const photo = getUserField(cachedUser, "photoUrl", "PhotoUrl") || getUserField(cachedUser, "profilePhoto", "ProfilePhoto");
                const imgEl = document.getElementById("profilePhoto");
                if (photo && imgEl) imgEl.src = photo;
            })
            .fail(function (xhr) {
                handleApiError(xhr);
            });
    }

    const pPhotoInput = document.getElementById("pPhoto");
    if (pPhotoInput) {
        pPhotoInput.addEventListener("change", function () {
            const file = this.files?.[0];
            if (!file) { profilePhotoDataUrl = null; return; }
            const img = new Image();
            img.onload = function () {
                const MAX = 256;
                const scale = Math.min(1, MAX / Math.max(img.width, img.height));
                const canvas = document.createElement("canvas");
                canvas.width = Math.round(img.width * scale);
                canvas.height = Math.round(img.height * scale);
                canvas.getContext("2d").drawImage(img, 0, 0, canvas.width, canvas.height);
                profilePhotoDataUrl = canvas.toDataURL("image/jpeg", 0.82);
                URL.revokeObjectURL(img.src);
                const imgEl = document.getElementById("profilePhoto");
                if (imgEl) imgEl.src = profilePhotoDataUrl;
            };
            img.onerror = function () {
                URL.revokeObjectURL(img.src);
                notify.error("No se pudo leer la imagen seleccionada.");
            };
            img.src = URL.createObjectURL(file);
        });
    }

    function updateProfile() {
        const firstName = document.getElementById("pFirstName")?.value.trim();
        const firstLastName = document.getElementById("pFirstLastName")?.value.trim();
        const secondLastName = document.getElementById("pSecondLastName")?.value.trim() || null;
        const email = document.getElementById("pEmail")?.value.trim();
        const phone = document.getElementById("pPhone")?.value.trim();
        
        const newPass = document.getElementById("pNewPass")?.value;
        const currentPass = document.getElementById("pCurrPass")?.value || "";
        
        const currentUser = cachedUser || {};

        const submitBtn = document.querySelector("#profileForm button[type='submit']");
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Guardando...';
        }

        apiClient.put("Users/Update", {
            id: parseInt(getUserField(currentUser, "id", "Id") || userId),
            identification: getUserField(currentUser, "identification", "Identification"), // Cédula no editable
            firstName: firstName,
            firstLastName: firstLastName,
            secondLastName: secondLastName,
            email: email,
            phoneNumber: phone || "",
            birthDate: getUserField(currentUser, "birthDate", "BirthDate") || null,
            password: newPass || currentPass || getUserField(currentUser, "password", "Password") || "",
            profilePhoto: profilePhotoDataUrl || getUserField(currentUser, "profilePhoto", "ProfilePhoto") || getUserField(currentUser, "photoUrl", "PhotoUrl") || null,
            role: getUserField(currentUser, "role", "Role") || session.getRole(),
            status: getUserField(currentUser, "status", "Status") || "Active"
        }).done(function () {
            notify.success("Datos de perfil actualizados con éxito.");
            if (document.getElementById("pCurrPass")) document.getElementById("pCurrPass").value = "";
            if (document.getElementById("pNewPass")) document.getElementById("pNewPass").value = "";
            loadUserProfile(); // Recargar datos para actualizar la tarjeta superior
        }).fail(function (xhr) {
            handleApiError(xhr);
        }).always(function () {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.textContent = "Guardar Cambios";
            }
        });
    }

    function deactivateAccount() {
        const btn = document.getElementById("confirmDeactBtn");
        if (btn) {
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Desactivando...';
        }

        apiClient.delete("Users/Delete", { id: parseInt(userId) }).done(function () {
            notify.info("Tu cuenta ha sido desactivada. Cerrando sesión...");
            setTimeout(() => {
                session.clear();
                window.location.href = "/Login";
            }, 2000);
        }).fail(function (xhr) {
            handleApiError(xhr);
            if (btn) {
                btn.disabled = false;
                btn.textContent = "Confirmar Desactivación";
            }
        });
    }

    function setText(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = value;
    }

    function setInputValue(id, value) {
        const el = document.getElementById(id);
        if (el && value) el.value = value;
    }
});
