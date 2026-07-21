-- SP para Activar el Usuario y consumir el Token OTP
CREATE OR ALTER PROCEDURE [dbo].[ACTIVATE_USER_ACCOUNT_PR]
    @P_EMAIL VARCHAR(150),
    @P_TOKEN_CODE VARCHAR(6)
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Actualizar el estado del usuario a Activo
    UPDATE [dbo].[Users]
    SET [Status] = 'Activo',
        [Updated] = GETDATE()
    WHERE [Email] = @P_EMAIL;

    -- 2. Marcar el token como usado (Seguridad de un solo uso RF-AUT-003)
    UPDATE [dbo].[OtpTokens]
    SET [IsUsed] = 1,
        [Updated] = GETDATE()
    WHERE [Email] = @P_EMAIL AND [TokenCode] = @P_TOKEN_CODE;
END
GO