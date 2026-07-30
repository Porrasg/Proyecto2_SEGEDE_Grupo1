// SystemAuditViewController.js (§22.1, §27) - Controlador para Auditoría Técnica del Sistema (registro inmutable / RN-030)
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando SystemAuditViewController...");

    const role = session.getRole();

    if (role !== "Engineer" && role !== "Administrator" && role !== "Admin") {
        notify.error("Acceso denegado. Requiere privilegios de Ingeniero o Administrador.");
        setTimeout(() => {
            window.location.href = "/Login";
        }, 1500);
        return;
    }

    const auditBody = document.getElementById("engAuditBody");
    // Entities-DTOs.Audit solo guarda UserId (nullable), no un UserName resuelto.
    let userNames = {};
    if (auditBody) {
        apiClient.get("Users/RetrieveAll").done(function (res) {
            (res?.data || res?.Data || []).forEach(function (u) {
                userNames[u.id ?? u.Id] = `${u.firstName || u.FirstName || ""} ${u.firstLastName || u.FirstLastName || ""}`.trim() || `Usuario #${u.id ?? u.Id}`;
            });
        }).always(loadAuditLogs);

        const btnFilter = document.getElementById("btnEngFilterAudit");
        if (btnFilter) {
            btnFilter.addEventListener("click", function () {
                loadAuditLogs();
            });
        }
    }

    function loadAuditLogs() {
        auditBody.innerHTML = '<tr><td colspan="7" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando bitácora inmutable...</td></tr>';

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
                list = list.filter(item => (userNames[item.userId ?? item.UserId] || "").toLowerCase().includes(userStr.toLowerCase()));
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
            const dateStr = a.createdAt || a.CreatedAt ? new Date(a.createdAt || a.CreatedAt).toLocaleString("es-CR") : "-";
            const userId = a.userId ?? a.UserId;
            const user = userId != null ? (userNames[userId] || `Usuario #${userId}`) : "System";
            // Entities-DTOs.Audit no distingue "módulo" de "entidad afectada" — es el mismo campo EntityName.
            const mod = a.entityName || a.EntityName || "-";
            const act = a.action || a.Action || "-";
            const ent = mod + " #" + (a.entityId || a.EntityId || 0);

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

            // Entities-DTOs.Audit no tiene PreviousValue/NewValue — se muestra la Description tal cual.
            const detail = escapeHtml(a.description || a.Description || "-");

            return `
                <tr>
                    <td>#${id}</td>
                    <td class="small">${dateStr}</td>
                    <td class="fw-bold">${escapeHtml(user)}</td>
                    <td>${modBadge}</td>
                    <td>${actBadge}</td>
                    <td class="small font-monospace">${ent}</td>
                    <td class="small">${detail}</td>
                </tr>
            `;
        }).join("");
    }
});
