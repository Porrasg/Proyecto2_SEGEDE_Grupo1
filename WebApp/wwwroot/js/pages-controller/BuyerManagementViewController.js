// BuyerManagementViewController.js (§22.1, §27) - Controlador JS para Pronósticos, Estados de Cuenta y Distribuciones
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando BuyerManagementViewController...");

    const role = session.getRole();
    const userId = session.getUserId() || 1;

    if (role !== "Distributor" && role !== "Administrator" && role !== "Admin") {
        notify.error("Acceso denegado. Requiere privilegios de Comprador.");
        setTimeout(() => {
            window.location.href = "/Login";
        }, 1500);
        return;
    }

    const monthsEs = ["", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"];

    // ==========================================
    // 1. MIS PRONÓSTICOS (/Buyer/Forecasts)
    // ==========================================
    const forecastsBody = document.getElementById("buyForecastsBody");
    if (forecastsBody) {
        let allForecasts = [];
        let editingForecastId = null;

        loadForecasts();

        const yearSelect = document.getElementById("buyForecastYear");
        if (yearSelect) yearSelect.addEventListener("change", filterAndRenderForecasts);

        const saveBtn = document.getElementById("saveForecastBtn");
        if (saveBtn) {
            saveBtn.addEventListener("click", function () {
                saveForecast();
            });
        }

        const modalEl = document.getElementById("forecastModal");
        if (modalEl) {
            modalEl.addEventListener("show.bs.modal", function () {
                if (!editingForecastId) {
                    document.getElementById("fAmount").value = "";
                }
            });
            modalEl.addEventListener("hidden.bs.modal", function () {
                editingForecastId = null;
                document.querySelector("#forecastModal .modal-title").textContent = "Registrar Pronóstico de Demanda";
            });
        }

        function loadForecasts() {
            forecastsBody.innerHTML = '<tr><td colspan="5" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando pronósticos...</td></tr>';
            apiClient.get("Forecasts/ByBuyer/" + userId)
                .done(function (res) {
                    allForecasts = res?.data || res?.Data || [];
                    filterAndRenderForecasts();
                })
                .fail(function (xhr) {
                    forecastsBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Error al cargar pronósticos.</td></tr>';
                    handleApiError(xhr);
                });
        }

        function filterAndRenderForecasts() {
            const year = parseInt(document.getElementById("buyForecastYear")?.value || 2026);
            const filtered = allForecasts.filter(f => (f.forecastYear || f.ForecastYear) === year);
            renderForecastsTable(filtered);
        }

        function renderForecastsTable(list) {
            if (!list.length) {
                forecastsBody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">No tienes pronósticos registrados para este año.</td></tr>';
                return;
            }

            forecastsBody.innerHTML = list.map(f => {
                const id = f.id || f.Id;
                const m = f.forecastMonth || f.ForecastMonth;
                const y = f.forecastYear || f.ForecastYear;
                const amt = Number(f.requestedEnergyMWh || f.RequestedEnergyMWh || 0).toLocaleString("es-CR", { minimumFractionDigits: 2 });
                const st = f.status || f.Status || "Pending";
                const badge = getStatusBadge(st);

                const canEdit = (st === "Pending" || st === "Modified");

                return `
                    <tr>
                        <td class="fw-bold">${monthsEs[m] || m}</td>
                        <td>${y}</td>
                        <td>${amt} MWh</td>
                        <td>${badge}</td>
                        <td>
                            ${canEdit ? `
                                <button class="btn btn-sm btn-outline-primary me-1 btn-edit-f" data-id="${id}" data-month="${m}" data-year="${y}" data-amt="${f.requestedEnergyMWh || f.RequestedEnergyMWh}" title="Modificar">
                                    <i class="bi bi-pencil"></i> Modificar
                                </button>
                                <button class="btn btn-sm btn-outline-danger btn-cancel-f" data-id="${id}" title="Cancelar Pronóstico">
                                    <i class="bi bi-x-circle"></i> Cancelar
                                </button>
                            ` : '<span class="text-muted small">Sin acciones</span>'}
                        </td>
                    </tr>
                `;
            }).join("");

            forecastsBody.querySelectorAll(".btn-edit-f").forEach(btn => {
                btn.addEventListener("click", () => openEditModal(btn.getAttribute("data-id"), btn.getAttribute("data-month"), btn.getAttribute("data-year"), btn.getAttribute("data-amt")));
            });

            forecastsBody.querySelectorAll(".btn-cancel-f").forEach(btn => {
                btn.addEventListener("click", () => cancelForecast(btn.getAttribute("data-id")));
            });
        }

        function openEditModal(id, month, year, amount) {
            editingForecastId = id;
            document.querySelector("#forecastModal .modal-title").textContent = "Modificar Pronóstico #" + id;
            document.getElementById("fMonth").value = month;
            document.getElementById("fYear").value = year;
            document.getElementById("fAmount").value = amount;
            new bootstrap.Modal(document.getElementById("forecastModal")).show();
        }

        function saveForecast() {
            const m = parseInt(document.getElementById("fMonth")?.value || 1);
            const y = parseInt(document.getElementById("fYear")?.value || 2026);
            const amt = parseFloat(document.getElementById("fAmount")?.value || 0);

            if (amt <= 0 || isNaN(amt)) {
                notify.warning("Por favor ingrese una cantidad válida de MWh mayor a 0.");
                return;
            }

            const btn = document.getElementById("saveForecastBtn");
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Guardando...';

            if (editingForecastId) {
                apiClient.put("Forecasts/Modify", {
                    forecastId: parseInt(editingForecastId),
                    newRequestedEnergyMWh: amt
                }).done(function () {
                    notify.success("Pronóstico de demanda actualizado exitosamente.");
                    bootstrap.Modal.getInstance(document.getElementById("forecastModal"))?.hide();
                    loadForecasts();
                }).fail(function (xhr) {
                    handleApiError(xhr);
                }).always(function () {
                    btn.disabled = false;
                    btn.textContent = "Guardar";
                });
            } else {
                apiClient.post("Forecasts/Register", {
                    forecastMonth: m,
                    forecastYear: y,
                    requestedEnergyMWh: amt
                }).done(function () {
                    notify.success("Nuevo pronóstico registrado y sujeto a distribución.");
                    bootstrap.Modal.getInstance(document.getElementById("forecastModal"))?.hide();
                    loadForecasts();
                }).fail(function (xhr) {
                    handleApiError(xhr);
                }).always(function () {
                    btn.disabled = false;
                    btn.textContent = "Guardar";
                });
            }
        }

        function cancelForecast(id) {
            notify.confirm("¿Está seguro que desea cancelar este pronóstico de demanda?", { dangerous: true, confirmText: "Cancelar pronóstico" }).then(function (ok) {
                if (!ok) return;
                apiClient.post("Forecasts/Cancel/" + id)
                    .done(function () {
                        notify.info("Pronóstico cancelado.");
                        loadForecasts();
                    })
                    .fail(function (xhr) {
                        handleApiError(xhr);
                    });
            });
        }
    }

    // ==========================================
    // 2. ESTADOS DE CUENTA (/Buyer/Statements)
    // ==========================================
    const stmtsBody = document.getElementById("buyStmtsBody");
    if (stmtsBody) {
        let allStatements = [];
        loadStatements();

        const stmtYear = document.getElementById("buyStmtYear");
        if (stmtYear) stmtYear.addEventListener("change", filterAndRenderStatements);

        function loadStatements() {
            stmtsBody.innerHTML = '<tr><td colspan="7" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando estados de cuenta...</td></tr>';
            apiClient.get("Billing/Statements?buyerId=" + userId)
                .done(function (res) {
                    allStatements = res?.data || res?.Data || [];
                    filterAndRenderStatements();
                })
                .fail(function (xhr) {
                    stmtsBody.innerHTML = '<tr><td colspan="7" class="text-center text-danger">Error al cargar estados de cuenta.</td></tr>';
                    handleApiError(xhr);
                });
        }

        function filterAndRenderStatements() {
            const year = parseInt(document.getElementById("buyStmtYear")?.value || 2026);
            const filtered = allStatements.filter(s => {
                const issueDate = s.issueDate ?? s.IssueDate;
                return issueDate && new Date(issueDate).getFullYear() === year;
            });
            renderStatementsTable(filtered);
        }

        // Entities-DTOs.Invoice.PaymentStatus real: Pending/Paid/Overdue/Cancelled (no "Issued"/"Annulled").
        function paymentStatusBadge(status) {
            const map = { Pending: ["bg-warning text-dark", "Pendiente"], Paid: ["bg-success", "Pagado"], Overdue: ["bg-danger", "Vencido"], Cancelled: ["bg-secondary", "Anulado"] };
            const [cls, txt] = map[status] || ["bg-secondary", status || "-"];
            return `<span class="badge ${cls}">${txt}</span>`;
        }

        function renderStatementsTable(list) {
            if (!list.length) {
                stmtsBody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">No tienes estados de cuenta emitidos en este período.</td></tr>';
                return;
            }

            stmtsBody.innerHTML = list.map(s => {
                const id = s.id || s.Id;
                const issueDate = new Date(s.issueDate ?? s.IssueDate);
                const m = issueDate.getMonth() + 1;
                const y = issueDate.getFullYear();
                const energy = Number(s.energyMWh || s.EnergyMWh || 0).toLocaleString("es-CR", { minimumFractionDigits: 2 });
                const sub = Number(s.subtotal || s.Subtotal || 0).toLocaleString("es-CR", { minimumFractionDigits: 2 });
                const tax = Number(s.taxAmount || s.TaxAmount || 0).toLocaleString("es-CR", { minimumFractionDigits: 2 });
                const total = Number(s.totalAmount || s.TotalAmount || 0).toLocaleString("es-CR", { minimumFractionDigits: 2 });
                const st = s.paymentStatus || s.PaymentStatus || "Pending";

                return `
                    <tr>
                        <td class="fw-bold">${monthsEs[m] || m} ${y}</td>
                        <td>${energy} MWh</td>
                        <td>₡${sub}</td>
                        <td>₡${tax}</td>
                        <td class="fw-bold text-success">₡${total}</td>
                        <td>${paymentStatusBadge(st)}</td>
                        <td>
                            <div class="btn-group btn-group-sm">
                                <button class="btn btn-outline-primary btn-view-stmt" data-idx="${list.indexOf(s)}" title="Ver Detalle"><i class="bi bi-eye"></i></button>
                                <button class="btn btn-outline-danger btn-export" data-id="${id}" data-fmt="PDF" title="Descargar PDF/HTML">PDF</button>
                                <button class="btn btn-outline-success btn-export" data-id="${id}" data-fmt="Excel" title="Descargar Excel">XLSX</button>
                                <button class="btn btn-outline-secondary btn-export" data-id="${id}" data-fmt="CSV" title="Descargar CSV">CSV</button>
                            </div>
                        </td>
                    </tr>
                `;
            }).join("");

            stmtsBody.querySelectorAll(".btn-export").forEach(btn => {
                btn.addEventListener("click", () => downloadStatement(btn.getAttribute("data-id"), btn.getAttribute("data-fmt"), btn));
            });
            stmtsBody.querySelectorAll(".btn-view-stmt").forEach(btn => {
                btn.addEventListener("click", () => showStatementDetail(list[parseInt(btn.getAttribute("data-idx"))]));
            });
        }

        const viewModalEl = document.getElementById("buyStmtViewModal");
        const viewModal = viewModalEl ? new bootstrap.Modal(viewModalEl) : null;

        function showStatementDetail(s) {
            if (!s) return;
            const issueDate = new Date(s.issueDate ?? s.IssueDate);
            const setText = (id, val) => { const el = document.getElementById(id); if (el) el.textContent = val; };
            setText("vsPeriod", `${monthsEs[issueDate.getMonth() + 1] || (issueDate.getMonth() + 1)} ${issueDate.getFullYear()}`);
            setText("vsAssigned", Number(s.energyMWh || s.EnergyMWh || 0).toLocaleString("es-CR", { minimumFractionDigits: 2 }) + " MWh");
            setText("vsUnitPrice", "₡" + Number(s.unitPrice || s.UnitPrice || 0).toLocaleString("es-CR", { minimumFractionDigits: 2 }));
            setText("vsSubtotal", "₡" + Number(s.subtotal || s.Subtotal || 0).toLocaleString("es-CR", { minimumFractionDigits: 2 }));
            // TaxPercentage ya se guarda como porcentaje (13 = 13%), no como fracción — InvoiceManager.Create hace Subtotal * (TaxPercentage / 100).
            setText("vsTax", `₡${Number(s.taxAmount || s.TaxAmount || 0).toLocaleString("es-CR", { minimumFractionDigits: 2 })} (${Number(s.taxPercentage || s.TaxPercentage || 0).toFixed(1)}%)`);
            setText("vsTotal", "₡" + Number(s.totalAmount || s.TotalAmount || 0).toLocaleString("es-CR", { minimumFractionDigits: 2 }));
            const status = s.paymentStatus || s.PaymentStatus || "Pending";
            const statusEl = document.getElementById("vsStatus");
            if (statusEl) statusEl.innerHTML = paymentStatusBadge(status);
            // Invoice no tiene un campo de motivo de anulación en el backend actual.
            setText("vsAnnulReason", "-");
            setText("vsIssueDate", issueDate.toLocaleString("es-CR"));
            viewModal?.show();
        }

        function downloadStatement(id, format, btn) {
            const origText = btn.textContent;
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';

            fetch(apiClient.url("Billing/Export?callerUserId=" + userId + "&callerRole=Buyer"), {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ statementId: parseInt(id), format: format })
            }).then(response => {
                if (!response.ok) {
                    console.warn('[SGDE apiClient] POST Billing/Export → HTTP ' + response.status);
                    throw new Error(response.status === 404 || response.status === 501
                        ? "Este módulo está en construcción: la exportación aún no está disponible en el servidor."
                        : "Error al exportar documento.");
                }
                return response.blob();
            }).then(blob => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement("a");
                a.style.display = "none";
                a.href = url;
                const ext = format.toUpperCase() === "EXCEL" ? "xlsx" : format.toLowerCase();
                a.download = `EstadoCuenta_${id}_${format}.${ext}`;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                notify.success(`Estado de cuenta #${id} descargado en formato ${format}.`);
            }).catch(err => {
                notify.error("No se pudo descargar el archivo: " + err.message);
            }).finally(() => {
                btn.disabled = false;
                btn.textContent = origText;
            });
        }
    }

    // ==========================================
    // 3. DISTRIBUCIONES (/Buyer/Distributions)
    // ==========================================
    const distBody = document.getElementById("buyDistBody");
    if (distBody) {
        loadDistributions();

        function loadDistributions() {
            distBody.innerHTML = '<tr><td colspan="5" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando distribuciones...</td></tr>';
            apiClient.get("Distributions/ByBuyer/" + userId)
                .done(function (res) {
                    const list = res?.data || res?.Data || [];
                    renderDistTable(list);
                })
                .fail(function (xhr) {
                    distBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Error al cargar distribuciones.</td></tr>';
                    handleApiError(xhr);
                });
        }

        function renderDistTable(list) {
            if (!list.length) {
                distBody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Aún no participas en cierres de distribución comercial.</td></tr>';
                return;
            }

            distBody.innerHTML = list.map(d => {
                const req = Number(d.requestedEnergyMWh || d.RequestedEnergyMWh || 0).toLocaleString("es-CR", { minimumFractionDigits: 2 });
                const assign = Number(d.assignedEnergyMWh || d.AssignedEnergyMWh || 0).toLocaleString("es-CR", { minimumFractionDigits: 2 });
                const unassigned = Number(d.unassignedEnergyMWh || d.UnassignedEnergyMWh || 0).toLocaleString("es-CR", { minimumFractionDigits: 2 });

                // Estado real de la fila (lo calcula DistributionManager.Create: Completed = 100% cubierto, Partial = prorrateado).
                const st = d.status || d.Status || "Completed";
                let scBadge = '<span class="badge bg-success">Suficiencia 100%</span>';
                if (st === "Partial") scBadge = '<span class="badge bg-warning text-dark">Escasez (Prorrateo)</span>';
                if (st === "Cancelled") scBadge = '<span class="badge bg-secondary">Cancelada</span>';

                const dateStr = d.distributionDate || d.DistributionDate ? new Date(d.distributionDate || d.DistributionDate).toLocaleDateString("es-CR", { month: 'long', year: 'numeric' }) : `Cierre #${d.distributionBatchId || d.DistributionBatchId}`;

                return `
                    <tr>
                        <td class="text-capitalize fw-bold">${dateStr}</td>
                        <td>${req} MWh</td>
                        <td class="text-success fw-bold">${assign} MWh</td>
                        <td class="${st === 'Partial' ? 'text-danger fw-bold' : ''}">${unassigned} MWh</td>
                        <td>${scBadge}</td>
                    </tr>
                `;
            }).join("");
        }
    }

    function getStatusBadge(st) {
        const s = (st || "").toLowerCase();
        if (s === "pending") return '<span class="badge bg-warning text-dark">Pendiente</span>';
        if (s === "modified") return '<span class="badge bg-info text-dark">Modificado</span>';
        if (s === "distributed") return '<span class="badge bg-success">Distribuido</span>';
        if (s === "blocked") return '<span class="badge bg-secondary">Bloqueado</span>';
        if (s === "cancelled") return '<span class="badge bg-danger">Cancelado</span>';
        return `<span class="badge bg-light text-dark">${st || "-"}</span>`;
    }
});
