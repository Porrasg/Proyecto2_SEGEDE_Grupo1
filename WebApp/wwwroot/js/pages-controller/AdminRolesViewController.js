// AdminRolesViewController.js - Matriz de Roles y Permisos (Admin/Roles)
document.addEventListener("DOMContentLoaded", function () {
    const body = document.getElementById("rolesMatrixBody");
    if (!body) return;

    const userId = session.getUserId() || 1;
    const roles = ["Administrator", "Engineer", "Distributor"];

    loadMatrix();

    function loadMatrix() {
        apiClient.get("Roles/Permissions")
            .done(function (res) {
                const items = res?.data || res?.Data || [];
                renderMatrix(items);
            })
            .fail(function (xhr) {
                body.innerHTML = '<tr><td colspan="4" class="text-center text-danger">Error al cargar la matriz de permisos.</td></tr>';
                handleApiError(xhr);
            });
    }

    function renderMatrix(items) {
        // Agrupar por módulo (mismo ModuleKey/ModuleLabel para los 3 roles)
        const modules = {};
        items.forEach(function (p) {
            const key = p.moduleKey ?? p.ModuleKey;
            if (!modules[key]) {
                modules[key] = { label: p.moduleLabel ?? p.ModuleLabel, byRole: {} };
            }
            modules[key].byRole[p.roleName ?? p.RoleName] = !!(p.canAccess ?? p.CanAccess);
        });

        const moduleKeys = Object.keys(modules);
        if (!moduleKeys.length) {
            body.innerHTML = '<tr><td colspan="4" class="text-center text-muted">Sin módulos configurados.</td></tr>';
            return;
        }

        body.innerHTML = moduleKeys.map(function (key) {
            const m = modules[key];
            const cells = roles.map(function (role) {
                const checked = m.byRole[role] ? "checked" : "";
                return `<td class="text-center">
                    <input type="checkbox" class="form-check-input role-perm-checkbox" ${checked}
                           data-module-key="${key}" data-module-label="${escapeHtml(m.label)}" data-role="${role}">
                </td>`;
            }).join("");
            return `<tr><td>${escapeHtml(m.label)}</td>${cells}</tr>`;
        }).join("");

        body.querySelectorAll(".role-perm-checkbox").forEach(function (cb) {
            cb.addEventListener("change", function () {
                const moduleKey = this.getAttribute("data-module-key");
                const moduleLabel = this.getAttribute("data-module-label");
                const role = this.getAttribute("data-role");
                const canAccess = this.checked;

                this.disabled = true;
                apiClient.put("Roles/SetPermission?callerUserId=" + userId, {
                    roleName: role,
                    moduleKey: moduleKey,
                    moduleLabel: moduleLabel,
                    canAccess: canAccess
                })
                    .done(function () {
                        notify.success("Permiso actualizado.");
                    })
                    .fail((xhr) => {
                        handleApiError(xhr);
                        cb.checked = !canAccess; // revertir visualmente si falló
                    })
                    .always(() => { cb.disabled = false; });
            });
        });
    }
});
