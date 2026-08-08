USE Proyecto2_SEGEDE_Grupo1;
GO

/*==============================================================================
    FOREIGN KEYS: tblMaintenances
==============================================================================*/

/* Relaciona el mantenimiento con la turbina. */
ALTER TABLE dbo.tblMaintenances WITH CHECK
ADD CONSTRAINT FK_tblMaintenances_tblTurbines
FOREIGN KEY (TurbineId)
REFERENCES dbo.tblTurbines (TurbineId);
GO

/* Relaciona el mantenimiento con el ingeniero responsable. */
ALTER TABLE dbo.tblMaintenances WITH CHECK
ADD CONSTRAINT FK_tblMaintenances_tblUsers
FOREIGN KEY (EngineerId)
REFERENCES dbo.tblUsers (UserId);
GO


/*==============================================================================
    FOREIGN KEYS: tblFailures
==============================================================================*/

/* Relaciona la falla con la turbina. */
ALTER TABLE dbo.tblFailures WITH CHECK
ADD CONSTRAINT FK_tblFailures_tblTurbines
FOREIGN KEY (TurbineId)
REFERENCES dbo.tblTurbines (TurbineId);
GO

/* Relaciona la falla con el ingeniero responsable. */
ALTER TABLE dbo.tblFailures WITH CHECK
ADD CONSTRAINT FK_tblFailures_tblUsers
FOREIGN KEY (EngineerId)
REFERENCES dbo.tblUsers (UserId);
GO


/*==============================================================================
    FOREIGN KEYS: tblBatteries
==============================================================================*/

/* Relaciona la batería con la turbina. */
ALTER TABLE dbo.tblBatteries WITH CHECK
ADD CONSTRAINT FK_tblBatteries_tblTurbines
FOREIGN KEY (TurbineId)
REFERENCES dbo.tblTurbines (TurbineId);
GO


/*==============================================================================
    FOREIGN KEYS: tblFlushes
==============================================================================*/

/* Relaciona el vaciado con la turbina. */
ALTER TABLE dbo.tblFlushes WITH CHECK
ADD CONSTRAINT FK_tblFlushes_tblTurbines
FOREIGN KEY (TurbineId)
REFERENCES dbo.tblTurbines (TurbineId);
GO

/* Relaciona el vaciado con la batería. */
ALTER TABLE dbo.tblFlushes WITH CHECK
ADD CONSTRAINT FK_tblFlushes_tblBatteries
FOREIGN KEY (BatteryId)
REFERENCES dbo.tblBatteries (BatteryId);
GO

/* Relaciona el vaciado con el banco central. */
ALTER TABLE dbo.tblFlushes WITH CHECK
ADD CONSTRAINT FK_tblFlushes_tblCentralBank
FOREIGN KEY (CentralBankId)
REFERENCES dbo.tblCentralBank (CentralBankId);
GO


/*==============================================================================
    FOREIGN KEYS: tblForecasts
==============================================================================*/

/* Relaciona la previsión con el comprador. */
ALTER TABLE dbo.tblForecasts WITH CHECK
ADD CONSTRAINT FK_tblForecasts_tblUsers
FOREIGN KEY (BuyerId)
REFERENCES dbo.tblUsers (UserId);
GO


/*==============================================================================
    FOREIGN KEYS: tblDistributions
==============================================================================*/

/* Relaciona la distribución con la previsión. */
ALTER TABLE dbo.tblDistributions WITH CHECK
ADD CONSTRAINT FK_tblDistributions_tblForecasts
FOREIGN KEY (ForecastId)
REFERENCES dbo.tblForecasts (ForecastId);
GO

/* Relaciona la distribución con el comprador. */
ALTER TABLE dbo.tblDistributions WITH CHECK
ADD CONSTRAINT FK_tblDistributions_tblUsers
FOREIGN KEY (BuyerId)
REFERENCES dbo.tblUsers (UserId);
GO

/* Relaciona la distribución con el banco central. */
ALTER TABLE dbo.tblDistributions WITH CHECK
ADD CONSTRAINT FK_tblDistributions_tblCentralBank
FOREIGN KEY (CentralBankId)
REFERENCES dbo.tblCentralBank (CentralBankId);
GO


/*==============================================================================
    FOREIGN KEYS: tblInvoices
==============================================================================*/

/* Relaciona la factura con la distribución. */
ALTER TABLE dbo.tblInvoices WITH CHECK
ADD CONSTRAINT FK_tblInvoices_tblDistributions
FOREIGN KEY (DistributionId)
REFERENCES dbo.tblDistributions (DistributionId);
GO

/* Relaciona la factura con el comprador. */
ALTER TABLE dbo.tblInvoices WITH CHECK
ADD CONSTRAINT FK_tblInvoices_tblUsers
FOREIGN KEY (BuyerId)
REFERENCES dbo.tblUsers (UserId);
GO


/*==============================================================================
    FOREIGN KEYS: tblNotifications
==============================================================================*/

/* Relaciona la notificación con el usuario. */
ALTER TABLE dbo.tblNotifications WITH CHECK
ADD CONSTRAINT FK_tblNotifications_tblUsers
FOREIGN KEY (UserId)
REFERENCES dbo.tblUsers (UserId);
GO


/*==============================================================================
    FOREIGN KEYS: tblAudit
==============================================================================*/

/* Relaciona el registro de auditoría con el usuario. */
ALTER TABLE dbo.tblAudit WITH CHECK
ADD CONSTRAINT FK_tblAudit_tblUsers
FOREIGN KEY (UserId)
REFERENCES dbo.tblUsers (UserId);
GO


-- Clave foránea para la tabla tblCentralBankMovements
/* Relaciona el movimiento con el banco central. */
ALTER TABLE dbo.tblCentralBankMovements WITH CHECK
ADD CONSTRAINT FK_tblCentralBankMovements_tblCentralBank
FOREIGN KEY (CentralBankId)
REFERENCES dbo.tblCentralBank (CentralBankId);
GO