// AdminStatementsViewController.js (§85 Admin/Statements) - Listado, anulación, regeneración y exportación de estados de cuenta
document.addEventListener("DOMContentLoaded", function () {
    const statementsBody = document.getElementById("statementsBody");
    if (!statementsBody) return;

    const searchInput = document.getElementById("stmtSearch");
    const statusFilter = document.getElementById("stmtStatus");
    let allStatements = [];
    let userNames = {};
    let selectedStatementId = null;

    const annulModalEl = document.getElementById("annulModal");
    const annulModal = annulModalEl ? new bootstrap.Modal(annulModalEl) : null;
    const regenModalEl = document.getElementById("regenModal");
    const regenModal = regenModalEl ? new bootstrap.Modal(regenModalEl) : null;
    const exportModalEl = document.getElementById("exportModal");
    const exportModal = exportModalEl ? new bootstrap.Modal(exportModalEl) : null;

    apiClient.get("Users/RetrieveAll").done(function (res) {
        (res?.data || res?.Data || []).forEach(function (u) {
            userNames[u.id || u.Id] = `${u.firstName || u.FirstName || ""} ${u.lastName || u.LastName || ""}`.trim() || `Usuario #${u.id || u.Id}`;
        });
    }).always(loadStatements);

    if (searchInput) searchInput.addEventListener("input", renderFiltered);
    if (statusFilter) statusFilter.addEventListener("change", renderFiltered);

    function loadStatements() {
        statementsBody.innerHTML = '<tr><td colspan="10" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando estados de cuenta...</td></tr>';
        apiClient.get("Billing/Statements")
            .done(function (res) {
                allStatements = res?.data || res?.Data || [];
                renderFiltered();
            })
            .fail(function (xhr) {
                statementsBody.innerHTML = '<tr><td colspan="10" class="text-center text-danger">Error al cargar los estados de cuenta.</td></tr>';
                handleApiError(xhr);
            });
    }

    function renderFiltered() {
        const q = (searchInput?.value || "").toLowerCase().trim();
        const status = statusFilter?.value || "";
        const filtered = allStatements.filter(function (s) {
            const buyerName = (userNames[s.buyerId ?? s.BuyerId] || "").toLowerCase();
            const matchesQ = !q || buyerName.includes(q);
            const matchesStatus = !status || (s.status || s.Status) === status;
            return matchesQ && matchesStatus;
        });
        render(filtered);
    }

    function render(items) {
        if (!items.length) {
            statementsBody.innerHTML = '<tr><td colspan="10" class="text-center text-muted">No se encontraron estados de cuenta.</td></tr>';
            return;
        }
        statementsBody.innerHTML = items.map(function (s) {
            const id = s.id ?? s.Id;
            const buyerId = s.buyerId ?? s.BuyerId;
            const status = s.status || s.Status || "-";
            const badge = status === "Issued" ? "bg-success" : status === "Annulled" ? "bg-danger" : "bg-secondary";
            const canAnnul = status === "Issued";
            const canRegen = status === "Annulled";
            return `<tr>
                <td>${id}</td>
                <td>${escapeHtml(userNames[buyerId] || `Comprador #${buyerId}`)}</td>
                <td>${s.month ?? s.Month}/${s.year ?? s.Year}</td>
                <td>${Number(s.assignedMWh ?? s.AssignedMWh ?? 0).toLocaleString("es-CR", { minimumFractionDigits: 2 })}</td>
                <td>${Number(s.subtotal ?? s.Subtotal ?? 0).toLocaleString("es-CR", { minimumFractionDigits: 2 })}</td>
                <td>${Number(s.taxAmount ?? s.TaxAmount ?? 0).toLocaleString("es-CR", { minimumFractionDigits: 2 })}</td>
                <td class="fw-bold">${Number(s.total ?? s.Total ?? 0).toLocaleString("es-CR", { minimumFractionDigits: 2 })}</td>
                <td><span class="badge ${badge}">${status}</span></td>
                <td>${s.revisionNumber ?? s.RevisionNumber ?? 0}</td>
                <td class="text-nowrap">
                    ${canAnnul ? `<button class="btn btn-sm btn-outline-danger btn-annul" data-id="${id}" title="Anular"><i class="bi bi-x-circle"></i></button>` : ""}
                    ${canRegen ? `<button class="btn btn-sm btn-outline-warning btn-regen" data-id="${id}" title="Regenerar"><i class="bi bi-arrow-repeat"></i></button>` : ""}
                    <button class="btn btn-sm btn-outline-primary btn-export" data-id="${id}" title="Exportar"><i class="bi bi-download"></i></button>
                </td>
            </tr>`;
        }).join("");

        statementsBody.querySelectorAll(".btn-annul").forEach(function (btn) {
            btn.addEventListener("click", function () {
                selectedStatementId = btn.getAttribute("data-id");
                document.getElementById("annulReason").value = "";
                annulModal?.show();
            });
        });
        statementsBody.querySelectorAll(".btn-regen").forEach(function (btn) {
            btn.addEventListener("click", function () {
                selectedStatementId = btn.getAttribute("data-id");
                regenModal?.show();
            });
        });
        statementsBody.querySelectorAll(".btn-export").forEach(function (btn) {
            btn.addEventListener("click", function () {
                selectedStatementId = btn.getAttribute("data-id");
                exportModal?.show();
            });
        });
    }

    const confirmAnnulBtn = document.getElementById("confirmAnnulBtn");
    if (confirmAnnulBtn) {
        confirmAnnulBtn.addEventListener("click", function () {
            const reason = document.getElementById("annulReason")?.value.trim();
            if (!reason) {
                notify.warning("El motivo de anulación es obligatorio.");
                return;
            }
            confirmAnnulBtn.disabled = true;
            const original = confirmAnnulBtn.innerHTML;
            confirmAnnulBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';
            apiClient.post("Billing/AnnulStatement", { statementId: parseInt(selectedStatementId), reason: reason })
                .done(function () {
                    notify.success("Estado de cuenta anulado.");
                    annulModal?.hide();
                    loadStatements();
                })
                .fail(function (xhr) { handleApiError(xhr); })
                .always(function () { confirmAnnulBtn.disabled = false; confirmAnnulBtn.innerHTML = original; });
        });
    }

    const confirmRegenBtn = document.getElementById("confirmRegenBtn");
    if (confirmRegenBtn) {
        confirmRegenBtn.addEventListener("click", function () {
            confirmRegenBtn.disabled = true;
            const original = confirmRegenBtn.innerHTML;
            confirmRegenBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';
            apiClient.post("Billing/RegenerateStatement", { originalStatementId: parseInt(selectedStatementId) })
                .done(function () {
                    notify.success("Estado de cuenta regenerado con una nueva revisión.");
                    regenModal?.hide();
                    loadStatements();
                })
                .fail(function (xhr) { handleApiError(xhr); })
                .always(function () { confirmRegenBtn.disabled = false; confirmRegenBtn.innerHTML = original; });
        });
    }

    const confirmExportBtn = document.getElementById("confirmExportBtn");
    if (confirmExportBtn) {
        confirmExportBtn.addEventListener("click", function () {
            const format = document.getElementById("exportFormat")?.value || "CSV";
            confirmExportBtn.disabled = true;
            const original = confirmExportBtn.innerHTML;
            confirmExportBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';

            $.ajax({
                url: apiClient.url("Billing/Export"),
                method: "POST",
                headers: apiClient.authHeader(),
                contentType: "application/json",
                data: JSON.stringify({ statementId: parseInt(selectedStatementId), format: format }),
                xhrFields: { responseType: "blob" }
            }).done(function (blob) {
                const ext = format === "EXCEL" ? "xlsx" : format.toLowerCase();
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement("a");
                a.href = url;
                a.download = `EstadoCuenta_${selectedStatementId}.${ext}`;
                document.body.appendChild(a);
                a.click();
                a.remove();
                window.URL.revokeObjectURL(url);
                notify.success("Estado de cuenta exportado.");
                exportModal?.hide();
            }).fail(function (xhr) {
                handleApiError(xhr);
            }).always(function () {
                confirmExportBtn.disabled = false;
                confirmExportBtn.innerHTML = original;
            });
        });
    }
});
