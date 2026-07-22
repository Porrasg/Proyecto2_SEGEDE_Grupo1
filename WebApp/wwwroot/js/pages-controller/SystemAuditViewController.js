// SystemAuditViewController.js (§22.1, §27) - Controlador para Auditoría Técnica del Sistema (WORM / RN-030)
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando SystemAuditViewController...");

    const token = session.getToken();
    const role = session.getRole();

    if (!token || (role !== "Engineer" && role !== "Administrator" && role !== "Admin")) {
        notify.error("Acceso denegado. Requiere privilegios de Ingeniero o Administrador.");
        setTimeout(() => {
            window.location.href = "/Login";
        }, 1500);
        return;
    }

    const auditBody = document.getElementById("engAuditBody");
    if (auditBody) {
        loadAuditLogs();

        const btnFilter = document.getElementById("btnEngFilterAudit");
        if (btnFilter) {
            btnFilter.addEventListener("click", function () {
                loadAuditLogs();
            });
        }
    }

    function loadAuditLogs() {
        auditBody.innerHTML = '<tr><td colspan="7" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando bitácora inmutable (WORM)...</td></tr>';

        const mod = document.getElementById("engAuditModule")?.value || "";
        const userStr = document.getElementById("engAuditUser")?.value.trim() || "";
        const fromDate = document.getElementById("engAuditFrom")?.value;
        const toDate = document.getElementById("engAuditTo")?.value;

        let endpoint = `Audits/ByModule?module=${encodeURIComponent(mod)}&callerRole=${encodeURIComponent(role)}&page=1&pageSize=50`;

        if (fromDate && toDate) {
            endpoint = `Audits/ByDateRange?from=${encodeURIComponent(fromDate)}&to=${encodeURIComponent(toDate)}&callerRole=${encodeURIComponent(role)}&page=1&pageSize=50`;
        } else if (userStr && !isNaN(userStr)) {
            endpoint = `Audits/ByUser?userId=${parseInt(userStr)}&page=1&pageSize=50`;
        }

        apiClient.get(endpoint).done(function (res) {
            let list = (res?.data?.items || res?.Data?.Items || res?.data || res?.Data || []);
            
            // Si el usuario escribió texto (ej: nombre de usuario) en el filtro rápido y no era un ID numérico, filtramos en memoria
            if (userStr && isNaN(userStr)) {
                list = list.filter(item => (item.userName || item.UserName || "").toLowerCase().includes(userStr.toLowerCase()));
            }

            renderAuditTable(list);
        }).fail(function (xhr) {
            auditBody.innerHTML = '<tr><td colspan="7" class="text-center text-danger">Error al obtener registros de auditoría. Verifique sus permisos (RN-030).</td></tr>';
            handleApiError(xhr);
        });
    }

    function renderAuditTable(list) {
        if (!list.length) {
            auditBody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">No se encontraron eventos en la bitácora para los criterios seleccionados.</td></tr>';
            return;
        }

        auditBody.innerHTML = list.map(a => {
            const id = a.id || a.Id;
            const dateStr = a.eventDate || a.EventDate ? new Date(a.eventDate || a.EventDate).toLocaleString("es-CR") : "-";
            const user = a.userName || a.UserName || "System";
            const mod = a.module || a.Module || "-";
            const act = a.action || a.Action || "-";
            const ent = (a.affectedEntity || a.AffectedEntity || "-") + " #" + (a.entityId || a.EntityId || 0);

            let actBadge = '<span class="badge bg-secondary">' + act + '</span>';
            if (act.toLowerCase() === "create") actBadge = '<span class="badge bg-success">Creación</span>';
            if (act.toLowerCase() === "update") actBadge = '<span class="badge bg-info text-dark">Edición</span>';
            if (act.toLowerCase() === "logicaldelete" || act.toLowerCase() === "delete") actBadge = '<span class="badge bg-danger">Baja / Anulación</span>';
            if (act.toLowerCase() === "execute") actBadge = '<span class="badge bg-primary">Ejecución ACID</span>';

            let modBadge = `<span class="badge bg-dark">${mod}</span>`;
            if (mod === "Turbines") modBadge = '<span class="badge bg-primary">Turbinas</span>';
            if (mod === "Maintenances") modBadge = '<span class="badge bg-info text-dark">Mantenimientos</span>';
            if (mod === "Failures") modBadge = '<span class="badge bg-danger">Averías</span>';
            if (mod === "CentralBank") modBadge = '<span class="badge bg-warning text-dark">Banco Central</span>';
            if (mod === "Forecasts") modBadge = '<span class="badge bg-success">Pronósticos</span>';
            if (mod === "Billing") modBadge = '<span class="badge bg-success">Facturación</span>';

            const prev = formatJsonVal(a.previousValue || a.PreviousValue);
            const next = formatJsonVal(a.newValue || a.NewValue);
            let detail = "-";
            if (prev !== "-" || next !== "-") {
                detail = `<span class="text-muted">${prev}</span> <i class="bi bi-arrow-right small text-primary"></i> <span class="fw-bold">${next}</span>`;
            }

            return `
                <tr>
                    <td>#${id}</td>
                    <td class="small">${dateStr}</td>
                    <td class="fw-bold">${user}</td>
                    <td>${modBadge}</td>
                    <td>${actBadge}</td>
                    <td class="small font-monospace">${ent}</td>
                    <td class="small">${detail}</td>
                </tr>
            `;
        }).join("");
    }

    function formatJsonVal(val) {
        if (!val || val === "null" || val === "None") return "-";
        try {
            const obj = JSON.parse(val);
            return typeof obj === "object" ? JSON.stringify(obj) : obj;
        } catch (e) {
            return val;
        }
    }
});
