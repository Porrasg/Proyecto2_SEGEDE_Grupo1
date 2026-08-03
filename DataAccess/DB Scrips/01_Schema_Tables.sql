USE Proyecto2_SEGEDE_Grupo1;
GO

/*==============================================================================
    TABLA: tblUsers (Usuarios del sistema)

    Descripción:
        Almacena la información personal, las credenciales, el rol y el estado
        actual de los usuarios registrados en el sistema SGDE.

        La edad no se almacena porque se calcula a partir de BirthDate.

==============================================================================*/

CREATE TABLE dbo.tblUsers
(
    UserId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    Identification NVARCHAR(20) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    FirstLastName NVARCHAR(100) NOT NULL,
    SecondLastName NVARCHAR(100) NULL,
    BirthDate DATE NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    ProfilePhoto NVARCHAR(MAX) NULL,
    Password NVARCHAR(255) NOT NULL,
    Age INT NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    FailedLoginAttempts INT NOT NULL,
    LockoutEndAt DATETIME NULL,
    LastLoginAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL
);
GO


/*==============================================================================
    TABLA: tblOtp (Verificación en dos pasos)

    Descripción:
        Almacena los códigos de verificación de un solo uso (OTP)
        generados por el sistema para validar la identidad de un usuario
        durante procesos sensibles como el registro de una cuenta,
        inicio de sesión, recuperación de contraseña o cualquier otra
        operación que requiera autenticación adicional.

        Cada registro contiene el correo electrónico asociado, el código
        OTP generado, su fecha de expiración y un indicador que determina
        si el código ya fue utilizado, garantizando que cada token pueda
        emplearse una única vez por motivos de seguridad.

==============================================================================*/

CREATE TABLE dbo.OtpTokens (
    Id INT IDENTITY(1,1) NOT NULL,
    Created DATETIME DEFAULT GETDATE() NOT NULL,
    Updated DATETIME DEFAULT GETDATE() NOT NULL,
    Email VARCHAR(150) NOT NULL,
    TokenCode VARCHAR(6) NOT NULL,        -- OTP estrictamente de 6 dígitos numéricos
    Purpose VARCHAR(30) NOT NULL,
    ExpirationDate DATETIME NOT NULL,     -- Fecha límite de vigencia (Calculada en C#)
    IsUsed BIT DEFAULT 0 NOT NULL,         -- Bandera de seguridad de un solo uso
    CONSTRAINT [PK_OtpTokens] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

/*==============================================================================
    TABLA: tblTurbines (Turbinas)

    Descripción:
        Almacena la información general y técnica de las turbinas registradas
        en el sistema SGDE.

        La generación de energía y los movimientos energéticos se administran
        desde las tablas correspondientes a baterías, vaciados y auditoría.

==============================================================================*/

CREATE TABLE dbo.tblTurbines
(
    TurbineId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    Code NVARCHAR(50) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Location NVARCHAR(250) NOT NULL,
    Brand NVARCHAR(100) NOT NULL, -- Fabricante/Marca
    Model NVARCHAR(100) NOT NULL,
    ManufactureYear INT NOT NULL,
    NominalWeeklyCapacityMWh DECIMAL(18,4) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL
);
GO

/*==============================================================================
    TABLA: tblMaintenances (Mantenimientos)

    Descripción:
        Almacena la información de los mantenimientos programados y ejecutados
        sobre las turbinas registradas en el sistema SGDE.

        La relación con la turbina y el ingeniero se agregará posteriormente
        mediante llaves foráneas.
==============================================================================*/

CREATE TABLE dbo.tblMaintenances
(
    MaintenanceId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    TurbineId INT NOT NULL,
    EngineerId INT NOT NULL,
    MaintenanceType NVARCHAR(50) NOT NULL, -- maneja valores Preventive, Corrective, Predictive, Inspection
    Description NVARCHAR(500) NOT NULL,
    EstimatedStartDate DATETIME NOT NULL,
    EstimatedEndDate DATETIME NOT NULL,
    ActualStartDate DATETIME NULL,
    ActualEndDate DATETIME NULL,
    Result NVARCHAR(500) NULL,
    Status NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL
);
GO

/*==============================================================================
    TABLA: tblFailures (Fallas reportadas)

    Descripción:
        Almacena las fallas detectadas en las turbinas registradas en el
        sistema SGDE.

        Cada falla es registrada y gestionada por un ingeniero. La tabla
        conserva la descripción del problema, su nivel de severidad, la
        solución aplicada y el estado actual de la falla.

        Las relaciones con la turbina y el ingeniero se agregarán
        posteriormente mediante llaves foráneas.

==============================================================================*/

CREATE TABLE dbo.tblFailures
(
    FailureId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    TurbineId INT NOT NULL,
    EngineerId INT NOT NULL,
    FailureDate DATETIME NOT NULL,
    Severity NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500) NOT NULL, --explica qué falla ocurrió
    Resolution NVARCHAR(500) NULL, -- explica cómo se solucionó.
    Status NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL
);
GO

/*==============================================================================
    TABLA: tblBatteries (Estado e historial acumulado de las baterías)

    Descripción:
        Almacena la información energética de las baterías vinculadas a las
        turbinas registradas en el sistema SGDE.

        Cada batería conserva su capacidad máxima, la energía disponible,
        la energía acumulada y las pérdidas producidas por saturación.

        La relación con la turbina se agregará posteriormente mediante una
        llave foránea.

==============================================================================*/

CREATE TABLE dbo.tblBatteries
(
    BatteryId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    TurbineId INT NOT NULL,
    MaximumCapacityMWh DECIMAL(18,4) NOT NULL, --capacidad máxima de almacenamiento
    CurrentEnergyMWh DECIMAL(18,4) NOT NULL, --almacena la energía disponible actualmente en la batería.
    TotalGeneratedMWh DECIMAL(18,4) NOT NULL, --conserva la energía total que la turbina ha enviado a la batería durante las simulaciones
    TotalTransferredMWh DECIMAL(18,4) NOT NULL, --registra la energía acumulada que ha sido vaciada desde la batería hacia el banco central
    TotalSaturationLossMWh DECIMAL(18,4) NOT NULL, -- guarda la energía que no pudo almacenarse porque la batería alcanzó su capacidad máxima
    Status NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL
);
GO

/*==============================================================================
    TABLA: tblFlushes (Vaciados hacia el banco central)

    Descripción:
        Almacena los movimientos de energía realizados desde las baterías
        hacia el banco central del sistema SGDE.

        Cada registro representa la participación de una batería en un proceso
        de vaciado. Los registros que pertenezcan al mismo proceso podrán
        agruparse mediante FlushBatchId.

        Las relaciones con la turbina, la batería y el banco central se
        agregarán posteriormente mediante llaves foráneas.

==============================================================================*/

CREATE TABLE dbo.tblFlushes
(
    FlushId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    FlushBatchId INT NOT NULL,
    TurbineId INT NOT NULL,
    BatteryId INT NOT NULL,
    CentralBankId INT NOT NULL,
    SnapshotEnergyMWh DECIMAL(18,4) NOT NULL, -- guarda la energía que tenía la batería justo antes del vaciado
    TransferredEnergyMWh DECIMAL(18,4) NOT NULL, -- almacena la cantidad de energía que realmente se transfirió al banco central
    SaturationLossMWh DECIMAL(18,4) NOT NULL, -- registra la energía que no pudo ingresar al banco central por falta de capacidad disponible
    ExecutionType NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    ExecutedAt DATETIME NOT NULL,
    CreatedAt DATETIME NOT NULL
);
GO

/*==============================================================================
    TABLA: tblCentralBank (Banco central de energía)

    Descripción:
        Almacena el estado general del banco central de energía del sistema
        SGDE.

        El banco central recibe la energía transferida desde las baterías,
        conserva el inventario energético disponible y registra los valores
        acumulados de energía recibida, distribuida y perdida por saturación.

==============================================================================*/

CREATE TABLE dbo.tblCentralBank
(
    CentralBankId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    MaximumCapacityMWh DECIMAL(18,4) NOT NULL, -- cantidad máxima de energía que puede almacenar el banco central
    CurrentInventoryMWh DECIMAL(18,4) NOT NULL,
    TotalReceivedMWh DECIMAL(18,4) NOT NULL, -- no representa el inventario actual, sino un total histórico
    TotalDistributedMWh DECIMAL(18,4) NOT NULL, -- acumula toda la energía que ya fue distribuida entre los compradores
    TotalSaturationLossMWh DECIMAL(18,4) NOT NULL, -- energía que no pudo ingresar al banco central porque este alcanzó su capacidad máxima
    Status NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL
);
GO

/*==============================================================================
    TABLA: tblForecasts (Solicitudes de energía de los compradores)

    Descripción:
        Almacena las solicitudes de compra de energía realizadas por los
        compradores registrados en el sistema SGDE.

        Cada pronóstico representa la cantidad de energía que un comprador
        solicita para un período determinado y será utilizado posteriormente
        durante el proceso de distribución de energía.

        La relación con el comprador se agregará posteriormente mediante una
        llave foránea.

==============================================================================*/

CREATE TABLE dbo.tblForecasts
(
    ForecastId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    BuyerId INT NOT NULL,
    ForecastYear INT NOT NULL,
    ForecastMonth INT NOT NULL,
    RequestedEnergyMWh DECIMAL(18,4) NOT NULL, -- cantidad de energía que el comprador solicita
    Status NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL
);
GO

/*==============================================================================
    TABLA: tblDistributions (Distribución de energía)

    Descripción:
        Almacena el historial de las distribuciones de energía realizadas
        desde el banco central hacia los compradores.

        Cada registro representa la asignación de energía a un forecast
        específico y conserva una copia de la solicitud original para mantener
        el historial de la distribución.

        Las relaciones con el forecast, el comprador y el banco central se
        agregarán posteriormente mediante llaves foráneas.

==============================================================================*/

CREATE TABLE dbo.tblDistributions
(
    DistributionId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    DistributionBatchId INT NOT NULL, -- distribuciones realizadas durante una misma ejecución
    ForecastId INT NOT NULL, -- identifica la solicitud que originó la distribución
    BuyerId INT NOT NULL, -- comprador que recibió la energía
    CentralBankId INT NOT NULL, -- indica desde que banco central se realizó la distribución
    RequestedEnergyMWh DECIMAL(18,4) NOT NULL, -- copia de la energía solicitada en el forecast
    AssignedEnergyMWh DECIMAL(18,4) NOT NULL, -- cantidad de energía entregada al comprador
    UnassignedEnergyMWh DECIMAL(18,4) NOT NULL, -- energía que no pudo entregarse
    UnitPrice DECIMAL(18,4) NOT NULL,
    DistributionDate DATETIME NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME NOT NULL
);
GO


/*==============================================================================
    TABLA: tblInvoices (Facturación)

    Descripción:
        Almacena las facturas generadas a partir de las distribuciones de
        energía realizadas por el sistema SGDE.

        Cada factura conserva una copia de la información utilizada para su
        generación con el fin de mantener la integridad del historial
        financiero, aunque posteriormente cambien los datos de la distribución.

        La relación con la distribución y el comprador se agregará
        posteriormente mediante llaves foráneas.

==============================================================================*/

CREATE TABLE dbo.tblInvoices
(
    InvoiceId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    InvoiceNumber NVARCHAR(50) NOT NULL,
    DistributionId INT NOT NULL,
    BuyerId INT NOT NULL,
    IssueDate DATETIME NOT NULL,
    DueDate DATETIME NOT NULL,
    EnergyMWh DECIMAL(18,4) NOT NULL,
    UnitPrice DECIMAL(18,4) NOT NULL,
    Subtotal DECIMAL(18,4) NOT NULL,
    TaxPercentage DECIMAL(5,2) NOT NULL,
    TaxAmount DECIMAL(18,4) NOT NULL,
    TotalAmount DECIMAL(18,4) NOT NULL,
    PaymentStatus NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME NOT NULL
);
GO

/*==============================================================================
    TABLA: tblNotifications (Notificaciones del sistema)

    Descripción:
        Almacena las notificaciones generadas por el sistema SGDE para los
        usuarios registrados.

        Cada notificación contiene un título, un mensaje, su tipo, el estado
        de lectura y, cuando corresponda, una referencia al registro que
        originó la notificación.

        La relación con el usuario se agregará posteriormente mediante una
        llave foránea.

==============================================================================*/

CREATE TABLE dbo.tblNotifications
(
    NotificationId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    UserId INT NOT NULL,
    Title NVARCHAR(150) NOT NULL,
    Message NVARCHAR(500) NOT NULL,
    NotificationType NVARCHAR(50) NOT NULL,
    ReferenceType NVARCHAR(50) NULL, -- que tipo de registro originó la notificación
    ReferenceId INT NULL,
    IsRead BIT NOT NULL,
    ReadAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL
);
GO


/*==============================================================================
    TABLA: tblAudit  (Auditoría de acciones realizadas) 

    Descripción:
        Almacena el historial de acciones realizadas por los usuarios dentro
        del sistema SGDE.

        Cada registro identifica al usuario que ejecutó la acción, el módulo
        afectado, el tipo de operación realizada y el registro relacionado.

        Esta tabla permite conservar trazabilidad sobre operaciones importantes
        como creación, modificación, eliminación, aprobación, distribución,
        facturación e inicio de sesión.

        La relación con el usuario se agregará posteriormente mediante una
        llave foránea.

==============================================================================*/

CREATE TABLE dbo.tblAudit
(
    AuditId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    UserId INT NULL,
    Action NVARCHAR(50) NOT NULL,
    EntityName NVARCHAR(100) NOT NULL, -- indica la entidad o módulo afectado
    EntityId INT NULL, --identificador del registro afectado
    Description NVARCHAR(500) NOT NULL,
    IpAddress NVARCHAR(50) NULL, -- dirección IP desde donde se realizó la operación
    CreatedAt DATETIME NOT NULL
);
GO

/*==============================================================================
    TABLA: tblEnergyGenerations  (Generación de energía) 

    Descripción:
        Almacena el historial de generación de energía de las turbinas dentro
        del sistema SGDE. Cada registro representa un evento de generación,
        incluyendo la energía generada, la velocidad del viento y la fecha
        y hora en que ocurrió la generación.       

==============================================================================*/

CREATE TABLE [dbo].[tblEnergyGenerations](
	[EnergyGenerationId] [int] IDENTITY(1,1) NOT NULL,
	[TurbineId] [int] NOT NULL,
	[GeneratedMWh] [decimal](18, 4) NOT NULL,
	[WindSpeedMs] [decimal](18, 4) NOT NULL,
	[GeneratedAt] [datetime] NOT NULL,
	[CreatedAt] [datetime] NOT NULL
    );
GO

/*==============================================================================
    TABLA: tblEnergyLosses (Pérdidas de energía)

    Descripción:
        Almacena el historial de pérdidas de energía ocurridas durante la
        operación de las turbinas dentro del sistema SGDE. Cada registro
        representa un evento de pérdida, incluyendo la cantidad de energía
        perdida, el motivo de la pérdida y la fecha y hora en que ocurrió.
        Opcionalmente, permite asociar la pérdida con una batería específica.

==============================================================================*/


CREATE TABLE [dbo].[tblEnergyLosses](
	[EnergyLossId] [int] IDENTITY(1,1) NOT NULL,
	[TurbineId] [int] NOT NULL,
	[LostMWh] [decimal](18, 4) NOT NULL,
	[Reason] [nvarchar](250) NOT NULL,
	[OccurredAt] [datetime] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[BatteryId] [int] NULL,
 CONSTRAINT [PK_tblEnergyLosses] PRIMARY KEY CLUSTERED 
(
	[EnergyLossId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
