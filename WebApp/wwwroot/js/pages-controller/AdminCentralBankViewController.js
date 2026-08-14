// AdminCentralBankViewController.js (§44, §85 Admin/CentralBank) - Inventario, capacidad máxima y bitácora de movimientos
document.addEventListener("DOMContentLoaded", function () {
    const currentEl = document.getElementById("cbCurrent");
    if (!currentEl) return;

    const autoCapEl = document.getElementById("cbAutoCap");
    const logsBody = document.getElementById("cbLogsBody");

    loadInventory();
    loadLogs();
    //para refrescar cada minuto, para que el usuario no tenga que recargar la página manualmente
    setInterval(function () {
    loadInventory();
    loadLogs();
    }, 60000);
    // Entities-DTOs.CentralBank solo tiene CurrentInventoryMWh y MaximumCapacityMWh — no existe una
    // capacidad "automática" separada de una "manual" en el backend, así que se eliminó ese control editable.
   function loadInventory() {
    apiClient.get("CentralBanks/RetrieveAll")
        .done(function (res) {

            const items = Array.isArray(res)
            ? res
            : (res?.data || res?.Data || []);

            const cb = Array.isArray(items)
                ? items[0]
                : items;

            if (!cb) {
                currentEl.textContent = "0.00 MWh";
                autoCapEl.textContent = "0.00 MWh";
                return;
            }

            const current = Number(
                cb.currentInventoryMWh ??
                cb.CurrentInventoryMWh ??
                0
            );

            const capacity = Number(
                cb.maximumCapacityMWh ??
                cb.MaximumCapacityMWh ??
                0
            );

            currentEl.textContent =
                current.toLocaleString("es-CR", {
                    minimumFractionDigits: 2
                }) + " MWh";

            autoCapEl.textContent =
                capacity.toLocaleString("es-CR", {
                    minimumFractionDigits: 2
                }) + " MWh";
        })
        .fail(function (xhr) {
            handleApiError(xhr);
        });
}
  
function loadLogs() {
    if (!logsBody) return;

    logsBody.innerHTML =
        '<tr><td colspan="5" class="text-center">' +
        '<span class="spinner-border spinner-border-sm"></span> ' +
        'Cargando movimientos...' +
        '</td></tr>';

    apiClient.get("CentralBankMovement/Retrieve")
        .done(function (res) {

            const items = Array.isArray(res)
            ? res
            : (res?.data || res?.Data || []);

            if (!items.length) {
                logsBody.innerHTML =
                    '<tr><td colspan="5" class="text-center text-muted">' +
                    'Sin movimientos registrados.' +
                    '</td></tr>';
                return;
            }

            logsBody.innerHTML = items.map(function (movement) {

                const id =
                    movement.id ??
                    movement.Id ??
                    "-";

                const type =
                    movement.movementType ??
                    movement.MovementType ??
                    "-";

                const energy =
                    Number(
                        movement.energyMWh ??
                        movement.EnergyMWh ??
                        0
                    );

                const createdAt =
                    movement.createdAt ??
                    movement.CreatedAt;

                const description =
                    movement.description ??
                    movement.Description ??
                    "-";

                let badge = "bg-secondary";

                if (type === "RECEIVE") {
                    badge = "bg-success";
                }
                else if (type === "DISTRIBUTE") {
                    badge = "bg-danger";
                }
                else if (type === "FLUSH") {
                    badge = "bg-primary";
                }

                return `<tr>
                    <td>${id}</td>
                    <td>
                        <span class="badge ${badge}">
                            ${type}
                        </span>
                    </td>
                    <td>
                        ${energy.toLocaleString("es-CR", {
                            minimumFractionDigits: 2
                        })} MWh
                    </td>
                    <td>
                        ${createdAt
                            ? new Date(createdAt).toLocaleString("es-CR")
                            : "-"}
                    </td>
                    <td>${description}</td>
                </tr>`;

            }).join("");
        })
        .fail(function (xhr) {

            logsBody.innerHTML =
                '<tr><td colspan="5" class="text-center text-danger">' +
                'Error al cargar los movimientos.' +
                '</td></tr>';

            handleApiError(xhr);
        });
}

});
