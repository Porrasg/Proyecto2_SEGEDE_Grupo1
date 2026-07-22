// site.js (§29.2, §29.3) - Control global de navegación, estética intuitiva, roles y seguridad

// Escapa datos de usuario antes de interpolarlos en innerHTML (defensa contra XSS almacenado).
// Se expone global porque site.js se carga antes que todos los *ViewController.js (ver _Layout).
function escapeHtml(value) {
    if (value === null || value === undefined) return "";
    return String(value).replace(/[&<>"']/g, function (c) {
        return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c];
    });
}
window.escapeHtml = escapeHtml;

document.addEventListener("DOMContentLoaded", function () {
    initNavigation();
    initSignOut();
    initLandingLogin();
    checkRouteSecurity();
    initInteractiveFilterChips();
    initInteractiveCardLinks();
    highlightActiveSidebarLink();
});

function initNavigation() {
    const isLoggedIn = !session.isExpired() && session.getToken();
    const role = session.getRole();
    const email = session.getEmail() || "Usuario Autenticado";

    const navLiAdmin = document.getElementById("navLiAdmin");
    const navLiEngineer = document.getElementById("navLiEngineer");
    const navLiBuyer = document.getElementById("navLiBuyer");
    const navLiPublicLogin = document.getElementById("navLiPublicLogin");
    const navLiPublicRegister = document.getElementById("navLiPublicRegister");
    const navUserName = document.getElementById("navUserName");
    const btnSignOut = document.getElementById("btnSignOut");

    if (isLoggedIn) {
        if (btnSignOut) btnSignOut.classList.remove("d-none");
        if (navLiPublicLogin) navLiPublicLogin.classList.add("d-none");
        if (navLiPublicRegister) navLiPublicRegister.classList.add("d-none");

        if (role === "Administrator" || role === "Admin") {
            if (navLiAdmin) navLiAdmin.classList.remove("d-none");
            if (navUserName) {
                navUserName.innerHTML = `<i class="bi bi-shield-check"></i> ${email} [Admin]`;
                navUserName.className = "badge bg-danger border border-dark text-white px-3 py-2 fw-bold shadow-sm";
            }
        } else if (role === "Engineer") {
            if (navLiEngineer) navLiEngineer.classList.remove("d-none");
            if (navUserName) {
                navUserName.innerHTML = `<i class="bi bi-lightning-charge-fill"></i> ${email} [Ingeniero]`;
                navUserName.className = "badge bg-info border border-dark text-dark px-3 py-2 fw-bold shadow-sm";
            }
        } else if (role === "Buyer") {
            if (navLiBuyer) navLiBuyer.classList.remove("d-none");
            if (navUserName) {
                navUserName.innerHTML = `<i class="bi bi-building"></i> ${email} [Comprador]`;
                navUserName.className = "badge bg-success border border-dark text-white px-3 py-2 fw-bold shadow-sm";
            }
        } else {
            if (navUserName) {
                navUserName.innerHTML = `<i class="bi bi-person-fill"></i> ${email}`;
                navUserName.className = "badge bg-primary border border-dark text-white px-3 py-2 fw-bold shadow-sm";
            }
        }
    } else {
        // No logueado: mantener ocultos menús de roles y mostrar botones de login/registro
        if (navLiAdmin) navLiAdmin.classList.add("d-none");
        if (navLiEngineer) navLiEngineer.classList.add("d-none");
        if (navLiBuyer) navLiBuyer.classList.add("d-none");
        if (btnSignOut) btnSignOut.classList.add("d-none");
        if (navLiPublicLogin) navLiPublicLogin.classList.remove("d-none");
        if (navLiPublicRegister) navLiPublicRegister.classList.remove("d-none");
        if (navUserName) {
            navUserName.innerHTML = `🔒 No Autenticado`;
            navUserName.className = "badge bg-secondary border border-dark text-dark px-3 py-2 fw-bold";
        }
    }
}

function initSignOut() {
    const btnSignOut = document.getElementById("btnSignOut");
    if (!btnSignOut) return;

    btnSignOut.addEventListener("click", function (e) {
        e.preventDefault();
        session.clear();
        if (typeof notify !== "undefined") {
            notify.info("Has cerrado sesión exitosamente.");
        }
        setTimeout(() => {
            window.location.href = "/Login";
        }, 300);
    });
}

function initLandingLogin() {
    const form = document.getElementById("landingLoginForm");
    if (!form) return;

    form.addEventListener("submit", function (e) {
        e.preventDefault();
        const ident = document.getElementById("loginIdent")?.value.trim();
        const pass = document.getElementById("loginPass")?.value;

        if (!ident || !pass) {
            if (typeof notify !== "undefined") notify.warning("Por favor ingrese usuario y contraseña.");
            return;
        }

        const btn = form.querySelector("button[type='submit']");
        const originalText = btn ? btn.innerHTML : "";
        if (btn) {
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Conectando...';
        }

        apiClient.post("Users/LoginStep1", { email: ident, password: pass })
            .done(function (res) {
                sessionStorage.setItem("sgde_login_email", ident);
                if (typeof notify !== "undefined") notify.success(res?.message || res?.Message || "Código OTP enviado a su correo.");
                setTimeout(function () {
                    window.location.href = "/LoginOtp";
                }, 1000);
            })
            .fail(function (xhr) {
                if (btn) {
                    btn.disabled = false;
                    btn.innerHTML = originalText;
                }
                handleApiError(xhr);
            });
    });
}

// Destino del dashboard según el rol de la sesión activa (usado por la regla A1.1 de la adenda v3: logo/raíz redirige por rol).
function dashboardUrlForRole(role) {
    if (role === "Administrator" || role === "Admin") return "/Admin/Dashboard";
    if (role === "Engineer") return "/Engineer/Dashboard";
    if (role === "Buyer") return "/Buyer/Dashboard";
    return null;
}

function checkRouteSecurity() {
    const path = window.location.pathname.toLowerCase();
    const isLoggedIn = !session.isExpired() && session.getToken();
    const role = session.getRole();

    // Regla A1.1: la raíz ("/" o "/Index") redirige de inmediato al dashboard del rol si hay sesión válida.
    // Sin sesión, se muestra la landing normalmente (no fuerza /Login).
    if (path === "/" || path === "/index") {
        if (isLoggedIn) {
            const target = dashboardUrlForRole(role);
            if (target) window.location.href = target;
        }
        return;
    }

    // Páginas públicas que no requieren validación de rol
    if (path.startsWith("/login") || path.startsWith("/register") || path.startsWith("/recover") || path.startsWith("/reset") || path.startsWith("/activate") || path.startsWith("/accessdenied")) {
        return;
    }

    if (!isLoggedIn) {
        window.location.href = `/Login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
        return;
    }

    // Validación por Rol
    if (path.startsWith("/admin/") && role !== "Administrator" && role !== "Admin") {
        window.location.href = "/AccessDenied";
        return;
    }
    if (path.startsWith("/engineer/") && role !== "Engineer" && role !== "Administrator" && role !== "Admin") {
        window.location.href = "/AccessDenied";
        return;
    }
    if (path.startsWith("/buyer/") && role !== "Buyer" && role !== "Administrator" && role !== "Admin") {
        window.location.href = "/AccessDenied";
        return;
    }
}

// ── Interactive Filter Chips Helper ────────────────────────────────────
function initInteractiveFilterChips() {
    const chips = document.querySelectorAll(".filter-chip");
    chips.forEach(chip => {
        chip.addEventListener("click", function () {
            const container = this.closest(".filter-chips-container");
            if (container) {
                container.querySelectorAll(".filter-chip").forEach(c => c.classList.remove("active"));
            }
            this.classList.add("active");

            const targetSelector = this.getAttribute("data-target") || "#filterStatus, #opFilterStatus, #statusFilter";
            const selectEl = document.querySelector(targetSelector);
            const statusValue = this.getAttribute("data-status");

            if (selectEl) {
                selectEl.value = statusValue || "";
                selectEl.dispatchEvent(new Event("change"));
            } else {
                // If no select element, check if there is an input search or custom filter callback
                const searchInput = document.querySelector("#searchTurbine, #opSearchTurbine, #searchInput");
                if (searchInput) {
                    searchInput.dispatchEvent(new Event("input"));
                }
            }
        });
    });

    // Helper for "Limpiar Filtros" button
    const clearBtn = document.querySelector("#btnClearFilters, .btn-clear-filters");
    if (clearBtn) {
        clearBtn.addEventListener("click", function () {
            const searchInput = document.querySelector("#searchTurbine, #opSearchTurbine, #searchInput");
            if (searchInput) {
                searchInput.value = "";
                searchInput.dispatchEvent(new Event("input"));
            }
            const defaultChip = document.querySelector(".filter-chip[data-status='']");
            if (defaultChip) defaultChip.click();
        });
    }
}

// ── Interactive KPI Card Links Helper ──────────────────────────────────
function initInteractiveCardLinks() {
    const interactiveCards = document.querySelectorAll(".kpi-card[data-navigate], .card-interactive[data-navigate], [data-href]");
    interactiveCards.forEach(card => {
        card.addEventListener("click", function (e) {
            // Prevent navigating if user clicked a button or link inside the card
            if (e.target.tagName === "A" || e.target.tagName === "BUTTON" || e.target.closest("button") || e.target.closest("a")) {
                return;
            }
            const url = this.getAttribute("data-navigate") || this.getAttribute("data-href");
            if (url) {
                window.location.href = url;
            }
        });
    });
}

/**
 * ============================================================================
 * SGDE Core — Role-Based UI Access Controller (Vanilla JavaScript)
 * ============================================================================
 */
class RoleAccessController {
    constructor() {
        this.storageKey = 'userRole';
        this.defaultRole = 'guest';
    }

    getCurrentRoles() {
        const sessionRole = typeof session !== 'undefined' ? session.getRole() : null;
        const storedValue = sessionRole || sessionStorage.getItem(this.storageKey) || this.defaultRole;
        return storedValue
            .toLowerCase()
            .split(',')
            .map(r => r.trim())
            .filter(r => r.length > 0);
    }

    applyUIFiltering() {
        const activeRoles = this.getCurrentRoles();
        const restrictedElements = document.querySelectorAll('[data-roles]');

        restrictedElements.forEach(element => {
            const allowedRolesAttr = element.getAttribute('data-roles');
            if (!allowedRolesAttr) return;

            const allowedRoles = allowedRolesAttr
                .toLowerCase()
                .split(',')
                .map(r => r.trim());

            const hasAccess = allowedRoles.some(role => activeRoles.includes(role));

            if (hasAccess) {
                element.classList.remove('d-none');
            } else {
                element.classList.add('d-none');
            }
        });

        this.updateSidebarUserProfile(activeRoles);
    }

    updateSidebarUserProfile(activeRoles) {
        const badgeEl = document.getElementById('sidebarUserRoleBadge');
        const nameEl = document.getElementById('sidebarUserName');
        if (badgeEl && activeRoles.length > 0) {
            badgeEl.textContent = activeRoles[0].toUpperCase();
        }
        if (nameEl && typeof session !== 'undefined') {
            const email = session.getEmail() || sessionStorage.getItem('sgde_login_email') || 'Usuario Sesión';
            nameEl.textContent = email;
        }
    }
}

document.addEventListener('DOMContentLoaded', () => {
    const rbacController = new RoleAccessController();
    rbacController.applyUIFiltering();

    window.SGDE_RBAC = {
        refresh: () => rbacController.applyUIFiltering(),
        setRole: (newRole) => {
            sessionStorage.setItem('userRole', newRole);
            rbacController.applyUIFiltering();
        }
    };
});

function highlightActiveSidebarLink() {
    const currentPath = window.location.pathname.toLowerCase().replace(/\/$/, '') || '/';
    document.querySelectorAll('.sgde-sidebar .nav-link').forEach(link => {
        const nav = (link.getAttribute('data-nav') || link.getAttribute('href') || '').toLowerCase().replace(/\/$/, '');
        if (!nav || nav === '#') return;
        const isIndex = nav === '' || nav === '/';
        if (isIndex) {
            if (currentPath === '' || currentPath === '/') link.classList.add('active');
        } else if (currentPath === nav || currentPath.startsWith(nav + '/')) {
            link.classList.add('active');
        }
    });
}


