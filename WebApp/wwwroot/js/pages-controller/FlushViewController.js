// FlushViewController.js (§60, §85 Admin/Flush) - Configuración y ejecución de vaciado (Flush) hacia el Banco Central
document.addEventListener("DOMContentLoaded", function () {
    const configForm = document.getElementById("flushConfigForm");
    if (!configForm) return;

    const timeInput = document.getElementById("flushTime");
    const autoCheck = document.getElementById("flushAuto");
    const statusBadge = document.getElementById("flushStatusBadge");
    const manualBtn = document.getElementById("btnManualFlush");
    const historyBody = document.getElementById("flushHistoryBody");

    loadConfig();
    loadHistory();
    checkActiveStatus();

    function loadConfig() {
        apiClient.get("Flushs/Config")
            .done(function (res) {
                const c = res?.data || res?.Data || {};
                if (timeInput && c.executionTime) timeInput.value = String(c.executionTime).substring(0, 5);
                if (autoCheck) autoCheck.checked = !!(c.isAutomatic ?? c.IsAutomatic);
            })
            .fail(function (xhr) { handleApiError(xhr); });
    }

    function checkActiveStatus() {
        apiClient.get("Flushs/History?page=1&pageSize=1")
            .done(function () {
                // El estado activo real se refleja en el badge tras cada acción; consulta ligera de disponibilidad.
                if (statusBadge) { statusBadge.textContent = "Inactivo"; statusBadge.className = "badge bg-success"; }
            });
    }

    function loadHistory() {
        if (!historyBody) return;
        historyBody.innerHTML = '<tr><td colspan="7" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando historial...</td></tr>';
        apiClient.get("Flushs/History?page=1&pageSize=50")
            .done(function (res) {
                const items = res?.data?.items || res?.Data?.Items || [];
                if (!items.length) {
                    historyBody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">Sin operaciones de flush registradas.</td></tr>';
                    return;
                }
                historyBody.innerHTML = items.map(function (f) {
                    const status = f.status || f.Status || "-";
                    const badge = status === "Completed" ? "bg-success" : status === "Failed" ? "bg-danger" : "bg-warning text-dark";
                    return `<tr>
                        <td>${f.id || f.Id}</td>
                        <td>${f.executionType || f.ExecutionType || "-"}</td>
                        <td>${new Date(f.startDate || f.StartDate).toLocaleString("es-CR")}</td>
                        <td>${(f.endDate || f.EndDate) ? new Date(f.endDate || f.EndDate).toLocaleString("es-CR") : "-"}</td>
                        <td>${Number(f.totalTransferredEnergy ?? f.TotalTransferredEnergy ?? 0).toLocaleString("es-CR", { minimumFractionDigits: 2 })}</td>
                        <td>${Number(f.saturationLoss ?? f.SaturationLoss ?? 0).toLocaleString("es-CR", { minimumFractionDigits: 2 })}</td>
                        <td><span class="badge ${badge}">${status}</span></td>
                    </tr>`;
                }).join("");
            })
            .fail(function (xhr) {
                historyBody.innerHTML = '<tr><td colspan="7" class="text-center text-danger">Error al cargar el historial de flush.</td></tr>';
                handleApiError(xhr);
            });
    }

    configForm.addEventListener("submit", function (e) {
        e.preventDefault();
        const btn = configForm.querySelector("button[type='submit']");
        const original = btn ? btn.innerHTML : "";
        if (btn) { btn.disabled = true; btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Guardando...'; }

        apiClient.put("Flushs/Config", {
            executionTime: (timeInput?.value || "00:00") + ":00",
            isAutomatic: !!autoCheck?.checked
        }).done(function () {
            notify.success("Configuración de flush actualizada.");
        }).fail(function (xhr) {
            handleApiError(xhr);
        }).always(function () {
            if (btn) { btn.disabled = false; btn.innerHTML = original; }
        });
    });

    if (manualBtn) {
        manualBtn.addEventListener("click", function () {
            notify.confirm("¿Ejecutar el flush manual ahora? Esta acción trasladará toda la energía disponible de las baterías locales al Banco Central y no se puede deshacer.", { dangerous: true, confirmText: "Ejecutar flush" }).then(function (ok) {
                if (!ok) return;
                manualBtn.disabled = true;
                const original = manualBtn.innerHTML;
                manualBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Ejecutando...';
                if (statusBadge) { statusBadge.textContent = "En ejecución"; statusBadge.className = "badge bg-warning text-dark"; }

                apiClient.post("Flushs/ExecuteManual", {})
                    .done(function () {
                        notify.success("Flush ejecutado con éxito hacia el Banco Central.");
                        loadHistory();
                    })
                    .fail(function (xhr) { handleApiError(xhr); })
                    .always(function () {
                        manualBtn.disabled = false;
                        manualBtn.innerHTML = original;
                        if (statusBadge) { statusBadge.textContent = "Inactivo"; statusBadge.className = "badge bg-success"; }
                    });
            });
        });
    }
});
