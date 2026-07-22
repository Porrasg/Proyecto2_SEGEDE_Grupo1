// ReportsViewController.js - Módulo de Reportes por perfil (Admin / Engineer / Buyer).
// Reutiliza exclusivamente endpoints existentes (regla de la adenda v3: sin caminos paralelos):
//   Admin:    Turbines/All + Energy/GenerationHistory/{id} + Maintenances/All|ByTurbine
//   Engineer: Turbines/All (estado/alertas) + Maintenances/All|ByTurbine
//   Buyer:    Distributions/ByBuyer/{id} + Forecasts/ByBuyer/{id} (join por forecastId para mes/año)
document.addEventListener("DOMContentLoaded", function () {
    const MONTHS = ["", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"];
    let charts = {};

    // ── Helpers compartidos ─────────────────────────────────────────────

    function esc(s) {
        return String(s ?? "").replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
    }

    function fmt(n, dec = 4) {
        return Number(n ?? 0).toLocaleString("es-CR", { minimumFractionDigits: dec, maximumFractionDigits: dec });
    }

    function fmtDate(d) {
        if (!d) return "—";
        const dt = new Date(d);
        return isNaN(dt) ? "—" : dt.toLocaleString("es-CR", { dateStyle: "short", timeStyle: "short" });
    }

    // Descarga CSV en el cliente (BOM UTF-8 para que Excel muestre acentos correctamente).
    function downloadCsv(filename, headers, rows) {
        const lines = [headers.join(";")].concat(rows.map(r => r.map(v => {
            const s = String(v ?? "");
            return /[;"\n]/.test(s) ? '"' + s.replace(/"/g, '""') + '"' : s;
        }).join(";")));
        const blob = new Blob(["﻿" + lines.join("\r\n")], { type: "text/csv;charset=utf-8;" });
        const a = document.createElement("a");
        a.href = URL.createObjectURL(blob);
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(a.href);
        if (typeof notify !== "undefined") notify.success("Reporte exportado: " + filename);
    }

    function renderChart(canvasId, type, labels, datasets) {
        const canvas = document.getElementById(canvasId);
        if (!canvas || typeof Chart === "undefined") return;
        if (charts[canvasId]) charts[canvasId].destroy();
        charts[canvasId] = new Chart(canvas, {
            type: type,
            data: { labels, datasets },
            options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: "bottom" } } }
        });
        canvas.parentElement.style.minHeight = "320px";
    }

    // Íconos por estado de turbina — texto + ícono, nunca solo color (accesibilidad).
    function turbineStatusBadge(status) {
        const map = {
            "Active": { cls: "bg-success", icon: "bi-check-circle", txt: "Activa" },
            "Maintenance": { cls: "bg-warning text-dark", icon: "bi-tools", txt: "En Mantenimiento" },
            "Damaged": { cls: "bg-danger", icon: "bi-exclamation-triangle", txt: "Dañada" },
            "Inactive": { cls: "bg-secondary", icon: "bi-pause-circle", txt: "Suspendida" },
            "Decommissioned": { cls: "bg-dark", icon: "bi-x-circle", txt: "Dada de Baja" }
        };
        const m = map[status] || { cls: "bg-secondary", icon: "bi-question-circle", txt: status };
        return `<span class="badge ${m.cls}"><i class="bi ${m.icon} me-1" aria-hidden="true"></i>${esc(m.txt)}</span>`;
    }

    function maintStatusBadge(status) {
        const map = {
            "Scheduled": { cls: "bg-info text-dark", icon: "bi-calendar-event", txt: "Programado" },
            "InProgress": { cls: "bg-warning text-dark", icon: "bi-arrow-repeat", txt: "En Progreso" },
            "Completed": { cls: "bg-success", icon: "bi-check-circle", txt: "Completado" },
            "Cancelled": { cls: "bg-secondary", icon: "bi-x-circle", txt: "Cancelado" }
        };
        const m = map[status] || { cls: "bg-secondary", icon: "bi-question-circle", txt: status };
        return `<span class="badge ${m.cls}"><i class="bi ${m.icon} me-1" aria-hidden="true"></i>${esc(m.txt)}</span>`;
    }

    function loadTurbinesInto(selectEl, includeAllOption) {
        return apiClient.get("Turbines/All").done(function (res) {
            const list = res?.data || res?.Data || [];
            selectEl.innerHTML = includeAllOption ? '<option value="">Todas las turbinas</option>' : "";
            list.forEach(t => {
                const opt = document.createElement("option");
                opt.value = t.id ?? t.Id;
                opt.textContent = `${t.code ?? t.Code} — ${t.name ?? t.Name}`;
                selectEl.appendChild(opt);
            });
            return list;
        });
    }

    // Trae TODAS las páginas del historial de generación de una turbina (pageSize alto para minimizar round-trips).
    function fetchAllGeneration(turbineId) {
        const PAGE_SIZE = 1000;
        function page(n, acc) {
            return apiClient.get(`Energy/GenerationHistory/${turbineId}?page=${n}&pageSize=${PAGE_SIZE}`).then(function (res) {
                const d = res?.data || res?.Data || {};
                const items = d.items || d.Items || [];
                acc = acc.concat(items);
                const totalPages = d.totalPages ?? d.TotalPages ?? 1;
                return n < totalPages ? page(n + 1, acc) : acc;
            });
        }
        return page(1, []);
    }

    // ── Reporte compartido Admin/Engineer: mantenimientos por turbina ──

    const maintBody = document.getElementById("maintReportBody");
    if (maintBody) {
        const sel = document.getElementById("repMaintTurbineSelect");
        const btnRun = document.getElementById("btnRunMaint");
        const btnExport = document.getElementById("btnExportMaint");
        let turbineNames = {};
        let lastRows = [];

        loadTurbinesInto(sel, true).done(function (res) {
            (res?.data || res?.Data || []).forEach(t => { turbineNames[t.id ?? t.Id] = `${t.code ?? t.Code} — ${t.name ?? t.Name}`; });
        });

        btnRun?.addEventListener("click", function () {
            const tid = sel.value;
            maintBody.innerHTML = '<tr><td colspan="8" class="text-center py-4"><span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Generando reporte...</td></tr>';
            const req = tid ? apiClient.get("Maintenances/ByTurbine/" + tid) : apiClient.get("Maintenances/All");
            req.done(function (res) {
                const list = res?.data || res?.Data || [];
                if (!list.length) {
                    maintBody.innerHTML = '<tr><td colspan="8" class="text-center text-muted py-4">No hay mantenimientos registrados para el criterio seleccionado.</td></tr>';
                    btnExport.disabled = true;
                    return;
                }
                lastRows = list;
                maintBody.innerHTML = list.map(m => `<tr>
                    <td>${m.id ?? m.Id}</td>
                    <td>${esc(turbineNames[m.turbineId ?? m.TurbineId] || ("Turbina #" + (m.turbineId ?? m.TurbineId)))}</td>
                    <td>${(m.maintenanceType ?? m.MaintenanceType) === "Preventive" ? "Preventivo" : "Correctivo"}</td>
                    <td>${fmtDate(m.estimatedStartDate ?? m.EstimatedStartDate)}</td>
                    <td>${fmtDate(m.estimatedEndDate ?? m.EstimatedEndDate)}</td>
                    <td>${fmtDate(m.actualEndDate ?? m.ActualEndDate)}</td>
                    <td>${maintStatusBadge(m.status ?? m.Status)}</td>
                    <td>${esc(m.result ?? m.Result ?? "—")}</td>
                </tr>`).join("");
                btnExport.disabled = false;
            }).fail(handleApiError);
        });

        btnExport?.addEventListener("click", function () {
            downloadCsv("reporte_mantenimientos.csv",
                ["ID", "Turbina", "Tipo", "Inicio Estimado", "Fin Estimado", "Fin Real", "Estado", "Resultado"],
                lastRows.map(m => [
                    m.id ?? m.Id,
                    turbineNames[m.turbineId ?? m.TurbineId] || (m.turbineId ?? m.TurbineId),
                    (m.maintenanceType ?? m.MaintenanceType) === "Preventive" ? "Preventivo" : "Correctivo",
                    fmtDate(m.estimatedStartDate ?? m.EstimatedStartDate),
                    fmtDate(m.estimatedEndDate ?? m.EstimatedEndDate),
                    fmtDate(m.actualEndDate ?? m.ActualEndDate),
                    m.status ?? m.Status,
                    m.result ?? m.Result ?? ""
                ]));
        });
    }

    // ── Admin: energía generada por turbina (día/mes/año) ──────────────

    const genBody = document.getElementById("generationReportBody");
    if (genBody) {
        const sel = document.getElementById("repTurbineSelect");
        const groupSel = document.getElementById("repGroupBy");
        const btnRun = document.getElementById("btnRunGeneration");
        const btnExport = document.getElementById("btnExportGeneration");
        let lastRows = [];

        loadTurbinesInto(sel, false);

        btnRun?.addEventListener("click", function () {
            const tid = sel.value;
            if (!tid) { if (typeof notify !== "undefined") notify.warning("Seleccione una turbina."); return; }
            const groupBy = groupSel.value;
            btnRun.disabled = true;
            genBody.innerHTML = '<tr><td colspan="3" class="text-center py-4"><span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Consolidando historial de generación...</td></tr>';

            fetchAllGeneration(tid).then(function (logs) {
                const groups = {};
                logs.forEach(l => {
                    const d = new Date(l.eventDate ?? l.EventDate);
                    let key, label;
                    if (groupBy === "day") { key = d.toISOString().slice(0, 10); label = d.toLocaleDateString("es-CR"); }
                    else if (groupBy === "year") { key = String(d.getFullYear()); label = key; }
                    else { key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}`; label = `${MONTHS[d.getMonth() + 1]} ${d.getFullYear()}`; }
                    if (!groups[key]) groups[key] = { label, count: 0, total: 0 };
                    groups[key].count++;
                    groups[key].total += Number(l.generatedEnergy ?? l.GeneratedEnergy ?? 0);
                });
                const keys = Object.keys(groups).sort();
                lastRows = keys.map(k => groups[k]);
                if (!keys.length) {
                    genBody.innerHTML = '<tr><td colspan="3" class="text-center text-muted py-4">Esta turbina no tiene registros de generación.</td></tr>';
                    btnExport.disabled = true;
                } else {
                    genBody.innerHTML = keys.map(k => `<tr><td>${esc(groups[k].label)}</td><td>${groups[k].count}</td><td>${fmt(groups[k].total)}</td></tr>`).join("");
                    btnExport.disabled = false;
                    renderChart("generationReportChart", "bar",
                        keys.map(k => groups[k].label),
                        [{ label: "Energía Generada (MWh)", data: keys.map(k => groups[k].total), backgroundColor: "rgba(94, 106, 210, 0.65)" }]);
                }
                btnRun.disabled = false;
            }).catch(function (xhr) { btnRun.disabled = false; handleApiError(xhr); });
        });

        btnExport?.addEventListener("click", function () {
            downloadCsv("reporte_generacion_turbina.csv",
                ["Período", "Registros", "Energía Generada (MWh)"],
                lastRows.map(g => [g.label, g.count, g.total.toFixed(4)]));
        });
    }

    // ── Admin: energía suplida por proveedor ────────────────────────────

    const supBody = document.getElementById("suppliersReportBody");
    if (supBody) {
        const btnRun = document.getElementById("btnRunSuppliers");
        const btnExport = document.getElementById("btnExportSuppliers");
        let lastRows = [];

        btnRun?.addEventListener("click", function () {
            btnRun.disabled = true;
            supBody.innerHTML = '<tr><td colspan="6" class="text-center py-4"><span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Consolidando generación de todas las plantas...</td></tr>';

            apiClient.get("Turbines/All").done(function (res) {
                const turbines = res?.data || res?.Data || [];
                Promise.all(turbines.map(t =>
                    fetchAllGeneration(t.id ?? t.Id).then(logs => ({
                        turbine: t,
                        total: logs.reduce((s, l) => s + Number(l.generatedEnergy ?? l.GeneratedEnergy ?? 0), 0)
                    }))
                )).then(function (results) {
                    const grandTotal = results.reduce((s, r) => s + r.total, 0);
                    results.sort((a, b) => b.total - a.total);
                    lastRows = results.map(r => ({
                        code: r.turbine.code ?? r.turbine.Code,
                        name: r.turbine.name ?? r.turbine.Name,
                        status: r.turbine.status ?? r.turbine.Status,
                        capacity: r.turbine.nominalWeeklyCapacityMWh ?? r.turbine.NominalWeeklyCapacityMWh,
                        total: r.total,
                        pct: grandTotal > 0 ? (r.total / grandTotal * 100) : 0
                    }));
                    supBody.innerHTML = lastRows.map(r => `<tr>
                        <td>${esc(r.code)}</td>
                        <td>${esc(r.name)}</td>
                        <td>${turbineStatusBadge(r.status)}</td>
                        <td>${fmt(r.capacity)}</td>
                        <td class="fw-semibold">${fmt(r.total)}</td>
                        <td>${r.pct.toFixed(2)} %</td>
                    </tr>`).join("");
                    btnExport.disabled = false;
                    btnRun.disabled = false;
                }).catch(function (xhr) { btnRun.disabled = false; handleApiError(xhr); });
            }).fail(function (xhr) { btnRun.disabled = false; handleApiError(xhr); });
        });

        btnExport?.addEventListener("click", function () {
            downloadCsv("reporte_energia_por_proveedor.csv",
                ["Código", "Proveedor", "Estado", "Capacidad Nominal (MWh)", "Total Suplido (MWh)", "% del Total"],
                lastRows.map(r => [r.code, r.name, r.status, Number(r.capacity).toFixed(4), r.total.toFixed(4), r.pct.toFixed(2)]));
        });
    }

    // ── Engineer: estado actual y alertas de turbinas ───────────────────

    const statusBody = document.getElementById("statusReportBody");
    if (statusBody) {
        const btnExport = document.getElementById("btnExportStatus");
        let lastRows = [];
        const OVERDUE_DAYS = 30;

        apiClient.get("Turbines/All").done(function (res) {
            const list = res?.data || res?.Data || [];
            if (!list.length) {
                statusBody.innerHTML = '<tr><td colspan="6" class="text-center text-muted py-4">No hay turbinas registradas.</td></tr>';
                return;
            }
            const now = new Date();
            lastRows = list.map(t => {
                const status = t.status ?? t.Status;
                const lastM = t.lastMaintenance ?? t.LastMaintenance;
                const alerts = [];
                if (status === "Damaged") alerts.push("Avería activa");
                if (status === "Maintenance") alerts.push("Mantenimiento en curso");
                if (status === "Inactive") alerts.push("Suspendida por incumplimiento");
                const overdue = !lastM || (now - new Date(lastM)) / 86400000 > OVERDUE_DAYS;
                if (overdue && status !== "Decommissioned") alerts.push(`Mantenimiento vencido (> ${OVERDUE_DAYS} días)`);
                return { t, status, lastM, alerts };
            });
            statusBody.innerHTML = lastRows.map(r => `<tr>
                <td>${esc(r.t.code ?? r.t.Code)}</td>
                <td>${esc(r.t.name ?? r.t.Name)}</td>
                <td>${esc(r.t.location ?? r.t.Location)}</td>
                <td>${turbineStatusBadge(r.status)}</td>
                <td>${fmtDate(r.lastM)}</td>
                <td>${r.alerts.length
                    ? r.alerts.map(a => `<span class="badge bg-danger d-inline-block mb-1 me-1"><i class="bi bi-bell me-1" aria-hidden="true"></i>${esc(a)}</span>`).join("")
                    : '<span class="badge bg-success"><i class="bi bi-check-circle me-1" aria-hidden="true"></i>Sin alertas</span>'}</td>
            </tr>`).join("");
            btnExport.disabled = false;
        }).fail(function (xhr) {
            statusBody.innerHTML = '<tr><td colspan="6" class="text-center text-danger py-4">Error al cargar el estado de las turbinas.</td></tr>';
            handleApiError(xhr);
        });

        btnExport?.addEventListener("click", function () {
            downloadCsv("reporte_estado_turbinas.csv",
                ["Código", "Nombre", "Ubicación", "Estado", "Último Mantenimiento", "Alertas"],
                lastRows.map(r => [
                    r.t.code ?? r.t.Code, r.t.name ?? r.t.Name, r.t.location ?? r.t.Location,
                    r.status, fmtDate(r.lastM), r.alerts.join(" | ") || "Sin alertas"
                ]));
        });
    }

    // ── Buyer: asignación mensual de energía ────────────────────────────

    const allocBody = document.getElementById("allocReportBody");
    if (allocBody) {
        const btnExport = document.getElementById("btnExportAlloc");
        const userId = session.getUserId();
        let lastRows = [];

        // Join cliente: DistributionDetail.forecastId → Forecast(mes/año) para etiquetar cada período.
        $.when(apiClient.get("Distributions/ByBuyer/" + userId), apiClient.get("Forecasts/ByBuyer/" + userId))
            .done(function (distRes, fcRes) {
                const details = distRes[0]?.data || distRes[0]?.Data || [];
                const forecasts = fcRes[0]?.data || fcRes[0]?.Data || [];
                const fcById = {};
                forecasts.forEach(f => { fcById[f.id ?? f.Id] = f; });

                if (!details.length) {
                    allocBody.innerHTML = '<tr><td colspan="5" class="text-center text-muted py-4">Aún no participas en cierres de distribución comercial.</td></tr>';
                    return;
                }

                lastRows = details.map(d => {
                    const fc = fcById[d.forecastId ?? d.ForecastId];
                    const label = fc ? `${MONTHS[fc.forecastMonth ?? fc.ForecastMonth]} ${fc.forecastYear ?? fc.ForecastYear}` : `Distribución #${d.distributionId ?? d.DistributionId}`;
                    const req = Number(d.requestedEnergyMWh ?? d.RequestedEnergyMWh ?? 0);
                    const asg = Number(d.assignedEnergyMWh ?? d.AssignedEnergyMWh ?? 0);
                    const uns = Number(d.unassignedEnergyMWh ?? d.UnassignedEnergyMWh ?? 0);
                    return { label, req, asg, uns, pct: req > 0 ? (asg / req * 100) : 100, sortKey: fc ? ((fc.forecastYear ?? fc.ForecastYear) * 100 + (fc.forecastMonth ?? fc.ForecastMonth)) : 0 };
                }).sort((a, b) => a.sortKey - b.sortKey);

                const totReq = lastRows.reduce((s, r) => s + r.req, 0);
                const totAsg = lastRows.reduce((s, r) => s + r.asg, 0);
                const totUns = lastRows.reduce((s, r) => s + r.uns, 0);
                document.getElementById("allocTotalRequested").textContent = fmt(totReq, 2);
                document.getElementById("allocTotalAssigned").textContent = fmt(totAsg, 2);
                document.getElementById("allocTotalUnsupplied").textContent = fmt(totUns, 2);

                allocBody.innerHTML = lastRows.map(r => `<tr>
                    <td>${esc(r.label)}</td>
                    <td>${fmt(r.req, 2)}</td>
                    <td>${fmt(r.asg, 2)}</td>
                    <td>${fmt(r.uns, 2)}</td>
                    <td><span class="badge ${r.pct >= 100 ? "bg-success" : "bg-warning text-dark"}"><i class="bi ${r.pct >= 100 ? "bi-check-circle" : "bi-exclamation-triangle"} me-1" aria-hidden="true"></i>${r.pct.toFixed(1)} %</span></td>
                </tr>`).join("");
                btnExport.disabled = false;

                const tail = lastRows.slice(-12);
                renderChart("allocReportChart", "bar",
                    tail.map(r => r.label),
                    [
                        { label: "Solicitado (MWh)", data: tail.map(r => r.req), backgroundColor: "rgba(148, 156, 218, 0.55)" },
                        { label: "Asignado (MWh)", data: tail.map(r => r.asg), backgroundColor: "rgba(94, 106, 210, 0.85)" }
                    ]);
            })
            .fail(function (xhr) {
                allocBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger py-4">Error al cargar el reporte de asignación.</td></tr>';
                handleApiError(xhr);
            });

        btnExport?.addEventListener("click", function () {
            downloadCsv("reporte_asignacion_mensual.csv",
                ["Período", "Solicitado (MWh)", "Asignado (MWh)", "No Suplido (MWh)", "% Cumplimiento"],
                lastRows.map(r => [r.label, r.req.toFixed(2), r.asg.toFixed(2), r.uns.toFixed(2), r.pct.toFixed(1)]));
        });
    }
});
