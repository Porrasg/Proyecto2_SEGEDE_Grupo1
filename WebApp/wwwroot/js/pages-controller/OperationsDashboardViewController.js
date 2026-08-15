// OperationsDashboardViewController.js - Controlador JS para el Panel de Operaciones / Ingeniero
document.addEventListener("DOMContentLoaded", function () {

    const role = session.getRole();
    if (role !== "Engineer" && role !== "Administrator" && role !== "Admin") {
        notify.error("Acceso denegado. Requiere privilegios de Ingeniero u Operaciones.");
        setTimeout(() => {
            window.location.href = "/Login";
        }, 1500);
        return;
    }

    // Cargo los datos reales del panel
    if (document.getElementById("opTotalTurbines")) {
        let opTurbinesChartInst = null;
        let opEnergyChartInst = null;

        loadOperationsDashboard();
        setInterval(loadOperationsDashboard, 15000);

        function loadOperationsDashboard() {
            apiClient.get("Dashboard/Engineer")
                .done(function (res) {
                    const data = res?.data || res?.Data || res || {};

                    const totalT = Number(data.totalTurbines ?? data.TotalTurbines ?? 0);
                    const activeT = Number(data.activeTurbines ?? data.ActiveTurbines ?? 0);
                    const maintT = Number(data.turbinesUnderMaintenance ?? data.TurbinesUnderMaintenance ?? 0);
                    const damagedT = Number(data.damagedTurbines ?? data.DamagedTurbines ?? 0);
                    const suspendedT = Number(data.suspendedTurbines ?? data.SuspendedTurbines ?? 0);
                    const cbInv = Number(data.centralBankInventory ?? data.CentralBankInventory ?? 0);
                    const alerts = Number(data.overdueMaintenanceAlerts ?? data.OverdueMaintenanceAlerts ?? 0);
                    const flushEnergy = Number(data.lastFlushEnergy ?? data.LastFlushEnergy ?? 0);

                    setText("opTotalTurbines", totalT);
                    setText("opActiveTurbines", activeT);
                    setText("opMaintTurbines", maintT);
                    setText("opDamagedTurbines", damagedT);
                    setText("opSuspendedTurbines", suspendedT);
                    setText("opCbInventory", formatNumber(cbInv) + " MWh");
                    setText("opOverdueAlerts", alerts);

                    const flushDate = data.lastFlushDate || data.LastFlushDate;
                    setText("opFlushDate", flushDate ? new Date(flushDate).toLocaleDateString("es-CR", { day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit" }) : "Sin registros");
                    setText("opFlushEnergy", formatNumber(flushEnergy) + " MWh");

                    renderOpCharts(activeT, maintT, damagedT, suspendedT, cbInv, flushEnergy);
                })
                .fail(function (xhr) {
                    handleApiError(xhr);
                });
        }

        function renderOpCharts(activeT, maintT, damagedT, suspendedT, cbInv, flushEnergy) {
            if (typeof Chart === "undefined") return;

            const ctxTurbines = document.getElementById("opTurbinesChart")?.getContext("2d");
            if (ctxTurbines) {
                if (opTurbinesChartInst) {
                    opTurbinesChartInst.data.datasets[0].data = [activeT, maintT, damagedT, suspendedT];
                    opTurbinesChartInst.update();
                } else {
                    opTurbinesChartInst = new Chart(ctxTurbines, {
                        type: "doughnut",
                        data: {
                            labels: ["Activas", "En Mantenimiento", "Dañadas", "Suspendidas"],
                            datasets: [{
                                data: [activeT, maintT, damagedT, suspendedT],
                                backgroundColor: ["#107C62", "#D97706", "#B91C1C", "#4B5563"],
                                borderWidth: 1
                            }]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {
                                legend: { position: "bottom" }
                            }
                        }
                    });
                }
            }

            const ctxEnergy = document.getElementById("opEnergyChart")?.getContext("2d");
            if (ctxEnergy) {
                if (opEnergyChartInst) {
                    opEnergyChartInst.data.datasets[0].data = [cbInv, flushEnergy];
                    opEnergyChartInst.update();
                } else {
                    opEnergyChartInst = new Chart(ctxEnergy, {
                        type: "bar",
                        data: {
                            labels: ["Inventario Banco Central", "Último Traslado Flush"],
                            datasets: [{
                                label: "Energía (MWh)",
                                data: [cbInv, flushEnergy],
                                backgroundColor: ["#5A2CA0", "#107C62"],
                                borderRadius: 6
                            }]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {
                                legend: { display: false }
                            },
                            scales: {
                                y: { beginAtZero: true }
                            }
                        }
                    });
                }
            }
        }
    }

    function setText(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = value;
    }

    function formatNumber(num) {
        return Number(num).toLocaleString("es-CR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }
});
