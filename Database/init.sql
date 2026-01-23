------------------------------------------------------------
-- Create database if missing
------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CanteenDb')
BEGIN
    CREATE DATABASE [CanteenDb];
    PRINT 'Database CanteenDb created successfully.';
END;
GO

USE CanteenDb;
GO

------------------------------------------------------------
-- MealTypes
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = 'MealTypes' AND type = 'U'
)
BEGIN
    CREATE TABLE dbo.MealTypes (
        MealTypeId TINYINT PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL UNIQUE
    );
    PRINT 'Table MealTypes created successfully.';
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MealTypes)
BEGIN
    INSERT INTO dbo.MealTypes (MealTypeId, Name) VALUES
    (1, 'Breakfast'),
    (2, 'Lunch'),
    (3, 'Dinner'),
    (4, 'Snack'),
    (5, 'Special');
    PRINT 'MealTypes table seeded successfully.';
END;
GO

------------------------------------------------------------
-- PersonRoles
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = 'PersonRoles' AND type = 'U'
)
BEGIN
    CREATE TABLE dbo.PersonRoles (
        RoleId TINYINT PRIMARY KEY,
        RoleName NVARCHAR(50) NOT NULL UNIQUE
    );
    PRINT 'Table PersonRoles created successfully.';
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.PersonRoles)
BEGIN
    INSERT INTO dbo.PersonRoles (RoleId, RoleName) VALUES
    (0, 'Student'),
    (1, 'Teacher'),
    (2, 'Staff'),
    (3, 'Admin');
    PRINT 'PersonRoles table seeded successfully.';
END;
GO

------------------------------------------------------------
-- People
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = 'People' AND type = 'U'
)
BEGIN
    CREATE TABLE dbo.People (
        PersonId INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(255) NOT NULL,
        Barcode NVARCHAR(50) NOT NULL UNIQUE,
        RoleId TINYINT NOT NULL DEFAULT 0,

        CONSTRAINT FK_People_PersonRoles
            FOREIGN KEY (RoleId) REFERENCES dbo.PersonRoles (RoleId)
    );
    PRINT 'Table People created successfully.';
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.People)
BEGIN
    INSERT INTO dbo.People (FullName, Barcode, RoleId) VALUES
    ('John Doe','SOD14090', 1),
    ('Jane Doe','SOD19050', 1);
    PRINT 'People table seeded successfully.';
END;
GO

------------------------------------------------------------
-- CanteenScans
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = 'CanteenScans' AND type = 'U'
)
BEGIN
    CREATE TABLE dbo.CanteenScans (
        CanteenScanId INT IDENTITY(1,1) PRIMARY KEY,
        ScanDate DATETIME2 NOT NULL 
            CONSTRAINT DF_CanteenScans_ScanDate DEFAULT (SYSUTCDATETIME()),
        Barcode NVARCHAR(50),
        MealType TINYINT NOT NULL,

        CONSTRAINT FK_CanteenScans_People
            FOREIGN KEY (Barcode) REFERENCES dbo.People (Barcode)
                ON DELETE SET NULL,

        CONSTRAINT FK_CanteenScans_MealTypes
            FOREIGN KEY (MealType) REFERENCES dbo.MealTypes (MealTypeId)
    );
    PRINT 'Table CanteenScans created successfully.';
END;
GO

------------------------------------------------------------
-- Admins
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = 'Admins' AND type = 'U'
)
BEGIN
    CREATE TABLE Admins (
    AdminId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARBINARY(512) NOT NULL,
    PasswordSalt VARBINARY(128) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastLogin DATETIME2 NULL
);
    PRINT 'Table CanteenScans created successfully.';
END;
GO

------------------------------------------------------------
-- Computed column for date-only uniqueness
------------------------------------------------------------
IF COL_LENGTH('dbo.CanteenScans', 'ScanDay') IS NULL
BEGIN
    ALTER TABLE dbo.CanteenScans
    ADD ScanDay AS CAST(ScanDate AS DATE) PERSISTED;
    PRINT 'Computed column ScanDay added.';
END;
GO

------------------------------------------------------------
-- Unique constraint (Barcode + MealType + ScanDay)
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UQ_CanteenScans_OnePerMealPerDay'
      AND object_id = OBJECT_ID('dbo.CanteenScans')
)
BEGIN
    ALTER TABLE dbo.CanteenScans
    ADD CONSTRAINT UQ_CanteenScans_OnePerMealPerDay
        UNIQUE (Barcode, MealType, ScanDay);
    PRINT 'Unique constraint added.';
END;
GO

------------------------------------------------------------
-- Indexes
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_CanteenScans_Barcode'
      AND object_id = OBJECT_ID('dbo.CanteenScans')
)
BEGIN
    CREATE INDEX IX_CanteenScans_Barcode
    ON dbo.CanteenScans (Barcode);
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_CanteenScans_ScanDate'
      AND object_id = OBJECT_ID('dbo.CanteenScans')
)
BEGIN
    CREATE INDEX IX_CanteenScans_ScanDate
    ON dbo.CanteenScans (ScanDate);
END;
GO

------------------------------------------------------------
-- Stored Procedure: Record Scan
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.procedures WHERE name = 'sp_RecordCanteenScan'
)
BEGIN
EXEC('
CREATE PROCEDURE dbo.sp_RecordCanteenScan
    @Barcode NVARCHAR(50),
    @MealType TINYINT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate barcode
    IF NOT EXISTS (SELECT 1 FROM dbo.People WHERE Barcode = @Barcode)
    BEGIN
        SELECT 
            0 AS Success, 
            ''Invalid Barcode'' AS Message,
            NULL AS PersonName,
            @MealType AS MealType;
        RETURN;
    END

    -- Validate meal type
    IF NOT EXISTS (SELECT 1 FROM dbo.MealTypes WHERE MealTypeId = @MealType)
    BEGIN
        SELECT 
            0 AS Success, 
            ''Invalid Meal Type'' AS Message,
            NULL AS PersonName,
            @MealType AS MealType;
        RETURN;
    END

    DECLARE @PersonName NVARCHAR(200);
    SELECT @PersonName = FullName FROM dbo.People WHERE Barcode = @Barcode;

    BEGIN TRY
        INSERT INTO dbo.CanteenScans (Barcode, MealType)
        VALUES (@Barcode, @MealType);

        SELECT 
            1 AS Success, 
            ''Canteen scan recorded successfully.'' AS Message,
            @PersonName AS PersonName,
            @MealType AS MealType;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() = 2627
            SELECT 
                0 AS Success, 
                ''Already scanned for this meal today.'' AS Message,
                @PersonName AS PersonName,
                @MealType AS MealType;
        ELSE
            SELECT 
                0 AS Success, 
                ''Error: '' + ERROR_MESSAGE() AS Message,
                @PersonName AS PersonName,
                @MealType AS MealType;
    END CATCH
END
');
END;
GO

------------------------------------------------------------
-- Create sp_CreateAdmin if it does not exist
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.procedures WHERE name = 'sp_CreateAdmin'
)
BEGIN
EXEC('
CREATE PROCEDURE dbo.sp_CreateAdmin
    @Username NVARCHAR(100),
    @PasswordHash VARBINARY(512),
    @PasswordSalt VARBINARY(128)
AS
BEGIN
    SET NOCOUNT ON;

    -- Prevent duplicate usernames
    IF EXISTS (SELECT 1 FROM dbo.Admins WHERE Username = @Username)
    BEGIN
        SELECT 0 AS Success, ''Username already exists'' AS Message;
        RETURN;
    END

    INSERT INTO dbo.Admins (Username, PasswordHash, PasswordSalt)
    VALUES (@Username, @PasswordHash, @PasswordSalt);

    SELECT 1 AS Success, ''Admin created successfully'' AS Message;
END
');
END;
GO

------------------------------------------------------------
-- Create sp_GetAdminByUsername if it does not exist
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.procedures WHERE name = 'sp_GetAdminByUsername'
)
BEGIN
EXEC('
CREATE PROCEDURE dbo.sp_GetAdminByUsername
    @Username NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        AdminId,
        Username,
        PasswordHash,
        PasswordSalt
    FROM dbo.Admins
    WHERE Username = @Username;
END
');
END;
GO


SELECT * FROM CanteenScans;

DELETE FROM dbo.CanteenScans;