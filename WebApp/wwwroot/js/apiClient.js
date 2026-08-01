// apiClient.js (§24.1) - Cliente HTTP modular para comunicación AJAX con la Web API.
// Implementado sobre fetch() nativo (sin jQuery) para controlar directamente las
// peticiones AJAX, según lo pedido en la revisión. Se expone una interfaz
// .done()/.fail()/.always() compatible con la que ya consumen las ~20 pantallas
// de wwwroot/js/pages-controller/, para no tener que reescribir cada llamada.
const apiClient = (function () {
    // Obtiene la base URL configurada en el servidor (Configuration["ApiBaseUrl"], ver
    // _Layout.cshtml) o cae al valor por defecto de desarrollo local si no está configurada.
    const BASE = window.SGDE_API_BASE || "http://localhost:5052/api/";

    // Construye la URL completa sumando la ruta relativa al endpoint base
    function url(path) {
        const cleanPath = path.startsWith('/') ? path.substring(1) : path;
        return BASE + cleanPath;
    }

    // No se envía encabezado de autorización porque este proyecto usa sesión por usuario autenticado
    function authHeader() {
        return {};
    }

    // Agrega .done()/.fail()/.always() sobre una Promise nativa, replicando la
    // forma en que se consumía el jqXHR de jQuery (múltiples callbacks encadenables,
    // cada uno recibe el valor resuelto o el error rechazado).
    function withCallbackShim(promise) {
        promise.done = function (cb) { promise.then(cb); return promise; };
        promise.fail = function (cb) { promise.catch(cb); return promise; };
        promise.always = function (cb) { promise.then(cb, cb); return promise; };
        return promise;
    }

    // Realiza una petición asíncrona con fetch() y normaliza la respuesta/error a la
    // misma forma que ya esperan las pantallas (xhr.status, xhr.responseJSON,
    // xhr.responseText), en particular el manejador global handleApiError()
    // (WebApp/wwwroot/js/notifications.js).
    function request(method, path, body) {
        const fullUrl = url(path);

        function buildError(message, status, statusText, responseJSON, responseText) {
            const err = new Error(message);
            err.status = status;
            err.statusText = statusText;
            err.responseJSON = responseJSON;
            err.responseText = responseText;
            err.sgdeMethod = method;
            err.sgdePath = path;
            err.sgdeUrl = fullUrl;
            return err;
        }

        const promise = fetch(fullUrl, {
            method: method,
            headers: Object.assign({ 'Content-Type': 'application/json' }, authHeader()),
            body: body ? JSON.stringify(body) : undefined
        })
            .catch(function () {
                // fetch() solo rechaza la promesa ante un fallo de red/CORS real (sin
                // respuesta del servidor) - jamás por un status HTTP de error.
                console.warn('[SGDE apiClient] ' + method + ' ' + fullUrl + ' → HTTP 0 (sin respuesta — la Web API no está disponible o hay un problema de CORS/red)');
                throw buildError('Sin respuesta del servidor', 0, '', null, '');
            })
            .then(function (response) {
                return response.text().then(function (text) {
                    let data = null;
                    if (text) {
                        try { data = JSON.parse(text); } catch (e) { data = null; }
                    }

                    if (!response.ok) {
                        console.warn('[SGDE apiClient] ' + method + ' ' + fullUrl + ' → HTTP ' + response.status);
                        const message = (data && (data.message || data.Message)) || response.statusText || ('HTTP ' + response.status);
                        throw buildError(message, response.status, response.statusText, data, text);
                    }

                    return data;
                });
            });

        return withCallbackShim(promise);
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
