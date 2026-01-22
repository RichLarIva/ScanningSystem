IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CanteenDb')
BEGIN
    CREATE DATABASE [CanteenDb];
    PRINT 'Database CanteenDb created successfully.';
END;
GO

USE CanteenDb;
GO

IF NOT EXISTS ( 
        SELECT 1 
        FROM sys.tables 
        WHERE name = 'MealTypes' 
        AND type = 'U' 
        ) 
BEGIN 
    CREATE TABLE dbo.MealTypes ( 
        MealTypeId TINYINT PRIMARY KEY, 
        Name NVARCHAR(50) NOT NULL UNIQUE 
    ); 
    
    PRINT 'Table MealTypes created successfully.'; 
END; 
GO

-- Seed Mealtypes if empty
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


IF NOT EXISTS (
        SELECT 1
        FROM sys.tables
        WHERE name = 'CanteenScans'
            AND type = 'U'
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
-- Add unique constraint to prevent multiple scans per meal/day 
------------------------------------------------------------
ALTER TABLE dbo.CanteenScans
	ADD ScanDay as CAST(ScanDate AS DATE) PERSISTED;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UQ_CanteenScans_OnePerMealPerDay'
        And object_id = OBJECT_ID('dbo.CanteenScans')
)
BEGIN
    ALTER TABLE dbo.CanteenScans
    ADD CONSTRAINT UQ_CanteenScans_OnePerMealPerDay
        UNIQUE (Barcode, MealType, ScanDay);

    PRINT 'Unique constraint UQ_CanteenScans_OnePerMealPerDay added successfully.';
END;
GO

------------------------------------------------------------
-- Create PersonRoles lookup table
------------------------------------------------------------
IF NOT EXISTS (
        SELECT 1
        FROM sys.tables
        WHERE name = 'PersonRoles'
            AND type = 'U'
)
BEGIN
    CREATE TABLE dbo.PersonRoles (
        RoleId TINYINT PRIMARY KEY,
        RoleName NVARCHAR(50) NOT NULL UNIQUE
    );

    PRINT 'Table PersonRoles created successfully.';
END;
GO

-- Index on RoleID for fast filtering
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PersonRoles_RoleId'
        And object_id = OBJECT_ID('dbo.PersonRoles')
)
BEGIN
    CREATE INDEX IX_PersonRoles_RoleId
    ON dbo.PersonRoles (RoleId);

    PRINT 'Index IX_PersonRoles_RoleId created successfully.';
END;

------------------------------------------------------------
-- Seed PersonRoles if empty
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.PersonRoles)
BEGIN
    INSERT INTO dbo.PersonRoles (RoleId, RoleName) VALUES
    (0, 'Student'),
    (1, 'Teacher'),
    (2, 'Staff'),
    (3, 'Admin');

    PRINT 'PersonRoles table seeded successfully.';
END;

------------------------------------------------------------
-- People table creation (if not exists)
------------------------------------------------------------

IF NOT EXISTS (
        SELECT 1
        FROM sys.tables
        WHERE name = 'People'
            AND type = 'U'
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

------------------------------------------------------------
-- Seed People table with sample data
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.People)
BEGIN
    INSERT INTO dbo.People (FullName, Barcode, RoleId) VALUES
    ('John Doe','SOD14090', 1),
    ('Jane Doe','SOD19050', 1);
    PRINT 'People table seeded successfully.';
END;
GO

------------------------------------------------------------
-- Indexes for performance
------------------------------------------------------------

-- Index for fast looksup by Barcode
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_CanteenScans_Barcode'
        And object_id = OBJECT_ID('dbo.CanteenScans')
)
BEGIN
    CREATE INDEX IX_CanteenScans_Barcode
    ON dbo.CanteenScans (Barcode);

    PRINT 'Index IX_CanteenScans_Barcode created successfully.';
END;
GO

-- Index for date-based reporting
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_CanteenScans_ScanDate'
        And object_id = OBJECT_ID('dbo.CanteenScans')
)
BEGIN
    CREATE INDEX IX_CanteenScans_ScanDate
    ON dbo.CanteenScans (ScanDate);

    PRINT 'Index IX_CanteenScans_ScanDate created successfully.';
END;

------------------------------------------------------------
-- Create Stored Procedures for common operations
------------------------------------------------------------

IF NOT EXISTS (
    SELECT 1
    FROM sys.procedures
    WHERE name = 'sp_RecordCanteenScan'
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
            SELECT 0 AS Success, ''Invalid Barcode'' AS Message;
            RETURN;
        END

        -- Validate meal type
        IF NOT EXISTS (SELECT 1 FROM dbo.MealTypes WHERE MealTypeId = @MealType)
        BEGIN
            SELECT 0 AS Success, ''Invalid Meal Type'' AS Message;
            RETURN;
        END
        
        -- Attempt to insert the canteen scan
        BEGIN TRY
            INSERT INTO dbo.CanteenScans (Barcode, MealType)
            VALUES (@Barcode, @MealType);

            SELECT 1 AS Success, ''Canteen scan recorded successfully.'' AS Message;
        END TRY
        BEGIN CATCH
            IF ERROR_NUMBER() = 2627 -- Unique constraint violation
            BEGIN
                SELECT 0 AS Success, ''Already scanned for this meal today.'' AS Message;
            END
            ELSE
            BEGIN
                SELECT 0 AS Success, ''Error occurred: '' + ERROR_MESSAGE() AS Message;
            END
        END CATCH
    END
    ');
END;
GO

------------------------------------------------------------
-- Create Stored Procedures for reporting (i.e., get how many scanned today)
------------------------------------------------------------
