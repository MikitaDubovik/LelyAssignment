-- Create the database if it doesn't exist
-- Note: Flyway connects to MilkingSystem database, so we need to create it first via init script
-- This script assumes the database already exists (created by init script)

-- Create Animals table
CREATE TABLE Animals (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdentificationNumber NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(100) NULL,
    BirthDate DATE NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Create Robots table
CREATE TABLE Robots (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Location NVARCHAR(200) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Create MilkingEvents table
CREATE TABLE MilkingEvents (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AnimalId INT NOT NULL,
    RobotId INT NOT NULL,
    Timestamp DATETIME2 NOT NULL,
    MilkYieldLiters DECIMAL(10, 2) NOT NULL,
    Duration INT NULL, -- Duration in seconds
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_MilkingEvents_Animals FOREIGN KEY (AnimalId) REFERENCES Animals(Id),
    CONSTRAINT FK_MilkingEvents_Robots FOREIGN KEY (RobotId) REFERENCES Robots(Id)
);

-- Create WeightMeasurements table
CREATE TABLE WeightMeasurements (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AnimalId INT NOT NULL,
    RobotId INT NOT NULL,
    Timestamp DATETIME2 NOT NULL,
    WeightKg DECIMAL(10, 2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_WeightMeasurements_Animals FOREIGN KEY (AnimalId) REFERENCES Animals(Id),
    CONSTRAINT FK_WeightMeasurements_Robots FOREIGN KEY (RobotId) REFERENCES Robots(Id)
);

-- Create indexes for common queries
CREATE INDEX IX_MilkingEvents_AnimalId_Timestamp ON MilkingEvents(AnimalId, Timestamp);
CREATE INDEX IX_MilkingEvents_RobotId ON MilkingEvents(RobotId);
CREATE INDEX IX_WeightMeasurements_AnimalId_Timestamp ON WeightMeasurements(AnimalId, Timestamp);
CREATE INDEX IX_WeightMeasurements_RobotId ON WeightMeasurements(RobotId);
CREATE INDEX IX_Animals_IdentificationNumber ON Animals(IdentificationNumber);
