// AdminDashboardViewController.js - Controlador JS para el Panel de Administración y Gestión de Usuarios
document.addEventListener("DOMContentLoaded", function () {

    // Verificación de seguridad en el cliente
    const role = session.getRole();
    const userId = session.getUserId() || 1;
    if (role !== "Administrator" && role !== "Admin") {
        notify.error("Acceso denegado. Requiere privilegios de Administrador.");
        setTimeout(() => {
            window.location.href = "/Login";
        }, 1500);
        return;
    }

 
    // 1. PANEL PRINCIPAL (/Admin/Dashboard)
    // Funciones principales: carga métricas y gráficos del administrador.
    if (document.getElementById("kpiTotalTurbines")) {
        let turbineChartInstance = null;
        let capacityChartInstance = null;

        loadAdminDashboard();
        loadUserStats();
        // Auto-refrescar en tiempo real cada 15 segundos
        setInterval(loadAdminDashboard, 15000);
        setInterval(loadUserStats, 30000);

        function loadAdminDashboard() {
            apiClient.get("Dashboard/Admin")
                .done(function (res) {
                    const data = res?.data || res?.Data || res || {};

                    const totalTurbines = Number(data.totalTurbines ?? data.TotalTurbines ?? 0);
                    const activeTurbines = Number(data.activeTurbines ?? data.ActiveTurbines ?? 0);
             
                    const centralBankInventoryMWh = Number(data.centralBankInventory ?? data.CentralBankInventory ?? 0);
                    const effectiveCapacityMWh = Number(data.effectiveCapacity ?? data.EffectiveCapacity ?? 0);
                    const monthForecasts = Number(data.monthForecasts ?? data.MonthForecasts ?? 0);
                    const totalDemandMWh = Number(data.monthTotalDemand ?? data.MonthTotalDemand ?? 0);
                    const totalBilledAmount = Number(data.monthTotalBilled ?? data.MonthTotalBilled ?? 0);
                    const periodProductionMWh = Number(data.monthTotalDistributed ?? data.MonthTotalDistributed ?? 0);
                    const flushDate = data.lastFlush || data.LastFlush;
                   

                    setText("kpiTotalTurbines", totalTurbines);
                    setText("kpiActiveTurbines", activeTurbines);
                    
                    setText("kpiTurbinesInMaintenance", Number(totalTurbines - activeTurbines > 0 ? totalTurbines - activeTurbines : 0));
                    setText("kpiCbInventory", formatNumber(centralBankInventoryMWh) + " MWh");
                    setText("kpiEffectiveCap", formatNumber(effectiveCapacityMWh) + " MWh");
                    setText("kpiPeriodProduction", formatNumber(periodProductionMWh) + " MWh");
                    setText("kpiMonthForecasts", monthForecasts);
                    setText("kpiTotalDemand", formatNumber(totalDemandMWh) + " MWh");
                    setText("kpiTotalBilled", "₡ " + formatNumber(totalBilledAmount));
                    setText("kpiLastFlush", flushDate ? new Date(flushDate).toLocaleDateString("es-CR", { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : "Sin registros");
                    
                    renderAdminCharts(totalTurbines, activeTurbines, centralBankInventoryMWh, effectiveCapacityMWh, totalDemandMWh);
                })
                .fail(function (xhr) {
                    handleApiError(xhr);
                    
                });
            apiClient.get("Batteries/RetrieveAllBatteries")
                .done(function (res) {
                const batteries = apiClient.unwrapList(res);
                setText("kpiBatteries", batteries.length);
            })
            .fail(function (xhr) {
                handleApiError(xhr);
                setText("kpiBatteries", "-");
            });
        }

        // Traigo todos los usuarios y cuento cuántos tienen estado activo.
        function loadUserStats() {
            apiClient.get("Users/RetrieveAll")
                .done(function (res) {
                    const users = apiClient.unwrapList(res);
                    const active = (users || []).filter(u => ((u.status || u.Status) || "").toLowerCase() === 'active').length;
                    const total = (users || []).length;
                    setText("kpiActiveUsers", active);
                    const hint = document.getElementById("kpiTotalUsersHint");
                    if (hint) hint.innerHTML = `<i class="bi bi-arrow-right-short"></i> ${active} activos de ${total} total`;
                })
                .fail(function () { setText("kpiActiveUsers", "-"); });
        }

        function renderAdminCharts(totalTurbines, activeTurbines, centralBankInventoryMWh, effectiveCapacityMWh, totalDemandMWh) {
            if (typeof Chart === "undefined") return;

            const inactiveTurbines = Math.max(0, totalTurbines - activeTurbines);
            const ctxTurbine = document.getElementById("adminTurbineChart")?.getContext("2d");
            if (ctxTurbine) {
                if (turbineChartInstance) {
                    turbineChartInstance.data.datasets[0].data = [activeTurbines, inactiveTurbines];
                    turbineChartInstance.update();
                } else {
                    turbineChartInstance = new Chart(ctxTurbine, {
                        type: "doughnut",
                        data: {
                            labels: ["Activas", "Inactivas / Mantenimiento"],
                                datasets: [{
                                data: [activeTurbines, inactiveTurbines],
                                backgroundColor: ["#107C62", "#D97706"],
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

            const ctxCap = document.getElementById("adminCapacityChart")?.getContext("2d");
            if (ctxCap) {
                if (capacityChartInstance) {
                    capacityChartInstance.data.datasets[0].data = [centralBankInventoryMWh, effectiveCapacityMWh, totalDemandMWh];
                    capacityChartInstance.update();
                } else {
                    capacityChartInstance = new Chart(ctxCap, {
                        type: "bar",
                        data: {
                            labels: ["Inventario Actual", "Capacidad Vigente", "Demanda Mes"],
                            datasets: [{
                                label: "Energía (MWh)",
                                data: [centralBankInventoryMWh, effectiveCapacityMWh, totalDemandMWh],
                                backgroundColor: ["#5A2CA0", "#2563EB", "#B91C1C"],
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

    // 2. GESTIÓN DE USUARIOS (/Admin/Users)
    const tableBody = document.getElementById("usersTableBody");
    if (tableBody) {
        let allUsers = [];
        let editingUserId = null;
        let editingUserSnapshot = null;
        let editPhotoDataUrl = null;

        // Inicialización robusta del modal
        const userModalEl = document.getElementById("userModal");
        const editUserModalEl = document.getElementById("editUserModal");
        let userModal = null;
        let editUserModal = null;
        if (userModalEl) {
            try {
                userModal = bootstrap.Modal.getOrCreateInstance(userModalEl);
            } catch (e) {
                console.warn("Could not initialize userModal:", e);
            }
        }
        if (editUserModalEl) {
            try {
                editUserModal = bootstrap.Modal.getOrCreateInstance(editUserModalEl);
            } catch (e) {
                console.warn("Could not initialize editUserModal:", e);
            }
        }

        loadUsers();

        // Búsqueda y filtrado en tiempo real
        const searchInput = document.getElementById("searchUser");
        const filterSelect = document.getElementById("filterRole");

        if (searchInput) searchInput.addEventListener("input", filterAndRenderUsers);
        if (filterSelect) filterSelect.addEventListener("change", filterAndRenderUsers);

        // Botón "Nuevo Usuario" — apertura manual (sin data-bs-toggle para evitar conflictos)
        const btnNewUser = document.getElementById("btnOpenNewUser");
        if (btnNewUser) {
            btnNewUser.addEventListener("click", function () {
                editingUserId = null;
                editingUserSnapshot = null;
                editPhotoDataUrl = null;
                document.getElementById("userForm")?.reset();
                const modalTitle = userModalEl?.querySelector(".modal-title");
                if (modalTitle) modalTitle.innerHTML = '<i class="bi bi-person-plus-fill me-2"></i>Nuevo Usuario Interno';

                
                // Habilitar todos los campos para la creación
                setFieldState("uId", true, false);
                setFieldState("uEmail", true, false);
                setFieldState("uPhone", true, false);
                setFieldState("uBirthDate", true, false);
                setFieldState("uAge", true, true);
                setFieldState("uPhoto", true, false);
                setFieldState("uRole", true, false);
                setFieldState("uPass", true, false);
                setFieldState("uLast1", true, false);
                setFieldState("uLast2", true, false);

                const passHint = document.getElementById("passHint");
                if (passHint) passHint.textContent = "(requerida)";

                const birthDateInput = document.getElementById("uBirthDate");
                if (birthDateInput) birthDateInput.value = "";
                const ageInput = document.getElementById("uAge");
                if (ageInput) ageInput.value = "";
                const photoPreview = document.getElementById("uPhotoPreview");
                if (photoPreview) photoPreview.classList.add("d-none");
                userPhotoDataUrl = null;
                attachPhotoPreview("uPhoto", "uPhotoPreview");
                updateCalculatedAge();

                if (userModal) userModal.show();
            });
        }

        function setFieldState(id, visible, disabled) {
            const el = document.getElementById(id);
            if (el) {
                el.disabled = disabled;
                const wrapper = el.closest('.col-md-6') || el.closest('.mb-3') || el.closest('.mb-2');
                if (wrapper) wrapper.style.display = visible ? '' : 'none';
            }
        }

        let userPhotoDataUrl = null;

        function attachPhotoPreview(inputId, previewId, dataUrlVariable) {
            const photoInput = document.getElementById(inputId);
            const photoPreview = document.getElementById(previewId);
            if (!photoInput || photoInput.dataset.bound === "true") return;
            photoInput.dataset.bound = "true";
            photoInput.addEventListener("change", function () {
                const file = this.files?.[0];
                if (!file) {
                    if (inputId === "editPhoto") editPhotoDataUrl = null;
                    else userPhotoDataUrl = null;
                    if (photoPreview) photoPreview.classList.add("d-none");
                    return;
                }
                const img = new Image();
                img.onload = function () {
                    const MAX = 256;
                    const scale = Math.min(1, MAX / Math.max(img.width, img.height));
                    const canvas = document.createElement("canvas");
                    canvas.width = Math.round(img.width * scale);
                    canvas.height = Math.round(img.height * scale);
                    canvas.getContext("2d").drawImage(img, 0, 0, canvas.width, canvas.height);
                    const dataUrl = canvas.toDataURL("image/jpeg", 0.82);
                    if (inputId === "editPhoto") editPhotoDataUrl = dataUrl;
                    else userPhotoDataUrl = dataUrl;
                    URL.revokeObjectURL(img.src);
                    if (photoPreview) {
                        photoPreview.src = dataUrl;
                        photoPreview.classList.remove("d-none");
                    }
                };
                img.onerror = function () {
                    URL.revokeObjectURL(img.src);
                    notify.error("No se pudo leer la imagen seleccionada.");
                };
                img.src = URL.createObjectURL(file);
            });
        }

        function updateCalculatedAge() {
            const birthDateInput = document.getElementById("uBirthDate");
            const ageInput = document.getElementById("uAge");
            if (!birthDateInput || !ageInput) return;
            const value = birthDateInput.value;
            if (!value) {
                ageInput.value = "";
                return;
            }
            const birth = new Date(value);
            const today = new Date();
            let age = today.getFullYear() - birth.getFullYear();
            const monthDiff = today.getMonth() - birth.getMonth();
            if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birth.getDate())) age--;
            ageInput.value = `${age} años`;
        }

        const birthDateField = document.getElementById("uBirthDate");
        if (birthDateField && birthDateField.dataset.bound !== "true") {
            birthDateField.dataset.bound = "true";
            birthDateField.addEventListener("input", updateCalculatedAge);
            birthDateField.addEventListener("change", updateCalculatedAge);
        }

        function getFirstNameValue(user) {
            return (user.firstName || user.FirstName || "").trim();
        }

        function getDisplayFullName(user) {
            const firstName = (user.firstName || user.FirstName || "").trim();
            const last1 = (user.firstLastName || user.FirstLastName || "").trim();
            const last2 = (user.secondLastName || user.SecondLastName || "").trim();

            const firstNameLower = firstName.toLowerCase();
            const hasLast1 = last1 && firstNameLower.includes(last1.toLowerCase());
            const hasLast2 = last2 && firstNameLower.includes(last2.toLowerCase());

            if (hasLast1 || hasLast2) {
                return firstName;
            }

            return [firstName, last1, last2].filter(Boolean).join(" ").replace(/\s+/g, " ").trim();
        }

        // se verifica que si o si trigan los roles correctamente para luego su filtrado 
        function normalizeRole(role) {
            const value = (role || "").toString().trim().toLowerCase();
            if (value === "admin" || value === "administrator") return "administrator";
            if (value === "ing" || value === "ing." || value === "engineer" || value === "ingeniero") return "engineer";
            if (value === "buyer" || value === "distributor" || value === "customer") return "distributor";
            return value;
        }

        // Carga la lista completa de usuarios y llama al filtrado/render.
        function loadUsers() {
            tableBody.innerHTML = '<tr><td colspan="7" class="text-center"><span class="spinner-border spinner-border-sm" role="status"></span> Cargando usuarios...</td></tr>';
            apiClient.get("Users/RetrieveAll")
                .done(function (res) {
                    allUsers = apiClient.unwrapList(res);
                    filterAndRenderUsers();
                })
                .fail(function (xhr) {
                    tableBody.innerHTML = '<tr><td colspan="7" class="text-center text-danger">Error al cargar la lista de usuarios.</td></tr>';
                    handleApiError(xhr);
                });
        }

        function filterAndRenderUsers() {
            const query = searchInput?.value.toLowerCase().trim() || "";
            const roleFilter = filterSelect?.value || "";

            const filtered = allUsers.filter(u => {
                const matchesQuery = !query ||
                    (u.identification || "").toLowerCase().includes(query) ||
                    (u.firstName || "").toLowerCase().includes(query) ||
                    (u.firstLastName || "").toLowerCase().includes(query) ||
                    (u.secondLastName || "").toLowerCase().includes(query) ||
                    (u.email || "").toLowerCase().includes(query);
                // Comparo el rol sin importar como venga
                const matchesRole = !roleFilter || normalizeRole(u.role) === normalizeRole(roleFilter);
                return matchesQuery && matchesRole;
            });

            renderUsersTable(filtered);
        }

        // Renderiza la tabla de usuarios en el DOM. Recibe un arreglo ya filtrado.
        function renderUsersTable(users) {
            if (!users.length) {
                tableBody.innerHTML = '<tr><td colspan="7" class="text-center text-muted py-4">No se encontraron usuarios que coincidan con los filtros.</td></tr>';
                return;
            }

            tableBody.innerHTML = users.map(u => {
                const fullName = getDisplayFullName(u);
                const age = u.age ?? u.Age ?? (u.birthDate || u.BirthDate ? calculateAgeFromDate(u.birthDate || u.BirthDate) : "-");
                const roleBadge = getRoleBadge(u.role);
                const statusBadge = getStatusBadge(u.status);
                const normalizedStatus = (u.status || "").toLowerCase();
                const isActive = normalizedStatus === "active";
                const isPending = normalizedStatus === "pending" || normalizedStatus === "pendingactivation";
                const isInactive = normalizedStatus === "inactive";
                const isLocked = normalizedStatus === "locked" || normalizedStatus === "blocked";
                const phone = u.phoneNumber || u.PhoneNumber || u.phone || u.Phone || "-";

                let actions = `<button class="btn btn-sm btn-outline-primary me-1 btn-edit" data-id="${u.id}" title="Editar usuario"><i class="bi bi-pencil"></i> Editar</button>`;
                if (isActive) {
                    actions += `<button class="btn btn-sm btn-outline-danger btn-deactivate" data-id="${u.id}" title="Desactivar usuario"><i class="bi bi-person-x"></i> Desactivar</button>`;
                } else if (isPending) {
                    actions += `<button class="btn btn-sm btn-outline-warning btn-reactivate" data-id="${u.id}" data-status="${escapeHtml(u.status || "")}" title="Activar usuario con OTP"><i class="bi bi-envelope-check"></i> Activar</button>`;
                } else if (isInactive) {
                    actions += `<button class="btn btn-sm btn-outline-success btn-reactivate" data-id="${u.id}" data-status="${escapeHtml(u.status || "")}" title="Reactivar usuario"><i class="bi bi-person-check"></i> Reactivar</button>`;
                } else if (isLocked) {
                    actions += `<button class="btn btn-sm btn-outline-secondary" disabled title="El desbloqueo requiere una acción separada"><i class="bi bi-lock"></i> Bloqueado</button>`;
                }

                return `
                    <tr>
                        <td class="fw-bold text-center">${escapeHtml(u.identification || "-")}</td>
                        <td class="text-center">${escapeHtml(fullName || "-")}</td>
                        <td class="text-center">${escapeHtml(age === "-" ? "-" : `${age} años`)}</td>
                        <td class="text-center">${escapeHtml(u.email || "-")}</td>
                        <td class="text-center">${escapeHtml(phone)}</td>
                        <td class="text-center">${roleBadge}</td>
                        <td class="text-center">${statusBadge}</td>
                        <td class="text-center text-nowrap">${actions}</td>
                    </tr>
                `;
            }).join("");

            // Vincular eventos de acción
            tableBody.querySelectorAll(".btn-edit").forEach(btn => {
                btn.addEventListener("click", () => openEditModal(btn.getAttribute("data-id")));
            });
            tableBody.querySelectorAll(".btn-deactivate").forEach(btn => {
                btn.addEventListener("click", () => deactivateUser(btn.getAttribute("data-id")));
            });
            tableBody.querySelectorAll(".btn-reactivate").forEach(btn => {
                btn.addEventListener("click", () => reactivateUser(
                    btn.getAttribute("data-id"),
                    btn.getAttribute("data-status")
                ));
            });
        }

        function getRoleBadge(role) {
            const normalizedRole = normalizeRole(role);
            if (normalizedRole === "administrator") return '<span class="badge bg-danger">Administrador</span>';
            if (normalizedRole === "engineer") return '<span class="badge bg-info text-dark">Ingeniero</span>';
            if (normalizedRole === "distributor") return '<span class="badge bg-success">Comprador</span>';
            return `<span class="badge bg-secondary">${role || "-"}</span>`;
        }

        function calculateAgeFromDate(dateValue) {
            if (!dateValue) return "-";
            const birth = new Date(dateValue);
            if (Number.isNaN(birth.getTime())) return "-";
            const today = new Date();
            let age = today.getFullYear() - birth.getFullYear();
            const monthDiff = today.getMonth() - birth.getMonth();
            if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birth.getDate())) age--;
            return age;
        }

        function getStatusBadge(status) {
            if ((status || "").toLowerCase() === "active") return '<span class="badge bg-success">Activo</span>';
            if ((status || "").toLowerCase() === "inactive") return '<span class="badge bg-secondary">Inactivo</span>';
            if (["locked", "blocked"].includes((status || "").toLowerCase())) return '<span class="badge bg-danger">Bloqueado</span>';
            if (["pending", "pendingactivation"].includes((status || "").toLowerCase())) return '<span class="badge bg-warning text-dark">Pendiente</span>';
            return `<span class="badge bg-warning text-dark">${status || "-"}</span>`;
        }

       
        // Ventana para editar
        function openEditModal(id) {
            const u = allUsers.find(item => String(item.id) === String(id));
            if (!u) {
                notify.error("No se encontró el usuario.");
                return;
            }

            const applyUserToEditModal = function (sourceUser) {
                editingUserId = sourceUser.id ?? sourceUser.Id ?? u.id;
                editingUserSnapshot = sourceUser;
                const modalTitle = editUserModalEl?.querySelector(".modal-title");
                if (modalTitle) modalTitle.innerHTML = '<i class="bi bi-pencil-square me-2"></i>Editar Usuario';

                const idInput = document.getElementById("editUserId");
                const emailInput = document.getElementById("editEmail");
                const fullNameInput = document.getElementById("editFullName");
                const last1Input = document.getElementById("editLast1");
                const last2Input = document.getElementById("editLast2");
                const phoneInput = document.getElementById("editPhone");
                const roleInput = document.getElementById("editRole");

                if (idInput) {
                    idInput.value = editingUserId || "";
                    idInput.disabled = true;
                }
                if (emailInput) {
                    emailInput.value = sourceUser.email || sourceUser.Email || "";
                    emailInput.disabled = false;
                }

                if (fullNameInput) { fullNameInput.value = getFirstNameValue(sourceUser); fullNameInput.disabled = false; }
                if (last1Input) { last1Input.value = sourceUser.firstLastName || sourceUser.FirstLastName || ""; last1Input.disabled = false; }
                if (last2Input) { last2Input.value = sourceUser.secondLastName || sourceUser.SecondLastName || ""; last2Input.disabled = false; }
                if (phoneInput) { phoneInput.value = sourceUser.phoneNumber || sourceUser.PhoneNumber || sourceUser.phone || sourceUser.Phone || ""; phoneInput.disabled = false; }

                const photoPreview = document.getElementById("editPhotoPreview");
                if (photoPreview) {
                    const photo = sourceUser.profilePhoto || sourceUser.ProfilePhoto || sourceUser.photoUrl || sourceUser.PhotoUrl;
                    if (photo) {
                        photoPreview.src = photo;
                        photoPreview.classList.remove("d-none");
                    } else {
                        photoPreview.classList.add("d-none");
                    }
                }

                if (roleInput) { roleInput.value = sourceUser.role || sourceUser.Role || "Engineer"; roleInput.disabled = false; }
                attachPhotoPreview("editPhoto", "editPhotoPreview");

                if (editUserModal) {
                    editUserModal.show();
                } else {
                    try {
                        editUserModal = new bootstrap.Modal(editUserModalEl);
                        editUserModal.show();
                    } catch(e) {
                        notify.error("Error al abrir el formulario de edición.");
                        console.error("Modal init error:", e);
                    }
                }
            };

            apiClient.get(`Users/RetrieveById/${id}`)
                .done(function (res) {
                    const sourceUser = res?.data || res?.Data || res || u;
                    applyUserToEditModal(sourceUser);
                })
                .fail(function () {
                    applyUserToEditModal(u);
                });
        }

        // Botón de guardar
        const saveBtn = document.getElementById("saveUserBtn");
        if (saveBtn) {
            saveBtn.addEventListener("click", function () {
                const isEditing = !!editingUserId;
                const idVal = (isEditing ? document.getElementById("editUserId") : document.getElementById("uId"))?.value.trim();
                const fullNameVal = (isEditing ? document.getElementById("editFullName") : document.getElementById("uFullName"))?.value.trim() || "";
                const l1 = (isEditing ? document.getElementById("editLast1") : document.getElementById("uLast1"))?.value.trim() || "";
                const l2 = (isEditing ? document.getElementById("editLast2") : document.getElementById("uLast2"))?.value.trim() || "";
                const emailVal = (isEditing ? document.getElementById("editEmail") : document.getElementById("uEmail"))?.value.trim();
                const phoneVal = (isEditing ? document.getElementById("editPhone") : document.getElementById("uPhone"))?.value.trim();
                const birthDateVal = (isEditing ? document.getElementById("editBirthDate") : document.getElementById("uBirthDate"))?.value || "";
                const ageVal = (isEditing ? document.getElementById("editAge") : document.getElementById("uAge"))?.value || "";
                const roleVal = (isEditing ? document.getElementById("editRole") : document.getElementById("uRole"))?.value;
                const passVal = (isEditing ? document.getElementById("editPass") : document.getElementById("uPass"))?.value;

                if (!fullNameVal) {
                    notify.warning("Por favor ingrese el nombre completo.");
                    return;
                }

                saveBtn.disabled = true;
                saveBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Guardando...';

                if (editingUserId) {
                    saveBtn.disabled = false;
                    saveBtn.innerHTML = '<i class="bi bi-check-lg me-1"></i>Guardar';
                    notify.warning("Use el formulario de edición para modificar usuarios.");
                    return;
                }

                if (!idVal || !emailVal) {
                    notify.warning("Por favor complete identificación y correo para nuevos usuarios.");
                    saveBtn.disabled = false;
                    saveBtn.innerHTML = '<i class="bi bi-check-lg me-1"></i>Guardar';
                    return;
                }

                if (!phoneVal) {
                    notify.warning("Por favor ingrese un número de teléfono.");
                    saveBtn.disabled = false;
                    saveBtn.innerHTML = '<i class="bi bi-check-lg me-1"></i>Guardar';
                    return;
                }

                if (!birthDateVal) {
                    notify.warning("Por favor ingrese la fecha de nacimiento.");
                    saveBtn.disabled = false;
                    saveBtn.innerHTML = '<i class="bi bi-check-lg me-1"></i>Guardar';
                    return;
                }

                const normalizedId = String(idVal).trim();
                const normalizedEmail = String(emailVal).trim().toLowerCase();
                const duplicateUser = allUsers.find(u => {
                    const existingId = String(u.identification || u.Identification || "").trim();
                    const existingEmail = String(u.email || u.Email || "").trim().toLowerCase();
                    return existingId === normalizedId || existingEmail === normalizedEmail;
                });

                if (duplicateUser) {
                    saveBtn.disabled = false;
                    saveBtn.innerHTML = '<i class="bi bi-check-lg me-1"></i>Guardar';
                    notify.warning("Ya existe un usuario con esa identificación o correo.");
                    return;
                }

                const dto = {
                    Identification: idVal,
                    FirstName: fullNameVal,
                    FirstLastName: l1,
                    SecondLastName: l2 || null,
                    Email: emailVal,
                    Role: roleVal,
                    PhoneNumber: phoneVal,
                    BirthDate: birthDateVal,
                    Age: Number.isFinite(parseInt(ageVal)) ? parseInt(ageVal) : 0,
                    ProfilePhoto: userPhotoDataUrl,
                    // Sin campo de contraseña en el modal de creación: el backend genera una
                    // aleatoria que nadie conoce; el usuario define la suya propia al activar
                    // la cuenta. passVal solo aplica al modal de edición (reseteo puntual).
                    Password: passVal || "",
                    Status: "Pending"
                };


                apiClient.post("Users/Create?callerUserId=" + userId, dto)
                    .done(function () {
                        notify.success("Usuario creado con éxito.");
                        userModal?.hide();
                        loadUsers();
                    })
                    .fail(function (xhr) {
                        handleApiError(xhr);
                    })
                    .always(function () {
                        saveBtn.disabled = false;
                        saveBtn.innerHTML = '<i class="bi bi-check-lg me-1"></i>Guardar';
                    });
            });
        }

        const saveEditBtn = document.getElementById("saveEditUserBtn");
        if (saveEditBtn) {
            saveEditBtn.addEventListener("click", function () {
                const idVal = document.getElementById("editUserId")?.value.trim();
                const emailVal = document.getElementById("editEmail")?.value.trim();
                const fullNameVal = document.getElementById("editFullName")?.value.trim() || "";
                const l1 = document.getElementById("editLast1")?.value.trim() || "";
                const l2 = document.getElementById("editLast2")?.value.trim() || "";
                const phoneVal = document.getElementById("editPhone")?.value.trim();
                const roleVal = document.getElementById("editRole")?.value;

                if (!idVal || !emailVal || !fullNameVal || !l1 || !phoneVal) {
                    notify.warning("Complete correo, nombres, apellido 1 y teléfono.");
                    return;
                }

                const btn = saveEditBtn;
                const original = btn.innerHTML;
                btn.disabled = true;
                btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Guardando...';

                const birthDateSource = editingUserSnapshot?.birthDate || editingUserSnapshot?.BirthDate;
                const dto = {
                    id: parseInt(idVal),
                    identification: editingUserSnapshot?.identification || editingUserSnapshot?.Identification || "",
                    firstName: fullNameVal,
                    firstLastName: l1,
                    secondLastName: l2 || null,
                    email: emailVal,
                    phoneNumber: phoneVal,
                    profilePhoto: editPhotoDataUrl || (editingUserSnapshot?.profilePhoto || editingUserSnapshot?.ProfilePhoto || null),
                    role: roleVal,
                    status: editingUserSnapshot?.status || editingUserSnapshot?.Status || "Active",
                    birthDate: birthDateSource ? new Date(birthDateSource).toISOString() : new Date().toISOString(),
                    password: ""
                };


                apiClient.put("Users/Update?callerUserId=" + userId, dto)
                    .done(function () {
                        notify.success("Usuario actualizado correctamente.");
                        editUserModal?.hide();
                        loadUsers();
                    })
                    .fail(function (xhr) {
                        handleApiError(xhr);
                    })
                    .always(function () {
                        btn.disabled = false;
                        btn.innerHTML = original;
                    });
            });
        }

        function deactivateUser(id) {
            notify.confirm("¿Está seguro de que desea desactivar este usuario?", { dangerous: true, confirmText: "Desactivar" }).then(function (ok) {
                if (!ok) return;
                apiClient.delete("Users/Delete?callerUserId=" + userId, { id: parseInt(id) })
                    .done(function () {
                        notify.success("Usuario desactivado correctamente.");
                        loadUsers();
                    })
                    .fail(function (xhr) {
                        handleApiError(xhr);
                    });
            });
        }

        function reactivateUser(id, status) {
            const normalizedStatus = (status || "").toLowerCase();
            if (normalizedStatus === "pending" || normalizedStatus === "pendingactivation") {
                // La activación ahora exige que el propio usuario establezca su
                // contraseña (§ auto-creación de contraseña en primer login), así que
                // el administrador ya no puede completarla en su nombre desde aquí.
                notify.info("El usuario debe activar su cuenta desde /Activate con el código enviado a su correo; ahí definirá su propia contraseña.");
                return;
            }

            if (normalizedStatus !== "inactive") {
                notify.warning("Solo se pueden reactivar usuarios inactivos.");
                return;
            }

            notify.confirm("¿Desea reactivar este usuario? Podrá volver a iniciar sesión con su contraseña actual.", {
                confirmText: "Reactivar"
            }).then(function (ok) {
                if (!ok) return;

                apiClient.post(`Users/Reactivate/${parseInt(id)}?callerUserId=${userId}`, {})
                    .done(function (res) {
                        notify.success(res?.message || "Usuario reactivado correctamente.");
                        loadUsers();
                    })
                    .fail(function (xhr) {
                        handleApiError(xhr);
                    });
            });
        }
    }
});
