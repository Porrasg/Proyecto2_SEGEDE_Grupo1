// SimulatorViewController.js (adenda v3 §129-135) - Detonadores del Simulator Panel (Admin y Engineer)
document.addEventListener("DOMContentLoaded", function () {
    const failTurbineSelect = document.getElementById("simFailTurbine");
    if (!failTurbineSelect) return; // Esta página no es el Simulator Panel.

    const battTurbineSelect = document.getElementById("simBattTurbine");
    const historyBody = document.getElementById("simHistoryBody");

    loadTurbines();

    function loadTurbines() {
        apiClient.get("Turbines/RetrieveAll")
            .done(function (res) {
                const turbines = res?.data || res?.Data || [];
                const options = turbines.map(function (t) {
                    const code = t.code || t.Code || `#${t.id}`;
                    return `<option value="${t.id}">${escapeHtml(code)} — ${escapeHtml(t.name || "")}</option>`;
                }).join("");
                if (failTurbineSelect) failTurbineSelect.innerHTML = options;
                if (battTurbineSelect) battTurbineSelect.innerHTML = options;
            })
            .fail(function (xhr) { handleApiError(xhr); });
    }

    function logHistory(trigger, params, success, message) {
        if (!historyBody) return;
        if (historyBody.children.length === 1 && historyBody.children[0].children.length === 1) {
            historyBody.innerHTML = "";
        }
        const row = document.createElement("tr");
        const badge = success ? '<span class="badge bg-success">OK</span>' : '<span class="badge bg-danger">Error</span>';
        row.innerHTML = `<td>${new Date().toLocaleTimeString("es-CR")}</td><td>${trigger}</td><td class="small text-muted">${params}</td><td>${badge} <span class="small">${message || ""}</span></td>`;
        historyBody.prepend(row);
    }

    function runTrigger(btn, confirmMsg, trigger, params, request) {
        notify.confirm(confirmMsg, { dangerous: true, confirmText: "Ejecutar" }).then(function (ok) {
            if (!ok) return;
            const original = btn.innerHTML;
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Ejecutando...';

            request()
                .done(function (res) {
                    const msg = res?.message || res?.Message || "Completado.";
                    notify.success(msg);
                    logHistory(trigger, params, true, msg);
                })
                .fail(function (xhr) {
                    const res = xhr.responseJSON;
                    const msg = res?.message || res?.Message || "Falló.";
                    logHistory(trigger, params, false, msg);
                    handleApiError(xhr);
                })
                .always(function () {
                    btn.disabled = false;
                    btn.innerHTML = original;
                });
        });
    }

    // 1. Forzar falla crítica
    const btnFail = document.getElementById("btnSimFail");
    if (btnFail) {
        btnFail.addEventListener("click", function () {
            const turbineId = parseInt(failTurbineSelect.value);
            const severity = document.getElementById("simFailSeverity")?.value || "Critical";
            const description = document.getElementById("simFailDescription")?.value.trim() || "Falla forzada desde Simulator Panel.";
            if (!turbineId) { notify.warning("Seleccione una turbina."); return; }

            runTrigger(btnFail, `¿Forzar una falla ${severity} en la turbina seleccionada?`, "Forzar Falla",
                `Turbina #${turbineId}, ${severity}`,
                function () { return apiClient.post("Failures/Register", { turbineId: turbineId, description: description, severity: severity }); });
        });
    }

    // 2. Alterar batería local
    const btnBattery = document.getElementById("btnSimBattery");
    if (btnBattery) {
        btnBattery.addEventListener("click", function () {
            const turbineId = parseInt(battTurbineSelect.value);
            const charge = parseFloat(document.getElementById("simBattCharge")?.value);
            if (!turbineId) { notify.warning("Seleccione una turbina."); return; }
            if (isNaN(charge) || charge < 0) { notify.warning("Ingrese una carga válida (>= 0)."); return; }

            runTrigger(btnBattery, `¿Forzar la batería de la turbina seleccionada a ${charge} MWh?`, "Alterar Batería",
                `Turbina #${turbineId} → ${charge} MWh`,
                function () { return apiClient.post("Energy/SetBatteryCharge", { turbineId: turbineId, currentEnergyMWh: charge }); });
        });
    }

    // 3. Ciclo de generación de energía
    const btnEnergyCycle = document.getElementById("btnSimEnergyCycle");
    if (btnEnergyCycle) {
        btnEnergyCycle.addEventListener("click", function () {
            runTrigger(btnEnergyCycle, "¿Ejecutar de inmediato un ciclo de simulación energética para todas las turbinas activas?", "Ciclo de Energía", "Todas las turbinas",
                function () { return apiClient.post("Energy/RunSimulation", {}); });
        });
    }

    // 4. Jobs en segundo plano
    const btnMaintCheck = document.getElementById("btnSimMaintCheck");
    if (btnMaintCheck) {
        btnMaintCheck.addEventListener("click", function () {
            runTrigger(btnMaintCheck, "¿Ejecutar la verificación de mantenimientos vencidos (> 40 días)?", "Verificar Mantenimientos", "-",
                function () { return apiClient.post("Turbines/CheckOverdueMaintenance", {}); });
        });
    }

    const btnFlush = document.getElementById("btnSimFlush");
    if (btnFlush) {
        btnFlush.addEventListener("click", function () {
            runTrigger(btnFlush, "¿Ejecutar un Flush manual ahora? Traslada toda la energía disponible al Banco Central y no se puede deshacer.", "Flush Manual", "-",
                function () { return apiClient.post("Flushs/ExecuteManual", {}); });
        });
    }

    const btnNotifications = document.getElementById("btnSimNotifications");
    if (btnNotifications) {
        btnNotifications.addEventListener("click", function () {
            runTrigger(btnNotifications, "¿Procesar de inmediato la cola de notificaciones pendientes?", "Procesar Notificaciones", "-",
                function () { return apiClient.post("Notifications/ProcessQueue", {}); });
        });
    }

    const btnDistribution = document.getElementById("btnSimDistribution");
    if (btnDistribution) {
        const monthSelect = document.getElementById("simDistMonth");
        const yearInput = document.getElementById("simDistYear");
        const now = new Date();
        if (monthSelect) monthSelect.value = String(now.getMonth() + 1);
        if (yearInput) yearInput.value = String(now.getFullYear());

        btnDistribution.addEventListener("click", function () {
            const month = parseInt(monthSelect?.value);
            const year = parseInt(yearInput?.value);
            runTrigger(btnDistribution, `¿Ejecutar la distribución comercial de ${month}/${year}? Cierra el período y genera estados de cuenta.`, "Distribución Mensual", `${month}/${year}`,
                function () { return apiClient.post(`Distributions/ExecuteMonthly?month=${month}&year=${year}`, {}); });
        });
    }
});
