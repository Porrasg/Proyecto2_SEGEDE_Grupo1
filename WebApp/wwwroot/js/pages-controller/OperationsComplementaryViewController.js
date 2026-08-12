// OperationsComplementaryViewController.js (§22.1, §27) - Controlador para Energía, Mantenimientos y Averías
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando OperationsComplementaryViewController...");

    const role = session.getRole();
    const userId = session.getUserId() || 1;

    if (role !== "Engineer" && role !== "Administrator" && role !== "Admin") {
        notify.error("Acceso denegado. Requiere privilegios de Ingeniero u Operaciones.");
        setTimeout(() => {
            window.location.href = "/Login";
        }, 1500);
        return;
    }

    let allTurbinesMap = {};

    apiClient.get("Turbines/RetrieveAll").done(function (res) {

        // Pruebas temporales para verificar la estructura de la respuesta
        console.log("Turbines respuesta =", res);
        console.log("¿Es arreglo? =", Array.isArray(res));
        console.log("res.data =", res?.data);

        // Soporta respuestas que vengan como arreglo directo o dentro de propiedades como data, Data o items
        const list = Array.isArray(res)
            ? res
            : (res?.data?.items ||
                res?.data?.Data ||
                res?.data ||
                res?.Data ||
                []);

        console.log("Lista de turbinas =", list);

        // Construir mapa de turbinas para llenar los selectores
        list.forEach(t => {
            const id = t.id ?? t.Id;
            const code = t.code ?? t.Code ?? ("Turbina #" + id);

            allTurbinesMap[id] = code;
        });

        // Inicializar los módulos después de cargar las turbinas
        initEnergyModule();
        initMaintenancesModule();
        initFailuresModule();

    }).fail(function (xhr) {

        console.error("Error al cargar turbinas:", xhr);

        initEnergyModule();
        initMaintenancesModule();
        initFailuresModule();
    });

    
    // 1. MONITORIZACIÓN ENERGÉTICA (/Engineer/Energy)
 
    function initEnergyModule() {
        const energyTurbineSelect = document.getElementById("engEnergyTurbine");
        if (!energyTurbineSelect) return;

        populateTurbineSelect(energyTurbineSelect, false);

        energyTurbineSelect.addEventListener("change", function () {
            loadEnergyData(this.value);
        });

        // Si ya hay turbinas en el select, cargar la primera
        setTimeout(() => {
            if (energyTurbineSelect.value) loadEnergyData(energyTurbineSelect.value);
        }, 500);
    }

    function loadEnergyData(turbineId) {

        if (!turbineId) return;

        // Obtiene los elementos HTML donde se mostrarán los historiales
        const genBody = document.getElementById("engGenBody");
        const lossBody = document.getElementById("engLossBody");
        const batLevel = document.getElementById("engBatLevel");
        const batCap = document.getElementById("engBatCap");

        if (genBody) {
            genBody.innerHTML =
                '<tr><td colspan="4" class="text-center">' +
                '<span class="spinner-border spinner-border-sm"></span> ' +
                'Cargando generación...</td></tr>';
        }

        if (lossBody) {
            lossBody.innerHTML =
                '<tr><td colspan="4" class="text-center">' +
                '<span class="spinner-border spinner-border-sm"></span> ' +
                'Cargando pérdidas...</td></tr>';
        }

        // 1. BATERIA LOCAL 

        // Obtiene la batería asociada a la turbina seleccionada.
        apiClient.get("Energy/LocalBattery/" + turbineId).done(function (res) {

            // Obtiene los datos de la batería independientemente de si la API los devuelve dentro de data/Data o directamente en la respuesta.
            const b = res?.data || res?.Data || res || {};

            // Muestra la cantidad de energía almacenada actualmente en la batería // Si la propiedad no existe, utiliza 0 como valor predeterminado
            if (batLevel) {
                batLevel.textContent =
                    formatNum(
                        b.currentEnergyMWh ??
                        b.CurrentEnergyMWh ??
                        0
                    ) + " MWh";
            }

            // Muestra la capacidad máxima de almacenamiento de la batería.// Si la propiedad no existe, utiliza 0 como valor predeterminado.
            if (batCap) {
                batCap.textContent =
                    formatNum(
                        b.maximumCapacityMWh ??
                        b.MaximumCapacityMWh ??
                        0
                    ) + " MWh";
            }
        });
        

        // 2. HISTORIAL DE GENERACION DE ENERGIA

        // Obtiene los últimos registros de generación de energía de la turbina seleccionada.// Se solicita la primera página con un máximo de 20 registros.
        apiClient.get("Energy/GenerationHistory/" + turbineId + "?page=1&pageSize=20").done(function (res) {

            // Obtiene la lista de registros independientemente de si la API devuelve los datos dentro de items, data/Data o directamente.// Si no existen registros, utiliza un arreglo vacío.\            
            const list =
                res?.data?.items ||
                res?.Data?.Items ||
                res?.data ||
                res?.Data ||
                [];

            // Envia la data obtenida para que sea mostrada en la tabla correspondiente. 
            renderGenTable(list);
        });

        // 3. HISTORIAL DE PERDIDAS

        // Obtiene los últimos registros de pérdidas de energía de la turbina seleccionada
        apiClient.get("Energy/LossHistory/" + turbineId + "?page=1&pageSize=20").done(function (res) {

            // Obtiene la lista de pérdidas independientemente de si la API// devuelve los datos dentro de items, data/Data o directamente.
            const list =
                res?.data?.items ||
                res?.Data?.Items ||
                res?.data ||
                res?.Data ||
                [];

            // Envía los registros obtenidos al método encargado
            renderLossTable(list);
        });
    }

    function renderGenTable(list) {
        const body = document.getElementById("engGenBody");
        if (!body) return;
        if (!list.length) {
            body.innerHTML = '<tr><td colspan="4" class="text-center text-muted">Sin registros recientes de generación.</td></tr>';
            return;
        }
        body.innerHTML = list.map(g => `
            <tr>
                <td>#${g.id || g.Id}</td>
                <td>${formatDateTime(g.timestamp || g.Timestamp)}</td>
                <td class="fw-bold text-success">${formatNum(g.generatedMWh || g.GeneratedMWh)} MWh</td>
                <td>${g.windSpeedMs || g.WindSpeedMs || "-"} m/s</td>
            </tr>
        `).join("");
    }

    function renderLossTable(list) {
        const body = document.getElementById("engLossBody");
        if (!body) return;
        if (!list.length) {
            body.innerHTML = '<tr><td colspan="4" class="text-center text-muted">Sin pérdidas térmicas o por transporte reportadas.</td></tr>';
            return;
        }
        body.innerHTML = list.map(l => `
            <tr>
                <td>#${l.id || l.Id}</td>
                <td>${formatDateTime(l.timestamp || l.Timestamp)}</td>
                <td class="fw-bold text-danger">-${formatNum(l.lostMWh || l.LostMWh)} MWh</td>
                <td>${escapeHtml(l.reason || l.Reason || "Pérdida operativa en red")}</td>
            </tr>
        `).join("");
    }

   
    // 2. MANTENIMIENTOS (/Engineer/Maintenances)
    function initMaintenancesModule() {
        const maintsBody = document.getElementById("engMaintsBody");
        if (!maintsBody) return;

        let editingMaintId = null;

        const filterTurbine = document.getElementById("engMaintTurbine");
        const filterStatus = document.getElementById("engMaintStatus");
        const modalTurbine = document.getElementById("mTurbine");

        populateTurbineSelect(filterTurbine, true);
        populateTurbineSelect(modalTurbine, false);

        loadMaintenances();

        if (filterTurbine) filterTurbine.addEventListener("change", loadMaintenances);
        if (filterStatus) filterStatus.addEventListener("change", loadMaintenances);

        const saveBtn = document.getElementById("saveMaintBtn");
        if (saveBtn) saveBtn.addEventListener("click", scheduleMaintenance);

        const compBtn = document.getElementById("confirmCompBtn");
        if (compBtn) compBtn.addEventListener("click", completeMaintenance);

        function loadMaintenances() {
            maintsBody.innerHTML = '<tr><td colspan="7" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando mantenimientos...</td></tr>';
            const tid = filterTurbine?.value;
            const endpoint = tid ? ("Maintenances/ByTurbine/" + tid) : "Maintenances/All";

            apiClient.get(endpoint).done(function (res) {
                let list = res?.data || res?.Data || [];
                const st = filterStatus?.value;
                if (st) {
                    list = list.filter(m => (m.status || m.Status) === st);
                }
                renderMaintsTable(list);
            }).fail(function (xhr) {
                maintsBody.innerHTML = '<tr><td colspan="7" class="text-center text-danger">Error al obtener historial de mantenimientos.</td></tr>';
                handleApiError(xhr);
            });
        }

        function renderMaintsTable(list) {
            if (!list.length) {
                maintsBody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">No se encontraron registros de mantenimiento.</td></tr>';
                return;
            }

            maintsBody.innerHTML = list.map(m => {
                const id = m.id || m.Id;
                const tid = m.turbineId || m.TurbineId;
                const tCode = allTurbinesMap[tid] || ("Turbina #" + tid);
                const type = m.maintenanceType || m.MaintenanceType || "Preventive";
                const typeBadges = {
                    Preventive: '<span class="badge bg-info text-dark">Preventivo</span>',
                    Corrective: '<span class="badge bg-warning text-dark">Correctivo</span>',
                    Predictive: '<span class="badge bg-primary">Predictivo</span>',
                    Inspection: '<span class="badge bg-secondary">Inspección</span>',
                    Emergency: '<span class="badge bg-danger">Emergencia</span>'
                };
                const typeBadge = typeBadges[type] || `<span class="badge bg-secondary">${type}</span>`;

                const estStart = formatDateTime(m.estimatedStartDate || m.EstimatedStartDate);
                const estEnd = formatDateTime(m.estimatedEndDate || m.EstimatedEndDate);
                const realStart = formatDateTime(m.actualStartDate || m.ActualStartDate);
                const realEnd = formatDateTime(m.actualEndDate || m.ActualEndDate);

                const st = m.status || m.Status || "Scheduled";
                let stBadge = '<span class="badge bg-secondary">' + st + '</span>';
                if (st === "Scheduled") stBadge = '<span class="badge bg-primary">Programado</span>';
                if (st === "InProgress") stBadge = '<span class="badge bg-warning text-dark">En Curso</span>';
                if (st === "Completed") stBadge = '<span class="badge bg-success">Completado</span>';
                if (st === "Cancelled") stBadge = '<span class="badge bg-danger">Cancelado</span>';

                const canComplete = (st === "Scheduled" || st === "InProgress");
                const canCancel = (st === "Scheduled");

                return `
                    <tr>
                        <td>#${id}</td>
                        <td class="fw-bold">${tCode}</td>
                        <td>${typeBadge}</td>
                        <td class="small"><b>Inicio:</b> ${estStart}<br><b>Fin:</b> ${estEnd}</td>
                        <td class="small">${realStart !== "-" ? `<b>Inicio:</b> ${realStart}<br><b>Fin:</b> ${realEnd}` : '<span class="text-muted">Pendiente</span>'}</td>
                        <td>${stBadge}</td>
                        <td>
                            ${canComplete ? `<button class="btn btn-sm btn-success btn-comp-m mb-1" data-id="${id}" title="Completar Mantenimiento"><i class="bi bi-check-circle"></i> Completar</button>` : ""}
                            ${canCancel ? `<button class="btn btn-sm btn-outline-danger btn-canc-m mb-1" data-id="${id}" title="Cancelar"><i class="bi bi-x-circle"></i></button>` : ""}
                            ${!canComplete && !canCancel ? '<span class="text-muted small">Sin acciones</span>' : ""}
                        </td>
                    </tr>
                `;
            }).join("");

            maintsBody.querySelectorAll(".btn-comp-m").forEach(btn => {
                btn.addEventListener("click", () => {
                    editingMaintId = btn.getAttribute("data-id");
                    document.getElementById("mResult").value = "";
                    new bootstrap.Modal(document.getElementById("compMaintModal")).show();
                });
            });

            maintsBody.querySelectorAll(".btn-canc-m").forEach(btn => {
                btn.addEventListener("click", () => cancelMaintenance(btn.getAttribute("data-id")));
            });
        }

        function scheduleMaintenance() {
            const tid = parseInt(document.getElementById("mTurbine")?.value || 0);
            const type = document.getElementById("mType")?.value;
            const start = document.getElementById("mStart")?.value;
            const end = document.getElementById("mEnd")?.value;

            if (!tid || !start || !end) {
                notify.warning("Por favor complete todos los campos obligatorios del mantenimiento.");
                return;
            }

            const btn = document.getElementById("saveMaintBtn");
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Programando...';

            apiClient.post("Maintenances/Schedule?callerUserId=" + userId, {
                turbineId: tid,
                maintenanceType: type,
                estimatedStartDate: new Date(start).toISOString(),
                estimatedEndDate: new Date(end).toISOString()
            }).done(function () {
                notify.success("Mantenimiento programado y turbina en transición de estado.");
                bootstrap.Modal.getInstance(document.getElementById("regMaintModal"))?.hide();
                loadMaintenances();
            }).fail(function (xhr) {
                handleApiError(xhr);
            }).always(function () {
                btn.disabled = false;
                btn.textContent = "Programar";
            });
        }

        function completeMaintenance() {
            const resultText = document.getElementById("mResult")?.value.trim();
            if (!resultText) {
                notify.warning("Especifique un informe técnico o resultado para completar la labor.");
                return;
            }

            const btn = document.getElementById("confirmCompBtn");
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Procesando...';

            apiClient.post("Maintenances/Complete?callerUserId=" + userId, {
                maintenanceId: parseInt(editingMaintId),
                result: resultText
            }).done(function () {
                notify.success("Mantenimiento finalizado y turbina reactivada operacionalmente.");
                bootstrap.Modal.getInstance(document.getElementById("compMaintModal"))?.hide();
                loadMaintenances();
            }).fail(function (xhr) {
                handleApiError(xhr);
            }).always(function () {
                btn.disabled = false;
                btn.textContent = "Marcar como Completado";
            });
        }

        function cancelMaintenance(id) {
            notify.confirm("¿Está seguro de cancelar este mantenimiento programado?", { dangerous: true, confirmText: "Cancelar mantenimiento" }).then(function (ok) {
                if (!ok) return;
                apiClient.post("Maintenances/Cancel/" + id).done(function () {
                    notify.info("Mantenimiento cancelado.");
                    loadMaintenances();
                }).fail(function (xhr) {
                    handleApiError(xhr);
                });
            });
        }
    }


    // 3. AVERÍAS Y FALLAS (/Engineer/Failures)
    function initFailuresModule() {
        const failsBody = document.getElementById("engFailsBody");
        if (!failsBody) return;

        const filterTurbine = document.getElementById("engFailTurbine");
        const modalTurbine = document.getElementById("fTurbine");

        populateTurbineSelect(filterTurbine, true);
        populateTurbineSelect(modalTurbine, false);

        loadFailures();

        if (filterTurbine) filterTurbine.addEventListener("change", loadFailures);

        const saveBtn = document.getElementById("saveFailBtn");
        if (saveBtn) saveBtn.addEventListener("click", registerFailure);

        function loadFailures() {
            failsBody.innerHTML = '<tr><td colspan="5" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando averías y alertas...</td></tr>';
            const tid = filterTurbine?.value;
            const endpoint = tid ? ("Failures/ByTurbine/" + tid) : "Failures/All";

            apiClient.get(endpoint).done(function (res) {
                const list = res?.data || res?.Data || [];
                renderFailsTable(list);
            }).fail(function (xhr) {
                failsBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Error al consultar el registro de fallas.</td></tr>';
                handleApiError(xhr);
            });
        }

        function renderFailsTable(list) {
            if (!list.length) {
                failsBody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">No se reportan fallas ni incidencias operativas en el parque eólico.</td></tr>';
                return;
            }

            failsBody.innerHTML = list.map(f => {
                const id = f.id || f.Id;
                const tid = f.turbineId || f.TurbineId;
                const tCode = allTurbinesMap[tid] || ("Turbina #" + tid);
                const sev = f.severity || f.Severity || "Normal";
                const sevBadge = sev.toLowerCase() === "critical" ? '<span class="badge bg-danger"><i class="bi bi-exclamation-triangle"></i> CRÍTICA</span>' : '<span class="badge bg-secondary">Normal</span>';
                const desc = f.description || f.Description || "-";
                const dateStr = formatDateTime(f.failureDate || f.FailureDate);

                return `
                    <tr class="${sev.toLowerCase() === 'critical' ? 'table-danger' : ''}">
                        <td>#${id}</td>
                        <td class="fw-bold">${tCode}</td>
                        <td>${sevBadge}</td>
                        <td>${desc}</td>
                        <td>${dateStr}</td>
                    </tr>
                `;
            }).join("");
        }

        function registerFailure() {
            const tid = parseInt(document.getElementById("fTurbine")?.value || 0);
            const sev = document.getElementById("fSeverity")?.value || "Normal";
            const desc = document.getElementById("fDesc")?.value.trim();

            if (!tid || !desc) {
                notify.warning("Seleccione la turbina e ingrese una descripción detallada de la avería.");
                return;
            }

            const btn = document.getElementById("saveFailBtn");
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Reportando...';

            apiClient.post("Failures/Register?callerUserId=" + userId, {
                turbineId: tid,
                severity: sev,
                description: desc
            }).done(function () {
                notify.success("Falla reportada." + (sev.toLowerCase() === "critical" ? " La turbina ha cambiado a estado DAÑADA por seguridad." : ""));
                bootstrap.Modal.getInstance(document.getElementById("regFailModal"))?.hide();
                loadFailures();
            }).fail(function (xhr) {
                handleApiError(xhr);
            }).always(function () {
                btn.disabled = false;
                btn.textContent = "Reportar";
            });
        }
    }

 
    // UTILIDADES
    function populateTurbineSelect(selectEl, includeAllOption) {
        if (!selectEl) return;
        const currentVal = selectEl.value;
        let html = includeAllOption ? '<option value="">Todas las Turbinas</option>' : '';
        Object.keys(allTurbinesMap).forEach(id => {
            html += `<option value="${id}">${allTurbinesMap[id]}</option>`;
        });
        selectEl.innerHTML = html;
        if (currentVal) selectEl.value = currentVal;
    }

    function formatNum(val) {
        return Number(val || 0).toLocaleString("es-CR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function formatDateTime(val) {
        if (!val) return "-";
        const d = new Date(val);
        return isNaN(d.getTime()) ? "-" : d.toLocaleString("es-CR");
    }
});
