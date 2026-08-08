USE Proyecto2_SEGEDE_Grupo1;
GO

/*==============================================================================
    STORED PROCEDURES: tblUsers (Usuarios)
==============================================================================*/


/*==============================================================================
    PROCEDIMIENTO: CRE_USER_PR

    Descripción:
        Registra un nuevo usuario y retorna su identificador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.CRE_USER_PR
(
    @Identification NVARCHAR(20),
    @FirstName NVARCHAR(100),
    @FirstLastName NVARCHAR(100),
    @SecondLastName NVARCHAR(100) = NULL,
    @BirthDate DATE,
    @PhoneNumber NVARCHAR(20),
    @Email NVARCHAR(150),
    @ProfilePhoto NVARCHAR(MAX) = NULL,
    @Password NVARCHAR(255),
    @Age INT,
    @Role NVARCHAR(50),
    @Status NVARCHAR(50),
    @FailedLoginAttempts INT,
    @LockoutEndAt DATETIME = NULL,
    @LastLoginAt DATETIME = NULL,
    @CreatedAt DATETIME,
    @UpdatedAt DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.tblUsers
    (
        Identification,
        FirstName,
        FirstLastName,
        SecondLastName,
        BirthDate,
        PhoneNumber,
        Email,
        ProfilePhoto,
        Password,
        Age,
        Role,
        Status,
        FailedLoginAttempts,
        LockoutEndAt,
        LastLoginAt,
        CreatedAt,
        UpdatedAt
    )
    VALUES
    (
        @Identification,
        @FirstName,
        @FirstLastName,
        @SecondLastName,
        @BirthDate,
        @PhoneNumber,
        @Email,
        @ProfilePhoto,
        @Password,
        @Age,
        @Role,
        @Status,
        @FailedLoginAttempts,
        @LockoutEndAt,
        @LastLoginAt,
        @CreatedAt,
        @UpdatedAt
    );
END;
GO

/*==============================================================================
    PROCEDIMIENTO: UPD_USER_PR

    Descripción:
        Actualiza la información de un usuario.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_USER_PR
(
    @UserId INT,
    @Identification NVARCHAR(20),
    @FirstName NVARCHAR(100),
    @FirstLastName NVARCHAR(100),
    @SecondLastName NVARCHAR(100) = NULL,
    @BirthDate DATE,
    @PhoneNumber NVARCHAR(20),
    @Email NVARCHAR(150),
    @ProfilePhoto NVARCHAR(MAX) = NULL,
    @Password NVARCHAR(255),
    @Age INT,
    @Role NVARCHAR(50),
    @Status NVARCHAR(50),
    @FailedLoginAttempts INT,
    @LockoutEndAt DATETIME = NULL,
    @LastLoginAt DATETIME = NULL,
    @UpdatedAt DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblUsers
    SET
        Identification = @Identification,
        FirstName = @FirstName,
        FirstLastName = @FirstLastName,
        SecondLastName = @SecondLastName,
        BirthDate = @BirthDate,
        PhoneNumber = @PhoneNumber,
        Email = @Email,
        ProfilePhoto = @ProfilePhoto,
        Password = @Password,
        Age = @Age,
        Role = @Role,
        Status = @Status,
        FailedLoginAttempts = @FailedLoginAttempts,
        LockoutEndAt = @LockoutEndAt,
        LastLoginAt = @LastLoginAt,
        UpdatedAt = @UpdatedAt
    WHERE UserId = @UserId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: DEL_USER_PR

    Descripción:
        Realiza la eliminación lógica de un usuario.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.DEL_USER_PR
(
    @UserId INT,
    @Status NVARCHAR(50),
    @UpdatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblUsers
    SET
        Status = @Status,
        UpdatedAt = @UpdatedAt
    WHERE UserId = @UserId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ID_USER_PR

    Descripción:
        Obtiene un usuario mediante su identificador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ID_USER_PR
(
    @UserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UserId,
        Identification,
        FirstName,
        FirstLastName,
        SecondLastName,
        BirthDate,
        PhoneNumber,
        Email,
        ProfilePhoto,
        Password,
        Age,
        Role,
        Status,
        FailedLoginAttempts,
        LockoutEndAt,
        LastLoginAt,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblUsers
    WHERE UserId = @UserId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ALL_USER_PR

    Descripción:
        Obtiene todos los usuarios registrados.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ALL_USER_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UserId,
        Identification,
        FirstName,
        FirstLastName,
        SecondLastName,
        BirthDate,
        PhoneNumber,
        Email,
        ProfilePhoto,
        Password,
        Age,
        Role,
        Status,
        FailedLoginAttempts,
        LockoutEndAt,
        LastLoginAt,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblUsers
    ORDER BY UserId DESC;
END;
GO

/*==============================================================================
    PROCEDIMIENTO: RET_BY_EMAIL_USER_PR

    Descripción:
        Obtiene la información de un usuario mediante su correo electrónico.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_EMAIL_USER_PR
(
    @Email NVARCHAR(150)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UserId,
        Identification,
        FirstName,
        FirstLastName,
        SecondLastName,
        BirthDate,
        PhoneNumber,
        Email,
        ProfilePhoto,
        Password,
        Age,
        Role,
        Status,
        FailedLoginAttempts,
        LockoutEndAt,
        LastLoginAt,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblUsers
    WHERE Email = @Email;
END;
GO

/*==============================================================================
    PROCEDIMIENTO: RET_BY_IDENTIFICATION_USER_PR

    Descripción:
        Obtiene un usuario mediante su identificación.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_IDENTIFICATION_USER_PR
(
    @Identification NVARCHAR(20)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UserId,
        Identification,
        FirstName,
        FirstLastName,
        SecondLastName,
        BirthDate,
        PhoneNumber,
        Email,
        ProfilePhoto,
        Password,
        Age,
        Role,
        Status,
        FailedLoginAttempts,
        LockoutEndAt,
        LastLoginAt,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblUsers
    WHERE Identification = @Identification;
END;
GO

/*==============================================================================
    PROCEDIMIENTO: UPD_LOGIN_ATTEMPTS_USER_PR

    Descripción:
        Actualiza la cantidad de intentos fallidos de inicio de sesión y la
        fecha de finalización del bloqueo de un usuario.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_LOGIN_ATTEMPTS_USER_PR
(
    @UserId INT,
    @FailedLoginAttempts INT,
    @LockoutEndAt DATETIME = NULL,
    @UpdatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblUsers
    SET
        FailedLoginAttempts = @FailedLoginAttempts,
        LockoutEndAt = @LockoutEndAt,
        UpdatedAt = @UpdatedAt
    WHERE UserId = @UserId;
END;
GO

/*==============================================================================
    PROCEDIMIENTO: UPD_LAST_LOGIN_USER_PR

    Descripción:
        Registra la fecha del último inicio de sesión exitoso de un usuario.
        También permite reiniciar los intentos fallidos y eliminar el bloqueo
        activo.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_LAST_LOGIN_USER_PR
(
    @UserId INT,
    @LastLoginAt DATETIME,
    @FailedLoginAttempts INT,
    @LockoutEndAt DATETIME = NULL,
    @UpdatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblUsers
    SET
        LastLoginAt = @LastLoginAt,
        FailedLoginAttempts = @FailedLoginAttempts,
        LockoutEndAt = @LockoutEndAt,
        UpdatedAt = @UpdatedAt
    WHERE UserId = @UserId;
END;
GO

/*==============================================================================
    PROCEDIMIENTO: UPD_PASSWORD_USER_PR

    Descripción:
        Actualiza la contraseña cifrada de un usuario.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_PASSWORD_USER_PR
(
    @UserId INT,
    @Password NVARCHAR(255),
    @UpdatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblUsers
    SET
        Password = @Password,
        UpdatedAt = @UpdatedAt
    WHERE UserId = @UserId;
END;
GO

/*==============================================================================
    STORED PROCEDURES: OtpTokens
==============================================================================*/


/*==============================================================================
    PROCEDIMIENTO: CRE_OTP_TOKEN_PR

    Descripción:
        Registra un nuevo código de verificación OTP asociado al correo
        electrónico de un usuario.

        El código se crea inicialmente como no utilizado y conserva la fecha
        de expiración calculada por la aplicación.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.CRE_OTP_TOKEN_PR
(
    @P_EMAIL VARCHAR(150),
    @P_TOKEN_CODE VARCHAR(6),
    @P_PURPOSE VARCHAR(30),
    @P_EXPIRATION_DATE DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.OtpTokens
    (
        Email,
        TokenCode,
        Purpose,
        ExpirationDate,
        IsUsed,
        Created,
        Updated
    )
    VALUES
    (
        @P_EMAIL,
        @P_TOKEN_CODE,
        @P_PURPOSE,
        @P_EXPIRATION_DATE,
        0,
        GETDATE(),
        GETDATE()
    );
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_VALID_OTP_TOKEN_PR

    Descripción:
        Obtiene un código OTP válido mediante el correo electrónico y el código
        recibido.

        Solamente retorna tokens que no han sido utilizados y cuya fecha de
        expiración todavía no ha finalizado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_VALID_OTP_TOKEN_PR
(
    @P_EMAIL VARCHAR(150),
    @P_TOKEN_CODE VARCHAR(6),
    @P_PURPOSE VARCHAR(30)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        Id,
        Created,
        Updated,
        Email,
        TokenCode,
        Purpose,
        ExpirationDate,
        IsUsed
    FROM dbo.OtpTokens
    WHERE Email = @P_EMAIL
      AND TokenCode = @P_TOKEN_CODE
      AND Purpose = @P_PURPOSE
      AND IsUsed = 0
    ORDER BY Id DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: ACTIVATE_USER_ACCOUNT_PR

    Descripción:
        Activa la cuenta de un usuario. La validación del OTP se realiza en la
        capa de negocio para permitir reintentos hasta la expiración.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.ACTIVATE_USER_ACCOUNT_PR
(
    @Email NVARCHAR(150),
    @TokenCode VARCHAR(6)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.OtpTokens
            WHERE Email = @Email
              AND TokenCode = @TokenCode
              AND Purpose = 'ACCOUNT_ACTIVATION'
              AND IsUsed = 0
        )
        BEGIN
            RAISERROR('El código OTP es inválido, ya fue utilizado o ha expirado.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END;

        UPDATE dbo.tblUsers
        SET
            Status = 'Active',
            UpdatedAt = GETDATE()
        WHERE Email = @Email
          AND Status = 'Pending';

        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('No existe un usuario registrado con el correo indicado o la cuenta no está pendiente de activación.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END;

        UPDATE dbo.OtpTokens
        SET
            IsUsed = 1,
            Updated = GETDATE()
        WHERE Email = @Email
          AND TokenCode = @TokenCode
          AND Purpose = 'ACCOUNT_ACTIVATION'
          AND IsUsed = 0;

        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('No se pudo marcar el OTP como utilizado.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO

/*==============================================================================
	PROCEDIMIENTO: ACTIVATE_USER_STATUS_PR

	Descripción:
		Activa únicamente el estado de la cuenta de usuario.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.ACTIVATE_USER_STATUS_PR
(
	@Email NVARCHAR(150)
)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.tblUsers
	SET
		Status = 'Active',
		UpdatedAt = GETDATE()
	WHERE Email = @Email;

	IF @@ROWCOUNT = 0
	BEGIN
		RAISERROR('No existe un usuario registrado con el correo indicado.', 16, 1);
		RETURN;
	END;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_LAST_OTP_TOKEN_BY_EMAIL_PR

    Descripción:
        Obtiene el último código OTP generado para un correo electrónico.

        Puede utilizarse para verificar el estado, la fecha de expiración o si
        el código ya fue consumido.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_LAST_OTP_TOKEN_BY_EMAIL_PR
(
    @Email VARCHAR(150)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        Id,
        Created,
        Updated,
        Email,
        TokenCode,
        ExpirationDate,
        IsUsed
    FROM dbo.OtpTokens
    WHERE Email = @Email
    ORDER BY Id DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: UPD_OTP_TOKEN_AS_USED_PR

    Descripción:
        Marca un código OTP como utilizado para impedir que pueda volver a
        emplearse.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_USED_OTP_TOKEN_PR
(
    @P_OTP_ID INT,
    @P_USED_DATE DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.OtpTokens
    SET
        IsUsed = 1,
        Updated = ISNULL(@P_USED_DATE, GETDATE())
    WHERE Id = @P_OTP_ID
      AND IsUsed = 0;
END;
GO

/*==============================================================================
    PROCEDIMIENTO: UPD_INVALIDATE_OTP_TOKEN_PR

    Descripción:
        Invalidar códigos anteriores
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_INVALIDATE_OTP_TOKEN_PR
(
    @P_EMAIL VARCHAR(150),
    @P_PURPOSE VARCHAR(30)
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.OtpTokens
    SET
        IsUsed = 1,
        Updated = GETDATE()
    WHERE Email = @P_EMAIL
      AND Purpose = @P_PURPOSE
      AND IsUsed = 0;
END;
GO


/*==============================================================================
    STORED PROCEDURES: tblTurbines
==============================================================================*/


/*==============================================================================
    PROCEDIMIENTO: CRE_TURBINE_PR

    Descripción:
        Registra una nueva turbina en el sistema SGDE y retorna el
        identificador generado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.CRE_TURBINE_PR
(
    @Code NVARCHAR(50),
    @Name NVARCHAR(100),
    @Location NVARCHAR(250),
    @Brand NVARCHAR(100),
    @Model NVARCHAR(100),
    @ManufactureYear INT,
    @NominalWeeklyCapacityMWh DECIMAL(18,4),
    @Status NVARCHAR(50),
    @CreatedAt DATETIME,
    @UpdatedAt DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.tblTurbines
    (
        Code,
        Name,
        Location,
        Brand,
        Model,
        ManufactureYear,
        NominalWeeklyCapacityMWh,
        Status,
        CreatedAt,
        UpdatedAt
    )
    VALUES
    (
        @Code,
        @Name,
        @Location,
        @Brand,
        @Model,
        @ManufactureYear,
        @NominalWeeklyCapacityMWh,
        @Status,
        @CreatedAt,
        @UpdatedAt
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS TurbineId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: UPD_TURBINE_PR

    Descripción:
        Actualiza la información general y técnica de una turbina registrada.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_TURBINE_PR
(
    @TurbineId INT,
    @Code NVARCHAR(50),
    @Name NVARCHAR(100),
    @Location NVARCHAR(250),
    @Brand NVARCHAR(100),
    @Model NVARCHAR(100),
    @ManufactureYear INT,
    @NominalWeeklyCapacityMWh DECIMAL(18,4),
    @Status NVARCHAR(50),
    @UpdatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblTurbines
    SET
        Code = @Code,
        Name = @Name,
        Location = @Location,
        Brand = @Brand,
        Model = @Model,
        ManufactureYear = @ManufactureYear,
        NominalWeeklyCapacityMWh = @NominalWeeklyCapacityMWh,
        Status = @Status,
        UpdatedAt = @UpdatedAt
    WHERE TurbineId = @TurbineId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: DEL_TURBINE_PR

    Descripción:
        Realiza la eliminación lógica de una turbina.
        La turbina no se elimina físicamente para conservar su historial de
        baterías, mantenimientos, fallas y movimientos energéticos.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.DEL_TURBINE_PR
(
    @TurbineId INT,
    @Status NVARCHAR(50),
    @UpdatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblTurbines
    SET
        Status = @Status,
        UpdatedAt = @UpdatedAt
    WHERE TurbineId = @TurbineId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ID_TURBINE_PR

    Descripción:
        Obtiene la información de una turbina mediante su identificador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ID_TURBINE_PR
(
    @TurbineId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        TurbineId,
        Code,
        Name,
        Location,
        Brand,
        Model,
        ManufactureYear,
        NominalWeeklyCapacityMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblTurbines
    WHERE TurbineId = @TurbineId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ALL_TURBINE_PR

    Descripción:
        Obtiene todas las turbinas registradas en el sistema SGDE.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ALL_TURBINE_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        TurbineId,
        Code,
        Name,
        Location,
        Brand,
        Model,
        ManufactureYear,
        NominalWeeklyCapacityMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblTurbines
    ORDER BY TurbineId DESC;
END;
GO

/*==============================================================================
    PROCEDIMIENTO: RET_BY_CODE_TURBINE_PR

    Descripción:
        Obtiene la información de una turbina mediante su código.
        Puede utilizarse para localizar una turbina o validar que el código
        no esté registrado previamente.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_CODE_TURBINE_PR
(
    @Code NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        TurbineId,
        Code,
        Name,
        Location,
        Brand,
        Model,
        ManufactureYear,
        NominalWeeklyCapacityMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblTurbines
    WHERE Code = @Code;
END;
GO

/*==============================================================================
    PROCEDIMIENTO: RET_BY_STATUS_TURBINE_PR

    Descripción:
        Obtiene las turbinas que coincidan con el estado indicado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_STATUS_TURBINE_PR
(
    @Status NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        TurbineId,
        Code,
        Name,
        Location,
        Brand,
        Model,
        ManufactureYear,
        NominalWeeklyCapacityMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblTurbines
    WHERE Status = @Status
    ORDER BY TurbineId DESC;
END;
GO

/*==============================================================================
    STORED PROCEDURES: tblMaintenances
==============================================================================*/


/*==============================================================================
    PROCEDIMIENTO: CRE_MAINTENANCE_PR

    Descripción:
        Registra un nuevo mantenimiento en el sistema SGDE y retorna el
        identificador generado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.CRE_MAINTENANCE_PR
(
    @TurbineId INT,
    @EngineerId INT,
    @MaintenanceType NVARCHAR(100),
    @Description NVARCHAR(1000),
    @EstimatedStartDate DATETIME,
    @EstimatedEndDate DATETIME,
    @ActualStartDate DATETIME = NULL,
    @ActualEndDate DATETIME = NULL,
    @Result NVARCHAR(1000) = NULL,
    @Status NVARCHAR(50),
    @CreatedAt DATETIME,
    @UpdatedAt DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.tblMaintenances
    (
        TurbineId,
        EngineerId,
        MaintenanceType,
        Description,
        EstimatedStartDate,
        EstimatedEndDate,
        ActualStartDate,
        ActualEndDate,
        Result,
        Status,
        CreatedAt,
        UpdatedAt
    )
    VALUES
    (
        @TurbineId,
        @EngineerId,
        @MaintenanceType,
        @Description,
        @EstimatedStartDate,
        @EstimatedEndDate,
        @ActualStartDate,
        @ActualEndDate,
        @Result,
        @Status,
        @CreatedAt,
        @UpdatedAt
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS MaintenanceId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: UPD_MAINTENANCE_PR

    Descripción:
        Actualiza la información de un mantenimiento registrado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_MAINTENANCE_PR
(
    @MaintenanceId INT,
    @TurbineId INT,
    @EngineerId INT,
    @MaintenanceType NVARCHAR(100),
    @Description NVARCHAR(1000),
    @EstimatedStartDate DATETIME,
    @EstimatedEndDate DATETIME,
    @ActualStartDate DATETIME = NULL,
    @ActualEndDate DATETIME = NULL,
    @Result NVARCHAR(1000) = NULL,
    @Status NVARCHAR(50),
    @UpdatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblMaintenances
    SET
        TurbineId = @TurbineId,
        EngineerId = @EngineerId,
        MaintenanceType = @MaintenanceType,
        Description = @Description,
        EstimatedStartDate = @EstimatedStartDate,
        EstimatedEndDate = @EstimatedEndDate,
        ActualStartDate = @ActualStartDate,
        ActualEndDate = @ActualEndDate,
        Result = @Result,
        Status = @Status,
        UpdatedAt = @UpdatedAt
    WHERE MaintenanceId = @MaintenanceId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: DEL_MAINTENANCE_PR

    Descripción:
        Realiza la eliminación lógica de un mantenimiento.
        El registro no se elimina físicamente para conservar el historial
        operativo de la turbina.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.DEL_MAINTENANCE_PR
(
    @MaintenanceId INT,
    @Status NVARCHAR(50),
    @UpdatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblMaintenances
    SET
        Status = @Status,
        UpdatedAt = @UpdatedAt
    WHERE MaintenanceId = @MaintenanceId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ID_MAINTENANCE_PR

    Descripción:
        Obtiene la información de un mantenimiento mediante su identificador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ID_MAINTENANCE_PR
(
    @MaintenanceId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MaintenanceId,
        TurbineId,
        EngineerId,
        MaintenanceType,
        Description,
        EstimatedStartDate,
        EstimatedEndDate,
        ActualStartDate,
        ActualEndDate,
        Result,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblMaintenances
    WHERE MaintenanceId = @MaintenanceId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ALL_MAINTENANCE_PR

    Descripción:
        Obtiene todos los mantenimientos registrados en el sistema SGDE.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ALL_MAINTENANCE_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MaintenanceId,
        TurbineId,
        EngineerId,
        MaintenanceType,
        Description,
        EstimatedStartDate,
        EstimatedEndDate,
        ActualStartDate,
        ActualEndDate,
        Result,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblMaintenances
    ORDER BY MaintenanceId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_TURBINE_MAINTENANCE_PR

    Descripción:
        Obtiene los mantenimientos asociados a una turbina específica.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_TURBINE_MAINTENANCE_PR
(
    @TurbineId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MaintenanceId,
        TurbineId,
        EngineerId,
        MaintenanceType,
        Description,
        EstimatedStartDate,
        EstimatedEndDate,
        ActualStartDate,
        ActualEndDate,
        Result,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblMaintenances
    WHERE TurbineId = @TurbineId
    ORDER BY EstimatedStartDate DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ENGINEER_MAINTENANCE_PR

    Descripción:
        Obtiene los mantenimientos asignados a un ingeniero específico.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ENGINEER_MAINTENANCE_PR
(
    @EngineerId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MaintenanceId,
        TurbineId,
        EngineerId,
        MaintenanceType,
        Description,
        EstimatedStartDate,
        EstimatedEndDate,
        ActualStartDate,
        ActualEndDate,
        Result,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblMaintenances
    WHERE EngineerId = @EngineerId
    ORDER BY EstimatedStartDate DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_STATUS_MAINTENANCE_PR

    Descripción:
        Obtiene los mantenimientos que coincidan con el estado indicado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_STATUS_MAINTENANCE_PR
(
    @Status NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MaintenanceId,
        TurbineId,
        EngineerId,
        MaintenanceType,
        Description,
        EstimatedStartDate,
        EstimatedEndDate,
        ActualStartDate,
        ActualEndDate,
        Result,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblMaintenances
    WHERE Status = @Status
    ORDER BY EstimatedStartDate DESC;
END;
GO

/*==============================================================================
    STORED PROCEDURES: tblFailures
==============================================================================*/


/*==============================================================================
    PROCEDIMIENTO: CRE_FAILURE_PR

    Descripción:
        Registra una nueva falla asociada a una turbina y retorna el
        identificador generado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.CRE_FAILURE_PR
(
    @TurbineId INT,
    @EngineerId INT,
    @FailureDate DATETIME,
    @Severity NVARCHAR(50),
    @Description NVARCHAR(1000),
    @Resolution NVARCHAR(1000) = NULL,
    @Status NVARCHAR(50),
    @CreatedAt DATETIME,
    @UpdatedAt DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.tblFailures
    (
        TurbineId,
        EngineerId,
        FailureDate,
        Severity,
        Description,
        Resolution,
        Status,
        CreatedAt,
        UpdatedAt
    )
    VALUES
    (
        @TurbineId,
        @EngineerId,
        @FailureDate,
        @Severity,
        @Description,
        @Resolution,
        @Status,
        @CreatedAt,
        @UpdatedAt
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS FailureId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: UPD_FAILURE_PR

    Descripción:
        Actualiza la información de una falla registrada.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_FAILURE_PR
(
    @FailureId INT,
    @TurbineId INT,
    @EngineerId INT,
    @FailureDate DATETIME,
    @Severity NVARCHAR(50),
    @Description NVARCHAR(1000),
    @Resolution NVARCHAR(1000) = NULL,
    @Status NVARCHAR(50),
    @UpdatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblFailures
    SET
        TurbineId = @TurbineId,
        EngineerId = @EngineerId,
        FailureDate = @FailureDate,
        Severity = @Severity,
        Description = @Description,
        Resolution = @Resolution,
        Status = @Status,
        UpdatedAt = @UpdatedAt
    WHERE FailureId = @FailureId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: DEL_FAILURE_PR

    Descripción:
        Realiza la eliminación lógica de una falla.
        El registro no se elimina físicamente para conservar el historial
        operativo y técnico de la turbina.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.DEL_FAILURE_PR
(
    @FailureId INT,
    @Status NVARCHAR(50),
    @UpdatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblFailures
    SET
        Status = @Status,
        UpdatedAt = @UpdatedAt
    WHERE FailureId = @FailureId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ID_FAILURE_PR

    Descripción:
        Obtiene la información de una falla mediante su identificador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ID_FAILURE_PR
(
    @FailureId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FailureId,
        TurbineId,
        EngineerId,
        FailureDate,
        Severity,
        Description,
        Resolution,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblFailures
    WHERE FailureId = @FailureId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ALL_FAILURE_PR

    Descripción:
        Obtiene todas las fallas registradas en el sistema SGDE.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ALL_FAILURE_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FailureId,
        TurbineId,
        EngineerId,
        FailureDate,
        Severity,
        Description,
        Resolution,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblFailures
    ORDER BY FailureDate DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_TURBINE_FAILURE_PR

    Descripción:
        Obtiene las fallas asociadas a una turbina específica.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_TURBINE_FAILURE_PR
(
    @TurbineId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FailureId,
        TurbineId,
        EngineerId,
        FailureDate,
        Severity,
        Description,
        Resolution,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblFailures
    WHERE TurbineId = @TurbineId
    ORDER BY FailureDate DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ENGINEER_FAILURE_PR

    Descripción:
        Obtiene las fallas asignadas o atendidas por un ingeniero específico.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ENGINEER_FAILURE_PR
(
    @EngineerId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FailureId,
        TurbineId,
        EngineerId,
        FailureDate,
        Severity,
        Description,
        Resolution,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblFailures
    WHERE EngineerId = @EngineerId
    ORDER BY FailureDate DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_STATUS_FAILURE_PR

    Descripción:
        Obtiene las fallas que coincidan con el estado indicado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_STATUS_FAILURE_PR
(
    @Status NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FailureId,
        TurbineId,
        EngineerId,
        FailureDate,
        Severity,
        Description,
        Resolution,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblFailures
    WHERE Status = @Status
    ORDER BY FailureDate DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_SEVERITY_FAILURE_PR

    Descripción:
        Obtiene las fallas que coincidan con el nivel de severidad indicado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_SEVERITY_FAILURE_PR
(
    @Severity NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FailureId,
        TurbineId,
        EngineerId,
        FailureDate,
        Severity,
        Description,
        Resolution,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblFailures
    WHERE Severity = @Severity
    ORDER BY FailureDate DESC;
END;
GO

/*==============================================================================
    STORED PROCEDURES: tblBatteries
==============================================================================*/


/*==============================================================================
    PROCEDIMIENTO: CRE_BATTERY_PR

    Descripción:
        Registra una nueva batería asociada a una turbina y retorna el
        identificador generado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.CRE_BATTERY_PR
(
    @TurbineId INT,
    @MaximumCapacityMWh DECIMAL(18,4),
    @CurrentEnergyMWh DECIMAL(18,4),
    @TotalGeneratedMWh DECIMAL(18,4),
    @TotalTransferredMWh DECIMAL(18,4),
    @TotalSaturationLossMWh DECIMAL(18,4),
    @Status NVARCHAR(50),
    @CreatedAt DATETIME,
    @UpdatedAt DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.tblBatteries
    (
        TurbineId,
        MaximumCapacityMWh,
        CurrentEnergyMWh,
        TotalGeneratedMWh,
        TotalTransferredMWh,
        TotalSaturationLossMWh,
        Status,
        CreatedAt,
        UpdatedAt
    )
    VALUES
    (
        @TurbineId,
        @MaximumCapacityMWh,
        @CurrentEnergyMWh,
        @TotalGeneratedMWh,
        @TotalTransferredMWh,
        @TotalSaturationLossMWh,
        @Status,
        @CreatedAt,
        @UpdatedAt
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS BatteryId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: UPD_BATTERY_PR

    Descripción:
        Actualiza la información energética y operativa de una batería.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_BATTERY_PR
(
    @BatteryId INT,
    @TurbineId INT,
    @MaximumCapacityMWh DECIMAL(18,4),
    @CurrentEnergyMWh DECIMAL(18,4),
    @TotalGeneratedMWh DECIMAL(18,4),
    @TotalTransferredMWh DECIMAL(18,4),
    @TotalSaturationLossMWh DECIMAL(18,4),
    @Status NVARCHAR(50),
    @UpdatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblBatteries
    SET
        TurbineId = @TurbineId,
        MaximumCapacityMWh = @MaximumCapacityMWh,
        CurrentEnergyMWh = @CurrentEnergyMWh,
        TotalGeneratedMWh = @TotalGeneratedMWh,
        TotalTransferredMWh = @TotalTransferredMWh,
        TotalSaturationLossMWh = @TotalSaturationLossMWh,
        Status = @Status,
        UpdatedAt = @UpdatedAt
    WHERE BatteryId = @BatteryId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: DEL_BATTERY_PR

    Descripción:
        Realiza la eliminación lógica de una batería.
        El registro no se elimina físicamente para conservar el historial de
        generación, transferencias, pérdidas y vaciados.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.DEL_BATTERY_PR
(
    @BatteryId INT,
    @Status NVARCHAR(50),
    @UpdatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblBatteries
    SET
        Status = @Status,
        UpdatedAt = @UpdatedAt
    WHERE BatteryId = @BatteryId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ID_BATTERY_PR

    Descripción:
        Obtiene la información de una batería mediante su identificador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ID_BATTERY_PR
(
    @BatteryId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        BatteryId,
        TurbineId,
        MaximumCapacityMWh,
        CurrentEnergyMWh,
        TotalGeneratedMWh,
        TotalTransferredMWh,
        TotalSaturationLossMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblBatteries
    WHERE BatteryId = @BatteryId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ALL_BATTERY_PR

    Descripción:
        Obtiene todas las baterías registradas en el sistema SGDE.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ALL_BATTERY_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        BatteryId,
        TurbineId,
        MaximumCapacityMWh,
        CurrentEnergyMWh,
        TotalGeneratedMWh,
        TotalTransferredMWh,
        TotalSaturationLossMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblBatteries
    ORDER BY BatteryId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_TURBINE_BATTERY_PR

    Descripción:
        Obtiene las baterías asociadas a una turbina específica.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_TURBINE_BATTERY_PR
(
    @TurbineId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        BatteryId,
        TurbineId,
        MaximumCapacityMWh,
        CurrentEnergyMWh,
        TotalGeneratedMWh,
        TotalTransferredMWh,
        TotalSaturationLossMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblBatteries
    WHERE TurbineId = @TurbineId
    ORDER BY BatteryId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_STATUS_BATTERY_PR

    Descripción:
        Obtiene las baterías que coincidan con el estado indicado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_STATUS_BATTERY_PR
(
    @Status NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        BatteryId,
        TurbineId,
        MaximumCapacityMWh,
        CurrentEnergyMWh,
        TotalGeneratedMWh,
        TotalTransferredMWh,
        TotalSaturationLossMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblBatteries
    WHERE Status = @Status
    ORDER BY BatteryId DESC;
END;
GO

/*==============================================================================
    STORED PROCEDURES: tblFlushes
==============================================================================*/


/*==============================================================================
    PROCEDIMIENTO: CRE_FLUSH_PR

    Descripción:
        Registra un movimiento de vaciado de energía desde una batería hacia
        el banco central y retorna el identificador generado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.CRE_FLUSH_PR
(
    @FlushBatchId INT,
    @TurbineId INT,
    @BatteryId INT,
    @CentralBankId INT,
    @ExecutionType NVARCHAR(50),
    @Status NVARCHAR(50),
    @ExecutedAt DATETIME,
    @CreatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @SnapshotEnergyMWh DECIMAL(18,4);
        DECLARE @BankCurrentInventoryMWh DECIMAL(18,4);
        DECLARE @BankMaximumCapacityMWh DECIMAL(18,4);
        DECLARE @AvailableCapacityMWh DECIMAL(18,4);
        DECLARE @TransferredEnergyMWh DECIMAL(18,4);
        DECLARE @SaturationLossMWh DECIMAL(18,4);

        -- Obtener y bloquear la batería durante la transferencia
        SELECT
            @SnapshotEnergyMWh = CurrentEnergyMWh
        FROM dbo.tblBatteries WITH (UPDLOCK, HOLDLOCK)
        WHERE BatteryId = @BatteryId
          AND TurbineId = @TurbineId
          AND Status = 'Active';

        IF @SnapshotEnergyMWh IS NULL
        BEGIN
            THROW 50001,
                'La batería seleccionada no existe, no pertenece a la turbina o no está activa',
                1;
        END;

        IF @SnapshotEnergyMWh <= 0
        BEGIN
            THROW 50002,
                'La batería no contiene energía para realizar el vaciado',
                1;
        END;

        -- Obtener y bloquear el banco central
        SELECT
            @BankCurrentInventoryMWh = CurrentInventoryMWh,
            @BankMaximumCapacityMWh = MaximumCapacityMWh
        FROM dbo.tblCentralBank WITH (UPDLOCK, HOLDLOCK)
        WHERE CentralBankId = @CentralBankId
          AND Status = 'Active';

        IF @BankCurrentInventoryMWh IS NULL
        BEGIN
            THROW 50003,
                'El banco central seleccionado no existe o no está activo',
                1;
        END;

        -- Calcular la capacidad disponible
        SET @AvailableCapacityMWh =
            @BankMaximumCapacityMWh -
            @BankCurrentInventoryMWh;

        IF @AvailableCapacityMWh < 0
        BEGIN
            SET @AvailableCapacityMWh = 0;
        END;

        -- Calcular la energía transferida y la pérdida
        IF @SnapshotEnergyMWh <= @AvailableCapacityMWh
        BEGIN
            SET @TransferredEnergyMWh =
                @SnapshotEnergyMWh;

            SET @SaturationLossMWh = 0;
        END;
        ELSE
        BEGIN
            SET @TransferredEnergyMWh =
                @AvailableCapacityMWh;

            SET @SaturationLossMWh =
                @SnapshotEnergyMWh -
                @AvailableCapacityMWh;
        END;

        -- Registrar el vaciado
        INSERT INTO dbo.tblFlushes
        (
            FlushBatchId,
            TurbineId,
            BatteryId,
            CentralBankId,
            SnapshotEnergyMWh,
            TransferredEnergyMWh,
            SaturationLossMWh,
            ExecutionType,
            Status,
            ExecutedAt,
            CreatedAt
        )
        VALUES
        (
            @FlushBatchId,
            @TurbineId,
            @BatteryId,
            @CentralBankId,
            @SnapshotEnergyMWh,
            @TransferredEnergyMWh,
            @SaturationLossMWh,
            @ExecutionType,
            @Status,
            @ExecutedAt,
            @CreatedAt
        );

        -- Vaciar la batería y actualizar sus acumulados
        UPDATE dbo.tblBatteries
        SET
            CurrentEnergyMWh = 0,
            TotalTransferredMWh =
                TotalTransferredMWh +
                @TransferredEnergyMWh,
            TotalSaturationLossMWh =
                TotalSaturationLossMWh +
                @SaturationLossMWh,
            UpdatedAt = GETDATE()
        WHERE BatteryId = @BatteryId;

        -- Actualizar el banco central
        UPDATE dbo.tblCentralBank
        SET
            CurrentInventoryMWh =
                CurrentInventoryMWh +
                @TransferredEnergyMWh,
            TotalReceivedMWh =
                TotalReceivedMWh +
                @TransferredEnergyMWh,
            TotalSaturationLossMWh =
                TotalSaturationLossMWh +
                @SaturationLossMWh,
            UpdatedAt = GETDATE()
        WHERE CentralBankId = @CentralBankId;

        COMMIT TRANSACTION;

        SELECT CAST(SCOPE_IDENTITY() AS INT) AS FlushId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END;

        THROW;
    END CATCH;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: UPD_FLUSH_PR

    Descripción:
        Actualiza la información de un movimiento de vaciado registrado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_FLUSH_PR
(
    @FlushId INT,
    @FlushBatchId INT,
    @TurbineId INT,
    @BatteryId INT,
    @CentralBankId INT,
    @SnapshotEnergyMWh DECIMAL(18,4),
    @TransferredEnergyMWh DECIMAL(18,4),
    @SaturationLossMWh DECIMAL(18,4),
    @ExecutionType NVARCHAR(50),
    @Status NVARCHAR(50),
    @ExecutedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblFlushes
    SET
        FlushBatchId = @FlushBatchId,
        TurbineId = @TurbineId,
        BatteryId = @BatteryId,
        CentralBankId = @CentralBankId,
        SnapshotEnergyMWh = @SnapshotEnergyMWh,
        TransferredEnergyMWh = @TransferredEnergyMWh,
        SaturationLossMWh = @SaturationLossMWh,
        ExecutionType = @ExecutionType,
        Status = @Status,
        ExecutedAt = @ExecutedAt
    WHERE FlushId = @FlushId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: DEL_FLUSH_PR

    Descripción:
        Realiza la eliminación lógica de un movimiento de vaciado mediante la
        actualización de su estado.
        El registro se conserva para mantener la trazabilidad energética.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.DEL_FLUSH_PR
(
    @FlushId INT,
    @Status NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblFlushes
    SET
        Status = @Status
    WHERE FlushId = @FlushId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ID_FLUSH_PR

    Descripción:
        Obtiene un movimiento de vaciado mediante su identificador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ID_FLUSH_PR
(
    @FlushId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FlushId,
        FlushBatchId,
        TurbineId,
        BatteryId,
        CentralBankId,
        SnapshotEnergyMWh,
        TransferredEnergyMWh,
        SaturationLossMWh,
        ExecutionType,
        Status,
        ExecutedAt,
        CreatedAt
    FROM dbo.tblFlushes
    WHERE FlushId = @FlushId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ALL_FLUSH_PR

    Descripción:
        Obtiene todos los movimientos de vaciado registrados en el sistema.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ALL_FLUSH_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FlushId,
        FlushBatchId,
        TurbineId,
        BatteryId,
        CentralBankId,
        SnapshotEnergyMWh,
        TransferredEnergyMWh,
        SaturationLossMWh,
        ExecutionType,
        Status,
        ExecutedAt,
        CreatedAt
    FROM dbo.tblFlushes
    ORDER BY ExecutedAt DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_BATCH_FLUSH_PR

    Descripción:
        Obtiene todos los movimientos que forman parte de un mismo lote de
        vaciado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_BATCH_FLUSH_PR
(
    @FlushBatchId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FlushId,
        FlushBatchId,
        TurbineId,
        BatteryId,
        CentralBankId,
        SnapshotEnergyMWh,
        TransferredEnergyMWh,
        SaturationLossMWh,
        ExecutionType,
        Status,
        ExecutedAt,
        CreatedAt
    FROM dbo.tblFlushes
    WHERE FlushBatchId = @FlushBatchId
    ORDER BY FlushId ASC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_TURBINE_FLUSH_PR

    Descripción:
        Obtiene los movimientos de vaciado asociados a una turbina.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_TURBINE_FLUSH_PR
(
    @TurbineId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FlushId,
        FlushBatchId,
        TurbineId,
        BatteryId,
        CentralBankId,
        SnapshotEnergyMWh,
        TransferredEnergyMWh,
        SaturationLossMWh,
        ExecutionType,
        Status,
        ExecutedAt,
        CreatedAt
    FROM dbo.tblFlushes
    WHERE TurbineId = @TurbineId
    ORDER BY ExecutedAt DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_BATTERY_FLUSH_PR

    Descripción:
        Obtiene los movimientos de vaciado asociados a una batería.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_BATTERY_FLUSH_PR
(
    @BatteryId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FlushId,
        FlushBatchId,
        TurbineId,
        BatteryId,
        CentralBankId,
        SnapshotEnergyMWh,
        TransferredEnergyMWh,
        SaturationLossMWh,
        ExecutionType,
        Status,
        ExecutedAt,
        CreatedAt
    FROM dbo.tblFlushes
    WHERE BatteryId = @BatteryId
    ORDER BY ExecutedAt DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_CENTRAL_BANK_FLUSH_PR

    Descripción:
        Obtiene los movimientos de vaciado recibidos por un banco central.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_CENTRAL_BANK_FLUSH_PR
(
    @CentralBankId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FlushId,
        FlushBatchId,
        TurbineId,
        BatteryId,
        CentralBankId,
        SnapshotEnergyMWh,
        TransferredEnergyMWh,
        SaturationLossMWh,
        ExecutionType,
        Status,
        ExecutedAt,
        CreatedAt
    FROM dbo.tblFlushes
    WHERE CentralBankId = @CentralBankId
    ORDER BY ExecutedAt DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_STATUS_FLUSH_PR

    Descripción:
        Obtiene los movimientos de vaciado que coincidan con el estado
        indicado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_STATUS_FLUSH_PR
(
    @Status NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FlushId,
        FlushBatchId,
        TurbineId,
        BatteryId,
        CentralBankId,
        SnapshotEnergyMWh,
        TransferredEnergyMWh,
        SaturationLossMWh,
        ExecutionType,
        Status,
        ExecutedAt,
        CreatedAt
    FROM dbo.tblFlushes
    WHERE Status = @Status
    ORDER BY ExecutedAt DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_EXECUTION_TYPE_FLUSH_PR

    Descripción:
        Obtiene los movimientos de vaciado que coincidan con el tipo de
        ejecución indicado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_EXECUTION_TYPE_FLUSH_PR
(
    @ExecutionType NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        FlushId,
        FlushBatchId,
        TurbineId,
        BatteryId,
        CentralBankId,
        SnapshotEnergyMWh,
        TransferredEnergyMWh,
        SaturationLossMWh,
        ExecutionType,
        Status,
        ExecutedAt,
        CreatedAt
    FROM dbo.tblFlushes
    WHERE ExecutionType = @ExecutionType
    ORDER BY ExecutedAt DESC;
END;
GO

/*==============================================================================
    STORED PROCEDURES: tblCentralBank
==============================================================================*/


/*==============================================================================
    PROCEDIMIENTO: CRE_CENTRAL_BANK_PR

    Descripción:
        Registra un nuevo banco central de energía en el sistema SGDE y
        retorna el identificador generado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.CRE_CENTRAL_BANK_PR
(
    @Name NVARCHAR(150),
    @MaximumCapacityMWh DECIMAL(18,4),
    @CurrentInventoryMWh DECIMAL(18,4),
    @TotalReceivedMWh DECIMAL(18,4),
    @TotalDistributedMWh DECIMAL(18,4),
    @TotalSaturationLossMWh DECIMAL(18,4),
    @Status NVARCHAR(50),
    @CreatedAt DATETIME,
    @UpdatedAt DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.tblCentralBank
    (
        Name,
        MaximumCapacityMWh,
        CurrentInventoryMWh,
        TotalReceivedMWh,
        TotalDistributedMWh,
        TotalSaturationLossMWh,
        Status,
        CreatedAt,
        UpdatedAt
    )
    VALUES
    (
        @Name,
        @MaximumCapacityMWh,
        @CurrentInventoryMWh,
        @TotalReceivedMWh,
        @TotalDistributedMWh,
        @TotalSaturationLossMWh,
        @Status,
        @CreatedAt,
        @UpdatedAt
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS CentralBankId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: UPD_CENTRAL_BANK_PR

    Descripción:
        Actualiza la información energética y operativa de un banco central.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_CENTRAL_BANK_PR
(
    @CentralBankId INT,
    @Name NVARCHAR(150),
    @MaximumCapacityMWh DECIMAL(18,4),
    @CurrentInventoryMWh DECIMAL(18,4),
    @TotalReceivedMWh DECIMAL(18,4),
    @TotalDistributedMWh DECIMAL(18,4),
    @TotalSaturationLossMWh DECIMAL(18,4),
    @Status NVARCHAR(50),
    @UpdatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblCentralBank
    SET
        Name = @Name,
        MaximumCapacityMWh = @MaximumCapacityMWh,
        CurrentInventoryMWh = @CurrentInventoryMWh,
        TotalReceivedMWh = @TotalReceivedMWh,
        TotalDistributedMWh = @TotalDistributedMWh,
        TotalSaturationLossMWh = @TotalSaturationLossMWh,
        Status = @Status,
        UpdatedAt = @UpdatedAt
    WHERE CentralBankId = @CentralBankId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: DEL_CENTRAL_BANK_PR

    Descripción:
        Realiza la eliminación lógica de un banco central.

        El registro no se elimina físicamente para conservar el historial de
        vaciados, distribuciones y movimientos energéticos relacionados.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.DEL_CENTRAL_BANK_PR
(
    @CentralBankId INT,
    @Status NVARCHAR(50),
    @UpdatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblCentralBank
    SET
        Status = @Status,
        UpdatedAt = @UpdatedAt
    WHERE CentralBankId = @CentralBankId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ID_CENTRAL_BANK_PR

    Descripción:
        Obtiene la información de un banco central mediante su identificador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ID_CENTRAL_BANK_PR
(
    @CentralBankId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CentralBankId,
        Name,
        MaximumCapacityMWh,
        CurrentInventoryMWh,
        TotalReceivedMWh,
        TotalDistributedMWh,
        TotalSaturationLossMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblCentralBank
    WHERE CentralBankId = @CentralBankId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ALL_CENTRAL_BANK_PR

    Descripción:
        Obtiene todos los bancos centrales registrados en el sistema SGDE.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ALL_CENTRAL_BANK_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CentralBankId,
        Name,
        MaximumCapacityMWh,
        CurrentInventoryMWh,
        TotalReceivedMWh,
        TotalDistributedMWh,
        TotalSaturationLossMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblCentralBank
    ORDER BY CentralBankId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_NAME_CENTRAL_BANK_PR

    Descripción:
        Obtiene la información de un banco central mediante su nombre.
        Puede utilizarse para localizar un banco central o validar nombres
        repetidos antes de realizar un registro.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_NAME_CENTRAL_BANK_PR
(
    @Name NVARCHAR(150)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CentralBankId,
        Name,
        MaximumCapacityMWh,
        CurrentInventoryMWh,
        TotalReceivedMWh,
        TotalDistributedMWh,
        TotalSaturationLossMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblCentralBank
    WHERE Name = @Name;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_STATUS_CENTRAL_BANK_PR

    Descripción:
        Obtiene los bancos centrales que coincidan con el estado indicado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_STATUS_CENTRAL_BANK_PR
(
    @Status NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CentralBankId,
        Name,
        MaximumCapacityMWh,
        CurrentInventoryMWh,
        TotalReceivedMWh,
        TotalDistributedMWh,
        TotalSaturationLossMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblCentralBank
    WHERE Status = @Status
    ORDER BY CentralBankId DESC;
END;
GO

/*==============================================================================
    STORED PROCEDURES: tblForecasts
==============================================================================*/


/*==============================================================================
    PROCEDIMIENTO: CRE_FORECAST_PR

    Descripción:
        Registra una nueva previsión de consumo energético para un comprador
        y retorna el identificador generado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.CRE_FORECAST_PR
(
    @BuyerId INT,
    @ForecastYear INT,
    @ForecastMonth INT,
    @RequestedEnergyMWh DECIMAL(18,4),
    @Status NVARCHAR(50),
    @CreatedAt DATETIME,
    @UpdatedAt DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.tblForecasts
    (
        BuyerId,
        ForecastYear,
        ForecastMonth,
        RequestedEnergyMWh,
        Status,
        CreatedAt,
        UpdatedAt
    )
    VALUES
    (
        @BuyerId,
        @ForecastYear,
        @ForecastMonth,
        @RequestedEnergyMWh,
        @Status,
        @CreatedAt,
        @UpdatedAt
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ForecastId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: UPD_FORECAST_PR

    Descripción:
        Actualiza la información de una previsión energética registrada.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_FORECAST_PR
(
    @ForecastId INT,
    @BuyerId INT,
    @ForecastYear INT,
    @ForecastMonth INT,
    @RequestedEnergyMWh DECIMAL(18,4),
    @Status NVARCHAR(50),
    @UpdatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblForecasts
    SET
        BuyerId = @BuyerId,
        ForecastYear = @ForecastYear,
        ForecastMonth = @ForecastMonth,
        RequestedEnergyMWh = @RequestedEnergyMWh,
        Status = @Status,
        UpdatedAt = @UpdatedAt
    WHERE ForecastId = @ForecastId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: DEL_FORECAST_PR

    Descripción:
        Realiza la eliminación lógica de una previsión energética.
        El registro no se elimina físicamente para conservar la trazabilidad
        de las solicitudes y sus posibles distribuciones relacionadas.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.DEL_FORECAST_PR
(
    @ForecastId INT,
    @Status NVARCHAR(50),
    @UpdatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblForecasts
    SET
        Status = @Status,
        UpdatedAt = @UpdatedAt
    WHERE ForecastId = @ForecastId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ID_FORECAST_PR

    Descripción:
        Obtiene una previsión energética mediante su identificador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ID_FORECAST_PR
(
    @ForecastId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ForecastId,
        BuyerId,
        ForecastYear,
        ForecastMonth,
        RequestedEnergyMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblForecasts
    WHERE ForecastId = @ForecastId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ALL_FORECAST_PR

    Descripción:
        Obtiene todas las previsiones energéticas registradas en el sistema.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ALL_FORECAST_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ForecastId,
        BuyerId,
        ForecastYear,
        ForecastMonth,
        RequestedEnergyMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblForecasts
    ORDER BY ForecastYear DESC, ForecastMonth DESC, ForecastId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_BUYER_FORECAST_PR

    Descripción:
        Obtiene las previsiones energéticas asociadas a un comprador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_BUYER_FORECAST_PR
(
    @BuyerId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ForecastId,
        BuyerId,
        ForecastYear,
        ForecastMonth,
        RequestedEnergyMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblForecasts
    WHERE BuyerId = @BuyerId
    ORDER BY ForecastYear DESC, ForecastMonth DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_PERIOD_FORECAST_PR

    Descripción:
        Obtiene las previsiones energéticas correspondientes a un año y mes
        específicos.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_PERIOD_FORECAST_PR
(
    @ForecastYear INT,
    @ForecastMonth INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ForecastId,
        BuyerId,
        ForecastYear,
        ForecastMonth,
        RequestedEnergyMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblForecasts
    WHERE ForecastYear = @ForecastYear
      AND ForecastMonth = @ForecastMonth
    ORDER BY ForecastId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_BUYER_PERIOD_FORECAST_PR

    Descripción:
        Obtiene la previsión energética de un comprador para un año y mes
        específicos.
        Puede utilizarse para validar si el comprador ya registró una solicitud
        para el periodo indicado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_BUYER_PERIOD_FORECAST_PR
(
    @BuyerId INT,
    @ForecastYear INT,
    @ForecastMonth INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ForecastId,
        BuyerId,
        ForecastYear,
        ForecastMonth,
        RequestedEnergyMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblForecasts
    WHERE BuyerId = @BuyerId
      AND ForecastYear = @ForecastYear
      AND ForecastMonth = @ForecastMonth;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_STATUS_FORECAST_PR

    Descripción:
        Obtiene las previsiones energéticas que coincidan con el estado
        indicado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_STATUS_FORECAST_PR
(
    @Status NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ForecastId,
        BuyerId,
        ForecastYear,
        ForecastMonth,
        RequestedEnergyMWh,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.tblForecasts
    WHERE Status = @Status
    ORDER BY ForecastYear DESC, ForecastMonth DESC, ForecastId DESC;
END;
GO

/*==============================================================================
    STORED PROCEDURES: tblDistributions
==============================================================================*/


/*==============================================================================
    PROCEDIMIENTO: CRE_DISTRIBUTION_PR

    Descripción:
        Registra una nueva distribución de energía y retorna el identificador
        generado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.CRE_DISTRIBUTION_PR
(
    @DistributionBatchId INT,
    @ForecastId INT,
    @BuyerId INT,
    @CentralBankId INT,
    @RequestedEnergyMWh DECIMAL(18,4),
    @AssignedEnergyMWh DECIMAL(18,4),
    @UnassignedEnergyMWh DECIMAL(18,4),
    @UnitPrice DECIMAL(18,4),
    @DistributionDate DATETIME,
    @Status NVARCHAR(50),
    @CreatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.tblDistributions
    (
        DistributionBatchId,
        ForecastId,
        BuyerId,
        CentralBankId,
        RequestedEnergyMWh,
        AssignedEnergyMWh,
        UnassignedEnergyMWh,
        UnitPrice,
        DistributionDate,
        Status,
        CreatedAt
    )
    VALUES
    (
        @DistributionBatchId,
        @ForecastId,
        @BuyerId,
        @CentralBankId,
        @RequestedEnergyMWh,
        @AssignedEnergyMWh,
        @UnassignedEnergyMWh,
        @UnitPrice,
        @DistributionDate,
        @Status,
        @CreatedAt
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS DistributionId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: UPD_DISTRIBUTION_PR

    Descripción:
        Actualiza la información de una distribución energética registrada.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_DISTRIBUTION_PR
(
    @DistributionId INT,
    @DistributionBatchId INT,
    @ForecastId INT,
    @BuyerId INT,
    @CentralBankId INT,
    @RequestedEnergyMWh DECIMAL(18,4),
    @AssignedEnergyMWh DECIMAL(18,4),
    @UnassignedEnergyMWh DECIMAL(18,4),
    @UnitPrice DECIMAL(18,4),
    @DistributionDate DATETIME,
    @Status NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblDistributions
    SET
        DistributionBatchId = @DistributionBatchId,
        ForecastId = @ForecastId,
        BuyerId = @BuyerId,
        CentralBankId = @CentralBankId,
        RequestedEnergyMWh = @RequestedEnergyMWh,
        AssignedEnergyMWh = @AssignedEnergyMWh,
        UnassignedEnergyMWh = @UnassignedEnergyMWh,
        UnitPrice = @UnitPrice,
        DistributionDate = @DistributionDate,
        Status = @Status
    WHERE DistributionId = @DistributionId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: DEL_DISTRIBUTION_PR

    Descripción:
        Realiza la eliminación lógica de una distribución mediante la
        actualización de su estado.
        El registro no se elimina físicamente para conservar la trazabilidad
        energética y financiera.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.DEL_DISTRIBUTION_PR
(
    @DistributionId INT,
    @Status NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblDistributions
    SET
        Status = @Status
    WHERE DistributionId = @DistributionId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ID_DISTRIBUTION_PR

    Descripción:
        Obtiene una distribución de energía mediante su identificador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ID_DISTRIBUTION_PR
(
    @DistributionId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DistributionId,
        DistributionBatchId,
        ForecastId,
        BuyerId,
        CentralBankId,
        RequestedEnergyMWh,
        AssignedEnergyMWh,
        UnassignedEnergyMWh,
        UnitPrice,
        DistributionDate,
        Status,
        CreatedAt
    FROM dbo.tblDistributions
    WHERE DistributionId = @DistributionId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ALL_DISTRIBUTION_PR

    Descripción:
        Obtiene todas las distribuciones registradas en el sistema SGDE.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ALL_DISTRIBUTION_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DistributionId,
        DistributionBatchId,
        ForecastId,
        BuyerId,
        CentralBankId,
        RequestedEnergyMWh,
        AssignedEnergyMWh,
        UnassignedEnergyMWh,
        UnitPrice,
        DistributionDate,
        Status,
        CreatedAt
    FROM dbo.tblDistributions
    ORDER BY DistributionDate DESC, DistributionId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_BATCH_DISTRIBUTION_PR

    Descripción:
        Obtiene todas las distribuciones que pertenecen a un mismo lote.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_BATCH_DISTRIBUTION_PR
(
    @DistributionBatchId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DistributionId,
        DistributionBatchId,
        ForecastId,
        BuyerId,
        CentralBankId,
        RequestedEnergyMWh,
        AssignedEnergyMWh,
        UnassignedEnergyMWh,
        UnitPrice,
        DistributionDate,
        Status,
        CreatedAt
    FROM dbo.tblDistributions
    WHERE DistributionBatchId = @DistributionBatchId
    ORDER BY DistributionId ASC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_FORECAST_DISTRIBUTION_PR

    Descripción:
        Obtiene las distribuciones asociadas a una previsión energética.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_FORECAST_DISTRIBUTION_PR
(
    @ForecastId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DistributionId,
        DistributionBatchId,
        ForecastId,
        BuyerId,
        CentralBankId,
        RequestedEnergyMWh,
        AssignedEnergyMWh,
        UnassignedEnergyMWh,
        UnitPrice,
        DistributionDate,
        Status,
        CreatedAt
    FROM dbo.tblDistributions
    WHERE ForecastId = @ForecastId
    ORDER BY DistributionDate DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_BUYER_DISTRIBUTION_PR

    Descripción:
        Obtiene las distribuciones asociadas a un comprador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_BUYER_DISTRIBUTION_PR
(
    @BuyerId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DistributionId,
        DistributionBatchId,
        ForecastId,
        BuyerId,
        CentralBankId,
        RequestedEnergyMWh,
        AssignedEnergyMWh,
        UnassignedEnergyMWh,
        UnitPrice,
        DistributionDate,
        Status,
        CreatedAt
    FROM dbo.tblDistributions
    WHERE BuyerId = @BuyerId
    ORDER BY DistributionDate DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_CENTRAL_BANK_DISTRIBUTION_PR

    Descripción:
        Obtiene las distribuciones realizadas desde un banco central.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_CENTRAL_BANK_DISTRIBUTION_PR
(
    @CentralBankId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DistributionId,
        DistributionBatchId,
        ForecastId,
        BuyerId,
        CentralBankId,
        RequestedEnergyMWh,
        AssignedEnergyMWh,
        UnassignedEnergyMWh,
        UnitPrice,
        DistributionDate,
        Status,
        CreatedAt
    FROM dbo.tblDistributions
    WHERE CentralBankId = @CentralBankId
    ORDER BY DistributionDate DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_STATUS_DISTRIBUTION_PR

    Descripción:
        Obtiene las distribuciones que coincidan con el estado indicado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_STATUS_DISTRIBUTION_PR
(
    @Status NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DistributionId,
        DistributionBatchId,
        ForecastId,
        BuyerId,
        CentralBankId,
        RequestedEnergyMWh,
        AssignedEnergyMWh,
        UnassignedEnergyMWh,
        UnitPrice,
        DistributionDate,
        Status,
        CreatedAt
    FROM dbo.tblDistributions
    WHERE Status = @Status
    ORDER BY DistributionDate DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_DATE_RANGE_DISTRIBUTION_PR

    Descripción:
        Obtiene las distribuciones realizadas dentro de un rango de fechas.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_DATE_RANGE_DISTRIBUTION_PR
(
    @StartDate DATETIME,
    @EndDate DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DistributionId,
        DistributionBatchId,
        ForecastId,
        BuyerId,
        CentralBankId,
        RequestedEnergyMWh,
        AssignedEnergyMWh,
        UnassignedEnergyMWh,
        UnitPrice,
        DistributionDate,
        Status,
        CreatedAt
    FROM dbo.tblDistributions
    WHERE DistributionDate >= @StartDate
      AND DistributionDate <= @EndDate
    ORDER BY DistributionDate DESC;
END;
GO

/*==============================================================================
    STORED PROCEDURES: tblInvoices
==============================================================================*/


/*==============================================================================
    PROCEDIMIENTO: CRE_INVOICE_PR

    Descripción:
        Registra una nueva factura asociada a una distribución de energía y
        retorna el identificador generado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.CRE_INVOICE_PR
(
    @InvoiceNumber NVARCHAR(100),
    @DistributionId INT,
    @BuyerId INT,
    @IssueDate DATE,
    @DueDate DATE,
    @EnergyMWh DECIMAL(18,4),
    @UnitPrice DECIMAL(18,4),
    @Subtotal DECIMAL(18,4),
    @TaxPercentage DECIMAL(18,4),
    @TaxAmount DECIMAL(18,4),
    @TotalAmount DECIMAL(18,4),
    @PaymentStatus NVARCHAR(50),
    @CreatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.tblInvoices
    (
        InvoiceNumber,
        DistributionId,
        BuyerId,
        IssueDate,
        DueDate,
        EnergyMWh,
        UnitPrice,
        Subtotal,
        TaxPercentage,
        TaxAmount,
        TotalAmount,
        PaymentStatus,
        CreatedAt
    )
    VALUES
    (
        @InvoiceNumber,
        @DistributionId,
        @BuyerId,
        @IssueDate,
        @DueDate,
        @EnergyMWh,
        @UnitPrice,
        @Subtotal,
        @TaxPercentage,
        @TaxAmount,
        @TotalAmount,
        @PaymentStatus,
        @CreatedAt
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS InvoiceId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: UPD_INVOICE_PR

    Descripción:
        Actualiza la información financiera y el estado de pago de una
        factura registrada.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_INVOICE_PR
(
    @InvoiceId INT,
    @InvoiceNumber NVARCHAR(100),
    @DistributionId INT,
    @BuyerId INT,
    @IssueDate DATE,
    @DueDate DATE,
    @EnergyMWh DECIMAL(18,4),
    @UnitPrice DECIMAL(18,4),
    @Subtotal DECIMAL(18,4),
    @TaxPercentage DECIMAL(18,4),
    @TaxAmount DECIMAL(18,4),
    @TotalAmount DECIMAL(18,4),
    @PaymentStatus NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblInvoices
    SET
        InvoiceNumber = @InvoiceNumber,
        DistributionId = @DistributionId,
        BuyerId = @BuyerId,
        IssueDate = @IssueDate,
        DueDate = @DueDate,
        EnergyMWh = @EnergyMWh,
        UnitPrice = @UnitPrice,
        Subtotal = @Subtotal,
        TaxPercentage = @TaxPercentage,
        TaxAmount = @TaxAmount,
        TotalAmount = @TotalAmount,
        PaymentStatus = @PaymentStatus
    WHERE InvoiceId = @InvoiceId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: DEL_INVOICE_PR

    Descripción:
        Elimina físicamente una factura mediante su identificador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.DEL_INVOICE_PR
(
    @InvoiceId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.tblInvoices
        WHERE InvoiceId = @InvoiceId
    )
    BEGIN
        THROW 50001, 'La factura indicada no existe.', 1;
    END;

    UPDATE dbo.tblInvoices
    SET PaymentStatus = 'Cancelled'
    WHERE InvoiceId = @InvoiceId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ID_INVOICE_PR

    Descripción:
        Obtiene una factura mediante su identificador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ID_INVOICE_PR
(
    @InvoiceId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        InvoiceId,
        InvoiceNumber,
        DistributionId,
        BuyerId,
        IssueDate,
        DueDate,
        EnergyMWh,
        UnitPrice,
        Subtotal,
        TaxPercentage,
        TaxAmount,
        TotalAmount,
        PaymentStatus,
        CreatedAt
    FROM dbo.tblInvoices
    WHERE InvoiceId = @InvoiceId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ALL_INVOICE_PR

    Descripción:
        Obtiene todas las facturas registradas en el sistema SGDE.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ALL_INVOICE_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        InvoiceId,
        InvoiceNumber,
        DistributionId,
        BuyerId,
        IssueDate,
        DueDate,
        EnergyMWh,
        UnitPrice,
        Subtotal,
        TaxPercentage,
        TaxAmount,
        TotalAmount,
        PaymentStatus,
        CreatedAt
    FROM dbo.tblInvoices
    ORDER BY IssueDate DESC, InvoiceId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_NUMBER_INVOICE_PR

    Descripción:
        Obtiene una factura mediante su número de factura.
        Puede utilizarse para localizar una factura o validar números
        repetidos antes de registrar una nueva.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_NUMBER_INVOICE_PR
(
    @InvoiceNumber NVARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        InvoiceId,
        InvoiceNumber,
        DistributionId,
        BuyerId,
        IssueDate,
        DueDate,
        EnergyMWh,
        UnitPrice,
        Subtotal,
        TaxPercentage,
        TaxAmount,
        TotalAmount,
        PaymentStatus,
        CreatedAt
    FROM dbo.tblInvoices
    WHERE InvoiceNumber = @InvoiceNumber;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_DISTRIBUTION_INVOICE_PR

    Descripción:
        Obtiene las facturas asociadas a una distribución de energía.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_DISTRIBUTION_INVOICE_PR
(
    @DistributionId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        InvoiceId,
        InvoiceNumber,
        DistributionId,
        BuyerId,
        IssueDate,
        DueDate,
        EnergyMWh,
        UnitPrice,
        Subtotal,
        TaxPercentage,
        TaxAmount,
        TotalAmount,
        PaymentStatus,
        CreatedAt
    FROM dbo.tblInvoices
    WHERE DistributionId = @DistributionId
    ORDER BY IssueDate DESC, InvoiceId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_BUYER_INVOICE_PR

    Descripción:
        Obtiene las facturas asociadas a un comprador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_BUYER_INVOICE_PR
(
    @BuyerId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        InvoiceId,
        InvoiceNumber,
        DistributionId,
        BuyerId,
        IssueDate,
        DueDate,
        EnergyMWh,
        UnitPrice,
        Subtotal,
        TaxPercentage,
        TaxAmount,
        TotalAmount,
        PaymentStatus,
        CreatedAt
    FROM dbo.tblInvoices
    WHERE BuyerId = @BuyerId
    ORDER BY IssueDate DESC, InvoiceId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_PAYMENT_STATUS_INVOICE_PR

    Descripción:
        Obtiene las facturas que coincidan con el estado de pago indicado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_PAYMENT_STATUS_INVOICE_PR
(
    @PaymentStatus NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        InvoiceId,
        InvoiceNumber,
        DistributionId,
        BuyerId,
        IssueDate,
        DueDate,
        EnergyMWh,
        UnitPrice,
        Subtotal,
        TaxPercentage,
        TaxAmount,
        TotalAmount,
        PaymentStatus,
        CreatedAt
    FROM dbo.tblInvoices
    WHERE PaymentStatus = @PaymentStatus
    ORDER BY DueDate ASC, InvoiceId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_DATE_RANGE_INVOICE_PR

    Descripción:
        Obtiene las facturas emitidas dentro de un rango de fechas.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_DATE_RANGE_INVOICE_PR
(
    @StartDate DATE,
    @EndDate DATE
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        InvoiceId,
        InvoiceNumber,
        DistributionId,
        BuyerId,
        IssueDate,
        DueDate,
        EnergyMWh,
        UnitPrice,
        Subtotal,
        TaxPercentage,
        TaxAmount,
        TotalAmount,
        PaymentStatus,
        CreatedAt
    FROM dbo.tblInvoices
    WHERE IssueDate >= @StartDate
      AND IssueDate <= @EndDate
    ORDER BY IssueDate DESC, InvoiceId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_OVERDUE_INVOICE_PR

    Descripción:
        Obtiene las facturas cuya fecha de vencimiento sea anterior a la fecha
        indicada y que todavía no tengan un estado de pago completado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_OVERDUE_INVOICE_PR
(
    @CurrentDate DATE,
    @PaidStatus NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        InvoiceId,
        InvoiceNumber,
        DistributionId,
        BuyerId,
        IssueDate,
        DueDate,
        EnergyMWh,
        UnitPrice,
        Subtotal,
        TaxPercentage,
        TaxAmount,
        TotalAmount,
        PaymentStatus,
        CreatedAt
    FROM dbo.tblInvoices
    WHERE DueDate < @CurrentDate
      AND PaymentStatus <> @PaidStatus
    ORDER BY DueDate ASC, InvoiceId ASC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: UPD_PAYMENT_STATUS_INVOICE_PR

    Descripción:
        Actualiza únicamente el estado de pago de una factura.
        Este procedimiento permite cambiar el estado sin modificar los datos
        financieros originales de la factura.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_PAYMENT_STATUS_INVOICE_PR
(
    @InvoiceId INT,
    @PaymentStatus NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblInvoices
    SET
        PaymentStatus = @PaymentStatus
    WHERE InvoiceId = @InvoiceId;
END;
GO

/*==============================================================================
    STORED PROCEDURES: tblNotifications
==============================================================================*/


/*==============================================================================
    PROCEDIMIENTO: CRE_NOTIFICATION_PR

    Descripción:
        Registra una nueva notificación asociada a un usuario y retorna el
        identificador generado..
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.CRE_NOTIFICATION_PR
(
    @UserId INT,
    @Title NVARCHAR(150),
    @Message NVARCHAR(MAX),
    @NotificationType NVARCHAR(50),
    @ReferenceType NVARCHAR(50) = NULL,
    @ReferenceId INT = NULL,
    @IsRead BIT,
    @ReadAt DATETIME = NULL,
    @CreatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.tblNotifications
    (
        UserId,
        Title,
        Message,
        NotificationType,
        ReferenceType,
        ReferenceId,
        IsRead,
        ReadAt,
        CreatedAt
    )
    VALUES
    (
        @UserId,
        @Title,
        @Message,
        @NotificationType,
        @ReferenceType,
        @ReferenceId,
        @IsRead,
        @ReadAt,
        @CreatedAt
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NotificationId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: UPD_NOTIFICATION_PR

    Descripción:
        Actualiza la información general de una notificación registrada.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_NOTIFICATION_PR
(
    @NotificationId INT,
    @UserId INT,
    @Title NVARCHAR(150),
    @Message NVARCHAR(MAX),
    @NotificationType NVARCHAR(50),
    @ReferenceType NVARCHAR(50) = NULL,
    @ReferenceId INT = NULL,
    @IsRead BIT,
    @ReadAt DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblNotifications
    SET
        UserId = @UserId,
        Title = @Title,
        Message = @Message,
        NotificationType = @NotificationType,
        ReferenceType = @ReferenceType,
        ReferenceId = @ReferenceId,
        IsRead = @IsRead,
        ReadAt = @ReadAt
    WHERE NotificationId = @NotificationId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: DEL_NOTIFICATION_PR

    Descripción:
        Elimina físicamente una notificación mediante su identificador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.DEL_NOTIFICATION_PR
(
    @NotificationId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.tblNotifications
    WHERE NotificationId = @NotificationId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ID_NOTIFICATION_PR

    Descripción:
        Obtiene una notificación mediante su identificador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ID_NOTIFICATION_PR
(
    @NotificationId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        NotificationId,
        UserId,
        Title,
        Message,
        NotificationType,
        ReferenceType,
        ReferenceId,
        IsRead,
        ReadAt,
        CreatedAt
    FROM dbo.tblNotifications
    WHERE NotificationId = @NotificationId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ALL_NOTIFICATION_PR

    Descripción:
        Obtiene todas las notificaciones registradas en el sistema SGDE.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ALL_NOTIFICATION_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        NotificationId,
        UserId,
        Title,
        Message,
        NotificationType,
        ReferenceType,
        ReferenceId,
        IsRead,
        ReadAt,
        CreatedAt
    FROM dbo.tblNotifications
    ORDER BY CreatedAt DESC, NotificationId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_USER_NOTIFICATION_PR

    Descripción:
        Obtiene todas las notificaciones asociadas a un usuario.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_USER_NOTIFICATION_PR
(
    @UserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        NotificationId,
        UserId,
        Title,
        Message,
        NotificationType,
        ReferenceType,
        ReferenceId,
        IsRead,
        ReadAt,
        CreatedAt
    FROM dbo.tblNotifications
    WHERE UserId = @UserId
    ORDER BY CreatedAt DESC, NotificationId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_UNREAD_BY_USER_NOTIFICATION_PR

    Descripción:
        Obtiene las notificaciones no leídas asociadas a un usuario.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_UNREAD_BY_USER_NOTIFICATION_PR
(
    @UserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        NotificationId,
        UserId,
        Title,
        Message,
        NotificationType,
        ReferenceType,
        ReferenceId,
        IsRead,
        ReadAt,
        CreatedAt
    FROM dbo.tblNotifications
    WHERE UserId = @UserId
      AND IsRead = 0
    ORDER BY CreatedAt DESC, NotificationId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_TYPE_NOTIFICATION_PR

    Descripción:
        Obtiene las notificaciones que coincidan con el tipo indicado.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_TYPE_NOTIFICATION_PR
(
    @NotificationType NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        NotificationId,
        UserId,
        Title,
        Message,
        NotificationType,
        ReferenceType,
        ReferenceId,
        IsRead,
        ReadAt,
        CreatedAt
    FROM dbo.tblNotifications
    WHERE NotificationType = @NotificationType
    ORDER BY CreatedAt DESC, NotificationId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_REFERENCE_NOTIFICATION_PR

    Descripción:
        Obtiene las notificaciones relacionadas con una entidad específica
        mediante su tipo y su identificador de referencia.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_REFERENCE_NOTIFICATION_PR
(
    @ReferenceType NVARCHAR(50),
    @ReferenceId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        NotificationId,
        UserId,
        Title,
        Message,
        NotificationType,
        ReferenceType,
        ReferenceId,
        IsRead,
        ReadAt,
        CreatedAt
    FROM dbo.tblNotifications
    WHERE ReferenceType = @ReferenceType
      AND ReferenceId = @ReferenceId
    ORDER BY CreatedAt DESC, NotificationId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_DATE_RANGE_NOTIFICATION_PR

    Descripción:
        Obtiene las notificaciones creadas dentro de un rango de fechas.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_DATE_RANGE_NOTIFICATION_PR
(
    @StartDate DATETIME,
    @EndDate DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        NotificationId,
        UserId,
        Title,
        Message,
        NotificationType,
        ReferenceType,
        ReferenceId,
        IsRead,
        ReadAt,
        CreatedAt
    FROM dbo.tblNotifications
    WHERE CreatedAt >= @StartDate
      AND CreatedAt <= @EndDate
    ORDER BY CreatedAt DESC, NotificationId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: UPD_READ_STATUS_NOTIFICATION_PR

    Descripción:
        Actualiza el estado de lectura de una notificación.
        Cuando la notificación se marca como leída, NotificationManager debe
        enviar la fecha y hora en ReadAt. Cuando se marca como no leída,
        ReadAt debe enviarse como NULL.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_READ_STATUS_NOTIFICATION_PR
(
    @NotificationId INT,
    @IsRead BIT,
    @ReadAt DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblNotifications
    SET
        IsRead = @IsRead,
        ReadAt = @ReadAt
    WHERE NotificationId = @NotificationId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: UPD_ALL_READ_BY_USER_NOTIFICATION_PR

    Descripción:
        Marca como leídas todas las notificaciones pendientes de un usuario.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_ALL_READ_BY_USER_NOTIFICATION_PR
(
    @UserId INT,
    @ReadAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblNotifications
    SET
        IsRead = 1,
        ReadAt = @ReadAt
    WHERE UserId = @UserId
      AND IsRead = 0;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_UNREAD_COUNT_BY_USER_NOTIFICATION_PR

    Descripción:
        Obtiene la cantidad de notificaciones no leídas de un usuario.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_UNREAD_COUNT_BY_USER_NOTIFICATION_PR
(
    @UserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) AS UnreadCount
    FROM dbo.tblNotifications
    WHERE UserId = @UserId
      AND IsRead = 0;
END;
GO

/*==============================================================================
    STORED PROCEDURES: tblAudit
==============================================================================*/


/*==============================================================================
    PROCEDIMIENTO: CRE_AUDIT_PR

    Descripción:
        Registra una nueva acción de auditoría en el sistema SGDE y retorna
        el identificador generado.
        El usuario puede ser NULL cuando la acción no esté asociada a una
        sesión autenticada, por ejemplo, un intento de acceso fallido.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.CRE_AUDIT_PR
(
    @UserId INT = NULL,
    @Action NVARCHAR(50),
    @EntityName NVARCHAR(100),
    @EntityId INT = NULL,
    @Description NVARCHAR(500),
    @IpAddress NVARCHAR(50) = NULL,
    @CreatedAt DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.tblAudit
    (
        UserId,
        Action,
        EntityName,
        EntityId,
        Description,
        IpAddress,
        CreatedAt
    )
    VALUES
    (
        @UserId,
        @Action,
        @EntityName,
        @EntityId,
        @Description,
        @IpAddress,
        @CreatedAt
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS AuditId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: UPD_AUDIT_PR

    Descripción:
        Actualiza la información de un registro de auditoría.
        Este procedimiento debe utilizarse únicamente en casos administrativos
        excepcionales, ya que los registros de auditoría normalmente no deben
        modificarse.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_AUDIT_PR
(
    @AuditId INT,
    @UserId INT = NULL,
    @Action NVARCHAR(50),
    @EntityName NVARCHAR(100),
    @EntityId INT = NULL,
    @Description NVARCHAR(500),
    @IpAddress NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tblAudit
    SET
        UserId = @UserId,
        Action = @Action,
        EntityName = @EntityName,
        EntityId = @EntityId,
        Description = @Description,
        IpAddress = @IpAddress
    WHERE AuditId = @AuditId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: DEL_AUDIT_PR

    Descripción:
        Elimina físicamente un registro de auditoría mediante su identificador.
        AuditManager debe restringir esta operación y permitirla únicamente
        a usuarios autorizados.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.DEL_AUDIT_PR
(
    @AuditId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.tblAudit
    WHERE AuditId = @AuditId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ID_AUDIT_PR

    Descripción:
        Obtiene un registro de auditoría mediante su identificador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ID_AUDIT_PR
(
    @AuditId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        AuditId,
        UserId,
        Action,
        EntityName,
        EntityId,
        Description,
        IpAddress,
        CreatedAt
    FROM dbo.tblAudit
    WHERE AuditId = @AuditId;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ALL_AUDIT_PR

    Descripción:
        Obtiene todos los registros de auditoría del sistema SGDE.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ALL_AUDIT_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        AuditId,
        UserId,
        Action,
        EntityName,
        EntityId,
        Description,
        IpAddress,
        CreatedAt
    FROM dbo.tblAudit
    ORDER BY CreatedAt DESC, AuditId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_USER_AUDIT_PR

    Descripción:
        Obtiene los registros de auditoría asociados a un usuario.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_USER_AUDIT_PR
(
    @UserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        AuditId,
        UserId,
        Action,
        EntityName,
        EntityId,
        Description,
        IpAddress,
        CreatedAt
    FROM dbo.tblAudit
    WHERE UserId = @UserId
    ORDER BY CreatedAt DESC, AuditId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ACTION_AUDIT_PR

    Descripción:
        Obtiene los registros de auditoría que coincidan con la acción
        indicada.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ACTION_AUDIT_PR
(
    @Action NVARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        AuditId,
        UserId,
        Action,
        EntityName,
        EntityId,
        Description,
        IpAddress,
        CreatedAt
    FROM dbo.tblAudit
    WHERE Action = @Action
    ORDER BY CreatedAt DESC, AuditId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ENTITY_AUDIT_PR

    Descripción:
        Obtiene los registros de auditoría asociados a un tipo de entidad.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ENTITY_AUDIT_PR
(
    @EntityName NVARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        AuditId,
        UserId,
        Action,
        EntityName,
        EntityId,
        Description,
        IpAddress,
        CreatedAt
    FROM dbo.tblAudit
    WHERE EntityName = @EntityName
    ORDER BY CreatedAt DESC, AuditId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_ENTITY_ID_AUDIT_PR

    Descripción:
        Obtiene los registros de auditoría asociados a una entidad específica
        mediante su nombre y su identificador.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_ENTITY_ID_AUDIT_PR
(
    @EntityName NVARCHAR(100),
    @EntityId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        AuditId,
        UserId,
        Action,
        EntityName,
        EntityId,
        Description,
        IpAddress,
        CreatedAt
    FROM dbo.tblAudit
    WHERE EntityName = @EntityName
      AND EntityId = @EntityId
    ORDER BY CreatedAt DESC, AuditId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_DATE_RANGE_AUDIT_PR

    Descripción:
        Obtiene los registros de auditoría creados dentro de un rango de
        fechas.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_DATE_RANGE_AUDIT_PR
(
    @StartDate DATETIME,
    @EndDate DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        AuditId,
        UserId,
        Action,
        EntityName,
        EntityId,
        Description,
        IpAddress,
        CreatedAt
    FROM dbo.tblAudit
    WHERE CreatedAt >= @StartDate
      AND CreatedAt <= @EndDate
    ORDER BY CreatedAt DESC, AuditId DESC;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: RET_BY_IP_ADDRESS_AUDIT_PR

    Descripción:
        Obtiene los registros de auditoría asociados a una dirección IP.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_BY_IP_ADDRESS_AUDIT_PR
(
    @IpAddress NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        AuditId,
        UserId,
        Action,
        EntityName,
        EntityId,
        Description,
        IpAddress,
        CreatedAt
    FROM dbo.tblAudit
    WHERE IpAddress = @IpAddress
    ORDER BY CreatedAt DESC, AuditId DESC;
END;
GO


--sp de centralbankmovement
CREATE OR ALTER PROCEDURE CRE_CENTRALBANKMOVEMENT_PR
(
    @CentralBankId INT,
    @MovementType NVARCHAR(50),
    @EnergyMWh DECIMAL(18,4),
    @InventoryAfterMovement DECIMAL(18,4),
    @SaturationLossMWh DECIMAL(18,4),
    @Description NVARCHAR(500),
    @CreatedAt DATETIME,
    @UpdatedAt DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.tblCentralBankMovements
    (
        CentralBankId,
        MovementType,
        EnergyMWh,
        InventoryAfterMovement,
        SaturationLossMWh,
        Description,
        CreatedAt,
        UpdatedAt
    )
    VALUES
    (
        @CentralBankId,
        @MovementType,
        @EnergyMWh,
        @InventoryAfterMovement,
        @SaturationLossMWh,
        @Description,
        @CreatedAt,
        @UpdatedAt
    );
END;
GO

CREATE OR ALTER PROCEDURE UPD_CENTRALBANKMOVEMENT_PR
(
    @MovementId INT,
    @CentralBankId INT,
    @MovementType NVARCHAR(50),
    @EnergyMWh DECIMAL(18,4),
    @InventoryAfterMovement DECIMAL(18,4),
    @SaturationLossMWh DECIMAL(18,4),
    @Description NVARCHAR(500),
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

CREATE OR ALTER PROCEDURE DEL_CENTRALBANKMOVEMENT_PR
(
    @MovementId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE
    FROM dbo.tblCentralBankMovements
    WHERE MovementId = @MovementId;
END;
GO

CREATE OR ALTER PROCEDURE RET_ALL_CENTRALBANKMOVEMENT_PR
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
    ORDER BY CreatedAt DESC;
END;
GO

CREATE OR ALTER PROCEDURE RET_BY_ID_CENTRALBANKMOVEMENT_PR
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

