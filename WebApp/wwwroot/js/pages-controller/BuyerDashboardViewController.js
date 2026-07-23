// BuyerDashboardViewController.js (§22.1, §27) - Controlador JS para el Panel de Comprador y Perfil
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando BuyerDashboardViewController...");

    const role = session.getRole();
    const userId = session.getUserId() || 1;

    if (role !== "Distributor" && role !== "Administrator" && role !== "Admin") {
        notify.error("Acceso denegado. Requiere privilegios de Comprador.");
        setTimeout(() => {
            window.location.href = "/Login";
        }, 1500);
        return;
    }

    // ==========================================
    // 1. PANEL PRINCIPAL (/Buyer/Dashboard)
    // ==========================================
    if (document.getElementById("buyActiveForecasts")) {
        let buyDemandChartInst = null;
        let buyBillingChartInst = null;

        loadBuyerDashboard();
        setInterval(loadBuyerDashboard, 15000);

        function loadBuyerDashboard() {
            Promise.all([
                apiClient.get("Forecasts/RetrieveByBuyerId/" + userId),
                apiClient.get("Distributions/RetrieveByBuyerId/" + userId),
                apiClient.get("Invoices/RetrieveAll")
            ]).then(function (responses) {
                const forecasts = responses[0]?.[0]?.data || responses[0]?.data || responses[0]?.Data || [];
                const distributions = responses[1]?.[0]?.data || responses[1]?.data || responses[1]?.Data || [];
                const invoices = responses[2]?.[0]?.data || responses[2]?.data || responses[2]?.Data || [];

                const reqMWh = forecasts.reduce(function (sum, f) { return sum + Number(f.requestedEnergyMWh ?? f.RequestedEnergyMWh ?? 0); }, 0);
                const assignMWh = distributions.reduce(function (sum, d) { return sum + Number(d.assignedEnergyMWh ?? d.AssignedEnergyMWh ?? 0); }, 0);
                const totalBill = invoices
                    .filter(function (i) { return (i.buyerId ?? i.BuyerId) === userId; })
                    .reduce(function (sum, i) { return sum + Number(i.totalAmount ?? i.TotalAmount ?? i.amount ?? i.Amount ?? 0); }, 0);
                const activeF = forecasts.filter(function (f) { return (f.status || f.Status) === "Active"; }).length;

                setText("buyActiveForecasts", activeF);
                setText("buyMonthReq", formatNumber(reqMWh) + " MWh");
                setText("buyLastAssign", formatNumber(assignMWh) + " MWh");
                setText("buyTotalBilled", formatNumber(totalBill) + " CRC");

                const lastInvoice = invoices.filter(function (i) { return (i.buyerId ?? i.BuyerId) === userId; }).sort(function (a, b) {
                    return new Date(b.createdAt || b.CreatedAt || 0) - new Date(a.createdAt || a.CreatedAt || 0);
                })[0];
                const dateVal = lastInvoice?.createdAt || lastInvoice?.CreatedAt;
                setText("buyLastStmtDate", dateVal ? new Date(dateVal).toLocaleDateString("es-CR") : "Sin registros");

                renderBuyCharts(reqMWh, assignMWh, totalBill);
            }).catch(function (xhr) {
                handleApiError(xhr);
            });
        }

        function renderBuyCharts(reqMWh, assignMWh, totalBill) {
            if (typeof Chart === "undefined") return;

            const ctxDemand = document.getElementById("buyDemandChart")?.getContext("2d");
            if (ctxDemand) {
                if (buyDemandChartInst) {
                    buyDemandChartInst.data.datasets[0].data = [reqMWh, assignMWh];
                    buyDemandChartInst.update();
                } else {
                    buyDemandChartInst = new Chart(ctxDemand, {
                        type: "bar",
                        data: {
                            labels: ["Demanda Solicitada", "Asignación Recibida"],
                            datasets: [{
                                label: "Energía (MWh)",
                                data: [reqMWh, assignMWh],
                                backgroundColor: ["#2563EB", "#107C62"],
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

            const ctxBill = document.getElementById("buyBillingChart")?.getContext("2d");
            if (ctxBill) {
                if (buyBillingChartInst) {
                    buyBillingChartInst.data.datasets[0].data = [totalBill, Math.max(0, totalBill * 0.15)];
                    buyBillingChartInst.update();
                } else {
                    buyBillingChartInst = new Chart(ctxBill, {
                        type: "doughnut",
                        data: {
                            labels: ["Facturado Pagado", "Impuestos / Pendiente"],
                            datasets: [{
                                data: [totalBill, Math.max(0, totalBill * 0.15)],
                                backgroundColor: ["#5A2CA0", "#D97706"],
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
        }
    }

    // ==========================================
    // 2. GESTIÓN DE PERFIL (/Buyer/Profile)
    // ==========================================
    if (document.getElementById("profName")) {
        loadBuyerProfile();

        const profileForm = document.getElementById("profileForm");
        if (profileForm) {
            profileForm.addEventListener("submit", function (e) {
                e.preventDefault();
                updateProfile();
            });
        }

        const confirmDeactBtn = document.getElementById("confirmDeactBtn");
        if (confirmDeactBtn) {
            confirmDeactBtn.addEventListener("click", function () {
                deactivateAccount();
            });
        }
    }

    // Foto nueva seleccionada en el perfil (data-URL base64, redimensionada en el cliente).
    let profilePhotoDataUrl = null;
    let cachedBuyerUser = null;

    function getUserField(user, camel, pascal) {
        return user?.[camel] ?? user?.[pascal] ?? "";
    }

    function getBuyerDisplayName(user) {
        return [getUserField(user, "firstName", "FirstName"), getUserField(user, "firstLastName", "FirstLastName"), getUserField(user, "secondLastName", "SecondLastName")].filter(Boolean).join(" ");
    }

    function loadBuyerProfile() {
        apiClient.get("Users/RetrieveById/" + userId)
            .done(function (res) {
                cachedBuyerUser = res?.data || res?.Data || {};
                const fullName = getBuyerDisplayName(cachedBuyerUser);
                setText("profName", fullName || "Comprador SGDE");
                setText("profEmail", getUserField(cachedBuyerUser, "email", "Email") || "-");
                setText("profId", getUserField(cachedBuyerUser, "identification", "Identification") || "-");

                const created = getUserField(cachedBuyerUser, "createdAt", "CreatedAt");
                setText("profDate", created ? new Date(created).toLocaleDateString("es-CR") : "-");

                const phoneInput = document.getElementById("pPhone");
                if (phoneInput) phoneInput.value = getUserField(cachedBuyerUser, "phone", "Phone") || getUserField(cachedBuyerUser, "phoneNumber", "PhoneNumber") || "";

                const photo = getUserField(cachedBuyerUser, "photoUrl", "PhotoUrl") || getUserField(cachedBuyerUser, "profilePhoto", "ProfilePhoto");
                const imgEl = document.getElementById("profilePhoto");
                if (photo && imgEl) imgEl.src = photo;
            })
            .fail(function (xhr) {
                handleApiError(xhr);
            });
    }

    // Redimensiona la foto elegida (máx. 256px) y muestra preview inmediato en el avatar.
    const pPhotoInput = document.getElementById("pPhoto");
    if (pPhotoInput) {
        pPhotoInput.addEventListener("change", function () {
            const file = this.files?.[0];
            if (!file) { profilePhotoDataUrl = null; return; }
            const img = new Image();
            img.onload = function () {
                const MAX = 256;
                const scale = Math.min(1, MAX / Math.max(img.width, img.height));
                const canvas = document.createElement("canvas");
                canvas.width = Math.round(img.width * scale);
                canvas.height = Math.round(img.height * scale);
                canvas.getContext("2d").drawImage(img, 0, 0, canvas.width, canvas.height);
                profilePhotoDataUrl = canvas.toDataURL("image/jpeg", 0.82);
                URL.revokeObjectURL(img.src);
                const imgEl = document.getElementById("profilePhoto");
                if (imgEl) imgEl.src = profilePhotoDataUrl;
            };
            img.onerror = function () {
                URL.revokeObjectURL(img.src);
                notify.error("No se pudo leer la imagen seleccionada.");
            };
            img.src = URL.createObjectURL(file);
        });
    }

    function updateProfile() {
        const phone = document.getElementById("pPhone")?.value.trim();
        const newPass = document.getElementById("pNewPass")?.value;
        const currentPass = document.getElementById("pCurrPass")?.value || "";
        const currentUser = cachedBuyerUser || {};

        const submitBtn = document.querySelector("#profileForm button[type='submit']");
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Guardando...';
        }

        apiClient.put("Users/Update", {
            id: parseInt(getUserField(currentUser, "id", "Id") || userId),
            identification: getUserField(currentUser, "identification", "Identification"),
            firstName: getUserField(currentUser, "firstName", "FirstName"),
            firstLastName: getUserField(currentUser, "firstLastName", "FirstLastName"),
            secondLastName: getUserField(currentUser, "secondLastName", "SecondLastName") || null,
            email: getUserField(currentUser, "email", "Email"),
            phoneNumber: phone || "",
            birthDate: getUserField(currentUser, "birthDate", "BirthDate") || null,
            password: newPass || currentPass || getUserField(currentUser, "password", "Password") || "",
            profilePhoto: profilePhotoDataUrl || getUserField(currentUser, "profilePhoto", "ProfilePhoto") || getUserField(currentUser, "photoUrl", "PhotoUrl") || null,
            role: getUserField(currentUser, "role", "Role") || session.getRole() || "Distributor",
            status: getUserField(currentUser, "status", "Status") || "Active"
        }).done(function () {
            notify.success("Datos de perfil y seguridad actualizados.");
            if (document.getElementById("pCurrPass")) document.getElementById("pCurrPass").value = "";
            if (document.getElementById("pNewPass")) document.getElementById("pNewPass").value = "";
        }).fail(function (xhr) {
            handleApiError(xhr);
        }).always(function () {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.textContent = "Guardar Cambios";
            }
        });
    }

    function deactivateAccount() {
        const keepForecasts = document.getElementById("keepForecasts")?.checked || false;
        const btn = document.getElementById("confirmDeactBtn");
        if (btn) {
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Desactivando...';
        }

        apiClient.delete("Users/Delete", { id: parseInt(userId) }).done(function () {
            notify.info("Tu cuenta ha sido desactivada. Cerrando sesión...");
            setTimeout(() => {
                session.clear();
                window.location.href = "/Login";
            }, 2000);
        }).fail(function (xhr) {
            handleApiError(xhr);
            if (btn) {
                btn.disabled = false;
                btn.textContent = "Confirmar Desactivación";
            }
        });
    }

    function setText(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = value;
    }

    function formatNumber(num) {
        return Number(num).toLocaleString("es-CR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }
});
