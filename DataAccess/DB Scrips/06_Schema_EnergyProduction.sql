/*==============================================================================
    MODULO: Operacion y Cortes para Almacenamiento - Produccion Neta (Admin/EnergyStorage)

    Descripcion:
        Historial de produccion neta calculada por turbina (produccion bruta
        segun capacidad nominal, menos horas de mantenimiento en el periodo).
        Cierra la brecha de "Operacion y Cortes" de la rubrica y provee los
        datos que WebApp/wwwroot/js/pages-controller/ReportsViewController.js
        ya espera consumir de Energy/GenerationHistory/{turbineId}
        (campos eventDate / generatedEnergy).

    Nota: script aditivo, no modifica ninguna tabla ni SP existente.
==============================================================================*/

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tblEnergyProduction' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.tblEnergyProduction
    (
        EnergyProductionId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        TurbineId INT NOT NULL,
        PeriodStart DATETIME NOT NULL,
        EventDate DATETIME NOT NULL, -- fecha del corte (fin del periodo calculado)
        GrossEnergyMWh DECIMAL(18,4) NOT NULL,
        MaintenanceLossMWh DECIMAL(18,4) NOT NULL,
        GeneratedEnergy DECIMAL(18,4) NOT NULL, -- producción neta = bruta - pérdida por mantenimiento/inactividad
        CreatedAt DATETIME NOT NULL
    );
END
GO


/*==============================================================================
    PROCEDIMIENTO: CRE_ENERGY_PRODUCTION_PR
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.CRE_ENERGY_PRODUCTION_PR
(
    @TurbineId INT,
    @PeriodStart DATETIME,
    @EventDate DATETIME,
    @GrossEnergyMWh DECIMAL(18,4),
    @MaintenanceLossMWh DECIMAL(18,4),
    @GeneratedEnergy DECIMAL(18,4),
    @CreatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.tblEnergyProduction
        (TurbineId, PeriodStart, EventDate, GrossEnergyMWh, MaintenanceLossMWh, GeneratedEnergy, CreatedAt)
    VALUES
        (@TurbineId, @PeriodStart, @EventDate, @GrossEnergyMWh, @MaintenanceLossMWh, @GeneratedEnergy, @CreatedAt);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS EnergyProductionId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_TURBINE_ENERGY_PRODUCTION_PR

    Descripcion:
        Historial de produccion de una turbina, mas reciente primero.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_TURBINE_ENERGY_PRODUCTION_PR
(
    @TurbineId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        EnergyProductionId,
        TurbineId,
        PeriodStart,
        EventDate,
        GrossEnergyMWh,
        MaintenanceLossMWh,
        GeneratedEnergy,
        CreatedAt
    FROM dbo.tblEnergyProduction
    WHERE TurbineId = @TurbineId
    ORDER BY EventDate DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_LAST_ENERGY_PRODUCTION_PR

    Descripcion:
        Ultimo corte de produccion registrado para una turbina (para saber
        desde donde calcular el siguiente periodo). NULL si nunca se ha
        calculado produccion para esa turbina.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_LAST_ENERGY_PRODUCTION_PR
(
    @TurbineId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        EnergyProductionId,
        TurbineId,
        PeriodStart,
        EventDate,
        GrossEnergyMWh,
        MaintenanceLossMWh,
        GeneratedEnergy,
        CreatedAt
    FROM dbo.tblEnergyProduction
    WHERE TurbineId = @TurbineId
    ORDER BY EventDate DESC;
END;
GO
