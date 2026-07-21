-- SP para Insertar un Token Nuevo (Mapea con OtpCrudFactory.Create)
CREATE OR ALTER PROCEDURE [dbo].[CRE_OTP_TOKEN_PR]
    @P_EMAIL VARCHAR(150),
    @P_TOKEN_CODE VARCHAR(6),
    @P_EXPIRATION_DATE DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[OtpTokens] (
        [Email], [TokenCode], [ExpirationDate], [IsUsed], [Created], [Updated]
    )
    VALUES (
        @P_EMAIL, @P_TOKEN_CODE, @P_EXPIRATION_DATE, 0, GETDATE(), GETDATE()
    );
END
