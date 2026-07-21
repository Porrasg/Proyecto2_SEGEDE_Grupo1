-- =========================================================================
-- 1. ESTRUCTURA DE LA TABLA DE TOKENS OTP (Soporta RF-AUT-003 y RF-AUT-007)
-- =========================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OtpTokens]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OtpTokens] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Created] DATETIME DEFAULT GETDATE() NOT NULL,
        [Updated] DATETIME DEFAULT GETDATE() NOT NULL,
        [Email] VARCHAR(150) NOT NULL,
        [TokenCode] VARCHAR(6) NOT NULL,        -- OTP estrictamente de 6 dígitos numéricos
        [ExpirationDate] DATETIME NOT NULL,     -- Fecha límite de vigencia (Calculada en C#)
        [IsUsed] BIT DEFAULT 0 NOT NULL,         -- Bandera de seguridad de un solo uso
        CONSTRAINT [PK_OtpTokens] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO