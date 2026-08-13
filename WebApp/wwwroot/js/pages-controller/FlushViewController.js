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
    apiClient.get("Flushs/RetrieveConfiguration")
        .done(function (res) {
            const config = res?.data || res?.Data || res;

            if (!config) {
                notify.error("No se encontró la configuración de Flush.");
                return;
            }

            if (timeInput) {
                const time = config.executionTime || config.ExecutionTime;

                if (time) {
                    timeInput.value = time.substring(0, 5);
                }
            }

            if (autoCheck) {
                autoCheck.checked =
                    config.isAutomatic ?? config.IsAutomatic ?? false;
            }
        })
        .fail(function (xhr) {
            handleApiError(xhr);
        });
}

    function checkActiveStatus() {
        if (statusBadge) { statusBadge.textContent = "Inactivo"; statusBadge.className = "badge bg-success"; }
    }

    function loadHistory() {
        if (!historyBody) return;
        historyBody.innerHTML = '<tr><td colspan="6" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando historial...</td></tr>';
        apiClient.get("Flushs/RetrieveAll")
            .done(function (res) {
                   const items = Array.isArray(res)
                    ? res
                    : (res?.data || res?.Data || []);

                if (!items.length) {
                    historyBody.innerHTML =
                        '<tr><td colspan="6" class="text-center text-muted">Sin operaciones de flush registradas.</td></tr>';
                    return;
                }
                historyBody.innerHTML = items.map(function (f) {
                    const status = f.status || f.Status || "-";
                    const badge = status === "Completed" ? "bg-success" : status === "Failed" ? "bg-danger" : "bg-warning text-dark";
                    return `<tr>
                        <td>${f.id || f.Id}</td>
                        <td>${f.executionType || f.ExecutionType || "-"}</td>
                        <td>${new Date(f.executedAt || f.ExecutedAt).toLocaleString("es-CR")}</td>
                        <td>${Number(f.transferredEnergyMWh ?? f.TransferredEnergyMWh ?? 0).toLocaleString("es-CR", { minimumFractionDigits: 2 })}</td>
                        <td>${Number(f.saturationLossMWh ?? f.SaturationLossMWh ?? 0).toLocaleString("es-CR", { minimumFractionDigits: 2 })}</td>
                        <td><span class="badge ${badge}">${status}</span></td>
                    </tr>`;
                }).join("");
            })
            .fail(function (xhr) {
                historyBody.innerHTML = '<tr><td colspan="6" class="text-center text-danger">Error al cargar el historial de flush.</td></tr>';
                handleApiError(xhr);
            });
    }

        configForm.addEventListener("submit", function (e) {
        e.preventDefault();

        const btn = configForm.querySelector("button[type='submit']");
        const original = btn ? btn.innerHTML : "";

        const executionTime = timeInput ? timeInput.value : "";
        const isAutomatic = autoCheck ? autoCheck.checked : false;

        if (!executionTime) {
            notify.error("Debe seleccionar una hora de ejecución.");
            return;
        }

        if (btn) {
            btn.disabled = true;
            btn.innerHTML =
                '<span class="spinner-border spinner-border-sm"></span> Guardando...';
        }

        apiClient.put("Flushs/UpdateConfiguration", {
            id: 1,
            executionTime: executionTime + ":00",
            isAutomatic: isAutomatic
        })
            .done(function (res) {
                notify.success("Configuración de Flush guardada correctamente.");
            })
            .fail(function (xhr) {
                handleApiError(xhr);
            })
            .always(function () {
                if (btn) {
                    btn.disabled = false;
                    btn.innerHTML = original;
                }
            });
    });

    // Para el botón de ejecución manual
    if (manualBtn) {
        manualBtn.addEventListener("click", function () {
            notify.confirm(
                "¿Ejecutar el flush manual ahora? Esta acción trasladará toda la energía disponible de las baterías locales al Banco Central y no se puede deshacer.",
                {
                    dangerous: true,
                    confirmText: "Ejecutar flush"
                }
            ).then(function (ok) {

                if (!ok) return;

                manualBtn.disabled = true;

                const original = manualBtn.innerHTML;

                manualBtn.innerHTML =
                    '<span class="spinner-border spinner-border-sm"></span> Ejecutando...';

                if (statusBadge) {
                    statusBadge.textContent = "En ejecución";
                    statusBadge.className = "badge bg-warning text-dark";
                }

                apiClient.post("Flushs/ExecuteManual")
                    .done(function (res) {

                        notify.success(
                            res?.message ||
                            res?.Message ||
                            "Flush manual ejecutado correctamente."
                        );

                        if (statusBadge) {
                            statusBadge.textContent = "Completado";
                            statusBadge.className = "badge bg-success";
                        }

                        // Actualizar el historial después del flush
                        loadHistory();
                    })
                    .fail(function (xhr) {

                        handleApiError(xhr);

                        if (statusBadge) {
                            statusBadge.textContent = "Error";
                            statusBadge.className = "badge bg-danger";
                        }
                    })
                    .always(function () {

                        manualBtn.disabled = false;
                        manualBtn.innerHTML = original;

                        setTimeout(function () {
                            if (statusBadge) {
                                statusBadge.textContent = "Inactivo";
                                statusBadge.className = "badge bg-success";
                            }
                        }, 3000);
                    });
            });
        });
    }
});