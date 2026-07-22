// AdminCentralBankViewController.js (§44, §85 Admin/CentralBank) - Inventario, capacidad máxima y bitácora de movimientos
document.addEventListener("DOMContentLoaded", function () {
    const currentEl = document.getElementById("cbCurrent");
    if (!currentEl) return;

    const autoCapEl = document.getElementById("cbAutoCap");
    const logsBody = document.getElementById("cbLogsBody");

    loadInventory();
    loadLogs();

    // Entities-DTOs.CentralBank solo tiene CurrentInventoryMWh y MaximumCapacityMWh — no existe una
    // capacidad "automática" separada de una "manual" en el backend, así que se eliminó ese control editable.
    function loadInventory() {
        apiClient.get("CentralBanks/Inventory")
            .done(function (res) {
                const cb = res?.data || res?.Data || {};
                const current = Number(cb.currentInventoryMWh ?? cb.CurrentInventoryMWh ?? 0);
                const capacity = Number(cb.maximumCapacityMWh ?? cb.MaximumCapacityMWh ?? 0);

                if (currentEl) currentEl.textContent = current.toLocaleString("es-CR", { minimumFractionDigits: 2 }) + " MWh";
                if (autoCapEl) autoCapEl.textContent = capacity.toLocaleString("es-CR", { minimumFractionDigits: 2 }) + " MWh";
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

});
