// AdminMaintenanceFailureViewController.js (§85, tras enlazar Admin/Maintenances y Admin/Failures en el navbar) - Vistas generales de solo lectura
document.addEventListener("DOMContentLoaded", function () {
    const readList = function (response) {
        return apiClient.unwrapList ? apiClient.unwrapList(response) : (Array.isArray(response) ? response : []);
    };
    
    // 1. VISIÓN GENERAL DE MANTENIMIENTOS (/Admin/Maintenances)
    
    const maintBody = document.getElementById("maintsOverviewBody");
    if (maintBody) {
        let allMaintenances = [];
        let turbineCodes = {};

        const statusFilter = document.getElementById("maintStatus");
        const typeFilter = document.getElementById("maintType");
        if (statusFilter) statusFilter.addEventListener("change", renderMaintFiltered);
        if (typeFilter) typeFilter.addEventListener("change", renderMaintFiltered);

        apiClient.get("Turbines/RetrieveAll").done(function (res) {
            readList(res).forEach(function (t) {
                const id = t.id ?? t.Id;
                turbineCodes[id] = t.code || t.Code || `#${id}`;
            });
        }).always(loadMaintenances);

        // Aviso de cumplimiento: turbinas sin mantenimiento agendado en el mes en curso
        // (obligatoriedad mensual de la rúbrica).
        const complianceAlert = document.getElementById("maintComplianceAlert");
        const complianceAlertText = document.getElementById("maintComplianceAlertText");
        if (complianceAlert && complianceAlertText) {
            apiClient.get("Maintenances/ComplianceAlert").done(function (res) {
                const turbines = res?.data || res?.Data || [];
                if (turbines.length > 0) {
                    const codes = turbines.map(t => t.code || t.Code || `#${t.id || t.Id}`).join(", ");
                    complianceAlertText.textContent =
                        `${turbines.length} turbina(s) sin mantenimiento agendado este mes: ${codes}.`;
                    complianceAlert.classList.remove("d-none");
                }
            });
        }

        function loadMaintenances() {
            maintBody.innerHTML = '<tr><td colspan="7" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando mantenimientos...</td></tr>';
            apiClient.get("Maintenances/RetrieveAll")
                .done(function (res) {
                    allMaintenances = readList(res);
                    renderMaintFiltered();
                })
                .fail(function (xhr) {
                    maintBody.innerHTML = '<tr><td colspan="7" class="text-center text-danger">Error al cargar los mantenimientos.</td></tr>';
                    handleApiError(xhr);
                });
        }

        function renderMaintFiltered() {
            const status = statusFilter?.value || "";
            const type = typeFilter?.value || "";
            const filtered = allMaintenances.filter(function (m) {
                return (!status || (m.status || m.Status) === status) && (!type || (m.maintenanceType || m.MaintenanceType) === type);
            });
            renderMaintenances(filtered);
            window.maintenanceCalendar?.setData(filtered, turbineCodes);
        }

        function renderMaintenances(items) {
            if (!items.length) {
                maintBody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">Sin mantenimientos registrados.</td></tr>';
                return;
            }
            const badge = { Scheduled: "bg-info text-dark", InProgress: "bg-warning text-dark", Completed: "bg-success", Cancelled: "bg-secondary" };
            const statusLabels = { Scheduled: "Programado", InProgress: "En Progreso", Completed: "Completado", Cancelled: "Cancelado" };
            const typeLabels = { Preventive: "Preventivo", Corrective: "Correctivo", Predictive: "Predictivo", Inspection: "Inspección", Emergency: "Emergencia" };
            maintBody.innerHTML = items.map(function (m) {
                const status = m.status || m.Status || "-";
                const type = m.maintenanceType || m.MaintenanceType || "-";
                const turbineId = m.turbineId ?? m.TurbineId;
                return `<tr>
                    <td>${escapeHtml(m.id ?? m.Id)}</td>
                    <td>${escapeHtml(turbineCodes[turbineId] || `#${turbineId}`)}</td>
                    <td><span class="badge bg-secondary">${escapeHtml(typeLabels[type] || type)}</span></td>
                    <td>${new Date(m.estimatedStartDate || m.EstimatedStartDate).toLocaleDateString("es-CR")}</td>
                    <td>${new Date(m.estimatedEndDate || m.EstimatedEndDate).toLocaleDateString("es-CR")}</td>
                    <td><span class="badge ${badge[status] || "bg-secondary"}">${escapeHtml(statusLabels[status] || status)}</span></td>
                    <td>${escapeHtml(m.result || m.Result || "-")}</td>
                </tr>`;
            }).join("");
        }
    }

   
    // 2. INFORMES DE FALLAS (/Admin/Failures)
  
    const failBody = document.getElementById("failsOverviewBody");
    if (failBody) {
        let allFailures = [];
        let filteredFailures = [];
        let turbineCodes = {};

        const severityFilter = document.getElementById("failSeverity");
        const statusFilter = document.getElementById("failStatus");
        const searchInput = document.getElementById("failSearch");
        if (severityFilter) severityFilter.addEventListener("change", renderFailFiltered);
        if (statusFilter) statusFilter.addEventListener("change", renderFailFiltered);
        if (searchInput) searchInput.addEventListener("input", renderFailFiltered);

        apiClient.get("Turbines/RetrieveAll").done(function (res) {
            readList(res).forEach(function (t) {
                const id = t.id ?? t.Id;
                turbineCodes[id] = t.code || t.Code || `#${id}`;
            });
        }).always(loadFailures);

        function loadFailures() {
            failBody.innerHTML = '<tr><td colspan="8" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando reportes de fallas...</td></tr>';
            apiClient.get("Failures/RetrieveAll")
                .done(function (res) {
                    allFailures = readList(res);
                    renderFailFiltered();
                })
                .fail(function (xhr) {
                    failBody.innerHTML = '<tr><td colspan="8" class="text-center text-danger">Error al cargar las fallas.</td></tr>';
                    handleApiError(xhr);
                });
        }

        function renderFailFiltered() {
            const sev = severityFilter?.value || "";
            const status = statusFilter?.value || "";
            const query = (searchInput?.value || "").trim().toLowerCase();
            filteredFailures = allFailures.filter(function (failure) {
                const turbineId = failure.turbineId ?? failure.TurbineId;
                const searchable = `${turbineCodes[turbineId] || turbineId} ${failure.description || failure.Description || ""} ${failure.resolution || failure.Resolution || ""}`.toLowerCase();
                return (!sev || (failure.severity || failure.Severity) === sev) &&
                    (!status || (failure.status || failure.Status) === status) &&
                    (!query || searchable.includes(query));
            });
            renderFailures(filteredFailures);
            renderFailureKpis();

            const csvButton = document.getElementById("btnExportFailuresCsv");
            const pdfButton = document.getElementById("btnExportFailuresPdf");
            if (csvButton) csvButton.disabled = filteredFailures.length === 0;
            if (pdfButton) pdfButton.disabled = filteredFailures.length === 0;
        }

        function renderFailureKpis() {
            const resolved = allFailures.filter(f => (f.status || f.Status) === "Resolved").length;
            const setText = (id, value) => { const element = document.getElementById(id); if (element) element.textContent = String(value); };
            setText("failureKpiTotal", allFailures.length);
            setText("failureKpiCritical", allFailures.filter(f => (f.severity || f.Severity) === "Critical").length);
            setText("failureKpiResolved", resolved);
            setText("failureKpiOpen", allFailures.length - resolved - allFailures.filter(f => (f.status || f.Status) === "Cancelled").length);
        }

        function renderFailures(items) {
            if (!items.length) {
                failBody.innerHTML = '<tr><td colspan="8" class="text-center text-muted">Sin fallas para los filtros seleccionados.</td></tr>';
                return;
            }
            const severityBadges = { Low: "bg-info text-dark", Medium: "bg-warning text-dark", High: "bg-danger-subtle text-danger", Critical: "bg-danger" };
            const severityLabels = { Low: "Baja", Medium: "Media", High: "Alta", Critical: "Crítica" };
            const statusBadges = { Reported: "bg-warning text-dark", InProgress: "bg-info text-dark", Resolved: "bg-success", Cancelled: "bg-secondary" };
            const statusLabels = { Reported: "Reportada", InProgress: "En progreso", Resolved: "Resuelta", Cancelled: "Cancelada" };
            failBody.innerHTML = items.map(function (f) {
                const sev = f.severity || f.Severity || "-";
                const status = f.status || f.Status || "Reported";
                const turbineId = f.turbineId ?? f.TurbineId;
                return `<tr>
                    <td>${escapeHtml(f.id ?? f.Id)}</td>
                    <td>${escapeHtml(turbineCodes[turbineId] || `#${turbineId}`)}</td>
                    <td><span class="badge ${severityBadges[sev] || "bg-secondary"}">${escapeHtml(severityLabels[sev] || sev)}</span></td>
                    <td><span class="badge ${statusBadges[status] || "bg-secondary"}">${escapeHtml(statusLabels[status] || status)}</span></td>
                    <td>${escapeHtml(f.description || f.Description || "-")}</td>
                    <td>${escapeHtml(f.resolution || f.Resolution || "Pendiente")}</td>
                    <td>#${f.engineerId ?? f.EngineerId ?? "—"}</td>
                    <td>${new Date(f.failureDate || f.FailureDate).toLocaleString("es-CR")}</td>
                </tr>`;
            }).join("");
        }

        function exportFailures(format) {
            const rows = filteredFailures.map(function (failure) {
                const turbineId = failure.turbineId ?? failure.TurbineId;
                return [
                    failure.id ?? failure.Id,
                    turbineCodes[turbineId] || `#${turbineId}`,
                    failure.severity || failure.Severity || "",
                    failure.status || failure.Status || "",
                    failure.description || failure.Description || "",
                    failure.resolution || failure.Resolution || "",
                    failure.engineerId ?? failure.EngineerId ?? "",
                    new Date(failure.failureDate || failure.FailureDate).toLocaleString("es-CR")
                ];
            });

            fileDownloads.exportTable({
                title: "Reporte técnico de averías e incidencias",
                fileName: "reporte_tecnico_fallas",
                format,
                headers: ["ID", "Turbina", "Severidad", "Estado", "Descripción", "Resolución", "Ingeniero", "Fecha"],
                rows
            }).then(function (fileName) {
                notify.success("Reporte exportado: " + fileName);
            }).catch(function (error) {
                notify.error("No se pudo exportar el reporte: " + error.message);
            });
        }

        document.getElementById("btnExportFailuresCsv")?.addEventListener("click", function () { exportFailures("CSV"); });
        document.getElementById("btnExportFailuresPdf")?.addEventListener("click", function () { exportFailures("PDF"); });
    }
});
