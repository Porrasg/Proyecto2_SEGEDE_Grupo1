// AdminAuditViewController.js (§85 Admin/Audit) - Bitácora inmutable con filtros por módulo/usuario/rango de fechas
document.addEventListener("DOMContentLoaded", function () {
    const auditBody = document.getElementById("auditBody");
    if (!auditBody) return;

    const moduleSelect = document.getElementById("auditModule");
    const userInput = document.getElementById("auditUser");
    const fromInput = document.getElementById("auditFrom");
    const toInput = document.getElementById("auditTo");
    const filterBtn = document.getElementById("btnFilterAudit");
    const detailModalEl = document.getElementById("auditModal");
    const detailModal = detailModalEl ? new bootstrap.Modal(detailModalEl) : null;
    const descEl = document.getElementById("audDesc");

    let allLogs = [];
    // Entities-DTOs.Audit solo guarda UserId (nullable), no un UserName resuelto — se arma el
    // mismo diccionario id→nombre que usan las demás vistas (Distribution/Forecasts/Statements).
    let userNames = {};

    const today = new Date();
    const monthAgo = new Date(today.getTime() - 30 * 24 * 60 * 60 * 1000);
    if (fromInput) fromInput.value = monthAgo.toISOString().slice(0, 10);
    if (toInput) toInput.value = today.toISOString().slice(0, 10);

    apiClient.get("Users/RetrieveAll").done(function (res) {
        const users = Array.isArray(res) ? res : (res?.data || res?.Data || []);
        users.forEach(function (u) {
            userNames[u.id ?? u.Id] = `${u.firstName || u.FirstName || ""} ${u.firstLastName || u.FirstLastName || ""}`.trim() || `Usuario #${u.id ?? u.Id}`;
        });
    }).always(loadAudit);

    if (filterBtn) filterBtn.addEventListener("click", loadAudit);

    function loadAudit() {
        auditBody.innerHTML = '<tr><td colspan="7" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando registros...</td></tr>';
        const from = fromInput?.value ? new Date(fromInput.value).toISOString() : monthAgo.toISOString();
        const to = toInput?.value ? new Date(toInput.value + "T23:59:59").toISOString() : today.toISOString();

        apiClient.get(`Audits/ByDateRange?from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}&page=1&pageSize=200`)
            .done(function (res) {
                allLogs = res?.data?.items || res?.Data?.Items || [];
                renderFiltered();
            })
            .fail(function (xhr) {
                auditBody.innerHTML = '<tr><td colspan="7" class="text-center text-danger">Error al cargar la bitácora de auditoría.</td></tr>';
                handleApiError(xhr);
            });
    }

    function renderFiltered() {
        const moduleVal = moduleSelect?.value || "";
        const userVal = (userInput?.value || "").toLowerCase().trim();
        const filtered = allLogs.filter(function (l) {
            // Entities-DTOs.Audit no distingue "módulo" de "entidad afectada" — es el mismo campo EntityName.
            const matchesModule = !moduleVal || (l.entityName || l.EntityName) === moduleVal;
            const userId = l.userId ?? l.UserId;
            const userName = (userNames[userId] || "").toLowerCase();
            const matchesUser = !userVal || userName.includes(userVal) || String(userId ?? "") === userVal;
            return matchesModule && matchesUser;
        });
        render(filtered);
    }

    function render(items) {
        if (!items.length) {
            auditBody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">Sin registros de auditoría para los filtros aplicados.</td></tr>';
            return;
        }
        auditBody.innerHTML = items.map(function (l, idx) {
            const userId = l.userId ?? l.UserId;
            const entityName = l.entityName || l.EntityName || "-";
            return `<tr>
                <td>${l.id ?? l.Id}</td>
                <td>${new Date(l.createdAt || l.CreatedAt).toLocaleString("es-CR")}</td>
                <td>${escapeHtml(userId != null ? (userNames[userId] || `Usuario #${userId}`) : "Sistema")}</td>
                <td><span class="badge bg-secondary">${escapeHtml(entityName)}</span></td>
                <td>${l.action || l.Action || "-"}</td>
                <td>${escapeHtml(entityName)} #${l.entityId ?? l.EntityId ?? "-"}</td>
                <td><button class="btn btn-sm btn-outline-secondary btn-audit-detail" data-idx="${idx}"><i class="bi bi-eye"></i></button></td>
            </tr>`;
        }).join("");

        const currentItems = items;
        auditBody.querySelectorAll(".btn-audit-detail").forEach(function (btn) {
            btn.addEventListener("click", function () {
                const item = currentItems[parseInt(btn.getAttribute("data-idx"))];
                if (descEl) descEl.textContent = item.description || item.Description || "(sin descripción)";
                detailModal?.show();
            });
        });
    }
});
