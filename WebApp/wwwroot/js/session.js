// session.js (§24.2) - Módulo de gestión de sesión en el navegador
const session = (function () {
    const KEY = 'sgde_session';

    // Almacena la respuesta del inicio de sesión exitoso en sessionStorage
    function save(loginResponse) { 
        sessionStorage.setItem(KEY, JSON.stringify(loginResponse)); 
    }

    // Recupera el objeto completo de la sesión actual
    function get() { 
        const r = sessionStorage.getItem(KEY); 
        return r ? JSON.parse(r) : null; 
    }

    // Indica si existe una sesión válida en el navegador
    function isAuthenticated() {
        return !!get();
    }

    // Devuelve el rol del usuario actual (ej. "Admin", "Engineer", "Buyer")
    function getRole() { 
        const s = get(); 
        return s?.role ?? s?.Role ?? null; 
    }

    // Devuelve el identificador único del usuario (userId)
    function getUserId() { 
        const s = get(); 
        return s?.userId ?? s?.UserId ?? s?.id ?? s?.Id ?? null;
    }

    // Devuelve el correo electrónico asociado a la sesión
    function getEmail() {
        const s = get();
        return s?.email ?? s?.Email ?? null;
    }

    // Devuelve el JWT de sesión emitido por Users/ValidateLoginOtp (null si no hay,
    // por ejemplo con las cuentas estáticas que todavía se saltan el OTP).
    function getToken() {
        const s = get();
        return s?.token ?? s?.Token ?? null;
    }

    // Verifica si la sesión ha caducado comparando la fecha de expiración
    function isExpired() { 
        const s = get(); 
        if (!s) return true;
        const exp = s.expiration ?? s.Expiration;
        if (!exp) return false;
        return new Date(exp) < new Date(); 
    }

    // Limpia la sesión del almacenamiento web
    function clear() { 
        sessionStorage.removeItem(KEY); 
    }

    return { save, get, getRole, getUserId, getEmail, getToken, isExpired, isAuthenticated, clear };
})();

// Exponer en el objeto global con inicial minúscula y mayúscula por compatibilidad
window.session = session;
window.Session = session;
