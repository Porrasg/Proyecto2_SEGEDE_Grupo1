// apiClient.js (§24.1) - Cliente HTTP modular para comunicación AJAX con la Web API
const apiClient = (function () {
    // Obtiene la base URL configurada en el servidor o utiliza el valor por defecto
    const BASE = window.SGDE_API_BASE || (window.location.protocol === 'http:' ? "http://localhost:5052/api/" : "https://localhost:7236/api/");

    // Construye la URL completa sumando la ruta relativa al endpoint base
    function url(path) { 
        const cleanPath = path.startsWith('/') ? path.substring(1) : path;
        return BASE + cleanPath; 
    }

    // No se envía encabezado de autorización porque este proyecto usa sesión por usuario autenticado
    function authHeader() {
        return {};
    }

    // Realiza una petición asíncrona utilizando jQuery AJAX con soporte de autenticación y JSON
    function request(method, path, body) {
        const fullUrl = url(path);
        const jqXHR = $.ajax({
            url: fullUrl,
            method: method,
            contentType: 'application/json',
            headers: authHeader(),
            data: body ? JSON.stringify(body) : null
        });

        // Metadatos de diagnóstico adjuntos al jqXHR para que cualquier .fail() aguas abajo
        // (incluido handleApiError) sepa qué endpoint falló, sin tener que repetirlo en cada llamada.
        jqXHR.sgdeMethod = method;
        jqXHR.sgdePath = path;
        jqXHR.sgdeUrl = fullUrl;

        // Registro centralizado en consola de toda petición fallida (ruta + estado HTTP),
        // independiente de si la página que originó la llamada maneja o no el error visualmente.
        jqXHR.fail(function (xhr) {
            const status = xhr.status || 0;
            const hint = status === 0 ? ' (sin respuesta — la Web API no está disponible o hay un problema de CORS/red)' : '';
            console.warn('[SGDE apiClient] ' + method + ' ' + fullUrl + ' → HTTP ' + status + hint);
        });

        return jqXHR;
    }

    return {
        url,
        authHeader,
        // Ejecuta petición GET asíncrona para obtener recursos
        get: p => request('GET', p),
        // Ejecuta petición POST asíncrona para crear recursos o procesar acciones
        post: (p, b) => request('POST', p, b),
        // Ejecuta petición PUT asíncrona para modificar recursos existentes
        put: (p, b) => request('PUT', p, b),
        // Ejecuta petición DELETE asíncrona para eliminar recursos
        delete: (p, b) => request('DELETE', p, b)
    };
})();

// Exponer en el objeto global con inicial minúscula y mayúscula por compatibilidad
window.apiClient = apiClient;
window.ApiClient = apiClient;
