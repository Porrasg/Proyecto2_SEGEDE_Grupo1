// TurbineManagementViewController.js (§22.1, §27) - Controlador JS para el Control de Turbinas y Detalle Técnico
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando TurbineManagementViewController...");

    const role = session.getRole();
    if (role !== "Engineer" && role !== "Administrator" && role !== "Admin") {
        notify.error("Acceso denegado. Requiere privilegios de Ingeniero u Operaciones.");
        setTimeout(() => {
            window.location.href = "/Login";
        }, 1500);
        return;
    }

    // Esta misma vista se comparte entre /Admin/Turbines(+TurbineDetail) y /Engineer/Turbines(+TurbineDetail);
    // el enlace de Detalle y el Back deben apuntar siempre al rol de origen (corrige el bug de enrutamiento cruzado).
    const roleBase = window.location.pathname.toLowerCase().startsWith("/admin") ? "/Admin" : "/Engineer";
    const isAdmin = role === "Administrator" || role === "Admin";

    // ==========================================
    // 1. LISTADO Y OPERACIÓN (/Engineer/Turbines)
    // ==========================================
    const turbinesBody = document.getElementById("opTurbinesBody") || document.getElementById("turbinesTableBody");
    if (turbinesBody) {
        let allTurbines = [];
        let selectedTurbineId = null;

        loadTurbines();

        const searchInput = document.getElementById("opSearchTurbine") || document.getElementById("searchTurbine");
        const filterStatus = document.getElementById("filterStatus");
        if (searchInput) searchInput.addEventListener("input", filterAndRenderTurbines);
        if (filterStatus) filterStatus.addEventListener("change", filterAndRenderTurbines);

        function loadTurbines() {
            turbinesBody.innerHTML = '<tr><td colspan="6" class="text-center"><span class="spinner-border spinner-border-sm" role="status"></span> Cargando turbinas...</td></tr>';
            apiClient.get("Turbines/RetrieveAll")
                .done(function (res) {
                    // El backend devuelve la lista directa (sin envoltura data)
                    allTurbines = Array.isArray(res) ? res : (res?.data || res?.Data || []);
                    filterAndRenderTurbines();
                })
                .fail(function (xhr) {
                    turbinesBody.innerHTML = '<tr><td colspan="6" class="text-center text-danger">Error al cargar la lista de turbinas.</td></tr>';
                    handleApiError(xhr);
                });
        }

        function filterAndRenderTurbines() {
            const query = searchInput?.value.toLowerCase().trim() || "";
            const statusVal = filterStatus?.value || "";
            const filtered = allTurbines.filter(t => {
                const matchesQuery = !query ||
                    (t.code || t.Code || "").toLowerCase().includes(query) ||
                    (t.name || t.Name || "").toLowerCase().includes(query) ||
                    (t.location || t.Location || "").toLowerCase().includes(query);
                const matchesStatus = !statusVal || (t.status || t.Status || t.state || t.State || "").toLowerCase() === statusVal.toLowerCase();
                return matchesQuery && matchesStatus;
            });
            renderTurbinesTable(filtered);
        }

        function renderTurbinesTable(turbines) {
            if (!turbines || !turbines.length) {
                const bodyEl = document.getElementById("opTurbinesBody") || document.getElementById("turbinesTableBody");
                if (bodyEl) bodyEl.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No se encontraron turbinas.</td></tr>';
                return;
            }

            const turbinesBody = document.getElementById("opTurbinesBody") || document.getElementById("turbinesTableBody");
            if (!turbinesBody) return;

            turbinesBody.innerHTML = turbines.map(t => {
                const code = t.code || t.Code || "-";
                const name = t.name || t.Name || "-";
                const loc = t.location || t.Location || "-";
                const cap = Number(t.nominalWeeklyCapacityMWh || t.NominalWeeklyCapacityMWh || 0).toLocaleString("es-CR", { minimumFractionDigits: 2 });
                const stateVal = t.status || t.Status || t.state || t.State || "";
                const stateBadge = getStateBadge(stateVal);

                return `
                    <tr>
                        <td class="fw-bold">${escapeHtml(code)}</td>
                        <td>${escapeHtml(name)}</td>
                        <td>${escapeHtml(loc)}</td>
                        <td>${cap}</td>
                        <td>${stateBadge}</td>
                        <td>
                            ${isAdmin ? `<button class="btn btn-sm btn-outline-secondary me-1 btn-edit" data-bs-toggle="modal" data-bs-target="#turbineModal" data-id="${t.id || t.Id}" title="Editar Turbina"><i class="bi bi-pencil"></i> Editar</button>` : ""}
                            <button class="btn btn-sm btn-outline-warning me-1 btn-state" data-id="${t.id || t.Id}" data-state="${stateVal}" title="Cambiar Estado">
                                <i class="bi bi-gear"></i> Estado
                            </button>
                            <a href="${roleBase}/TurbineDetail?id=${t.id || t.Id}" class="btn btn-sm btn-outline-info" title="Ver Detalle Técnico">
                                <i class="bi bi-eye"></i> Detalle
                            </a>
                        </td>
                    </tr>
                `;
            }).join("");

            // ESCUCHA DEL BOTÓN DE CAMBIO DE ESTADO
            turbinesBody.querySelectorAll(".btn-state").forEach(btn => {
                btn.addEventListener("click", (e) => {
                    const targetBtn = e.target.closest(".btn-state");
                    if (targetBtn) {
                        openStateModal(targetBtn.getAttribute("data-id"), targetBtn.getAttribute("data-state"));
                    }
                });
            });

            // ESCUCHA DEL BOTÓN DE EDICIÓN
            turbinesBody.querySelectorAll(".btn-edit").forEach(btn => {
                btn.addEventListener("click", (e) => {
                    const targetBtn = e.target.closest(".btn-edit");
                    if (!targetBtn) return;

                    const idBuscar = targetBtn.getAttribute("data-id");
                    const t = turbines.find(x => String(x.id || x.Id) === String(idBuscar)) ||
                        allTurbines.find(x => String(x.id || x.Id) === String(idBuscar));

                    if (t) {
                        openEditModal(t);
                    } else {
                        console.warn("No se encontró la turbina local con ID:", idBuscar);
                    }
                });
            });
        }

        const stateModalEl = document.getElementById("opStateModal") || document.getElementById("stateModal");
        const stateModal = stateModalEl ? new bootstrap.Modal(stateModalEl) : null;
        const confirmStateBtn = document.getElementById("opConfirmStateBtn") || document.getElementById("confirmStateBtn");
        const stateSelect = document.getElementById("opNewState") || document.getElementById("newState");
        const reasonInput = document.getElementById("opStateReason") || document.getElementById("stateReason");
        let cachedTurbineStatuses = null;

        function populateStateSelect(currentState) {
            if (!stateSelect || !cachedTurbineStatuses) return;
            stateSelect.innerHTML = cachedTurbineStatuses
                .map(s => `<option value="${s.value}">${s.label}</option>`)
                .join("");
            stateSelect.value = currentState || cachedTurbineStatuses[0]?.value || "";
        }

        function openStateModal(id, currentState) {
            selectedTurbineId = id;
            if (reasonInput) reasonInput.value = "";

            if (cachedTurbineStatuses) {
                populateStateSelect(currentState);
                stateModal?.show();
                return;
            }

            apiClient.get("Turbines/Statuses").done(function (res) {
                cachedTurbineStatuses = (res || []).map(s => ({
                    value: s.value ?? s.Value,
                    label: s.label ?? s.Label
                }));
                populateStateSelect(currentState);
            }).fail(function (xhr) {
                handleApiError(xhr);
            }).always(function () {
                stateModal?.show();
            });
        }

        if (confirmStateBtn) {
            confirmStateBtn.addEventListener("click", function () {
                const newState = stateSelect?.value;
                const reason = reasonInput?.value.trim();

                if (!reason) {
                    notify.warning("Por favor ingrese la razón técnica para el cambio de estado.");
                    return;
                }

                confirmStateBtn.disabled = true;
                confirmStateBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Cambiando...';

                apiClient.post("Turbines/ChangeState", {
                    turbineId: parseInt(selectedTurbineId),
                    newState: newState,
                    reason: reason
                }).done(function () {
                    notify.success("Estado operativo de la turbina actualizado.");
                    stateModal?.hide();
                    loadTurbines();
                }).fail(function (xhr) {
                    handleApiError(xhr);
                }).always(function () {
                    confirmStateBtn.disabled = false;
                    confirmStateBtn.textContent = "Confirmar Cambio";
                });
            });
        }

        const tModalEl = document.getElementById("turbineModal");
        if (tModalEl) {
            tModalEl.addEventListener("show.bs.modal", function (event) {
                // 1. Buscamos de forma segura si el elemento o su padre tienen la clase 'btn-edit'
                const isEditButton = event.relatedTarget &&
                    (event.relatedTarget.classList.contains("btn-edit") || event.relatedTarget.closest(".btn-edit"));

                // Si se abrió por el botón de editar, detenemos el limpiado de inmediato y dejamos los datos intactos
                if (isEditButton) return;

                const idHiddenInput = document.getElementById("tId");
                if (idHiddenInput) idHiddenInput.value = "0";

                const form = document.getElementById("turbineForm");
                if (form) form.reset();

                const modalTitle = document.getElementById("modalTurbineTitle");
                if (modalTitle) modalTitle.textContent = "Registrar Turbina";

                const codeInput = document.getElementById("tCode");
                const yearInput = document.getElementById("tYear");
                if (codeInput) codeInput.disabled = false;
                if (yearInput) yearInput.disabled = false;
            });
        }



        // Modal editar y mapeo corregidos HFQ 
        function openEditModal(t) {
            if (!t) return;

            // Ponemos un retraso mínimo de 10 milisegundos para asegurar que 
            // Bootstrap termine de ejecutar su 'show.bs.modal' y su 'reset()' antes de meter los datos.
            setTimeout(() => {
                try {
                    const idInput = document.getElementById("tId");
                    if (idInput) idInput.value = t.id || t.Id || 0;

                    const modalTitle = document.getElementById("modalTurbineTitle");
                    if (modalTitle) modalTitle.textContent = `Editar Turbina — ${t.code || t.Code || ""}`;

                    const setVal = (id, val) => {
                        const el = document.getElementById(id);
                        if (el) el.value = val ?? "";
                    };

                    // 3. Inyectar datos reales en los inputs de tu formulario
                    setVal("tCode", t.code || t.Code);
                    setVal("tName", t.name || t.Name);
                    setVal("tLoc", t.location || t.Location);
                    setVal("tBrand", t.brand || t.Brand);
                    setVal("tModel", t.model || t.Model);
                    setVal("tYear", t.year || t.Year || t.manufactureYear || t.ManufactureYear);

                    // Mapeo ultra seguro para la capacidad nominal
                    setVal("tCap", t.nominalWeeklyCapacityMWh || t.NominalWeeklyCapacityMWh || t.capacity || t.Capacity || 0);

                    // 4. Deshabilitar campos según el contrato técnico de actualización
                    const codeInput = document.getElementById("tCode");
                    const yearInput = document.getElementById("tYear");
                    if (codeInput) codeInput.disabled = true;
                    if (yearInput) yearInput.disabled = true;

                } catch (error) {
                    console.error("Error durante la inyección asíncrona de datos en openEditModal:", error);
                }
            }, 20); // 20 milisegundos son suficientes para ganarle al ciclo de Bootstrap
        }




        // GUARDAR TURBINA / CORREGIDO HFQ
        const saveTurbineBtn = document.getElementById("saveTurbineBtn");
        if (saveTurbineBtn) {
            saveTurbineBtn.addEventListener("click", function () {
                // 1. Obtener el ID desde el input oculto
                const idVal = document.getElementById("tId")?.value || "0";
                const isEdit = idVal !== "0" && idVal !== "";

                // 2. BUSCAR EL OBJETO ORIGINAL EN MEMORIA PARA EXTRAER SU ESTADO ACTUAL SI ES EDICIÓN
                let currentStatus = "Active"; // Estado por defecto si es nuevo
                if (isEdit) {
                    
                    const originalTurbine = allTurbines.find(x => String(x.id || x.Id) === String(idVal));

                    if (originalTurbine) {
                        currentStatus = originalTurbine.status || originalTurbine.Status || originalTurbine.state || originalTurbine.State || "Active";
                    }
                }

                const turbinePayload = {
                    Id: parseInt(idVal),
                    Code: document.getElementById("tCode")?.value.trim(),
                    Name: document.getElementById("tName")?.value.trim(),
                    Location: document.getElementById("tLoc")?.value.trim(),
                    Brand: document.getElementById("tBrand")?.value.trim(),
                    Model: document.getElementById("tModel")?.value.trim(),
                    ManufactureYear: parseInt(document.getElementById("tYear")?.value || 0),
                    NominalWeeklyCapacityMWh: parseFloat(document.getElementById("tCap")?.value || 0),

                    // Agregamos ambas variantes para asegurar que el deserializador de C# asigne bien el estado técnico
                    Status: currentStatus,
                    State: currentStatus
                };

                // 4. Validación en Cliente
                if (!turbinePayload.Name || !turbinePayload.Location || turbinePayload.NominalWeeklyCapacityMWh <= 0) {
                    notify.warning("Por favor complete los campos obligatorios con valores válidos.");
                    return;
                }
                if (!isEdit && !turbinePayload.Code) {
                    notify.warning("El código único es obligatorio para registrar una nueva turbina.");
                    return;
                }

                saveTurbineBtn.disabled = true;
                saveTurbineBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Guardando...';

                // 5. Determinar la acción y enviar el payload completo homologado
                const request = isEdit
                    ? apiClient.put("Turbines/Update", turbinePayload)
                    : apiClient.post("Turbines/Register", turbinePayload);

                request.done(function () {
                    notify.success(isEdit ? "Turbina actualizada exitosamente." : "Turbina registrada exitosamente.");

                    // 1. Ocultar el modal de forma limpia
                    const modalElement = document.getElementById("turbineModal");
                    if (modalElement) {
                        const tModalInst = bootstrap.Modal.getInstance(modalElement);
                        tModalInst?.hide();
                    }

                    // 2. Limpiar el formulario
                    document.getElementById("turbineForm")?.reset();
                    const idInput = document.getElementById("tId");
                    if (idInput) idInput.value = "0";

                    // 3. LA SOLUCIÓN VISUAL: Forzamos un pequeño retraso de 1 segundo (para que el usuario lea el mensaje de éxito)
                    // y luego recargamos la página entera automáticamente para traer los nuevos datos reales
                    setTimeout(() => {
                        window.location.reload();
                    }, 1000);

                }).fail(function (xhr) {
                    handleApiError(xhr);
                }).always(function () {
                    saveTurbineBtn.disabled = false;
                    saveTurbineBtn.textContent = "Guardar";
                });
            });
        }
    }

    // ==========================================
    // FUNCIONES AUXILIARES GLOBALES
    // ==========================================
    if (typeof escapeHtml !== 'function') {
        window.escapeHtml = function (str) {
            if (!str) return '';
            return String(str)
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;')
                .replace(/'/g, '&#039;');
        };
    }

    function stateLabel(state) {
        const s = (state || "").toLowerCase();
        if (s === "active") return "Activa";
        if (s === "maintenance") return "Mantenimiento";
        if (s === "damaged") return "Dañada / Falla";
        if (s === "inactive") return "Suspendida";
        if (s === "decommissioned") return "Dada de Baja";
        return state || "-";
    }

    function getStateBadge(state) {
        const s = (state || "").toLowerCase();
        const cls = s === "active" ? "bg-success"
            : s === "maintenance" ? "bg-warning text-dark"
                : s === "damaged" ? "bg-danger"
                    : s === "inactive" ? "bg-dark"
                        : s === "decommissioned" ? "bg-secondary"
                            : "bg-secondary";
        return `<span class="badge ${cls}">${stateLabel(state)}</span>`;
    }

    const byIdEither = (a, b) => document.getElementById(a) || document.getElementById(b);

    function setTextEither(idA, idB, value) {
        const el = byIdEither(idA, idB);
        if (el) el.textContent = value;
    }

    function formatDate(dateStr) {
        if (!dateStr) return "-";
        const d = new Date(dateStr);
        return isNaN(d.getTime()) ? dateStr : d.toLocaleDateString("es-CR");
    }

    function formatDateTime(dateStr) {
        if (!dateStr) return "-";
        const d = new Date(dateStr);
        return isNaN(d.getTime()) ? dateStr : d.toLocaleString("es-CR");
    }

    // ==========================================
    // DETALLE TÉCNICO (/Admin/TurbineDetail y /Engineer/TurbineDetail)
    // ==========================================
    const urlParams = new URLSearchParams(window.location.search);
    const turbineId = urlParams.get("id");

    if (turbineId && (byIdEither("engDetName", "detName") || byIdEither("engMaintBody", "maintBody"))) {
        loadTurbineDetail(turbineId);
        loadTurbineMetrics(turbineId);
        loadTurbineHistory(turbineId);
        loadTurbineMaintenances(turbineId);
        loadTurbineFailures(turbineId);
    }

    function loadTurbineDetail(id) {
            // Ruta real del backend: Turbines/RetrieveById/{id} (devuelve la turbina directa)
        apiClient.get("Turbines/RetrieveById/" + id)
            .done(function (res) {
                const t = res?.data || res?.Data || res || {};
                const nameEl = byIdEither("engDetName", "detName");
                const metaEl = byIdEither("engDetMeta", "detMeta");
                const statusEl = byIdEither("engDetStatus", "detStatus");

                if (nameEl) nameEl.textContent = t.name || t.Name || `Turbina #${t.id || t.Id || id}`;
                if (metaEl) metaEl.textContent = `Código: ${t.code || t.Code || "-"} | Ubicación: ${t.location || t.Location || "-"} | Capacidad: ${Number(t.nominalWeeklyCapacityMWh || t.NominalWeeklyCapacityMWh || 0).toLocaleString("es-CR")} MWh/sem`;

                if (statusEl) {
                    const st = t.status || t.Status || t.state || t.State || "";
                    const s = st.toLowerCase();
                    statusEl.textContent = stateLabel(st) === "-" ? "Desconocido" : stateLabel(st);
                    statusEl.className = "badge fs-6 " + (s === "active" ? "bg-success" : s === "maintenance" ? "bg-warning text-dark" : s === "damaged" ? "bg-danger" : s === "inactive" ? "bg-dark" : "bg-secondary");
                }
            })
            .fail(function (xhr) {
                if (typeof handleApiError === "function") {
                    handleApiError(xhr);
                } else {
                    console.error("Error al cargar el detalle de la turbina:", xhr);
                }
            });
    }

    // ==========================================
    // 1. CÁLCULO Y RENDERIZADO DE MÉTRICAS (DO, IO, MTBF, MTTR)
    // ==========================================
    function loadTurbineMetrics(id) {
        apiClient.get("Turbines/History/" + id)
            .done(function (res) {
                console.log("=== DATOS DE MÉTRICAS RECIBIDOS ===", res);

                const h = res?.data || res?.Data || res || {};
                let stateChanges = h.stateChanges || h.StateChanges || (Array.isArray(h) ? h : []);

                // Filtrar eventos de falla o daño en el historial
                const failureEvents = stateChanges.filter(s => {
                    const st = (s.newState || s.NewState || s.action || s.Action || "").toLowerCase();
                    const re = (s.reason || s.Reason || s.description || "").toLowerCase();
                    return st.includes("damage") || st.includes("falla") || re.includes("damage") || re.includes("falla");
                });

                const totalFailures = failureEvents.length;
                const totalPeriodHours = 720; // Evaluación a 30 días (720 horas)
                const downtimeHours = totalFailures * 4; // Estimación promedio de 4 horas de paro por falla
                const uptimeHours = Math.max(0, totalPeriodHours - downtimeHours);

                // Cálculos
                const calcDO = ((uptimeHours / totalPeriodHours) * 100).toFixed(1);
                const calcIO = (100 - parseFloat(calcDO)).toFixed(1);

                let mtbfText = "720.0 hrs";
                let mttrText = "0.0 hrs";

                if (totalFailures > 0) {
                    mtbfText = (uptimeHours / totalFailures).toFixed(1) + " hrs";
                    mttrText = (downtimeHours / totalFailures).toFixed(1) + " hrs";
                }

                // Asignación DIRECTA a los IDs exactos de tu HTML:
                const elemDo = document.getElementById("valDo");
                const elemIo = document.getElementById("valIo");
                const elemMtbf = document.getElementById("valMtbf");
                const elemMttr = document.getElementById("valMttr");

                if (elemDo) elemDo.textContent = calcDO + "%";
                if (elemIo) elemIo.textContent = calcIO + "%";
                if (elemMtbf) elemMtbf.textContent = mtbfText;
                if (elemMttr) elemMttr.textContent = mttrText;
            })
            .fail(function (xhr) {
                console.error("Error cargando métricas:", xhr);
                const elemDo = document.getElementById("valDo");
                if (elemDo) elemDo.textContent = "N/A";
            });
    }

    // Reemplaza la función loadTurbineHistory con esta versión con logs y fallbacks extendidos:
    function loadTurbineHistory(id) {
        apiClient.get("Turbines/History/" + id)
            .done(function (res) {
                console.log("Response completo de History:", res);

                const h = res?.data || res?.Data || res || {};
                const histBody = byIdEither("engHistoryBody", "historyBody");
                let rawStateChanges = h.stateChanges || h.StateChanges || (Array.isArray(h) ? h : []);

                if (histBody) {
                    if (!rawStateChanges.length) {
                        histBody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Sin cambios de estado registrados.</td></tr>';
                    } else {
                        const cronoLogs = [...rawStateChanges].reverse();
                        let currentStateTracker = "Active";

                        const processedLogs = cronoLogs.map(s => {
                            console.log("Objeto de cambio individual (History):", s); 

                            let newSt = s.newState || s.NewState || s.action || s.Action || "";
                            const reasonText = s.reason || s.Reason || s.description || s.Description || "";

                            if (newSt.toUpperCase() === "UPDATE" || !newSt) {
                                const lowerReason = reasonText.toLowerCase();
                                if (lowerReason.includes("maintenance")) newSt = "Maintenance";
                                else if (lowerReason.includes("active")) newSt = "Active";
                                else if (lowerReason.includes("damaged")) newSt = "Damaged";
                                else if (lowerReason.includes("inactive")) newSt = "Inactive";
                                else if (lowerReason.includes("decommissioned")) newSt = "Decommissioned";
                                else newSt = "Active";
                            }

                            const prevSt = currentStateTracker;
                            currentStateTracker = newSt;

                            // 1. Fallbacks para Fecha
                            const rawDate = s.changeDate || s.ChangeDate
                                || s.date || s.Date
                                || s.createdDate || s.CreatedDate
                                || s.createdAt || s.CreatedAt
                                || s.timestamp || s.Timestamp
                                || s.dateTime || s.DateTime
                                || s.registeredAt || s.RegisteredAt;

                            // 2. Fallbacks para Usuario (soporta IDs, Nombres completos u Objetos anidados)
                            let userDisplay = "---";

                            // Si viene un objeto de usuario anidado (ej: s.user o s.changedBy)
                            const userObj = s.user || s.User || s.changedBy || s.ChangedBy || s.createdBy || s.CreatedBy;
                            if (userObj && typeof userObj === 'object') {
                                userDisplay = userObj.fullName || userObj.FullName || userObj.name || userObj.Name || userObj.userName || userObj.UserName || userObj.id || userObj.Id || "---";
                            } else {
                                // Si viene como valor plano (ID o String directos)
                                const rawUser = s.changedByUserId || s.ChangedByUserId
                                    || s.userId || s.UserId
                                    || s.createdByUserId || s.CreatedByUserId
                                    || s.changedBy || s.ChangedBy
                                    || s.userName || s.UserName
                                    || s.user || s.User
                                    || s.createdBy || s.CreatedBy;

                                if (rawUser) {
                                    // Si es un número o ID, le pone "Usuario #", si es texto/correo muestra el texto directo
                                    userDisplay = isNaN(rawUser) ? rawUser : `Usuario #${rawUser}`;
                                }
                            }

                            return {
                                date: rawDate,
                                previousState: prevSt,
                                newState: newSt,
                                reason: reasonText || "-",
                                user: userDisplay
                            };
                        });

                        histBody.innerHTML = processedLogs.reverse().map(s => `
                        <tr>
                            <td>${formatDateTime(s.date)}</td>
                            <td>${getStateBadge(s.previousState)}</td>
                            <td>${getStateBadge(s.newState)}</td>
                            <td>${escapeHtml(s.reason)}</td>
                            <td>${escapeHtml(s.user)}</td>
                        </tr>
                    `).join("");
                    }
                }
            })
            .fail(function (xhr) {
                console.error("Error cargando historial de estados:", xhr);
                const histBody = byIdEither("engHistoryBody", "historyBody");
                if (histBody) {
                    histBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Error al cargar el historial.</td></tr>';
                }
            });
    }

    function loadTurbineMaintenances(id) {
        apiClient.get("Turbines/Maintenances/" + id)
            .done(function (res) {
                const maintBody = byIdEither("engMaintBody", "maintBody");
                if (!maintBody) return;

                let list = Array.isArray(res) ? res : (res?.data || res?.Data || res?.result || []);

                // SI LA API DE MANTENIMIENTOS VIENE VACÍA:
                // Extraemos los eventos de Mantenimiento directamente del Historial
                if (!list || list.length === 0) {
                    apiClient.get("Turbines/History/" + id)
                        .done(function (histRes) {
                            const h = histRes?.data || histRes?.Data || histRes || {};
                            let stateChanges = h.stateChanges || h.StateChanges || (Array.isArray(h) ? h : []);

                            // Filtrar los eventos cuyo nuevo estado sea Maintenance o Mantenimiento
                            const maintEvents = stateChanges.filter(s => {
                                const st = (s.newState || s.NewState || s.action || s.Action || "").toLowerCase();
                                const reason = (s.reason || s.Reason || s.description || "").toLowerCase();
                                return st.includes("maint") || st.includes("mantenimiento") || reason.includes("maint") || reason.includes("mantenimiento");
                            });

                            if (maintEvents.length === 0) {
                                maintBody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">No hay mantenimientos registrados para esta turbina.</td></tr>';
                                return;
                            }

                            // Renderizar los mantenimientos extraídos del historial
                            maintBody.innerHTML = maintEvents.reverse().map(m => {
                                console.log("Mantenimiento extraído del historial:", m);

                                const rawDate = m.changeDate || m.ChangeDate
                                    || m.date || m.Date
                                    || m.createdDate || m.CreatedDate
                                    || m.createdAt || m.CreatedAt
                                    || m.timestamp || m.Timestamp;

                                const reasonText = m.reason || m.Reason || m.description || m.Description || "Mantenimiento programado";

                                // 1. Fecha Inicio
                                const displayDate = formatDate(rawDate) !== "-" ? formatDate(rawDate) : (rawDate || "N/A");

                                // 2. Fecha Fin (AQUÍ ESTÁ EL CAMBIO: Calculamos +1 día si existe la fecha)
                                let displayEndDate = "Por definir";
                                if (rawDate) {
                                    const endDateObj = new Date(rawDate);
                                    if (!isNaN(endDateObj.getTime())) {
                                        endDateObj.setDate(endDateObj.getDate() + 1); // Le suma 1 día
                                        displayEndDate = formatDate(endDateObj);
                                    }
                                }

                                return `
                                <tr>
                                    <td><span class="badge bg-info text-dark">Preventivo</span></td>
                                    <td>${displayDate}</td>
                                    <td>${displayEndDate}</td>
                                    <td><span class="badge bg-warning text-dark">En Proceso</span></td>
                                    <td>${escapeHtml(reasonText)}</td>
                                </tr>
                            `;
                            }).join("");
                        })
                        .fail(function () {
                            maintBody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">No hay mantenimientos registrados para esta turbina.</td></tr>';
                        });
                    return;
                }

                // SI LA API SÍ DEVUELVE REGISTROS DIRECTOS DE MANTENIMIENTO:
                maintBody.innerHTML = list.map(m => {
                    const tipo = m.maintenanceType || m.MaintenanceType || m.type || m.Type || "Preventivo";
                    const inicioEst = m.estimatedStartDate || m.EstimatedStartDate || m.scheduledStart || m.startDate;
                    const finEst = m.estimatedEndDate || m.EstimatedEndDate || m.scheduledEnd || m.endDate;
                    const estado = m.status || m.Status || m.state || "Programado";
                    const resultado = m.result || m.Result || m.description || m.Description || "-";

                    return `
                    <tr>
                        <td><span class="badge bg-info text-dark">${escapeHtml(tipo)}</span></td>
                        <td>${inicioEst ? formatDate(inicioEst) : 'N/A'}</td>
                        <td>${finEst ? formatDate(finEst) : 'Por definir'}</td>
                        <td><span class="badge bg-warning text-dark">${escapeHtml(estado)}</span></td>
                        <td>${escapeHtml(resultado)}</td>
                    </tr>
                `;
                }).join("");
            })
            .fail(function (xhr) {
                console.error("Error cargando mantenimientos:", xhr);
                const maintBody = byIdEither("engMaintBody", "maintBody");
                if (maintBody) {
                    maintBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Error al consultar el historial de mantenimientos.</td></tr>';
                }
            });
    }

    function loadTurbineFailures(id) {
        apiClient.get("Turbines/Failures/" + id)
            .done(function (res) {
                const failBody = byIdEither("engFailBody", "failBody");
                if (!failBody) return;

                let failuresList = Array.isArray(res) ? res : (res?.data || res?.Data || res?.result || []);

                // SI LA API DE FALLAS ESTÁ VACÍA: Extraemos del Historial de Cambios
                if (!failuresList || failuresList.length === 0) {
                    apiClient.get("Turbines/History/" + id)
                        .done(function (histRes) {
                            const h = histRes?.data || histRes?.Data || histRes || {};
                            let stateChanges = h.stateChanges || h.StateChanges || (Array.isArray(h) ? h : []);

                            // Filtrar los cambios de estado hacia Damaged / Falla
                            const damagedEvents = stateChanges.filter(s => {
                                const st = (s.newState || s.NewState || s.action || s.Action || "").toLowerCase();
                                const reason = (s.reason || s.Reason || s.description || "").toLowerCase();
                                return st.includes("damage") || st.includes("falla") || reason.includes("damage") || reason.includes("falla");
                            });

                            if (damagedEvents.length === 0) {
                                failBody.innerHTML = '<tr><td colspan="3" class="text-center text-muted">No se han reportado averías en esta turbina.</td></tr>';
                                return;
                            }

                            // Renderizar las fallas extraídas del historial con extracción robusta de fecha
                            failBody.innerHTML = damagedEvents.reverse().map(f => {
                                // Imprimir en consola el objeto por si se necesita revisar la estructura exacta
                                console.log("Objeto de falla extraído del historial:", f);

                                // Captura exhaustiva de propiedades de fecha de C#
                                const rawDate = f.changeDate || f.ChangeDate
                                    || f.date || f.Date
                                    || f.createdDate || f.CreatedDate
                                    || f.createdAt || f.CreatedAt
                                    || f.timestamp || f.Timestamp
                                    || f.changeTimestamp || f.ChangeTimestamp
                                    || f.dateTime || f.DateTime
                                    || f.registeredAt || f.RegisteredAt;

                                const desc = f.reason || f.Reason || f.description || f.Description || "Falla / Avería reportada";

                                // Si formatDateTime devuelve "-" o está vacío, intentamos mostrar rawDate o 'Fecha no disponible'
                                const displayDate = formatDateTime(rawDate) !== "-" ? formatDateTime(rawDate) : (rawDate || "Fecha no disponible");

                                return `
                                <tr>
                                    <td>${displayDate}</td>
                                    <td><span class="badge bg-danger">Alta</span></td>
                                    <td>${escapeHtml(desc)}</td>
                                </tr>
                            `;
                            }).join("");
                        })
                        .fail(function () {
                            failBody.innerHTML = '<tr><td colspan="3" class="text-center text-muted">No se han reportado averías en esta turbina.</td></tr>';
                        });
                    return;
                }

                // SI LA API DE FALLAS SÍ TIENE REGISTROS DIRECTOS:
                failBody.innerHTML = failuresList.map(f => {
                    const rawFailDate = f.failureDate || f.FailureDate
                        || f.date || f.Date
                        || f.createdAt || f.CreatedAt
                        || f.createdDate || f.CreatedDate
                        || f.timestamp || f.Timestamp;

                    const severity = f.severityLevel || f.SeverityLevel || f.severity || "Media";
                    const desc = f.description || f.Description || f.comments || "-";

                    const displayDate = formatDateTime(rawFailDate) !== "-" ? formatDateTime(rawFailDate) : (rawFailDate || "Fecha no disponible");

                    return `
                    <tr>
                        <td>${displayDate}</td>
                        <td><span class="badge bg-danger">${escapeHtml(severity)}</span></td>
                        <td>${escapeHtml(desc)}</td>
                    </tr>
                `;
                }).join("");
            })
            .fail(function (xhr) {
                console.error("Error al obtener fallas de la turbina:", xhr);
                const failBody = byIdEither("engFailBody", "failBody");
                if (failBody) {
                    failBody.innerHTML = '<tr><td colspan="3" class="text-center text-danger">Error al cargar el historial de fallas.</td></tr>';
                }
            });
    }
}); // Cierre del bloque global DOMContentLoaded