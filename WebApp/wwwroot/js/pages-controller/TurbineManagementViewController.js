// TurbineManagementViewController.js (§22.1, §27) - Controlador JS para el Control de Turbinas y Detalle Técnico
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando TurbineManagementViewController...");

    const token = session.getToken();
    const role = session.getRole();
    if (!token || (role !== "Engineer" && role !== "Administrator" && role !== "Admin")) {
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
                    allTurbines = res?.data || res?.Data || [];
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
                    (t.uniqueCode || t.UniqueCode || t.turbineCode || t.Code || "").toLowerCase().includes(query) || 
                    (t.name || "").toLowerCase().includes(query) || 
                    (t.location || "").toLowerCase().includes(query);
                const matchesStatus = !statusVal || (t.status || t.Status || t.state || t.State || "").toLowerCase() === statusVal.toLowerCase();
                return matchesQuery && matchesStatus;
            });
            renderTurbinesTable(filtered);
        }

        function renderTurbinesTable(turbines) {
            if (!turbines.length) {
                turbinesBody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No se encontraron turbinas.</td></tr>';
                return;
            }

            turbinesBody.innerHTML = turbines.map(t => {
                const code = t.uniqueCode || t.UniqueCode || t.turbineCode || t.Code || "-";
                const name = t.name || "-";
                const loc = t.location || "-";
                const cap = Number(t.weeklyNominalCapacity || t.WeeklyNominalCapacity || 0).toLocaleString("es-CR", { minimumFractionDigits: 2 });
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
                            ${isAdmin ? `<button class="btn btn-sm btn-outline-secondary me-1 btn-edit" data-id="${t.id}" title="Editar Turbina"><i class="bi bi-pencil"></i> Editar</button>` : ""}
                            <button class="btn btn-sm btn-outline-warning me-1 btn-state" data-id="${t.id}" data-state="${stateVal}" title="Cambiar Estado">
                                <i class="bi bi-gear"></i> Estado
                            </button>
                            <a href="${roleBase}/TurbineDetail?id=${t.id}" class="btn btn-sm btn-outline-info" title="Ver Detalle Técnico">
                                <i class="bi bi-eye"></i> Detalle
                            </a>
                        </td>
                    </tr>
                `;
            }).join("");

            turbinesBody.querySelectorAll(".btn-state").forEach(btn => {
                btn.addEventListener("click", () => openStateModal(btn.getAttribute("data-id"), btn.getAttribute("data-state")));
            });
            turbinesBody.querySelectorAll(".btn-edit").forEach(btn => {
                btn.addEventListener("click", () => {
                    const t = allTurbines.find(x => String(x.id) === btn.getAttribute("data-id"));
                    if (t) openEditModal(t);
                });
            });
        }

        function getStateBadge(state) {
            const s = (state || "").toLowerCase();
            if (s === "active") return '<span class="badge bg-success">Activa</span>';
            if (s === "undermaintenance") return '<span class="badge bg-warning text-dark">Mantenimiento</span>';
            if (s === "damaged") return '<span class="badge bg-danger">Dañada / Falla</span>';
            if (s === "suspended" || s === "suspendedfornoncompliance") return '<span class="badge bg-dark">Suspendida</span>';
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
                    <option value="UnderMaintenance">UnderMaintenance (En Mantenimiento)</option>
                    <option value="Damaged">Damaged (Falla Técnica)</option>
                    <option value="SuspendedForNonCompliance">Suspended (Incumplimiento / Parada)</option>
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

        // Al abrir el modal para "Nueva Turbina" (no vía botón Editar), limpia cualquier estado de edición previo.
        if (tModalEl) {
            tModalEl.addEventListener("show.bs.modal", function (event) {
                if (event.relatedTarget && event.relatedTarget.classList.contains("btn-edit")) return;
                editingTurbineId = null;
                document.getElementById("turbineForm")?.reset();
                if (tModalTitle) tModalTitle.textContent = "Registrar Turbina";
                if (tCodeInput) tCodeInput.disabled = false;
                if (tYearInput) tYearInput.disabled = false;
                if (tCodeField) tCodeField.classList.remove("d-none");
            });
        }

        function openEditModal(t) {
            editingTurbineId = t.id;
            if (tModalTitle) tModalTitle.textContent = `Editar Turbina — ${t.uniqueCode || t.UniqueCode || ""}`;
            const setVal = (id, val) => { const el = document.getElementById(id); if (el) el.value = val ?? ""; };
            setVal("tCode", t.uniqueCode || t.UniqueCode);
            setVal("tName", t.name);
            setVal("tLoc", t.location);
            setVal("tBrand", t.brand || t.Brand);
            setVal("tModel", t.model || t.Model);
            setVal("tYear", t.year || t.Year);
            setVal("tCap", t.weeklyNominalCapacity || t.WeeklyNominalCapacity);
            // UniqueCode y Year no son editables (UpdateTurbineRequest no los admite) — se muestran de solo lectura.
            if (tCodeInput) tCodeInput.disabled = true;
            if (tYearInput) tYearInput.disabled = true;
            const tModalInst = tModalEl ? (bootstrap.Modal.getInstance(tModalEl) || new bootstrap.Modal(tModalEl)) : null;
            tModalInst?.show();
        }

        const saveTurbineBtn = document.getElementById("saveTurbineBtn");
        if (saveTurbineBtn) {
            saveTurbineBtn.addEventListener("click", function () {
                const isEdit = editingTurbineId != null;
                const dto = {
                    uniqueCode: document.getElementById("tCode")?.value.trim(),
                    name: document.getElementById("tName")?.value.trim(),
                    location: document.getElementById("tLoc")?.value.trim(),
                    brand: document.getElementById("tBrand")?.value.trim(),
                    model: document.getElementById("tModel")?.value.trim(),
                    year: parseInt(document.getElementById("tYear")?.value || 0),
                    weeklyNominalCapacity: parseFloat(document.getElementById("tCap")?.value || 0)
                };
                if (!dto.uniqueCode || !dto.name || !dto.location || !dto.weeklyNominalCapacity) {
                    notify.warning("Por favor complete los campos obligatorios.");
                    return;
                }
                saveTurbineBtn.disabled = true;
                saveTurbineBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Guardando...';

                const request = isEdit
                    ? apiClient.put("Turbines/Update", { turbineId: editingTurbineId, name: dto.name, location: dto.location, brand: dto.brand, model: dto.model, weeklyNominalCapacity: dto.weeklyNominalCapacity })
                    : apiClient.post("Turbines/Register", dto);

                request.done(function () {
                    notify.success(isEdit ? "Turbina actualizada exitosamente." : "Turbina registrada exitosamente.");
                    const tModalInst = bootstrap.Modal.getInstance(tModalEl) || (tModalEl ? new bootstrap.Modal(tModalEl) : null);
                    tModalInst?.hide();
                    document.getElementById("turbineForm")?.reset();
                    editingTurbineId = null;
                    loadTurbines();
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
        apiClient.get("Turbines/" + id)
            .done(function (res) {
                const t = res?.data || res?.Data || {};
                const nameEl = byIdEither("engDetName", "detName");
                const metaEl = byIdEither("engDetMeta", "detMeta");
                const statusEl = byIdEither("engDetStatus", "detStatus");

                if (nameEl) nameEl.textContent = t.name || `Turbina #${t.id || id}`;
                if (metaEl) metaEl.textContent = `Código: ${t.uniqueCode || t.UniqueCode || t.turbineCode || t.Code || "-"} | Ubicación: ${t.location || "-"} | Capacidad: ${Number(t.weeklyNominalCapacity || t.WeeklyNominalCapacity || 0).toLocaleString("es-CR")} MWh/sem`;
                
                if (statusEl) {
                    const st = t.status || t.Status || t.state || t.State || "Unknown";
                    const s = st.toLowerCase();
                    statusEl.textContent = st;
                    statusEl.className = "badge fs-6 " + (s === "active" ? "bg-success" : s === "undermaintenance" ? "bg-warning text-dark" : s === "damaged" ? "bg-danger" : "bg-secondary");
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
});
