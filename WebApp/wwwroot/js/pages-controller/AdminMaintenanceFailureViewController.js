// AdminMaintenanceFailureViewController.js (§85, tras enlazar Admin/Maintenances y Admin/Failures en el navbar) - Vistas generales de solo lectura
document.addEventListener("DOMContentLoaded", function () {
    // ==========================================
    // 1. VISIÓN GENERAL DE MANTENIMIENTOS (/Admin/Maintenances)
    // ==========================================
    const maintBody = document.getElementById("maintsOverviewBody");
    if (maintBody) {
        let allMaintenances = [];
        let turbineCodes = {};

        const statusFilter = document.getElementById("maintStatus");
        const typeFilter = document.getElementById("maintType");
        if (statusFilter) statusFilter.addEventListener("change", renderMaintFiltered);
        if (typeFilter) typeFilter.addEventListener("change", renderMaintFiltered);

        apiClient.get("Turbines/RetrieveAll").done(function (res) {
            (res?.data || res?.Data || []).forEach(function (t) {
                turbineCodes[t.id] = t.code || t.Code || `#${t.id}`;
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
                    allMaintenances = res?.data || res?.Data || [];
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
                    <td>${m.id ?? m.Id}</td>
                    <td>${turbineCodes[turbineId] || `#${turbineId}`}</td>
                    <td><span class="badge bg-secondary">${typeLabels[type] || type}</span></td>
                    <td>${new Date(m.estimatedStartDate || m.EstimatedStartDate).toLocaleDateString("es-CR")}</td>
                    <td>${new Date(m.estimatedEndDate || m.EstimatedEndDate).toLocaleDateString("es-CR")}</td>
                    <td><span class="badge ${badge[status] || "bg-secondary"}">${statusLabels[status] || status}</span></td>
                    <td>${escapeHtml(m.result || m.Result || "-")}</td>
                </tr>`;
            }).join("");
        }
    }

    // ==========================================
    // 2. INFORMES DE FALLAS (/Admin/Failures)
    // ==========================================
    const failBody = document.getElementById("failsOverviewBody");
    if (failBody) {
        let allFailures = [];
        let turbineCodes = {};

        const severityFilter = document.getElementById("failSeverity");
        if (severityFilter) severityFilter.addEventListener("change", renderFailFiltered);

        apiClient.get("Turbines/RetrieveAll").done(function (res) {
            (res?.data || res?.Data || []).forEach(function (t) {
                turbineCodes[t.id] = t.code || t.Code || `#${t.id}`;
            });
        }).always(loadFailures);

        function loadFailures() {
            failBody.innerHTML = '<tr><td colspan="5" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando reportes de fallas...</td></tr>';
            apiClient.get("Failures/RetrieveAll")
                .done(function (res) {
                    allFailures = res?.data || res?.Data || [];
                    renderFailFiltered();
                })
                .fail(function (xhr) {
                    failBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Error al cargar las fallas.</td></tr>';
                    handleApiError(xhr);
                });
        }

        function renderFailFiltered() {
            const sev = severityFilter?.value || "";
            const filtered = !sev ? allFailures : allFailures.filter(function (f) { return (f.severityLevel || f.SeverityLevel) === sev; });
            renderFailures(filtered);
        }

        function renderFailures(items) {
            if (!items.length) {
                failBody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Sin fallas reportadas.</td></tr>';
                return;
            }
            failBody.innerHTML = items.map(function (f) {
                const sev = f.severityLevel || f.SeverityLevel || "-";
                const turbineId = f.turbineId ?? f.TurbineId;
                return `<tr>
                    <td>${f.id ?? f.Id}</td>
                    <td>${turbineCodes[turbineId] || `#${turbineId}`}</td>
                    <td><span class="badge ${sev === "Critical" ? "bg-danger" : "bg-warning text-dark"}">${sev}</span></td>
                    <td>${escapeHtml(f.description || f.Description || "-")}</td>
                    <td>${new Date(f.failureDate || f.FailureDate).toLocaleString("es-CR")}</td>
                </tr>`;
            }).join("");
        }
    }
});
