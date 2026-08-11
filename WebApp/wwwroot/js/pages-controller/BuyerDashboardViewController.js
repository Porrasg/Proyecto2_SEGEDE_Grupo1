// BuyerDashboardViewController.js (§22.1, §27) - Controlador JS para el Panel de Comprador y Perfil
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando BuyerDashboardViewController...");

    const role = session.getRole();
    const userId = session.getUserId() || 1;

    if (role !== "Distributor" && role !== "Administrator" && role !== "Admin") {
        notify.error("Acceso denegado. Requiere privilegios de Comprador.");
        setTimeout(() => {
            window.location.href = "/Login";
        }, 1500);
        return;
    }

    // 1. PANEL PRINCIPAL (/Buyer/Dashboard)
    if (document.getElementById("buyActiveForecasts")) {
        let buyDemandChartInst = null;
        let buyBillingChartInst = null;

        loadBuyerDashboard();
        setInterval(loadBuyerDashboard, 15000);

        function loadBuyerDashboard() {
            Promise.all([
                apiClient.get("Forecasts/RetrieveByBuyerId/" + userId),
                apiClient.get("Distributions/RetrieveByBuyerId/" + userId),
                apiClient.get("Invoices/RetrieveAll")
            ]).then(function (responses) {
                const forecasts = responses[0]?.[0]?.data || responses[0]?.data || responses[0]?.Data || [];
                const distributions = responses[1]?.[0]?.data || responses[1]?.data || responses[1]?.Data || [];
                const invoices = responses[2]?.[0]?.data || responses[2]?.data || responses[2]?.Data || [];

                const reqMWh = forecasts.reduce(function (sum, f) { return sum + Number(f.requestedEnergyMWh ?? f.RequestedEnergyMWh ?? 0); }, 0);
                const assignMWh = distributions.reduce(function (sum, d) { return sum + Number(d.assignedEnergyMWh ?? d.AssignedEnergyMWh ?? 0); }, 0);
                const totalBill = invoices
                    .filter(function (i) { return (i.buyerId ?? i.BuyerId) === userId; })
                    .reduce(function (sum, i) { return sum + Number(i.totalAmount ?? i.TotalAmount ?? i.amount ?? i.Amount ?? 0); }, 0);
                const activeF = forecasts.filter(function (f) { return (f.status || f.Status) === "Active"; }).length;

                setText("buyActiveForecasts", activeF);
                setText("buyMonthReq", formatNumber(reqMWh) + " MWh");
                setText("buyLastAssign", formatNumber(assignMWh) + " MWh");
                setText("buyTotalBilled", formatNumber(totalBill) + " CRC");

                const lastInvoice = invoices.filter(function (i) { return (i.buyerId ?? i.BuyerId) === userId; }).sort(function (a, b) {
                    return new Date(b.createdAt || b.CreatedAt || 0) - new Date(a.createdAt || a.CreatedAt || 0);
                })[0];
                const dateVal = lastInvoice?.createdAt || lastInvoice?.CreatedAt;
                setText("buyLastStmtDate", dateVal ? new Date(dateVal).toLocaleDateString("es-CR") : "Sin registros");

                // Próxima factura pendiente: la de PaymentStatus "Pending" con la fecha
                // de vencimiento (DueDate) más próxima.
                const pendingInvoices = invoices
                    .filter(function (i) { return (i.buyerId ?? i.BuyerId) === userId && (i.paymentStatus || i.PaymentStatus) === "Pending"; })
                    .sort(function (a, b) { return new Date(a.dueDate || a.DueDate) - new Date(b.dueDate || b.DueDate); });
                const nextDue = pendingInvoices[0];
                if (nextDue) {
                    setText("buyNextDueAmount", formatNumber(Number(nextDue.totalAmount ?? nextDue.TotalAmount ?? 0)) + " CRC");
                    const dueDateVal = nextDue.dueDate || nextDue.DueDate;
                    const dueLabel = document.getElementById("buyNextDueDate");
                    if (dueLabel) dueLabel.innerHTML = '<i class="bi bi-arrow-right-short"></i> Vence el ' + (dueDateVal ? new Date(dueDateVal).toLocaleDateString("es-CR") : "-");
                } else {
                    setText("buyNextDueAmount", "0 CRC");
                    const dueLabel = document.getElementById("buyNextDueDate");
                    if (dueLabel) dueLabel.innerHTML = '<i class="bi bi-arrow-right-short"></i> Sin pendientes';
                }

                renderBuyCharts(reqMWh, assignMWh, totalBill);
            }).catch(function (xhr) {
                handleApiError(xhr);
            });
        }

        function renderBuyCharts(reqMWh, assignMWh, totalBill) {
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
                    buyBillingChartInst.data.datasets[0].data = [totalBill, Math.max(0, totalBill * 0.15)];
                    buyBillingChartInst.update();
                } else {
                    buyBillingChartInst = new Chart(ctxBill, {
                        type: "doughnut",
                        data: {
                            labels: ["Facturado Pagado", "Impuestos / Pendiente"],
                            datasets: [{
                                data: [totalBill, Math.max(0, totalBill * 0.15)],
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
    
    function formatNumber(num) {
        return Number(num).toLocaleString("es-CR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }
});
