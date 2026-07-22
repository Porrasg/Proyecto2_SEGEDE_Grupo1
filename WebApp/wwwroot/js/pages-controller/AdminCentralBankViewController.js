// AdminCentralBankViewController.js (§44, §85 Admin/CentralBank) - Inventario, capacidad manual y bitácora de movimientos
document.addEventListener("DOMContentLoaded", function () {
    const currentEl = document.getElementById("cbCurrent");
    if (!currentEl) return;

    const autoCapEl = document.getElementById("cbAutoCap");
    const effectiveEl = document.getElementById("cbEffective");
    const manualInput = document.getElementById("cbManualCap");
    const saveBtn = document.getElementById("btnSaveCap");
    const logsBody = document.getElementById("cbLogsBody");

    loadInventory();
    loadLogs();

    function loadInventory() {
        apiClient.get("CentralBanks/Inventory")
            .done(function (res) {
                const cb = res?.data || res?.Data || {};
                const current = Number(cb.currentInventory ?? cb.CurrentInventory ?? 0);
                const auto = Number(cb.automaticCapacity ?? cb.AutomaticCapacity ?? 0);
                const manual = cb.manualCapacity ?? cb.ManualCapacity;
                const effective = manual != null ? Number(manual) : auto;

                if (currentEl) currentEl.textContent = current.toLocaleString("es-CR", { minimumFractionDigits: 2 }) + " MWh";
                if (autoCapEl) autoCapEl.textContent = auto.toLocaleString("es-CR", { minimumFractionDigits: 2 }) + " MWh";
                if (effectiveEl) effectiveEl.textContent = effective.toLocaleString("es-CR", { minimumFractionDigits: 2 }) + " MWh";
                if (manualInput) manualInput.value = manual != null ? manual : "";
            })
            .fail(function (xhr) { handleApiError(xhr); });
    }

    function loadLogs() {
        if (!logsBody) return;
        logsBody.innerHTML = '<tr><td colspan="5" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando movimientos...</td></tr>';
        apiClient.get("CentralBanks/MovementLogs?page=1&pageSize=50")
            .done(function (res) {
                const items = res?.data?.items || res?.Data?.Items || [];
                if (!items.length) {
                    logsBody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Sin movimientos registrados.</td></tr>';
                    return;
                }
                logsBody.innerHTML = items.map(function (l) {
                    const type = l.movementType || l.MovementType || "-";
                    const badge = type === "Inflow" ? "bg-success" : "bg-danger";
                    const origin = (l.flushId || l.FlushId) ? `Flush #${l.flushId || l.FlushId}` : (l.distributionId || l.DistributionId) ? `Distribución #${l.distributionId || l.DistributionId}` : "-";
                    return `<tr>
                        <td>${l.id || l.Id}</td>
                        <td><span class="badge ${badge}">${type}</span></td>
                        <td>${Number(l.amount ?? l.Amount ?? 0).toLocaleString("es-CR", { minimumFractionDigits: 2 })}</td>
                        <td>${new Date(l.eventDate || l.EventDate).toLocaleString("es-CR")}</td>
                        <td>${origin}</td>
                    </tr>`;
                }).join("");
            })
            .fail(function (xhr) {
                logsBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Error al cargar los movimientos.</td></tr>';
                handleApiError(xhr);
            });
    }

    if (saveBtn) {
        saveBtn.addEventListener("click", function () {
            const raw = manualInput?.value.trim();
            const capacity = raw === "" ? null : parseFloat(raw);

            saveBtn.disabled = true;
            const original = saveBtn.innerHTML;
            saveBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';

            apiClient.put("CentralBanks/ManualCapacity", { capacity: capacity })
                .done(function () {
                    notify.success(capacity == null ? "Capacidad restablecida a automática." : "Capacidad manual actualizada.");
                    loadInventory();
                })
                .fail(function (xhr) { handleApiError(xhr); })
                .always(function () {
                    saveBtn.disabled = false;
                    saveBtn.innerHTML = original;
                });
        });
    }
});
