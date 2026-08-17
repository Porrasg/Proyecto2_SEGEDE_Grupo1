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
            const users = apiClient.unwrapList(res);
            users.forEach(function (u) {
                userNames[u.id || u.Id] = u.firstName || u.FirstName ? `${u.firstName || u.FirstName} ${u.firstLastName || u.FirstLastName || ""}`.trim() : `Usuario #${u.id || u.Id}`;
            });
        });
    }

    // Entities-DTOs no tiene un "Scenario" (Sufficient/Shortage/...) ni un batch agregado con mes/año:
    // Distribution es una fila por comprador (DistributionBatchId, RequestedEnergyMWh, AssignedEnergyMWh,
    // UnassignedEnergyMWh, DistributionDate, Status). Este badge usa el Status real que ya calcula
    // DistributionManager.Create ("Completed" si se asignó el 100%, "Partial" en caso contrario).
    function statusBadge(status) {
        const map = { Completed: "bg-success", Partial: "bg-warning text-dark", Cancelled: "bg-secondary" };
        const txt = { Completed: "Cubierta", Partial: "Escasez (Prorrateo)", Cancelled: "Cancelada" };
        return `<span class="badge ${map[status] || "bg-secondary"}">${txt[status] || status || "-"}</span>`;
    }

    function consultDistribution() {
        const month = parseInt(monthSelect.value);
        const year = parseInt(yearInput?.value || now.getFullYear());

        if (detailsBody) detailsBody.innerHTML = '<tr><td colspan="5" class="text-center"><span class="spinner-border spinner-border-sm"></span> Consultando...</td></tr>';

        apiClient.get(`Forecasts/ByMonth?month=${month}&year=${year}`)
            .done(function (forecastRes) {

                const forecasts = apiClient.unwrapList(forecastRes);

                // Obtener los ForecastId correspondientes al período seleccionado
                const forecastIds = forecasts.map(function (f) {
                    return f.id ?? f.Id;
                });

                apiClient.get("Distributions/RetrieveAll")
                    .done(function (res) {

                        const items = apiClient.unwrapList(res);

                        // Una distribución pertenece al período según su Forecast,
                        // no según la fecha en que fue ejecutada.
                        const rowsInPeriod = items.filter(function (d) {

                            const forecastId =
                                d.forecastId ??
                                d.ForecastId;

                            return forecastIds.includes(forecastId);
                        });

                        if (!rowsInPeriod.length) {
                            if (totalDemandEl) totalDemandEl.textContent = "- MWh";
                            if (availInvEl) availInvEl.textContent = "- MWh";
                            if (scenarioEl) scenarioEl.innerHTML =
                                '<span class="badge bg-secondary">-</span>';
                            if (dateEl) dateEl.textContent = "-";

                            if (detailsBody) {
                                detailsBody.innerHTML =
                                    '<tr><td colspan="5" class="text-center text-muted">Sin distribución ejecutada para este mes.</td></tr>';
                            }

                            return;
                        }

                        const totalDemand = rowsInPeriod.reduce(
                            (s, d) =>
                                s + Number(
                                    d.requestedEnergyMWh ??
                                    d.RequestedEnergyMWh ??
                                    0
                                ),
                            0
                        );

                        const totalAssigned = rowsInPeriod.reduce(
                            (s, d) =>
                                s + Number(
                                    d.assignedEnergyMWh ??
                                    d.AssignedEnergyMWh ??
                                    0
                                ),
                            0
                        );

                        const batchId =
                            rowsInPeriod[0].distributionBatchId ??
                            rowsInPeriod[0].DistributionBatchId;

                        const executionDate =
                            rowsInPeriod[0].distributionDate ??
                            rowsInPeriod[0].DistributionDate;

                        const batchStatus = rowsInPeriod.some(
                            d => (d.status ?? d.Status) === "Partial"
                        )
                            ? "Partial"
                            : "Completed";

                        if (totalDemandEl) {
                            totalDemandEl.textContent =
                                totalDemand.toLocaleString("es-CR", {
                                    minimumFractionDigits: 2
                                }) + " MWh";
                        }

                        if (availInvEl) {
                            availInvEl.textContent =
                                totalAssigned.toLocaleString("es-CR", {
                                    minimumFractionDigits: 2
                                }) + " MWh";
                        }

                        if (scenarioEl) {
                            scenarioEl.innerHTML = statusBadge(batchStatus);
                        }

                        if (dateEl) {
                            dateEl.textContent = executionDate
                                ? new Date(executionDate).toLocaleString("es-CR")
                                : "-";
                        }

                        loadDetails(batchId);
                    })
                    .fail(function (xhr) {
                        if (detailsBody) {
                            detailsBody.innerHTML =
                                '<tr><td colspan="5" class="text-center text-danger">Error al consultar la distribución.</td></tr>';
                        }

                        handleApiError(xhr);
                    });
            })
            .fail(function (xhr) {
                if (detailsBody) {
                    detailsBody.innerHTML =
                        '<tr><td colspan="5" class="text-center text-danger">Error al consultar los pronósticos del período.</td></tr>';
                }

                handleApiError(xhr);
            });
    }

    function loadDetails(distributionBatchId) {
        if (!detailsBody) return;
        detailsBody.innerHTML = '<tr><td colspan="5" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando detalle...</td></tr>';

        apiClient.get("Distributions/RetrieveByBatchId/" + distributionBatchId)
            .done(function (res) {
                const items = apiClient.unwrapList(res);
                if (!items.length) {
                    detailsBody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Sin asignaciones registradas para esta distribución.</td></tr>';
                    return;
                }
                detailsBody.innerHTML = items.map(function (d) {
                    const requested = Number(d.requestedEnergyMWh ?? d.RequestedEnergyMWh ?? 0);
                    const assigned = Number(d.assignedEnergyMWh ?? d.AssignedEnergyMWh ?? 0);
                    const unassigned = Number(d.unassignedEnergyMWh ?? d.UnassignedEnergyMWh ?? 0);
                    const pct = requested > 0 ? (assigned / requested) * 100 : 100;
                    const buyerId = d.buyerId ?? d.BuyerId;
                    return `<tr>
                        <td>${escapeHtml(userNames[buyerId] || `Comprador #${buyerId}`)}</td>
                        <td>${requested.toLocaleString("es-CR", { minimumFractionDigits: 2 })}</td>
                        <td>${assigned.toLocaleString("es-CR", { minimumFractionDigits: 2 })}</td>
                        <td>${unassigned.toLocaleString("es-CR", { minimumFractionDigits: 2 })}</td>
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

                const callerId = window.session?.getUserId();
                let endpoint = `Distributions/ExecuteMonthly?year=${year}&month=${month}`;
                if (callerId != null) endpoint += "&callerUserId=" + encodeURIComponent(callerId);

                apiClient.post(endpoint)
                    .done(function (res) {
                        notify.success(res?.message || "Distribución ejecutada con éxito.");
                        consultDistribution();
                    })
                    .fail(function (xhr) {
                        handleApiError(xhr);
                    })
                    .always(function () {
                        executeBtn.disabled = false;
                        executeBtn.innerHTML = original;
                    });
            });
        });
    }
});
