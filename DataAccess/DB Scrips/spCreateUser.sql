-- SP para Insertar un Usuario Nuevo
CREATE OR ALTER PROCEDURE [dbo].[CRE_USER_PR]
    @P_USER_CODE VARCHAR(50),
    @P_NAME VARCHAR(100),
    @P_LAST_NAME1 VARCHAR(100),
    @P_LAST_NAME2 VARCHAR(100),
    @P_EMAIL VARCHAR(150),
    @P_PASSWORD VARCHAR(255),
    @P_BIRTH_DATE DATETIME,
    @P_AGE INT,
    @P_STATUS VARCHAR(50),
    @P_PHONE_NUMBER VARCHAR(20),
    @P_PROFILE_PHOTO VARCHAR(MAX),
    @P_ROLE VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [dbo].[Users] (
        [UserCode], [Name], [LastName1], [LastName2], [Email], 
        [Password], [BirthDate], [Age], [Status], [PhoneNumber], 
        [ProfilePhoto], [Role], [Created], [Updated]
    )
    VALUES (
        @P_USER_CODE, @P_NAME, @P_LAST_NAME1, @P_LAST_NAME2, @P_EMAIL, 
        @P_PASSWORD, @P_BIRTH_DATE, @P_AGE, @P_STATUS, @P_PHONE_NUMBER, 
        @P_PROFILE_PHOTO, @P_ROLE, GETDATE(), GETDATE()
    );
END
GO