// AdminDashboardViewController.js (§22.1, §27) - Controlador JS para el Panel de Administración y Gestión de Usuarios
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando AdminDashboardViewController...");

    // Verificación de seguridad en el cliente (RBAC §24.2)
    const role = session.getRole();
    const userId = session.getUserId() || 1;
    if (role !== "Administrator" && role !== "Admin") {
        notify.error("Acceso denegado. Requiere privilegios de Administrador.");
        setTimeout(() => {
            window.location.href = "/Login";
        }, 1500);
        return;
    }

    // ==========================================
    // 1. PANEL PRINCIPAL (/Admin/Dashboard)
    if (document.getElementById("kpiTotalTurbines")) {
        let turbineChartInstance = null;
        let capacityChartInstance = null;

        loadAdminDashboard();
        loadUserStats();
        // Auto-refrescar en tiempo real cada 15 segundos
        setInterval(loadAdminDashboard, 15000);
        setInterval(loadUserStats, 30000);

        function loadAdminDashboard() {
            Promise.all([
                apiClient.get("Turbines/RetrieveAll"),
                apiClient.get("CentralBanks/RetrieveAll"),
                apiClient.get("Forecasts/RetrieveAll"),
                apiClient.get("Distributions/RetrieveAll"),
                apiClient.get("Invoices/RetrieveAll"),
                apiClient.get("Flushs/RetrieveAll")
            ]).then(function (responses) {
                const turbines = responses[0]?.[0]?.data || responses[0]?.data || responses[0]?.Data || [];
                const centralBanks = responses[1]?.[0]?.data || responses[1]?.data || responses[1]?.Data || [];
                const forecasts = responses[2]?.[0]?.data || responses[2]?.data || responses[2]?.Data || [];
                const distributions = responses[3]?.[0]?.data || responses[3]?.data || responses[3]?.Data || [];
                const invoices = responses[4]?.[0]?.data || responses[4]?.data || responses[4]?.Data || [];
                const flushes = responses[5]?.[0]?.data || responses[5]?.data || responses[5]?.Data || [];

                const totalT = turbines.length;
                const activeT = turbines.filter(function (t) { return (t.status || t.Status) === "Active"; }).length;
                const cbInv = Number((centralBanks[0]?.currentInventoryMWh ?? centralBanks[0]?.CurrentInventoryMWh) || 0);
                const effCap = Number((centralBanks[0]?.maximumCapacityMWh ?? centralBanks[0]?.MaximumCapacityMWh) || 0);
                const monthF = forecasts.length;
                const totalDem = forecasts.reduce(function (sum, f) { return sum + Number(f.requestedEnergyMWh ?? f.RequestedEnergyMWh ?? 0); }, 0);
                const totalBill = invoices.reduce(function (sum, i) { return sum + Number(i.totalAmount ?? i.TotalAmount ?? i.amount ?? i.Amount ?? 0); }, 0);
                const flushDate = flushes[0]?.executedAt || flushes[0]?.ExecutedAt;

                setText("kpiTotalTurbines", totalT);
                setText("kpiActiveTurbines", activeT);
                setText("kpiCbInventory", formatNumber(cbInv) + " MWh");
                setText("kpiEffectiveCap", formatNumber(effCap) + " MWh");
                setText("kpiMonthForecasts", monthF);
                setText("kpiTotalDemand", formatNumber(totalDem) + " MWh");
                setText("kpiTotalBilled", "₡ " + formatNumber(totalBill));
                setText("kpiLastFlush", flushDate ? new Date(flushDate).toLocaleDateString("es-CR", { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : "Sin registros");

                renderAdminCharts(totalT, activeT, cbInv, effCap, totalDem);
            }).catch(function (xhr) {
                handleApiError(xhr);
            });
        }

        function loadUserStats() {
            apiClient.get("Users/RetrieveAll")
                .done(function (res) {
                    const users = res?.data || res?.Data || [];
                    const active = users.filter(u => (u.status || u.Status) === 'Active').length;
                    const total = users.length;
                    setText("kpiActiveUsers", active);
                    const hint = document.getElementById("kpiTotalUsersHint");
                    if (hint) hint.innerHTML = `<i class="bi bi-arrow-right-short"></i> ${active} activos de ${total} total`;
                })
                .fail(function () { setText("kpiActiveUsers", "-"); });
        }

        function renderAdminCharts(totalT, activeT, cbInv, effCap, totalDem) {
            if (typeof Chart === "undefined") return;

            const inactiveT = Math.max(0, totalT - activeT);
            const ctxTurbine = document.getElementById("adminTurbineChart")?.getContext("2d");
            if (ctxTurbine) {
                if (turbineChartInstance) {
                    turbineChartInstance.data.datasets[0].data = [activeT, inactiveT];
                    turbineChartInstance.update();
                } else {
                    turbineChartInstance = new Chart(ctxTurbine, {
                        type: "doughnut",
                        data: {
                            labels: ["Activas", "Inactivas / Mantenimiento"],
                            datasets: [{
                                data: [activeT, inactiveT],
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
                    capacityChartInstance.data.datasets[0].data = [cbInv, effCap, totalDem];
                    capacityChartInstance.update();
                } else {
                    capacityChartInstance = new Chart(ctxCap, {
                        type: "bar",
                        data: {
                            labels: ["Inventario Actual", "Capacidad Vigente", "Demanda Mes"],
                            datasets: [{
                                label: "Energía (MWh)",
                                data: [cbInv, effCap, totalDem],
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

    // ==========================================
    // 2. GESTIÓN DE USUARIOS (/Admin/Users)
    // ==========================================
    const tableBody = document.getElementById("usersTableBody");
    if (tableBody) {
        let allUsers = [];
        let editingUserId = null;
        let editingUserSnapshot = null;
        let editPhotoDataUrl = null;

        // Robust modal initialization
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

        // "Nuevo Usuario" button — manual open (no data-bs-toggle to avoid conflicts)
        const btnNewUser = document.getElementById("btnOpenNewUser");
        if (btnNewUser) {
            btnNewUser.addEventListener("click", function () {
                editingUserId = null;
                editingUserSnapshot = null;
                editPhotoDataUrl = null;
                document.getElementById("userForm")?.reset();
                const modalTitle = userModalEl?.querySelector(".modal-title");
                if (modalTitle) modalTitle.innerHTML = '<i class="bi bi-person-plus-fill me-2"></i>Nuevo Usuario Interno';

                // Enable all fields for creation
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
                attachPhotoPreview();
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

        function attachEditPhotoPreview() {
            const photoInput = document.getElementById("editPhoto");
            const photoPreview = document.getElementById("editPhotoPreview");
            if (!photoInput || photoInput.dataset.bound === "true") return;
            photoInput.dataset.bound = "true";
            photoInput.addEventListener("change", function () {
                const file = this.files?.[0];
                if (!file) { editPhotoDataUrl = null; if (photoPreview) photoPreview.classList.add("d-none"); return; }
                const img = new Image();
                img.onload = function () {
                    const MAX = 256;
                    const scale = Math.min(1, MAX / Math.max(img.width, img.height));
                    const canvas = document.createElement("canvas");
                    canvas.width = Math.round(img.width * scale);
                    canvas.height = Math.round(img.height * scale);
                    canvas.getContext("2d").drawImage(img, 0, 0, canvas.width, canvas.height);
                    editPhotoDataUrl = canvas.toDataURL("image/jpeg", 0.82);
                    URL.revokeObjectURL(img.src);
                    if (photoPreview) { photoPreview.src = editPhotoDataUrl; photoPreview.classList.remove("d-none"); }
                };
                img.onerror = function () { URL.revokeObjectURL(img.src); notify.error("No se pudo leer la imagen seleccionada."); };
                img.src = URL.createObjectURL(file);
            });
        }

        let userPhotoDataUrl = null;

        function attachPhotoPreview() {
            const photoInput = document.getElementById("uPhoto");
            const photoPreview = document.getElementById("uPhotoPreview");
            if (!photoInput || photoInput.dataset.bound === "true") return;
            photoInput.dataset.bound = "true";
            photoInput.addEventListener("change", function () {
                const file = this.files?.[0];
                if (!file) {
                    userPhotoDataUrl = null;
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
                    userPhotoDataUrl = canvas.toDataURL("image/jpeg", 0.82);
                    URL.revokeObjectURL(img.src);
                    if (photoPreview) {
                        photoPreview.src = userPhotoDataUrl;
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

        function getFullNameValue(user) {
            const name = (user.firstName || user.FirstName || "").trim();
            const last1 = (user.firstLastName || user.FirstLastName || "").trim();
            const last2 = (user.secondLastName || user.SecondLastName || "").trim();
            return [name, last1, last2].filter(Boolean).join(" ");
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

        function loadUsers() {
            tableBody.innerHTML = '<tr><td colspan="7" class="text-center"><span class="spinner-border spinner-border-sm" role="status"></span> Cargando usuarios...</td></tr>';
            apiClient.get("Users/RetrieveAll")
                .done(function (res) {
                    allUsers = Array.isArray(res) ? res : (res?.data || res?.Data || res?.items || res?.Items || []);
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
                const matchesRole = !roleFilter || (u.role || "") === roleFilter;
                return matchesQuery && matchesRole;
            });

            renderUsersTable(filtered);
        }

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
                const isActive = (u.status || "").toLowerCase() === "active";
                const isPending = (u.status || "").toLowerCase() === "pending" || (u.status || "").toLowerCase() === "pendingactivation";
                const phone = u.phoneNumber || u.PhoneNumber || u.phone || u.Phone || "-";

                let actions = `<button class="btn btn-sm btn-outline-primary me-1 btn-edit" data-id="${u.id}" title="Editar usuario"><i class="bi bi-pencil"></i> Editar</button>`;
                if (isActive) {
                    actions += `<button class="btn btn-sm btn-outline-danger btn-deactivate" data-id="${u.id}" title="Desactivar usuario"><i class="bi bi-person-x"></i> Desactivar</button>`;
                } else {
                    actions += `<button class="btn btn-sm btn-outline-success btn-reactivate" data-id="${u.id}" data-email="${escapeHtml(u.email || "")}" data-status="${escapeHtml(u.status || "")}" title="${isPending ? "Activar usuario" : "Reactivar usuario"}"><i class="bi bi-person-check"></i> ${isPending ? "Activar" : "Reactivar"}</button>`;
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

            // Bind action events
            tableBody.querySelectorAll(".btn-edit").forEach(btn => {
                btn.addEventListener("click", () => openEditModal(btn.getAttribute("data-id")));
            });
            tableBody.querySelectorAll(".btn-deactivate").forEach(btn => {
                btn.addEventListener("click", () => deactivateUser(btn.getAttribute("data-id")));
            });
            tableBody.querySelectorAll(".btn-reactivate").forEach(btn => {
                btn.addEventListener("click", () => reactivateUser(
                    btn.getAttribute("data-id"),
                    btn.getAttribute("data-email"),
                    btn.getAttribute("data-status")
                ));
            });
        }

        function getRoleBadge(role) {
            if (role === "Administrator") return '<span class="badge bg-danger">Administrador</span>';
            if (role === "Engineer") return '<span class="badge bg-info text-dark">Ingeniero</span>';
            if (role === "Distributor") return '<span class="badge bg-success">Comprador</span>';
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
            if ((status || "").toLowerCase() === "active") return '<span class="badge bg-success">Active</span>';
            if ((status || "").toLowerCase() === "inactive") return '<span class="badge bg-secondary">Inactive</span>';
            if ((status || "").toLowerCase() === "blocked") return '<span class="badge bg-danger">Blocked</span>';
            if ((status || "").toLowerCase() === "pendingactivation") return '<span class="badge bg-warning text-dark">Pending</span>';
            return `<span class="badge bg-warning text-dark">${status || "-"}</span>`;
        }

        // Edit modal
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
                attachEditPhotoPreview();

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

        // Save button
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

                console.log("[AdminUsers] create payload", dto);

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

                console.log("[AdminUsers] update payload", dto);

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

        function reactivateUser(id, email, status) {
            const normalizedStatus = (status || "").toLowerCase();
            if (normalizedStatus === "pending" || normalizedStatus === "pendingactivation") {
                // La activación ahora exige que el propio usuario establezca su
                // contraseña (§ auto-creación de contraseña en primer login), así que
                // el administrador ya no puede completarla en su nombre desde aquí.
                notify.info("El usuario debe activar su cuenta desde /Activate con el código enviado a su correo; ahí definirá su propia contraseña.");
                return;
            }

            notify.warning("Solo los usuarios pendientes de activación pueden reactivarse desde esta pantalla.");
        }
    }
});
