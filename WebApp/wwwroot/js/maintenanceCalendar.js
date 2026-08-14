// Calendario mensual reutilizable para Admin/Maintenances y Engineer/Maintenances.
(function () {
    "use strict";

    const TYPE_LABELS = {
        Preventive: "Preventivo",
        Corrective: "Correctivo",
        Predictive: "Predictivo",
        Inspection: "Inspección",
        Emergency: "Emergencia"
    };
    const STATUS_LABELS = {
        Scheduled: "Programado",
        InProgress: "En progreso",
        Completed: "Completado",
        Cancelled: "Cancelado"
    };
    const MONTH_FORMATTER = new Intl.DateTimeFormat("es-CR", { month: "long", year: "numeric" });
    const DATE_FORMATTER = new Intl.DateTimeFormat("es-CR", { dateStyle: "full", timeStyle: "short" });

    let root;
    let title;
    let grid;
    let detail;
    let currentMonth = startOfMonth(new Date());
    let maintenances = [];
    let turbineLabels = {};
    let dayClickHandler = null; 
    let selectedDay = null;

    function startOfMonth(date) {
        return new Date(date.getFullYear(), date.getMonth(), 1);
    }

    function read(item, camel, pascal) {
        return item?.[camel] ?? item?.[pascal];
    }

    function validDate(value) {
        const date = value ? new Date(value) : null;
        return date && !Number.isNaN(date.getTime()) ? date : null;
    }

    function sameDay(a, b) {
        return a.getFullYear() === b.getFullYear() &&
            a.getMonth() === b.getMonth() &&
            a.getDate() === b.getDate();
    }

    function dateKey(date) {
        return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;
    }

    function eventsForDay(day) {
        return maintenances.filter(function (item) {
            const start = validDate(read(item, "estimatedStartDate", "EstimatedStartDate"));
            const end = validDate(read(item, "estimatedEndDate", "EstimatedEndDate")) || start;
            if (!start || !end) return false;

            const dayStart = new Date(day.getFullYear(), day.getMonth(), day.getDate());
            const dayEnd = new Date(dayStart);
            dayEnd.setDate(dayEnd.getDate() + 1);
            return start < dayEnd && end >= dayStart;
        }).sort(function (a, b) {
            return validDate(read(a, "estimatedStartDate", "EstimatedStartDate")) -
                validDate(read(b, "estimatedStartDate", "EstimatedStartDate"));
        });
    }

    function makeEvent(item) {
        const button = document.createElement("button");
        const turbineId = read(item, "turbineId", "TurbineId");
        const type = read(item, "maintenanceType", "MaintenanceType") || "Preventive";
        const status = read(item, "status", "Status") || "Scheduled";
        const start = validDate(read(item, "estimatedStartDate", "EstimatedStartDate"));

        button.type = "button";
        button.className = `maintenance-calendar-event is-${status.toLowerCase()}`;
        button.title = `${turbineLabels[turbineId] || `Turbina #${turbineId}`} — ${TYPE_LABELS[type] || type}`;
        button.innerHTML = `<span class="maintenance-calendar-event-time">${start ? start.toLocaleTimeString("es-CR", { hour: "2-digit", minute: "2-digit" }) : "—"}</span> <span>${escapeText(turbineLabels[turbineId] || `#${turbineId}`)}</span>`;
        button.addEventListener("click", function () { showDetail(item); });
        return button;
    }

    function escapeText(value) {
        return String(value ?? "").replace(/[&<>"']/g, function (character) {
            return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[character];
        });
    }

    function showDetail(item) {
        if (!detail) return;
        const turbineId = read(item, "turbineId", "TurbineId");
        const type = read(item, "maintenanceType", "MaintenanceType") || "—";
        const status = read(item, "status", "Status") || "—";
        const start = validDate(read(item, "estimatedStartDate", "EstimatedStartDate"));
        const end = validDate(read(item, "estimatedEndDate", "EstimatedEndDate"));
        const description = read(item, "description", "Description") || "Sin descripción";
        const result = read(item, "result", "Result");

        detail.classList.remove("d-none");
        detail.innerHTML = `
            <div class="d-flex justify-content-between align-items-start gap-3">
                <div>
                    <div class="fw-semibold">${escapeText(turbineLabels[turbineId] || `Turbina #${turbineId}`)} · ${escapeText(TYPE_LABELS[type] || type)}</div>
                    <div class="small text-muted">${escapeText(start ? DATE_FORMATTER.format(start) : "Sin inicio")} — ${escapeText(end ? DATE_FORMATTER.format(end) : "Sin fin")}</div>
                    <div class="small mt-1">${escapeText(description)}</div>
                    ${result ? `<div class="small mt-1"><strong>Resultado:</strong> ${escapeText(result)}</div>` : ""}
                </div>
                <span class="badge bg-secondary">${escapeText(STATUS_LABELS[status] || status)}</span>
            </div>`;
    }

    function render() {
        if (!grid || !title) return;
        title.textContent = MONTH_FORMATTER.format(currentMonth).replace(/^./, c => c.toUpperCase());
        grid.innerHTML = "";

        const firstVisible = new Date(currentMonth);
        firstVisible.setDate(1 - firstVisible.getDay());
        const today = new Date();

        for (let index = 0; index < 42; index++) {
            const day = new Date(firstVisible);
            day.setDate(firstVisible.getDate() + index);

            const cell = document.createElement("section");
            cell.className = "maintenance-calendar-day";
            cell.dataset.date = dateKey(day);

            cell.addEventListener("click", function ()
            {
                //prueba

                //Guardar el dia seleccionado
                selectedDay = new Date(day);

                //Recargar de nuevo el calendario para mostrar el dia seleccionado
                render();
                    
                if (dayClickHandler) {
                    dayClickHandler(new Date(day))
                }
                else {

                    //prueba
                }


            });

            // Dias fuera del mes actual
            if (day.getMonth() !== currentMonth.getMonth()) {
                cell.classList.add("is-outside");
            }
            //Si existe un dia seleccionado, unicamente se resalta ese dia.
            if (selectedDay) {
                if (sameDay(day, selectedDay)) {
                    cell.classList.add("is-selected");
                }
            }
            // Si no se ha seleccionado el dia, se marca el dia actual
            else if (sameDay(day, today)) {
                cell.classList.add("is-today");
            }

            const heading = document.createElement("div");
            heading.className = "maintenance-calendar-day-number";
            heading.textContent = String(day.getDate());
            cell.appendChild(heading);

            const events = eventsForDay(day);
            events.slice(0, 3).forEach(item => cell.appendChild(makeEvent(item)));
            if (events.length > 3) {
                const more = document.createElement("div");
                more.className = "maintenance-calendar-more";
                more.textContent = `+${events.length - 3} más`;
                cell.appendChild(more);
            }
            grid.appendChild(cell);
        }
    }

    function initialize() {
        root = document.getElementById("maintenanceCalendar");
        if (!root) return;
        title = document.getElementById("maintenanceCalendarTitle");
        grid = document.getElementById("maintenanceCalendarGrid");
        detail = document.getElementById("maintenanceCalendarDetail");

        document.getElementById("maintenanceCalendarPrevious")?.addEventListener("click", function () {
            currentMonth = new Date(currentMonth.getFullYear(), currentMonth.getMonth() - 1, 1);
            render();
        });
        document.getElementById("maintenanceCalendarNext")?.addEventListener("click", function () {
            currentMonth = new Date(currentMonth.getFullYear(), currentMonth.getMonth() + 1, 1);
            render();
        });
        document.getElementById("maintenanceCalendarToday")?.addEventListener("click", function () {
            currentMonth = startOfMonth(new Date());
            render();
        });
        render();
    }

    document.addEventListener("DOMContentLoaded", initialize);

    window.maintenanceCalendar = {
        setData: function (items, labels) {
            maintenances = Array.isArray(items) ? items : [];
            turbineLabels = labels || {};
            render();
        },

        onDayClick: function (callback) {
            dayClickHandler = callback;
        },

        refresh: render
    };
})();
