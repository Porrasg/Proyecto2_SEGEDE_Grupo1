document.addEventListener("DOMContentLoaded", function () {

    const tableBody = document.getElementById("batteriesTableBody");
    const searchInput = document.getElementById("searchBattery");
    const refreshButton = document.getElementById("btnRefreshBatteries");

    let allBatteries = [];

    // CARGA INICIAL

    loadBatteries();

    // BOTÓN ACTUALIZAR

    if (refreshButton) {
        refreshButton.addEventListener("click", function () {
            loadBatteries();
        });
    }

    // BUSCADOR

    if (searchInput) {
        searchInput.addEventListener("input", function () {
            filterAndRender();
        });
    }

    // OBTENER BATERÍAS

    function loadBatteries() {

        if (tableBody) {
            tableBody.innerHTML = `
                <tr>
                    <td colspan="8" class="text-center py-5">
                        <span class="spinner-border spinner-border-sm me-2"></span>
                        Cargando baterías...
                    </td>
                </tr>
            `;
        }

        apiClient.get("Batteries/RetrieveAllBatteries")
            .done(function (res) {

                allBatteries = apiClient.unwrapList(res) || [];

                updateKPIs(allBatteries);
                filterAndRender();

            })
            .fail(function (xhr) {

                if (tableBody) {
                    tableBody.innerHTML = `
                        <tr>
                            <td colspan="8"
                                class="text-center text-danger py-5">
                                <i class="bi bi-exclamation-triangle me-2"></i>
                                No se pudieron cargar las baterías.
                            </td>
                        </tr>
                    `;
                }

                handleApiError(xhr);
            });
    }

    // KPIs 

    function updateKPIs(batteries) {

        const activeBatteries = batteries.filter(b =>
            getValue(b, "status").toLowerCase() === "active"
        ).length;

        const currentEnergy = batteries.reduce(
            (total, b) => total + getNumber(b, "currentEnergyMWh"),
            0
        );

        const transferredEnergy = batteries.reduce(
            (total, b) => total + getNumber(b, "totalTransferredMWh"),
            0
        );

        const saturationLoss = batteries.reduce(
            (total, b) => total + getNumber(b, "totalSaturationLossMWh"),
            0
        );

        setText("kpiActiveBatteries", activeBatteries);

        setText(
            "kpiCurrentEnergy",
            formatNumber(currentEnergy) + " MWh"
        );

        setText(
            "kpiTransferredEnergy",
            formatNumber(transferredEnergy) + " MWh"
        );

        setText(
            "kpiSaturationLoss",
            formatNumber(saturationLoss) + " MWh"
        );
    }
    // FILTRAR

    function filterAndRender() {

        const query = searchInput?.value
            ?.toLowerCase()
            .trim() || "";

        const filtered = allBatteries.filter(b => {

            const batteryId = String(
                getValue(b, "id")
            ).toLowerCase();

            const turbineId = String(
                getValue(b, "turbineId")
            ).toLowerCase();

            const status = getValue(b, "status")
                .toLowerCase();

            return !query ||
                batteryId.includes(query) ||
                turbineId.includes(query) ||
                status.includes(query);
        });

        renderTable(filtered);
    }

    // TABLA DE BATERÍAS

    function renderTable(batteries) {

        if (!tableBody) return;

        if (!batteries.length) {

            tableBody.innerHTML = `
                <tr>
                    <td colspan="8"
                        class="text-center text-muted py-5">

                        <i class="bi bi-battery me-2"></i>
                        No se encontraron baterías.

                    </td>
                </tr>
            `;

            return;
        }

        tableBody.innerHTML = batteries.map(b => {

            const id = getValue(b, "id");
            const turbineId = getValue(b, "turbineId");

            const capacity = getNumber(
                b,
                "maximumCapacityMWh"
            );

            const currentEnergy = getNumber(
                b,
                "currentEnergyMWh"
            );

            const generated = getNumber(
                b,
                "totalGeneratedMWh"
            );

            const transferred = getNumber(
                b,
                "totalTransferredMWh"
            );

            const saturationLoss = getNumber(
                b,
                "totalSaturationLossMWh"
            );

            const status = getValue(
                b,
                "status"
            );

            return `
                <tr>

                    <td class="text-center fw-bold">
                        ${escapeHtml(id)}
                    </td>

                    <td class="text-center">
                        <span class="badge bg-secondary">
                            Turbina ${escapeHtml(turbineId)}
                        </span>
                    </td>

                    <td class="text-center">
                        ${formatNumber(capacity)} MWh
                    </td>

                    <td class="text-center fw-semibold">
                        ${formatNumber(currentEnergy)} MWh
                    </td>

                    <td class="text-center">
                        ${formatNumber(generated)} MWh
                    </td>

                    <td class="text-center">
                        ${formatNumber(transferred)} MWh
                    </td>

                    <td class="text-center">
                        ${formatNumber(saturationLoss)} MWh
                    </td>

                    <td class="text-center">
                        ${getStatusBadge(status)}
                    </td>

                </tr>
            `;

        }).join("");
    }

    // HELPERS

    function getValue(object, property) {

        if (!object) return "";

        const value =
            object[property] ??
            object[property.charAt(0).toUpperCase() + property.slice(1)];

        return value ?? "";
    }

    function getNumber(object, property) {

        const value = getValue(object, property);

        const number = Number(value);

        return Number.isFinite(number)
            ? number
            : 0;
    }
    function setText(id, value) {

        const element = document.getElementById(id);

        if (element) {
            element.textContent = value;
        }
    }
    function formatNumber(number) {

        return Number(number).toLocaleString(
            "es-CR",
            {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }
        );
    }
    function getStatusBadge(status) {

        const normalized = (status || "")
            .toString()
            .toLowerCase();

        if (normalized === "active") {
            return `
                <span class="badge bg-success">
                    Activa
                </span>
            `;
        }

        if (normalized === "inactive") {
            return `
                <span class="badge bg-secondary">
                    Inactiva
                </span>
            `;
        }

        return `
            <span class="badge bg-warning text-dark">
                ${escapeHtml(status || "-")}
            </span>
        `;
    }
    function escapeHtml(value) {

        return String(value ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

});