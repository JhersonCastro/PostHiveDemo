/*CREACION DE LAS TABLAS*/

/*
CREATE TABLE report_reasons (
    ReportReasonId INT NOT NULL PRIMARY KEY CLUSTERED,
    ReportReason NVARCHAR(128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Description NVARCHAR(512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);

CREATE TABLE report (
    ReportId INT NOT NULL PRIMARY KEY CLUSTERED,
    PostId INT NOT NULL,
    ReportDate DATETIME2(7) NOT NULL,
    ReportReasonId INT NOT NULL,
    Description NVARCHAR(512) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT FK_report_posts FOREIGN KEY (PostId) REFERENCES posts(PostId),
    CONSTRAINT FK_report_reasons FOREIGN KEY (ReportReasonId) REFERENCES report_reasons(ReportReasonId)
);

-- Índices no clusterizados
CREATE INDEX IX_REPORT_PostId ON report(PostId);
CREATE INDEX IX_REPORT_ReportReasonId ON report(ReportReasonId);

CREATE TABLE relationship (
    RelationshipKeyId INT NOT NULL PRIMARY KEY CLUSTERED,
    UserId INT NOT NULL,
    RelationshipUserIdA INT NOT NULL,
    Status INT NOT NULL,
    DateFriendship DATETIME2(7) NOT NULL,
    CONSTRAINT FK_relationship_users FOREIGN KEY (UserId) REFERENCES users(UserId),
    CONSTRAINT FK_relationship_usersA FOREIGN KEY (RelationshipUserIdA) REFERENCES users(UserId)
);

-- Índices no clusterizados
CREATE INDEX IX_RELATIONSHIP_UserId ON relationship(UserId);
CREATE INDEX IX_RELATIONSHIP_RelationshipUserIdA ON relationship(RelationshipUserIdA);


CREATE TABLE post (
    PostId INT NOT NULL PRIMARY KEY CLUSTERED,
    UserId INT NOT NULL,
    Body NVARCHAR(2048) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL,
    Privacy INT NOT NULL,
    CONSTRAINT FK_post_users FOREIGN KEY (UserId) REFERENCES users(UserId)
);

-- Índice no clusterizado en UserId
CREATE INDEX IX_POST_UserId ON post(UserId);


CREATE TABLE notification (
    NotificationId INT NOT NULL PRIMARY KEY CLUSTERED,
    UserId INT NOT NULL,
    Title NVARCHAR(128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Reason NVARCHAR(512) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL,
    IsRead BIT NOT NULL,
    Uri NVARCHAR(2048) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT FK_notification_users FOREIGN KEY (UserId) REFERENCES users(UserId)
);

-- Índice no clusterizado en UserId
CREATE INDEX IX_NOTIFICATION_UserId ON notification(UserId);


CREATE TABLE users (
    UserId INT NOT NULL PRIMARY KEY CLUSTERED,
    Name NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    NickName NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Bio NVARCHAR(256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Avatar NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
);

CREATE TABLE credentials (
    CredentialId INT NOT NULL PRIMARY KEY CLUSTERED,
    Email NVARCHAR(200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Password NVARCHAR(200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    UserId INT NOT NULL,
    CONSTRAINT FK_credentials_users FOREIGN KEY (UserId) REFERENCES db10189.dbo.users(UserId) ON DELETE CASCADE
);

-- Índice no clusterizado en UserId
CREATE INDEX IX_credentials_UserId ON credentials(UserId);

CREATE TABLE files (
    FilesId INT NOT NULL PRIMARY KEY CLUSTERED,
    PostId INT NULL,
    CommentId INT NULL,
    FileType NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    Uri NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT FK_files_posts FOREIGN KEY (PostId) REFERENCES posts(PostId),
    CONSTRAINT FK_files_comments FOREIGN KEY (CommentId) REFERENCES comments(CommentId)
);

-- Índices no clusterizados
CREATE INDEX IX_files_PostId ON files(PostId);
CREATE INDEX IX_files_CommentId ON files(CommentId);


CREATE TABLE ban (
    BanId INT NOT NULL PRIMARY KEY CLUSTERED,
    UserId INT NOT NULL,
    AdminReason NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    BanDuration DATETIME2(7) NOT NULL,
    ReportId INT NOT NULL,
    CONSTRAINT FK_ban_users FOREIGN KEY (UserId) REFERENCES users(UserId),
    CONSTRAINT FK_ban_reports FOREIGN KEY (ReportId) REFERENCES reports(ReportId)
);

CREATE TABLE comments (
    CommentId INT NOT NULL PRIMARY KEY CLUSTERED,
    CommentText NVARCHAR(512) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    UserId INT NOT NULL,
    PostId INT NOT NULL,
    CONSTRAINT FK_comments_users FOREIGN KEY (UserId) REFERENCES users(UserId),
    CONSTRAINT FK_comments_posts FOREIGN KEY (PostId) REFERENCES posts(PostId)
);

-- Índices no clusterizados
CREATE INDEX IX_comments_PostId ON comments(PostId);
CREATE INDEX IX_comments_UserId ON comments(UserId);

CREATE TABLE CookiesResearch (
    Id INT NOT NULL PRIMARY KEY CLUSTERED,
    CookieCurrentSession NVARCHAR(512) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    UserId INT NOT NULL,
    CONSTRAINT FK_CookiesResearch_users FOREIGN KEY (UserId) REFERENCES db10189.dbo.users(UserId) ON DELETE CASCADE
);

-- Índice no clusterizado en UserId
CREATE INDEX IX_CookiesResearch_UserId ON CookiesResearch(UserId);

*/



IF OBJECT_ID('spUserRegister', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE spUserRegister;
END
GO

CREATE PROCEDURE spUserRegister
    @Name NVARCHAR(50),
    @NickName NVARCHAR(50),
    @Email NVARCHAR(100),
    @Password NVARCHAR(100),
    @UserId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;
    BEGIN TRY

        INSERT INTO users
        (
            Name,
            NickName,
            Avatar,
            Bio
        )
        VALUES
        (@Name, @NickName, 'default.png', '')
        SET @UserId = SCOPE_IDENTITY();
        INSERT INTO credentials
        (
            Email,
            Password,
            UserId
        )
        VALUES
        (@Email, @Password, @UserId)

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(3000);

        SELECT @ErrorMessage = ERROR_MESSAGE();

        RAISERROR(@ErrorMessage, 15, 3);

        ROLLBACK TRANSACTION;
    END CATCH;
END
GO

DROP TRIGGER IF EXISTS EmailAlreadyExists;
GO

CREATE TRIGGER EmailAlreadyExists
ON credentials
AFTER INSERT, UPDATE
AS
BEGIN
    DECLARE @Email NVARCHAR(100);

    -- Obtener el correo electrónico del registro que se está insertando o actualizando
    SELECT TOP 1
        @Email = Email
    FROM inserted;

    -- Verificar si el correo electrónico ya existe en la tabla, excluyendo el registro actual
    IF EXISTS
    (
        SELECT 1
        FROM dbo.credentials
        WHERE Email = @Email
              AND CredentialId NOT IN (
                                          SELECT CredentialId FROM inserted
                                      ) -- Excluir el registro actual
    )
    BEGIN
        -- Lanzar un error si el correo ya existe
        RAISERROR('EMAIL: %s ALREADY EXISTS', 16, 1, @Email);
        ROLLBACK TRANSACTION; -- Revertir la transacción
    END
END;
GO

DROP TRIGGER IF EXISTS NickNameAlreadyExists;
GO

CREATE TRIGGER NickNameAlreadyExists
ON users
AFTER INSERT, UPDATE
AS
BEGIN
    DECLARE @NickName NVARCHAR(100);

    -- Obtener el NickName del registro que se está insertando o actualizando
    SELECT TOP 1
        @NickName = NickName
    FROM inserted;

    -- Verificar si el NickName ya existe en la tabla, excluyendo el registro actual
    IF EXISTS
    (
        SELECT 1
        FROM dbo.users
        WHERE NickName = @NickName
              AND UserId NOT IN (
                                    SELECT UserId FROM inserted
                                ) -- Excluir el registro actual
    )
    BEGIN
        -- Lanzar un error si el NickName ya existe
        RAISERROR('NickName: %s ALREADY EXISTS', 16, 1, @NickName);
        ROLLBACK TRANSACTION; -- Revertir la transacción
    END
END;
GO


IF OBJECT_ID('spUpdateAvatar', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE spUpdateAvatar;
END
GO

CREATE PROCEDURE spUpdateAvatar
    @Avatar NVARCHAR(50),
    @UserId INT
AS
BEGIN
    UPDATE users
    SET Avatar = @Avatar
    WHERE UserId = @UserId;
END;
GO

DROP TYPE FilesColecction;
GO

CREATE TYPE FilesColecction AS TABLE
(
    FileType NVARCHAR(100),
    URI NVARCHAR(100)
);
GO
DROP PROCEDURE IF EXISTS INSERTPOST;
GO
CREATE PROCEDURE INSERTPOST
    @CreatedDate DATETIME2,
    @UserId INT,
    @Privacy INT,
    @Body VARCHAR(1024),
    @PostId INT OUTPUT
AS
BEGIN
    INSERT INTO POST
    (
        CreatedDate,
        UserId,
        Privacy,
        Body
    )
    VALUES
    (@CreatedDate, @UserId, @Privacy, @Body);

    SET @PostId = SCOPE_IDENTITY();
END
GO

DROP PROCEDURE IF EXISTS ProcInsertPost;
GO

CREATE PROCEDURE ProcInsertPost
(
    @UserId INT,
    @Body NVARCHAR(1024),
    @CreatedDate DATETIME2,
    @Privacy INT,
    @PostId INT OUTPUT,
    @TotalDur DATETIME2 OUTPUT
)
AS
BEGIN
    -- Inicializo @TotalDur con la fecha de creación
    SET @TotalDur = @CreatedDate;
    DECLARE @BanDurationTemp DATETIME2;

    -- Cursor para obtener las fechas de bans
    DECLARE BanCursor CURSOR FOR
    SELECT CAST(BAN.BanDuration AS DATETIME2)
    FROM BAN
    WHERE BAN.BanDuration > @CreatedDate;

    OPEN BanCursor;
    FETCH NEXT FROM BanCursor INTO @BanDurationTemp;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @TotalDur = DATEADD(HOUR, DATEDIFF(HOUR, @TotalDur, @BanDurationTemp), @TotalDur);
        FETCH NEXT FROM BanCursor INTO @BanDurationTemp;
    END;

    CLOSE BanCursor;
    DEALLOCATE BanCursor;

    -- Si no hay "ban" (TotalDur se mantiene igual o anterior)
    IF (@TotalDur <= @CreatedDate)
    BEGIN
        DECLARE @TempPostId INT;
        -- Llamamos al procedimiento INSERTPOST para insertar el post
        EXEC INSERTPOST @CreatedDate, @UserId, @Privacy, @Body, @TempPostId OUTPUT;
        SET @PostId = @TempPostId;
        -- @TotalDur queda igual que @CreatedDate porque no hubo actualización
    END
    ELSE
    BEGIN
        -- Si se detectó un "ban", devolvemos nulls
        SET @PostId = NULL;
        SET @TotalDur = NULL;
    END;
END;
GO

DROP PROCEDURE IF EXISTS INSERT_POST_FILES;
GO

CREATE PROCEDURE INSERT_POST_FILES
    @UserId INT,
    @Body NVARCHAR(1024),
    @CreatedDate DATETIME2,
    @Privacy INT,
    @Files FilesColecction READONLY,
	@ReturnPostId INT OUTPUT
AS
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @BanDuration DATETIME2;
    DECLARE @PostId INT;

    PRINT 'Inicio del procedimiento INSERT_POST_FILES';

    PRINT 'Llamando al procedimiento ProcInsertPost...';
    DECLARE @TempPostId INT, @TempTotalDur DATETIME2;
    EXEC dbo.ProcInsertPost @UserId, @Body, @CreatedDate, @Privacy, @TempPostId OUTPUT, @TempTotalDur OUTPUT;
    SET @PostId = @TempPostId;
    SET @BanDuration = @TempTotalDur;

    PRINT 'Valores recibidos de ProcInsertPost:';
    PRINT 'PostId: ' + ISNULL(CAST(@PostId AS NVARCHAR), 'NULL');
    PRINT 'BanDuration: ' + ISNULL(CAST(@BanDuration AS NVARCHAR), 'NULL');

    DECLARE @ERRMESSAGE NVARCHAR(50);
    IF (@BanDuration > @CreatedDate)
    BEGIN
        SET @ERRMESSAGE = FORMAT(@BanDuration, 'HH:mm:ss');
        PRINT 'Ban activo detectado. Tiempo restante: ' + @ERRMESSAGE;
    END
    ELSE
    BEGIN
        SET @ERRMESSAGE = 'No duration available';
        PRINT 'Sin duración de ban detectada.';
    END

    IF (@BanDuration > @CreatedDate AND @PostId IS NULL)
    BEGIN
        PRINT 'Error detectado: Ban activo con PostId nulo.';
        RAISERROR('Error, you have a ban, time least: %s', 15, 1, @ERRMESSAGE);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    PRINT 'Insertando archivos en la tabla dbo.files...';
    INSERT INTO dbo.files
    (
        PostId,
        FileType,
        Uri
    )
    SELECT @PostId,
           FileType,
           URI
    FROM @Files;

    PRINT 'Archivos insertados correctamente en dbo.files';
    PRINT 'Fin del procedimiento INSERT_POST_FILES';
	SET @ReturnPostId = @PostId;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    PRINT 'Error detectado: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0
       ROLLBACK TRANSACTION;
    RAISERROR('Error', 16, 1);
END CATCH;
GO



sp_help 'dbo.files';

DECLARE @Files FilesColecction;
DECLARE @PostIdReturn INT;
EXEC INSERT_POST_FILES
    @UserId = 1, -- ID del usuario
    @Body = N'Este es un ejemplo de publicación.', -- Contenido de la publicación
    @CreatedDate = '2025-04-20 21:30:00',
	@Privacy = 0, -- Configuración de privacidad
    @Files =  @Files, -- Colección de archivos
	@ReturnPostId = @PostIdReturn OUTPUT;

PRINT @PostIdReturn
GO
SELECT * FROM dbo.POST
SELECT * FROM dbo.files
DELETE FROM Files
WHERE URI = 'https://example.com/image1.jpg';
DELETE FROM POST
WHERE Body = 'Este es un ejemplo de publicación.';
