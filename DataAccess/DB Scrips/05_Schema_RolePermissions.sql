/*==============================================================================
    MODULO: Roles y Permisos (Admin/Roles)

    Descripcion:
        Matriz de permisos por rol y modulo, para la pantalla de
        "configuracion detallada de roles y sus respectivos permisos"
        de la rubrica (Administracion del Sistema).

        Alcance: esta tabla persiste que modulos puede ver cada rol y el
        admin puede editarlo desde la UI. La autorizacion real de cada
        endpoint sigue basada en el rol fijo del usuario (Administrator/
        Engineer/Distributor) como ya funciona hoy en toda la aplicacion;
        esta matriz es la capa de configuracion/visibilidad administrable,
        no reemplaza la autorizacion de backend existente.

    Nota: script aditivo, no modifica ninguna tabla ni SP existente.
==============================================================================*/

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tblRolePermissions' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.tblRolePermissions
    (
        RolePermissionId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        RoleName NVARCHAR(50) NOT NULL,
        ModuleKey NVARCHAR(50) NOT NULL,
        ModuleLabel NVARCHAR(100) NOT NULL,
        CanAccess BIT NOT NULL,
        UpdatedAt DATETIME NULL,
        CONSTRAINT UQ_RolePermissions_Role_Module UNIQUE (RoleName, ModuleKey)
    );
END
GO

-- Semilla inicial: refleja el acceso que cada rol ya tiene hoy en la navegacion real
-- (ver _Navbar/_Layout y checkRouteSecurity en site.js), para que la matriz arranque
-- consistente con el comportamiento actual de la aplicacion.
IF NOT EXISTS (SELECT 1 FROM dbo.tblRolePermissions)
BEGIN
    INSERT INTO dbo.tblRolePermissions (RoleName, ModuleKey, ModuleLabel, CanAccess, UpdatedAt) VALUES
    ('Administrator', 'Turbines',       'Turbinas',              1, NULL),
    ('Administrator', 'Maintenances',   'Mantenimientos',        1, NULL),
    ('Administrator', 'Failures',       'Fallas del Sistema',    1, NULL),
    ('Administrator', 'EnergyStorage',  'Control de Caudal',     1, NULL),
    ('Administrator', 'CentralBank',    'Banco Central',         1, NULL),
    ('Administrator', 'Forecasts',      'Pronosticos',           1, NULL),
    ('Administrator', 'Distribution',   'Distribucion',          1, NULL),
    ('Administrator', 'Billing',        'Precios e Impuestos',   1, NULL),
    ('Administrator', 'Statements',     'Estado de Cuentas',     1, NULL),
    ('Administrator', 'Reports',        'Reportes',              1, NULL),
    ('Administrator', 'Audit',          'Bitacora de Auditoria', 1, NULL),
    ('Administrator', 'Users',          'Usuarios',              1, NULL),

    ('Engineer', 'Turbines',       'Turbinas',              1, NULL),
    ('Engineer', 'Maintenances',   'Mantenimientos',        1, NULL),
    ('Engineer', 'Failures',       'Fallas del Sistema',    1, NULL),
    ('Engineer', 'EnergyStorage',  'Control de Caudal',     1, NULL),
    ('Engineer', 'CentralBank',    'Banco Central',         1, NULL),
    ('Engineer', 'Forecasts',      'Pronosticos',           0, NULL),
    ('Engineer', 'Distribution',   'Distribucion',          0, NULL),
    ('Engineer', 'Billing',        'Precios e Impuestos',   0, NULL),
    ('Engineer', 'Statements',     'Estado de Cuentas',     0, NULL),
    ('Engineer', 'Reports',        'Reportes',              1, NULL),
    ('Engineer', 'Audit',          'Bitacora de Auditoria', 1, NULL),
    ('Engineer', 'Users',          'Usuarios',              0, NULL),

    ('Distributor', 'Turbines',       'Turbinas',              0, NULL),
    ('Distributor', 'Maintenances',   'Mantenimientos',        0, NULL),
    ('Distributor', 'Failures',       'Fallas del Sistema',    0, NULL),
    ('Distributor', 'EnergyStorage',  'Control de Caudal',     0, NULL),
    ('Distributor', 'CentralBank',    'Banco Central',         0, NULL),
    ('Distributor', 'Forecasts',      'Pronosticos',           1, NULL),
    ('Distributor', 'Distribution',   'Distribucion',          1, NULL),
    ('Distributor', 'Billing',        'Precios e Impuestos',   0, NULL),
    ('Distributor', 'Statements',     'Estado de Cuentas',     1, NULL),
    ('Distributor', 'Reports',        'Reportes',              1, NULL),
    ('Distributor', 'Audit',          'Bitacora de Auditoria', 0, NULL),
    ('Distributor', 'Users',          'Usuarios',              0, NULL);
END
GO


/*==============================================================================
    PROCEDIMIENTO: RET_ALL_ROLE_PERMISSION_PR
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.RET_ALL_ROLE_PERMISSION_PR
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        RolePermissionId,
        RoleName,
        ModuleKey,
        ModuleLabel,
        CanAccess,
        UpdatedAt
    FROM dbo.tblRolePermissions
    ORDER BY RoleName, ModuleLabel;
END;
GO


/*==============================================================================
    PROCEDIMIENTO: UPD_ROLE_PERMISSION_PR

    Descripcion:
        Actualiza si un rol tiene acceso a un modulo. Usa MERGE para
        insertar la fila si por alguna razon no existiera todavia.
==============================================================================*/

CREATE OR ALTER PROCEDURE dbo.UPD_ROLE_PERMISSION_PR
(
    @RoleName NVARCHAR(50),
    @ModuleKey NVARCHAR(50),
    @ModuleLabel NVARCHAR(100),
    @CanAccess BIT
)
AS
BEGIN
    SET NOCOUNT ON;

    MERGE dbo.tblRolePermissions AS target
    USING (SELECT @RoleName AS RoleName, @ModuleKey AS ModuleKey) AS src
        ON target.RoleName = src.RoleName AND target.ModuleKey = src.ModuleKey
    WHEN MATCHED THEN
        UPDATE SET CanAccess = @CanAccess, UpdatedAt = GETDATE()
    WHEN NOT MATCHED THEN
        INSERT (RoleName, ModuleKey, ModuleLabel, CanAccess, UpdatedAt)
        VALUES (@RoleName, @ModuleKey, @ModuleLabel, @CanAccess, GETDATE());
END;
GO
