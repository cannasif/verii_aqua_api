IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE TABLE [RII_PASSWORD_RESET_REQUEST] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] bigint NOT NULL,
        [TokenHash] nvarchar(2000) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [UsedAt] datetime2 NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_PASSWORD_RESET_REQUEST] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE TABLE [RII_PERMISSION_DEFINITIONS] (
        [Id] bigint NOT NULL IDENTITY,
        [Code] nvarchar(120) NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_PERMISSION_DEFINITIONS] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE TABLE [RII_PERMISSION_GROUP_PERMISSIONS] (
        [Id] bigint NOT NULL IDENTITY,
        [PermissionGroupId] bigint NOT NULL,
        [PermissionDefinitionId] bigint NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_PERMISSION_GROUP_PERMISSIONS] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_PERMISSION_GROUP_PERMISSIONS_RII_PERMISSION_DEFINITIONS_PermissionDefinitionId] FOREIGN KEY ([PermissionDefinitionId]) REFERENCES [RII_PERMISSION_DEFINITIONS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE TABLE [RII_PERMISSION_GROUPS] (
        [Id] bigint NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsSystemAdmin] bit NOT NULL DEFAULT CAST(0 AS bit),
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_PERMISSION_GROUPS] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE TABLE [RII_SMTP_SETTING] (
        [Id] bigint NOT NULL IDENTITY,
        [Host] nvarchar(200) NOT NULL,
        [Port] int NOT NULL,
        [EnableSsl] bit NOT NULL,
        [Username] nvarchar(200) NOT NULL,
        [PasswordEncrypted] nvarchar(2000) NOT NULL,
        [FromEmail] nvarchar(200) NOT NULL,
        [FromName] nvarchar(200) NOT NULL,
        [Timeout] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [CreatedByUserId] bigint NULL,
        [UpdatedBy] bigint NULL,
        [UpdatedByUserId] bigint NULL,
        [DeletedBy] bigint NULL,
        [DeletedByUserId] bigint NULL,
        CONSTRAINT [PK_RII_SMTP_SETTING] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE TABLE [RII_STOCK] (
        [Id] bigint NOT NULL IDENTITY,
        [ErpStockCode] nvarchar(50) NOT NULL,
        [StockName] nvarchar(250) NOT NULL,
        [Unit] nvarchar(20) NULL,
        [UreticiKodu] nvarchar(50) NULL,
        [GrupKodu] nvarchar(50) NULL,
        [GrupAdi] nvarchar(250) NULL,
        [Kod1] nvarchar(50) NULL,
        [Kod1Adi] nvarchar(250) NULL,
        [Kod2] nvarchar(50) NULL,
        [Kod2Adi] nvarchar(250) NULL,
        [Kod3] nvarchar(50) NULL,
        [Kod3Adi] nvarchar(250) NULL,
        [Kod4] nvarchar(50) NULL,
        [Kod4Adi] nvarchar(250) NULL,
        [Kod5] nvarchar(50) NULL,
        [Kod5Adi] nvarchar(250) NULL,
        [BranchCode] int NOT NULL DEFAULT 0,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_STOCK] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE TABLE [RII_STOCK_DETAIL] (
        [Id] bigint NOT NULL IDENTITY,
        [StockId] bigint NOT NULL,
        [HtmlDescription] nvarchar(max) NOT NULL,
        [TechnicalSpecsJson] nvarchar(max) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_STOCK_DETAIL] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_STOCK_DETAIL_RII_STOCK_StockId] FOREIGN KEY ([StockId]) REFERENCES [RII_STOCK] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE TABLE [RII_STOCK_IMAGE] (
        [Id] bigint NOT NULL IDENTITY,
        [StockId] bigint NOT NULL,
        [FilePath] nvarchar(500) NOT NULL,
        [AltText] nvarchar(200) NULL,
        [SortOrder] int NOT NULL DEFAULT 0,
        [IsPrimary] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_STOCK_IMAGE] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_STOCK_IMAGE_RII_STOCK_StockId] FOREIGN KEY ([StockId]) REFERENCES [RII_STOCK] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE TABLE [RII_STOCK_RELATION] (
        [Id] bigint NOT NULL IDENTITY,
        [StockId] bigint NOT NULL,
        [RelatedStockId] bigint NOT NULL,
        [Quantity] decimal(18,6) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsMandatory] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_STOCK_RELATION] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_STOCK_RELATION_RII_STOCK_RelatedStockId] FOREIGN KEY ([RelatedStockId]) REFERENCES [RII_STOCK] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RII_STOCK_RELATION_RII_STOCK_StockId] FOREIGN KEY ([StockId]) REFERENCES [RII_STOCK] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE TABLE [RII_USER_AUTHORITY] (
        [Id] bigint NOT NULL IDENTITY,
        [Title] nvarchar(30) NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_USER_AUTHORITY] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE TABLE [RII_USERS] (
        [Id] bigint NOT NULL IDENTITY,
        [Username] nvarchar(50) NOT NULL,
        [Email] nvarchar(100) NOT NULL,
        [PasswordHash] nvarchar(255) NOT NULL,
        [FirstName] nvarchar(50) NULL,
        [LastName] nvarchar(50) NULL,
        [PhoneNumber] nvarchar(20) NULL,
        [RoleId] bigint NOT NULL,
        [IsEmailConfirmed] bit NOT NULL DEFAULT CAST(0 AS bit),
        [LastLoginDate] datetime2 NULL,
        [RefreshToken] nvarchar(500) NULL,
        [RefreshTokenExpiryTime] datetime2 NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_USERS] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_USERS_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_USERS_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_USERS_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_USERS_RII_USER_AUTHORITY_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [RII_USER_AUTHORITY] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE TABLE [RII_USER_DETAIL] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] bigint NOT NULL,
        [ProfilePictureUrl] nvarchar(500) NULL,
        [Height] decimal(5,2) NULL,
        [Weight] decimal(5,2) NULL,
        [Description] nvarchar(2000) NULL,
        [Gender] tinyint NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_USER_DETAIL] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_USER_DETAIL_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_USER_DETAIL_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_USER_DETAIL_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_USER_DETAIL_RII_USERS_UserId] FOREIGN KEY ([UserId]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE TABLE [RII_USER_PERMISSION_GROUPS] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] bigint NOT NULL,
        [PermissionGroupId] bigint NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_USER_PERMISSION_GROUPS] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_USER_PERMISSION_GROUPS_RII_PERMISSION_GROUPS_PermissionGroupId] FOREIGN KEY ([PermissionGroupId]) REFERENCES [RII_PERMISSION_GROUPS] ([Id]),
        CONSTRAINT [FK_RII_USER_PERMISSION_GROUPS_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_USER_PERMISSION_GROUPS_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_USER_PERMISSION_GROUPS_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_USER_PERMISSION_GROUPS_RII_USERS_UserId] FOREIGN KEY ([UserId]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE TABLE [RII_USER_SESSION] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] bigint NOT NULL,
        [SessionId] uniqueidentifier NOT NULL,
        [Token] nvarchar(2000) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [RevokedAt] datetime2 NULL,
        [IpAddress] nvarchar(100) NULL,
        [UserAgent] nvarchar(500) NULL,
        [DeviceInfo] nvarchar(100) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_USER_SESSION] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_USER_SESSION_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_USER_SESSION_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_USER_SESSION_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_USER_SESSION_RII_USERS_UserId] FOREIGN KEY ([UserId]) REFERENCES [RII_USERS] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedBy', N'CreatedDate', N'DeletedBy', N'DeletedDate', N'Description', N'IsActive', N'IsDeleted', N'Name', N'UpdatedBy', N'UpdatedDate') AND [object_id] = OBJECT_ID(N'[RII_PERMISSION_DEFINITIONS]'))
        SET IDENTITY_INSERT [RII_PERMISSION_DEFINITIONS] ON;
    EXEC(N'INSERT INTO [RII_PERMISSION_DEFINITIONS] ([Id], [Code], [CreatedBy], [CreatedDate], [DeletedBy], [DeletedDate], [Description], [IsActive], [IsDeleted], [Name], [UpdatedBy], [UpdatedDate])
    VALUES (CAST(1 AS bigint), N''dashboard.view'', NULL, ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''Dashboard View'', NULL, NULL),
    (CAST(2 AS bigint), N''customers.view'', NULL, ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''Customers View'', NULL, NULL),
    (CAST(3 AS bigint), N''salesmen360.view'', NULL, ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''Salesmen 360 View'', NULL, NULL),
    (CAST(4 AS bigint), N''customer360.view'', NULL, ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''Customer 360 View'', NULL, NULL),
    (CAST(5 AS bigint), N''powerbi.view'', NULL, ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''Power BI View'', NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedBy', N'CreatedDate', N'DeletedBy', N'DeletedDate', N'Description', N'IsActive', N'IsDeleted', N'Name', N'UpdatedBy', N'UpdatedDate') AND [object_id] = OBJECT_ID(N'[RII_PERMISSION_DEFINITIONS]'))
        SET IDENTITY_INSERT [RII_PERMISSION_DEFINITIONS] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedBy', N'CreatedDate', N'DeletedBy', N'DeletedDate', N'Description', N'IsActive', N'IsDeleted', N'IsSystemAdmin', N'Name', N'UpdatedBy', N'UpdatedDate') AND [object_id] = OBJECT_ID(N'[RII_PERMISSION_GROUPS]'))
        SET IDENTITY_INSERT [RII_PERMISSION_GROUPS] ON;
    EXEC(N'INSERT INTO [RII_PERMISSION_GROUPS] ([Id], [CreatedBy], [CreatedDate], [DeletedBy], [DeletedDate], [Description], [IsActive], [IsDeleted], [IsSystemAdmin], [Name], [UpdatedBy], [UpdatedDate])
    VALUES (CAST(1 AS bigint), NULL, ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, N''Full system access'', CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''System Admin'', NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedBy', N'CreatedDate', N'DeletedBy', N'DeletedDate', N'Description', N'IsActive', N'IsDeleted', N'IsSystemAdmin', N'Name', N'UpdatedBy', N'UpdatedDate') AND [object_id] = OBJECT_ID(N'[RII_PERMISSION_GROUPS]'))
        SET IDENTITY_INSERT [RII_PERMISSION_GROUPS] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedBy', N'CreatedDate', N'DeletedBy', N'DeletedDate', N'IsDeleted', N'Title', N'UpdatedBy', N'UpdatedDate') AND [object_id] = OBJECT_ID(N'[RII_USER_AUTHORITY]'))
        SET IDENTITY_INSERT [RII_USER_AUTHORITY] ON;
    EXEC(N'INSERT INTO [RII_USER_AUTHORITY] ([Id], [CreatedBy], [CreatedDate], [DeletedBy], [DeletedDate], [IsDeleted], [Title], [UpdatedBy], [UpdatedDate])
    VALUES (CAST(1 AS bigint), NULL, ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(0 AS bit), N''User'', NULL, NULL),
    (CAST(2 AS bigint), NULL, ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(0 AS bit), N''Manager'', NULL, NULL),
    (CAST(3 AS bigint), NULL, ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(0 AS bit), N''Admin'', NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedBy', N'CreatedDate', N'DeletedBy', N'DeletedDate', N'IsDeleted', N'Title', N'UpdatedBy', N'UpdatedDate') AND [object_id] = OBJECT_ID(N'[RII_USER_AUTHORITY]'))
        SET IDENTITY_INSERT [RII_USER_AUTHORITY] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_PASSWORD_RESET_REQUEST_CreatedBy] ON [RII_PASSWORD_RESET_REQUEST] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_PASSWORD_RESET_REQUEST_DeletedBy] ON [RII_PASSWORD_RESET_REQUEST] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_PASSWORD_RESET_REQUEST_UpdatedBy] ON [RII_PASSWORD_RESET_REQUEST] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_PASSWORD_RESET_REQUEST_UserId] ON [RII_PASSWORD_RESET_REQUEST] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PermissionDefinitions_Code] ON [RII_PERMISSION_DEFINITIONS] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PermissionDefinitions_IsDeleted] ON [RII_PERMISSION_DEFINITIONS] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_PERMISSION_DEFINITIONS_CreatedBy] ON [RII_PERMISSION_DEFINITIONS] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_PERMISSION_DEFINITIONS_DeletedBy] ON [RII_PERMISSION_DEFINITIONS] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_PERMISSION_DEFINITIONS_UpdatedBy] ON [RII_PERMISSION_DEFINITIONS] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PermissionGroupPermission_GroupId_DefinitionId] ON [RII_PERMISSION_GROUP_PERMISSIONS] ([PermissionGroupId], [PermissionDefinitionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PermissionGroupPermission_IsDeleted] ON [RII_PERMISSION_GROUP_PERMISSIONS] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_PERMISSION_GROUP_PERMISSIONS_CreatedBy] ON [RII_PERMISSION_GROUP_PERMISSIONS] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_PERMISSION_GROUP_PERMISSIONS_DeletedBy] ON [RII_PERMISSION_GROUP_PERMISSIONS] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_PERMISSION_GROUP_PERMISSIONS_PermissionDefinitionId] ON [RII_PERMISSION_GROUP_PERMISSIONS] ([PermissionDefinitionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_PERMISSION_GROUP_PERMISSIONS_UpdatedBy] ON [RII_PERMISSION_GROUP_PERMISSIONS] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PermissionGroups_IsDeleted] ON [RII_PERMISSION_GROUPS] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PermissionGroups_Name] ON [RII_PERMISSION_GROUPS] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_PERMISSION_GROUPS_CreatedBy] ON [RII_PERMISSION_GROUPS] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_PERMISSION_GROUPS_DeletedBy] ON [RII_PERMISSION_GROUPS] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_PERMISSION_GROUPS_UpdatedBy] ON [RII_PERMISSION_GROUPS] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_SMTP_SETTING_CreatedByUserId] ON [RII_SMTP_SETTING] ([CreatedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_SMTP_SETTING_DeletedByUserId] ON [RII_SMTP_SETTING] ([DeletedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_SMTP_SETTING_UpdatedByUserId] ON [RII_SMTP_SETTING] ([UpdatedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_STOCK_CreatedBy] ON [RII_STOCK] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_STOCK_DeletedBy] ON [RII_STOCK] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_STOCK_UpdatedBy] ON [RII_STOCK] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Stock_ErpStockCode] ON [RII_STOCK] ([ErpStockCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Stock_IsDeleted] ON [RII_STOCK] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Stock_StockName] ON [RII_STOCK] ([StockName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_STOCK_DETAIL_CreatedBy] ON [RII_STOCK_DETAIL] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_STOCK_DETAIL_DeletedBy] ON [RII_STOCK_DETAIL] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_STOCK_DETAIL_UpdatedBy] ON [RII_STOCK_DETAIL] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockDetail_IsDeleted] ON [RII_STOCK_DETAIL] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StockDetail_StockId] ON [RII_STOCK_DETAIL] ([StockId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_STOCK_IMAGE_CreatedBy] ON [RII_STOCK_IMAGE] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_STOCK_IMAGE_DeletedBy] ON [RII_STOCK_IMAGE] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_STOCK_IMAGE_UpdatedBy] ON [RII_STOCK_IMAGE] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockImage_IsDeleted] ON [RII_STOCK_IMAGE] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockImage_StockId] ON [RII_STOCK_IMAGE] ([StockId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_STOCK_RELATION_CreatedBy] ON [RII_STOCK_RELATION] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_STOCK_RELATION_DeletedBy] ON [RII_STOCK_RELATION] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_STOCK_RELATION_UpdatedBy] ON [RII_STOCK_RELATION] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockRelation_IsDeleted] ON [RII_STOCK_RELATION] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockRelation_RelatedStockId] ON [RII_STOCK_RELATION] ([RelatedStockId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockRelation_StockId] ON [RII_STOCK_RELATION] ([StockId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockRelation_StockId_RelatedStockId] ON [RII_STOCK_RELATION] ([StockId], [RelatedStockId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_USER_AUTHORITY_CreatedBy] ON [RII_USER_AUTHORITY] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_USER_AUTHORITY_DeletedBy] ON [RII_USER_AUTHORITY] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_USER_AUTHORITY_UpdatedBy] ON [RII_USER_AUTHORITY] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserAuthority_IsDeleted] ON [RII_USER_AUTHORITY] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserAuthority_Title] ON [RII_USER_AUTHORITY] ([Title]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_USER_DETAIL_CreatedBy] ON [RII_USER_DETAIL] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_USER_DETAIL_DeletedBy] ON [RII_USER_DETAIL] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_USER_DETAIL_UpdatedBy] ON [RII_USER_DETAIL] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserDetail_IsDeleted] ON [RII_USER_DETAIL] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserDetail_UserId] ON [RII_USER_DETAIL] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_USER_PERMISSION_GROUPS_CreatedBy] ON [RII_USER_PERMISSION_GROUPS] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_USER_PERMISSION_GROUPS_DeletedBy] ON [RII_USER_PERMISSION_GROUPS] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_USER_PERMISSION_GROUPS_PermissionGroupId] ON [RII_USER_PERMISSION_GROUPS] ([PermissionGroupId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_USER_PERMISSION_GROUPS_UpdatedBy] ON [RII_USER_PERMISSION_GROUPS] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserPermissionGroup_IsDeleted] ON [RII_USER_PERMISSION_GROUPS] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserPermissionGroup_UserId_GroupId] ON [RII_USER_PERMISSION_GROUPS] ([UserId], [PermissionGroupId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_USER_SESSION_CreatedBy] ON [RII_USER_SESSION] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_USER_SESSION_DeletedBy] ON [RII_USER_SESSION] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_USER_SESSION_UpdatedBy] ON [RII_USER_SESSION] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserSession_IsDeleted] ON [RII_USER_SESSION] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserSession_RevokedAt] ON [RII_USER_SESSION] ([RevokedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserSession_SessionId] ON [RII_USER_SESSION] ([SessionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserSession_UserId] ON [RII_USER_SESSION] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_USERS_CreatedBy] ON [RII_USERS] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_USERS_DeletedBy] ON [RII_USERS] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_USERS_RoleId] ON [RII_USERS] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RII_USERS_UpdatedBy] ON [RII_USERS] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [RII_USERS] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_IsDeleted] ON [RII_USERS] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Username] ON [RII_USERS] ([Username]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_PASSWORD_RESET_REQUEST] ADD CONSTRAINT [FK_RII_PASSWORD_RESET_REQUEST_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_PASSWORD_RESET_REQUEST] ADD CONSTRAINT [FK_RII_PASSWORD_RESET_REQUEST_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_PASSWORD_RESET_REQUEST] ADD CONSTRAINT [FK_RII_PASSWORD_RESET_REQUEST_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_PASSWORD_RESET_REQUEST] ADD CONSTRAINT [FK_RII_PASSWORD_RESET_REQUEST_RII_USERS_UserId] FOREIGN KEY ([UserId]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_PERMISSION_DEFINITIONS] ADD CONSTRAINT [FK_RII_PERMISSION_DEFINITIONS_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_PERMISSION_DEFINITIONS] ADD CONSTRAINT [FK_RII_PERMISSION_DEFINITIONS_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_PERMISSION_DEFINITIONS] ADD CONSTRAINT [FK_RII_PERMISSION_DEFINITIONS_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_PERMISSION_GROUP_PERMISSIONS] ADD CONSTRAINT [FK_RII_PERMISSION_GROUP_PERMISSIONS_RII_PERMISSION_GROUPS_PermissionGroupId] FOREIGN KEY ([PermissionGroupId]) REFERENCES [RII_PERMISSION_GROUPS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_PERMISSION_GROUP_PERMISSIONS] ADD CONSTRAINT [FK_RII_PERMISSION_GROUP_PERMISSIONS_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_PERMISSION_GROUP_PERMISSIONS] ADD CONSTRAINT [FK_RII_PERMISSION_GROUP_PERMISSIONS_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_PERMISSION_GROUP_PERMISSIONS] ADD CONSTRAINT [FK_RII_PERMISSION_GROUP_PERMISSIONS_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_PERMISSION_GROUPS] ADD CONSTRAINT [FK_RII_PERMISSION_GROUPS_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_PERMISSION_GROUPS] ADD CONSTRAINT [FK_RII_PERMISSION_GROUPS_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_PERMISSION_GROUPS] ADD CONSTRAINT [FK_RII_PERMISSION_GROUPS_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_SMTP_SETTING] ADD CONSTRAINT [FK_RII_SMTP_SETTING_RII_USERS_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_SMTP_SETTING] ADD CONSTRAINT [FK_RII_SMTP_SETTING_RII_USERS_DeletedByUserId] FOREIGN KEY ([DeletedByUserId]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_SMTP_SETTING] ADD CONSTRAINT [FK_RII_SMTP_SETTING_RII_USERS_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_STOCK] ADD CONSTRAINT [FK_RII_STOCK_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_STOCK] ADD CONSTRAINT [FK_RII_STOCK_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_STOCK] ADD CONSTRAINT [FK_RII_STOCK_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_STOCK_DETAIL] ADD CONSTRAINT [FK_RII_STOCK_DETAIL_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_STOCK_DETAIL] ADD CONSTRAINT [FK_RII_STOCK_DETAIL_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_STOCK_DETAIL] ADD CONSTRAINT [FK_RII_STOCK_DETAIL_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_STOCK_IMAGE] ADD CONSTRAINT [FK_RII_STOCK_IMAGE_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_STOCK_IMAGE] ADD CONSTRAINT [FK_RII_STOCK_IMAGE_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_STOCK_IMAGE] ADD CONSTRAINT [FK_RII_STOCK_IMAGE_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_STOCK_RELATION] ADD CONSTRAINT [FK_RII_STOCK_RELATION_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_STOCK_RELATION] ADD CONSTRAINT [FK_RII_STOCK_RELATION_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_STOCK_RELATION] ADD CONSTRAINT [FK_RII_STOCK_RELATION_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_USER_AUTHORITY] ADD CONSTRAINT [FK_RII_USER_AUTHORITY_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_USER_AUTHORITY] ADD CONSTRAINT [FK_RII_USER_AUTHORITY_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    ALTER TABLE [RII_USER_AUTHORITY] ADD CONSTRAINT [FK_RII_USER_AUTHORITY_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219073237_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260219073237_InitialCreate', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_Cage] (
        [Id] bigint NOT NULL IDENTITY,
        [CageCode] nvarchar(50) NOT NULL,
        [CageName] nvarchar(200) NOT NULL,
        [CapacityCount] int NULL,
        [CapacityGram] decimal(18,6) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_Cage] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_Cage_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_Cage_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_Cage_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_NetOperationType] (
        [Id] bigint NOT NULL IDENTITY,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_NetOperationType] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_NetOperationType_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_NetOperationType_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_NetOperationType_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_Project] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectCode] nvarchar(50) NOT NULL,
        [ProjectName] nvarchar(200) NOT NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NULL,
        [Status] tinyint NOT NULL,
        [Note] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_Project] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_Project_Status] CHECK ([Status] IN (0,1,2)),
        CONSTRAINT [FK_RII_Project_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_Project_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_Project_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_WeatherSeverity] (
        [Id] bigint NOT NULL IDENTITY,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Score] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_WeatherSeverity] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_WeatherSeverity_Score] CHECK ([Score] >= 0),
        CONSTRAINT [FK_RII_WeatherSeverity_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_WeatherSeverity_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_WeatherSeverity_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_WeatherType] (
        [Id] bigint NOT NULL IDENTITY,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_WeatherType] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_WeatherType_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_WeatherType_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_WeatherType_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_Feeding] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectId] bigint NOT NULL,
        [FeedingNo] nvarchar(50) NOT NULL,
        [FeedingDate] datetime2(3) NOT NULL,
        [FeedingSlot] tinyint NOT NULL,
        [SourceType] tinyint NOT NULL,
        [Status] tinyint NOT NULL,
        [Note] nvarchar(500) NULL,
        [FeedingDateOnly] AS CAST([FeedingDate] AS date) PERSISTED,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_Feeding] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_Feeding_Slot] CHECK ([FeedingSlot] IN (0,1)),
        CONSTRAINT [CK_RII_Feeding_Status] CHECK ([Status] IN (0,1,2)),
        CONSTRAINT [FK_RII_Feeding_RII_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_Feeding_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_Feeding_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_Feeding_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_GoodsReceipt] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectId] bigint NULL,
        [ReceiptNo] nvarchar(50) NOT NULL,
        [ReceiptDate] datetime2(3) NOT NULL,
        [Status] tinyint NOT NULL,
        [SupplierId] bigint NULL,
        [WarehouseId] bigint NULL,
        [Note] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_GoodsReceipt] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_GoodsReceipt_Status] CHECK ([Status] IN (0,1,2)),
        CONSTRAINT [FK_RII_GoodsReceipt_RII_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_GoodsReceipt_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_GoodsReceipt_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_GoodsReceipt_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_Mortality] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectId] bigint NOT NULL,
        [MortalityDate] date NOT NULL,
        [Status] tinyint NOT NULL,
        [Note] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_Mortality] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_Mortality_Status] CHECK ([Status] IN (0,1,2)),
        CONSTRAINT [FK_RII_Mortality_RII_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_Mortality_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_Mortality_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_Mortality_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_NetOperation] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectId] bigint NOT NULL,
        [OperationTypeId] bigint NOT NULL,
        [OperationNo] nvarchar(50) NOT NULL,
        [OperationDate] datetime2(3) NOT NULL,
        [Status] tinyint NOT NULL,
        [Note] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_NetOperation] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_NetOperation_Status] CHECK ([Status] IN (0,1,2)),
        CONSTRAINT [FK_RII_NetOperation_RII_NetOperationType_OperationTypeId] FOREIGN KEY ([OperationTypeId]) REFERENCES [RII_NetOperationType] ([Id]),
        CONSTRAINT [FK_RII_NetOperation_RII_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_NetOperation_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_NetOperation_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_NetOperation_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_ProjectCage] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectId] bigint NOT NULL,
        [CageId] bigint NOT NULL,
        [AssignedDate] datetime2(3) NOT NULL,
        [ReleasedDate] datetime2(3) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_ProjectCage] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_ProjectCage_AssignRelease] CHECK ([ReleasedDate] IS NULL OR [ReleasedDate] >= [AssignedDate]),
        CONSTRAINT [FK_RII_ProjectCage_RII_Cage_CageId] FOREIGN KEY ([CageId]) REFERENCES [RII_Cage] ([Id]),
        CONSTRAINT [FK_RII_ProjectCage_RII_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_ProjectCage_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_ProjectCage_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_ProjectCage_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_StockConvert] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectId] bigint NOT NULL,
        [ConvertNo] nvarchar(50) NOT NULL,
        [ConvertDate] datetime2(3) NOT NULL,
        [Status] tinyint NOT NULL,
        [Note] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_StockConvert] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_StockConvert_Status] CHECK ([Status] IN (0,1,2)),
        CONSTRAINT [FK_RII_StockConvert_RII_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_StockConvert_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_StockConvert_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_StockConvert_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_Transfer] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectId] bigint NOT NULL,
        [TransferNo] nvarchar(50) NOT NULL,
        [TransferDate] datetime2(3) NOT NULL,
        [Status] tinyint NOT NULL,
        [Note] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_Transfer] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_Transfer_Status] CHECK ([Status] IN (0,1,2)),
        CONSTRAINT [FK_RII_Transfer_RII_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_Transfer_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_Transfer_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_Transfer_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_Weighing] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectId] bigint NOT NULL,
        [WeighingNo] nvarchar(50) NOT NULL,
        [WeighingDate] datetime2(3) NOT NULL,
        [Status] tinyint NOT NULL,
        [Note] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_Weighing] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_Weighing_Status] CHECK ([Status] IN (0,1,2)),
        CONSTRAINT [FK_RII_Weighing_RII_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_Weighing_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_Weighing_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_Weighing_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_DailyWeather] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectId] bigint NOT NULL,
        [WeatherDate] date NOT NULL,
        [WeatherTypeId] bigint NOT NULL,
        [WeatherSeverityId] bigint NOT NULL,
        [TemperatureC] decimal(18,6) NULL,
        [WindKnot] decimal(18,6) NULL,
        [Note] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_DailyWeather] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_DailyWeather_RII_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_DailyWeather_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_DailyWeather_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_DailyWeather_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_DailyWeather_RII_WeatherSeverity_WeatherSeverityId] FOREIGN KEY ([WeatherSeverityId]) REFERENCES [RII_WeatherSeverity] ([Id]),
        CONSTRAINT [FK_RII_DailyWeather_RII_WeatherType_WeatherTypeId] FOREIGN KEY ([WeatherTypeId]) REFERENCES [RII_WeatherType] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_FeedingLine] (
        [Id] bigint NOT NULL IDENTITY,
        [FeedingId] bigint NOT NULL,
        [StockId] bigint NOT NULL,
        [QtyUnit] decimal(18,6) NOT NULL,
        [GramPerUnit] decimal(18,6) NOT NULL,
        [TotalGram] decimal(18,6) NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_FeedingLine] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_FeedingLine_Positive] CHECK ([QtyUnit] > 0 AND [GramPerUnit] > 0 AND [TotalGram] > 0),
        CONSTRAINT [FK_RII_FeedingLine_RII_Feeding_FeedingId] FOREIGN KEY ([FeedingId]) REFERENCES [RII_Feeding] ([Id]),
        CONSTRAINT [FK_RII_FeedingLine_RII_STOCK_StockId] FOREIGN KEY ([StockId]) REFERENCES [RII_STOCK] ([Id]),
        CONSTRAINT [FK_RII_FeedingLine_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_FeedingLine_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_FeedingLine_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_BatchCageBalance] (
        [Id] bigint NOT NULL IDENTITY,
        [FishBatchId] bigint NOT NULL,
        [ProjectCageId] bigint NOT NULL,
        [LiveCount] int NOT NULL,
        [AverageGram] decimal(18,6) NOT NULL,
        [BiomassGram] decimal(18,6) NOT NULL,
        [AsOfDate] datetime2(3) NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_BatchCageBalance] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_BatchCageBalance_NonNegative] CHECK ([LiveCount] >= 0 AND [AverageGram] >= 0 AND [BiomassGram] >= 0),
        CONSTRAINT [FK_RII_BatchCageBalance_RII_ProjectCage_ProjectCageId] FOREIGN KEY ([ProjectCageId]) REFERENCES [RII_ProjectCage] ([Id]),
        CONSTRAINT [FK_RII_BatchCageBalance_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_BatchCageBalance_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_BatchCageBalance_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_BatchMovement] (
        [Id] bigint NOT NULL IDENTITY,
        [FishBatchId] bigint NOT NULL,
        [ProjectCageId] bigint NULL,
        [MovementDate] datetime2(3) NOT NULL,
        [MovementType] tinyint NOT NULL,
        [SignedCount] int NOT NULL,
        [SignedBiomassGram] decimal(18,6) NOT NULL,
        [ReferenceTable] nvarchar(50) NOT NULL,
        [ReferenceId] bigint NOT NULL,
        [Note] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_BatchMovement] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_BatchMovement_MovementType] CHECK ([MovementType] IN (0,1,2,3,4,5)),
        CONSTRAINT [FK_RII_BatchMovement_RII_ProjectCage_ProjectCageId] FOREIGN KEY ([ProjectCageId]) REFERENCES [RII_ProjectCage] ([Id]),
        CONSTRAINT [FK_RII_BatchMovement_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_BatchMovement_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_BatchMovement_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_FeedingDistribution] (
        [Id] bigint NOT NULL IDENTITY,
        [FeedingLineId] bigint NOT NULL,
        [FishBatchId] bigint NOT NULL,
        [ProjectCageId] bigint NOT NULL,
        [FeedGram] decimal(18,6) NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_FeedingDistribution] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_FeedingDistribution_FeedGram] CHECK ([FeedGram] > 0),
        CONSTRAINT [FK_RII_FeedingDistribution_RII_FeedingLine_FeedingLineId] FOREIGN KEY ([FeedingLineId]) REFERENCES [RII_FeedingLine] ([Id]),
        CONSTRAINT [FK_RII_FeedingDistribution_RII_ProjectCage_ProjectCageId] FOREIGN KEY ([ProjectCageId]) REFERENCES [RII_ProjectCage] ([Id]),
        CONSTRAINT [FK_RII_FeedingDistribution_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_FeedingDistribution_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_FeedingDistribution_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_FishBatch] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectId] bigint NOT NULL,
        [BatchCode] nvarchar(50) NOT NULL,
        [FishStockId] bigint NOT NULL,
        [CurrentAverageGram] decimal(18,6) NOT NULL,
        [StartDate] datetime2(3) NOT NULL,
        [SourceGoodsReceiptLineId] bigint NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_FishBatch] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_FishBatch_CurrentAverageGram] CHECK ([CurrentAverageGram] > 0),
        CONSTRAINT [FK_RII_FishBatch_RII_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_FishBatch_RII_STOCK_FishStockId] FOREIGN KEY ([FishStockId]) REFERENCES [RII_STOCK] ([Id]),
        CONSTRAINT [FK_RII_FishBatch_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_FishBatch_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_FishBatch_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_GoodsReceiptLine] (
        [Id] bigint NOT NULL IDENTITY,
        [GoodsReceiptId] bigint NOT NULL,
        [ItemType] tinyint NOT NULL,
        [StockId] bigint NOT NULL,
        [QtyUnit] decimal(18,6) NULL,
        [GramPerUnit] decimal(18,6) NULL,
        [TotalGram] decimal(18,6) NULL,
        [FishCount] int NULL,
        [FishAverageGram] decimal(18,6) NULL,
        [FishTotalGram] decimal(18,6) NULL,
        [FishBatchId] bigint NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_GoodsReceiptLine] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_GoodsReceiptLine_ItemType] CHECK ([ItemType] IN (0,1)),
        CONSTRAINT [FK_RII_GoodsReceiptLine_RII_FishBatch_FishBatchId] FOREIGN KEY ([FishBatchId]) REFERENCES [RII_FishBatch] ([Id]),
        CONSTRAINT [FK_RII_GoodsReceiptLine_RII_GoodsReceipt_GoodsReceiptId] FOREIGN KEY ([GoodsReceiptId]) REFERENCES [RII_GoodsReceipt] ([Id]),
        CONSTRAINT [FK_RII_GoodsReceiptLine_RII_STOCK_StockId] FOREIGN KEY ([StockId]) REFERENCES [RII_STOCK] ([Id]),
        CONSTRAINT [FK_RII_GoodsReceiptLine_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_GoodsReceiptLine_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_GoodsReceiptLine_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_MortalityLine] (
        [Id] bigint NOT NULL IDENTITY,
        [MortalityId] bigint NOT NULL,
        [FishBatchId] bigint NOT NULL,
        [ProjectCageId] bigint NOT NULL,
        [DeadCount] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_MortalityLine] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_MortalityLine_DeadCount] CHECK ([DeadCount] > 0),
        CONSTRAINT [FK_RII_MortalityLine_RII_FishBatch_FishBatchId] FOREIGN KEY ([FishBatchId]) REFERENCES [RII_FishBatch] ([Id]),
        CONSTRAINT [FK_RII_MortalityLine_RII_Mortality_MortalityId] FOREIGN KEY ([MortalityId]) REFERENCES [RII_Mortality] ([Id]),
        CONSTRAINT [FK_RII_MortalityLine_RII_ProjectCage_ProjectCageId] FOREIGN KEY ([ProjectCageId]) REFERENCES [RII_ProjectCage] ([Id]),
        CONSTRAINT [FK_RII_MortalityLine_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_MortalityLine_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_MortalityLine_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_NetOperationLine] (
        [Id] bigint NOT NULL IDENTITY,
        [NetOperationId] bigint NOT NULL,
        [ProjectCageId] bigint NOT NULL,
        [FishBatchId] bigint NULL,
        [Quantity] decimal(18,6) NOT NULL,
        [UnitGram] decimal(18,6) NULL,
        [Note] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_NetOperationLine] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_NetOperationLine_Quantity] CHECK ([Quantity] > 0),
        CONSTRAINT [FK_RII_NetOperationLine_RII_FishBatch_FishBatchId] FOREIGN KEY ([FishBatchId]) REFERENCES [RII_FishBatch] ([Id]),
        CONSTRAINT [FK_RII_NetOperationLine_RII_NetOperation_NetOperationId] FOREIGN KEY ([NetOperationId]) REFERENCES [RII_NetOperation] ([Id]),
        CONSTRAINT [FK_RII_NetOperationLine_RII_ProjectCage_ProjectCageId] FOREIGN KEY ([ProjectCageId]) REFERENCES [RII_ProjectCage] ([Id]),
        CONSTRAINT [FK_RII_NetOperationLine_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_NetOperationLine_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_NetOperationLine_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_StockConvertLine] (
        [Id] bigint NOT NULL IDENTITY,
        [StockConvertId] bigint NOT NULL,
        [FromFishBatchId] bigint NOT NULL,
        [ToFishBatchId] bigint NOT NULL,
        [FromProjectCageId] bigint NOT NULL,
        [ToProjectCageId] bigint NOT NULL,
        [FishCount] int NOT NULL,
        [AverageGram] decimal(18,6) NOT NULL,
        [BiomassGram] decimal(18,6) NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_StockConvertLine] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_StockConvertLine_Positive] CHECK ([FishCount] > 0 AND [AverageGram] > 0 AND [BiomassGram] > 0),
        CONSTRAINT [FK_RII_StockConvertLine_RII_FishBatch_FromFishBatchId] FOREIGN KEY ([FromFishBatchId]) REFERENCES [RII_FishBatch] ([Id]),
        CONSTRAINT [FK_RII_StockConvertLine_RII_FishBatch_ToFishBatchId] FOREIGN KEY ([ToFishBatchId]) REFERENCES [RII_FishBatch] ([Id]),
        CONSTRAINT [FK_RII_StockConvertLine_RII_ProjectCage_FromProjectCageId] FOREIGN KEY ([FromProjectCageId]) REFERENCES [RII_ProjectCage] ([Id]),
        CONSTRAINT [FK_RII_StockConvertLine_RII_ProjectCage_ToProjectCageId] FOREIGN KEY ([ToProjectCageId]) REFERENCES [RII_ProjectCage] ([Id]),
        CONSTRAINT [FK_RII_StockConvertLine_RII_StockConvert_StockConvertId] FOREIGN KEY ([StockConvertId]) REFERENCES [RII_StockConvert] ([Id]),
        CONSTRAINT [FK_RII_StockConvertLine_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_StockConvertLine_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_StockConvertLine_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_TransferLine] (
        [Id] bigint NOT NULL IDENTITY,
        [TransferId] bigint NOT NULL,
        [FishBatchId] bigint NOT NULL,
        [FromProjectCageId] bigint NOT NULL,
        [ToProjectCageId] bigint NOT NULL,
        [FishCount] int NOT NULL,
        [AverageGram] decimal(18,6) NOT NULL,
        [BiomassGram] decimal(18,6) NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_TransferLine] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_TransferLine_FromToDiff] CHECK ([FromProjectCageId] <> [ToProjectCageId]),
        CONSTRAINT [CK_RII_TransferLine_Positive] CHECK ([FishCount] > 0 AND [AverageGram] > 0 AND [BiomassGram] > 0),
        CONSTRAINT [FK_RII_TransferLine_RII_FishBatch_FishBatchId] FOREIGN KEY ([FishBatchId]) REFERENCES [RII_FishBatch] ([Id]),
        CONSTRAINT [FK_RII_TransferLine_RII_ProjectCage_FromProjectCageId] FOREIGN KEY ([FromProjectCageId]) REFERENCES [RII_ProjectCage] ([Id]),
        CONSTRAINT [FK_RII_TransferLine_RII_ProjectCage_ToProjectCageId] FOREIGN KEY ([ToProjectCageId]) REFERENCES [RII_ProjectCage] ([Id]),
        CONSTRAINT [FK_RII_TransferLine_RII_Transfer_TransferId] FOREIGN KEY ([TransferId]) REFERENCES [RII_Transfer] ([Id]),
        CONSTRAINT [FK_RII_TransferLine_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_TransferLine_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_TransferLine_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_WeighingLine] (
        [Id] bigint NOT NULL IDENTITY,
        [WeighingId] bigint NOT NULL,
        [FishBatchId] bigint NOT NULL,
        [ProjectCageId] bigint NOT NULL,
        [MeasuredCount] int NOT NULL,
        [MeasuredAverageGram] decimal(18,6) NOT NULL,
        [MeasuredBiomassGram] decimal(18,6) NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_WeighingLine] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_WeighingLine_Positive] CHECK ([MeasuredCount] > 0 AND [MeasuredAverageGram] > 0 AND [MeasuredBiomassGram] > 0),
        CONSTRAINT [FK_RII_WeighingLine_RII_FishBatch_FishBatchId] FOREIGN KEY ([FishBatchId]) REFERENCES [RII_FishBatch] ([Id]),
        CONSTRAINT [FK_RII_WeighingLine_RII_ProjectCage_ProjectCageId] FOREIGN KEY ([ProjectCageId]) REFERENCES [RII_ProjectCage] ([Id]),
        CONSTRAINT [FK_RII_WeighingLine_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_WeighingLine_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_WeighingLine_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_WeighingLine_RII_Weighing_WeighingId] FOREIGN KEY ([WeighingId]) REFERENCES [RII_Weighing] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE TABLE [RII_GoodsReceiptFishDistribution] (
        [Id] bigint NOT NULL IDENTITY,
        [GoodsReceiptLineId] bigint NOT NULL,
        [ProjectCageId] bigint NOT NULL,
        [FishBatchId] bigint NOT NULL,
        [FishCount] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_GoodsReceiptFishDistribution] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_GoodsReceiptFishDistribution_Count] CHECK ([FishCount] > 0),
        CONSTRAINT [FK_RII_GoodsReceiptFishDistribution_RII_FishBatch_FishBatchId] FOREIGN KEY ([FishBatchId]) REFERENCES [RII_FishBatch] ([Id]),
        CONSTRAINT [FK_RII_GoodsReceiptFishDistribution_RII_GoodsReceiptLine_GoodsReceiptLineId] FOREIGN KEY ([GoodsReceiptLineId]) REFERENCES [RII_GoodsReceiptLine] ([Id]),
        CONSTRAINT [FK_RII_GoodsReceiptFishDistribution_RII_ProjectCage_ProjectCageId] FOREIGN KEY ([ProjectCageId]) REFERENCES [RII_ProjectCage] ([Id]),
        CONSTRAINT [FK_RII_GoodsReceiptFishDistribution_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_GoodsReceiptFishDistribution_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_GoodsReceiptFishDistribution_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_BatchCageBalance_CreatedBy] ON [RII_BatchCageBalance] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_BatchCageBalance_DeletedBy] ON [RII_BatchCageBalance] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_BatchCageBalance_ProjectCageId] ON [RII_BatchCageBalance] ([ProjectCageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_BatchCageBalance_UpdatedBy] ON [RII_BatchCageBalance] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_BatchCageBalance_BatchCage_Active] ON [RII_BatchCageBalance] ([FishBatchId], [ProjectCageId]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_BatchMovement_BatchDate] ON [RII_BatchMovement] ([FishBatchId], [MovementDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_BatchMovement_CreatedBy] ON [RII_BatchMovement] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_BatchMovement_DeletedBy] ON [RII_BatchMovement] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_BatchMovement_ProjectCageId] ON [RII_BatchMovement] ([ProjectCageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_BatchMovement_UpdatedBy] ON [RII_BatchMovement] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Cage_CreatedBy] ON [RII_Cage] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Cage_DeletedBy] ON [RII_Cage] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Cage_UpdatedBy] ON [RII_Cage] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_Cage_CageCode_Active] ON [RII_Cage] ([CageCode]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_DailyWeather_CreatedBy] ON [RII_DailyWeather] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_DailyWeather_DeletedBy] ON [RII_DailyWeather] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_DailyWeather_UpdatedBy] ON [RII_DailyWeather] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_DailyWeather_WeatherSeverityId] ON [RII_DailyWeather] ([WeatherSeverityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_DailyWeather_WeatherTypeId] ON [RII_DailyWeather] ([WeatherTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_DailyWeather_ProjectDate_Active] ON [RII_DailyWeather] ([ProjectId], [WeatherDate]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Feeding_CreatedBy] ON [RII_Feeding] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Feeding_DeletedBy] ON [RII_Feeding] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Feeding_UpdatedBy] ON [RII_Feeding] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_Feeding_FeedingNo_Active] ON [RII_Feeding] ([FeedingNo]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_Feeding_Project_Date_Slot_Active] ON [RII_Feeding] ([ProjectId], [FeedingDateOnly], [FeedingSlot]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_FeedingDistribution_CreatedBy] ON [RII_FeedingDistribution] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_FeedingDistribution_DeletedBy] ON [RII_FeedingDistribution] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_FeedingDistribution_FeedingLineId] ON [RII_FeedingDistribution] ([FeedingLineId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_FeedingDistribution_FishBatchId] ON [RII_FeedingDistribution] ([FishBatchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_FeedingDistribution_ProjectCageId] ON [RII_FeedingDistribution] ([ProjectCageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_FeedingDistribution_UpdatedBy] ON [RII_FeedingDistribution] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_FeedingLine_CreatedBy] ON [RII_FeedingLine] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_FeedingLine_DeletedBy] ON [RII_FeedingLine] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_FeedingLine_FeedingId] ON [RII_FeedingLine] ([FeedingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_FeedingLine_StockId] ON [RII_FeedingLine] ([StockId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_FeedingLine_UpdatedBy] ON [RII_FeedingLine] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_FishBatch_CreatedBy] ON [RII_FishBatch] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_FishBatch_DeletedBy] ON [RII_FishBatch] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_FishBatch_FishStockId] ON [RII_FishBatch] ([FishStockId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_FishBatch_SourceGoodsReceiptLineId] ON [RII_FishBatch] ([SourceGoodsReceiptLineId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_FishBatch_UpdatedBy] ON [RII_FishBatch] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_FishBatch_Project_BatchCode_Active] ON [RII_FishBatch] ([ProjectId], [BatchCode]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_GoodsReceipt_CreatedBy] ON [RII_GoodsReceipt] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_GoodsReceipt_DeletedBy] ON [RII_GoodsReceipt] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_GoodsReceipt_ProjectId] ON [RII_GoodsReceipt] ([ProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_GoodsReceipt_UpdatedBy] ON [RII_GoodsReceipt] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_GoodsReceipt_ReceiptNo_Active] ON [RII_GoodsReceipt] ([ReceiptNo]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_GoodsReceiptFishDistribution_CreatedBy] ON [RII_GoodsReceiptFishDistribution] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_GoodsReceiptFishDistribution_DeletedBy] ON [RII_GoodsReceiptFishDistribution] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_GoodsReceiptFishDistribution_FishBatchId] ON [RII_GoodsReceiptFishDistribution] ([FishBatchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_GoodsReceiptFishDistribution_ProjectCageId] ON [RII_GoodsReceiptFishDistribution] ([ProjectCageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_GoodsReceiptFishDistribution_UpdatedBy] ON [RII_GoodsReceiptFishDistribution] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_GoodsReceiptFishDistribution_LineCage_Active] ON [RII_GoodsReceiptFishDistribution] ([GoodsReceiptLineId], [ProjectCageId]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_GoodsReceiptLine_CreatedBy] ON [RII_GoodsReceiptLine] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_GoodsReceiptLine_DeletedBy] ON [RII_GoodsReceiptLine] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_GoodsReceiptLine_FishBatchId] ON [RII_GoodsReceiptLine] ([FishBatchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_GoodsReceiptLine_GoodsReceiptId] ON [RII_GoodsReceiptLine] ([GoodsReceiptId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_GoodsReceiptLine_StockId] ON [RII_GoodsReceiptLine] ([StockId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_GoodsReceiptLine_UpdatedBy] ON [RII_GoodsReceiptLine] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Mortality_CreatedBy] ON [RII_Mortality] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Mortality_DeletedBy] ON [RII_Mortality] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Mortality_UpdatedBy] ON [RII_Mortality] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_Mortality_ProjectDate_Active] ON [RII_Mortality] ([ProjectId], [MortalityDate]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_MortalityLine_CreatedBy] ON [RII_MortalityLine] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_MortalityLine_DeletedBy] ON [RII_MortalityLine] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_MortalityLine_FishBatchId] ON [RII_MortalityLine] ([FishBatchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_MortalityLine_MortalityId] ON [RII_MortalityLine] ([MortalityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_MortalityLine_ProjectCageId] ON [RII_MortalityLine] ([ProjectCageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_MortalityLine_UpdatedBy] ON [RII_MortalityLine] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_NetOperation_CreatedBy] ON [RII_NetOperation] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_NetOperation_DeletedBy] ON [RII_NetOperation] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_NetOperation_OperationTypeId] ON [RII_NetOperation] ([OperationTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_NetOperation_ProjectId] ON [RII_NetOperation] ([ProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_NetOperation_UpdatedBy] ON [RII_NetOperation] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_NetOperation_OperationNo_Active] ON [RII_NetOperation] ([OperationNo]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_NetOperationLine_CreatedBy] ON [RII_NetOperationLine] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_NetOperationLine_DeletedBy] ON [RII_NetOperationLine] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_NetOperationLine_FishBatchId] ON [RII_NetOperationLine] ([FishBatchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_NetOperationLine_NetOperationId] ON [RII_NetOperationLine] ([NetOperationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_NetOperationLine_ProjectCageId] ON [RII_NetOperationLine] ([ProjectCageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_NetOperationLine_UpdatedBy] ON [RII_NetOperationLine] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_NetOperationType_CreatedBy] ON [RII_NetOperationType] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_NetOperationType_DeletedBy] ON [RII_NetOperationType] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_NetOperationType_UpdatedBy] ON [RII_NetOperationType] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_NetOperationType_Code_Active] ON [RII_NetOperationType] ([Code]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Project_CreatedBy] ON [RII_Project] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Project_DeletedBy] ON [RII_Project] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Project_UpdatedBy] ON [RII_Project] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_Project_ProjectCode_Active] ON [RII_Project] ([ProjectCode]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectCage_CreatedBy] ON [RII_ProjectCage] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectCage_DeletedBy] ON [RII_ProjectCage] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectCage_ProjectId] ON [RII_ProjectCage] ([ProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectCage_UpdatedBy] ON [RII_ProjectCage] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_ProjectCage_CageId_ActiveAssignment] ON [RII_ProjectCage] ([CageId]) WHERE [ReleasedDate] IS NULL AND [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_StockConvert_CreatedBy] ON [RII_StockConvert] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_StockConvert_DeletedBy] ON [RII_StockConvert] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_StockConvert_ProjectId] ON [RII_StockConvert] ([ProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_StockConvert_UpdatedBy] ON [RII_StockConvert] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_StockConvert_ConvertNo_Active] ON [RII_StockConvert] ([ConvertNo]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_StockConvertLine_CreatedBy] ON [RII_StockConvertLine] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_StockConvertLine_DeletedBy] ON [RII_StockConvertLine] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_StockConvertLine_FromFishBatchId] ON [RII_StockConvertLine] ([FromFishBatchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_StockConvertLine_FromProjectCageId] ON [RII_StockConvertLine] ([FromProjectCageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_StockConvertLine_StockConvertId] ON [RII_StockConvertLine] ([StockConvertId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_StockConvertLine_ToFishBatchId] ON [RII_StockConvertLine] ([ToFishBatchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_StockConvertLine_ToProjectCageId] ON [RII_StockConvertLine] ([ToProjectCageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_StockConvertLine_UpdatedBy] ON [RII_StockConvertLine] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Transfer_CreatedBy] ON [RII_Transfer] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Transfer_DeletedBy] ON [RII_Transfer] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Transfer_ProjectId] ON [RII_Transfer] ([ProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Transfer_UpdatedBy] ON [RII_Transfer] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_Transfer_TransferNo_Active] ON [RII_Transfer] ([TransferNo]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_TransferLine_CreatedBy] ON [RII_TransferLine] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_TransferLine_DeletedBy] ON [RII_TransferLine] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_TransferLine_FishBatchId] ON [RII_TransferLine] ([FishBatchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_TransferLine_FromProjectCageId] ON [RII_TransferLine] ([FromProjectCageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_TransferLine_ToProjectCageId] ON [RII_TransferLine] ([ToProjectCageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_TransferLine_TransferId] ON [RII_TransferLine] ([TransferId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_TransferLine_UpdatedBy] ON [RII_TransferLine] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_WeatherSeverity_CreatedBy] ON [RII_WeatherSeverity] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_WeatherSeverity_DeletedBy] ON [RII_WeatherSeverity] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_WeatherSeverity_UpdatedBy] ON [RII_WeatherSeverity] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_WeatherSeverity_Code_Active] ON [RII_WeatherSeverity] ([Code]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_WeatherType_CreatedBy] ON [RII_WeatherType] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_WeatherType_DeletedBy] ON [RII_WeatherType] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_WeatherType_UpdatedBy] ON [RII_WeatherType] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_WeatherType_Code_Active] ON [RII_WeatherType] ([Code]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Weighing_CreatedBy] ON [RII_Weighing] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Weighing_DeletedBy] ON [RII_Weighing] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Weighing_ProjectId] ON [RII_Weighing] ([ProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_Weighing_UpdatedBy] ON [RII_Weighing] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_Weighing_WeighingNo_Active] ON [RII_Weighing] ([WeighingNo]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_WeighingLine_CreatedBy] ON [RII_WeighingLine] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_WeighingLine_DeletedBy] ON [RII_WeighingLine] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_WeighingLine_FishBatchId] ON [RII_WeighingLine] ([FishBatchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_WeighingLine_ProjectCageId] ON [RII_WeighingLine] ([ProjectCageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_WeighingLine_UpdatedBy] ON [RII_WeighingLine] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    CREATE INDEX [IX_RII_WeighingLine_WeighingId] ON [RII_WeighingLine] ([WeighingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    ALTER TABLE [RII_BatchCageBalance] ADD CONSTRAINT [FK_RII_BatchCageBalance_RII_FishBatch_FishBatchId] FOREIGN KEY ([FishBatchId]) REFERENCES [RII_FishBatch] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    ALTER TABLE [RII_BatchMovement] ADD CONSTRAINT [FK_RII_BatchMovement_RII_FishBatch_FishBatchId] FOREIGN KEY ([FishBatchId]) REFERENCES [RII_FishBatch] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    ALTER TABLE [RII_FeedingDistribution] ADD CONSTRAINT [FK_RII_FeedingDistribution_RII_FishBatch_FishBatchId] FOREIGN KEY ([FishBatchId]) REFERENCES [RII_FishBatch] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    ALTER TABLE [RII_FishBatch] ADD CONSTRAINT [FK_RII_FishBatch_RII_GoodsReceiptLine_SourceGoodsReceiptLineId] FOREIGN KEY ([SourceGoodsReceiptLineId]) REFERENCES [RII_GoodsReceiptLine] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219105815_AquaModuleInit'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260219105815_AquaModuleInit', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222215052_EnforceSingleGoodsReceiptPerProject'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260222215052_EnforceSingleGoodsReceiptPerProject', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    ALTER TABLE [RII_NetOperationLine] DROP CONSTRAINT [CK_RII_NetOperationLine_Quantity];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    ALTER TABLE [RII_BatchMovement] DROP CONSTRAINT [CK_RII_BatchMovement_MovementType];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_NetOperationLine]') AND [c].[name] = N'Quantity');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [RII_NetOperationLine] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [RII_NetOperationLine] DROP COLUMN [Quantity];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_NetOperationLine]') AND [c].[name] = N'UnitGram');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [RII_NetOperationLine] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [RII_NetOperationLine] DROP COLUMN [UnitGram];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    CREATE TABLE [RII_Shipment] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectId] bigint NOT NULL,
        [ShipmentNo] nvarchar(50) NOT NULL,
        [ShipmentDate] datetime2(3) NOT NULL,
        [TargetWarehouse] nvarchar(100) NULL,
        [Status] tinyint NOT NULL,
        [Note] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_Shipment] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_Shipment_Status] CHECK ([Status] IN (0,1,2)),
        CONSTRAINT [FK_RII_Shipment_RII_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_Shipment_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_Shipment_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_Shipment_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    CREATE TABLE [RII_ShipmentLine] (
        [Id] bigint NOT NULL IDENTITY,
        [ShipmentId] bigint NOT NULL,
        [FishBatchId] bigint NOT NULL,
        [FromProjectCageId] bigint NOT NULL,
        [FishCount] int NOT NULL,
        [AverageGram] decimal(18,6) NOT NULL,
        [BiomassGram] decimal(18,6) NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_ShipmentLine] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_ShipmentLine_Positive] CHECK ([FishCount] > 0 AND [AverageGram] >= 0 AND [BiomassGram] >= 0),
        CONSTRAINT [FK_RII_ShipmentLine_RII_FishBatch_FishBatchId] FOREIGN KEY ([FishBatchId]) REFERENCES [RII_FishBatch] ([Id]),
        CONSTRAINT [FK_RII_ShipmentLine_RII_ProjectCage_FromProjectCageId] FOREIGN KEY ([FromProjectCageId]) REFERENCES [RII_ProjectCage] ([Id]),
        CONSTRAINT [FK_RII_ShipmentLine_RII_Shipment_ShipmentId] FOREIGN KEY ([ShipmentId]) REFERENCES [RII_Shipment] ([Id]),
        CONSTRAINT [FK_RII_ShipmentLine_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_ShipmentLine_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_ShipmentLine_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    EXEC(N'ALTER TABLE [RII_BatchMovement] ADD CONSTRAINT [CK_RII_BatchMovement_MovementType] CHECK ([MovementType] IN (0,1,2,3,4,5,6))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    CREATE INDEX [IX_RII_Shipment_CreatedBy] ON [RII_Shipment] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    CREATE INDEX [IX_RII_Shipment_DeletedBy] ON [RII_Shipment] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    CREATE INDEX [IX_RII_Shipment_ProjectId] ON [RII_Shipment] ([ProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    CREATE INDEX [IX_RII_Shipment_UpdatedBy] ON [RII_Shipment] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_Shipment_ShipmentNo_Active] ON [RII_Shipment] ([ShipmentNo]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    CREATE INDEX [IX_RII_ShipmentLine_CreatedBy] ON [RII_ShipmentLine] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    CREATE INDEX [IX_RII_ShipmentLine_DeletedBy] ON [RII_ShipmentLine] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    CREATE INDEX [IX_RII_ShipmentLine_FishBatchId] ON [RII_ShipmentLine] ([FishBatchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    CREATE INDEX [IX_RII_ShipmentLine_FromProjectCageId] ON [RII_ShipmentLine] ([FromProjectCageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    CREATE INDEX [IX_RII_ShipmentLine_ShipmentId] ON [RII_ShipmentLine] ([ShipmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    CREATE INDEX [IX_RII_ShipmentLine_UpdatedBy] ON [RII_ShipmentLine] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223200845_AddShipmentAndCloseProjectOnFullShipment'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260223200845_AddShipmentAndCloseProjectOnFullShipment', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225105033_AddBatchMovementFeedAndActor'
)
BEGIN
    ALTER TABLE [RII_BatchMovement] DROP CONSTRAINT [CK_RII_BatchMovement_MovementType];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225105033_AddBatchMovementFeedAndActor'
)
BEGIN
    ALTER TABLE [RII_BatchMovement] ADD [ActorUserId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225105033_AddBatchMovementFeedAndActor'
)
BEGIN
    ALTER TABLE [RII_BatchMovement] ADD [FeedGram] decimal(18,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225105033_AddBatchMovementFeedAndActor'
)
BEGIN
    EXEC(N'ALTER TABLE [RII_BatchMovement] ADD CONSTRAINT [CK_RII_BatchMovement_MovementType] CHECK ([MovementType] IN (0,1,2,3,4,5,6,7))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225105033_AddBatchMovementFeedAndActor'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260225105033_AddBatchMovementFeedAndActor', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225132056_AddBatchMovementStockContext'
)
BEGIN
    ALTER TABLE [RII_BatchMovement] ADD [FromAverageGram] decimal(18,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225132056_AddBatchMovementStockContext'
)
BEGIN
    ALTER TABLE [RII_BatchMovement] ADD [FromProjectCageId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225132056_AddBatchMovementStockContext'
)
BEGIN
    ALTER TABLE [RII_BatchMovement] ADD [FromStockId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225132056_AddBatchMovementStockContext'
)
BEGIN
    ALTER TABLE [RII_BatchMovement] ADD [ToAverageGram] decimal(18,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225132056_AddBatchMovementStockContext'
)
BEGIN
    ALTER TABLE [RII_BatchMovement] ADD [ToProjectCageId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225132056_AddBatchMovementStockContext'
)
BEGIN
    ALTER TABLE [RII_BatchMovement] ADD [ToStockId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225132056_AddBatchMovementStockContext'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260225132056_AddBatchMovementStockContext', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225134857_AdjustStockConvertLineGramAsIncrement'
)
BEGIN
    ALTER TABLE [RII_StockConvertLine] DROP CONSTRAINT [CK_RII_StockConvertLine_Positive];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225134857_AdjustStockConvertLineGramAsIncrement'
)
BEGIN
    ALTER TABLE [RII_StockConvertLine] ADD [NewAverageGram] decimal(18,6) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225134857_AdjustStockConvertLineGramAsIncrement'
)
BEGIN
    EXEC(N'ALTER TABLE [RII_StockConvertLine] ADD CONSTRAINT [CK_RII_StockConvertLine_Positive] CHECK ([FishCount] > 0 AND [AverageGram] > 0 AND [NewAverageGram] >= 0 AND [BiomassGram] > 0)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225134857_AdjustStockConvertLineGramAsIncrement'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260225134857_AdjustStockConvertLineGramAsIncrement', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312115533_AddHangfireJobFailureLog'
)
BEGIN
    CREATE TABLE [RII_JOB_FAILURE_LOG] (
        [Id] bigint NOT NULL IDENTITY,
        [JobId] nvarchar(100) NOT NULL,
        [JobName] nvarchar(500) NOT NULL,
        [FailedAt] datetime2 NOT NULL,
        [Reason] nvarchar(2000) NULL,
        [ExceptionType] nvarchar(500) NULL,
        [ExceptionMessage] nvarchar(4000) NULL,
        [StackTrace] nvarchar(4000) NULL,
        [Queue] nvarchar(100) NULL,
        [RetryCount] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_JOB_FAILURE_LOG] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_JOB_FAILURE_LOG_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_JOB_FAILURE_LOG_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_JOB_FAILURE_LOG_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312115533_AddHangfireJobFailureLog'
)
BEGIN
    CREATE INDEX [IX_JobFailureLog_FailedAt] ON [RII_JOB_FAILURE_LOG] ([FailedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312115533_AddHangfireJobFailureLog'
)
BEGIN
    CREATE INDEX [IX_JobFailureLog_JobId] ON [RII_JOB_FAILURE_LOG] ([JobId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312115533_AddHangfireJobFailureLog'
)
BEGIN
    CREATE INDEX [IX_JobFailureLog_JobName] ON [RII_JOB_FAILURE_LOG] ([JobName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312115533_AddHangfireJobFailureLog'
)
BEGIN
    CREATE INDEX [IX_RII_JOB_FAILURE_LOG_CreatedBy] ON [RII_JOB_FAILURE_LOG] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312115533_AddHangfireJobFailureLog'
)
BEGIN
    CREATE INDEX [IX_RII_JOB_FAILURE_LOG_DeletedBy] ON [RII_JOB_FAILURE_LOG] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312115533_AddHangfireJobFailureLog'
)
BEGIN
    CREATE INDEX [IX_RII_JOB_FAILURE_LOG_UpdatedBy] ON [RII_JOB_FAILURE_LOG] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312115533_AddHangfireJobFailureLog'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260312115533_AddHangfireJobFailureLog', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_WeighingLine]') AND [c].[name] = N'CreatedDate');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [RII_WeighingLine] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [RII_WeighingLine] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_Weighing]') AND [c].[name] = N'CreatedDate');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [RII_Weighing] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [RII_Weighing] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_WeatherType]') AND [c].[name] = N'CreatedDate');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [RII_WeatherType] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [RII_WeatherType] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_WeatherSeverity]') AND [c].[name] = N'CreatedDate');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [RII_WeatherSeverity] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [RII_WeatherSeverity] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_USERS]') AND [c].[name] = N'CreatedDate');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [RII_USERS] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [RII_USERS] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_USER_SESSION]') AND [c].[name] = N'CreatedDate');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [RII_USER_SESSION] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [RII_USER_SESSION] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_USER_PERMISSION_GROUPS]') AND [c].[name] = N'CreatedDate');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [RII_USER_PERMISSION_GROUPS] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [RII_USER_PERMISSION_GROUPS] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_USER_DETAIL]') AND [c].[name] = N'CreatedDate');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [RII_USER_DETAIL] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [RII_USER_DETAIL] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_USER_AUTHORITY]') AND [c].[name] = N'CreatedDate');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [RII_USER_AUTHORITY] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [RII_USER_AUTHORITY] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_TransferLine]') AND [c].[name] = N'CreatedDate');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [RII_TransferLine] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [RII_TransferLine] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_Transfer]') AND [c].[name] = N'CreatedDate');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [RII_Transfer] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [RII_Transfer] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_StockConvertLine]') AND [c].[name] = N'CreatedDate');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [RII_StockConvertLine] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [RII_StockConvertLine] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_StockConvert]') AND [c].[name] = N'CreatedDate');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [RII_StockConvert] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [RII_StockConvert] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_STOCK_RELATION]') AND [c].[name] = N'CreatedDate');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [RII_STOCK_RELATION] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [RII_STOCK_RELATION] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_STOCK_IMAGE]') AND [c].[name] = N'CreatedDate');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [RII_STOCK_IMAGE] DROP CONSTRAINT [' + @var16 + '];');
    ALTER TABLE [RII_STOCK_IMAGE] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_STOCK_DETAIL]') AND [c].[name] = N'CreatedDate');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [RII_STOCK_DETAIL] DROP CONSTRAINT [' + @var17 + '];');
    ALTER TABLE [RII_STOCK_DETAIL] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_STOCK]') AND [c].[name] = N'CreatedDate');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [RII_STOCK] DROP CONSTRAINT [' + @var18 + '];');
    ALTER TABLE [RII_STOCK] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var19 sysname;
    SELECT @var19 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_ShipmentLine]') AND [c].[name] = N'CreatedDate');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [RII_ShipmentLine] DROP CONSTRAINT [' + @var19 + '];');
    ALTER TABLE [RII_ShipmentLine] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var20 sysname;
    SELECT @var20 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_Shipment]') AND [c].[name] = N'CreatedDate');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [RII_Shipment] DROP CONSTRAINT [' + @var20 + '];');
    ALTER TABLE [RII_Shipment] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var21 sysname;
    SELECT @var21 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_ProjectCage]') AND [c].[name] = N'CreatedDate');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [RII_ProjectCage] DROP CONSTRAINT [' + @var21 + '];');
    ALTER TABLE [RII_ProjectCage] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var22 sysname;
    SELECT @var22 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_Project]') AND [c].[name] = N'CreatedDate');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [RII_Project] DROP CONSTRAINT [' + @var22 + '];');
    ALTER TABLE [RII_Project] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var23 sysname;
    SELECT @var23 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_PERMISSION_GROUPS]') AND [c].[name] = N'CreatedDate');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [RII_PERMISSION_GROUPS] DROP CONSTRAINT [' + @var23 + '];');
    ALTER TABLE [RII_PERMISSION_GROUPS] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var24 sysname;
    SELECT @var24 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_PERMISSION_GROUP_PERMISSIONS]') AND [c].[name] = N'CreatedDate');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [RII_PERMISSION_GROUP_PERMISSIONS] DROP CONSTRAINT [' + @var24 + '];');
    ALTER TABLE [RII_PERMISSION_GROUP_PERMISSIONS] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var25 sysname;
    SELECT @var25 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_PERMISSION_DEFINITIONS]') AND [c].[name] = N'CreatedDate');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [RII_PERMISSION_DEFINITIONS] DROP CONSTRAINT [' + @var25 + '];');
    ALTER TABLE [RII_PERMISSION_DEFINITIONS] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var26 sysname;
    SELECT @var26 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_PASSWORD_RESET_REQUEST]') AND [c].[name] = N'CreatedDate');
    IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [RII_PASSWORD_RESET_REQUEST] DROP CONSTRAINT [' + @var26 + '];');
    ALTER TABLE [RII_PASSWORD_RESET_REQUEST] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var27 sysname;
    SELECT @var27 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_NetOperationType]') AND [c].[name] = N'CreatedDate');
    IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [RII_NetOperationType] DROP CONSTRAINT [' + @var27 + '];');
    ALTER TABLE [RII_NetOperationType] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var28 sysname;
    SELECT @var28 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_NetOperationLine]') AND [c].[name] = N'CreatedDate');
    IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [RII_NetOperationLine] DROP CONSTRAINT [' + @var28 + '];');
    ALTER TABLE [RII_NetOperationLine] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var29 sysname;
    SELECT @var29 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_NetOperation]') AND [c].[name] = N'CreatedDate');
    IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [RII_NetOperation] DROP CONSTRAINT [' + @var29 + '];');
    ALTER TABLE [RII_NetOperation] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var30 sysname;
    SELECT @var30 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_MortalityLine]') AND [c].[name] = N'CreatedDate');
    IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [RII_MortalityLine] DROP CONSTRAINT [' + @var30 + '];');
    ALTER TABLE [RII_MortalityLine] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var31 sysname;
    SELECT @var31 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_Mortality]') AND [c].[name] = N'CreatedDate');
    IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [RII_Mortality] DROP CONSTRAINT [' + @var31 + '];');
    ALTER TABLE [RII_Mortality] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var32 sysname;
    SELECT @var32 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_JOB_FAILURE_LOG]') AND [c].[name] = N'CreatedDate');
    IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [RII_JOB_FAILURE_LOG] DROP CONSTRAINT [' + @var32 + '];');
    ALTER TABLE [RII_JOB_FAILURE_LOG] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var33 sysname;
    SELECT @var33 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_GoodsReceiptLine]') AND [c].[name] = N'CreatedDate');
    IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [RII_GoodsReceiptLine] DROP CONSTRAINT [' + @var33 + '];');
    ALTER TABLE [RII_GoodsReceiptLine] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var34 sysname;
    SELECT @var34 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_GoodsReceiptFishDistribution]') AND [c].[name] = N'CreatedDate');
    IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [RII_GoodsReceiptFishDistribution] DROP CONSTRAINT [' + @var34 + '];');
    ALTER TABLE [RII_GoodsReceiptFishDistribution] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var35 sysname;
    SELECT @var35 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_GoodsReceipt]') AND [c].[name] = N'CreatedDate');
    IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [RII_GoodsReceipt] DROP CONSTRAINT [' + @var35 + '];');
    ALTER TABLE [RII_GoodsReceipt] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var36 sysname;
    SELECT @var36 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_FishBatch]') AND [c].[name] = N'CreatedDate');
    IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [RII_FishBatch] DROP CONSTRAINT [' + @var36 + '];');
    ALTER TABLE [RII_FishBatch] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var37 sysname;
    SELECT @var37 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_FeedingLine]') AND [c].[name] = N'CreatedDate');
    IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [RII_FeedingLine] DROP CONSTRAINT [' + @var37 + '];');
    ALTER TABLE [RII_FeedingLine] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var38 sysname;
    SELECT @var38 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_FeedingDistribution]') AND [c].[name] = N'CreatedDate');
    IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [RII_FeedingDistribution] DROP CONSTRAINT [' + @var38 + '];');
    ALTER TABLE [RII_FeedingDistribution] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var39 sysname;
    SELECT @var39 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_Feeding]') AND [c].[name] = N'CreatedDate');
    IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [RII_Feeding] DROP CONSTRAINT [' + @var39 + '];');
    ALTER TABLE [RII_Feeding] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var40 sysname;
    SELECT @var40 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_DailyWeather]') AND [c].[name] = N'CreatedDate');
    IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [RII_DailyWeather] DROP CONSTRAINT [' + @var40 + '];');
    ALTER TABLE [RII_DailyWeather] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var41 sysname;
    SELECT @var41 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_Cage]') AND [c].[name] = N'CreatedDate');
    IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [RII_Cage] DROP CONSTRAINT [' + @var41 + '];');
    ALTER TABLE [RII_Cage] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var42 sysname;
    SELECT @var42 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_BatchMovement]') AND [c].[name] = N'CreatedDate');
    IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [RII_BatchMovement] DROP CONSTRAINT [' + @var42 + '];');
    ALTER TABLE [RII_BatchMovement] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    DECLARE @var43 sysname;
    SELECT @var43 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_BatchCageBalance]') AND [c].[name] = N'CreatedDate');
    IF @var43 IS NOT NULL EXEC(N'ALTER TABLE [RII_BatchCageBalance] DROP CONSTRAINT [' + @var43 + '];');
    ALTER TABLE [RII_BatchCageBalance] ADD DEFAULT (GETDATE()) FOR [CreatedDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    CREATE INDEX [IX_Stock_ErpStockCode_BranchCode] ON [RII_STOCK] ([ErpStockCode], [BranchCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312221824_UseLocalTimeForAuditDates'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260312221824_UseLocalTimeForAuditDates', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414110131_AddAquaSettings'
)
BEGIN
    CREATE TABLE [RII_AquaSetting] (
        [Id] bigint NOT NULL IDENTITY,
        [PartialTransferOccupiedCageMode] int NOT NULL DEFAULT 0,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_AquaSetting] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_AquaSetting_PartialTransferOccupiedCageMode] CHECK ([PartialTransferOccupiedCageMode] IN (0,1,2)),
        CONSTRAINT [FK_RII_AquaSetting_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_AquaSetting_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_AquaSetting_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414110131_AddAquaSettings'
)
BEGIN
    CREATE INDEX [IX_RII_AquaSetting_CreatedBy] ON [RII_AquaSetting] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414110131_AddAquaSettings'
)
BEGIN
    CREATE INDEX [IX_RII_AquaSetting_DeletedBy] ON [RII_AquaSetting] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414110131_AddAquaSettings'
)
BEGIN
    CREATE INDEX [IX_RII_AquaSetting_UpdatedBy] ON [RII_AquaSetting] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414110131_AddAquaSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260414110131_AddAquaSettings', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414125103_LinkWeatherSeverityToWeatherType'
)
BEGIN
    DROP INDEX [UX_RII_WeatherSeverity_Code_Active] ON [RII_WeatherSeverity];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414125103_LinkWeatherSeverityToWeatherType'
)
BEGIN
    ALTER TABLE [RII_WeatherSeverity] ADD [WeatherTypeId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414125103_LinkWeatherSeverityToWeatherType'
)
BEGIN
    IF ((SELECT COUNT(1) FROM [RII_WeatherType] WHERE [IsDeleted] = 0) = 1)
    BEGIN
        UPDATE ws
        SET [WeatherTypeId] = (
            SELECT TOP (1) wt.[Id]
            FROM [RII_WeatherType] wt
            WHERE wt.[IsDeleted] = 0
            ORDER BY wt.[Id]
        )
        FROM [RII_WeatherSeverity] ws
        WHERE ws.[IsDeleted] = 0
          AND ws.[WeatherTypeId] IS NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414125103_LinkWeatherSeverityToWeatherType'
)
BEGIN
    CREATE INDEX [IX_RII_WeatherSeverity_WeatherTypeId] ON [RII_WeatherSeverity] ([WeatherTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414125103_LinkWeatherSeverityToWeatherType'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_WeatherSeverity_WeatherType_Code_Active] ON [RII_WeatherSeverity] ([WeatherTypeId], [Code]) WHERE [IsDeleted] = 0 AND [WeatherTypeId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414125103_LinkWeatherSeverityToWeatherType'
)
BEGIN
    ALTER TABLE [RII_WeatherSeverity] ADD CONSTRAINT [FK_RII_WeatherSeverity_RII_WeatherType_WeatherTypeId] FOREIGN KEY ([WeatherTypeId]) REFERENCES [RII_WeatherType] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414125103_LinkWeatherSeverityToWeatherType'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260414125103_LinkWeatherSeverityToWeatherType', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414131238_AllowProjectMerge_RequireFullTransfer_AppSettings'
)
BEGIN
    ALTER TABLE [RII_AquaSetting] ADD [AllowProjectMerge] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414131238_AllowProjectMerge_RequireFullTransfer_AppSettings'
)
BEGIN
    ALTER TABLE [RII_AquaSetting] ADD [RequireFullTransfer] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414131238_AllowProjectMerge_RequireFullTransfer_AppSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260414131238_AllowProjectMerge_RequireFullTransfer_AppSettings', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE TABLE [RII_ProjectMerge] (
        [Id] bigint NOT NULL IDENTITY,
        [TargetProjectId] bigint NOT NULL,
        [TargetProjectCode] nvarchar(50) NOT NULL,
        [TargetProjectName] nvarchar(200) NOT NULL,
        [MergeDate] date NOT NULL,
        [Description] nvarchar(500) NULL,
        [SourceProjectStateAfterMerge] tinyint NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_ProjectMerge] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_ProjectMerge_RII_Project_TargetProjectId] FOREIGN KEY ([TargetProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_ProjectMerge_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_ProjectMerge_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_ProjectMerge_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE TABLE [RII_ProjectMergeCage] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectMergeId] bigint NOT NULL,
        [SourceProjectId] bigint NOT NULL,
        [ProjectCageId] bigint NOT NULL,
        [CageId] bigint NOT NULL,
        [CageCode] nvarchar(50) NOT NULL,
        [CageName] nvarchar(200) NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_ProjectMergeCage] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_ProjectMergeCage_RII_Cage_CageId] FOREIGN KEY ([CageId]) REFERENCES [RII_Cage] ([Id]),
        CONSTRAINT [FK_RII_ProjectMergeCage_RII_ProjectCage_ProjectCageId] FOREIGN KEY ([ProjectCageId]) REFERENCES [RII_ProjectCage] ([Id]),
        CONSTRAINT [FK_RII_ProjectMergeCage_RII_ProjectMerge_ProjectMergeId] FOREIGN KEY ([ProjectMergeId]) REFERENCES [RII_ProjectMerge] ([Id]),
        CONSTRAINT [FK_RII_ProjectMergeCage_RII_Project_SourceProjectId] FOREIGN KEY ([SourceProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_ProjectMergeCage_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_ProjectMergeCage_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_ProjectMergeCage_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE TABLE [RII_ProjectMergeSource] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectMergeId] bigint NOT NULL,
        [SourceProjectId] bigint NOT NULL,
        [SourceProjectCode] nvarchar(50) NOT NULL,
        [SourceProjectName] nvarchar(200) NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_ProjectMergeSource] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_ProjectMergeSource_RII_ProjectMerge_ProjectMergeId] FOREIGN KEY ([ProjectMergeId]) REFERENCES [RII_ProjectMerge] ([Id]),
        CONSTRAINT [FK_RII_ProjectMergeSource_RII_Project_SourceProjectId] FOREIGN KEY ([SourceProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_ProjectMergeSource_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_ProjectMergeSource_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_ProjectMergeSource_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectMerge_CreatedBy] ON [RII_ProjectMerge] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectMerge_DeletedBy] ON [RII_ProjectMerge] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectMerge_TargetProjectId] ON [RII_ProjectMerge] ([TargetProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectMerge_UpdatedBy] ON [RII_ProjectMerge] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectMergeCage_CageId] ON [RII_ProjectMergeCage] ([CageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectMergeCage_CreatedBy] ON [RII_ProjectMergeCage] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectMergeCage_DeletedBy] ON [RII_ProjectMergeCage] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectMergeCage_ProjectCageId] ON [RII_ProjectMergeCage] ([ProjectCageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectMergeCage_ProjectMergeId] ON [RII_ProjectMergeCage] ([ProjectMergeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectMergeCage_SourceProjectId] ON [RII_ProjectMergeCage] ([SourceProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectMergeCage_UpdatedBy] ON [RII_ProjectMergeCage] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectMergeSource_CreatedBy] ON [RII_ProjectMergeSource] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectMergeSource_DeletedBy] ON [RII_ProjectMergeSource] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectMergeSource_ProjectMergeId] ON [RII_ProjectMergeSource] ([ProjectMergeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectMergeSource_SourceProjectId] ON [RII_ProjectMergeSource] ([SourceProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    CREATE INDEX [IX_RII_ProjectMergeSource_UpdatedBy] ON [RII_ProjectMergeSource] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414163345_ProjectMergeAndCrossProjectTransfer'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260414163345_ProjectMergeAndCrossProjectTransfer', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415064348_AquaLinePricingFields'
)
BEGIN
    ALTER TABLE [RII_ShipmentLine] ADD [CurrencyCode] nvarchar(10) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415064348_AquaLinePricingFields'
)
BEGIN
    ALTER TABLE [RII_ShipmentLine] ADD [ExchangeRate] decimal(18,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415064348_AquaLinePricingFields'
)
BEGIN
    ALTER TABLE [RII_ShipmentLine] ADD [LineAmount] decimal(18,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415064348_AquaLinePricingFields'
)
BEGIN
    ALTER TABLE [RII_ShipmentLine] ADD [LocalLineAmount] decimal(18,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415064348_AquaLinePricingFields'
)
BEGIN
    ALTER TABLE [RII_ShipmentLine] ADD [LocalUnitPrice] decimal(18,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415064348_AquaLinePricingFields'
)
BEGIN
    ALTER TABLE [RII_ShipmentLine] ADD [UnitPrice] decimal(18,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415064348_AquaLinePricingFields'
)
BEGIN
    ALTER TABLE [RII_GoodsReceiptLine] ADD [CurrencyCode] nvarchar(10) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415064348_AquaLinePricingFields'
)
BEGIN
    ALTER TABLE [RII_GoodsReceiptLine] ADD [ExchangeRate] decimal(18,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415064348_AquaLinePricingFields'
)
BEGIN
    ALTER TABLE [RII_GoodsReceiptLine] ADD [LineAmount] decimal(18,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415064348_AquaLinePricingFields'
)
BEGIN
    ALTER TABLE [RII_GoodsReceiptLine] ADD [LocalLineAmount] decimal(18,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415064348_AquaLinePricingFields'
)
BEGIN
    ALTER TABLE [RII_GoodsReceiptLine] ADD [LocalUnitPrice] decimal(18,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415064348_AquaLinePricingFields'
)
BEGIN
    ALTER TABLE [RII_GoodsReceiptLine] ADD [UnitPrice] decimal(18,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415064348_AquaLinePricingFields'
)
BEGIN
    ALTER TABLE [RII_AquaSetting] ADD [FeedCostFallbackStrategy] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415064348_AquaLinePricingFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260415064348_AquaLinePricingFields', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415114617_GoodsReceiptWarehouseCodeRefactor'
)
BEGIN
    IF COL_LENGTH('dbo.RII_GoodsReceipt', 'WarehouseId') IS NOT NULL
    BEGIN
        ALTER TABLE [dbo].[RII_GoodsReceipt] DROP COLUMN [WarehouseId];
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415114617_GoodsReceiptWarehouseCodeRefactor'
)
BEGIN
    IF COL_LENGTH('dbo.RII_GoodsReceipt', 'WarehouseBranchCode') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_GoodsReceipt] ADD [WarehouseBranchCode] int NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415114617_GoodsReceiptWarehouseCodeRefactor'
)
BEGIN
    IF COL_LENGTH('dbo.RII_GoodsReceipt', 'WarehouseCode') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_GoodsReceipt] ADD [WarehouseCode] smallint NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415114617_GoodsReceiptWarehouseCodeRefactor'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_RII_GoodsReceipt_WarehouseCode_BranchCode'
          AND object_id = OBJECT_ID(N'[dbo].[RII_GoodsReceipt]')
    )
    BEGIN
        CREATE INDEX [IX_RII_GoodsReceipt_WarehouseCode_BranchCode]
        ON [dbo].[RII_GoodsReceipt] ([WarehouseCode], [WarehouseBranchCode]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415114617_GoodsReceiptWarehouseCodeRefactor'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_Warehouse]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[RII_Warehouse]
        (
            [Id] bigint IDENTITY(1,1) NOT NULL,
            [ErpWarehouseCode] smallint NOT NULL,
            [WarehouseName] nvarchar(150) NOT NULL,
            [CustomerCode] nvarchar(25) NULL,
            [BranchCode] int NOT NULL,
            [IsLocked] bit NOT NULL CONSTRAINT [DF_RII_Warehouse_IsLocked] DEFAULT(0),
            [AllowNegativeBalance] bit NOT NULL CONSTRAINT [DF_RII_Warehouse_AllowNegativeBalance] DEFAULT(0),
            [LastSyncedAt] datetime2 NULL,
            [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_Warehouse_CreatedDate] DEFAULT(GETDATE()),
            [UpdatedDate] datetime2 NULL,
            [DeletedDate] datetime2 NULL,
            [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_Warehouse_IsDeleted] DEFAULT(0),
            [CreatedBy] bigint NULL,
            [UpdatedBy] bigint NULL,
            [DeletedBy] bigint NULL,
            CONSTRAINT [PK_RII_Warehouse] PRIMARY KEY ([Id])
        );
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415114617_GoodsReceiptWarehouseCodeRefactor'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'UX_RII_Warehouse_ErpWarehouseCode_BranchCode'
          AND object_id = OBJECT_ID(N'[dbo].[RII_Warehouse]')
    )
    BEGIN
        CREATE UNIQUE INDEX [UX_RII_Warehouse_ErpWarehouseCode_BranchCode]
        ON [dbo].[RII_Warehouse] ([ErpWarehouseCode], [BranchCode]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415114617_GoodsReceiptWarehouseCodeRefactor'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_RII_Warehouse_WarehouseName'
          AND object_id = OBJECT_ID(N'[dbo].[RII_Warehouse]')
    )
    BEGIN
        CREATE INDEX [IX_RII_Warehouse_WarehouseName]
        ON [dbo].[RII_Warehouse] ([WarehouseName]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415114617_GoodsReceiptWarehouseCodeRefactor'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260415114617_GoodsReceiptWarehouseCodeRefactor', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    DROP INDEX [IX_RII_GoodsReceipt_WarehouseCode_BranchCode] ON [RII_GoodsReceipt];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    ALTER TABLE [RII_BatchMovement] DROP CONSTRAINT [CK_RII_BatchMovement_MovementType];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    DECLARE @var44 sysname;
    SELECT @var44 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_Shipment]') AND [c].[name] = N'TargetWarehouse');
    IF @var44 IS NOT NULL EXEC(N'ALTER TABLE [RII_Shipment] DROP CONSTRAINT [' + @var44 + '];');
    ALTER TABLE [RII_Shipment] DROP COLUMN [TargetWarehouse];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    DECLARE @var45 sysname;
    SELECT @var45 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_GoodsReceipt]') AND [c].[name] = N'WarehouseBranchCode');
    IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [RII_GoodsReceipt] DROP CONSTRAINT [' + @var45 + '];');
    ALTER TABLE [RII_GoodsReceipt] DROP COLUMN [WarehouseBranchCode];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    DECLARE @var46 sysname;
    SELECT @var46 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_GoodsReceipt]') AND [c].[name] = N'WarehouseCode');
    IF @var46 IS NOT NULL EXEC(N'ALTER TABLE [RII_GoodsReceipt] DROP CONSTRAINT [' + @var46 + '];');
    ALTER TABLE [RII_GoodsReceipt] DROP COLUMN [WarehouseCode];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    ALTER TABLE [RII_Shipment] ADD [TargetWarehouseId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    ALTER TABLE [RII_GoodsReceipt] ADD [WarehouseId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    ALTER TABLE [RII_BatchMovement] ADD [FromWarehouseId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    ALTER TABLE [RII_BatchMovement] ADD [ToWarehouseId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    ALTER TABLE [RII_BatchMovement] ADD [WarehouseId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE TABLE [RII_BatchWarehouseBalance] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectId] bigint NOT NULL,
        [FishBatchId] bigint NOT NULL,
        [WarehouseId] bigint NOT NULL,
        [LiveCount] int NOT NULL,
        [AverageGram] decimal(18,6) NOT NULL,
        [BiomassGram] decimal(18,6) NOT NULL,
        [AsOfDate] datetime2(3) NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_BatchWarehouseBalance] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_BatchWarehouseBalance_NonNegative] CHECK ([LiveCount] >= 0 AND [AverageGram] >= 0 AND [BiomassGram] >= 0),
        CONSTRAINT [FK_RII_BatchWarehouseBalance_RII_FishBatch_FishBatchId] FOREIGN KEY ([FishBatchId]) REFERENCES [RII_FishBatch] ([Id]),
        CONSTRAINT [FK_RII_BatchWarehouseBalance_RII_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_BatchWarehouseBalance_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_BatchWarehouseBalance_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_BatchWarehouseBalance_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_BatchWarehouseBalance_RII_Warehouse_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [RII_Warehouse] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE TABLE [RII_CageWarehouseTransfer] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectId] bigint NOT NULL,
        [TransferNo] nvarchar(40) NOT NULL,
        [TransferDate] datetime2(3) NOT NULL,
        [Status] tinyint NOT NULL,
        [Note] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_CageWarehouseTransfer] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_CageWarehouseTransfer_RII_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_CageWarehouseTransfer_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_CageWarehouseTransfer_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_CageWarehouseTransfer_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE TABLE [RII_WarehouseCageTransfer] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectId] bigint NOT NULL,
        [TransferNo] nvarchar(40) NOT NULL,
        [TransferDate] datetime2(3) NOT NULL,
        [Status] tinyint NOT NULL,
        [Note] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_WarehouseCageTransfer] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_WarehouseCageTransfer_RII_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_WarehouseCageTransfer_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_WarehouseCageTransfer_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_WarehouseCageTransfer_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE TABLE [RII_WarehouseTransfer] (
        [Id] bigint NOT NULL IDENTITY,
        [ProjectId] bigint NOT NULL,
        [TransferNo] nvarchar(50) NOT NULL,
        [TransferDate] datetime2 NOT NULL,
        [Status] tinyint NOT NULL,
        [Note] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_WarehouseTransfer] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_WarehouseTransfer_RII_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [RII_Project] ([Id]),
        CONSTRAINT [FK_RII_WarehouseTransfer_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_WarehouseTransfer_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_WarehouseTransfer_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE TABLE [RII_CageWarehouseTransferLine] (
        [Id] bigint NOT NULL IDENTITY,
        [CageWarehouseTransferId] bigint NOT NULL,
        [FishBatchId] bigint NOT NULL,
        [FromProjectCageId] bigint NOT NULL,
        [ToWarehouseId] bigint NOT NULL,
        [FishCount] int NOT NULL,
        [AverageGram] decimal(18,6) NOT NULL,
        [BiomassGram] decimal(18,6) NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_CageWarehouseTransferLine] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_CageWarehouseTransferLine_Positive] CHECK ([FishCount] > 0 AND [AverageGram] > 0 AND [BiomassGram] > 0),
        CONSTRAINT [FK_RII_CageWarehouseTransferLine_RII_CageWarehouseTransfer_CageWarehouseTransferId] FOREIGN KEY ([CageWarehouseTransferId]) REFERENCES [RII_CageWarehouseTransfer] ([Id]),
        CONSTRAINT [FK_RII_CageWarehouseTransferLine_RII_FishBatch_FishBatchId] FOREIGN KEY ([FishBatchId]) REFERENCES [RII_FishBatch] ([Id]),
        CONSTRAINT [FK_RII_CageWarehouseTransferLine_RII_ProjectCage_FromProjectCageId] FOREIGN KEY ([FromProjectCageId]) REFERENCES [RII_ProjectCage] ([Id]),
        CONSTRAINT [FK_RII_CageWarehouseTransferLine_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_CageWarehouseTransferLine_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_CageWarehouseTransferLine_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_CageWarehouseTransferLine_RII_Warehouse_ToWarehouseId] FOREIGN KEY ([ToWarehouseId]) REFERENCES [RII_Warehouse] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE TABLE [RII_WarehouseCageTransferLine] (
        [Id] bigint NOT NULL IDENTITY,
        [WarehouseCageTransferId] bigint NOT NULL,
        [FishBatchId] bigint NOT NULL,
        [FromWarehouseId] bigint NOT NULL,
        [ToProjectCageId] bigint NOT NULL,
        [FishCount] int NOT NULL,
        [AverageGram] decimal(18,6) NOT NULL,
        [BiomassGram] decimal(18,6) NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_WarehouseCageTransferLine] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_WarehouseCageTransferLine_Positive] CHECK ([FishCount] > 0 AND [AverageGram] > 0 AND [BiomassGram] > 0),
        CONSTRAINT [FK_RII_WarehouseCageTransferLine_RII_FishBatch_FishBatchId] FOREIGN KEY ([FishBatchId]) REFERENCES [RII_FishBatch] ([Id]),
        CONSTRAINT [FK_RII_WarehouseCageTransferLine_RII_ProjectCage_ToProjectCageId] FOREIGN KEY ([ToProjectCageId]) REFERENCES [RII_ProjectCage] ([Id]),
        CONSTRAINT [FK_RII_WarehouseCageTransferLine_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_WarehouseCageTransferLine_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_WarehouseCageTransferLine_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_WarehouseCageTransferLine_RII_WarehouseCageTransfer_WarehouseCageTransferId] FOREIGN KEY ([WarehouseCageTransferId]) REFERENCES [RII_WarehouseCageTransfer] ([Id]),
        CONSTRAINT [FK_RII_WarehouseCageTransferLine_RII_Warehouse_FromWarehouseId] FOREIGN KEY ([FromWarehouseId]) REFERENCES [RII_Warehouse] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE TABLE [RII_WarehouseTransferLine] (
        [Id] bigint NOT NULL IDENTITY,
        [WarehouseTransferId] bigint NOT NULL,
        [FishBatchId] bigint NOT NULL,
        [FromWarehouseId] bigint NOT NULL,
        [ToWarehouseId] bigint NOT NULL,
        [FishCount] int NOT NULL,
        [AverageGram] decimal(18,6) NOT NULL,
        [BiomassGram] decimal(18,6) NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_WarehouseTransferLine] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RII_WarehouseTransferLine_FromToDiff] CHECK ([FromWarehouseId] <> [ToWarehouseId]),
        CONSTRAINT [CK_RII_WarehouseTransferLine_Positive] CHECK ([FishCount] > 0 AND [AverageGram] > 0 AND [BiomassGram] > 0),
        CONSTRAINT [FK_RII_WarehouseTransferLine_RII_FishBatch_FishBatchId] FOREIGN KEY ([FishBatchId]) REFERENCES [RII_FishBatch] ([Id]),
        CONSTRAINT [FK_RII_WarehouseTransferLine_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_WarehouseTransferLine_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_WarehouseTransferLine_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_WarehouseTransferLine_RII_WarehouseTransfer_WarehouseTransferId] FOREIGN KEY ([WarehouseTransferId]) REFERENCES [RII_WarehouseTransfer] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_Shipment_TargetWarehouseId] ON [RII_Shipment] ([TargetWarehouseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_GoodsReceipt_WarehouseId] ON [RII_GoodsReceipt] ([WarehouseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    EXEC(N'ALTER TABLE [RII_BatchMovement] ADD CONSTRAINT [CK_RII_BatchMovement_MovementType] CHECK ([MovementType] IN (0,1,2,3,4,5,6,7,8))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_BatchWarehouseBalance_CreatedBy] ON [RII_BatchWarehouseBalance] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_BatchWarehouseBalance_DeletedBy] ON [RII_BatchWarehouseBalance] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_BatchWarehouseBalance_FishBatchId] ON [RII_BatchWarehouseBalance] ([FishBatchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_BatchWarehouseBalance_UpdatedBy] ON [RII_BatchWarehouseBalance] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_BatchWarehouseBalance_WarehouseId] ON [RII_BatchWarehouseBalance] ([WarehouseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_BatchWarehouseBalance_ProjectBatchWarehouse_Active] ON [RII_BatchWarehouseBalance] ([ProjectId], [FishBatchId], [WarehouseId]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseTransfer_CreatedBy] ON [RII_CageWarehouseTransfer] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseTransfer_DeletedBy] ON [RII_CageWarehouseTransfer] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseTransfer_ProjectId] ON [RII_CageWarehouseTransfer] ([ProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseTransfer_TransferNo] ON [RII_CageWarehouseTransfer] ([TransferNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseTransfer_UpdatedBy] ON [RII_CageWarehouseTransfer] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseTransferLine_CageWarehouseTransferId] ON [RII_CageWarehouseTransferLine] ([CageWarehouseTransferId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseTransferLine_CreatedBy] ON [RII_CageWarehouseTransferLine] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseTransferLine_DeletedBy] ON [RII_CageWarehouseTransferLine] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseTransferLine_FishBatchId] ON [RII_CageWarehouseTransferLine] ([FishBatchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseTransferLine_FromProjectCageId] ON [RII_CageWarehouseTransferLine] ([FromProjectCageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseTransferLine_ToWarehouseId] ON [RII_CageWarehouseTransferLine] ([ToWarehouseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseTransferLine_UpdatedBy] ON [RII_CageWarehouseTransferLine] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseCageTransfer_CreatedBy] ON [RII_WarehouseCageTransfer] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseCageTransfer_DeletedBy] ON [RII_WarehouseCageTransfer] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseCageTransfer_ProjectId] ON [RII_WarehouseCageTransfer] ([ProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseCageTransfer_TransferNo] ON [RII_WarehouseCageTransfer] ([TransferNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseCageTransfer_UpdatedBy] ON [RII_WarehouseCageTransfer] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseCageTransferLine_CreatedBy] ON [RII_WarehouseCageTransferLine] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseCageTransferLine_DeletedBy] ON [RII_WarehouseCageTransferLine] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseCageTransferLine_FishBatchId] ON [RII_WarehouseCageTransferLine] ([FishBatchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseCageTransferLine_FromWarehouseId] ON [RII_WarehouseCageTransferLine] ([FromWarehouseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseCageTransferLine_ToProjectCageId] ON [RII_WarehouseCageTransferLine] ([ToProjectCageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseCageTransferLine_UpdatedBy] ON [RII_WarehouseCageTransferLine] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseCageTransferLine_WarehouseCageTransferId] ON [RII_WarehouseCageTransferLine] ([WarehouseCageTransferId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseTransfer_CreatedBy] ON [RII_WarehouseTransfer] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseTransfer_DeletedBy] ON [RII_WarehouseTransfer] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseTransfer_ProjectId] ON [RII_WarehouseTransfer] ([ProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseTransfer_UpdatedBy] ON [RII_WarehouseTransfer] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseTransferLine_CreatedBy] ON [RII_WarehouseTransferLine] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseTransferLine_DeletedBy] ON [RII_WarehouseTransferLine] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseTransferLine_FishBatchId] ON [RII_WarehouseTransferLine] ([FishBatchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseTransferLine_UpdatedBy] ON [RII_WarehouseTransferLine] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    CREATE INDEX [IX_RII_WarehouseTransferLine_WarehouseTransferId] ON [RII_WarehouseTransferLine] ([WarehouseTransferId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416063248_WarehouseTransferTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260416063248_WarehouseTransferTables', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416131331_OpeningImportInitialMigration'
)
BEGIN
    ALTER TABLE [RII_BatchMovement] DROP CONSTRAINT [CK_RII_BatchMovement_MovementType];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416131331_OpeningImportInitialMigration'
)
BEGIN
    CREATE TABLE [RII_OpeningImportJob] (
        [Id] bigint NOT NULL IDENTITY,
        [FileName] nvarchar(260) NOT NULL,
        [SourceSystem] nvarchar(100) NULL,
        [Status] tinyint NOT NULL,
        [MappingsJson] nvarchar(max) NULL,
        [SummaryJson] nvarchar(max) NULL,
        [PreviewedAt] datetime2(3) NULL,
        [AppliedAt] datetime2(3) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_OpeningImportJob] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_OpeningImportJob_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_OpeningImportJob_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_OpeningImportJob_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416131331_OpeningImportInitialMigration'
)
BEGIN
    CREATE TABLE [RII_OpeningImportRow] (
        [Id] bigint NOT NULL IDENTITY,
        [OpeningImportJobId] bigint NOT NULL,
        [SheetName] nvarchar(50) NOT NULL,
        [RowNumber] int NOT NULL,
        [Status] tinyint NOT NULL,
        [RawDataJson] nvarchar(max) NOT NULL,
        [NormalizedDataJson] nvarchar(max) NULL,
        [MessagesJson] nvarchar(max) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_OpeningImportRow] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_OpeningImportRow_RII_OpeningImportJob_OpeningImportJobId] FOREIGN KEY ([OpeningImportJobId]) REFERENCES [RII_OpeningImportJob] ([Id]),
        CONSTRAINT [FK_RII_OpeningImportRow_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_OpeningImportRow_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_OpeningImportRow_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416131331_OpeningImportInitialMigration'
)
BEGIN
    EXEC(N'ALTER TABLE [RII_BatchMovement] ADD CONSTRAINT [CK_RII_BatchMovement_MovementType] CHECK ([MovementType] IN (0,1,2,3,4,5,6,7,8,9))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416131331_OpeningImportInitialMigration'
)
BEGIN
    CREATE INDEX [IX_RII_OpeningImportJob_CreatedBy] ON [RII_OpeningImportJob] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416131331_OpeningImportInitialMigration'
)
BEGIN
    CREATE INDEX [IX_RII_OpeningImportJob_DeletedBy] ON [RII_OpeningImportJob] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416131331_OpeningImportInitialMigration'
)
BEGIN
    CREATE INDEX [IX_RII_OpeningImportJob_UpdatedBy] ON [RII_OpeningImportJob] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416131331_OpeningImportInitialMigration'
)
BEGIN
    CREATE INDEX [IX_RII_OpeningImportRow_CreatedBy] ON [RII_OpeningImportRow] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416131331_OpeningImportInitialMigration'
)
BEGIN
    CREATE INDEX [IX_RII_OpeningImportRow_DeletedBy] ON [RII_OpeningImportRow] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416131331_OpeningImportInitialMigration'
)
BEGIN
    CREATE INDEX [IX_RII_OpeningImportRow_JobSheetRow] ON [RII_OpeningImportRow] ([OpeningImportJobId], [SheetName], [RowNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416131331_OpeningImportInitialMigration'
)
BEGIN
    CREATE INDEX [IX_RII_OpeningImportRow_UpdatedBy] ON [RII_OpeningImportRow] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416131331_OpeningImportInitialMigration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260416131331_OpeningImportInitialMigration', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420204038_AquaHangfireExecutionLog'
)
BEGIN
    CREATE TABLE [RII_JOB_EXECUTION_LOG] (
        [Id] bigint NOT NULL IDENTITY,
        [JobId] nvarchar(100) NOT NULL,
        [RecurringJobId] nvarchar(100) NULL,
        [JobName] nvarchar(500) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [Queue] nvarchar(100) NULL,
        [StartedAt] datetime2 NOT NULL,
        [FinishedAt] datetime2 NOT NULL,
        [DurationMs] int NOT NULL,
        [Reason] nvarchar(2000) NULL,
        [ExceptionType] nvarchar(500) NULL,
        [ExceptionMessage] nvarchar(4000) NULL,
        [RetryCount] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_JOB_EXECUTION_LOG] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_JOB_EXECUTION_LOG_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_JOB_EXECUTION_LOG_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_JOB_EXECUTION_LOG_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420204038_AquaHangfireExecutionLog'
)
BEGIN
    CREATE INDEX [IX_JobExecutionLog_FinishedAt] ON [RII_JOB_EXECUTION_LOG] ([FinishedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420204038_AquaHangfireExecutionLog'
)
BEGIN
    CREATE INDEX [IX_JobExecutionLog_JobId] ON [RII_JOB_EXECUTION_LOG] ([JobId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420204038_AquaHangfireExecutionLog'
)
BEGIN
    CREATE INDEX [IX_JobExecutionLog_JobName] ON [RII_JOB_EXECUTION_LOG] ([JobName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420204038_AquaHangfireExecutionLog'
)
BEGIN
    CREATE INDEX [IX_JobExecutionLog_RecurringJobId] ON [RII_JOB_EXECUTION_LOG] ([RecurringJobId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420204038_AquaHangfireExecutionLog'
)
BEGIN
    CREATE INDEX [IX_JobExecutionLog_Status] ON [RII_JOB_EXECUTION_LOG] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420204038_AquaHangfireExecutionLog'
)
BEGIN
    CREATE INDEX [IX_RII_JOB_EXECUTION_LOG_CreatedBy] ON [RII_JOB_EXECUTION_LOG] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420204038_AquaHangfireExecutionLog'
)
BEGIN
    CREATE INDEX [IX_RII_JOB_EXECUTION_LOG_DeletedBy] ON [RII_JOB_EXECUTION_LOG] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420204038_AquaHangfireExecutionLog'
)
BEGIN
    CREATE INDEX [IX_RII_JOB_EXECUTION_LOG_UpdatedBy] ON [RII_JOB_EXECUTION_LOG] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420204038_AquaHangfireExecutionLog'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260420204038_AquaHangfireExecutionLog', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421061105_FishBatchExtendedFieldsCheck2'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260421061105_FishBatchExtendedFieldsCheck2', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421061243_FishBatchExtendedFieldsCheck'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260421061243_FishBatchExtendedFieldsCheck', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421091500_AddMissingFishBatchExtendedFields'
)
BEGIN
    IF COL_LENGTH('dbo.RII_FishBatch', 'SupplierId') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_FishBatch] ADD [SupplierId] bigint NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421091500_AddMissingFishBatchExtendedFields'
)
BEGIN
    IF COL_LENGTH('dbo.RII_FishBatch', 'SupplierLotCode') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_FishBatch] ADD [SupplierLotCode] nvarchar(100) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421091500_AddMissingFishBatchExtendedFields'
)
BEGIN
    IF COL_LENGTH('dbo.RII_FishBatch', 'HatcheryName') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_FishBatch] ADD [HatcheryName] nvarchar(150) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421091500_AddMissingFishBatchExtendedFields'
)
BEGIN
    IF COL_LENGTH('dbo.RII_FishBatch', 'OriginCountryCode') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_FishBatch] ADD [OriginCountryCode] nvarchar(10) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421091500_AddMissingFishBatchExtendedFields'
)
BEGIN
    IF COL_LENGTH('dbo.RII_FishBatch', 'StrainCode') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_FishBatch] ADD [StrainCode] nvarchar(50) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421091500_AddMissingFishBatchExtendedFields'
)
BEGIN
    IF COL_LENGTH('dbo.RII_FishBatch', 'GenerationCode') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_FishBatch] ADD [GenerationCode] nvarchar(50) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421091500_AddMissingFishBatchExtendedFields'
)
BEGIN
    IF COL_LENGTH('dbo.RII_FishBatch', 'BroodstockCode') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_FishBatch] ADD [BroodstockCode] nvarchar(50) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421091500_AddMissingFishBatchExtendedFields'
)
BEGIN
    IF COL_LENGTH('dbo.RII_FishBatch', 'IsVaccinated') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_FishBatch] ADD [IsVaccinated] bit NOT NULL CONSTRAINT [DF_RII_FishBatch_IsVaccinated] DEFAULT(0);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421091500_AddMissingFishBatchExtendedFields'
)
BEGIN
    IF COL_LENGTH('dbo.RII_FishBatch', 'VaccinationDate') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_FishBatch] ADD [VaccinationDate] datetime2(3) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421091500_AddMissingFishBatchExtendedFields'
)
BEGIN
    IF COL_LENGTH('dbo.RII_FishBatch', 'VaccinationNote') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_FishBatch] ADD [VaccinationNote] nvarchar(500) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421091500_AddMissingFishBatchExtendedFields'
)
BEGIN
    IF COL_LENGTH('dbo.RII_FishBatch', 'TreatmentHistoryNote') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_FishBatch] ADD [TreatmentHistoryNote] nvarchar(1000) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421091500_AddMissingFishBatchExtendedFields'
)
BEGIN
    IF COL_LENGTH('dbo.RII_FishBatch', 'TargetHarvestAverageGram') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_FishBatch] ADD [TargetHarvestAverageGram] decimal(18,3) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421091500_AddMissingFishBatchExtendedFields'
)
BEGIN
    IF COL_LENGTH('dbo.RII_FishBatch', 'TargetHarvestDate') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_FishBatch] ADD [TargetHarvestDate] datetime2(3) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421091500_AddMissingFishBatchExtendedFields'
)
BEGIN
    IF COL_LENGTH('dbo.RII_FishBatch', 'TargetHarvestClass') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_FishBatch] ADD [TargetHarvestClass] nvarchar(50) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421091500_AddMissingFishBatchExtendedFields'
)
BEGIN
    IF COL_LENGTH('dbo.RII_FishBatch', 'QualityGrade') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_FishBatch] ADD [QualityGrade] nvarchar(50) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421091500_AddMissingFishBatchExtendedFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260421091500_AddMissingFishBatchExtendedFields', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_DailyWeather', 'WaterTemperatureSurfaceC') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_DailyWeather] ADD [WaterTemperatureSurfaceC] decimal(18,6) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_DailyWeather', 'WaterTemperatureDepthC') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_DailyWeather] ADD [WaterTemperatureDepthC] decimal(18,6) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_DailyWeather', 'DissolvedOxygenMgL') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_DailyWeather] ADD [DissolvedOxygenMgL] decimal(18,6) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_DailyWeather', 'SalinityPpt') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_DailyWeather] ADD [SalinityPpt] decimal(18,6) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_DailyWeather', 'Ph') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_DailyWeather] ADD [Ph] decimal(18,6) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_DailyWeather', 'CurrentSpeedKn') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_DailyWeather] ADD [CurrentSpeedKn] decimal(18,6) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_DailyWeather', 'WaveHeightM') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_DailyWeather] ADD [WaveHeightM] decimal(18,6) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_DailyWeather', 'TurbidityNtu') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_DailyWeather] ADD [TurbidityNtu] decimal(18,6) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_DailyWeather', 'AmmoniaMgL') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_DailyWeather] ADD [AmmoniaMgL] decimal(18,6) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_DailyWeather', 'NitriteMgL') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_DailyWeather] ADD [NitriteMgL] decimal(18,6) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_DailyWeather', 'AlgalBloomIndex') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_DailyWeather] ADD [AlgalBloomIndex] decimal(18,6) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_DailyWeather', 'SensorHealthScore') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_DailyWeather] ADD [SensorHealthScore] decimal(18,6) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_DailyWeather', 'SensorRecordedAt') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_DailyWeather] ADD [SensorRecordedAt] datetime2(3) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_DailyWeather', 'DataSource') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RII_DailyWeather] ADD [DataSource] nvarchar(50) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[RII_ComplianceAudit] (
            [Id] bigint IDENTITY(1,1) NOT NULL,
            [ProjectId] bigint NOT NULL,
            [ProjectCageId] bigint NULL,
            [FishBatchId] bigint NULL,
            [AuditDate] datetime2(3) NOT NULL,
            [StandardCode] nvarchar(50) NOT NULL,
            [ChecklistCode] nvarchar(50) NULL,
            [Status] nvarchar(40) NOT NULL,
            [FindingCount] int NOT NULL,
            [AuditorName] nvarchar(150) NULL,
            [Summary] nvarchar(2000) NULL,
            [NextAuditDate] datetime2(3) NULL,
            [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_ComplianceAudit_CreatedDate] DEFAULT(GETDATE()),
            [UpdatedDate] datetime2 NULL,
            [DeletedDate] datetime2 NULL,
            [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_ComplianceAudit_IsDeleted] DEFAULT(0),
            [CreatedBy] bigint NULL,
            [UpdatedBy] bigint NULL,
            [DeletedBy] bigint NULL,
            CONSTRAINT [PK_RII_ComplianceAudit] PRIMARY KEY ([Id])
        );
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[RII_ComplianceCorrectiveAction] (
            [Id] bigint IDENTITY(1,1) NOT NULL,
            [ComplianceAuditId] bigint NOT NULL,
            [ActionCode] nvarchar(50) NOT NULL,
            [Description] nvarchar(1000) NOT NULL,
            [Status] nvarchar(40) NOT NULL,
            [OwnerName] nvarchar(150) NULL,
            [DueDate] datetime2(3) NULL,
            [ClosedDate] datetime2(3) NULL,
            [ClosureNote] nvarchar(1000) NULL,
            [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_ComplianceCorrectiveAction_CreatedDate] DEFAULT(GETDATE()),
            [UpdatedDate] datetime2 NULL,
            [DeletedDate] datetime2 NULL,
            [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_ComplianceCorrectiveAction_IsDeleted] DEFAULT(0),
            [CreatedBy] bigint NULL,
            [UpdatedBy] bigint NULL,
            [DeletedBy] bigint NULL,
            CONSTRAINT [PK_RII_ComplianceCorrectiveAction] PRIMARY KEY ([Id])
        );
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[RII_FishHealthEvent] (
            [Id] bigint IDENTITY(1,1) NOT NULL,
            [ProjectId] bigint NOT NULL,
            [ProjectCageId] bigint NULL,
            [FishBatchId] bigint NULL,
            [EventDate] datetime2(3) NOT NULL,
            [EventType] nvarchar(80) NOT NULL,
            [Severity] nvarchar(40) NOT NULL,
            [Status] nvarchar(40) NOT NULL,
            [AffectedFishCount] int NULL,
            [AffectedRatioPct] decimal(18,6) NULL,
            [MortalityCount] int NULL,
            [IsConfirmed] bit NOT NULL CONSTRAINT [DF_RII_FishHealthEvent_IsConfirmed] DEFAULT(0),
            [RequiresVeterinaryReview] bit NOT NULL CONSTRAINT [DF_RII_FishHealthEvent_RequiresVeterinaryReview] DEFAULT(0),
            [VeterinarianName] nvarchar(150) NULL,
            [Observation] nvarchar(2000) NULL,
            [RecommendedAction] nvarchar(1000) NULL,
            [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_FishHealthEvent_CreatedDate] DEFAULT(GETDATE()),
            [UpdatedDate] datetime2 NULL,
            [DeletedDate] datetime2 NULL,
            [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_FishHealthEvent_IsDeleted] DEFAULT(0),
            [CreatedBy] bigint NULL,
            [UpdatedBy] bigint NULL,
            [DeletedBy] bigint NULL,
            CONSTRAINT [PK_RII_FishHealthEvent] PRIMARY KEY ([Id])
        );
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[RII_FishLabSample] (
            [Id] bigint IDENTITY(1,1) NOT NULL,
            [ProjectId] bigint NOT NULL,
            [ProjectCageId] bigint NULL,
            [FishBatchId] bigint NULL,
            [FishHealthEventId] bigint NULL,
            [SampleDate] datetime2(3) NOT NULL,
            [SampleCode] nvarchar(80) NOT NULL,
            [SampleType] nvarchar(80) NOT NULL,
            [LaboratoryName] nvarchar(150) NULL,
            [RequestedBy] nvarchar(150) NULL,
            [Note] nvarchar(1000) NULL,
            [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_FishLabSample_CreatedDate] DEFAULT(GETDATE()),
            [UpdatedDate] datetime2 NULL,
            [DeletedDate] datetime2 NULL,
            [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_FishLabSample_IsDeleted] DEFAULT(0),
            [CreatedBy] bigint NULL,
            [UpdatedBy] bigint NULL,
            [DeletedBy] bigint NULL,
            CONSTRAINT [PK_RII_FishLabSample] PRIMARY KEY ([Id])
        );
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabResult]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[RII_FishLabResult] (
            [Id] bigint IDENTITY(1,1) NOT NULL,
            [FishLabSampleId] bigint NOT NULL,
            [ResultDate] datetime2(3) NOT NULL,
            [ResultType] nvarchar(80) NOT NULL,
            [PathogenName] nvarchar(120) NULL,
            [ResultValue] nvarchar(120) NULL,
            [Unit] nvarchar(30) NULL,
            [IsPositive] bit NOT NULL CONSTRAINT [DF_RII_FishLabResult_IsPositive] DEFAULT(0),
            [Interpretation] nvarchar(1000) NULL,
            [RecommendedAction] nvarchar(1000) NULL,
            [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_FishLabResult_CreatedDate] DEFAULT(GETDATE()),
            [UpdatedDate] datetime2 NULL,
            [DeletedDate] datetime2 NULL,
            [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_FishLabResult_IsDeleted] DEFAULT(0),
            [CreatedBy] bigint NULL,
            [UpdatedBy] bigint NULL,
            [DeletedBy] bigint NULL,
            CONSTRAINT [PK_RII_FishLabResult] PRIMARY KEY ([Id])
        );
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[RII_FishTreatment] (
            [Id] bigint IDENTITY(1,1) NOT NULL,
            [ProjectId] bigint NOT NULL,
            [ProjectCageId] bigint NULL,
            [FishBatchId] bigint NULL,
            [FishHealthEventId] bigint NULL,
            [TreatmentDate] datetime2(3) NOT NULL,
            [TreatmentType] nvarchar(80) NOT NULL,
            [MedicationName] nvarchar(120) NOT NULL,
            [ActiveIngredient] nvarchar(120) NULL,
            [DoseValue] decimal(18,6) NULL,
            [DoseUnit] nvarchar(30) NULL,
            [DurationDays] int NULL,
            [WithdrawalEndDate] datetime2(3) NULL,
            [Status] nvarchar(40) NOT NULL,
            [VeterinarianName] nvarchar(150) NULL,
            [TreatmentReason] nvarchar(500) NULL,
            [Note] nvarchar(1000) NULL,
            [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_FishTreatment_CreatedDate] DEFAULT(GETDATE()),
            [UpdatedDate] datetime2 NULL,
            [DeletedDate] datetime2 NULL,
            [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_FishTreatment_IsDeleted] DEFAULT(0),
            [CreatedBy] bigint NULL,
            [UpdatedBy] bigint NULL,
            [DeletedBy] bigint NULL,
            CONSTRAINT [PK_RII_FishTreatment] PRIMARY KEY ([Id])
        );
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[RII_ProjectCageDailyKpiSnapshot] (
            [Id] bigint IDENTITY(1,1) NOT NULL,
            [ProjectId] bigint NOT NULL,
            [ProjectCageId] bigint NOT NULL,
            [FishBatchId] bigint NOT NULL,
            [SnapshotDate] datetime2 NOT NULL,
            [InitialCount] int NOT NULL,
            [LiveCount] int NOT NULL,
            [DeadCountPeriod] int NOT NULL,
            [AverageGram] decimal(18,6) NOT NULL,
            [BiomassKg] decimal(18,6) NOT NULL,
            [FeedKgPeriod] decimal(18,6) NOT NULL,
            [BiomassGainKgPeriod] decimal(18,6) NOT NULL,
            [SurvivalPct] decimal(18,6) NOT NULL,
            [MortalityPctPeriod] decimal(18,6) NOT NULL,
            [Fcr] decimal(18,6) NOT NULL,
            [Adg] decimal(18,6) NOT NULL,
            [Sgr] decimal(18,6) NOT NULL,
            [CapacityUsagePct] decimal(18,6) NOT NULL,
            [ForecastBiomassKg30Days] decimal(18,6) NOT NULL,
            [HarvestReadinessScore] decimal(18,6) NOT NULL,
            [DataQualityScore] decimal(18,6) NOT NULL,
            [FormulaNote] nvarchar(1000) NULL,
            [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_ProjectCageDailyKpiSnapshot_CreatedDate] DEFAULT(GETDATE()),
            [UpdatedDate] datetime2 NULL,
            [DeletedDate] datetime2 NULL,
            [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_ProjectCageDailyKpiSnapshot_IsDeleted] DEFAULT(0),
            [CreatedBy] bigint NULL,
            [UpdatedBy] bigint NULL,
            [DeletedBy] bigint NULL,
            CONSTRAINT [PK_RII_ProjectCageDailyKpiSnapshot] PRIMARY KEY ([Id])
        );
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[RII_WelfareAssessment] (
            [Id] bigint IDENTITY(1,1) NOT NULL,
            [ProjectId] bigint NOT NULL,
            [ProjectCageId] bigint NULL,
            [FishBatchId] bigint NULL,
            [AssessmentDate] datetime2(3) NOT NULL,
            [WelfareScore] decimal(18,6) NOT NULL,
            [StockingDensityKgM3] decimal(18,6) NULL,
            [AppetiteScore] decimal(18,6) NULL,
            [BehaviorScore] decimal(18,6) NULL,
            [GillScore] decimal(18,6) NULL,
            [SkinScore] decimal(18,6) NULL,
            [FinScore] decimal(18,6) NULL,
            [AssessedBy] nvarchar(150) NULL,
            [Observation] nvarchar(1000) NULL,
            [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_WelfareAssessment_CreatedDate] DEFAULT(GETDATE()),
            [UpdatedDate] datetime2 NULL,
            [DeletedDate] datetime2 NULL,
            [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_WelfareAssessment_IsDeleted] DEFAULT(0),
            [CreatedBy] bigint NULL,
            [UpdatedBy] bigint NULL,
            [DeletedBy] bigint NULL,
            CONSTRAINT [PK_RII_WelfareAssessment] PRIMARY KEY ([Id])
        );
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_ComplianceAudit_ProjectId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_ComplianceAudit]')
       )
    BEGIN
        CREATE INDEX [IX_RII_ComplianceAudit_ProjectId] ON [dbo].[RII_ComplianceAudit] ([ProjectId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_ComplianceAudit_ProjectCageId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_ComplianceAudit]')
       )
    BEGIN
        CREATE INDEX [IX_RII_ComplianceAudit_ProjectCageId] ON [dbo].[RII_ComplianceAudit] ([ProjectCageId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_ComplianceAudit_FishBatchId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_ComplianceAudit]')
       )
    BEGIN
        CREATE INDEX [IX_RII_ComplianceAudit_FishBatchId] ON [dbo].[RII_ComplianceAudit] ([FishBatchId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_ComplianceAudit_CreatedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_ComplianceAudit]')
       )
    BEGIN
        CREATE INDEX [IX_RII_ComplianceAudit_CreatedBy] ON [dbo].[RII_ComplianceAudit] ([CreatedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_ComplianceAudit_DeletedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_ComplianceAudit]')
       )
    BEGIN
        CREATE INDEX [IX_RII_ComplianceAudit_DeletedBy] ON [dbo].[RII_ComplianceAudit] ([DeletedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_ComplianceAudit_UpdatedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_ComplianceAudit]')
       )
    BEGIN
        CREATE INDEX [IX_RII_ComplianceAudit_UpdatedBy] ON [dbo].[RII_ComplianceAudit] ([UpdatedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_ComplianceCorrectiveAction_ComplianceAuditId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]')
       )
    BEGIN
        CREATE INDEX [IX_RII_ComplianceCorrectiveAction_ComplianceAuditId] ON [dbo].[RII_ComplianceCorrectiveAction] ([ComplianceAuditId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_ComplianceCorrectiveAction_CreatedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]')
       )
    BEGIN
        CREATE INDEX [IX_RII_ComplianceCorrectiveAction_CreatedBy] ON [dbo].[RII_ComplianceCorrectiveAction] ([CreatedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_ComplianceCorrectiveAction_DeletedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]')
       )
    BEGIN
        CREATE INDEX [IX_RII_ComplianceCorrectiveAction_DeletedBy] ON [dbo].[RII_ComplianceCorrectiveAction] ([DeletedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_ComplianceCorrectiveAction_UpdatedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]')
       )
    BEGIN
        CREATE INDEX [IX_RII_ComplianceCorrectiveAction_UpdatedBy] ON [dbo].[RII_ComplianceCorrectiveAction] ([UpdatedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishHealthEvent_ProjectId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishHealthEvent]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishHealthEvent_ProjectId] ON [dbo].[RII_FishHealthEvent] ([ProjectId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishHealthEvent_ProjectCageId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishHealthEvent]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishHealthEvent_ProjectCageId] ON [dbo].[RII_FishHealthEvent] ([ProjectCageId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishHealthEvent_FishBatchId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishHealthEvent]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishHealthEvent_FishBatchId] ON [dbo].[RII_FishHealthEvent] ([FishBatchId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishHealthEvent_CreatedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishHealthEvent]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishHealthEvent_CreatedBy] ON [dbo].[RII_FishHealthEvent] ([CreatedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishHealthEvent_DeletedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishHealthEvent]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishHealthEvent_DeletedBy] ON [dbo].[RII_FishHealthEvent] ([DeletedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishHealthEvent_UpdatedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishHealthEvent]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishHealthEvent_UpdatedBy] ON [dbo].[RII_FishHealthEvent] ([UpdatedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishLabSample_ProjectId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishLabSample]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishLabSample_ProjectId] ON [dbo].[RII_FishLabSample] ([ProjectId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishLabSample_ProjectCageId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishLabSample]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishLabSample_ProjectCageId] ON [dbo].[RII_FishLabSample] ([ProjectCageId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishLabSample_FishBatchId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishLabSample]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishLabSample_FishBatchId] ON [dbo].[RII_FishLabSample] ([FishBatchId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishLabSample_FishHealthEventId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishLabSample]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishLabSample_FishHealthEventId] ON [dbo].[RII_FishLabSample] ([FishHealthEventId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishLabSample_CreatedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishLabSample]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishLabSample_CreatedBy] ON [dbo].[RII_FishLabSample] ([CreatedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishLabSample_DeletedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishLabSample]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishLabSample_DeletedBy] ON [dbo].[RII_FishLabSample] ([DeletedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishLabSample_UpdatedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishLabSample]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishLabSample_UpdatedBy] ON [dbo].[RII_FishLabSample] ([UpdatedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabResult]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishLabResult_FishLabSampleId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishLabResult]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishLabResult_FishLabSampleId] ON [dbo].[RII_FishLabResult] ([FishLabSampleId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabResult]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishLabResult_CreatedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishLabResult]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishLabResult_CreatedBy] ON [dbo].[RII_FishLabResult] ([CreatedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabResult]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishLabResult_DeletedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishLabResult]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishLabResult_DeletedBy] ON [dbo].[RII_FishLabResult] ([DeletedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabResult]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishLabResult_UpdatedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishLabResult]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishLabResult_UpdatedBy] ON [dbo].[RII_FishLabResult] ([UpdatedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishTreatment_ProjectId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishTreatment]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishTreatment_ProjectId] ON [dbo].[RII_FishTreatment] ([ProjectId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishTreatment_ProjectCageId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishTreatment]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishTreatment_ProjectCageId] ON [dbo].[RII_FishTreatment] ([ProjectCageId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishTreatment_FishBatchId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishTreatment]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishTreatment_FishBatchId] ON [dbo].[RII_FishTreatment] ([FishBatchId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishTreatment_FishHealthEventId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishTreatment]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishTreatment_FishHealthEventId] ON [dbo].[RII_FishTreatment] ([FishHealthEventId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishTreatment_CreatedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishTreatment]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishTreatment_CreatedBy] ON [dbo].[RII_FishTreatment] ([CreatedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishTreatment_DeletedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishTreatment]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishTreatment_DeletedBy] ON [dbo].[RII_FishTreatment] ([DeletedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_FishTreatment_UpdatedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_FishTreatment]')
       )
    BEGIN
        CREATE INDEX [IX_RII_FishTreatment_UpdatedBy] ON [dbo].[RII_FishTreatment] ([UpdatedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_ProjectCageDailyKpiSnapshot_ProjectId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]')
       )
    BEGIN
        CREATE INDEX [IX_RII_ProjectCageDailyKpiSnapshot_ProjectId] ON [dbo].[RII_ProjectCageDailyKpiSnapshot] ([ProjectId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_ProjectCageDailyKpiSnapshot_ProjectCageId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]')
       )
    BEGIN
        CREATE INDEX [IX_RII_ProjectCageDailyKpiSnapshot_ProjectCageId] ON [dbo].[RII_ProjectCageDailyKpiSnapshot] ([ProjectCageId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_ProjectCageDailyKpiSnapshot_FishBatchId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]')
       )
    BEGIN
        CREATE INDEX [IX_RII_ProjectCageDailyKpiSnapshot_FishBatchId] ON [dbo].[RII_ProjectCageDailyKpiSnapshot] ([FishBatchId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_ProjectCageDailyKpiSnapshot_CreatedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]')
       )
    BEGIN
        CREATE INDEX [IX_RII_ProjectCageDailyKpiSnapshot_CreatedBy] ON [dbo].[RII_ProjectCageDailyKpiSnapshot] ([CreatedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_ProjectCageDailyKpiSnapshot_DeletedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]')
       )
    BEGIN
        CREATE INDEX [IX_RII_ProjectCageDailyKpiSnapshot_DeletedBy] ON [dbo].[RII_ProjectCageDailyKpiSnapshot] ([DeletedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_ProjectCageDailyKpiSnapshot_UpdatedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]')
       )
    BEGIN
        CREATE INDEX [IX_RII_ProjectCageDailyKpiSnapshot_UpdatedBy] ON [dbo].[RII_ProjectCageDailyKpiSnapshot] ([UpdatedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_WelfareAssessment_ProjectId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_WelfareAssessment]')
       )
    BEGIN
        CREATE INDEX [IX_RII_WelfareAssessment_ProjectId] ON [dbo].[RII_WelfareAssessment] ([ProjectId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_WelfareAssessment_ProjectCageId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_WelfareAssessment]')
       )
    BEGIN
        CREATE INDEX [IX_RII_WelfareAssessment_ProjectCageId] ON [dbo].[RII_WelfareAssessment] ([ProjectCageId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_WelfareAssessment_FishBatchId'
             AND object_id = OBJECT_ID(N'[dbo].[RII_WelfareAssessment]')
       )
    BEGIN
        CREATE INDEX [IX_RII_WelfareAssessment_FishBatchId] ON [dbo].[RII_WelfareAssessment] ([FishBatchId]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_WelfareAssessment_CreatedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_WelfareAssessment]')
       )
    BEGIN
        CREATE INDEX [IX_RII_WelfareAssessment_CreatedBy] ON [dbo].[RII_WelfareAssessment] ([CreatedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_WelfareAssessment_DeletedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_WelfareAssessment]')
       )
    BEGIN
        CREATE INDEX [IX_RII_WelfareAssessment_DeletedBy] ON [dbo].[RII_WelfareAssessment] ([DeletedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.indexes
           WHERE name = N'IX_RII_WelfareAssessment_UpdatedBy'
             AND object_id = OBJECT_ID(N'[dbo].[RII_WelfareAssessment]')
       )
    BEGIN
        CREATE INDEX [IX_RII_WelfareAssessment_UpdatedBy] ON [dbo].[RII_WelfareAssessment] ([UpdatedBy]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_ComplianceAudit', 'ProjectId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_Project]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_ComplianceAudit_RII_Project_ProjectId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_ComplianceAudit]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_ComplianceAudit] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_ComplianceAudit_RII_Project_ProjectId]
        FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[RII_Project] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_ComplianceAudit', 'ProjectCageId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_ProjectCage]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_ComplianceAudit_RII_ProjectCage_ProjectCageId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_ComplianceAudit]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_ComplianceAudit] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_ComplianceAudit_RII_ProjectCage_ProjectCageId]
        FOREIGN KEY ([ProjectCageId]) REFERENCES [dbo].[RII_ProjectCage] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_ComplianceAudit', 'FishBatchId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_FishBatch]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_ComplianceAudit_RII_FishBatch_FishBatchId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_ComplianceAudit]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_ComplianceAudit] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_ComplianceAudit_RII_FishBatch_FishBatchId]
        FOREIGN KEY ([FishBatchId]) REFERENCES [dbo].[RII_FishBatch] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_ComplianceAudit', 'CreatedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_ComplianceAudit_RII_USERS_CreatedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_ComplianceAudit]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_ComplianceAudit] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_ComplianceAudit_RII_USERS_CreatedBy]
        FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_ComplianceAudit', 'DeletedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_ComplianceAudit_RII_USERS_DeletedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_ComplianceAudit]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_ComplianceAudit] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_ComplianceAudit_RII_USERS_DeletedBy]
        FOREIGN KEY ([DeletedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_ComplianceAudit', 'UpdatedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_ComplianceAudit_RII_USERS_UpdatedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_ComplianceAudit]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_ComplianceAudit] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_ComplianceAudit_RII_USERS_UpdatedBy]
        FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_ComplianceCorrectiveAction', 'ComplianceAuditId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_ComplianceCorrectiveAction_RII_ComplianceAudit_ComplianceAuditId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_ComplianceCorrectiveAction] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_ComplianceCorrectiveAction_RII_ComplianceAudit_ComplianceAuditId]
        FOREIGN KEY ([ComplianceAuditId]) REFERENCES [dbo].[RII_ComplianceAudit] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_ComplianceCorrectiveAction', 'CreatedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_ComplianceCorrectiveAction_RII_USERS_CreatedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_ComplianceCorrectiveAction] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_ComplianceCorrectiveAction_RII_USERS_CreatedBy]
        FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_ComplianceCorrectiveAction', 'DeletedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_ComplianceCorrectiveAction_RII_USERS_DeletedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_ComplianceCorrectiveAction] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_ComplianceCorrectiveAction_RII_USERS_DeletedBy]
        FOREIGN KEY ([DeletedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_ComplianceCorrectiveAction', 'UpdatedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_ComplianceCorrectiveAction_RII_USERS_UpdatedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_ComplianceCorrectiveAction] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_ComplianceCorrectiveAction_RII_USERS_UpdatedBy]
        FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishHealthEvent', 'ProjectId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_Project]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishHealthEvent_RII_Project_ProjectId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishHealthEvent]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishHealthEvent] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishHealthEvent_RII_Project_ProjectId]
        FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[RII_Project] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishHealthEvent', 'ProjectCageId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_ProjectCage]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishHealthEvent_RII_ProjectCage_ProjectCageId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishHealthEvent]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishHealthEvent] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishHealthEvent_RII_ProjectCage_ProjectCageId]
        FOREIGN KEY ([ProjectCageId]) REFERENCES [dbo].[RII_ProjectCage] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishHealthEvent', 'FishBatchId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_FishBatch]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishHealthEvent_RII_FishBatch_FishBatchId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishHealthEvent]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishHealthEvent] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishHealthEvent_RII_FishBatch_FishBatchId]
        FOREIGN KEY ([FishBatchId]) REFERENCES [dbo].[RII_FishBatch] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishHealthEvent', 'CreatedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishHealthEvent_RII_USERS_CreatedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishHealthEvent]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishHealthEvent] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishHealthEvent_RII_USERS_CreatedBy]
        FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishHealthEvent', 'DeletedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishHealthEvent_RII_USERS_DeletedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishHealthEvent]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishHealthEvent] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishHealthEvent_RII_USERS_DeletedBy]
        FOREIGN KEY ([DeletedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishHealthEvent', 'UpdatedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishHealthEvent_RII_USERS_UpdatedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishHealthEvent]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishHealthEvent] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishHealthEvent_RII_USERS_UpdatedBy]
        FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishLabSample', 'ProjectId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_Project]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishLabSample_RII_Project_ProjectId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishLabSample]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishLabSample] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishLabSample_RII_Project_ProjectId]
        FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[RII_Project] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishLabSample', 'ProjectCageId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_ProjectCage]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishLabSample_RII_ProjectCage_ProjectCageId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishLabSample]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishLabSample] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishLabSample_RII_ProjectCage_ProjectCageId]
        FOREIGN KEY ([ProjectCageId]) REFERENCES [dbo].[RII_ProjectCage] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishLabSample', 'FishBatchId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_FishBatch]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishLabSample_RII_FishBatch_FishBatchId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishLabSample]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishLabSample] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishLabSample_RII_FishBatch_FishBatchId]
        FOREIGN KEY ([FishBatchId]) REFERENCES [dbo].[RII_FishBatch] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishLabSample', 'CreatedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishLabSample_RII_USERS_CreatedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishLabSample]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishLabSample] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishLabSample_RII_USERS_CreatedBy]
        FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishLabSample', 'DeletedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishLabSample_RII_USERS_DeletedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishLabSample]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishLabSample] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishLabSample_RII_USERS_DeletedBy]
        FOREIGN KEY ([DeletedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishLabSample', 'UpdatedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishLabSample_RII_USERS_UpdatedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishLabSample]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishLabSample] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishLabSample_RII_USERS_UpdatedBy]
        FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishLabSample', 'FishHealthEventId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishLabSample_RII_FishHealthEvent_FishHealthEventId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishLabSample]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishLabSample] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishLabSample_RII_FishHealthEvent_FishHealthEventId]
        FOREIGN KEY ([FishHealthEventId]) REFERENCES [dbo].[RII_FishHealthEvent] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabResult]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishLabResult', 'FishLabSampleId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishLabResult_RII_FishLabSample_FishLabSampleId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishLabResult]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishLabResult] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishLabResult_RII_FishLabSample_FishLabSampleId]
        FOREIGN KEY ([FishLabSampleId]) REFERENCES [dbo].[RII_FishLabSample] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabResult]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishLabResult', 'CreatedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishLabResult_RII_USERS_CreatedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishLabResult]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishLabResult] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishLabResult_RII_USERS_CreatedBy]
        FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabResult]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishLabResult', 'DeletedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishLabResult_RII_USERS_DeletedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishLabResult]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishLabResult] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishLabResult_RII_USERS_DeletedBy]
        FOREIGN KEY ([DeletedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishLabResult]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishLabResult', 'UpdatedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishLabResult_RII_USERS_UpdatedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishLabResult]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishLabResult] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishLabResult_RII_USERS_UpdatedBy]
        FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishTreatment', 'ProjectId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_Project]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishTreatment_RII_Project_ProjectId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishTreatment]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishTreatment] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishTreatment_RII_Project_ProjectId]
        FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[RII_Project] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishTreatment', 'ProjectCageId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_ProjectCage]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishTreatment_RII_ProjectCage_ProjectCageId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishTreatment]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishTreatment] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishTreatment_RII_ProjectCage_ProjectCageId]
        FOREIGN KEY ([ProjectCageId]) REFERENCES [dbo].[RII_ProjectCage] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishTreatment', 'FishBatchId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_FishBatch]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishTreatment_RII_FishBatch_FishBatchId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishTreatment]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishTreatment] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishTreatment_RII_FishBatch_FishBatchId]
        FOREIGN KEY ([FishBatchId]) REFERENCES [dbo].[RII_FishBatch] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishTreatment', 'CreatedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishTreatment_RII_USERS_CreatedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishTreatment]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishTreatment] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishTreatment_RII_USERS_CreatedBy]
        FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishTreatment', 'DeletedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishTreatment_RII_USERS_DeletedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishTreatment]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishTreatment] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishTreatment_RII_USERS_DeletedBy]
        FOREIGN KEY ([DeletedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishTreatment', 'UpdatedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishTreatment_RII_USERS_UpdatedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishTreatment]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishTreatment] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishTreatment_RII_USERS_UpdatedBy]
        FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_FishTreatment', 'FishHealthEventId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_FishTreatment_RII_FishHealthEvent_FishHealthEventId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_FishTreatment]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_FishTreatment] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_FishTreatment_RII_FishHealthEvent_FishHealthEventId]
        FOREIGN KEY ([FishHealthEventId]) REFERENCES [dbo].[RII_FishHealthEvent] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_ProjectCageDailyKpiSnapshot', 'ProjectId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_Project]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_ProjectCageDailyKpiSnapshot_RII_Project_ProjectId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_ProjectCageDailyKpiSnapshot] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_ProjectCageDailyKpiSnapshot_RII_Project_ProjectId]
        FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[RII_Project] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_ProjectCageDailyKpiSnapshot', 'ProjectCageId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_ProjectCage]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_ProjectCageDailyKpiSnapshot_RII_ProjectCage_ProjectCageId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_ProjectCageDailyKpiSnapshot] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_ProjectCageDailyKpiSnapshot_RII_ProjectCage_ProjectCageId]
        FOREIGN KEY ([ProjectCageId]) REFERENCES [dbo].[RII_ProjectCage] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_ProjectCageDailyKpiSnapshot', 'FishBatchId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_FishBatch]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_ProjectCageDailyKpiSnapshot_RII_FishBatch_FishBatchId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_ProjectCageDailyKpiSnapshot] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_ProjectCageDailyKpiSnapshot_RII_FishBatch_FishBatchId]
        FOREIGN KEY ([FishBatchId]) REFERENCES [dbo].[RII_FishBatch] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_ProjectCageDailyKpiSnapshot', 'CreatedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_ProjectCageDailyKpiSnapshot_RII_USERS_CreatedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_ProjectCageDailyKpiSnapshot] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_ProjectCageDailyKpiSnapshot_RII_USERS_CreatedBy]
        FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_ProjectCageDailyKpiSnapshot', 'DeletedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_ProjectCageDailyKpiSnapshot_RII_USERS_DeletedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_ProjectCageDailyKpiSnapshot] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_ProjectCageDailyKpiSnapshot_RII_USERS_DeletedBy]
        FOREIGN KEY ([DeletedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_ProjectCageDailyKpiSnapshot', 'UpdatedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_ProjectCageDailyKpiSnapshot_RII_USERS_UpdatedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_ProjectCageDailyKpiSnapshot] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_ProjectCageDailyKpiSnapshot_RII_USERS_UpdatedBy]
        FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_WelfareAssessment', 'ProjectId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_Project]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_WelfareAssessment_RII_Project_ProjectId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_WelfareAssessment]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_WelfareAssessment] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_WelfareAssessment_RII_Project_ProjectId]
        FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[RII_Project] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_WelfareAssessment', 'ProjectCageId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_ProjectCage]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_WelfareAssessment_RII_ProjectCage_ProjectCageId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_WelfareAssessment]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_WelfareAssessment] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_WelfareAssessment_RII_ProjectCage_ProjectCageId]
        FOREIGN KEY ([ProjectCageId]) REFERENCES [dbo].[RII_ProjectCage] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_WelfareAssessment', 'FishBatchId') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_FishBatch]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_WelfareAssessment_RII_FishBatch_FishBatchId'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_WelfareAssessment]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_WelfareAssessment] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_WelfareAssessment_RII_FishBatch_FishBatchId]
        FOREIGN KEY ([FishBatchId]) REFERENCES [dbo].[RII_FishBatch] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_WelfareAssessment', 'CreatedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_WelfareAssessment_RII_USERS_CreatedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_WelfareAssessment]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_WelfareAssessment] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_WelfareAssessment_RII_USERS_CreatedBy]
        FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_WelfareAssessment', 'DeletedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_WelfareAssessment_RII_USERS_DeletedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_WelfareAssessment]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_WelfareAssessment] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_WelfareAssessment_RII_USERS_DeletedBy]
        FOREIGN KEY ([DeletedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NOT NULL
       AND COL_LENGTH('dbo.RII_WelfareAssessment', 'UpdatedBy') IS NOT NULL
       AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = N'FK_RII_WelfareAssessment_RII_USERS_UpdatedBy'
             AND parent_object_id = OBJECT_ID(N'[dbo].[RII_WelfareAssessment]')
       )
    BEGIN
        ALTER TABLE [dbo].[RII_WelfareAssessment] WITH NOCHECK
        ADD CONSTRAINT [FK_RII_WelfareAssessment_RII_USERS_UpdatedBy]
        FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424093000_RepairMissingAquaSchemaDrift'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260424093000_RepairMissingAquaSchemaDrift', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518060055_AddCageWarehouseMappings'
)
BEGIN
    CREATE TABLE [RII_CageWarehouseMapping] (
        [Id] bigint NOT NULL IDENTITY,
        [CageId] bigint NOT NULL,
        [WarehouseId] bigint NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [Note] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_CageWarehouseMapping] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RII_CageWarehouseMapping_RII_Cage_CageId] FOREIGN KEY ([CageId]) REFERENCES [RII_Cage] ([Id]),
        CONSTRAINT [FK_RII_CageWarehouseMapping_RII_USERS_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_CageWarehouseMapping_RII_USERS_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_CageWarehouseMapping_RII_USERS_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [RII_USERS] ([Id]),
        CONSTRAINT [FK_RII_CageWarehouseMapping_RII_Warehouse_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [RII_Warehouse] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518060055_AddCageWarehouseMappings'
)
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseMapping_CreatedBy] ON [RII_CageWarehouseMapping] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518060055_AddCageWarehouseMappings'
)
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseMapping_DeletedBy] ON [RII_CageWarehouseMapping] ([DeletedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518060055_AddCageWarehouseMappings'
)
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseMapping_UpdatedBy] ON [RII_CageWarehouseMapping] ([UpdatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518060055_AddCageWarehouseMappings'
)
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseMapping_WarehouseId] ON [RII_CageWarehouseMapping] ([WarehouseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518060055_AddCageWarehouseMappings'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_RII_CageWarehouseMapping_Cage_Active] ON [RII_CageWarehouseMapping] ([CageId]) WHERE [IsDeleted] = 0 AND [IsActive] = 1');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518060055_AddCageWarehouseMappings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260518060055_AddCageWarehouseMappings', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530122542_AddUserManagerRelation'
)
BEGIN
    ALTER TABLE [RII_USERS] ADD [ManagerUserId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530122542_AddUserManagerRelation'
)
BEGIN
    CREATE INDEX [IX_Users_ManagerUserId] ON [RII_USERS] ([ManagerUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530122542_AddUserManagerRelation'
)
BEGIN
    ALTER TABLE [RII_USERS] ADD CONSTRAINT [FK_RII_USERS_RII_USERS_ManagerUserId] FOREIGN KEY ([ManagerUserId]) REFERENCES [RII_USERS] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530122542_AddUserManagerRelation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260530122542_AddUserManagerRelation', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602075010_DecoupleWeatherSeverityFromType'
)
BEGIN
    ALTER TABLE [RII_WeatherSeverity] DROP CONSTRAINT [FK_RII_WeatherSeverity_RII_WeatherType_WeatherTypeId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602075010_DecoupleWeatherSeverityFromType'
)
BEGIN
    DROP INDEX [UX_RII_WeatherSeverity_WeatherType_Code_Active] ON [RII_WeatherSeverity];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602075010_DecoupleWeatherSeverityFromType'
)
BEGIN
    DROP INDEX [IX_RII_WeatherSeverity_WeatherTypeId] ON [RII_WeatherSeverity];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602075010_DecoupleWeatherSeverityFromType'
)
BEGIN
    DECLARE @var47 sysname;
    SELECT @var47 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RII_WeatherSeverity]') AND [c].[name] = N'WeatherTypeId');
    IF @var47 IS NOT NULL EXEC(N'ALTER TABLE [RII_WeatherSeverity] DROP CONSTRAINT [' + @var47 + '];');
    ALTER TABLE [RII_WeatherSeverity] DROP COLUMN [WeatherTypeId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602075010_DecoupleWeatherSeverityFromType'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_RII_WeatherSeverity_Code_Active] ON [RII_WeatherSeverity] ([Code]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602075010_DecoupleWeatherSeverityFromType'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260602075010_DecoupleWeatherSeverityFromType', N'8.0.11');
END;
GO

COMMIT;
GO

