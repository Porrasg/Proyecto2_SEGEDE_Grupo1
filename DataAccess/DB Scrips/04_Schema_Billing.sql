/*==============================================================================
    MODULO: Facturacion / Configuracion de Precios e Impuestos (Admin)

    Descripcion:
        Tablas y procedimientos para la pantalla Admin/Prices y Admin/Taxes.
        Cierra la brecha "definicion maestra de precios por Megavatio" de la
        rubrica (Administracion del Sistema). El frontend (BillingConfigViewController.js)
        ya llama a Billing/SetPrice, Billing/PriceHistory, Billing/SetTax,
        Billing/TaxHistory; este script agrega el respaldo de base de datos
        que faltaba.

    Nota: script aditivo, no modifica ninguna tabla ni SP existente. Seguro
    de ejecutar sobre la base de datos compartida ya poblada.
==============================================================================*/

/*==============================================================================
    TABLA: tblPrices  (Historial de precios de energia por MWh)

    Descripcion:
        Guarda el historial de precios vigentes por Megavatio-hora. Solo un
        registro puede estar activo (IsActive = 1, ValidTo NULL) a la vez;
        al registrar un precio nuevo, el vigente anterior se cierra.
==============================================================================*/

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tblPrices' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.tblPrices
    (
        PriceId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        PriceCRCPerMWh DECIMAL(18,2) NOT NULL,
        ValidFrom DATETIME NOT NULL,
        ValidTo DATETIME NULL,
        IsActive BIT NOT NULL,
        CreatedAt DATETIME NOT NULL
    );
END
GO

/*==============================================================================
    TABLA: tblTaxes  (Historial de porcentajes de impuesto aplicados)

    Descripcion:
        Igual patron que tblPrices pero para el porcentaje de impuesto
        (Percentage se guarda como fraccion, ej. 0.13 para 13%).
==============================================================================*/

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tblTaxes' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.tblTaxes
    (
        TaxId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        Percentage DECIMAL(9,6) NOT NULL,
        ValidFrom DATETIME NOT NULL,
        ValidTo DATETIME NULL,
        IsActive BIT NOT NULL,
        CreatedAt DATETIME NOT NULL
    );
END
GO


/*==============================================================================
    PROCEDIMIENTO: CRE_PRICE_PR

    Descripcion:
        Registra un nuevo precio vigente por MWh. Cierra automaticamente
        (ValidTo = GETDATE(), IsActive = 0) cualquier precio que siguiera
        activo, dentro de la misma transaccion.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.CRE_PRICE_PR
(
    @PriceCRCPerMWh DECIMAL(18,2)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;

    UPDATE dbo.tblPrices
    SET ValidTo = GETDATE(), IsActive = 0
    WHERE IsActive = 1;

    INSERT INTO dbo.tblPrices (PriceCRCPerMWh, ValidFrom, ValidTo, IsActive, CreatedAt)
    VALUES (@PriceCRCPerMWh, GETDATE(), NULL, 1, GETDATE());

    COMMIT TRANSACTION;

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS PriceId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ALL_PRICE_PR

    Descripcion:
        Obtiene el historial completo de precios, mas reciente primero.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ALL_PRICE_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        PriceId,
        PriceCRCPerMWh,
        ValidFrom,
        ValidTo,
        IsActive,
        CreatedAt
    FROM dbo.tblPrices
    ORDER BY ValidFrom DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ACTIVE_PRICE_PR

    Descripcion:
        Obtiene el precio actualmente vigente (si existe).
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ACTIVE_PRICE_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        PriceId,
        PriceCRCPerMWh,
        ValidFrom,
        ValidTo,
        IsActive,
        CreatedAt
    FROM dbo.tblPrices
    WHERE IsActive = 1
    ORDER BY ValidFrom DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: CRE_TAX_PR

    Descripcion:
        Registra un nuevo porcentaje de impuesto vigente. Cierra
        automaticamente cualquier impuesto que siguiera activo.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.CRE_TAX_PR
(
    @Name NVARCHAR(100),
    @Percentage DECIMAL(9,6)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;

    UPDATE dbo.tblTaxes
    SET ValidTo = GETDATE(), IsActive = 0
    WHERE IsActive = 1;

    INSERT INTO dbo.tblTaxes (Name, Percentage, ValidFrom, ValidTo, IsActive, CreatedAt)
    VALUES (@Name, @Percentage, GETDATE(), NULL, 1, GETDATE());

    COMMIT TRANSACTION;

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS TaxId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ALL_TAX_PR

    Descripcion:
        Obtiene el historial completo de impuestos, mas reciente primero.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ALL_TAX_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        TaxId,
        Name,
        Percentage,
        ValidFrom,
        ValidTo,
        IsActive,
        CreatedAt
    FROM dbo.tblTaxes
    ORDER BY ValidFrom DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ACTIVE_TAX_PR

    Descripcion:
        Obtiene el impuesto actualmente vigente (si existe).
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ACTIVE_TAX_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        TaxId,
        Name,
        Percentage,
        ValidFrom,
        ValidTo,
        IsActive,
        CreatedAt
    FROM dbo.tblTaxes
    WHERE IsActive = 1
    ORDER BY ValidFrom DESC;
END;
GO
