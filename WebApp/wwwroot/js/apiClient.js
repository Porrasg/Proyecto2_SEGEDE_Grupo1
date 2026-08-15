// apiClient.js (§24.1) - Cliente HTTP modular para comunicación AJAX con la Web API.
// Implementado sobre fetch() nativo (sin jQuery) para controlar directamente las
// peticiones AJAX, según lo pedido en la revisión. Se expone una interfaz
// .done()/.fail()/.always() compatible con la que ya consumen las ~20 pantallas
// de wwwroot/js/pages-controller/, para no tener que reescribir cada llamada.
const apiClient = (function () {
    // Para trabajar local o en web
    const isLocal =
        window.location.hostname === "localhost" ||
        window.location.hostname === "127.0.0.1";

    // Local usa la API local; Azure usa la API publicada
    const configuredBase = window.SGDE_API_BASE || (isLocal
        ? "https://localhost:7236/api/"
        : "https://segede-api-ebdtf4escubpbmf3.centralus-01.azurewebsites.net/api/");
    const BASE = configuredBase.endsWith("/") ? configuredBase : configuredBase + "/";

    // Construye la URL completa sumando la ruta relativa al endpoint base
    function url(path) {
        const cleanPath = path.startsWith('/') ? path.substring(1) : path;
        return BASE + cleanPath;
    }

    // Agrega .done()/.fail()/.always() sobre una Promise nativa, replicando la
    // forma en que se consumía el jqXHR de jQuery (múltiples callbacks encadenables,
    // cada uno recibe el valor resuelto o el error rechazado).
    function withCallbackShim(promise) {
        // Las suscripciones de compatibilidad observan una promesa que siempre se
        // resuelve. Así, un .done() seguido de .fail() no crea promesas rechazadas
        // secundarias sin manejador, pero el objeto sigue siendo await/then-able.
        const settled = promise.then(
            value => ({ ok: true, value }),
            error => ({ ok: false, value: error })
        );

        function invokeSafely(cb, value) {
            try { cb(value); }
            catch (error) { console.error("[SGDE apiClient] Error en callback", error); }
        }

        const facade = {
            done(cb) {
                settled.then(result => { if (result.ok) invokeSafely(cb, result.value); });
                return facade;
            },
            fail(cb) {
                settled.then(result => { if (!result.ok) invokeSafely(cb, result.value); });
                return facade;
            },
            always(cb) {
                settled.then(result => invokeSafely(cb, result.value));
                return facade;
            },
            then(onFulfilled, onRejected) { return promise.then(onFulfilled, onRejected); },
            catch(onRejected) { return promise.catch(onRejected); },
            finally(onFinally) { return promise.finally(onFinally); }
        };

        return facade;
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
            headers: { 'Content-Type': 'application/json' },
            body: body ? JSON.stringify(body) : undefined
        })
            .catch(function () {
                // fetch() solo rechaza la promesa ante un fallo de red/CORS real (sin respuesta del servidor) - jamás por un status HTTP de error.
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

    // Normaliza las formas de lista usadas históricamente por la API:
    // arreglo directo, { data: [...] }, { Data: [...] } y wrappers paginados.
    function unwrapList(response) {
        if (Array.isArray(response)) return response;

        const candidate = response?.data ?? response?.Data ?? response?.items ?? response?.Items ?? response?.result ?? response?.Result;
        if (Array.isArray(candidate)) return candidate;
        if (Array.isArray(candidate?.items)) return candidate.items;
        if (Array.isArray(candidate?.Items)) return candidate.Items;
        if (Array.isArray(candidate?.data)) return candidate.data;
        if (Array.isArray(candidate?.Data)) return candidate.Data;
        return [];
    }

    return {
        url,
        unwrapList,
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
