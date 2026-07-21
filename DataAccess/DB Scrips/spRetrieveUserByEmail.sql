-- SP para Buscar Usuario por Email
CREATE OR ALTER PROCEDURE [dbo].[RET_USER_BY_EMAIL_PR]
    @P_EMAIL VARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        [Id], [Created], [Updated], [UserCode], [Name], [LastName1], 
        [LastName2], [Email], [Password], [BirthDate], [Age], 
        [Status], [PhoneNumber], [ProfilePhoto], [Role]
    FROM [dbo].[Users]
    WHERE [Email] = @P_EMAIL;
END
GO