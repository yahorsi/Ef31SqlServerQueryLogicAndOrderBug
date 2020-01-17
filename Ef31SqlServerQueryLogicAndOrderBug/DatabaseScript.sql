USE [master]
GO

DROP DATABASE IF EXISTS Ef31SqlServerQueryLogicAndOrderBug
GO

CREATE DATABASE Ef31SqlServerQueryLogicAndOrderBug
GO 

USE Ef31SqlServerQueryLogicAndOrderBug
GO

CREATE TABLE Keyboard
(
    [Id] INT NOT NULL,
	[Value] NVARCHAR(MAX),
    CONSTRAINT [PK_Keyboard] PRIMARY KEY CLUSTERED ([Id] ASC)

)
GO

CREATE TABLE Button
(
    [Id] INT NOT NULL PRIMARY KEY,
    [KeyboardId] INT NOT NULL,
	[Value] NVARCHAR(MAX),
    CONSTRAINT [FK_Keyboard_Id] FOREIGN KEY ([KeyboardId]) REFERENCES [Keyboard]([Id]) ON DELETE CASCADE
)
GO