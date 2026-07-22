// AdminExportsViewController.js (§85 Admin/Exports) - Bitácora de evidencia de exportaciones (WORM, solo lectura)
document.addEventListener("DOMContentLoaded", function () {
    const exportsBody = document.getElementById("exportsBody");
    if (!exportsBody) return;

    const searchInput = document.getElementById("exportSearch");
    let allLogs = [];
    let userNames = {};
    let userRoles = {};

    apiClient.get("Users/RetrieveAll").done(function (res) {
        (res?.data || res?.Data || []).forEach(function (u) {
            const id = u.id || u.Id;
            userNames[id] = `${u.firstName || u.FirstName || ""} ${u.lastName || u.LastName || ""}`.trim() || `Usuario #${id}`;
            userRoles[id] = u.role || u.Role || "-";
        });
    }).always(loadExports);

    if (searchInput) searchInput.addEventListener("input", renderFiltered);

    function loadExports() {
        exportsBody.innerHTML = '<tr><td colspan="6" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando bitácora de exportaciones...</td></tr>';
        apiClient.get("Billing/ExportLogs")
            .done(function (res) {
                allLogs = (res?.data || res?.Data || []).slice().sort(function (a, b) {
                    return new Date(b.eventDate || b.EventDate) - new Date(a.eventDate || a.EventDate);
                });
                renderFiltered();
            })
            .fail(function (xhr) {
                exportsBody.innerHTML = '<tr><td colspan="6" class="text-center text-danger">Error al cargar la bitácora de exportaciones.</td></tr>';
                handleApiError(xhr);
            });
    }

    function renderFiltered() {
        const q = (searchInput?.value || "").toLowerCase().trim();
        const filtered = !q ? allLogs : allLogs.filter(function (l) {
            const name = (userNames[l.userId ?? l.UserId] || "").toLowerCase();
            const format = (l.format || l.Format || "").toLowerCase();
            return name.includes(q) || format.includes(q);
        });
        render(filtered);
    }

    function render(items) {
        if (!items.length) {
            exportsBody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Sin exportaciones registradas.</td></tr>';
            return;
        }
        exportsBody.innerHTML = items.map(function (l) {
            const userId = l.userId ?? l.UserId;
            return `<tr>
                <td>${l.id ?? l.Id}</td>
                <td>${new Date(l.eventDate || l.EventDate).toLocaleString("es-CR")}</td>
                <td>${escapeHtml(userNames[userId] || `Usuario #${userId}`)}</td>
                <td>${userRoles[userId] || "-"}</td>
                <td>${l.documentType || l.DocumentType || "-"} #${l.documentId ?? l.DocumentId ?? "-"}</td>
                <td><span class="badge bg-info text-dark">${l.format || l.Format || "-"}</span></td>
            </tr>`;
        }).join("");
    }
});
