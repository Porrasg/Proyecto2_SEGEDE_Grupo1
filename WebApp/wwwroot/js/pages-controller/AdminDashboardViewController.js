// AdminDashboardViewController.js (§22.1, §27) - Controlador JS para el Panel de Administración y Gestión de Usuarios
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando AdminDashboardViewController...");

    // Verificación de seguridad en el cliente (RBAC §24.2)
    const token = session.getToken();
    const role = session.getRole();
    if (!token || (role !== "Administrator" && role !== "Admin")) {
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
            apiClient.get("Dashboard/Admin")
                .done(function (res) {
                    const data = res?.data || res?.Data || {};
                    
                    const totalT = data.totalTurbines ?? data.TotalTurbines ?? 0;
                    const activeT = data.activeTurbines ?? data.ActiveTurbines ?? 0;
                    const cbInv = data.centralBankInventory ?? data.CentralBankInventory ?? 0;
                    const effCap = data.effectiveCapacity ?? data.EffectiveCapacity ?? 0;
                    const monthF = data.monthForecasts ?? data.MonthForecasts ?? 0;
                    const totalDem = data.monthTotalDemand ?? data.MonthTotalDemand ?? 0;
                    const totalBill = data.monthTotalBilled ?? data.MonthTotalBilled ?? 0;

                    setText("kpiTotalTurbines", totalT);
                    setText("kpiActiveTurbines", activeT);
                    setText("kpiCbInventory", formatNumber(cbInv) + " MWh");
                    setText("kpiEffectiveCap", formatNumber(effCap) + " MWh");
                    setText("kpiMonthForecasts", monthF);
                    setText("kpiTotalDemand", formatNumber(totalDem) + " MWh");
                    setText("kpiTotalBilled", "₡ " + formatNumber(totalBill));
                    
                    const flushDate = data.lastFlush || data.LastFlush;
                    setText("kpiLastFlush", flushDate ? new Date(flushDate).toLocaleDateString("es-CR", { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : "Sin registros");

                    renderAdminCharts(totalT, activeT, cbInv, effCap, totalDem);
                })
                .fail(function (xhr) {
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

        // Robust modal initialization
        const userModalEl = document.getElementById("userModal");
        let userModal = null;
        if (userModalEl) {
            try {
                userModal = bootstrap.Modal.getOrCreateInstance(userModalEl);
            } catch (e) {
                console.warn("Could not initialize userModal:", e);
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
                document.getElementById("userForm")?.reset();
                const modalTitle = userModalEl?.querySelector(".modal-title");
                if (modalTitle) modalTitle.innerHTML = '<i class="bi bi-person-plus-fill me-2"></i>Nuevo Usuario Interno';

                // Enable all fields for creation
                setFieldState("uId", true, false);
                setFieldState("uEmail", true, false);
                setFieldState("uPhone", true, false);
                setFieldState("uBirthDate", true, false);
                setFieldState("uPass", true, false);

                const passHint = document.getElementById("passHint");
                if (passHint) passHint.textContent = "(requerida)";

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

        function loadUsers() {
            tableBody.innerHTML = '<tr><td colspan="7" class="text-center"><span class="spinner-border spinner-border-sm" role="status"></span> Cargando usuarios...</td></tr>';
            apiClient.get("Users/RetrieveAll")
                .done(function (res) {
                    allUsers = res?.data || res?.Data || [];
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
                    (u.lastName || "").toLowerCase().includes(query) || 
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
                const fullName = `${u.firstName || ""} ${u.lastName || ""}`.trim();
                const roleBadge = getRoleBadge(u.role);
                const statusBadge = getStatusBadge(u.status);
                const isActive = (u.status || "").toLowerCase() === "active";

                let actions = `<button class="btn btn-sm btn-outline-primary me-1 btn-edit" data-id="${u.id}" title="Editar usuario"><i class="bi bi-pencil"></i> Editar</button>`;
                if (isActive) {
                    actions += `<button class="btn btn-sm btn-outline-danger btn-deactivate" data-id="${u.id}" title="Desactivar usuario"><i class="bi bi-person-x"></i> Desactivar</button>`;
                } else {
                    actions += `<button class="btn btn-sm btn-outline-success btn-reactivate" data-id="${u.id}" title="Reactivar usuario"><i class="bi bi-person-check"></i> Reactivar</button>`;
                }

                return `
                    <tr>
                        <td class="fw-bold">${escapeHtml(u.identification || "-")}</td>
                        <td>${escapeHtml(fullName || "-")}</td>
                        <td>${escapeHtml(u.email || "-")}</td>
                        <td>${u.phone || "-"}</td>
                        <td>${roleBadge}</td>
                        <td>${statusBadge}</td>
                        <td class="text-end text-nowrap">${actions}</td>
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
                btn.addEventListener("click", () => reactivateUser(btn.getAttribute("data-id")));
            });
        }

        function getRoleBadge(role) {
            if (role === "Administrator") return '<span class="badge bg-danger">Administrador</span>';
            if (role === "Engineer") return '<span class="badge bg-info text-dark">Ingeniero</span>';
            if (role === "Buyer") return '<span class="badge bg-success">Comprador</span>';
            return `<span class="badge bg-secondary">${role || "-"}</span>`;
        }

        function getStatusBadge(status) {
            if ((status || "").toLowerCase() === "active") return '<span class="badge bg-success">Activo</span>';
            if ((status || "").toLowerCase() === "inactive") return '<span class="badge bg-secondary">Inactivo</span>';
            if ((status || "").toLowerCase() === "blocked") return '<span class="badge bg-danger">Bloqueado</span>';
            if ((status || "").toLowerCase() === "pendingactivation") return '<span class="badge bg-warning text-dark">Pendiente</span>';
            return `<span class="badge bg-warning text-dark">${status || "-"}</span>`;
        }

        // Edit modal
        function openEditModal(id) {
            const u = allUsers.find(item => String(item.id) === String(id));
            if (!u) {
                notify.error("No se encontró el usuario.");
                return;
            }

            editingUserId = u.id;
            const modalTitle = userModalEl?.querySelector(".modal-title");
            if (modalTitle) modalTitle.innerHTML = '<i class="bi bi-pencil-square me-2"></i>Editar Usuario';

            // Populate fields
            const idInput = document.getElementById("uId");
            const first1Input = document.getElementById("uFirst1");
            const first2Input = document.getElementById("uFirst2");
            const last1Input = document.getElementById("uLast1");
            const last2Input = document.getElementById("uLast2");
            const emailInput = document.getElementById("uEmail");
            const phoneInput = document.getElementById("uPhone");
            const birthDateInput = document.getElementById("uBirthDate");
            const roleInput = document.getElementById("uRole");
            const passInput = document.getElementById("uPass");

            if (idInput) { idInput.value = u.identification || ""; idInput.disabled = true; }
            if (emailInput) { emailInput.value = u.email || ""; emailInput.disabled = true; }

            if (first1Input) {
                const names = (u.firstName || "").split(" ");
                first1Input.value = names[0] || "";
                first1Input.disabled = false;
                if (first2Input) { first2Input.value = names.slice(1).join(" ") || ""; first2Input.disabled = false; }
            }
            if (last1Input) {
                const lasts = (u.lastName || "").split(" ");
                last1Input.value = lasts[0] || "";
                last1Input.disabled = false;
                if (last2Input) { last2Input.value = lasts.slice(1).join(" ") || ""; last2Input.disabled = false; }
            }

            if (phoneInput) { phoneInput.value = u.phone || ""; phoneInput.disabled = false; }

            if (birthDateInput) {
                if (u.birthDate) {
                    const d = new Date(u.birthDate);
                    birthDateInput.value = d.toISOString().split('T')[0];
                }
                birthDateInput.disabled = true;  // BirthDate not editable
            }

            if (roleInput) { roleInput.value = u.role || "Engineer"; roleInput.disabled = false; }
            if (passInput) { passInput.value = ""; passInput.disabled = false; }

            const passHint = document.getElementById("passHint");
            if (passHint) passHint.textContent = "(dejar vacío para no cambiar)";

            // Show birthDate row but disabled, hide password requirement
            setFieldState("uBirthDate", true, true);
            setFieldState("uPass", true, false);

            if (userModal) {
                userModal.show();
            } else {
                // Fallback: try to create the modal instance again
                try {
                    userModal = new bootstrap.Modal(userModalEl);
                    userModal.show();
                } catch(e) {
                    notify.error("Error al abrir el formulario de edición.");
                    console.error("Modal init error:", e);
                }
            }
        }

        // Save button
        const saveBtn = document.getElementById("saveUserBtn");
        if (saveBtn) {
            saveBtn.addEventListener("click", function () {
                const idVal = document.getElementById("uId")?.value.trim();
                const f1 = document.getElementById("uFirst1")?.value.trim() || "";
                const f2 = document.getElementById("uFirst2")?.value.trim() || "";
                const l1 = document.getElementById("uLast1")?.value.trim() || "";
                const l2 = document.getElementById("uLast2")?.value.trim() || "";
                
                const firstVal = f2 ? `${f1} ${f2}` : f1;
                const lastVal = l2 ? `${l1} ${l2}` : l1;
                const emailVal = document.getElementById("uEmail")?.value.trim();
                const phoneVal = document.getElementById("uPhone")?.value.trim();
                const birthDateVal = document.getElementById("uBirthDate")?.value;
                const roleVal = document.getElementById("uRole")?.value;
                const passVal = document.getElementById("uPass")?.value;

                if (!firstVal || !lastVal) {
                    notify.warning("Por favor ingrese primer nombre y primer apellido.");
                    return;
                }

                saveBtn.disabled = true;
                saveBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Guardando...';

                if (editingUserId) {
                    // Edit existing user
                    const dto = {
                        userId: editingUserId,
                        firstName: firstVal,
                        lastName: lastVal,
                        phone: phoneVal || "88888888",
                        role: roleVal,
                        status: "Active"
                    };

                    apiClient.put("Users/Update", dto)
                        .done(function (res) {
                            notify.success("Usuario actualizado correctamente.");
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
                } else {
                    // Create new internal user
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

                    const dto = {
                        identification: idVal,
                        firstName: firstVal,
                        lastName: lastVal,
                        email: emailVal,
                        role: roleVal,
                        phone: phoneVal,
                        birthDate: birthDateVal,
                        password: passVal || "SEGEDE_Temp123!"
                    };

                    apiClient.post("Users/Internal", dto)
                        .done(function (res) {
                            notify.success("Usuario interno creado con éxito. Contraseña temporal generada.");
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
                }
            });
        }

        function deactivateUser(id) {
            notify.confirm("¿Está seguro de que desea desactivar este usuario?", { dangerous: true, confirmText: "Desactivar" }).then(function (ok) {
                if (!ok) return;
                apiClient.post("Users/Deactivate", { userId: parseInt(id), cancelForecasts: true })
                    .done(function () {
                        notify.success("Usuario desactivado correctamente.");
                        loadUsers();
                    })
                    .fail(function (xhr) {
                        handleApiError(xhr);
                    });
            });
        }

        function reactivateUser(id) {
            notify.confirm("¿Desea reactivar este usuario?", { confirmText: "Reactivar" }).then(function (ok) {
                if (!ok) return;
                apiClient.post(`Users/Reactivate/${id}`)
                    .done(function () {
                        notify.success("Usuario reactivado con éxito.");
                        loadUsers();
                    })
                    .fail(function (xhr) {
                        handleApiError(xhr);
                    });
            });
        }
    }
});
