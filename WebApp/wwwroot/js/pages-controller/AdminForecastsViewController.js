// AdminForecastsViewController.js (§85 Admin/Forecasts) - Consulta de pronósticos de demanda por mes/año (solo lectura)
document.addEventListener("DOMContentLoaded", function () {
    const monthSelect = document.getElementById("filterMonth");
    if (!monthSelect) return;

    const yearInput = document.getElementById("filterYear");
    const filterBtn = document.getElementById("btnFilterForecasts");
    const body = document.getElementById("forecastsBody");
    let userNames = {};

    const now = new Date();
    monthSelect.value = String(now.getMonth() + 1);
    if (yearInput) yearInput.value = String(now.getFullYear());

    apiClient.get("Users/RetrieveAll").done(function (res) {
        (res?.data || res?.Data || []).forEach(function (u) {
            const id = u.id || u.Id;
            userNames[id] = `${u.firstName || u.FirstName || ""} ${u.firstLastName || u.FirstLastName || ""}`.trim() || `Usuario #${id}`;
        });
    }).always(consultForecasts);

    if (filterBtn) filterBtn.addEventListener("click", consultForecasts);

    function statusBadge(status) {
        const map = { Pending: "bg-warning text-dark", Modified: "bg-info text-dark", Distributed: "bg-success", Cancelled: "bg-secondary", Blocked: "bg-danger" };
        return `<span class="badge ${map[status] || "bg-secondary"}">${status || "-"}</span>`;
    }

    function consultForecasts() {
        const month = parseInt(monthSelect.value);
        const year = parseInt(yearInput?.value || now.getFullYear());

        body.innerHTML = '<tr><td colspan="5" class="text-center"><span class="spinner-border spinner-border-sm"></span> Consultando...</td></tr>';

        apiClient.get(`Forecasts/ByMonth?month=${month}&year=${year}`)
            .done(function (res) {
                const items = res?.data || res?.Data || [];
                if (!items.length) {
                    body.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Sin pronósticos registrados para este período.</td></tr>';
                    return;
                }
                body.innerHTML = items.map(function (f) {
                    const buyerId = f.buyerId ?? f.BuyerId;
                    return `<tr>
                        <td>${escapeHtml(userNames[buyerId] || `Comprador #${buyerId}`)}</td>
                        <td>${f.forecastMonth ?? f.ForecastMonth}/${f.forecastYear ?? f.ForecastYear}</td>
                        <td>${Number(f.requestedEnergyMWh ?? f.RequestedEnergyMWh ?? 0).toLocaleString("es-CR", { minimumFractionDigits: 2 })}</td>
                        <td>${statusBadge(f.status || f.Status)}</td>
                        <td>${new Date(f.createdAt || f.CreatedAt).toLocaleDateString("es-CR")}</td>
                    </tr>`;
                }).join("");
            })
            .fail(function (xhr) {
                body.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Error al consultar los pronósticos.</td></tr>';
                handleApiError(xhr);
            });
    }
});
