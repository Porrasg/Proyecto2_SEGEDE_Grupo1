// BuyerDashboardViewController.js (§22.1, §27) - Controlador JS para el Panel de Comprador y Perfil
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando BuyerDashboardViewController...");

    const token = session.getToken();
    const role = session.getRole();
    const userId = session.getUserId() || 1;

    if (!token || (role !== "Buyer" && role !== "Administrator" && role !== "Admin")) {
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
            apiClient.get("Dashboard/Buyer?buyerUserId=" + userId)
                .done(function (res) {
                    const d = res?.data || res?.Data || {};

                    const reqMWh = d.monthRequestedMWh ?? d.MonthRequestedMWh ?? 0;
                    const assignMWh = d.lastAssignment ?? d.LastAssignment ?? 0;
                    const totalBill = d.totalBilledAccumulated ?? d.TotalBilledAccumulated ?? 0;
                    const activeF = d.activeForecasts ?? d.ActiveForecasts ?? 0;

                    setText("buyActiveForecasts", activeF);
                    setText("buyMonthReq", formatNumber(reqMWh) + " MWh");
                    setText("buyLastAssign", formatNumber(assignMWh) + " MWh");
                    setText("buyTotalBilled", formatNumber(totalBill) + " CRC");

                    const dateVal = d.lastStatementDate || d.LastStatementDate;
                    setText("buyLastStmtDate", dateVal ? new Date(dateVal).toLocaleDateString("es-CR") : "Sin registros");

                    renderBuyCharts(reqMWh, assignMWh, totalBill);
                })
                .fail(function (xhr) {
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

    function loadBuyerProfile() {
        apiClient.get("Users/" + userId)
            .done(function (res) {
                const u = res?.data || res?.Data || {};
                const fullName = [u.firstName || u.FirstName, u.lastName || u.LastName].filter(Boolean).join(" ");
                setText("profName", fullName || "Comprador SGDE");
                setText("profEmail", u.email || u.Email || "-");
                setText("profId", u.identification || u.Identification || "-");

                const created = u.created || u.Created;
                setText("profDate", created ? new Date(created).toLocaleDateString("es-CR") : "-");

                const phoneInput = document.getElementById("pPhone");
                if (phoneInput) phoneInput.value = u.phone || u.Phone || "";

                const photo = u.photoUrl || u.PhotoUrl;
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

        const submitBtn = document.querySelector("#profileForm button[type='submit']");
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Guardando...';
        }

        // Users/UpdateProfile opera siempre sobre el usuario autenticado (ownership implícito en el backend).
        apiClient.put("Users/UpdateProfile", {
            phone: phone,
            photoUrl: profilePhotoDataUrl,
            newPassword: newPass || null
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

        apiClient.post("Users/Deactivate", {
            userId: parseInt(userId),
            cancelFutureForecasts: !keepForecasts
        }).done(function () {
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
