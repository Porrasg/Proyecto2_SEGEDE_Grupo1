/**
 * ProfileViewController.js
 * Versión simple: carga los datos del usuario desde sessionStorage (window.session.get())
 * y los muestra en la vista sin llamar a la API. Evita errores 500 cuando la API
 * devuelve id inválidos. Si no hay sesión válida redirige a Login.
 */
document.addEventListener("DOMContentLoaded", function () {
    const hasSession = window.session && typeof window.session.isAuthenticated === "function"
        ? window.session.isAuthenticated() && !window.session.isExpired()
        : false;

    if (!hasSession) {
        window.location.href = `/Login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
        return;
    }

    // Obtener datos desde la sesión local
    let s = null;
    try {
        s = typeof window.session.get === "function"
            ? window.session.get()
            : (sessionStorage.getItem("sgde_session") ? JSON.parse(sessionStorage.getItem("sgde_session")) : null);
    } catch (e) {
        s = null;
    }

    if (!s) {
        window.location.href = `/Login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
        return;
    }

    const email = s.email || s.Email || "";
    const role = s.role || s.Role || "";
    const name = s.name || s.fullName || s.FullName || email || "Usuario SGDE";

    function setText(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = value;
    }

    // Poblar campos visibles
    setText("profName", name || "-");
    setText("profEmail", email || "-");
    setText("profRole", role || "-");
    setText("detName", name || "-");
    setText("detEmail", email || "-");
    setText("detRole", role || "-");
    setText("detPhone", s.phone || s.Phone || s.phoneNumber || s.PhoneNumber || "-");
    setText("profId", s.userId || s.UserId || s.id || s.Id || "-");
    setText("profDate", (s.createdAt || s.CreatedAt) ? new Date(s.createdAt || s.CreatedAt).toLocaleDateString("es-CR") : "-");

    const imgEl = document.getElementById("profilePhoto");
    const photo = s.photoUrl || s.profilePhoto || s.PhotoUrl || s.ProfilePhoto || null;
    if (imgEl && photo) imgEl.src = photo;
});
