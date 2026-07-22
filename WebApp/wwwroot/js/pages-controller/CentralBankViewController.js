// CentralBankViewController.js (§22.1, §27) - Controlador para Banco Central y Traslados de Energía (Flushes)
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando CentralBankViewController...");

    const token = session.getToken();
    const role = session.getRole();
    const userId = session.getUserId() || 1;

    if (!token || (role !== "Engineer" && role !== "Administrator" && role !== "Admin")) {
        notify.error("Acceso denegado. Requiere privilegios de Operaciones.");
        setTimeout(() => {
            window.location.href = "/Login";
        }, 1500);
        return;
    }

    // ==========================================
    // 1. BANCO CENTRAL DE ENERGÍA (/Engineer/CentralBank)
    // ==========================================
    if (document.getElementById("cbInv")) {
        loadCentralBankStatus();
    }

    function loadCentralBankStatus() {
        // 1. Nivel de Inventario
        apiClient.get("CentralBanks/Inventory").done(function (res) {
            const cb = res?.data || res?.Data || {};
            const inv = cb.currentInventoryMWh ?? cb.CurrentInventoryMWh ?? 0;
            const updated = cb.updatedAt || cb.UpdatedAt;

            setText("cbInv", formatNum(inv) + " MWh");
            setText("cbLast", updated ? new Date(updated).toLocaleString("es-CR") : "Reciente");
        }).fail(function (xhr) {
            setText("cbInv", "Error");
            handleApiError(xhr);
        });

        // 2. Bitácora de Saturación y Movimientos
        const logsBody = document.getElementById("cbLogsBody");
        if (logsBody) {
            logsBody.innerHTML = '<tr><td colspan="5" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando bitácora inmutable...</td></tr>';
            apiClient.get("CentralBanks/MovementLogs?page=1&pageSize=30").done(function (res) {
                const list = (res?.data?.items || res?.Data?.Items || res?.data || res?.Data || []);
                renderMovementLogs(list);
            }).fail(function () {
                logsBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Error al consultar bitácora del Banco Central.</td></tr>';
            });
        }
    }

    function renderMovementLogs(list) {
        const body = document.getElementById("cbLogsBody");
        if (!body) return;
        if (!list.length) {
            body.innerHTML = '<tr><td colspan="5" class="text-center text-muted">No se registran traslados, salidas ni eventos de saturación.</td></tr>';
            return;
        }

        body.innerHTML = list.map(l => {
            const id = l.id || l.Id;
            const type = l.movementType || l.MovementType || "Inflow";
            let typeBadge = '<span class="badge bg-success">Entrada (Flush)</span>';
            if (type.toLowerCase() === "outflow") typeBadge = '<span class="badge bg-primary">Salida (Distribución)</span>';
            if (type.toLowerCase() === "saturation" || type.toLowerCase() === "loss") typeBadge = '<span class="badge bg-danger"><i class="bi bi-exclamation-triangle"></i> Saturación / Pérdida</span>';

            const amt = formatNum(l.amount || l.Amount);
            const resInv = formatNum(l.resultingInventory || l.ResultingInventory);
            const dateStr = l.eventDate || l.EventDate ? new Date(l.eventDate || l.EventDate).toLocaleString("es-CR") : "-";

            return `
                <tr class="${type.toLowerCase() === 'saturation' ? 'table-warning' : ''}">
                    <td>#${id}</td>
                    <td>${typeBadge}</td>
                    <td class="fw-bold">${amt} MWh</td>
                    <td class="text-secondary">${resInv} MWh</td>
                    <td>${dateStr}</td>
                </tr>
            `;
        }).join("");
    }

    // ==========================================
    // 2. HISTORIAL DE FLUSHES (/Engineer/FlushHistory)
    // ==========================================
    if (document.getElementById("flushesBody")) {
        loadFlushHistory();
        loadFlushConfig();

        const btnSaveConf = document.getElementById("btnSaveConfig");
        if (btnSaveConf) {
            btnSaveConf.addEventListener("click", function () {
                updateFlushConfig(btnSaveConf);
            });
        }
    }

    function loadFlushHistory() {
        const body = document.getElementById("flushesBody");
        if (!body) return;
        body.innerHTML = '<tr><td colspan="6" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando historial de flushes...</td></tr>';

        apiClient.get("Flushs/History?page=1&pageSize=50").done(function (res) {
            const list = (res?.data?.items || res?.Data?.Items || res?.data || res?.Data || []);
            if (!list.length) {
                body.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Aún no se han ejecutado traslados o flushes en la red.</td></tr>';
                return;
            }

            body.innerHTML = list.map(f => {
                const id = f.id || f.Id;
                const dateStr = f.executedAt || f.ExecutedAt ? new Date(f.executedAt || f.ExecutedAt).toLocaleString("es-CR") : "-";
                const tid = f.turbineId || f.TurbineId || "-";
                const amt = formatNum(f.transferredEnergyMWh ?? f.TransferredEnergyMWh ?? 0);
                const type = f.executionType || f.ExecutionType || "Automatic";
                const typeBadge = type.toLowerCase() === "manual" ? '<span class="badge bg-secondary">Manual</span>' : '<span class="badge bg-info text-dark">Automático</span>';
                const st = f.status || f.Status || "Completed";
                const stBadge = st.toLowerCase() === "completed" ? '<span class="badge bg-success">Exitoso</span>' : '<span class="badge bg-danger">Fallido</span>';

                return `
                    <tr>
                        <td>#${id}</td>
                        <td>${dateStr}</td>
                        <td class="fw-bold">Turbina #${tid}</td>
                        <td class="text-success fw-bold">+${amt} MWh</td>
                        <td>${typeBadge}</td>
                        <td>${stBadge}</td>
                    </tr>
                `;
            }).join("");
        }).fail(function (xhr) {
            body.innerHTML = '<tr><td colspan="6" class="text-center text-danger">Error al obtener el historial de flushes.</td></tr>';
            handleApiError(xhr);
        });
    }

    function loadFlushConfig() {
        apiClient.get("Flushs/Config").done(function (res) {
            const c = res?.data || res?.Data || {};
            const freq = document.getElementById("flFreq");
            const thresh = document.getElementById("flThresh");
            if (freq) freq.value = c.flushFrequencyHours ?? c.FlushFrequencyHours ?? 6;
            if (thresh) thresh.value = c.batteryThresholdPercentage ?? c.BatteryThresholdPercentage ?? 80;
        });
    }

    function updateFlushConfig(btn) {
        const freq = parseInt(document.getElementById("flFreq")?.value || 6);
        const thresh = parseFloat(document.getElementById("flThresh")?.value || 80);

        if (freq <= 0 || thresh <= 0 || thresh > 100) {
            notify.warning("Ingrese parámetros válidos (Frecuencia > 0h, Umbral entre 1% y 100%).");
            return;
        }

        const origText = btn.innerHTML;
        btn.disabled = true;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Guardando...';

        apiClient.put("Flushs/Config?callerUserId=" + userId, {
            flushFrequencyHours: freq,
            batteryThresholdPercentage: thresh
        }).done(function () {
            notify.success("Configuración de disparos automáticos actualizada.");
            bootstrap.Modal.getInstance(document.getElementById("configModal"))?.hide();
        }).fail(function (xhr) {
            handleApiError(xhr);
        }).always(function () {
            btn.disabled = false;
            btn.innerHTML = origText;
        });
    }

    function setText(id, val) {
        const el = document.getElementById(id);
        if (el) el.textContent = val;
    }

    function formatNum(num) {
        return Number(num || 0).toLocaleString("es-CR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }
});
