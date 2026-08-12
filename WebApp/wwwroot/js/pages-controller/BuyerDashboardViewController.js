// BuyerDashboardViewController.js - Controlador JS para el Panel de Comprador
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando BuyerDashboardViewController...");

    const role = session.getRole();
    const userId = session.getUserId();
    const email = session.getEmail();

    // Aquí dejo pasar el rol real del comprador.
    if (!["buyer", "customer", "distributor", "administrator", "admin"].includes(String(role || "").toLowerCase())) {
        notify.error("Acceso denegado. Requiere privilegios de Comprador.");
        setTimeout(() => {
            window.location.href = "/Login";
        }, 1500);
        return;
    }

    if (!userId) {
        notify.warning("No se pudo obtener el usuario de la sesión.");
        return;
    }

    // 1. PANEL PRINCIPAL (/Buyer/Dashboard)
    if (document.getElementById("buyActiveForecasts")) {
        let buyDemandChartInst = null;
        let buyBillingChartInst = null;

        loadBuyerDashboard();
        setInterval(loadBuyerDashboard, 15000);

        function loadBuyerDashboard() {
            apiClient.get("Dashboard/Buyer?buyerUserId=" + userId)
                .done(function (res) {
                    const data = res?.data || res?.Data || res || {};

                    const reqMWh = Number(data.monthRequestedMWh ?? data.MonthRequestedMWh ?? 0);
                    const assignMWh = Number(data.lastAssignment ?? data.LastAssignment ?? 0);
                    const totalBill = Number(data.totalBilledAccumulated ?? data.TotalBilledAccumulated ?? 0);
                    const activeF = Number(data.activeForecasts ?? data.ActiveForecasts ?? 0);
                    const currentMonthBill = Number(data.currentMonthBilled ?? data.CurrentMonthBilled ?? 0);
                    const paidAmount = Number(data.paidAmount ?? data.PaidAmount ?? 0);
                    const pendingAmount = Number(data.pendingAmount ?? data.PendingAmount ?? 0);

                    setText("buyActiveForecasts", activeF);
                    setText("buyMonthReq", formatNumber(reqMWh) + " MWh");
                    setText("buyLastAssign", formatNumber(assignMWh) + " MWh");
                    setText("buyTotalBilled", formatNumber(totalBill) + " CRC");

                    const lastStatementDate = data.lastStatementDate || data.LastStatementDate;
                    setText("buyLastStmtDate", lastStatementDate ? new Date(lastStatementDate).toLocaleDateString("es-CR") : "Sin registros");

                    const nextDueAmount = Number(data.nextDueAmount ?? data.NextDueAmount ?? 0);
                    const nextDueDate = data.nextDueDate || data.NextDueDate;
                    setText("buyNextDueAmount", formatNumber(nextDueAmount) + " CRC");
                    const dueLabel = document.getElementById("buyNextDueDate");
                    if (dueLabel) {
                        dueLabel.innerHTML = nextDueDate
                            ? '<i class="bi bi-arrow-right-short"></i> Vence el ' + new Date(nextDueDate).toLocaleDateString("es-CR")
                            : '<i class="bi bi-arrow-right-short"></i> Sin pendientes';
                    }

                    renderBuyCharts(reqMWh, assignMWh, paidAmount, pendingAmount || currentMonthBill);
                })
                .fail(function (xhr) {
                    handleApiError(xhr);
                });
        }

        function renderBuyCharts(reqMWh, assignMWh, paidAmount, pendingAmount) {
            if (typeof Chart === "undefined") return;

            const ctxDemand = document.getElementById("buyDemandChart")?.getContext("2d");
            if (ctxDemand) {
                if (buyDemandChartInst) {
                    buyDemandChartInst.data.datasets[0].data = [reqMWh, assignMWh];
                    buyDemandChartInst.update();
                } else {
                    buyDemandChartInst = new Chart(ctxDemand, {
                        type: "bar",
                        data: {
                            labels: ["Demanda Solicitada", "Asignación Recibida"],
                            datasets: [{
                                label: "Energía (MWh)",
                                data: [reqMWh, assignMWh],
                                backgroundColor: ["#2563EB", "#107C62"],
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

            const ctxBill = document.getElementById("buyBillingChart")?.getContext("2d");
            if (ctxBill) {
                if (buyBillingChartInst) {
                    buyBillingChartInst.data.datasets[0].data = [paidAmount, pendingAmount];
                    buyBillingChartInst.update();
                } else {
                    buyBillingChartInst = new Chart(ctxBill, {
                        type: "doughnut",
                        data: {
                            labels: ["Pagado", "Pendiente"],
                            datasets: [{
                                data: [paidAmount, pendingAmount],
                                backgroundColor: ["#5A2CA0", "#D97706"],
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
