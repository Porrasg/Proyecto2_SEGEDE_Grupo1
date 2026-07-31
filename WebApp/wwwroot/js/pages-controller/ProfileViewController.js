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

    // Obtener datos de email y rol se prueban en mayúscula y minúscula porque la sesión podría venir de JavaScript o del backend
    const email = s.email || s.Email || "";
    const role = s.role || s.Role || "";

    // Obtener nombre a partir de campos individuales
    const firstName = s.firstName || s.FirstName || s.firstname || s.Firstname || null;
    const firstLastName = s.firstLastName || s.FirstLastName || s.firstLast || s.first_last || null;
    const secondLastName = s.secondLastName || s.SecondLastName || s.secondLast || s.second_last || null;
    
    // Juntar esos campos que existan
    const name = [];
    if (firstName) name.push(firstName);
    if (firstLastName) name.push(firstLastName);
    if (secondLastName) name.push(secondLastName);

    // Construir el nombre completo
    const fullName = name.length > 0
        ? name.join(" ")
        : email || "Usuario SGDE"; // si no trae el nombre usamos email o "Usuario SGDE"

    // Determinar ID interno y cédula/identificación.
    // Se prueban varias llaves por compatibilidad con distintos formatos de sesión.
    const userIdValue = s.userId || s.UserId || s.id || s.Id || null;
    const identificationValue = s.identification || s.Identification || s.cedula || s.Cedula || s.nationalId || s.NationalId || s.identificationNumber || s.IdentificationNumber || null;

    // Helper simple para escribir texto en un elemento si existe
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
    setText('detUserId', userIdValue || '-');
    setText('detIdentification', identificationValue || '-');
    setText("detName", fullName || "-");
    setText("detEmail", email || "-");
    setText("detRole", role || "-");
    setText("detPhone", s.phone || s.Phone || s.phoneNumber || s.PhoneNumber || "-");

    // Asignar foto de perfil si la sesión trae una URL.
    const imgEl = document.getElementById("profilePhoto");
    const photo = s.photoUrl || s.profilePhoto || s.PhotoUrl || s.ProfilePhoto || null;
    if (imgEl && photo) imgEl.src = photo;
});
