// AdminAuditViewController.js (§85 Admin/Audit) - Bitácora WORM con filtros por módulo/usuario/rango de fechas
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
    const prevEl = document.getElementById("audPrev");
    const newEl = document.getElementById("audNew");

    let allLogs = [];

    const today = new Date();
    const monthAgo = new Date(today.getTime() - 30 * 24 * 60 * 60 * 1000);
    if (fromInput) fromInput.value = monthAgo.toISOString().slice(0, 10);
    if (toInput) toInput.value = today.toISOString().slice(0, 10);

    loadAudit();
    if (filterBtn) filterBtn.addEventListener("click", loadAudit);

    function loadAudit() {
        auditBody.innerHTML = '<tr><td colspan="8" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando registros...</td></tr>';
        const from = fromInput?.value ? new Date(fromInput.value).toISOString() : monthAgo.toISOString();
        const to = toInput?.value ? new Date(toInput.value + "T23:59:59").toISOString() : today.toISOString();

        apiClient.get(`Audits/ByDateRange?from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}&page=1&pageSize=200`)
            .done(function (res) {
                allLogs = res?.data?.items || res?.Data?.Items || [];
                renderFiltered();
            })
            .fail(function (xhr) {
                auditBody.innerHTML = '<tr><td colspan="8" class="text-center text-danger">Error al cargar la bitácora de auditoría.</td></tr>';
                handleApiError(xhr);
            });
    }

    function renderFiltered() {
        const moduleVal = moduleSelect?.value || "";
        const userVal = (userInput?.value || "").toLowerCase().trim();
        const filtered = allLogs.filter(function (l) {
            const matchesModule = !moduleVal || (l.module || l.Module) === moduleVal;
            const userName = (l.userName || l.UserName || "").toLowerCase();
            const userId = String(l.userId ?? l.UserId ?? "");
            const matchesUser = !userVal || userName.includes(userVal) || userId === userVal;
            return matchesModule && matchesUser;
        });
        render(filtered);
    }

    function render(items) {
        if (!items.length) {
            auditBody.innerHTML = '<tr><td colspan="8" class="text-center text-muted">Sin registros de auditoría para los filtros aplicados.</td></tr>';
            return;
        }
        auditBody.innerHTML = items.map(function (l, idx) {
            return `<tr>
                <td>${l.id ?? l.Id}</td>
                <td>${new Date(l.eventDate || l.EventDate).toLocaleString("es-CR")}</td>
                <td>${escapeHtml(l.userName || l.UserName || "Sistema")}</td>
                <td><span class="badge bg-secondary">${l.module || l.Module || "-"}</span></td>
                <td>${l.action || l.Action || "-"}</td>
                <td>${l.affectedEntity || l.AffectedEntity || "-"} #${l.entityId ?? l.EntityId ?? "-"}</td>
                <td>${(l.isColdArchive ?? l.IsColdArchive) ? '<span class="badge bg-dark">Archivo Frío</span>' : '<span class="badge bg-light text-dark border">Activo</span>'}</td>
                <td><button class="btn btn-sm btn-outline-secondary btn-audit-detail" data-idx="${idx}"><i class="bi bi-eye"></i></button></td>
            </tr>`;
        }).join("");

        const currentItems = items;
        auditBody.querySelectorAll(".btn-audit-detail").forEach(function (btn) {
            btn.addEventListener("click", function () {
                const item = currentItems[parseInt(btn.getAttribute("data-idx"))];
                if (prevEl) prevEl.textContent = item.previousValue || item.PreviousValue || "(sin valor previo)";
                if (newEl) newEl.textContent = item.newValue || item.NewValue || "(sin valor nuevo)";
                detailModal?.show();
            });
        });
    }
});
