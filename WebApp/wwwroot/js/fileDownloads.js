// Cliente común para descargas binarias generadas y auditadas por la Web API.
(function () {
    "use strict";

    function withCaller(endpoint) {
        const callerUserId = parseInt(session.getUserId() || 0);
        const separator = endpoint.includes("?") ? "&" : "?";
        return endpoint + separator + "callerUserId=" + encodeURIComponent(callerUserId);
    }

    function fileNameFromDisposition(disposition, fallback) {
        if (!disposition) return fallback;
        const utfMatch = disposition.match(/filename\*=UTF-8''([^;]+)/i);
        if (utfMatch) return decodeURIComponent(utfMatch[1]);
        const regularMatch = disposition.match(/filename="?([^";]+)"?/i);
        return regularMatch ? regularMatch[1] : fallback;
    }

    function downloadBlob(blob, fileName) {
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = fileName;
        anchor.style.display = "none";
        document.body.appendChild(anchor);
        anchor.click();
        anchor.remove();
        setTimeout(function () { URL.revokeObjectURL(url); }, 0);
    }

    async function request(endpoint, body, fallbackFileName) {
        const response = await fetch(apiClient.url(withCaller(endpoint)), {
            method: "POST",
            headers: Object.assign({ "Content-Type": "application/json" }, apiClient.authHeader()),
            body: JSON.stringify(body)
        });

        if (!response.ok) {
            const raw = await response.text();
            let message = raw || `HTTP ${response.status}`;
            try {
                const parsed = JSON.parse(raw);
                message = parsed.message || parsed.Message || message;
            } catch (_) { }
            const error = new Error(message);
            error.status = response.status;
            throw error;
        }

        const fileName = fileNameFromDisposition(response.headers.get("Content-Disposition"), fallbackFileName || "descarga_sgde");
        downloadBlob(await response.blob(), fileName);
        return fileName;
    }

    function exportTable(options) {
        const format = String(options.format || "CSV").toUpperCase();
        const extension = format === "EXCEL" ? "xlsx" : format.toLowerCase();
        const baseName = String(options.fileName || "reporte_sgde").replace(/\.[^.]+$/, "");
        const headers = (options.headers || []).map(value => String(value ?? ""));
        const rows = (options.rows || []).map(row =>
            (row || []).map(value => String(value ?? ""))
        );
        return request("FileExports/Download", {
            title: options.title || "Reporte SGDE",
            fileName: baseName,
            format: format,
            headers,
            rows
        }, `${baseName}.${extension}`);
    }

    function downloadInvoice(statementId, format) {
        const normalized = String(format || "CSV").toUpperCase();
        const extension = normalized === "EXCEL" || normalized === "XLSX" ? "xlsx" : normalized.toLowerCase();
        return request("Invoices/Export", {
            statementId: parseInt(statementId),
            format: normalized
        }, `EstadoCuenta_${statementId}.${extension}`);
    }

    window.fileDownloads = { request, exportTable, downloadInvoice };
})();
