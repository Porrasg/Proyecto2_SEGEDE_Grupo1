// BillingConfigViewController.js (§85 Admin/Prices, Admin/Taxes) - Configuración de precio por MWh e impuestos vigentes
document.addEventListener("DOMContentLoaded", function () {
    // ==========================================
    // 1. PRECIOS (/Admin/Prices)
    // ==========================================
    const priceForm = document.getElementById("priceForm");
    if (priceForm) {
        loadPriceHistory();

        priceForm.addEventListener("submit", function (e) {
            e.preventDefault();
            const value = parseFloat(document.getElementById("priceValue")?.value || 0);
            if (value <= 0) {
                notify.warning("El precio debe ser mayor a cero.");
                return;
            }
            const btn = priceForm.querySelector("button[type='submit']");
            const original = btn ? btn.innerHTML : "";
            if (btn) { btn.disabled = true; btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Guardando...'; }

            apiClient.post("Billing/SetPrice", { priceCRCPerMWh: value })
                .done(function () {
                    notify.success("Nuevo precio registrado. El anterior se cerró automáticamente.");
                    priceForm.reset();
                    loadPriceHistory();
                })
                .fail(function (xhr) { handleApiError(xhr); })
                .always(function () { if (btn) { btn.disabled = false; btn.innerHTML = original; } });
        });
    }

    function loadPriceHistory() {
        const body = document.getElementById("pricesHistoryBody");
        if (!body) return;
        body.innerHTML = '<tr><td colspan="4" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando precios...</td></tr>';
        apiClient.get("Billing/PriceHistory")
            .done(function (res) {
                const items = (res?.data || res?.Data || []).slice().sort(function (a, b) {
                    return new Date(b.validFrom || b.ValidFrom) - new Date(a.validFrom || a.ValidFrom);
                });
                if (!items.length) {
                    body.innerHTML = '<tr><td colspan="4" class="text-center text-muted">Sin precios registrados.</td></tr>';
                    return;
                }
                body.innerHTML = items.map(function (p) {
                    const active = p.isActive ?? p.IsActive;
                    return `<tr>
                        <td>${Number(p.priceCRCPerMWh ?? p.PriceCRCPerMWh ?? 0).toLocaleString("es-CR", { minimumFractionDigits: 2 })}</td>
                        <td>${new Date(p.validFrom || p.ValidFrom).toLocaleDateString("es-CR")}</td>
                        <td>${(p.validTo || p.ValidTo) ? new Date(p.validTo || p.ValidTo).toLocaleDateString("es-CR") : "-"}</td>
                        <td>${active ? '<span class="badge bg-success">Vigente</span>' : '<span class="badge bg-secondary">Cerrado</span>'}</td>
                    </tr>`;
                }).join("");
            })
            .fail(function (xhr) {
                body.innerHTML = '<tr><td colspan="4" class="text-center text-danger">Error al cargar el historial de precios.</td></tr>';
                handleApiError(xhr);
            });
    }

    // ==========================================
    // 2. IMPUESTOS (/Admin/Taxes)
    // ==========================================
    const taxForm = document.getElementById("taxForm");
    if (taxForm) {
        loadTaxHistory();

        taxForm.addEventListener("submit", function (e) {
            e.preventDefault();
            const name = document.getElementById("taxName")?.value.trim();
            const pct = parseFloat(document.getElementById("taxValue")?.value || 0);
            if (!name) {
                notify.warning("El nombre del impuesto es obligatorio.");
                return;
            }
            if (pct < 0 || pct >= 100) {
                notify.warning("El porcentaje debe estar entre 0 y 100.");
                return;
            }
            const btn = taxForm.querySelector("button[type='submit']");
            const original = btn ? btn.innerHTML : "";
            if (btn) { btn.disabled = true; btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Guardando...'; }

            apiClient.post("Billing/SetTax", { name: name, percentage: pct / 100 })
                .done(function () {
                    notify.success("Nuevo porcentaje de impuesto registrado.");
                    taxForm.reset();
                    loadTaxHistory();
                })
                .fail(function (xhr) { handleApiError(xhr); })
                .always(function () { if (btn) { btn.disabled = false; btn.innerHTML = original; } });
        });
    }

    function loadTaxHistory() {
        const body = document.getElementById("taxesHistoryBody");
        if (!body) return;
        body.innerHTML = '<tr><td colspan="4" class="text-center"><span class="spinner-border spinner-border-sm"></span> Cargando impuestos...</td></tr>';
        apiClient.get("Billing/TaxHistory")
            .done(function (res) {
                const items = (res?.data || res?.Data || []).slice().sort(function (a, b) {
                    return new Date(b.validFrom || b.ValidFrom) - new Date(a.validFrom || a.ValidFrom);
                });
                if (!items.length) {
                    body.innerHTML = '<tr><td colspan="4" class="text-center text-muted">Sin impuestos registrados.</td></tr>';
                    return;
                }
                body.innerHTML = items.map(function (t) {
                    const active = t.isActive ?? t.IsActive;
                    const pct = Number(t.percentage ?? t.Percentage ?? 0) * 100;
                    return `<tr>
                        <td>${pct.toLocaleString("es-CR", { minimumFractionDigits: 2 })}%</td>
                        <td>${new Date(t.validFrom || t.ValidFrom).toLocaleDateString("es-CR")}</td>
                        <td>${(t.validTo || t.ValidTo) ? new Date(t.validTo || t.ValidTo).toLocaleDateString("es-CR") : "-"}</td>
                        <td>${active ? '<span class="badge bg-success">Vigente</span>' : '<span class="badge bg-secondary">Cerrado</span>'}</td>
                    </tr>`;
                }).join("");
            })
            .fail(function (xhr) {
                body.innerHTML = '<tr><td colspan="4" class="text-center text-danger">Error al cargar el historial de impuestos.</td></tr>';
                handleApiError(xhr);
            });
    }
});
