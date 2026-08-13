(function () {

    // Banderas de control para evitar que peticiones simultáneas saturen el renderizado
    let bateriasCargadas = false;
    let bancoCargado = false;
    let historialCargado = false;

    // Forzar formateo estricto de 4 decimales requeridos
    const formatDecimal = v => v != null && !isNaN(v) ? Number(v).toFixed(4) : "0.0000";

    // 1. CARGAR DATOS DEL BANCO CENTRAL
    async function loadCentralBank() {
        if (bancoCargado) return;
        bancoCargado = true;

        try {

            const res = await apiClient.get('CentralBanks/RetrieveAll');

            // PRUEBAS
            console.log("CentralBank res =", res);
            console.log("CentralBank data =", res.data);
            console.log("Es arreglo:", Array.isArray(res));

            const dataList = Array.isArray(res)
                ? res
                : (res.data?.items || res.data?.Data || res.data || []);

            console.log("dataList =", dataList);

            if (dataList.length === 0) {
                return;
            }

            const data = Array.isArray(dataList)
                ? dataList[0]
                : dataList;

            const current = data.currentInventoryMWh ?? data.CurrentInventoryMWh ?? 0;
            const capacity = data.maximumCapacityMWh ?? data.MaximumCapacityMWh ?? 0;

            $('#cbInventory').text(formatDecimal(current) + ' MWh');

            const percent = capacity > 0
                ? Math.min(100, (current / capacity) * 100)
                : 0;

            $('#cbProgress')
                .css('width', percent + '%')
                .text(Math.round(percent) + '%');

            $('#cbCapacityText')
                .text(formatDecimal(current) + ' / ' + formatDecimal(capacity) + ' MWh');

        } catch (e) {
            console.error("Error loadCentralBank:", e);
        } finally {
            bancoCargado = false;
        }
    }

    // 2. CARGAR PANEL DE MONITOREO DE BATERÍAS
    async function loadBatteries() {
        if (bateriasCargadas) return; // CORTAFUEGOS: Si ya se está ejecutando, frena la duplicidad masiva
        bateriasCargadas = true;
        try {
            const bRes = await apiClient.get('Batteries/RetrieveAllBatteries');

            //PRUEBAS DE ERROR
            console.log("bRes =", bRes);
            console.log("typeof =", typeof bRes);
            console.log("Array =", Array.isArray(bRes));
            console.log("bRes.data =", bRes.data);

            //const batteries = bRes.data?.items || bRes.data?.Data || bRes.data || [];
            const batteries = Array.isArray(bRes)
                ? bRes
                : (bRes?.data?.items || bRes?.data?.Data || bRes?.data || []);

            const cards = [];

            //PRUEBAS DE ERROR
            console.log("batteries =", batteries);
            console.log("cantidad =", batteries.length);

            for (let i = 0; i < batteries.length; i++) { 

                //PRUEBAS DE ERROR
                console.log("Entró al for", batteries[i]);

                const b = batteries[i];

                const idBateria = b.id ?? b.Id;
                const idTurbina = b.turbineId ?? b.TurbineId;
                const energiaAlmacenada = b.currentEnergyMWh ?? b.CurrentEnergyMWh ?? 0;
                const estado = b.status ?? b.Status ?? "Active";
                const badgeColor = estado === "Active" ? "bg-success-subtle text-success" : "bg-secondary-subtle text-secondary";

                cards.push(`<div class="card p-3 shadow-sm border" style="width: 14rem;">
                <div class="d-flex align-items-center gap-2">
                    <div class="sgde-avatar rounded-circle bg-success text-white d-flex align-items-center justify-content-center" style="width:40px;height:40px;">
                        <i class="bi bi-lightning-charge-fill fs-5" aria-hidden="true"></i>
                    </div>
                    <div>
                        <div class="fw-semibold text-truncate" style="max-width: 8rem;">Batería #${idBateria}</div>
                        <div class="text-muted fs-xs">Turbina #${idTurbina}</div>
                    </div>
                </div>
                <div class="mt-3">
                    <div class="text-muted fs-xs">Energía Almacenada</div>
                    <div class="fs-6 font-monospace fw-bold text-success">${formatDecimal(energiaAlmacenada)} MWh</div>
                    <span class="badge mt-1 ${badgeColor}">${estado}</span>
                </div>
            </div>`);
            }

            $('#batteriesGrid').html(cards.length > 0 ? cards.join('') : '<p class="text-muted fs-sm p-2">No hay baterías registradas actualmente.</p>');
        } catch (e) {
            console.error('Error loadBatteries:', e);
        } finally {
            bateriasCargadas = false; // Libera el bloqueo al terminar
        }
    }

    // 3. CARGAR TABLA HISTÓRICA DE VACIADOS (VERSIÓN BLINDADA DE PROPIEDADES)
    async function loadHistory() {
        if (historialCargado) return; // CORTAFUEGOS
        historialCargado = true;
        try {
            const res = await apiClient.get('Flushs/RetrieveAll');

            //PRUEBAS DE ERROR
            console.log("FLUSH res =", res);
            console.log("FLUSH typeof =", typeof res);
            console.log("FLUSH Array =", Array.isArray(res));
            console.log("FLUSH res.data =", res.data);


           
            // El listado de flushes puede venir directo en un arreglo o anidado en .items
            //const rows = res.data?.items || res.data?.Data || res.data || [];

            const rows = Array.isArray(res)
                ? res
                : (res.data?.items || res.data?.Data || res.data || []);

            console.log("FLUSH rows =", rows);
            console.log("FLUSH cantidad =", rows.length);


            const trs = rows.map(r => {
                // Sincronización flexible para soportar minúsculas y mayúsculas de tu API real
                const id = r.id ?? r.Id ?? 'N/D';
                const executionType = r.executionType ?? r.ExecutionType ?? 'Manual';
                const status = r.status ?? r.Status ?? 'Completed';
                const executedAt = r.executedAt ?? r.ExecutedAt;
                const transferredEnergy = r.transferredEnergyMWh ?? r.TransferredEnergyMWh ?? 0;
                const saturationLoss = r.saturationLossMWh ?? r.SaturationLossMWh ?? 0;

                const badgeColor = status === 'Completed' ? 'bg-success' : 'bg-danger';

                return `<tr>
                <td>${id}</td>
                <td><span class="badge bg-light text-dark border">${executionType}</span></td>
                <td><span class="badge ${badgeColor}">${status}</span></td>
                <td>${executedAt ? new Date(executedAt).toLocaleString() : 'N/D'}</td>
                <td class="font-monospace text-end">${formatDecimal(transferredEnergy)}</td>
                <td class="font-monospace text-end text-danger">${formatDecimal(saturationLoss)}</td>
            </tr>`;
            }).join('');

            $('#flushHistoryTable tbody').html(trs);

            if ($.fn.DataTable) {
                $('#flushHistoryTable').DataTable({ destroy: true, order: [[0, 'desc']] });
            }
        } catch (e) {
            console.error('Error loadHistory:', e);
        } finally {
            historialCargado = false; // Libera el bloqueo al terminar
        }
    }


    // 4. EJECUTAR VACIADO MANUAL DE LAS BATERÍAS HACIA EL BANCO CENTRAL
    async function doFlush() {

        try {

            // Mostrar una ventana de confirmación antes de ejecutar el vaciado.
            // Esto evita que el usuario realice accidentalmente una operación
            // que modificará la energía almacenada en las baterías.
            const result = await Swal.fire({
                title: 'Confirmar Vaciado Manual',
                text: '¿Desea ejecutar un vaciado manual ahora? La energía almacenada en las baterías activas será transferida al Banco Central. Si se supera su capacidad máxima, el excedente se registrará como pérdida por saturación.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Sí, Ejecutar Corte',
                cancelButtonText: 'Cancelar',
                confirmButtonColor: '#dc3545'
            });


            // Si el usuario cancela la operación, se detiene inmediatamente
            // la ejecución de la función y no se llama a la API.
            if (!result.isConfirmed) {
                return;
            }


            // Obtener el identificador del usuario que tiene la sesión activa.
            // Este identificador será enviado a la API para registrar quién
            // ejecutó el vaciado manual en la bitácora de auditoría.
            const callerId = window.session?.getUserId();


            // Endpoint encargado de ejecutar el vaciado manual.
            // Corresponde al método ExecuteManual del FlushsController.
            let endpoint = 'Flushs/ExecuteManual';


            // Si existe un usuario autenticado, agregar su identificador
            // como parámetro de consulta (query parameter).
            //
            // Ejemplo:
            // Flushs/ExecuteManual?callerUserId=5
            if (callerId != null) {
                endpoint += '?callerUserId=' + encodeURIComponent(callerId);
            }


            // Realizar la petición POST a la Web API.
            //
            // A partir de aquí el flujo del backend será:
            //
            // FlushsController.ExecuteManual()
            //          ↓
            // FlushManager.Create()
            //          ↓
            // FlushCrudFactory.Create()
            //          ↓
            // CRE_FLUSH_PR
            //
            // El Stored Procedure realiza finalmente la transferencia
            // de energía desde las baterías hacia el Banco Central.
            const res = await apiClient.post(endpoint);


            // Obtener el mensaje enviado por la API.
            //
            // Se contemplan diferentes estructuras de respuesta para mantener
            // compatibilidad con la forma en que apiClient devuelve los datos.
            // Si no viene ningún mensaje, se utiliza uno predeterminado.
            const msg =
                res?.message ||
                res?.data?.message ||
                'Vaciado ejecutado con éxito.';


            // Informar al usuario que el proceso terminó correctamente.
            await Swal.fire(
                'Proceso Completado',
                msg,
                'success'
            );


            // Actualizar nuevamente toda la información mostrada en pantalla.
            //
            // Esto permite visualizar inmediatamente:
            //  - El nuevo inventario del Banco Central.
            //  - Las baterías después del vaciado.
            //  - El nuevo registro en el historial de Flushes.
            await refreshAll();

        } catch (e) {

            // Registrar el error completo en la consola del navegador.
            // Esto es útil para depuración durante el desarrollo.
            console.error('Error doFlush:', e);


            // Mensaje predeterminado en caso de que la API no proporcione
            // información específica sobre el error.
            let errorMsg = 'Error al procesar el corte.';


            // Algunos clientes basados en jQuery devuelven el error
            // del servidor dentro de responseText.
            if (e.responseText) {

                try {

                    // Intentar convertir la respuesta del servidor de texto
                    // JSON a un objeto JavaScript.
                    const parsedError = JSON.parse(e.responseText);

                    // Obtener el mensaje enviado por el backend.
                    errorMsg =
                        parsedError.message ||
                        parsedError.Message ||
                        errorMsg;

                } catch {

                    // Si responseText no contiene JSON válido,
                    // mostrar directamente el texto recibido.
                    errorMsg = e.responseText;
                }

            }

            // En otras respuestas jQuery el error puede venir
            // directamente dentro de responseJSON.
            else if (e.responseJSON) {

                errorMsg =
                    e.responseJSON.message ||
                    e.responseJSON.Message ||
                    errorMsg;

            }

            // Compatibilidad con respuestas de clientes HTTP como Axios,
            // donde normalmente el backend se encuentra en response.data.
            else if (e.response?.data) {

                errorMsg =
                    e.response.data.message ||
                    e.response.data.Message ||
                    errorMsg;
            }


            // Mostrar al usuario el mensaje de error correspondiente.
            Swal.fire(
                'Error',
                errorMsg,
                'error'
            );
        }
    }


    // 5. CALCULAR PRODUCCIÓN NETA DEL PERIODO PARA TODAS LAS TURBINAS ACTIVAS
    // Flujo: EnergyController.ExecutePeriodProduction() -> EnergyManager (calcula
    // producción bruta según capacidad nominal menos horas de mantenimiento del
    // periodo desde el último corte) -> BatteryManager.StoreEnergy (acredita la
    // producción neta a la batería de cada turbina).
    async function doExecuteProduction() {
        try {
            const result = await Swal.fire({
                title: 'Calcular Producción del Periodo',
                text: 'Se calculará la producción neta de cada turbina activa desde su último corte y se acreditará a su batería. ¿Continuar?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Sí, Calcular',
                cancelButtonText: 'Cancelar'
            });

            if (!result.isConfirmed) {
                return;
            }

            const callerId = window.session?.getUserId();
            let endpoint = 'EnergyProduction/ExecutePeriodProduction';
            if (callerId != null) {
                endpoint += '?callerUserId=' + encodeURIComponent(callerId);
            }

            const res = await apiClient.post(endpoint);

            await Swal.fire('Producción Calculada', res?.message || 'Producción registrada con éxito.', 'success');
            await refreshAll();
        } catch (e) {
            console.error('Error doExecuteProduction:', e);
            const msg = e?.responseJSON?.message || e?.responseJSON?.Message || 'No se pudo calcular la producción del periodo.';
            Swal.fire('Error', msg, 'error');
        }
    }

       async function loadProductionHistory() {
    try {
        const res = await apiClient.get('EnergyProduction/RetrieveAll');

        const rows = Array.isArray(res)
            ? res
            : (res?.data?.items || res?.data?.Data || res?.data || []);

        const trs = rows.map(r => {

            const turbineId = r.turbineId ?? r.TurbineId ?? 'N/D';

            const periodStart =
                r.periodStart ?? r.PeriodStart;

            const eventDate =
                r.eventDate ?? r.EventDate;

            const gross =
                r.grossEnergyMWh ?? r.GrossEnergyMWh ?? 0;

            const loss =
                r.maintenanceLossMWh ?? r.MaintenanceLossMWh ?? 0;

            const generated =
                r.generatedEnergy ?? r.GeneratedEnergy ?? 0;

            return `<tr>
                <td>${turbineId}</td>

                <td>
                    ${periodStart
                        ? new Date(periodStart).toLocaleString('es-CR')
                        : 'N/D'}
                </td>

                <td>
                    ${eventDate
                        ? new Date(eventDate).toLocaleString('es-CR')
                        : 'N/D'}
                </td>

                <td class="font-monospace text-end">
                    ${formatDecimal(gross)}
                </td>

                <td class="font-monospace text-end">
                    ${formatDecimal(loss)}
                </td>

                <td class="font-monospace text-end text-success fw-bold">
                    ${formatDecimal(generated)}
                </td>
            </tr>`;
        }).join('');

        $('#energyProductionTable tbody').html(
            trs ||
            '<tr><td colspan="6" class="text-center text-muted">No hay registros de producción.</td></tr>'
        );

        if ($.fn.DataTable) {
            $('#energyProductionTable').DataTable({
                destroy: true,
                order: [[2, 'desc']]
            });
        }

    } catch (e) {
        console.error('Error loadProductionHistory:', e);

        $('#energyProductionTable tbody').html(
            '<tr><td colspan="6" class="text-center text-danger">Error al cargar el historial de producción.</td></tr>'
        );
    }
    }

    // 6. CARGAR CAPTURAS TÉCNICAS DE LAS BATERÍAS
    async function loadBatterySnapshots() {

        try {

            const res = await apiClient.get('BatterySnapshots/RetrieveAll');

            console.log("BatterySnapshots res =", res);

            const rows = Array.isArray(res)
                ? res
                : (res?.data?.items ||
                    res?.data?.Data ||
                    res?.data ||
                    []);

            console.log("BatterySnapshots rows =", rows);
            console.log("BatterySnapshots cantidad =", rows.length);

            const trs = rows.map(r => {

                const flushId =
                    r.flushId ??
                    r.FlushId ??
                    'N/D';

                const batteryId =
                    r.batteryId ??
                    r.BatteryId ??
                    'N/D';

                const turbineId =
                    r.turbineId ??
                    r.TurbineId ??
                    'N/D';

                const maximumCapacity =
                    r.maximumCapacityMWh ??
                    r.MaximumCapacityMWh ??
                    0;

                const currentEnergy =
                    r.currentEnergyMWh ??
                    r.CurrentEnergyMWh ??
                    0;

                const totalGenerated =
                    r.totalGeneratedMWh ??
                    r.TotalGeneratedMWh ??
                    0;

                const totalTransferred =
                    r.totalTransferredMWh ??
                    r.TotalTransferredMWh ??
                    0;

                const totalSaturationLoss =
                    r.totalSaturationLossMWh ??
                    r.TotalSaturationLossMWh ??
                    0;

                const status =
                    r.status ??
                    r.Status ??
                    'N/D';

                const capturedAt =
                    r.capturedAt ??
                    r.CapturedAt;

                const badgeColor =
                    status === 'Active'
                        ? 'bg-success'
                        : 'bg-secondary';

                return `<tr>
                <td>${flushId}</td>

                <td>${batteryId}</td>

                <td>${turbineId}</td>

                <td class="font-monospace text-end">
                    ${formatDecimal(maximumCapacity)}
                </td>

                <td class="font-monospace text-end fw-bold">
                    ${formatDecimal(currentEnergy)}
                </td>

                <td class="font-monospace text-end">
                    ${formatDecimal(totalGenerated)}
                </td>

                <td class="font-monospace text-end">
                    ${formatDecimal(totalTransferred)}
                </td>

                <td class="font-monospace text-end text-danger">
                    ${formatDecimal(totalSaturationLoss)}
                </td>

                <td>
                    <span class="badge ${badgeColor}">
                        ${status}
                    </span>
                </td>

                <td>
                    ${capturedAt
                        ? new Date(capturedAt).toLocaleString('es-CR')
                        : 'N/D'}
                </td>
            </tr>`;
            }).join('');

            $('#batterySnapshotsTable tbody').html(
                trs ||
                '<tr><td colspan="10" class="text-center text-muted">No hay capturas técnicas registradas.</td></tr>'
            );

            if ($.fn.DataTable) {

                $('#batterySnapshotsTable').DataTable({
                    destroy: true,
                    order: [[9, 'desc']]
                });

            }

        } catch (e) {

            console.error('Error loadBatterySnapshots:', e);

            $('#batterySnapshotsTable tbody').html(
                '<tr><td colspan="10" class="text-center text-danger">Error al cargar las capturas técnicas.</td></tr>'
            );
        }
    }

    // CORRECCIÓN: Forzamos la ejecución secuencial estricta para evitar bloqueos
       async function refreshAll() {
        await loadCentralBank();
        await loadBatteries();
        await loadHistory();
        await loadProductionHistory();
        await loadBatterySnapshots();
    }


    $(document).ready(function () {
        // Ejecución lineal única y segura al cargar el DOM
        refreshAll();

        $('#btnFlush, #btnFlushSmall').off('click').on('click', function (e) {
            e.preventDefault();
            doFlush();
        });

        $('#btnExecuteProduction').off('click').on('click', function (e) {
            e.preventDefault();
            doExecuteProduction();
        });
    });
})();
