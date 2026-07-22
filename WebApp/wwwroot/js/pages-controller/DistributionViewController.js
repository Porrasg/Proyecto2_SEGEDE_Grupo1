// DistributionViewController.js (§46, §85 Admin/Distribution) - Ejecución y consulta de la distribución comercial mensual
document.addEventListener("DOMContentLoaded", function () {
    const monthSelect = document.getElementById("distMonth");
    if (!monthSelect) return;

    const yearInput = document.getElementById("distYear");
    const consultBtn = document.getElementById("btnConsultDist");
    const executeBtn = document.getElementById("btnExecuteDist");
    const detailsBody = document.getElementById("distDetailsBody");
    const totalDemandEl = document.getElementById("distTotalDemand");
    const availInvEl = document.getElementById("distAvailInv");
    const scenarioEl = document.getElementById("distScenario");
    const dateEl = document.getElementById("distDate");

    const now = new Date();
    monthSelect.value = String(now.getMonth() + 1);
    if (yearInput) yearInput.value = String(now.getFullYear());

    let userNames = {};
    loadUserNames().always(consultDistribution);

    function loadUserNames() {
        return apiClient.get("Users/RetrieveAll").done(function (res) {
            const users = res?.data || res?.Data || [];
            users.forEach(function (u) {
                userNames[u.id || u.Id] = u.firstName || u.FirstName ? `${u.firstName || u.FirstName} ${u.lastName || u.LastName || ""}`.trim() : `Usuario #${u.id || u.Id}`;
            });
        });
    }

    function scenarioBadge(scenario) {
        const map = {
            Sufficient: "bg-success", Shortage: "bg-warning text-dark",
            ZeroInventory: "bg-danger", ZeroDemand: "bg-secondary"
        };
        return `<span class="badge ${map[scenario] || "bg-secondary"}">${scenario || "-"}</span>`;
    }

    function consultDistribution() {
        const month = parseInt(monthSelect.value);
        const year = parseInt(yearInput?.value || now.getFullYear());

        if (detailsBody) detailsBody.innerHTML = '<tr><td colspan="5" class="text-center"><span class="spinner-border spinner-border-sm"></span> Consultando...</td></tr>';

        apiClient.get("Distributions/History")
            .done(function (res) {
                const items = res?.data || res?.Data || [];
                const dist = items.find(function (d) { return (d.month || d.Month) === month && (d.year || d.Year) === year; });

                if (!dist) {
                    if (totalDemandEl) totalDemandEl.textContent = "- MWh";
                    if (availInvEl) availInvEl.textContent = "- MWh";
                    if (scenarioEl) scenarioEl.innerHTML = '<span class="badge bg-secondary">-</span>';
                    if (dateEl) dateEl.textContent = "-";
                    if (detailsBody) detailsBody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Sin distribución ejecutada para este mes.</td></tr>';
                    return;
                }

                if (totalDemandEl) totalDemandEl.textContent = Number(dist.totalDemand ?? dist.TotalDemand ?? 0).toLocaleString("es-CR", { minimumFractionDigits: 2 }) + " MWh";
                if (availInvEl) availInvEl.textContent = Number(dist.availableInventory ?? dist.AvailableInventory ?? 0).toLocaleString("es-CR", { minimumFractionDigits: 2 }) + " MWh";
                if (scenarioEl) scenarioEl.innerHTML = scenarioBadge(dist.scenario || dist.Scenario);
                if (dateEl) dateEl.textContent = new Date(dist.executionDate || dist.ExecutionDate).toLocaleString("es-CR");

                loadDetails(dist.id || dist.Id);
            })
            .fail(function (xhr) {
                if (detailsBody) detailsBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Error al consultar la distribución.</td></tr>';
                handleApiError(xhr);
            });
    }

    function loadDetails(distributionId) {
        if (!detailsBody) return;
        detailsBody.innerHTML = '<tr><td colspan="5" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando detalle...</td></tr>';

        apiClient.get("Distributions/Detail/" + distributionId)
            .done(function (res) {
                const items = res?.data || res?.Data || [];
                if (!items.length) {
                    detailsBody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Sin asignaciones registradas para esta distribución.</td></tr>';
                    return;
                }
                detailsBody.innerHTML = items.map(function (d) {
                    const requested = Number(d.requestedMWh ?? d.RequestedMWh ?? 0);
                    const assigned = Number(d.assignedMWh ?? d.AssignedMWh ?? 0);
                    const unsupplied = Number(d.unsuppliedDemand ?? d.UnsuppliedDemand ?? 0);
                    const pct = requested > 0 ? (assigned / requested) * 100 : 100;
                    const buyerId = d.buyerId ?? d.BuyerId;
                    return `<tr>
                        <td>${escapeHtml(userNames[buyerId] || `Comprador #${buyerId}`)}</td>
                        <td>${requested.toLocaleString("es-CR", { minimumFractionDigits: 2 })}</td>
                        <td>${assigned.toLocaleString("es-CR", { minimumFractionDigits: 2 })}</td>
                        <td>${unsupplied.toLocaleString("es-CR", { minimumFractionDigits: 2 })}</td>
                        <td>${pct.toFixed(1)}%</td>
                    </tr>`;
                }).join("");
            })
            .fail(function (xhr) {
                detailsBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Error al cargar el detalle.</td></tr>';
                handleApiError(xhr);
            });
    }

    if (consultBtn) consultBtn.addEventListener("click", consultDistribution);

    if (executeBtn) {
        executeBtn.addEventListener("click", function () {
            const month = parseInt(monthSelect.value);
            const year = parseInt(yearInput?.value || now.getFullYear());

            notify.confirm(`¿Ejecutar la distribución comercial de ${month}/${year}? Esta acción cierra el período, genera estados de cuenta y no se puede deshacer.`, { dangerous: true, confirmText: "Ejecutar distribución" }).then(function (ok) {
                if (!ok) return;
                executeBtn.disabled = true;
                const original = executeBtn.innerHTML;
                executeBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Ejecutando...';

                apiClient.post(`Distributions/ExecuteMonthly?month=${month}&year=${year}`, {})
                    .done(function () {
                        notify.success("Distribución comercial ejecutada con éxito.");
                        consultDistribution();
                    })
                    .fail(function (xhr) { handleApiError(xhr); })
                    .always(function () {
                        executeBtn.disabled = false;
                        executeBtn.innerHTML = original;
                    });
            });
        });
    }
});
