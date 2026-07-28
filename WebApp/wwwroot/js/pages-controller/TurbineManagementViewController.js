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
                    (t.name || "").toLowerCase().includes(query) ||
                    (t.location || "").toLowerCase().includes(query);
                const matchesStatus = !statusVal || (t.status || t.Status || t.state || t.State || "").toLowerCase() === statusVal.toLowerCase();
                return matchesQuery && matchesStatus;
            });
            renderTurbinesTable(filtered);
        }


        //CORREGIDO HFQ 
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
                const name = t.name || "-";
                const loc = t.location || "-";
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



        function getStateBadge(state) {
            const s = (state || "").toLowerCase();
            if (s === "active") return '<span class="badge bg-success">Activa</span>';
            if (s === "maintenance") return '<span class="badge bg-warning text-dark">Mantenimiento</span>';
            if (s === "damaged") return '<span class="badge bg-danger">Dañada / Falla</span>';
            if (s === "inactive") return '<span class="badge bg-dark">Suspendida</span>';
            return `<span class="badge bg-secondary">${state || "-"}</span>`;
        }

        const stateModalEl = document.getElementById("opStateModal") || document.getElementById("stateModal");
        const stateModal = stateModalEl ? new bootstrap.Modal(stateModalEl) : null;
        const confirmStateBtn = document.getElementById("opConfirmStateBtn") || document.getElementById("confirmStateBtn");
        const stateSelect = document.getElementById("opNewState") || document.getElementById("newState");
        const reasonInput = document.getElementById("opStateReason") || document.getElementById("stateReason");

        function openStateModal(id, currentState) {
            selectedTurbineId = id;
            if (stateSelect) {
                stateSelect.innerHTML = `
                    <option value="Active">Active (Operación Normal)</option>
                    <option value="Maintenance">Maintenance (En Mantenimiento)</option>
                    <option value="Damaged">Damaged (Falla Técnica)</option>
                    <option value="Inactive">Inactive (Incumplimiento / Parada)</option>
                `;
                stateSelect.value = currentState || "Active";
            }
            if (reasonInput) reasonInput.value = "";
            stateModal?.show();
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

        let editingTurbineId = null;
        const tModalEl = document.getElementById("turbineModal");
        const tCodeInput = document.getElementById("tCode");
        const tYearInput = document.getElementById("tYear");
        const tModalTitle = tModalEl?.querySelector(".modal-title");
        const tCodeField = tCodeInput?.closest(".col") || tCodeInput;


        //CORREGIDO HFQ 
        // Al abrir el modal para "Nueva Turbina" (no vía botón Editar), limpia cualquier estado de edición previo.
        if (tModalEl) {
            tModalEl.addEventListener("show.bs.modal", function (event) {
                // 1. Buscamos de forma segura si el elemento o su padre tienen la clase 'btn-edit'
                const isEditButton = event.relatedTarget &&
                    (event.relatedTarget.classList.contains("btn-edit") || event.relatedTarget.closest(".btn-edit"));

                // Si se abrió por el botón de editar, detenemos el limpiado de inmediato y dejamos los datos intactos
                if (isEditButton) return;

                // 2. Si se abrió de forma normal (Nueva Turbina), limpiamos el formulario con selectores nativos directos
                editingTurbineId = null;

                const idHiddenInput = document.getElementById("tId");
                if (idHiddenInput) idHiddenInput.value = "0";

                const form = document.getElementById("turbineForm");
                if (form) form.reset();

                const modalTitle = document.getElementById("modalTurbineTitle");
                if (modalTitle) modalTitle.textContent = "Registrar Turbina";

                // Habilitar los campos para un nuevo registro limpio
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
                    // 1. Guardar el ID directamente en el input oculto
                    const idInput = document.getElementById("tId");
                    if (idInput) {
                        idInput.value = t.id || t.Id || 0;
                    }

                    // 2. Cambiar el título usando el ID limpio que añadiste a tu HTML actualizado
                    const modalTitle = document.getElementById("modalTurbineTitle");
                    if (modalTitle) {
                        modalTitle.textContent = `Editar Turbina — ${t.code || t.Code || ""}`;
                    }

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
                        // Extraer el estado tal cual viene del servidor original
                        currentStatus = originalTurbine.status || originalTurbine.Status ||
                            originalTurbine.state || originalTurbine.State || "Active";
                    }
                }

                // 3. CONSTRUIR EL OBJETO EXACTO PARA C# (Incluyendo Status / State para pasar IsValidStatus)
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




        // ==========================================
        // 2. DETALLE TÉCNICO (/Admin/TurbineDetail y /Engineer/TurbineDetail — IDs con o sin prefijo "eng")
        // ==========================================
        const byIdEither = (a, b) => document.getElementById(a) || document.getElementById(b);

        if (byIdEither("engDetName", "detName")) {
            const urlParams = new URLSearchParams(window.location.search);
            const turbineId = urlParams.get("id");

            if (!turbineId) {
                notify.error("Identificador de turbina no especificado.");
                setTimeout(() => window.location.href = roleBase + "/Turbines", 1500);
                return;
            }

            loadTurbineDetail(turbineId);
            loadTurbineMetrics(turbineId);
            loadTurbineHistory(turbineId);
        }

        function loadTurbineDetail(id) {
            // Ruta real del backend: Turbines/RetrieveById/{id} (devuelve la turbina directa)
            apiClient.get("Turbines/RetrieveById/" + id)
                .done(function (res) {
                    const t = res?.data || res?.Data || res || {};
                    const nameEl = byIdEither("engDetName", "detName");
                    const metaEl = byIdEither("engDetMeta", "detMeta");
                    const statusEl = byIdEither("engDetStatus", "detStatus");

                    if (nameEl) nameEl.textContent = t.name || `Turbina #${t.id || id}`;
                    if (metaEl) metaEl.textContent = `Código: ${t.code || t.Code || "-"} | Ubicación: ${t.location || "-"} | Capacidad: ${Number(t.nominalWeeklyCapacityMWh || t.NominalWeeklyCapacityMWh || 0).toLocaleString("es-CR")} MWh/sem`;

                    if (statusEl) {
                        const st = t.status || t.Status || t.state || t.State || "Unknown";
                        const s = st.toLowerCase();
                        statusEl.textContent = st;
                        statusEl.className = "badge fs-6 " + (s === "active" ? "bg-success" : s === "maintenance" ? "bg-warning text-dark" : s === "damaged" ? "bg-danger" : "bg-secondary");
                    }
                })
                .fail(function (xhr) {
                    handleApiError(xhr);
                });
        }

        function loadTurbineMetrics(id) {
            apiClient.get("Turbines/Metrics/" + id)
                .done(function (res) {
                    const m = res?.data || res?.Data || {};
                    setTextEither("engValDo", "valDo", (m.operationalAvailability ?? m.OperationalAvailability ?? 0) + "%");
                    setTextEither("engValIo", "valIo", (m.operationalUnavailability ?? m.OperationalUnavailability ?? 0) + "%");
                    setTextEither("engValMtbf", "valMtbf", Number(m.mtbf ?? m.MTBF ?? 0).toLocaleString("es-CR", { maximumFractionDigits: 1 }) + " hrs");
                    setTextEither("engValMttr", "valMttr", Number(m.mttr ?? m.MTTR ?? 0).toLocaleString("es-CR", { maximumFractionDigits: 1 }) + " hrs");
                })
                .fail(function (xhr) {
                    console.error("Error cargando métricas:", xhr);
                });
        }

        function loadTurbineHistory(id) {
            apiClient.get("Turbines/History/" + id)
                .done(function (res) {
                    const h = res?.data || res?.Data || {};

                    // Render Estado Histórico
                    const histBody = byIdEither("engHistoryBody", "historyBody");
                    const stateChanges = h.stateChanges || h.StateChanges || [];
                    if (histBody) {
                        histBody.innerHTML = stateChanges.length ? stateChanges.map(s => `
                        <tr>
                            <td>${new Date(s.changeDate || s.ChangeDate).toLocaleString("es-CR")}</td>
                            <td><span class="badge bg-secondary">${s.previousState || s.PreviousState || "-"}</span></td>
                            <td><span class="badge bg-primary">${s.newState || s.NewState || "-"}</span></td>
                            <td>${escapeHtml(s.reason || s.Reason || "-")}</td>
                            <td>Usuario #${s.changedByUserId || s.ChangedByUserId || "---"}</td>
                        </tr>
                    `).join("") : '<tr><td colspan="5" class="text-center text-muted">Sin cambios de estado registrados.</td></tr>';
                    }

                    // Render Mantenimientos
                    const maintBody = byIdEither("engMaintBody", "maintBody");
                    const maintenances = h.maintenances || h.Maintenances || [];
                    if (maintBody) {
                        maintBody.innerHTML = maintenances.length ? maintenances.map(m => `
                        <tr>
                            <td><span class="badge bg-info text-dark">${m.maintenanceType || m.MaintenanceType || "-"}</span></td>
                            <td>${new Date(m.scheduledStart || m.ScheduledStart).toLocaleDateString("es-CR")}</td>
                            <td>${new Date(m.scheduledEnd || m.ScheduledEnd).toLocaleDateString("es-CR")}</td>
                            <td><span class="badge bg-warning text-dark">${m.status || m.Status || "-"}</span></td>
                            <td>${m.outcomeNotes || m.OutcomeNotes || "-"}</td>
                        </tr>
                    `).join("") : '<tr><td colspan="5" class="text-center text-muted">No hay mantenimientos registrados.</td></tr>';
                    }

                    // Render Fallas
                    const failBody = byIdEither("engFailBody", "failBody");
                    const failures = h.failures || h.Failures || [];
                    if (failBody) {
                        failBody.innerHTML = failures.length ? failures.map(f => `
                        <tr>
                            <td>${new Date(f.failureDate || f.FailureDate).toLocaleString("es-CR")}</td>
                            <td><span class="badge bg-danger">${f.severityLevel || f.SeverityLevel || "-"}</span></td>
                            <td>${escapeHtml(f.description || f.Description || "-")}</td>
                        </tr>
                    `).join("") : '<tr><td colspan="3" class="text-center text-muted">No se han reportado averías en esta turbina.</td></tr>';
                    }
                })
                .fail(function (xhr) {
                    console.error("Error cargando historial de turbina:", xhr);
                });
        }

        function setText(id, value) {
            const el = document.getElementById(id);
            if (el) el.textContent = value;
        }

        function setTextEither(idA, idB, value) {
            const el = byIdEither(idA, idB);
            if (el) el.textContent = value;
        }

    } // Cierre del bloque if (turbinesBody)

}); // Cierre del bloque global DOMContentLoaded
