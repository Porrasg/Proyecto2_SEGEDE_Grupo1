/*==============================================================================
    MODULO: Bitacora de Movimientos del Banco Central (CentralBankMovement)

    Descripcion:
        Historial de movimientos de energia del banco central: entradas
        (RECEIVE, desde vaciados de bateria), salidas (DISTRIBUTE, hacia
        compradores) y vaciados directos (FLUSH). Cada registro deja
        constancia del inventario resultante y de la perdida por saturacion,
        si la hubo. Usado por CentralBankMovementManager/CrudFactory
        (CoreApp/DataAccess) y CentralBankManager.ReceiveEnergy/DistributeEnergy
        y FlushManager.ExecuteFlush, que registran un movimiento en cada
        operacion.

    Nota: script aditivo, no modifica ninguna tabla ni SP existente. Si la
    tabla tblCentralBankMovements ya fue creada manualmente en Azure con una
    estructura distinta a la de aqui, este script NO la va a alterar (el
    CREATE TABLE esta protegido con IF NOT EXISTS) — en ese caso hay que
    revisar a mano que las columnas coincidan con lo que esperan los SPs.
==============================================================================*/

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tblCentralBankMovements' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.tblCentralBankMovements
    (
        MovementId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        CentralBankId INT NOT NULL,
        MovementType NVARCHAR(20) NOT NULL, -- RECEIVE | DISTRIBUTE | FLUSH
        EnergyMWh DECIMAL(18,4) NOT NULL,
        InventoryAfterMovement DECIMAL(18,4) NOT NULL,
        SaturationLossMWh DECIMAL(18,4) NOT NULL,
        Description NVARCHAR(250) NOT NULL,
        CreatedAt DATETIME NOT NULL,
        UpdatedAt DATETIME NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_tblCentralBankMovements_tblCentralBank')
BEGIN
    ALTER TABLE dbo.tblCentralBankMovements WITH CHECK
    ADD CONSTRAINT FK_tblCentralBankMovements_tblCentralBank
    FOREIGN KEY (CentralBankId)
    REFERENCES dbo.tblCentralBank (CentralBankId);
END
GO


/*==============================================================================
    PROCEDIMIENTO: CRE_CENTRALBANKMOVEMENT_PR
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.CRE_CENTRALBANKMOVEMENT_PR
(
    @CentralBankId INT,
    @MovementType NVARCHAR(20),
    @EnergyMWh DECIMAL(18,4),
    @InventoryAfterMovement DECIMAL(18,4),
    @SaturationLossMWh DECIMAL(18,4),
    @Description NVARCHAR(250),
    @CreatedAt DATETIME,
    @UpdatedAt DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.tblCentralBankMovements
        (CentralBankId, MovementType, EnergyMWh, InventoryAfterMovement, SaturationLossMWh, Description, CreatedAt, UpdatedAt)
    VALUES
        (@CentralBankId, @MovementType, @EnergyMWh, @InventoryAfterMovement, @SaturationLossMWh, @Description, @CreatedAt, @UpdatedAt);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS MovementId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: UPD_CENTRALBANKMOVEMENT_PR
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_CENTRALBANKMOVEMENT_PR
(
    @MovementId INT,
    @CentralBankId INT,
    @MovementType NVARCHAR(20),
    @EnergyMWh DECIMAL(18,4),
    @InventoryAfterMovement DECIMAL(18,4),
    @SaturationLossMWh DECIMAL(18,4),
    @Description NVARCHAR(250),
    @UpdatedAt DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblCentralBankMovements
    SET
        CentralBankId = @CentralBankId,
        MovementType = @MovementType,
        EnergyMWh = @EnergyMWh,
        InventoryAfterMovement = @InventoryAfterMovement,
        SaturationLossMWh = @SaturationLossMWh,
        Description = @Description,
        UpdatedAt = @UpdatedAt
    WHERE MovementId = @MovementId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: DEL_CENTRALBANKMOVEMENT_PR
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.DEL_CENTRALBANKMOVEMENT_PR
(
    @MovementId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.tblCentralBankMovements
    WHERE MovementId = @MovementId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ALL_CENTRALBANKMOVEMENT_PR
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ALL_CENTRALBANKMOVEMENT_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MovementId,
        CentralBankId,
        MovementType,
        EnergyMWh,
        InventoryAfterMovement,
        SaturationLossMWh,
        Description,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblCentralBankMovements
    ORDER BY CreatedAt DESC, MovementId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ID_CENTRALBANKMOVEMENT_PR
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ID_CENTRALBANKMOVEMENT_PR
(
    @MovementId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MovementId,
        CentralBankId,
        MovementType,
        EnergyMWh,
        InventoryAfterMovement,
        SaturationLossMWh,
        Description,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblCentralBankMovements
    WHERE MovementId = @MovementId;
END;
GO
