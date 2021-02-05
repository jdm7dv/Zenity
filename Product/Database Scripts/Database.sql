/*
Deployment script for Zentity
*/

GO
SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;

SET NUMERIC_ROUNDABORT OFF;


GO

GO
DECLARE @BracketEscapedDatabaseName nvarchar(256),@StringEscapedDatabaseName nvarchar(256),@BracketAndStringEscapedDatabaseName nvarchar(256);
SELECT @BracketEscapedDatabaseName = REPLACE(DB_NAME(), ']', ']]');
SELECT @StringEscapedDatabaseName = REPLACE(DB_NAME(), '''', '''''');
SELECT @BracketAndStringEscapedDatabaseName = REPLACE(@BracketEscapedDatabaseName, '''', '''''');
DECLARE @CmdAlterDatabase nvarchar(max);
SELECT @CmdAlterDatabase = N'


IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'''+@StringEscapedDatabaseName+''')
    BEGIN
        ALTER DATABASE ['+@BracketEscapedDatabaseName+']
            SET ANSI_NULLS ON,
                ANSI_PADDING ON,
                ANSI_WARNINGS ON,
                ARITHABORT ON,
                CONCAT_NULL_YIELDS_NULL ON,
                NUMERIC_ROUNDABORT OFF,
                QUOTED_IDENTIFIER ON,
                ANSI_NULL_DEFAULT ON,
                CURSOR_DEFAULT LOCAL,
                RECOVERY FULL,
                CURSOR_CLOSE_ON_COMMIT OFF,
                AUTO_CREATE_STATISTICS ON,
                AUTO_SHRINK OFF,
                AUTO_UPDATE_STATISTICS ON,
                RECURSIVE_TRIGGERS OFF 
            WITH ROLLBACK IMMEDIATE;
        ALTER DATABASE ['+@BracketEscapedDatabaseName+']
            SET AUTO_CLOSE OFF 
            WITH ROLLBACK IMMEDIATE;
    END


IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'''+@StringEscapedDatabaseName+''')
    BEGIN
        ALTER DATABASE ['+@BracketEscapedDatabaseName+']
            SET ALLOW_SNAPSHOT_ISOLATION OFF;
    END


IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'''+@StringEscapedDatabaseName+''')
    BEGIN
        ALTER DATABASE ['+@BracketEscapedDatabaseName+']
            SET READ_COMMITTED_SNAPSHOT OFF;
    END


IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'''+@StringEscapedDatabaseName+''')
    BEGIN
        ALTER DATABASE ['+@BracketEscapedDatabaseName+']
            SET AUTO_UPDATE_STATISTICS_ASYNC OFF,
                PAGE_VERIFY NONE,
                DATE_CORRELATION_OPTIMIZATION OFF,
                DISABLE_BROKER,
                PARAMETERIZATION SIMPLE 
            WITH ROLLBACK IMMEDIATE;
    END


IF IS_SRVROLEMEMBER(N''sysadmin'') = 1
    BEGIN
        IF EXISTS (SELECT 1
                   FROM   [master].[dbo].[sysdatabases]
                   WHERE  [name] = N'''+@StringEscapedDatabaseName+''')
            BEGIN
                EXECUTE sp_executesql N''ALTER DATABASE ['+@BracketAndStringEscapedDatabaseName+']
    SET TRUSTWORTHY OFF,
        DB_CHAINING OFF 
    WITH ROLLBACK IMMEDIATE'';
            END
    END
ELSE
    BEGIN
        PRINT N''The database settings for DB_CHAINING or TRUSTWORTHY cannot be modified. You must be a SysAdmin to apply these settings.'';
    END


IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'''+@StringEscapedDatabaseName+''')
    BEGIN
        ALTER DATABASE ['+@BracketEscapedDatabaseName+']
            SET HONOR_BROKER_PRIORITY OFF 
            WITH ROLLBACK IMMEDIATE;
    END



'
EXEC(@CmdAlterDatabase);
GO

GO
IF fulltextserviceproperty(N'IsFulltextInstalled') = 1
    EXECUTE sp_fulltext_database 'enable';


GO

GO

GO
PRINT N'Creating Administration...';


GO
CREATE SCHEMA [Administration]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Core...';


GO
CREATE SCHEMA [Core]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Administration.Command...';


GO
CREATE TABLE [Administration].[Command] (
    [CommandName]  NVARCHAR (256) NOT NULL,
    [CommandValue] NVARCHAR (MAX) NOT NULL,
    PRIMARY KEY CLUSTERED ([CommandName] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating Core.AfterSchemaChangesHandler...';


GO
CREATE TABLE [Core].[AfterSchemaChangesHandler] (
    [HandlerName] NVARCHAR (256) NOT NULL,
    [Query]       NVARCHAR (MAX) NOT NULL,
    PRIMARY KEY CLUSTERED ([HandlerName] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating Core.Association...';


GO
CREATE TABLE [Core].[Association] (
    [Id]                          UNIQUEIDENTIFIER ROWGUIDCOL NOT NULL,
    [Name]                        NVARCHAR (100)   NOT NULL,
    [Uri]                         NVARCHAR (1024)  NULL,
    [SubjectNavigationPropertyId] UNIQUEIDENTIFIER NOT NULL,
    [ObjectNavigationPropertyId]  UNIQUEIDENTIFIER NOT NULL,
    [PredicateId]                 UNIQUEIDENTIFIER NOT NULL,
    [SubjectMultiplicity]         NVARCHAR (32)    NOT NULL,
    [ObjectMultiplicity]          NVARCHAR (32)    NOT NULL,
    [ViewName]                    NVARCHAR (150)   NULL,
    CONSTRAINT [PK_Association] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating Core.Configuration...';


GO
CREATE TABLE [Core].[Configuration] (
    [ConfigName]  NVARCHAR (256)  NOT NULL,
    [ConfigValue] NVARCHAR (4000) NOT NULL,
    CONSTRAINT [PK_Configuration] PRIMARY KEY CLUSTERED ([ConfigName] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating Core.Content...';


GO
CREATE TABLE [Core].[Content] (
    [ResourceId]    UNIQUEIDENTIFIER           ROWGUIDCOL NOT NULL,
    [Content]       VARBINARY (MAX) FILESTREAM NOT NULL,
    [FileExtension] NVARCHAR (128)             NULL,
    CONSTRAINT [PK_Content] PRIMARY KEY CLUSTERED ([ResourceId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF),
    UNIQUE NONCLUSTERED ([ResourceId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating Core.DataModelModule...';


GO
CREATE TABLE [Core].[DataModelModule] (
    [Id]          UNIQUEIDENTIFIER ROWGUIDCOL NOT NULL,
    [Namespace]   NVARCHAR (150)   NOT NULL,
    [Uri]         NVARCHAR (1024)  NULL,
    [Description] NVARCHAR (MAX)   NULL,
    [IsMsShipped] BIT              NOT NULL,
    CONSTRAINT [PK_DataModelModule] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating Core.NavigationProperty...';


GO
CREATE TABLE [Core].[NavigationProperty] (
    [Id]             UNIQUEIDENTIFIER ROWGUIDCOL NOT NULL,
    [ResourceTypeId] UNIQUEIDENTIFIER NOT NULL,
    [Name]           NVARCHAR (100)   NOT NULL,
    [Uri]            NVARCHAR (1024)  NULL,
    [Description]    NVARCHAR (MAX)   NULL,
    [TableName]      NVARCHAR (128)   NULL,
    [ColumnName]     NVARCHAR (100)   NULL,
    CONSTRAINT [PK_NavigationProperty] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating Core.Predicate...';


GO
CREATE TABLE [Core].[Predicate] (
    [Id]   UNIQUEIDENTIFIER ROWGUIDCOL NOT NULL,
    [Name] NVARCHAR (128)   NOT NULL,
    [Uri]  NVARCHAR (1024)  NOT NULL,
    CONSTRAINT [PK_Predicate] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating Core.PredicateProperty...';


GO
CREATE TABLE [Core].[PredicateProperty] (
    [Id]          UNIQUEIDENTIFIER ROWGUIDCOL NOT NULL,
    [PredicateId] UNIQUEIDENTIFIER NOT NULL,
    [PropertyId]  UNIQUEIDENTIFIER NOT NULL,
    [Value]       NVARCHAR (MAX)   NOT NULL,
    CONSTRAINT [PK_PredicateProperty] PRIMARY KEY NONCLUSTERED ([Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating Core.Property...';


GO
CREATE TABLE [Core].[Property] (
    [Id]   UNIQUEIDENTIFIER ROWGUIDCOL NOT NULL,
    [Name] NVARCHAR (50)    NOT NULL,
    [Uri]  NVARCHAR (1024)  NULL,
    CONSTRAINT [PK_Property] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating Core.Relationship...';


GO
CREATE TABLE [Core].[Relationship] (
    [Id]                UNIQUEIDENTIFIER ROWGUIDCOL NOT NULL,
    [SubjectResourceId] UNIQUEIDENTIFIER NOT NULL,
    [ObjectResourceId]  UNIQUEIDENTIFIER NOT NULL,
    [PredicateId]       UNIQUEIDENTIFIER NOT NULL,
    [OrdinalPosition]   INT              NULL,
    [DateAdded]         DATETIME         NULL,
    CONSTRAINT [PK_Relationship] PRIMARY KEY NONCLUSTERED ([Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF),
    CONSTRAINT [UQ_Relationship] UNIQUE CLUSTERED ([SubjectResourceId] ASC, [ObjectResourceId] ASC, [PredicateId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating Core.RelationshipProperty...';


GO
CREATE TABLE [Core].[RelationshipProperty] (
    [Id]         UNIQUEIDENTIFIER ROWGUIDCOL NOT NULL,
    [TripletId]  UNIQUEIDENTIFIER NOT NULL,
    [PropertyId] UNIQUEIDENTIFIER NOT NULL,
    [Value]      NVARCHAR (MAX)   NOT NULL,
    CONSTRAINT [PK_RelationshipProperty] PRIMARY KEY NONCLUSTERED ([Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating Core.Resource...';


GO
CREATE TABLE [Core].[Resource] (
    [Id]             UNIQUEIDENTIFIER ROWGUIDCOL NOT NULL,
    [ResourceTypeId] UNIQUEIDENTIFIER NOT NULL,
    [Discriminator]  INT              NOT NULL,
    [DateAdded]      DATETIME         NULL,
    [DateModified]   DATETIME         NULL,
    [Description]    NVARCHAR (MAX)   NULL,
    [Title]          NVARCHAR (425)   NULL,
    [Uri]            NVARCHAR (1024)  NULL,
    [Checksum]       NVARCHAR (256)   NULL,
    [FileExtension]  NVARCHAR (128)   NULL,
    [MimeType]       NVARCHAR (128)   NULL,
    [Size]           BIGINT           NULL,
    CONSTRAINT [PK_Resource] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating Core.ResourceProperty...';


GO
CREATE TABLE [Core].[ResourceProperty] (
    [Id]         UNIQUEIDENTIFIER ROWGUIDCOL NOT NULL,
    [ResourceId] UNIQUEIDENTIFIER NOT NULL,
    [PropertyId] UNIQUEIDENTIFIER NOT NULL,
    [Value]      NVARCHAR (MAX)   NOT NULL,
    CONSTRAINT [PK_ResourceProperty] PRIMARY KEY NONCLUSTERED ([Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating Core.ResourceType...';


GO
CREATE TABLE [Core].[ResourceType] (
    [Id]                UNIQUEIDENTIFIER ROWGUIDCOL NOT NULL,
    [DataModelModuleId] UNIQUEIDENTIFIER NOT NULL,
    [BaseTypeId]        UNIQUEIDENTIFIER NULL,
    [Name]              NVARCHAR (100)   NOT NULL,
    [Uri]               NVARCHAR (1024)  NULL,
    [Description]       NVARCHAR (4000)  NULL,
    [Discriminator]     INT              NOT NULL,
    CONSTRAINT [PK_ResourceType] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating Core.ScalarProperty...';


GO
CREATE TABLE [Core].[ScalarProperty] (
    [Id]             UNIQUEIDENTIFIER ROWGUIDCOL NOT NULL,
    [ResourceTypeId] UNIQUEIDENTIFIER NOT NULL,
    [Name]           NVARCHAR (100)   NOT NULL,
    [Uri]            NVARCHAR (1024)  NULL,
    [Description]    NVARCHAR (MAX)   NULL,
    [DataType]       NVARCHAR (100)   NOT NULL,
    [Nullable]       BIT              NOT NULL,
    [MaxLength]      INT              NULL,
    [Scale]          INT              NULL,
    [Precision]      INT              NULL,
    [TableName]      NVARCHAR (128)   NOT NULL,
    [ColumnName]     NVARCHAR (100)   NOT NULL,
    CONSTRAINT [PK_ScalarProperty] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);


GO
PRINT N'Creating Core.Association.UQ_Association_Name...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Association_Name]
    ON [Core].[Association]([Name] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.Association.UQ_Association_PredicateId...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Association_PredicateId]
    ON [Core].[Association]([PredicateId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.DataModelModule.UQ_DataModelModule...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_DataModelModule]
    ON [Core].[DataModelModule]([Namespace] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.NavigationProperty.UQ_NavigationProperty...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_NavigationProperty]
    ON [Core].[NavigationProperty]([ResourceTypeId] ASC, [Name] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.Predicate.UQ_PredicateName...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_PredicateName]
    ON [Core].[Predicate]([Name] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.Property.UQ_PropertyName...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_PropertyName]
    ON [Core].[Property]([Name] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.Relationship.IX_RelationshipPredicate...';


GO
CREATE NONCLUSTERED INDEX [IX_RelationshipPredicate]
    ON [Core].[Relationship]([PredicateId] ASC, [SubjectResourceId] ASC, [ObjectResourceId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.RelationshipProperty.IX_RelationshipProperty...';


GO
CREATE CLUSTERED INDEX [IX_RelationshipProperty]
    ON [Core].[RelationshipProperty]([TripletId] ASC, [PropertyId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.Resource.IX_Discriminator...';


GO
CREATE NONCLUSTERED INDEX [IX_Discriminator]
    ON [Core].[Resource]([Discriminator] ASC, [Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.Resource.IX_File_MimeType...';


GO
CREATE NONCLUSTERED INDEX [IX_File_MimeType]
    ON [Core].[Resource]([MimeType] ASC, [Id] ASC, [Discriminator] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.Resource.IX_File_Size...';


GO
CREATE NONCLUSTERED INDEX [IX_File_Size]
    ON [Core].[Resource]([Size] ASC, [Id] ASC, [Discriminator] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.Resource.IX_Resource_DateAdded...';


GO
CREATE NONCLUSTERED INDEX [IX_Resource_DateAdded]
    ON [Core].[Resource]([DateAdded] ASC, [Id] ASC, [Discriminator] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.Resource.IX_Resource_DateModified...';


GO
CREATE NONCLUSTERED INDEX [IX_Resource_DateModified]
    ON [Core].[Resource]([DateModified] ASC, [Id] ASC, [Discriminator] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.Resource.IX_ResourceTypeId...';


GO
CREATE NONCLUSTERED INDEX [IX_ResourceTypeId]
    ON [Core].[Resource]([ResourceTypeId] ASC, [Id] ASC, [Discriminator] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.Resource.IX_Title...';


GO
CREATE NONCLUSTERED INDEX [IX_Title]
    ON [Core].[Resource]([Title] ASC, [Id] ASC, [Discriminator] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.ResourceProperty.IX_ResourceProperty...';


GO
CREATE CLUSTERED INDEX [IX_ResourceProperty]
    ON [Core].[ResourceProperty]([ResourceId] ASC, [PropertyId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.ResourceType.UQ_Discriminator...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Discriminator]
    ON [Core].[ResourceType]([Discriminator] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.ResourceType.UQ_ResourceType...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_ResourceType]
    ON [Core].[ResourceType]([DataModelModuleId] ASC, [Name] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.ResourceType.UQ_ResourceTypeIdAndDiscriminator...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_ResourceTypeIdAndDiscriminator]
    ON [Core].[ResourceType]([Id] ASC, [Discriminator] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.ScalarProperty.UQ_ScalarProperty...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_ScalarProperty]
    ON [Core].[ScalarProperty]([ResourceTypeId] ASC, [Name] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating On column: IsMsShipped...';


GO
ALTER TABLE [Core].[DataModelModule]
    ADD DEFAULT 0 FOR [IsMsShipped];


GO
PRINT N'Creating Core.AssociationHasObjectNavigationProperty...';


GO
ALTER TABLE [Core].[Association]
    ADD CONSTRAINT [AssociationHasObjectNavigationProperty] FOREIGN KEY ([ObjectNavigationPropertyId]) REFERENCES [Core].[NavigationProperty] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating Core.AssociationHasPredicate...';


GO
ALTER TABLE [Core].[Association]
    ADD CONSTRAINT [AssociationHasPredicate] FOREIGN KEY ([PredicateId]) REFERENCES [Core].[Predicate] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating Core.AssociationHasSubjectNavigationProperty...';


GO
ALTER TABLE [Core].[Association]
    ADD CONSTRAINT [AssociationHasSubjectNavigationProperty] FOREIGN KEY ([SubjectNavigationPropertyId]) REFERENCES [Core].[NavigationProperty] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating On column: ResourceId...';


GO
ALTER TABLE [Core].[Content]
    ADD FOREIGN KEY ([ResourceId]) REFERENCES [Core].[Resource] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION;


GO
PRINT N'Creating Core.NavigationPropertyBelongsToResourceType...';


GO
ALTER TABLE [Core].[NavigationProperty]
    ADD CONSTRAINT [NavigationPropertyBelongsToResourceType] FOREIGN KEY ([ResourceTypeId]) REFERENCES [Core].[ResourceType] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating Core.PredicateHasProperty...';


GO
ALTER TABLE [Core].[PredicateProperty]
    ADD CONSTRAINT [PredicateHasProperty] FOREIGN KEY ([PredicateId]) REFERENCES [Core].[Predicate] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating Core.PredicatePropertyIs...';


GO
ALTER TABLE [Core].[PredicateProperty]
    ADD CONSTRAINT [PredicatePropertyIs] FOREIGN KEY ([PropertyId]) REFERENCES [Core].[Property] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating Core.RelationshipIsAboutResource...';


GO
ALTER TABLE [Core].[Relationship]
    ADD CONSTRAINT [RelationshipIsAboutResource] FOREIGN KEY ([ObjectResourceId]) REFERENCES [Core].[Resource] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating Core.RelationshipPredicateIs...';


GO
ALTER TABLE [Core].[Relationship]
    ADD CONSTRAINT [RelationshipPredicateIs] FOREIGN KEY ([PredicateId]) REFERENCES [Core].[Predicate] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating Core.ResourceHasRelationship...';


GO
ALTER TABLE [Core].[Relationship]
    ADD CONSTRAINT [ResourceHasRelationship] FOREIGN KEY ([SubjectResourceId]) REFERENCES [Core].[Resource] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating Core.RelationshipHasProperty...';


GO
ALTER TABLE [Core].[RelationshipProperty]
    ADD CONSTRAINT [RelationshipHasProperty] FOREIGN KEY ([TripletId]) REFERENCES [Core].[Relationship] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating Core.TripletPropertyIs...';


GO
ALTER TABLE [Core].[RelationshipProperty]
    ADD CONSTRAINT [TripletPropertyIs] FOREIGN KEY ([PropertyId]) REFERENCES [Core].[Property] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating Core.ResourceHasType...';


GO
ALTER TABLE [Core].[Resource]
    ADD CONSTRAINT [ResourceHasType] FOREIGN KEY ([ResourceTypeId]) REFERENCES [Core].[ResourceType] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating Core.ResourceHasProperty...';


GO
ALTER TABLE [Core].[ResourceProperty]
    ADD CONSTRAINT [ResourceHasProperty] FOREIGN KEY ([ResourceId]) REFERENCES [Core].[Resource] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating Core.ResourcePropertyIs...';


GO
ALTER TABLE [Core].[ResourceProperty]
    ADD CONSTRAINT [ResourcePropertyIs] FOREIGN KEY ([PropertyId]) REFERENCES [Core].[Property] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating Core.ResourceTypeBelongsToDataModelModule...';


GO
ALTER TABLE [Core].[ResourceType]
    ADD CONSTRAINT [ResourceTypeBelongsToDataModelModule] FOREIGN KEY ([DataModelModuleId]) REFERENCES [Core].[DataModelModule] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating Core.ResourceTypeDerivesFromResourceType...';


GO
ALTER TABLE [Core].[ResourceType]
    ADD CONSTRAINT [ResourceTypeDerivesFromResourceType] FOREIGN KEY ([BaseTypeId]) REFERENCES [Core].[ResourceType] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating Core.ScalarPropertyBelongsToResourceType...';


GO
ALTER TABLE [Core].[ScalarProperty]
    ADD CONSTRAINT [ScalarPropertyBelongsToResourceType] FOREIGN KEY ([ResourceTypeId]) REFERENCES [Core].[ResourceType] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


GO
PRINT N'Creating Core.ObjectMultiplicityHasValues...';


GO
ALTER TABLE [Core].[Association]
    ADD CONSTRAINT [ObjectMultiplicityHasValues] CHECK (([ObjectMultiplicity]='ZeroOrOne' OR [ObjectMultiplicity]='Many' OR [ObjectMultiplicity]='One'));


GO
PRINT N'Creating Core.SubjectMultiplicityHasValues...';


GO
ALTER TABLE [Core].[Association]
    ADD CONSTRAINT [SubjectMultiplicityHasValues] CHECK (([ObjectMultiplicity]='ZeroOrOne' OR [ObjectMultiplicity]='Many' OR [ObjectMultiplicity]='One'));


GO
PRINT N'Creating Core.CK_Predicate_Name...';


GO
ALTER TABLE [Core].[Predicate]
    ADD CONSTRAINT [CK_Predicate_Name] CHECK ([Name] <> '');


GO
PRINT N'Creating Core.CK_Property_Name...';


GO
ALTER TABLE [Core].[Property]
    ADD CONSTRAINT [CK_Property_Name] CHECK ([Name] <> '');


GO
PRINT N'Creating Administration.DisableChangeHistory...';


GO
CREATE PROCEDURE [Administration].[DisableChangeHistory]

AS
BEGIN
	BEGIN TRY
		-- Remove change history objects in a transaction.
		BEGIN TRANSACTION RemoveChangeHistoryObjects

			----------------------------------------------------------------------------------------
			-- Delete the LSN processing job.
			----------------------------------------------------------------------------------------
			IF EXISTS(SELECT 1 FROM msdb.dbo.sysjobs WHERE [name] = N'ProcessNextLSN')
			BEGIN
				RAISERROR(N'Removing LSN processing job.', 10, 1);
				EXEC msdb.dbo.sp_delete_job @job_name = N'ProcessNextLSN', @delete_unused_schedule=1
			END

			----------------------------------------------------------------------------------------
			-- Disable Change History on the database.
			-- Since is_cdc_enabled column is not present in 2K5, 
			-- we fire the following query dynamically.
			----------------------------------------------------------------------------------------
			EXEC('IF EXISTS(SELECT 1 FROM sys.databases 
				WHERE [database_id] = DB_ID() AND [is_cdc_enabled] = 1)
			BEGIN
				RAISERROR(N''Disabling CDC on database.'', 10, 1);
				EXEC sys.sp_cdc_disable_db;
			END'
			);

			----------------------------------------------------------------------------------------
			-- Un-Register the event handler for schema changes.
			----------------------------------------------------------------------------------------
			IF EXISTS(SELECT 1 FROM [Core].[AfterSchemaChangesHandler] 
				WHERE [HandlerName] = N'Administration.UpdateCaptureInstances')
			BEGIN
				RAISERROR(N'Un-registering schema changes event handler.', 10, 1);
				DELETE FROM [Core].[AfterSchemaChangesHandler] 
				WHERE [HandlerName] = N'Administration.UpdateCaptureInstances';
			END

			----------------------------------------------------------------------------------------
			-- Finally update the configuration value.
			----------------------------------------------------------------------------------------
			RAISERROR(N'Updating configuration value.', 10, 1);
			UPDATE [Core].[Configuration] SET [ConfigValue] = 'False' 
			WHERE [ConfigName] = N'IsChangeHistoryEnabled';

		COMMIT TRANSACTION RemoveChangeHistoryObjects
	END TRY
	BEGIN CATCH
		-- It is important to check for the presence of correct transaction before doing a ROLLBACK.
		IF EXISTS(SELECT 1 FROM sys.dm_tran_active_transactions 
			WHERE name=N'RemoveChangeHistoryObjects')
				ROLLBACK TRANSACTION RemoveChangeHistoryObjects;
		DECLARE @Msg nvarchar(4000);
		SELECT @Msg = ERROR_MESSAGE();
		RAISERROR(@Msg, 16, 1);
		GOTO FINISH;
	END CATCH

	FINISH:
END


GO
PRINT N'Creating Administration.DisableFullTextSearch...';


GO
CREATE PROCEDURE [Administration].[DisableFullTextSearch]

AS
BEGIN
	BEGIN TRY
		-- Remove full text search objects in a transaction.
		BEGIN TRANSACTION RemoveFullTextSearchObjects

			----------------------------------------------------------------------------------------
			-- Delete the Full Text update job.
			----------------------------------------------------------------------------------------
			IF EXISTS(SELECT 1 FROM msdb.dbo.sysjobs WHERE [name] = N'IncrementalIndexPopulation')
			BEGIN
				RAISERROR(N'Removing incremental index population job.', 10, 1);
				EXEC msdb.dbo.sp_delete_job @job_name = N'IncrementalIndexPopulation', 
					@delete_unused_schedule=1
			END

			----------------------------------------------------------------------------------------
			-- Finally update the configuration value.
			----------------------------------------------------------------------------------------
			RAISERROR(N'Updating configuration value.', 10, 1);
			UPDATE [Core].[Configuration] SET [ConfigValue] = 'False' 
			WHERE [ConfigName] = N'IsFullTextSearchEnabled';
			
		COMMIT TRANSACTION RemoveFullTextSearchObjects
	END TRY
	BEGIN CATCH
		-- It is important to check for the presence of correct transaction before doing a ROLLBACK.
		IF EXISTS(SELECT 1 FROM sys.dm_tran_active_transactions 
			WHERE name=N'RemoveFullTextSearchObjects')
				ROLLBACK TRANSACTION RemoveFullTextSearchObjects;
		DECLARE @Msg nvarchar(4000);
		SELECT @Msg = ERROR_MESSAGE();
		RAISERROR(@Msg, 16, 1);
		GOTO FINISH;
	END CATCH

	FINISH:
END


GO
PRINT N'Creating Core.AfterSchemaChanges...';


GO
CREATE PROCEDURE [Core].[AfterSchemaChanges]

AS
BEGIN
	-- Iterate through all the event handlers and execute their queries.
	DECLARE @Query nvarchar(max);
	DECLARE QueryCursor CURSOR FOR
	SELECT [Query] FROM [Core].[AfterSchemaChangesHandler];
	
	BEGIN TRY
		OPEN QueryCursor;
		FETCH NEXT FROM QueryCursor INTO @Query;
		WHILE @@FETCH_STATUS = 0
		BEGIN
			EXEC(@Query);
			FETCH NEXT FROM QueryCursor INTO @Query;
		END
		CLOSE QueryCursor;
		DEALLOCATE QueryCursor;	
	END TRY
	BEGIN CATCH
		IF(CURSOR_STATUS('local','QueryCursor')=0 OR CURSOR_STATUS('local','QueryCursor')=1)
		BEGIN
			CLOSE QueryCursor;
			DEALLOCATE QueryCursor;	
		END

		DECLARE @Msg nvarchar(4000);
		SELECT @Msg = ERROR_MESSAGE();
		RAISERROR(@Msg, 16, 1);				
	END CATCH
END


GO
PRINT N'Creating Core.CreateOrUpdateAssociation...';


GO
CREATE PROCEDURE [Core].[CreateOrUpdateAssociation]
@Id UNIQUEIDENTIFIER, @Name NVARCHAR (100), @Uri NVARCHAR (1024), @SubjectNavigationPropertyId UNIQUEIDENTIFIER, @ObjectNavigationPropertyId UNIQUEIDENTIFIER, @PredicateId UNIQUEIDENTIFIER, @SubjectMultiplicity NVARCHAR (32), @ObjectMultiplicity NVARCHAR (32), @ViewName NVARCHAR (150)
AS
BEGIN

	IF (SELECT COUNT(1) FROM [Core].[Association] WHERE Id = @Id )=0
	BEGIN
		INSERT INTO [Core].[Association]
           ([Id]
           ,[Name]
           ,[Uri]
           ,[SubjectNavigationPropertyId]
           ,[ObjectNavigationPropertyId]
           ,[PredicateId]
           ,[SubjectMultiplicity]
           ,[ObjectMultiplicity]
           ,[ViewName])
		VALUES(@Id
           ,@Name
           ,@Uri
           ,@SubjectNavigationPropertyId
           ,@ObjectNavigationPropertyId
           ,@PredicateId
           ,@SubjectMultiplicity
           ,@ObjectMultiplicity
           ,@ViewName)
	END
	ELSE
	BEGIN
		UPDATE [Core].[Association]
		   SET [Name] = @Name
			  ,[Uri] = @Uri
			  ,[SubjectNavigationPropertyId] = @SubjectNavigationPropertyId
			  ,[ObjectNavigationPropertyId] = @ObjectNavigationPropertyId
			  ,[PredicateId] = @PredicateId
			  ,[SubjectMultiplicity] = @SubjectMultiplicity
			  ,[ObjectMultiplicity] = @ObjectMultiplicity
			  ,[ViewName] = @ViewName
		 WHERE [Id]= @Id  
	END
END


GO
PRINT N'Creating Core.CreateOrUpdateDataModelModule...';


GO
CREATE PROCEDURE [Core].[CreateOrUpdateDataModelModule]
@Id UNIQUEIDENTIFIER, @Namespace NVARCHAR (150), @Uri NVARCHAR (1024), @Description NVARCHAR (MAX)
AS
BEGIN
	IF (SELECT COUNT(1) FROM [Core].[DataModelModule] WHERE Id = @Id) = 0 
	BEGIN
		INSERT INTO [Core].[DataModelModule]
           ([Id]
           ,[Namespace]
           ,[Uri]
           ,[Description])
     VALUES
           (@Id
           ,@Namespace
           ,@Uri
           ,@Description) 
	END
	ELSE
	BEGIN
		UPDATE [Core].[DataModelModule]
		   SET [Namespace] = @Namespace
			  ,[Uri] = @Uri
			  ,[Description] = @Description
		 WHERE [Id] =@Id
	END
END


GO
PRINT N'Creating Core.CreateOrUpdateNavigationProperty...';


GO
CREATE PROCEDURE [Core].[CreateOrUpdateNavigationProperty]
@Id UNIQUEIDENTIFIER, @ResourceTypeId UNIQUEIDENTIFIER, @Name NVARCHAR (100), @Uri NVARCHAR (1024), @Description NVARCHAR (4000), @TableName NVARCHAR (128), @ColumnName NVARCHAR (100)
AS
BEGIN

	IF (SELECT COUNT(1) FROM [Core].[NavigationProperty] WHERE Id = @Id )=0
	BEGIN
		INSERT INTO [Core].[NavigationProperty]
           ([Id]
           ,[ResourceTypeId] 
           ,[Name]
           ,[Uri]
           ,[Description]
           ,[TableName]
           ,[ColumnName])
        VALUES
			(@Id
			,@ResourceTypeId
            ,@Name
            ,@Uri
            ,@Description
            ,@TableName
            ,@ColumnName);
	END
	ELSE
	BEGIN
		UPDATE [Core].[NavigationProperty]
		   SET [ResourceTypeId]  = @ResourceTypeId
			  ,[Name] = @Name 
			  ,[Uri] = @Uri
			  ,[Description] = @Description
			  ,[TableName] = @TableName
			  ,[ColumnName] = @ColumnName
		 WHERE [Id] =@Id 
	END
END


GO
PRINT N'Creating Core.CreateOrUpdateResourceType...';


GO
CREATE PROCEDURE [Core].[CreateOrUpdateResourceType]
@Id UNIQUEIDENTIFIER, @DataModelModuleId UNIQUEIDENTIFIER, @BaseTypeId UNIQUEIDENTIFIER, @Name NVARCHAR (100), @Uri NVARCHAR (1024), @Description NVARCHAR (4000), @Discriminator INT
AS
BEGIN
	IF (SELECT COUNT(1) FROM [Core].[ResourceType] WHERE Id = @Id) = 0
	BEGIN

		INSERT INTO [Core].[ResourceType]
           ([Id]
           ,[DataModelModuleId]
           ,[BaseTypeId]
           ,[Name]
           ,[Uri]
           ,[Description]
           ,[Discriminator])
		VALUES
           (@Id
           ,@DataModelModuleId
           ,@BaseTypeId
           ,@Name
           ,@Uri
           ,@Description
           ,@Discriminator);
	END
	ELSE
	BEGIN
		UPDATE [Core].[ResourceType]
		   SET [DataModelModuleId] = @DataModelModuleId
			  ,[BaseTypeId] = @BaseTypeId
			  ,[Name] = @Name 
			  ,[Uri] = @Uri
			  ,[Description] = @Description
			  ,[Discriminator] = @Discriminator
		 WHERE [Id] = @Id
	END
	
	-- Update the max discriminator value.
	DECLARE @MaxDiscriminator [int];
	DECLARE @ConfigurationExists [bit];
	SELECT @ConfigurationExists = 1, @MaxDiscriminator = [ConfigValue]
	FROM [Core].[Configuration]
	WHERE [ConfigName] = 'MaxDiscriminator';

	IF @ConfigurationExists IS NULL
	BEGIN
		INSERT INTO [Core].[Configuration]
		SELECT 'MaxDiscriminator', 
		CASE WHEN MAX([Discriminator]) > @Discriminator THEN MAX([Discriminator])
		ELSE @Discriminator END
		FROM [Core].[ResourceType];
	END
	ELSE
	BEGIN
		UPDATE [Core].[Configuration]
		SET [ConfigValue] = 
		CASE WHEN CAST([ConfigValue] AS [int]) > @Discriminator THEN [ConfigValue]
		ELSE @Discriminator END
		WHERE [ConfigName] = 'MaxDiscriminator';
	END
END


GO
PRINT N'Creating Core.CreateOrUpdateResourceTypeProperty...';


GO
CREATE PROCEDURE [Core].[CreateOrUpdateResourceTypeProperty]
@Id UNIQUEIDENTIFIER, @ResourceTypeId UNIQUEIDENTIFIER, @Name NVARCHAR (100), @Uri NVARCHAR (1024), @Description NVARCHAR (4000)
AS
BEGIN
	IF (SELECT COUNT(1) FROM [Core].[ResourceTypeProperty] WHERE [Id] = @Id )=0
	BEGIN
		INSERT INTO [Core].[ResourceTypeProperty]
           ([Id]
           ,[ResourceTypeId] 
           ,[Name]
           ,[Uri]
           ,[Description])
     VALUES
           (@Id
           ,@ResourceTypeId
           ,@Name
           ,@Uri
           ,@Description) 
	END
	ELSE
	BEGIN
		UPDATE [Core].[ResourceTypeProperty]
		   SET [Name] = @Name 
			  ,[Uri] = @Uri
			  ,[Description] = @Description
			  ,[ResourceTypeId]  = @ResourceTypeId
		 WHERE [Id] =@Id 
	END
END


GO
PRINT N'Creating Core.CreateOrUpdateScalarProperty...';


GO
CREATE PROCEDURE [Core].[CreateOrUpdateScalarProperty]
@Id UNIQUEIDENTIFIER, @ResourceTypeId UNIQUEIDENTIFIER, @Name NVARCHAR (100), @Uri NVARCHAR (1024), @Description NVARCHAR (4000), @DataType NVARCHAR (100), @Nullable BIT, @MaxLength INT, @Scale INT, @Precision INT, @TableName NVARCHAR (128), @ColumnName NVARCHAR (100)
AS
BEGIN
													   
	IF (SELECT COUNT(1) FROM [Core].[ScalarProperty] WHERE Id = @Id )=0
	BEGIN
		INSERT INTO [Core].[ScalarProperty]
           ([Id]
           ,[ResourceTypeId] 
           ,[Name]
           ,[Uri]
           ,[Description]
           ,[DataType]
           ,[Nullable]
           ,[MaxLength]
           ,[Scale]
           ,[Precision]
           ,[TableName]
           ,[ColumnName])
     VALUES
           (@Id
           ,@ResourceTypeId
           ,@Name
           ,@Uri
           ,@Description
           ,@DataType
           ,@Nullable
           ,@MaxLength
           ,@Scale
           ,@Precision
           ,@TableName
           ,@ColumnName);
	END
	ELSE
	BEGIN
		UPDATE [Core].[ScalarProperty]
		   SET [ResourceTypeId]=@ResourceTypeId 
			   ,[Name]=@Name
			   ,[Uri]=@Uri
			   ,[Description]=@Description
			   ,[DataType]=@DataType
			   ,[Nullable]=@Nullable
			   ,[MaxLength]=@MaxLength
			   ,[Scale]=@Scale
			   ,[Precision]=@Precision
			   ,[TableName] = @TableName
			   ,[ColumnName] = @ColumnName
		 WHERE [Id] =@Id 
	END
END


GO
PRINT N'Creating Core.CreatePredicate...';


GO
CREATE PROCEDURE [Core].[CreatePredicate]
@Id UNIQUEIDENTIFIER, @Name NVARCHAR (128), @Uri NVARCHAR (1024)
AS
BEGIN
	INSERT INTO [Core].[Predicate]
	   ([Id]
	   ,[Name]
	   ,[Uri])
	VALUES(@Id
	   ,@Name
	   ,@Uri)
END


GO
PRINT N'Creating Core.CreateUniqueIndexesOnMetadata...';


GO
CREATE PROCEDURE [Core].[CreateUniqueIndexesOnMetadata]

AS
BEGIN
	CREATE UNIQUE NONCLUSTERED INDEX [UQ_DataModelModule] ON [Core].[DataModelModule] 
	(
		[Namespace] ASC
	)

	CREATE UNIQUE NONCLUSTERED INDEX [UQ_ResourceType] ON [Core].[ResourceType] 
	(
		[DataModelModuleId] ASC,
		[Name] ASC
	)

	CREATE UNIQUE NONCLUSTERED INDEX [UQ_Discriminator] ON [Core].[ResourceType] 
	(
		[Discriminator] ASC
	)

	CREATE UNIQUE NONCLUSTERED INDEX [UQ_Association_Name] ON [Core].[Association] 
	(
		[Name] ASC
	)

	CREATE UNIQUE NONCLUSTERED INDEX [UQ_Association_PredicateId] ON [Core].[Association] 
	(
		[PredicateId] ASC
	)

	CREATE UNIQUE NONCLUSTERED INDEX [UQ_ScalarProperty] ON [Core].[ScalarProperty] 
	(
		[ResourceTypeId] ASC,
		[Name] ASC
	)

	CREATE UNIQUE NONCLUSTERED INDEX [UQ_NavigationProperty] ON [Core].[NavigationProperty] 
	(
		[ResourceTypeId] ASC,
		[Name] ASC
	)
	
	EXEC(N'
	CREATE TRIGGER [Core].[AfterInsertUpdateOnScalarProperty]
	ON [Core].[ScalarProperty]
	AFTER INSERT, UPDATE 
	AS 
	BEGIN
		EXEC [Core].[ValidateInsertsIntoResourceTypeProperty]
	END
	')
	
	EXEC(N'
	CREATE TRIGGER [Core].[AfterInsertUpdateOnNavigationProperty]
	ON [Core].[NavigationProperty]
	AFTER INSERT, UPDATE 
	AS 
	BEGIN
		EXEC [Core].[ValidateInsertsIntoResourceTypeProperty]
	END	
	')
	
	CREATE UNIQUE NONCLUSTERED INDEX [UQ_PredicateName] ON [Core].[Predicate] 
	(
		[Name] ASC
	)
	
END


GO
PRINT N'Creating Core.DeleteFile...';


GO
CREATE PROCEDURE [Core].[DeleteFile]
@Id UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM [Core].[Resource] 
	WHERE [Id] = @Id;
END


GO
PRINT N'Creating Core.DeleteRelationship...';


GO
CREATE PROCEDURE [Core].[DeleteRelationship]
@Id UNIQUEIDENTIFIER, @SubjectResourceId UNIQUEIDENTIFIER, @ObjectResourceId UNIQUEIDENTIFIER, @PredicateId UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;
	-- Even though we take as an input the subject, object and predicate
	-- Ids, we do not use them. The input parameters are only to make EF
	-- happy.
	DELETE FROM [Core].[Relationship] 
	WHERE [Id] = @Id;
	
	-- We do not reject the deletions of OneToXXX or XXXToOne relationships
	-- here. This logic is moved to ZentityContext_SavingChanges. We allow
	-- the deletions if the relationship deletion and the entity deletion
	-- are done in the same batch.
END


GO
PRINT N'Creating Core.DeleteResource...';


GO
CREATE PROCEDURE [Core].[DeleteResource]
@Id UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM [Core].[Resource] 
	WHERE [Id] = @Id;
END


GO
PRINT N'Creating Core.DeleteResourceHasFile...';


GO
CREATE PROCEDURE [Core].[DeleteResourceHasFile]
@SubjectResourceId UNIQUEIDENTIFIER, @ObjectResourceId UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = '818a93f5-25a9-4149-a8d2-19104a352da0';
END


GO
PRINT N'Creating Core.GetConfigurationValue...';


GO
CREATE PROCEDURE [Core].[GetConfigurationValue]
@ConfigName NVARCHAR (256), @ConfigValue NVARCHAR (4000) OUTPUT
AS
BEGIN
	SELECT @ConfigValue = ConfigValue 
	FROM [Core].[Configuration]
	WHERE ConfigName = @ConfigName;
END


GO
PRINT N'Creating Core.GetDataModelModules...';


GO
CREATE PROCEDURE [Core].[GetDataModelModules]
@DataModelModules NVARCHAR (MAX) OUTPUT
AS
BEGIN
	DECLARE @IsFullTextEnable [bit];
	DECLARE @MaxDiscriminator [int];
	DECLARE @ConfigMaxDiscriminator [int];
	DECLARE @ResourceTypeMaxDiscriminator [int];
	DECLARE @ZentityVersion [nvarchar](256);

	SET @MaxDiscriminator = 0;
	SELECT @ConfigMaxDiscriminator = CAST([ConfigValue] AS [int])
	FROM [Core].[Configuration]
	WHERE [ConfigName] = 'MaxDiscriminator';

	IF (@ConfigMaxDiscriminator IS NOT NULL) 
		AND (@ConfigMaxDiscriminator > @MaxDiscriminator)
	SELECT @MaxDiscriminator = @ConfigMaxDiscriminator;

	SELECT @ResourceTypeMaxDiscriminator = MAX([Discriminator])
	FROM [Core].[ResourceType];

	IF(@ResourceTypeMaxDiscriminator IS NOT NULL) 
		AND (@ResourceTypeMaxDiscriminator > @MaxDiscriminator)
	SELECT @MaxDiscriminator = @ResourceTypeMaxDiscriminator;

	SELECT @ZentityVersion = [ConfigValue]
	FROM [Core].[Configuration]
	WHERE [ConfigName] = 'ZentityVersion';

	SELECT TOP(1) @IsFullTextEnable = CASE WHEN [ConfigValue] = 'True' THEN 1 ELSE 0 END
	FROM [Core].[Configuration]
	WHERE [ConfigName] ='IsFullTextSearchEnabled';

	DECLARE @Temp TABLE
	(
		[Tag] [int],
		[Parent] [int],

		[DataModel!1!!element] [nvarchar](256),
		[DataModel!1!ZentityVersion] [nvarchar](256),
		[DataModel!1!MaxDiscriminator] [int],

		[DataModelModule!2!Id] [uniqueidentifier],
		[DataModelModule!2!Namespace] [nvarchar](150),
		[DataModelModule!2!Uri] [nvarchar](1024),
		[DataModelModule!2!Description] [nvarchar](max),
		[DataModelModule!2!IsMsShipped] [bit],

		[ResourceType!3!Id] [uniqueidentifier],
		[ResourceType!3!BaseTypeId] [uniqueidentifier],
		[ResourceType!3!Name] [nvarchar](100),
		[ResourceType!3!Uri] [nvarchar](1024),
		[ResourceType!3!Description] [nvarchar](4000),
		[ResourceType!3!Discriminator] [int],

		[ScalarProperty!4!Id] [uniqueidentifier],
		[ScalarProperty!4!Name] [nvarchar](100),
		[ScalarProperty!4!Uri] [nvarchar](1024),
		[ScalarProperty!4!Description] [nvarchar](max),
		[ScalarProperty!4!DataType] [nvarchar](100),
		[ScalarProperty!4!Nullable] [bit],
		[ScalarProperty!4!MaxLength] [int],
		[ScalarProperty!4!Scale] [int],
		[ScalarProperty!4!Precision] [int],
		[ScalarProperty!4!TableName] [nvarchar](128),
		[ScalarProperty!4!ColumnName] [nvarchar](100),
		[ScalarProperty!4!IsFullTextIndexed] [bit],

		[NavigationProperty!5!Id] [uniqueidentifier],
		[NavigationProperty!5!Name] [nvarchar](100),
		[NavigationProperty!5!Uri] [nvarchar](1024),
		[NavigationProperty!5!Description] [nvarchar](max),
		[NavigationProperty!5!TableName] [nvarchar](128),
		[NavigationProperty!5!ColumnName] [nvarchar](100),

		[Association!6!Id] [uniqueidentifier],
		[Association!6!Name] [nvarchar](100),
		[Association!6!Uri] [nvarchar](1024),
		[Association!6!SubjectNavigationPropertyId] [uniqueidentifier],
		[Association!6!ObjectNavigationPropertyId] [uniqueidentifier],
		[Association!6!PredicateId] [uniqueidentifier],
		[Association!6!SubjectMultiplicity] [nvarchar](32),
		[Association!6!ObjectMultiplicity] [nvarchar](32),
		[Association!6!ViewName] [nvarchar](150)
	)
	
	INSERT INTO @Temp VALUES
	(
		1,
		NULL,
		
		NULL,
		@ZentityVersion,
		@MaxDiscriminator,
		
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL
	)
	
	DECLARE @DataModelModuleId [uniqueidentifier],
		@DataModelModuleNamespace [nvarchar](150),
		@DataModelModuleUri [nvarchar](1024),
		@DataModelModuleDescription [nvarchar](max),
		@DataModelModuleIsMsShipped [bit];

	DECLARE DataModelModuleCursor CURSOR
	FOR SELECT [Id], [Namespace], [Uri], [Description], [IsMsShipped]
		FROM [Core].[DataModelModule]
	
	OPEN DataModelModuleCursor;
	FETCH NEXT FROM DataModelModuleCursor INTO 
	@DataModelModuleId, @DataModelModuleNamespace, @DataModelModuleUri,
	@DataModelModuleDescription, @DataModelModuleIsMsShipped;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		INSERT INTO @Temp VALUES
		(
			2,
			1,
		
			NULL,
			NULL,
			NULL,
		
			@DataModelModuleId,
			@DataModelModuleNamespace,
			@DataModelModuleUri,
			@DataModelModuleDescription,
			@DataModelModuleIsMsShipped,
		
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
		
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
		
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
		
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL
		);

		INSERT INTO @Temp
		SELECT 3,
			2,
		
			NULL,
			NULL,
			NULL,
		
			@DataModelModuleId,
			NULL,
			NULL,
			NULL,
			NULL,
		
			[R].[Id],
			[R].[BaseTypeId],
			[R].[Name],
			[R].[Uri],
			[R].[Description],
			[R].[Discriminator],
			
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
		
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL		
		FROM [Core].[ResourceType] [R]
		WHERE [R].[DataModelModuleId] = @DataModelModuleId;
		
		DECLARE @ResourceTypeId [uniqueidentifier]
		
		DECLARE ResourceTypeCursor CURSOR
		FOR SELECT [Id]
			FROM [Core].[ResourceType] [R]
			WHERE [R].[DataModelModuleId] = @DataModelModuleId;
	
		OPEN ResourceTypeCursor;
		
		FETCH NEXT FROM ResourceTypeCursor INTO 
			@ResourceTypeId;
		WHILE @@FETCH_STATUS = 0
		BEGIN
			
			INSERT INTO @Temp
			SELECT 4,
				3,
				
				NULL,
				NULL,
				NULL,
				
				@DataModelModuleId,
				NULL,
				NULL,
				NULL,
				NULL,
				
				@ResourceTypeId,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
			
				[S].[Id],
				[S].[Name],
				[S].[Uri],
				[S].[Description],
				[S].[DataType],
				[S].[Nullable],
				[S].[MaxLength],
				[S].[Scale],
				[S].[Precision],
				[S].[TableName],
				[S].[ColumnName],
				CASE WHEN @IsFullTextEnable = 1 AND [FTSDetails].[IsFullTextIndexed] = 1 THEN 1 
				ELSE 0 END [IsFullTextIndexed],
				
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
			
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL				
			FROM [Core].[ScalarProperty] [S]
			LEFT OUTER JOIN
			(
				SELECT [T].[name] [TableName], [C].[name] [ColumnName], 1 [IsFullTextIndexed]
				FROM [sys].[fulltext_index_columns] [FTS]
				INNER JOIN [sys].[columns] [C]
				ON [FTS].[object_id] = [C].[object_id]
				AND [FTS].[column_id] = [C].[column_id]
				INNER JOIN [sys].[tables] [T]
				ON [C].[object_id] = [T].[object_id]
				INNER JOIN [sys].[schemas] [S]
				ON [T].[schema_id] = [S].[schema_id]
				AND [S].[name] = 'Core'
			) [FTSDetails]
			ON [S].[TableName] = [FTSDetails].[TableName]
			AND [S].[ColumnName] = [FTSDetails].[ColumnName]
			WHERE [S].ResourceTypeId = @ResourceTypeId
			
			
			INSERT INTO @Temp
			SELECT 5,
				3,
				
				NULL,
				NULL,
				NULL,
				
				@DataModelModuleId,
				NULL,
				NULL,
				NULL,
				NULL,
				
				@ResourceTypeId,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
			
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
				
				[N].[Id],
				[N].[Name],
				[N].[Uri],
				[N].[Description],
				[N].[TableName],
				[N].[ColumnName],
			
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL,
				NULL				
			FROM [Core].[NavigationProperty] [N]
			WHERE @ResourceTypeId = [N].[ResourceTypeId]
			
			FETCH NEXT FROM ResourceTypeCursor INTO 
			@ResourceTypeId;
		END
		CLOSE ResourceTypeCursor;
		DEALLOCATE ResourceTypeCursor;
	
		FETCH NEXT FROM DataModelModuleCursor INTO 
		@DataModelModuleId, @DataModelModuleNamespace, @DataModelModuleUri,
		@DataModelModuleDescription, @DataModelModuleIsMsShipped;
	END
	CLOSE DataModelModuleCursor;
	DEALLOCATE DataModelModuleCursor;

	INSERT INTO @Temp
	SELECT 6,
		1,

		NULL,
		NULL,
		NULL,

		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL,

		[A].[Id],
		[A].[Name],
		[A].[Uri],
		[A].[SubjectNavigationPropertyId],
		[A].[ObjectNavigationPropertyId],
		[A].[PredicateId],
		[A].[SubjectMultiplicity],
		[A].[ObjectMultiplicity],
		[A].[ViewName]
	FROM [Core].[Association] [A]

	SET @DataModelModules = (SELECT * FROM @Temp 
	ORDER BY [DataModelModule!2!Id],[ResourceType!3!Id],[ScalarProperty!4!Id],
	[NavigationProperty!5!Id],[Association!6!Id]
	FOR XML EXPLICIT);
END


GO
PRINT N'Creating Core.GetServerProperty...';


GO
CREATE PROCEDURE [Core].[GetServerProperty]
@PropertyName NVARCHAR (4000), @PropertyValue NVARCHAR (4000) OUTPUT
AS
BEGIN
	SELECT @PropertyValue = CAST( SERVERPROPERTY(@PropertyName) AS nvarchar(4000));
END


GO
PRINT N'Creating Core.GetTableModelMap...';


GO
CREATE PROCEDURE [Core].[GetTableModelMap]
@TableModelMap NVARCHAR (MAX) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @Temp TABLE
	(
		[Tag] [int],
		[Parent] [int],

		[Tables!1!!element] [nvarchar](256),

		[Table!2!Name] [nvarchar](128),

		[Column!3!Name] [nvarchar](100),
		[Column!3!IsMapped] [bit],
		[Column!3!IsScalarProperty] [bit],
		[Column!3!PropertyId] [uniqueidentifier]
	);

	-- Prepare the root <Tables> element.
	INSERT INTO @Temp([Tag]) VALUES(1);

	-- Prepare <Table> elements.
	INSERT INTO @Temp([Tag], [Parent], [Tables!1!!element], [Table!2!Name])
	SELECT DISTINCT 2, 1, NULL, [TableName]
	FROM
	(
		SELECT [TableName] FROM [Core].[ScalarProperty] WHERE [TableName] IS NOT NULL
		UNION
		SELECT [TableName] FROM [Core].[NavigationProperty] WHERE [TableName] IS NOT NULL
	)[T];

	-- Prepare <Column> elements from each column in the tables that are being used to
	-- store data model information.
	INSERT INTO @Temp([Tag], [Parent], [Table!2!Name], [Column!3!Name])
	SELECT DISTINCT 3, 2, [T].[name], [C].[name]
	FROM [sys].[schemas] [S]
	INNER JOIN [sys].[tables] [T]
	ON [S].[schema_id] = [T].[schema_id]
	AND [S].[name]='Core'
	AND [T].[name] IN
	(
		SELECT [Table!2!Name] FROM @Temp WHERE [Table!2!Name] IS NOT NULL
	)
	INNER JOIN [sys].[columns] [C]
	ON [T].[object_id] = [C].[object_id]

	-- Update <Column> elements with property information.
	UPDATE @Temp
	SET [Column!3!IsMapped] = CASE WHEN [Property].[IsMapped] = 1 THEN 1 ELSE 0 END,
	[Column!3!IsScalarProperty] = [Property].[IsScalarProperty], 
	[Column!3!PropertyId] = [Property].[PropertyId]
	FROM @Temp [T]
	LEFT OUTER JOIN
	(
		SELECT [TableName], [ColumnName], 1 AS [IsMapped], 
			1 AS [IsScalarProperty], [SP].[Id] [PropertyId]
		FROM [Core].[ScalarProperty] [SP] 
		INNER JOIN [Core].[ResourceType] [RT]
		ON [SP].[ResourceTypeId] = [RT].[Id]
		AND [SP].[TableName] IS NOT NULL
		INNER JOIN [Core].[DataModelModule] [DMM]
		ON [RT].[DataModelModuleId] = [DMM].[Id]
		UNION ALL
		SELECT [TableName], [ColumnName], 1 AS [IsMapped], 
			0 AS [IsScalarProperty], [NP].[Id] [PropertyId]
		FROM [Core].[NavigationProperty] [NP] 
		INNER JOIN [Core].[ResourceType] [RT]
		ON [NP].[ResourceTypeId] = [RT].[Id]
		AND [NP].[TableName] IS NOT NULL
		INNER JOIN [Core].[DataModelModule] [DMM]
		ON [RT].[DataModelModuleId] = [DMM].[Id]
	) [Property]
	ON [T].[Table!2!Name] = [Property].[TableName]
	AND [T].[Column!3!Name] = [Property].[ColumnName]

	SET @TableModelMap = (SELECT [Tag], [Parent], [Tables!1!!element], [Table!2!Name],
		[Column!3!Name], [Column!3!IsMapped], [Column!3!IsScalarProperty],
		[Column!3!PropertyId]
		FROM @Temp 
		ORDER BY [Table!2!Name], [Column!3!Name] 
		FOR XML EXPLICIT);
END


GO
PRINT N'Creating Core.InsertFile...';


GO
CREATE PROCEDURE [Core].[InsertFile]
@Id UNIQUEIDENTIFIER, @DateAdded DATETIME, @DateModified DATETIME, @Description NVARCHAR (MAX), @Title NVARCHAR (425), @Uri NVARCHAR (1024), @Checksum NVARCHAR (256), @FileExtension NVARCHAR (128), @MimeType NVARCHAR (128), @Size BIGINT
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO [Core].[Resource]
	( 
		[Id], 
		[ResourceTypeId],
		[Discriminator],
		[DateAdded], 
		[DateModified], 
		[Description], 
		[Title], 
		[Uri],
		[Checksum],
		[FileExtension],
		[MimeType],
		[Size]
	)
	VALUES
	( 
		@Id, 
		'94c567f8-b3eb-4ccb-9bf1-fc88970f78d7',
		2,
		@DateAdded, 
		@DateModified, 
		@Description, 
		@Title, 
		@Uri,
		@Checksum,
		@FileExtension,
		@MimeType,
		@Size
	);
END


GO
PRINT N'Creating Core.InsertResource...';


GO
CREATE PROCEDURE [Core].[InsertResource]
@Id UNIQUEIDENTIFIER, @DateAdded DATETIME, @DateModified DATETIME, @Description NVARCHAR (MAX), @Title NVARCHAR (425), @Uri NVARCHAR (1024)
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO [Core].[Resource]
	( 
		[Id], 
		[ResourceTypeId],
		[Discriminator],
		[DateAdded], 
		[DateModified], 
		[Description], 
		[Title], 
		[Uri]
	)
	VALUES
	( 
		@Id, 
		'd2bd64df-6609-4ea4-ae99-9669da69bf7a', 
		1,
		@DateAdded, 
		@DateModified, 
		@Description, 
		@Title, 
		@Uri
	);
END


GO
PRINT N'Creating Core.InsertResourceHasFile...';


GO
CREATE PROCEDURE [Core].[InsertResourceHasFile]
@SubjectResourceId UNIQUEIDENTIFIER, @ObjectResourceId UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO [Core].[Relationship]
	(	
		[Id],
		[SubjectResourceId],
		[ObjectResourceId],
		[PredicateId]
	)
	VALUES
	(
		NEWID(),
		@SubjectResourceId, 
		@ObjectResourceId, 
		'818a93f5-25a9-4149-a8d2-19104a352da0'
	);
END


GO
PRINT N'Creating Core.SpExecuteSql...';


GO
CREATE PROCEDURE [Core].[SpExecuteSql]
@Cmd NVARCHAR (MAX)
AS
BEGIN
	
	EXEC sp_executesql @Cmd;

END


GO
PRINT N'Creating Core.UpdateFile...';


GO
CREATE PROCEDURE [Core].[UpdateFile]
@Id UNIQUEIDENTIFIER, @DateAdded DATETIME, @DateModified DATETIME, @Description NVARCHAR (MAX), @Title NVARCHAR (425), @Uri NVARCHAR (1024), @Checksum NVARCHAR (256), @FileExtension NVARCHAR (128), @MimeType NVARCHAR (128), @Size BIGINT
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE [Core].[Resource] 
	SET	[DateAdded] = @DateAdded, 
		[DateModified] = @DateModified, 
		[Description] = @Description, 
		[Title] = @Title, 
		[Uri] = @Uri,
		[Checksum] = @Checksum, 
		[FileExtension] = @FileExtension, 
		[MimeType] = @MimeType, 
		[Size] = @Size
	WHERE [Id] = @Id;
END


GO
PRINT N'Creating Core.UpdateResource...';


GO
CREATE PROCEDURE [Core].[UpdateResource]
@Id UNIQUEIDENTIFIER, @DateAdded DATETIME, @DateModified DATETIME, @Description NVARCHAR (MAX), @Title NVARCHAR (425), @Uri NVARCHAR (1024)
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE [Core].[Resource] 
	SET [DateAdded] = @DateAdded, 
		[DateModified] = @DateModified, 
		[Description] = @Description, 
		[Title] = @Title, 
		[Uri] = @Uri
	WHERE [Id] = @Id;
END


GO
PRINT N'Creating Core.ValidateOneToXxxDeletion...';


GO
CREATE PROCEDURE [Core].[ValidateOneToXxxDeletion]
@CsvDeletedRelationshipIds NVARCHAR (MAX), @CsvDeletedResourceIds NVARCHAR (MAX)
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @DeletedRelationshipId TABLE
	(
		[Id] uniqueidentifier
	);

	DECLARE @DeletedResourceId TABLE
	(
		[Id] uniqueidentifier
	);

	DECLARE @LagIndex [int], @Length [int], @LeadIndex [int], @Loop [bit];
	
	IF(@CsvDeletedRelationshipIds IS NOT NULL AND LEN(@CsvDeletedRelationshipIds) > 0)
	BEGIN

		SELECT @LagIndex = 1, @LeadIndex = 1, @Length = LEN(@CsvDeletedRelationshipIds), 
			@Loop = 1;
		WHILE(@Loop = 1)
		BEGIN
			SELECT @LeadIndex = CHARINDEX(',', @CsvDeletedRelationshipIds, @LagIndex);
			IF(@LeadIndex <= 0)
				SELECT @LeadIndex = @Length + 1, @Loop = 0;
			INSERT INTO @DeletedRelationshipId 
			SELECT CAST(SUBSTRING(@CsvDeletedRelationshipIds, @LagIndex, 
				(@LeadIndex - @LagIndex)) AS [uniqueidentifier]);
			SELECT @LagIndex = @LeadIndex + 1;
		END
	END

	IF(@CsvDeletedRelationshipIds IS NOT NULL AND LEN(@CsvDeletedResourceIds) > 0)
	BEGIN
		SELECT @LagIndex = 1, @LeadIndex = 1, @Length = LEN(@CsvDeletedResourceIds), 
		@Loop = 1;
		WHILE(@Loop = 1)
		BEGIN
			SELECT @LeadIndex = CHARINDEX(',', @CsvDeletedResourceIds, @LagIndex);
			IF(@LeadIndex <= 0)
				SELECT @LeadIndex = @Length + 1, @Loop = 0;
			INSERT INTO @DeletedResourceId 
			SELECT CAST(SUBSTRING(@CsvDeletedResourceIds, @LagIndex, 
				(@LeadIndex - @LagIndex)) AS [uniqueidentifier]);
			SELECT @LagIndex = @LeadIndex + 1;
		END
	END
	
	-- Filter the relationships.
	DECLARE @RelationshipToProcess TABLE
	(
		[RelationshipId] [uniqueidentifier],
		[SubjectResourceId] [uniqueidentifier],
		[ObjectResourceId] [uniqueidentifier],
		[Predicate] [uniqueidentifier],
		[SubjectMultiplicity] [nvarchar](128),
		[ObjectMultiplicity] [nvarchar](128)
	);

	INSERT INTO @RelationshipToProcess
	SELECT [R].[Id], [R].[SubjectResourceId], [R].[ObjectResourceId],
	[R].[PredicateId], [A].[SubjectMultiplicity], [A].[ObjectMultiplicity]
	FROM @DeletedRelationshipId [D]
	INNER JOIN [Core].[Relationship] [R]
	ON [D].[Id] = [R].[Id]
	INNER JOIN [Core].[Predicate] [P]
	ON [R].[PredicateId] = [P].[Id]
	-- The inner join here picks only the relevent rows.
	INNER JOIN [Core].[Association] [A]
	ON [P].[Id] = [A].[PredicateId]
	AND ([A].[SubjectMultiplicity] = 'One' OR [A].[ObjectMultiplicity] = 'One');

	DECLARE @RelationshipId [uniqueidentifier], @SubjectResourceId [uniqueidentifier],
	@ObjectResourceId [uniqueidentifier], @SubjectMultiplicity [nvarchar](128),
	@ObjectMultiplicity [nvarchar](128);

	SELECT @RelationshipId = [RelationshipId], @ObjectResourceId = [ObjectResourceId],
	@ObjectMultiplicity = [ObjectMultiplicity]
	FROM @RelationshipToProcess [R]
	WHERE [R].[SubjectMultiplicity] = 'One'
	AND [R].[ObjectResourceId] NOT IN (SELECT [Id] FROM @DeletedResourceId);

	IF(@RelationshipId IS NOT NULL)
	BEGIN
		DECLARE @Msg [nvarchar](4000);
		SET @Msg = 'The Relationship with Id = [' + CAST(@RelationshipId as [nvarchar](128)) + 
		'] cannot be deleted. It represents a One-to-' + @ObjectMultiplicity + 
		' association. For such associations, it is required to delete the Relationship and Resource on ''' + 
		@ObjectMultiplicity + ''' side together. Resource with Id = [' + 
		CAST(@ObjectResourceId as [nvarchar](128)) + '] also needs to be deleted in the same batch.';

		RAISERROR (@Msg, 16, 1);
		RETURN -1;
		-- No need to do a rollback transaction here.
	END;

	SELECT @RelationshipId = NULL;
	SELECT @RelationshipId = [RelationshipId], @SubjectResourceId = [SubjectResourceId],
	@SubjectMultiplicity = [SubjectMultiplicity]
	FROM @RelationshipToProcess [R]
	WHERE [R].[ObjectMultiplicity] = 'One'
	AND [R].[SubjectResourceId] NOT IN (SELECT [Id] FROM @DeletedResourceId);

	IF(@RelationshipId IS NOT NULL)
	BEGIN
		SET @Msg = 'The Relationship with Id = [' + CAST(@RelationshipId as [nvarchar](128)) + 
		'] cannot be deleted. It represents a ' + @SubjectMultiplicity + '-to-One' + 
		' association. For such associations, it is required to delete the Relationship and Resource on ''' + 
		@SubjectMultiplicity + ''' side together. Resource with Id = [' + 
		CAST(@SubjectResourceId as [nvarchar](128)) + '] also needs to be deleted in the same batch.';

		RAISERROR (@Msg, 16, 1);
		RETURN -1;
	END;
END


GO
PRINT N'Creating Administration.fn_hexstrtovarbin...';


GO
CREATE FUNCTION [Administration].[fn_hexstrtovarbin]
(@Input NVARCHAR (4000))
RETURNS VARBINARY (4000)
AS
BEGIN
	DECLARE @Result varbinary(4000), @I int, @L int;

	SET @Result = 0x;
	SET @L = LEN(@Input)/2;
	SET @I = 2;

	WHILE @I <= @L
	BEGIN
		SET @Result = @Result +
		CAST(
			CAST(
				CASE LOWER(SUBSTRING(@Input, @I*2-1, 1))
					WHEN '0' THEN 0x00
					WHEN '1' THEN 0x10
					WHEN '2' THEN 0x20
					WHEN '3' THEN 0x30
					WHEN '4' THEN 0x40
					WHEN '5' THEN 0x50
					WHEN '6' THEN 0x60
					WHEN '7' THEN 0x70
					WHEN '8' THEN 0x80
					WHEN '9' THEN 0x90
					WHEN 'a' THEN 0xa0
					WHEN 'b' THEN 0xb0
					WHEN 'c' THEN 0xc0
					WHEN 'd' THEN 0xd0
					WHEN 'e' THEN 0xe0
					WHEN 'f' THEN 0xf0
				END 
			AS tinyint) |
			CAST(
				CASE LOWER(SUBSTRING(@Input, @I*2, 1))
					WHEN '0' THEN 0x00
					WHEN '1' THEN 0x01
					WHEN '2' THEN 0x02
					WHEN '3' THEN 0x03
					WHEN '4' THEN 0x04
					WHEN '5' THEN 0x05
					WHEN '6' THEN 0x06
					WHEN '7' THEN 0x07
					WHEN '8' THEN 0x08
					WHEN '9' THEN 0x09
					WHEN 'a' THEN 0x0a
					WHEN 'b' THEN 0x0b
					WHEN 'c' THEN 0x0c
					WHEN 'd' THEN 0x0d
					WHEN 'e' THEN 0x0e
					WHEN 'f' THEN 0x0f
				END 
			AS tinyint) 
		AS binary(1));
		
		SET @I = @I + 1;
	END

RETURN @Result;
END


GO
PRINT N'Creating Core.EscapeBrackets...';


GO
CREATE FUNCTION [Core].[EscapeBrackets]
(@Input NVARCHAR (1024))
RETURNS NVARCHAR (2048)
AS
BEGIN
	RETURN REPLACE(@Input, ']', ']]')
END


GO
PRINT N'Creating Core.EscapeQuotes...';


GO
CREATE FUNCTION [Core].[EscapeQuotes]
(@Input NVARCHAR (MAX))
RETURNS NVARCHAR (MAX)
AS
BEGIN
	RETURN REPLACE(@Input, '''', '''''')
END


GO
PRINT N'Creating Administration.EnableChangeHistory...';


GO
CREATE PROCEDURE [Administration].[EnableChangeHistory]
@ChangeHistoryFilePath NVARCHAR (512)
AS
BEGIN
	------------------------------------------------------------------------------------------------
	-- Validate input parameters.
	------------------------------------------------------------------------------------------------
	IF(@ChangeHistoryFilePath IS NULL OR @ChangeHistoryFilePath = '')
	BEGIN
		RAISERROR(N'@ChangeHistoryFilePath cannot be null or empty.', 16, 1);
		GOTO FINISH;
	END

	------------------------------------------------------------------------------------------------
	-- Verify server version.
	------------------------------------------------------------------------------------------------
	RAISERROR(N'Verifying server version.', 10, 1);
	DECLARE @Version nvarchar(128), @MajorVersion int, @EditionId int;
	SELECT @Version = CAST( SERVERPROPERTY('ProductVersion') AS nvarchar(4000));
	SELECT @MajorVersion = CAST(LEFT(@Version,CHARINDEX('.',@Version,0)-1) AS int);
	SELECT @EditionId = CAST(SERVERPROPERTY('EditionID') AS int);
	
	IF
	(
		@MajorVersion <> 10 OR 
		(
			@EditionId <> 1804890536  AND -- Enterprise Edition
			@EditionId <> 610778273 AND -- Enterprise Evaluation Edition
			@EditionId <> -2117995310 -- Developer Edition
		)
	)
	BEGIN
		DECLARE @MsgCHNotAvailable nvarchar(4000);
		SET @MsgCHNotAvailable = N'Change history feature is not available on this version of ' + 
		N'SQL Server. The supported versions are SQL Server 2008, Enterprise and Enterprise ' + 
		N'Evaluation editions.'
		RAISERROR(@MsgCHNotAvailable, 16, 1);
		GOTO FINISH;
	END

	BEGIN TRY
		DECLARE @DatabaseName nvarchar(128);
		SELECT @DatabaseName = DB_NAME();
		DECLARE @Cmd nvarchar(max);

		--------------------------------------------------------------------------------------------
		-- Create change history filegroup and files. We do not enclose these in a transaction since
		-- ALTER DATABASE statement is not allowed within multi-statement transaction.
		--------------------------------------------------------------------------------------------
		IF NOT EXISTS(SELECT 1 FROM sys.filegroups WHERE [name] = N'ChangeHistory')
		BEGIN
			RAISERROR(N'Creating change history filegroup.', 10, 1);
			SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
			WHERE [CommandName]  = 
				N'ChangeHistory.FileGroup' ;
			-- Replace token.
			SELECT @Cmd = REPLACE(@Cmd, '#(DatabaseName)', [Core].[EscapeBrackets](@DatabaseName));
			EXEC(@Cmd);
		END
		
		-- NOTE that we always add files to filegroups without removing earlier files. So earlier
		-- data is always preserved.
		RAISERROR(N'Adding file to change history filegroup.', 10, 1);
		SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
		WHERE [CommandName]  = 
			N'ChangeHistory.File' ;
		-- Replace tokens.
		SELECT @Cmd = REPLACE(@Cmd, '#(DatabaseName)', [Core].[EscapeBrackets](@DatabaseName));
		SELECT @Cmd = REPLACE(@Cmd, '#(ChangeHistory)', [Core].[EscapeBrackets]('ChangeHistory' + 
			CAST(NEWID() AS [nvarchar](128))));
		SELECT @Cmd = REPLACE(@Cmd, '#(ChangeHistoryFilePath)', 
			[Core].[EscapeQuotes](@ChangeHistoryFilePath));
		EXEC(@Cmd);

		-- Create rest of the database objects in a transaction.
		BEGIN TRANSACTION CreateChangeHistoryObjects
		
			----------------------------------------------------------------------------------------
			-- Create Administration.PropertyChangeQueryForCaptureInstance.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.tables [T] INNER JOIN sys.schemas [S] 
				ON [T].schema_id = [S].schema_id 
				WHERE [T].[name] = N'PropertyChangeQueryForCaptureInstance' 
				AND [S].[name] = N'Administration')
			BEGIN
				RAISERROR(N'Creating Administration.PropertyChangeQueryForCaptureInstance', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.PropertyChangeQueryForCaptureInstance' ;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.CreatePropertyChangeQueryForCaptureInstance.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'CreatePropertyChangeQueryForCaptureInstance' 
				AND [S].[name] = N'Administration' AND [O].[type] = N'P')
			BEGIN
				RAISERROR(N'Creating Administration.CreatePropertyChangeQueryForCaptureInstance', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.CreatePropertyChangeQueryForCaptureInstance' 
					;
				EXEC(@Cmd);
			END
			
			----------------------------------------------------------------------------------------
			-- Create UpdateCaptureInstances and Register it as an event handler for schema changes.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'UpdateCaptureInstances' 
				AND [S].[name] = N'Administration' AND [O].[type] = N'P')
			BEGIN
				RAISERROR(N'Creating Administration.UpdateCaptureInstances.', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName] = N'Administration.UpdateCaptureInstances';
				EXEC(@Cmd);
			END
				
			IF NOT EXISTS(SELECT 1 FROM [Core].[AfterSchemaChangesHandler] 
				WHERE [HandlerName] = N'Administration.UpdateCaptureInstances')
			BEGIN
				RAISERROR(N'Adding Administration.UpdateCaptureInstances schema change handler.', 10, 1);
				INSERT INTO [Core].[AfterSchemaChangesHandler] ([HandlerName], [Query])
				VALUES('Administration.UpdateCaptureInstances',
					'/* Quick Fix: We were seeing transaction deadlocks if the change history feature is enabled and we update database schema. A wait here before invoking change history procedures fixes this issue.*/
					WAITFOR DELAY ''00:02:00'';
					EXEC [Administration].[UpdateCaptureInstances];');
			END
			----------------------------------------------------------------------------------------
			-- Create capture instances.
			----------------------------------------------------------------------------------------
			SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
			WHERE [CommandName]  = 
				N'CreateCaptureInstances' ;
			EXEC(@Cmd);

			----------------------------------------------------------------------------------------
			-- Create Administration.ChangeSet.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.tables [T] INNER JOIN sys.schemas [S] 
				ON [T].schema_id = [S].schema_id WHERE [T].[name] = N'ChangeSet' 
				AND [S].[name] = N'Administration')
			BEGIN
				RAISERROR(N'Creating Administration.ChangeSet', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ChangeSet' ;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.Operation.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.tables [T] INNER JOIN sys.schemas [S] 
				ON [T].schema_id = [S].schema_id WHERE [T].[name] = N'Operation' 
				AND [S].[name] = N'Administration')
			BEGIN
				RAISERROR(N'Creating Administration.Operation', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.Operation' ;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.PredicateChange.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.tables [T] INNER JOIN sys.schemas [S] 
				ON [T].schema_id = [S].schema_id WHERE [T].[name] = N'PredicateChange' 
				AND [S].[name] = N'Administration')
			BEGIN
				RAISERROR(N'Creating Administration.PredicateChange', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.PredicateChange' ;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.PropertyChange.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.tables [T] INNER JOIN sys.schemas [S] 
				ON [T].schema_id = [S].schema_id WHERE [T].[name] = N'PropertyChange' 
				AND [S].[name] = N'Administration')
			BEGIN
				RAISERROR(N'Creating Administration.PropertyChange', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.PropertyChange' ;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.RelationshipChange.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.tables [T] INNER JOIN sys.schemas [S] 
				ON [T].schema_id = [S].schema_id WHERE [T].[name] = N'RelationshipChange' 
				AND [S].[name] = N'Administration')
			BEGIN
				RAISERROR(N'Creating Administration.RelationshipChange', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.RelationshipChange' ;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.PredicatePropertyChange.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.tables [T] INNER JOIN sys.schemas [S] 
				ON [T].schema_id = [S].schema_id WHERE [T].[name] = N'PredicatePropertyChange' 
				AND [S].[name] = N'Administration')
			BEGIN
				RAISERROR(N'Creating Administration.PredicatePropertyChange', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.PredicatePropertyChange' ;
				EXEC(@Cmd);
			END
			
			----------------------------------------------------------------------------------------
			-- Create Administration.RelationshipPropertyChange.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.tables [T] INNER JOIN sys.schemas [S] 
				ON [T].schema_id = [S].schema_id WHERE [T].[name] = N'RelationshipPropertyChange' 
				AND [S].[name] = N'Administration')
			BEGIN
				RAISERROR(N'Creating Administration.RelationshipPropertyChange', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.RelationshipPropertyChange' ;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.ResourceChange.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.tables [T] INNER JOIN sys.schemas [S] 
				ON [T].schema_id = [S].schema_id WHERE [T].[name] = N'ResourceChange' 
				AND [S].[name] = N'Administration')
			BEGIN
				RAISERROR(N'Creating Administration.ResourceChange', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ResourceChange' ;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.ResourcePropertyChange.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.tables [T] INNER JOIN sys.schemas [S] 
				ON [T].schema_id = [S].schema_id WHERE [T].[name] = N'ResourcePropertyChange' 
				AND [S].[name] = N'Administration')
			BEGIN
				RAISERROR(N'Creating Administration.ResourcePropertyChange', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ResourcePropertyChange' ;
				EXEC(@Cmd);
			END
			
			----------------------------------------------------------------------------------------
			-- Create Administration.DataModelModuleChange.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.tables [T] INNER JOIN sys.schemas [S] 
				ON [T].schema_id = [S].schema_id WHERE [T].[name] = N'DataModelModuleChange' 
				AND [S].[name] = N'Administration')
			BEGIN
				RAISERROR(N'Creating Administration.DataModelModuleChange', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.DataModelModuleChange' ;
				EXEC(@Cmd);
			END
			
			----------------------------------------------------------------------------------------
			-- Create Administration.AssociationChange.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.tables [T] INNER JOIN sys.schemas [S] 
				ON [T].schema_id = [S].schema_id WHERE [T].[name] = N'AssociationChange' 
				AND [S].[name] = N'Administration')
			BEGIN
				RAISERROR(N'Creating Administration.AssociationChange', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.AssociationChange' ;
				EXEC(@Cmd);
			END
			
			----------------------------------------------------------------------------------------
			-- Create Administration.ResourceTypeChange.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.tables [T] INNER JOIN sys.schemas [S] 
				ON [T].schema_id = [S].schema_id WHERE [T].[name] = N'ResourceTypeChange' 
				AND [S].[name] = N'Administration')
			BEGIN
				RAISERROR(N'Creating Administration.ResourceTypeChange', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ResourceTypeChange' ;
				EXEC(@Cmd);
			END

			
			----------------------------------------------------------------------------------------
			-- Create Administration.ScalarPropertyChange.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.tables [T] INNER JOIN sys.schemas [S] 
				ON [T].schema_id = [S].schema_id WHERE [T].[name] = N'ScalarPropertyChange' 
				AND [S].[name] = N'Administration')
			BEGIN
				RAISERROR(N'Creating Administration.ScalarPropertyChange', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ScalarPropertyChange' ;
				EXEC(@Cmd);
			END
			
			----------------------------------------------------------------------------------------
			-- Create Administration.NavigationPropertyChange.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.tables [T] INNER JOIN sys.schemas [S] 
				ON [T].schema_id = [S].schema_id WHERE [T].[name] = N'NavigationPropertyChange' 
				AND [S].[name] = N'Administration')
			BEGIN
				RAISERROR(N'Creating Administration.NavigationPropertyChange', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.NavigationPropertyChange' ;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.GetEscapedBinaryPropertyChangeXml.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'GetEscapedBinaryPropertyChangeXml' 
				AND [S].[name] = N'Administration' 
				AND ([O].[type] = N'FN' OR [O].[type]=N'IF' OR [O].[type]=N'TF'))
			BEGIN
				RAISERROR(N'Creating Administration.GetEscapedBinaryPropertyChangeXml', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName] = N'Administration.GetEscapedBinaryPropertyChangeXml';
				EXEC(@Cmd);
			END
			
			----------------------------------------------------------------------------------------
			-- Create Administration.GetEscapedPropertyChangeXml.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'GetEscapedPropertyChangeXml' 
				AND [S].[name] = N'Administration' 
				AND ([O].[type] = N'FN' OR [O].[type]=N'IF' OR [O].[type]=N'TF'))
			BEGIN
				RAISERROR(N'Creating Administration.GetEscapedPropertyChangeXml', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName] = N'Administration.GetEscapedPropertyChangeXml';
				EXEC(@Cmd);
			END
				
			----------------------------------------------------------------------------------------
			-- Create Administration.GetResourceAtChangeSet.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'GetResourceAtChangeSet' 
				AND [S].[name] = N'Administration' AND [O].[type] = N'P')
			BEGIN
				RAISERROR(N'Creating Administration.GetResourceAtChangeSet', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.GetResourceAtChangeSet' 
					;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.ProcessPredicateChanges.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'ProcessPredicateChanges' 
				AND [S].[name] = N'Administration' AND [O].[type] = N'P')
			BEGIN
				RAISERROR(N'Creating Administration.ProcessPredicateChanges', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ProcessPredicateChanges' 
					;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.ProcessPropertyChanges.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'ProcessPropertyChanges' 
				AND [S].[name] = N'Administration' AND [O].[type] = N'P')
			BEGIN
				RAISERROR(N'Creating Administration.ProcessPropertyChanges', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ProcessPropertyChanges' 
					;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.ProcessRelationshipChanges.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'ProcessRelationshipChanges' 
				AND [S].[name] = N'Administration' AND [O].[type] = N'P')
			BEGIN
				RAISERROR(N'Creating Administration.ProcessRelationshipChanges', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ProcessRelationshipChanges' 
					;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.ProcessPredicatePropertyChanges.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'ProcessPredicatePropertyChanges' 
				AND [S].[name] = N'Administration' AND [O].[type] = N'P')
			BEGIN
				RAISERROR(N'Creating Administration.ProcessPredicatePropertyChanges', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ProcessPredicatePropertyChanges' 
					;
				EXEC(@Cmd);
			END
			
			----------------------------------------------------------------------------------------
			-- Create Administration.ProcessRelationshipPropertyChanges.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'ProcessRelationshipPropertyChanges' 
				AND [S].[name] = N'Administration' AND [O].[type] = N'P')
			BEGIN
				RAISERROR(N'Creating Administration.ProcessRelationshipPropertyChanges', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ProcessRelationshipPropertyChanges' 
					;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.ProcessResourceChanges.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'ProcessResourceChanges' 
				AND [S].[name] = N'Administration' AND [O].[type] = N'P')
			BEGIN
				RAISERROR(N'Creating Administration.ProcessResourceChanges', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ProcessResourceChanges' 
					;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.ProcessResourcePropertyChanges.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'ProcessResourcePropertyChanges' 
				AND [S].[name] = N'Administration' AND [O].[type] = N'P')
			BEGIN
				RAISERROR(N'Creating Administration.ProcessResourcePropertyChanges', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ProcessResourcePropertyChanges' 
					;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.ProcessDataModelModuleChanges.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'ProcessDataModelModuleChanges' 
				AND [S].[name] = N'Administration' AND [O].[type] = N'P')
			BEGIN
				RAISERROR(N'Creating Administration.ProcessDataModelModuleChanges.', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ProcessDataModelModuleChanges' 
					;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.ProcessResourceTypeChanges.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'ProcessResourceTypeChanges' 
				AND [S].[name] = N'Administration' AND [O].[type] = N'P')
			BEGIN
				RAISERROR(N'Creating Administration.ProcessResourceTypeChanges', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ProcessResourceTypeChanges' 
					;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.ProcessAssociationChanges.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'ProcessAssociationChanges' 
				AND [S].[name] = N'Administration' AND [O].[type] = N'P')
			BEGIN
				RAISERROR(N'Creating Administration.ProcessAssociationChanges', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ProcessAssociationChanges';
				EXEC(@Cmd);
			END
			
			
			----------------------------------------------------------------------------------------
			-- Create Administration.ProcessScalarPropertyChanges.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'ProcessScalarPropertyChanges' 
				AND [S].[name] = N'Administration' AND [O].[type] = N'P')
			BEGIN
				RAISERROR(N'Creating Administration.ProcessScalarPropertyChanges', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ProcessScalarPropertyChanges' 
					;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Create Administration.ProcessNavigationPropertyChanges.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'ProcessNavigationPropertyChanges' 
				AND [S].[name] = N'Administration' AND [O].[type] = N'P')
			BEGIN
				RAISERROR(N'Creating Administration.ProcessNavigationPropertyChanges', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ProcessNavigationPropertyChanges' 
					;
				EXEC(@Cmd);
			END
			
			----------------------------------------------------------------------------------------
			-- Create Administration.ProcessNextLSN.
			----------------------------------------------------------------------------------------
			IF NOT EXISTS(SELECT 1 FROM sys.objects [O] INNER JOIN sys.schemas [S] 
				ON [O].schema_id = [S].schema_id 
				WHERE [O].[name] = N'ProcessNextLSN' 
				AND [S].[name] = N'Administration' AND [O].[type] = N'P')
			BEGIN
				RAISERROR(N'Creating Administration.ProcessNextLSN', 10, 1);
				SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
				WHERE [CommandName]  = 
					N'Administration.ProcessNextLSN' 
					;
				EXEC(@Cmd);
			END

			----------------------------------------------------------------------------------------
			-- Populate operations.
			----------------------------------------------------------------------------------------
			RAISERROR(N'Updating Administration.Operation.', 10, 1);
			SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
			WHERE [CommandName]  = 
				N'PopulateOperations' ;
			EXEC(@Cmd);

			----------------------------------------------------------------------------------------
			-- Create Job To Process Next LSN
			----------------------------------------------------------------------------------------
			SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
			WHERE [CommandName]  = 
				N'CreateJobToProcessNextLSN' ;
			SELECT @Cmd = REPLACE(@Cmd, '#(DatabaseName)', [Core].[EscapeQuotes](@DatabaseName));
			EXEC(@Cmd);
		
			----------------------------------------------------------------------------------------
			-- Finally update the configuration value.
			----------------------------------------------------------------------------------------
			RAISERROR(N'Updating configuration value.', 10, 1);
			UPDATE [Core].[Configuration] SET [ConfigValue] = 'True' 
			WHERE [ConfigName] = N'IsChangeHistoryEnabled';
			
		COMMIT TRANSACTION CreateChangeHistoryObjects
	END TRY
	BEGIN CATCH
		-- It is important to check for the presence of correct transaction before doing a ROLLBACK.
		-- For example, if this procedure is invoked from within a transaction, ALTER DATABASE
		-- statements will fail and the control comes here before the creation of 
		-- CreateChangeHistoryObjects transaction. Now if we rollback CreateChangeHistoryObjects,
		-- exceptions will be raised since it was never created!
		IF EXISTS(SELECT 1 FROM sys.dm_tran_active_transactions 
			WHERE name=N'CreateChangeHistoryObjects')
				ROLLBACK TRANSACTION CreateChangeHistoryObjects;
		DECLARE @Msg nvarchar(4000);
		SELECT @Msg = ERROR_MESSAGE();
		RAISERROR(@Msg, 16, 1);
		GOTO FINISH;
	END CATCH

FINISH:	

END


GO
PRINT N'Creating Administration.EnableFullTextSearch...';


GO
CREATE PROCEDURE [Administration].[EnableFullTextSearch]
@FullTextCatalogFilePath NVARCHAR (512)
AS
BEGIN
	------------------------------------------------------------------------------------------------
	-- Validate input parameters.
	------------------------------------------------------------------------------------------------
	IF(@FullTextCatalogFilePath IS NULL OR @FullTextCatalogFilePath = '')
	BEGIN
		RAISERROR(N'@FullTextCatalogFilePath cannot be null or empty.', 16, 1);
		GOTO FINISH;
	END

	------------------------------------------------------------------------------------------------
	-- Verify that the full text search is installed with the current instance.
	------------------------------------------------------------------------------------------------
	RAISERROR(N'Verifying full text search installation.', 10, 1);
	IF
	(
		FULLTEXTSERVICEPROPERTY('IsFulltextInstalled') <> 1
	)
	BEGIN
		RAISERROR(N'The full-text component is not installed with the current instance of SQL Server.', 
			16, 1);
		GOTO FINISH;
	END


	BEGIN TRY
		DECLARE @DatabaseName nvarchar(128);
		SELECT @DatabaseName = DB_NAME();
		DECLARE @Cmd nvarchar(max);

		--------------------------------------------------------------------------------------------
		-- Create change full text search filegroup and files. We do not enclose these in a 
		-- transaction since ALTER DATABASE statement is not allowed within multi-statement 
		-- transaction. Also, we do not remove earlier files from the filegroup, so multiple
		-- invocations of this procedure would result in multiple files for the FTS file group.
		--------------------------------------------------------------------------------------------
		IF NOT EXISTS(SELECT 1 FROM sys.filegroups WHERE [name] = N'FullTextSearch')
		BEGIN
			RAISERROR(N'Creating full text search filegroup.', 10, 1);
			SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
			WHERE [CommandName]  = 
				N'FullTextSearch.FileGroup' ;
			-- Replace token.
			SELECT @Cmd = REPLACE(@Cmd, '#(DatabaseName)', [Core].[EscapeBrackets](@DatabaseName));
			EXEC(@Cmd);
		END
		
		RAISERROR(N'Adding file to full text search filegroup.', 10, 1);
		SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
		WHERE [CommandName]  = 
			N'FullTextSearch.File' ;
		-- Replace tokens.
		SELECT @Cmd = REPLACE(@Cmd, '#(DatabaseName)', [Core].[EscapeBrackets](@DatabaseName));
		SELECT @Cmd = REPLACE(@Cmd, '#(FullTextSearch)', [Core].[EscapeBrackets]('FullTextSearch' + 
			CAST(NEWID() AS [nvarchar](128))));
		SELECT @Cmd = REPLACE(@Cmd, '#(FullTextCatalogFilePath)', 
			[Core].[EscapeQuotes](@FullTextCatalogFilePath));
		EXEC(@Cmd);
		
		IF NOT EXISTS(SELECT 1 FROM sys.fulltext_catalogs WHERE [name] = N'ZentityCatalog')
		BEGIN
			RAISERROR(N'Creating full text catalog.', 10, 1);
			SELECT @Cmd = [CommandValue] FROM [Administration].[Command]
			WHERE [CommandName]  = 
				N'ZentityCatalog.Catalog' ;
			EXEC(@Cmd);
		END

		-- Create rest of the database objects.
		DECLARE @Version nvarchar(128), @MajorVersion int;
		SELECT @Version = CAST( SERVERPROPERTY('ProductVersion') AS nvarchar(4000));
		SELECT @MajorVersion = CAST(LEFT(@Version,CHARINDEX('.',@Version,0)-1) AS int);

		----------------------------------------------------------------------------------------
		-- Create Core.Content Full Text Index. Again these statements are out of a transaction
		-- since CREATE FULLTEXT INDEX statement cannot be used inside a user transaction.
		----------------------------------------------------------------------------------------
		IF (@MajorVersion = 10) AND 
			NOT EXISTS(SELECT 1 FROM sys.fulltext_indexes 
				WHERE [object_id] = OBJECT_ID('Core.Content'))
		BEGIN
			RAISERROR(N'Creating Core.Content Full Text Index.', 10, 1);
			EXEC(N'	
				CREATE FULLTEXT INDEX ON [Core].[Content]
				(
					[Content] TYPE COLUMN [FileExtension]
				)
				KEY INDEX [PK_Content]
				ON [ZentityCatalog]
				WITH CHANGE_TRACKING OFF, NO POPULATION;
				
				ALTER FULLTEXT INDEX ON [Core].[Content] ENABLE;
			');
		END

		----------------------------------------------------------------------------------------
		-- Create Core.Resource Full Text Index. Note that we've not included @MajorVersion
		-- check here. We also allow other versions of SQL provided they have FTS installed.
		----------------------------------------------------------------------------------------
		IF NOT EXISTS(SELECT 1 FROM sys.fulltext_indexes 
				WHERE [object_id] = OBJECT_ID('Core.Resource'))
		BEGIN
			RAISERROR(N'Creating Core.Resource Full Text Index.', 10, 1);
			EXEC(N'
				CREATE FULLTEXT INDEX ON [Core].[Resource]
				(
					[Title],
					[Uri],
					[Description]
				)
				KEY INDEX [PK_Resource]
				ON [ZentityCatalog]
				WITH CHANGE_TRACKING OFF, NO POPULATION;

				ALTER FULLTEXT INDEX ON [Core].[Resource] ENABLE;
			');
		END

		----------------------------------------------------------------------------------------
		-- Create Core.ResourceProperty Full Text Index.
		----------------------------------------------------------------------------------------
		IF NOT EXISTS(SELECT 1 FROM sys.fulltext_indexes 
				WHERE [object_id] = OBJECT_ID('Core.ResourceProperty'))
		BEGIN
			RAISERROR(N'Creating Core.ResourceProperty Full Text Index.', 10, 1);
			EXEC(N'
				CREATE FULLTEXT INDEX ON [Core].[ResourceProperty]
				(
					[Value]
				)
				KEY INDEX [PK_ResourceProperty]
				ON [ZentityCatalog]
				WITH CHANGE_TRACKING OFF, NO POPULATION;
				
				ALTER FULLTEXT INDEX ON [Core].[ResourceProperty] ENABLE;
			');
		END

		-- Create rest of the objects in a transaction.
		BEGIN TRANSACTION CreateFullTextSearchObjects
	
			----------------------------------------------------------------------------------------
			-- Create Full Text Index population job.
			----------------------------------------------------------------------------------------
			DECLARE @CmdIncrementalPopulation nvarchar(4000);
			SELECT @CmdIncrementalPopulation = N'';
			
			IF EXISTS(SELECT 1 FROM sys.fulltext_indexes WHERE [object_id] = OBJECT_ID('[Core].[Content]'))
				SELECT @CmdIncrementalPopulation = @CmdIncrementalPopulation + 
				N'ALTER FULLTEXT INDEX ON [Core].[Content] START INCREMENTAL POPULATION;';

			IF EXISTS(SELECT 1 FROM sys.fulltext_indexes WHERE [object_id] = OBJECT_ID('[Core].[Resource]'))
				SELECT @CmdIncrementalPopulation = @CmdIncrementalPopulation + 
				N'ALTER FULLTEXT INDEX ON [Core].[Resource] START INCREMENTAL POPULATION;';

			IF EXISTS(SELECT 1 FROM sys.fulltext_indexes WHERE [object_id] = OBJECT_ID('[Core].[ResourceProperty]'))
				SELECT @CmdIncrementalPopulation = @CmdIncrementalPopulation + 
				N'ALTER FULLTEXT INDEX ON [Core].[ResourceProperty] START INCREMENTAL POPULATION;';

			IF  EXISTS (SELECT job_id FROM msdb.dbo.sysjobs_view 
				WHERE [name] = N'IncrementalIndexPopulation')
			BEGIN
				RAISERROR(N'Deleting IncrementalIndexPopulation job.', 10, 1);
				EXEC msdb.dbo.sp_delete_job @job_name=N'IncrementalIndexPopulation';
			END

			IF NOT EXISTS (SELECT [name] FROM msdb.dbo.syscategories 
				WHERE name=N'Full-Text' AND category_class=1)
				   EXEC msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'Full-Text';

			RAISERROR(N'Creating IncrementalIndexPopulation job.', 10, 1);
			DECLARE @jobId BINARY(16);
			EXEC  msdb.dbo.sp_add_job @job_name=N'IncrementalIndexPopulation', 
				@enabled=1, 
				@start_step_id=1, 
				@category_name=N'Full-Text', 
				@job_id = @jobId OUTPUT;

			DECLARE @DbName nvarchar(256);
			SELECT @DbName = DB_NAME();
			EXEC msdb.dbo.sp_add_jobstep @job_id=@jobId, 
				@step_name=N'StartIndexing', 
				@step_id=1, 
				@cmdexec_success_code=0, 
				@on_success_action=1, 
				@on_success_step_id=-1, 
				@on_fail_action=2, 
				@on_fail_step_id=-1, 
				@retry_attempts=0, 
				@retry_interval=0, 
				@os_run_priority=0, @subsystem=N'TSQL', 
				@command=@CmdIncrementalPopulation, 
				@database_name=@DbName, 
				@flags=0;

			EXEC msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1;

			EXEC msdb.dbo.sp_add_jobserver @job_id=@jobId;

			DECLARE @schedule_id int
			EXEC msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'IncrementalIndexPopulationSchedule', 
				@enabled=1, 
				@freq_type=4, 
				@freq_interval=1, 
				@freq_subday_type=2, 
				@freq_subday_interval=10, 
				@freq_relative_interval=0, 
				@freq_recurrence_factor=1, 
				@active_start_date=20080804, 
				@active_end_date=99991231, 
				@active_start_time=0, 
				@active_end_time=235959, 
				@schedule_id = @schedule_id OUTPUT

			----------------------------------------------------------------------------------------
			-- Finally update the configuration value.
			----------------------------------------------------------------------------------------
			RAISERROR(N'Updating configuration value.', 10, 1);
			UPDATE [Core].[Configuration] SET [ConfigValue] = 'True' 
			WHERE [ConfigName] = N'IsFullTextSearchEnabled';
			
		COMMIT TRANSACTION CreateFullTextSearchObjects

	END TRY
	BEGIN CATCH
		-- It is important to check for the presence of correct transaction before doing a ROLLBACK.
		IF EXISTS(SELECT 1 FROM sys.dm_tran_active_transactions 
			WHERE name=N'CreateFullTextSearchObjects')
				ROLLBACK TRANSACTION CreateFullTextSearchObjects;
		DECLARE @Msg nvarchar(4000);
		SELECT @Msg = ERROR_MESSAGE();
		RAISERROR(@Msg, 16, 1);
		GOTO FINISH;
	END CATCH

	FINISH:	
END


GO
PRINT N'Creating Core.ValidateInsertsIntoResourceTypeProperty...';


GO
CREATE PROCEDURE [Core].[ValidateInsertsIntoResourceTypeProperty]

AS
BEGIN
	-- Check for duplicate Ids.
	DECLARE @DuplicateId [uniqueidentifier];
	DECLARE @Msg [nvarchar](4000);
	SELECT TOP(1) @DuplicateId = [Id] FROM 
	(
		SELECT [Id] FROM [Core].[ScalarProperty]
		UNION ALL
		SELECT [Id] FROM [Core].[NavigationProperty]
	) [AllIds]
	GROUP BY [AllIds].[Id]
	HAVING COUNT(1) > 1;

	IF @DuplicateId IS NOT NULL
	BEGIN
		SET @Msg = 'Duplicate resource property Id {' + CAST(@DuplicateId as [nvarchar](128)) +
		'} detected. Make sure that the identifiers are unique across all the scalar and navigation properties.';

		RAISERROR (@Msg, 16, 1)
		ROLLBACK TRANSACTION
	END

	-- Check for duplicate Names across same resource types.
	DECLARE @DuplicateResourceTypeId [uniqueidentifier];
	DECLARE @DuplicateName [nvarchar](100);
	SELECT TOP(1) @DuplicateResourceTypeId = [ResourceTypeId], @DuplicateName = [Name] FROM 
	(
		SELECT [ResourceTypeId], [Name] FROM [Core].[ScalarProperty]
		UNION ALL
		SELECT [ResourceTypeId], [Name] FROM [Core].[NavigationProperty]
	) [AllNames]
	GROUP BY [AllNames].[ResourceTypeId], [AllNames].[Name] 
	HAVING COUNT(1) > 1;

	IF @DuplicateResourceTypeId IS NOT NULL
	BEGIN
		SELECT @Msg = 'Duplicate resource property name [' + [Core].EscapeQuotes(@DuplicateName) +
		'] detected for the resource type with Id = {' + CAST(@DuplicateResourceTypeId as [nvarchar](128))+
		'}. Make sure that the property names are unique across all the scalar and navigation properties for a resource type.';

		RAISERROR (@Msg, 16, 1)
		ROLLBACK TRANSACTION
	END
END


GO
PRINT N'Creating Core.GetClassHierarchy...';


GO
CREATE FUNCTION [Core].[GetClassHierarchy]
(@Id UNIQUEIDENTIFIER)
RETURNS 
    @Hierarchy TABLE (
        [Id]              UNIQUEIDENTIFIER NULL,
        [Namespace]       NVARCHAR (150)   NULL,
        [Name]            NVARCHAR (100)   NULL,
        [IsWellknownType] BIT              NULL,
        [BaseTypeId]      UNIQUEIDENTIFIER NULL)
AS
BEGIN
	WITH [ClassHierarchy] AS
	(
		-- Anchor query: Start from the entry in resource type catalog for the input class.
		SELECT [resType].[Id], 
				[module].[Namespace], 
				[resType].[Name], 
				[module].[IsMsShipped] AS [IsWellknownType], 
				[resType].[BaseTypeId]
		FROM [Core].[DataModelModule] AS [module]
		INNER JOIN [Core].[ResourceType] AS [resType]
		ON [module].[Id] = [resType].[DataModelModuleId]
		WHERE [resType].[Id] = @Id
		
		UNION ALL
		
		-- Recursive query: Fetch the base type of resource type retrieved in previous run.
		-- Continue until we hit a null base type.
		SELECT [resType].[Id], 
				[module].[Namespace], 
				[resType].[Name], 
				[module].[IsMsShipped] AS [IsWellknownType], 
				[resType].[BaseTypeId]
		FROM [Core].[DataModelModule] AS [module]
		INNER JOIN [Core].[ResourceType] AS [resType]
		ON [module].[Id] = [resType].[DataModelModuleId]
		INNER JOIN [ClassHierarchy] [B] 
		ON [B].[BaseTypeId] IS NOT NULL 
		AND [resType].[Id] = [B].[BaseTypeId]
	)
	INSERT @Hierarchy
	SELECT * FROM [ClassHierarchy];
	
	RETURN
END


GO
PRINT N'Creating Core.GetDerivedTypes...';


GO
CREATE FUNCTION [Core].[GetDerivedTypes]
(@Id UNIQUEIDENTIFIER)
RETURNS 
    @Hierarchy TABLE (
        [Id] UNIQUEIDENTIFIER NULL)
AS
BEGIN
	WITH [Hierarchy] AS
	(
		-- Anchor query: Start from the entry in ResourceType for the input class.
		SELECT [RT].[Id] [Id] 
		FROM [Core].[ResourceType] [RT] 
		WHERE [RT].[Id] = @Id
		
		UNION ALL
		
		-- Recursive query: Fetch the derived types of the resource types retrieved in previous run.
		SELECT [Derived].[Id] [Id]
		FROM [Core].[ResourceType] [Derived] 
		INNER JOIN [Hierarchy] [B] 
		ON [Derived].[BaseTypeId] = [B].[Id]
	)
	INSERT @Hierarchy
	SELECT * FROM [Hierarchy];
	
	RETURN
END


GO
PRINT N'Creating Core.CheckResourceTypeCompatibilityForTriple...';


GO
CREATE PROCEDURE [Core].[CheckResourceTypeCompatibilityForTriple]
@SubjectResourceId UNIQUEIDENTIFIER, @ObjectResourceId UNIQUEIDENTIFIER, @PredicateId UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

	-- Validate parameters.
	IF(@SubjectResourceId IS NULL OR @ObjectResourceId IS NULL OR @PredicateId IS NULL)
	BEGIN
		DECLARE @MsgInvalidParameters [nvarchar](1024);
		SET @MsgInvalidParameters = N'Parameters SubjectResourceId, ObjectResourceId and ' + 
		'PredicateId cannot have NULL values.';
		RAISERROR(@MsgInvalidParameters, 16, 1);
		RETURN -1;
	END

	IF NOT EXISTS(SELECT 1 FROM [Core].[Predicate] WHERE [Id] = @PredicateId)
	BEGIN
		RAISERROR('Cannot find the predicate in the repository.', 16, 1);
		RETURN -1;
	END
	
	-- We return success if the predicate is a custom predicate and is not an association predicate.
	DECLARE @AssociationId [uniqueidentifier];
	DECLARE @AssociationSubjectTypeId [uniqueidentifier];
	DECLARE @AssociationObjectTypeId [uniqueidentifier];
	
	SELECT @AssociationId = [A].[Id], 
		@AssociationSubjectTypeId = [SubjectNP].[ResourceTypeId],
		@AssociationObjectTypeId = [ObjectNP].[ResourceTypeId]
	FROM [Core].[Association] [A]
	INNER JOIN [Core].[NavigationProperty] [SubjectNP]
	ON [A].[SubjectNavigationPropertyId] = [SubjectNP].[Id]
	INNER JOIN [Core].[NavigationProperty] [ObjectNP]
	ON [A].[ObjectNavigationPropertyId] = [ObjectNP].[Id]
	WHERE [A].[PredicateId] = @PredicateId;
	
	IF(@AssociationId IS NULL)
		RETURN 0;
		
	-- Predicate is an association predicate.
	-- Retrieve Resource Type Ids of Subject and Object Resources.
	DECLARE @ResourceSubjectTypeId [uniqueidentifier];
	DECLARE @ResourceObjectTypeId [uniqueidentifier];

	-- We are trying to avoid joins here, they might take some time for large
	-- number of resources.
	SELECT 
	@ResourceSubjectTypeId = CASE WHEN [Id] = @SubjectResourceId THEN [ResourceTypeId] 
		ELSE @ResourceSubjectTypeId END,
	@ResourceObjectTypeId = CASE WHEN [Id] = @ObjectResourceId THEN [ResourceTypeId] 
		ELSE @ResourceObjectTypeId END
	FROM [Core].[Resource]
	WHERE [Id] = @SubjectResourceId OR
	[Id] = @ObjectResourceId;

	IF(@ResourceSubjectTypeId IS NULL)
	BEGIN
		RAISERROR('Cannot find the subject resource in the repository.', 16, 1);
		RETURN -1;
	END

	IF(@ResourceObjectTypeId IS NULL)
	BEGIN
		RAISERROR('Cannot find the object resource in the repository.', 16, 1);
		RETURN -1;
	END

	-- Check the compatibilities of resource subject and object types with association
	-- subject and object types.
	IF NOT EXISTS( SELECT 1 FROM 
		[Core].[GetDerivedTypes](@AssociationSubjectTypeId) [SubjectTypes],
		[Core].[GetDerivedTypes](@AssociationObjectTypeId) [ObjectTypes]
		WHERE [SubjectTypes].[Id] = @ResourceSubjectTypeId
		AND [ObjectTypes].[Id] = @ResourceObjectTypeId)
	BEGIN
		DECLARE @MsgInvalidAssociation [nvarchar](4000);
		SET @MsgInvalidAssociation = 'The triple {SubjectId = ' + 
		CAST(@SubjectResourceId AS [nvarchar](128)) + 
		', ObjectId = ' + CAST(@ObjectResourceId AS [nvarchar](128)) + 
		', PredicateId = ' + CAST(@PredicateId AS [nvarchar](128)) + 
		'} is invalid. The Predicate Id corresponds to an Association that cannot be created ' +
		'between resources of Subject Type Id = ' +
		CAST(@ResourceSubjectTypeId AS [nvarchar](128)) + ' and Object Type Id = ' +
		CAST(@ResourceObjectTypeId AS [nvarchar](128)) + '.';
		RAISERROR (@MsgInvalidAssociation, 16, 1)
		RETURN -1;
	END
	
	RETURN 0;
END


GO
PRINT N'Creating Core.InsertRelationship...';


GO
CREATE PROCEDURE [Core].[InsertRelationship]
@Id UNIQUEIDENTIFIER, @SubjectResourceId UNIQUEIDENTIFIER, @ObjectResourceId UNIQUEIDENTIFIER, @PredicateId UNIQUEIDENTIFIER, @OrdinalPosition INT, @DateAdded DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
	-- We need not check for NULLs or duplicates here, that is taken care of by the table and index
	-- constraints.
	
	-- Check Resource Type Compatibility.
	DECLARE	@Result [int];
	EXEC @Result = [Core].[CheckResourceTypeCompatibilityForTriple]
		@SubjectResourceId = @SubjectResourceId,
		@ObjectResourceId = @ObjectResourceId,
		@PredicateId = @PredicateId;
	IF(@Result<>0)
		RETURN -1;

	-- If the relationship predicate has a corresponding OneToXXX or XXXToOne association, 
	-- reject this insert.
	DECLARE @SubjectMultiplicity [nvarchar](32);
	DECLARE @ObjectMultiplicity [nvarchar](32);

	SELECT TOP(1) @SubjectMultiplicity = [A].[SubjectMultiplicity],
		@ObjectMultiplicity = [A].[ObjectMultiplicity]
	FROM [Core].[Association] [A]
	WHERE [A].[PredicateId] = @PredicateId
	AND([A].[SubjectMultiplicity] = 'One'
	OR [A].[ObjectMultiplicity] = 'One');

	IF @SubjectMultiplicity IS NOT NULL
	BEGIN
		DECLARE @Msg [nvarchar](4000);
		SELECT @Msg = 'The triple {Subject=' + CAST(@SubjectResourceId AS [nvarchar](64)) + 
		', Object=' + CAST(@ObjectResourceId AS [nvarchar](64)) + 
		', Predicate=' + CAST(@PredicateId AS [nvarchar](64)) + 
		'} cannot be inserted. The predicate represents a ' + @SubjectMultiplicity + 
		' to ' + @ObjectMultiplicity + 
		' association. Such relationships are created only at time when the entity on [' + 
		CASE WHEN @SubjectMultiplicity = 'One' THEN @ObjectMultiplicity 
		ELSE @SubjectMultiplicity END + 
		'] side is created with its navigation property pointing to the entity on [One] side set correct.';

		RAISERROR (@Msg, 16, 1);
		RETURN -1;
	END

	-- Insert into Relationship
	INSERT INTO [Core].[Relationship] 
	( 
		[Id], 
		[SubjectResourceId], 
		[ObjectResourceId], 
		[PredicateId], 
		[OrdinalPosition], 
		[DateAdded]
	)
	VALUES
	( 
		@Id, 
		@SubjectResourceId, 
		@ObjectResourceId, 
		@PredicateId, 
		@OrdinalPosition, 
		@DateAdded
	);
	
	RETURN 0;
END


GO
PRINT N'Creating Core.UpdateRelationship...';


GO
CREATE PROCEDURE [Core].[UpdateRelationship]
@Id UNIQUEIDENTIFIER, @SubjectResourceId UNIQUEIDENTIFIER, @ObjectResourceId UNIQUEIDENTIFIER, @PredicateId UNIQUEIDENTIFIER, @OrdinalPosition INT, @DateAdded DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
	-- We need not check for NULLs or duplicates or the presence of actual resource here.
	-- It is taken care of by the FKs and index constraints.

	-- Prevent updates of association predicates.
	DECLARE @PreviousPredicateId [uniqueidentifier];
	SELECT @PreviousPredicateId = [PredicateId]
	FROM [Core].[Relationship]
	WHERE [Id] = @Id;

	IF EXISTS(SELECT 1 FROM [Core].[Association] 
		WHERE [PredicateId] = @PreviousPredicateId
		AND @PreviousPredicateId <> @PredicateId)
	BEGIN
		DECLARE @Msg [nvarchar](4000);
		SELECT @Msg = 'The relationship with Id = ' + CAST(@Id AS [nvarchar](64)) + 
		' has undergone a predicate change. Previous predicate is being used by an association.' + 
		' We do not support updates of association predicates in this version of Zentity.';
		RAISERROR (@Msg, 16, 1)
		RETURN -1;
	END
	
	-- Check Resource Type Compatibility.
	DECLARE	@Result [int];
	EXEC @Result = [Core].[CheckResourceTypeCompatibilityForTriple]
		@SubjectResourceId = @SubjectResourceId,
		@ObjectResourceId = @ObjectResourceId,
		@PredicateId = @PredicateId;
	IF(@Result<>0)
		RETURN -1;

	-- If the relationship predicate has a corresponding OneToZeroOrOne or ZeroOrOneToOne
	-- association, return error. We do not support the updates of relationships with 
	-- these cardinalities explicitly. Users can create associations using navigation properties
	-- on these entities. The main issue is the propagation of these changes to the resource
	-- table. We had to ensure that such changes happen in pair. For example, if there are two
	-- relationships A <--> a and B <--> b. And we updated them to A <--> a and A <--> b. This
	-- breaks the OneToZeroOrOne constraint since A is now connected to two dependent objects.
	-- So there must be another update that removes one of these entries. The logic is complex
	-- and thus, we are leaving it for now.
	DECLARE @SubjectMultiplicity [nvarchar](32);
	DECLARE @ObjectMultiplicity [nvarchar](32);
	
	SELECT @SubjectMultiplicity = [A].[SubjectMultiplicity],
		@ObjectMultiplicity = [A].[ObjectMultiplicity]
	FROM [Core].[Association] [A]
	WHERE [A].[PredicateId] = @PredicateId;
	
	IF (@SubjectMultiplicity = 'One' AND @ObjectMultiplicity = 'ZeroOrOne')
	OR (@SubjectMultiplicity = 'ZeroOrOne' AND @ObjectMultiplicity = 'One')
	BEGIN
		-- Custom predicate ids won't pass the condition.
		SELECT @Msg = 'The triple {Subject=' + CAST(@SubjectResourceId AS [nvarchar](64)) + 
		', Object=' + CAST(@ObjectResourceId AS [nvarchar](64)) + 
		', Predicate=' + CAST(@PredicateId AS [nvarchar](64)) + 
		'} cannot be updated. The predicate represents a ' + @SubjectMultiplicity + 
		' to ' + @ObjectMultiplicity + ' association and explicit updating ' +
		'of ''One to ZeroOrOne'' or ''ZeroOrOne to One'' association is not supported ' +
		'in this version of Zentity. Please use the navigation properties on these entities ' +
		'to update the relationships rather than manipulating the relationship directly.';

		RETURN -1;
	END
	
	UPDATE [Core].[Relationship] 
	SET
		[SubjectResourceId] = @SubjectResourceId, 
		[ObjectResourceId] = @ObjectResourceId, 
		[PredicateId] = @PredicateId, 
		[OrdinalPosition] = @OrdinalPosition, 
		[DateAdded] = @DateAdded
	WHERE [Id] = @Id;
	
	-- Prepare the query to be fired on Resource table.
	DECLARE @Query [nvarchar](2048);
	SELECT @Query = 
	CASE
		WHEN [SubjectNP].[ColumnName] IS NOT NULL THEN 
			'UPDATE [Core].[' + [SubjectNP].[TableName] + 
			'] SET [' + [SubjectNP].[ColumnName] + 
			'] = @ObjectResourceId WHERE [Id] = @SubjectResourceId;'
		ELSE
			'UPDATE [Core].[' + [ObjectNP].[TableName] + 
			'] SET [' + [ObjectNP].[ColumnName] + 
			'] = @SubjectResourceId WHERE [Id] = @ObjectResourceId;' 
	END
	FROM [Core].[Association] [A]
	INNER JOIN [Core].[NavigationProperty] [SubjectNP]
	ON [A].[SubjectNavigationPropertyId] = [SubjectNP].[Id]
	INNER JOIN [Core].[NavigationProperty] [ObjectNP]
	ON [A].[ObjectNavigationPropertyId] = [ObjectNP].[Id]
	WHERE [A].[PredicateId] = @PredicateId
	AND([SubjectNP].[ColumnName] IS NOT NULL 
	OR [ObjectNP].[ColumnName] IS NOT NULL);
	
	IF @Query IS NOT NULL
	BEGIN
		EXEC sp_executesql @Query,
		N'@SubjectResourceId [uniqueidentifier], @ObjectResourceId [uniqueidentifier]',	
		@SubjectResourceId,	@ObjectResourceId;
	END
	
	RETURN 0;
END


GO
PRINT N'Creating Core.AllowedAssociationAndTypes...';


GO
CREATE VIEW [Core].[AllowedAssociationAndTypes]
AS
SELECT [A].[Id] [AssociationId], [A].[PredicateId], 
	[AllowedSubjectTypeIds].[Id] [AllowedSubjectTypeId],	
	[AllowedObjectTypeIds].[Id] [AllowedObjectTypeId] 
	FROM [Core].[Association] [A]
	INNER JOIN [Core].[NavigationProperty] [SubjectNP]
	ON [A].[SubjectNavigationPropertyId] = [SubjectNP].[Id]
	INNER JOIN [Core].[NavigationProperty] [ObjectNP]
	ON [A].[ObjectNavigationPropertyId] = [ObjectNP].[Id]
	CROSS APPLY [Core].[GetDerivedTypes]([SubjectNP].[ResourceTypeId]) [AllowedSubjectTypeIds]
	CROSS APPLY [Core].[GetDerivedTypes]([ObjectNP].[ResourceTypeId]) [AllowedObjectTypeIds];


GO
PRINT N'Creating Core.ResourceHasFile...';


GO
CREATE VIEW [Core].[ResourceHasFile]
WITH SCHEMABINDING
AS
SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = '818A93F5-25A9-4149-A8D2-19104A352DA0';


GO
PRINT N'Creating Core.ResourceHasFile.PK_ResourceHasFile...';


GO
CREATE UNIQUE CLUSTERED INDEX [PK_ResourceHasFile]
    ON [Core].[ResourceHasFile]([SubjectResourceId] ASC, [ObjectResourceId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating Core.Configuration.ConfigName.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Name of the configuration.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'Configuration', @level2type = N'COLUMN', @level2name = N'ConfigName';


GO
PRINT N'Creating Core.Configuration.ConfigValue.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Value of the configuration.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'Configuration', @level2type = N'COLUMN', @level2name = N'ConfigValue';


GO
PRINT N'Creating Core.Predicate.Id.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for the predicate.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'Predicate', @level2type = N'COLUMN', @level2name = N'Id';


GO
PRINT N'Creating Core.Predicate.Uri.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Uri of the predicate.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'Predicate', @level2type = N'COLUMN', @level2name = N'Uri';


GO
PRINT N'Creating Core.PredicateProperty.Id.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The surrogate key for Predicate property.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'PredicateProperty', @level2type = N'COLUMN', @level2name = N'Id';


GO
PRINT N'Creating Core.PredicateProperty.PredicateId.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Id of the predicate.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'PredicateProperty', @level2type = N'COLUMN', @level2name = N'PredicateId';


GO
PRINT N'Creating Core.PredicateProperty.PropertyId.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Id of the property.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'PredicateProperty', @level2type = N'COLUMN', @level2name = N'PropertyId';


GO
PRINT N'Creating Core.PredicateProperty.Value.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Value of the Predicate property.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'PredicateProperty', @level2type = N'COLUMN', @level2name = N'Value';


GO
PRINT N'Creating Core.Property.Id.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Primary key for the property.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'Property', @level2type = N'COLUMN', @level2name = N'Id';


GO
PRINT N'Creating Core.Property.Name.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Name of the property.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'Property', @level2type = N'COLUMN', @level2name = N'Name';


GO
PRINT N'Creating Core.Relationship.Id.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Surrogate key for the relationship triple.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'Relationship', @level2type = N'COLUMN', @level2name = N'Id';


GO
PRINT N'Creating Core.Relationship.SubjectResourceId.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The subject Resource of the relationship triple.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'Relationship', @level2type = N'COLUMN', @level2name = N'SubjectResourceId';


GO
PRINT N'Creating Core.Relationship.ObjectResourceId.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The object Resource of the relationship triple.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'Relationship', @level2type = N'COLUMN', @level2name = N'ObjectResourceId';


GO
PRINT N'Creating Core.Relationship.PredicateId.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The predicate of the relationship triple.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'Relationship', @level2type = N'COLUMN', @level2name = N'PredicateId';


GO
PRINT N'Creating Core.Relationship.OrdinalPosition.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The ordinal position of the subject of the relationship with respect to the object.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'Relationship', @level2type = N'COLUMN', @level2name = N'OrdinalPosition';


GO
PRINT N'Creating Core.RelationshipProperty.Id.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Surrogate key for the relationship triple property.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'RelationshipProperty', @level2type = N'COLUMN', @level2name = N'Id';


GO
PRINT N'Creating Core.RelationshipProperty.TripletId.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Id of the relationship triple.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'RelationshipProperty', @level2type = N'COLUMN', @level2name = N'TripletId';


GO
PRINT N'Creating Core.RelationshipProperty.PropertyId.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Property for the relationship triple.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'RelationshipProperty', @level2type = N'COLUMN', @level2name = N'PropertyId';


GO
PRINT N'Creating Core.RelationshipProperty.Value.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Value of the relationship triple property.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'RelationshipProperty', @level2type = N'COLUMN', @level2name = N'Value';


GO
PRINT N'Creating Core.ResourceProperty.Id.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The surrogate key for Resource property.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'ResourceProperty', @level2type = N'COLUMN', @level2name = N'Id';


GO
PRINT N'Creating Core.ResourceProperty.ResourceId.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Id of the Resource.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'ResourceProperty', @level2type = N'COLUMN', @level2name = N'ResourceId';


GO
PRINT N'Creating Core.ResourceProperty.PropertyId.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Id of the property.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'ResourceProperty', @level2type = N'COLUMN', @level2name = N'PropertyId';


GO
PRINT N'Creating Core.ResourceProperty.Value.MS_Description...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Value of the Resource property.', @level0type = N'SCHEMA', @level0name = N'Core', @level1type = N'TABLE', @level1name = N'ResourceProperty', @level2type = N'COLUMN', @level2name = N'Value';


GO
PRINT N'Creating Core.AfterDeleteFromAssociation...';


GO
CREATE TRIGGER [Core].[AfterDeleteFromAssociation]
    ON [Core].[Association]
    AFTER DELETE
    AS BEGIN
	DECLARE @AssociationId [uniqueidentifier];
	DECLARE @DataModelModuleId [uniqueidentifier];

	SELECT TOP(1) @AssociationId = [MsShippedAssociations].[AssociationId], 
	@DataModelModuleId = [MsShippedAssociations].[DataModelModuleId]
	FROM
	( 
		SELECT [A].[Id] [AssociationId], [M].[Id] [DataModelModuleId]
		FROM deleted [A]
		INNER JOIN [Core].[NavigationProperty] [SP]
		ON [A].[SubjectNavigationPropertyId] = [SP].[Id]
		INNER JOIN [Core].[ResourceType] [RT]
		ON [SP].[ResourceTypeId] = [RT].[Id]
		INNER JOIN [Core].[DataModelModule] [M]
		ON [RT].[DataModelModuleId] = [M].[Id]
		WHERE [M].[IsMsShipped] = 1
		UNION
		SELECT [A].[Id] [AssociationId], [M].[Id] [DataModelModuleId]
		FROM deleted [A]
		INNER JOIN [Core].[NavigationProperty] [OP]
		ON [A].[ObjectNavigationPropertyId] = [OP].[Id]
		INNER JOIN [Core].[ResourceType] [RT]
		ON [OP].[ResourceTypeId] = [RT].[Id]
		INNER JOIN [Core].[DataModelModule] [M]
		ON [RT].[DataModelModuleId] = [M].[Id]
		WHERE [M].[IsMsShipped] = 1
	)[MsShippedAssociations];

	IF(@AssociationId IS NOT NULL)
	BEGIN
		DECLARE @Msg [nvarchar](4000);
		SET @Msg = 'The association with Id ={' + CAST(@AssociationId AS [nvarchar](128)) + 
		'} belongs to an MsShipped module. It is an error to delete items from MsShipped modules.';
		
		RAISERROR (@Msg, 16, 1)
		ROLLBACK TRANSACTION
	END
END


GO
PRINT N'Creating Core.AfterInsertUpdateOnNavigationProperty...';


GO
CREATE TRIGGER [Core].[AfterInsertUpdateOnNavigationProperty]
    ON [Core].[NavigationProperty]
    AFTER INSERT, UPDATE
    AS BEGIN
	EXEC [Core].[ValidateInsertsIntoResourceTypeProperty]
END


GO
PRINT N'Creating Core.AfterInsertUpdateOnPredicate...';


GO
CREATE TRIGGER [Core].[AfterInsertUpdateOnPredicate]
    ON [Core].[Predicate]
    AFTER INSERT, UPDATE
    AS BEGIN
	-- Check for empty Uri.
	DECLARE @PredicateName [nvarchar](128);
	DECLARE @DuplicateUri [nvarchar](1024);
	DECLARE @Msg [nvarchar](4000);

	-- Predicate Name is NOT NULL UNIQUE, so we can use it in the error message here.
	SELECT TOP(1) @PredicateName = [Name] FROM [Core].[Predicate] WHERE [Uri] = '';
	IF @PredicateName IS NOT NULL
	BEGIN
		SET @Msg = 'Predicates cannot have an empty Uri. Please provide a valid Uri value for predicate {' + @PredicateName + '} .';
		RAISERROR (@Msg, 16, 1);
		ROLLBACK TRANSACTION;
	END
	
	-- Check for duplicate Uri.
	SELECT TOP(1) @DuplicateUri = [Uri] FROM [Core].[Predicate]
	GROUP BY [Uri]
	HAVING COUNT(1) > 1;

	IF @DuplicateUri IS NOT NULL
	BEGIN
		SET @Msg = 'Duplicate predicate Uri value {' + @DuplicateUri + '} detected. Make sure that the Uri values are unique across all predicates.';
		RAISERROR (@Msg, 16, 1)
		ROLLBACK TRANSACTION
	END
END


GO
PRINT N'Creating Core.AfterInsertOnResource...';


GO
CREATE TRIGGER [Core].[AfterInsertOnResource]
    ON [Core].[Resource]
    AFTER INSERT
    AS BEGIN
	SET NOCOUNT ON;
	
	INSERT INTO [Core].[Content] ([ResourceId], [Content], [FileExtension])
	SELECT [I].[Id], 0x, [FileExtension] FROM [inserted] [I]
	WHERE [I].[ResourceTypeId] IN 
		(SELECT [Id] FROM [Core].[GetDerivedTypes]('94c567f8-b3eb-4ccb-9bf1-fc88970f78d7'))
END


GO
PRINT N'Creating Core.AfterUpdateOnResource...';


GO
CREATE TRIGGER [Core].[AfterUpdateOnResource]
    ON [Core].[Resource]
    AFTER UPDATE
    AS BEGIN
	SET NOCOUNT ON;
	
	UPDATE [Core].[Content] 
	SET [FileExtension] = [I].[FileExtension]
	FROM [Core].[Content] [C] INNER JOIN [inserted] [I]
	ON [C].[ResourceId] = [I].[Id]
	AND [I].[ResourceTypeId] IN 
		(SELECT [Id] FROM [Core].[GetDerivedTypes]('94c567f8-b3eb-4ccb-9bf1-fc88970f78d7'))

END


GO
PRINT N'Creating Core.AfterInsertUpdateOnScalarProperty...';


GO
CREATE TRIGGER [Core].[AfterInsertUpdateOnScalarProperty]
    ON [Core].[ScalarProperty]
    AFTER INSERT, UPDATE
    AS BEGIN
	EXEC [Core].[ValidateInsertsIntoResourceTypeProperty]
END


GO
PRINT N'Creating Core.DropUniqueIndexesFromMetadata...';


GO
CREATE PROCEDURE [Core].[DropUniqueIndexesFromMetadata]

AS
BEGIN
	IF  EXISTS (SELECT 1 FROM [sys].[indexes] 
		WHERE object_id = OBJECT_ID(N'[Core].[DataModelModule]') 
		AND [name] = N'UQ_DataModelModule')
	DROP INDEX [UQ_DataModelModule] ON [Core].[DataModelModule];

	IF  EXISTS (SELECT 1 FROM [sys].[indexes] 
		WHERE object_id = OBJECT_ID(N'[Core].[ResourceType]') 
		AND [name] = N'UQ_ResourceType')
	DROP INDEX [UQ_ResourceType] ON [Core].[ResourceType]

	IF  EXISTS (SELECT 1 FROM [sys].[indexes] 
		WHERE object_id = OBJECT_ID(N'[Core].[ResourceType]') 
		AND [name] = N'UQ_Discriminator')
	DROP INDEX [UQ_Discriminator] ON [Core].[ResourceType]

	IF  EXISTS (SELECT 1 FROM [sys].[indexes] 
		WHERE object_id = OBJECT_ID(N'[Core].[Association]') 
		AND [name] = N'UQ_Association_PredicateId')
	DROP INDEX [UQ_Association_PredicateId] ON [Core].[Association]

	IF  EXISTS (SELECT 1 FROM [sys].[indexes] 
		WHERE object_id = OBJECT_ID(N'[Core].[Association]') 
		AND [name] = N'UQ_Association_Name')
	DROP INDEX [UQ_Association_Name] ON [Core].[Association]

	IF  EXISTS (SELECT 1 FROM [sys].[indexes] 
		WHERE object_id = OBJECT_ID(N'[Core].[ScalarProperty]') 
		AND [name] = N'UQ_ScalarProperty')
	DROP INDEX [UQ_ScalarProperty] ON [Core].[ScalarProperty]

	IF  EXISTS (SELECT 1 FROM [sys].[indexes] 
		WHERE object_id = OBJECT_ID(N'[Core].[NavigationProperty]') 
		AND [name] = N'UQ_NavigationProperty')
	DROP INDEX [UQ_NavigationProperty] ON [Core].[NavigationProperty]
	
	IF  EXISTS (SELECT 1 FROM [sys].[triggers] 
		WHERE object_id = OBJECT_ID(N'[Core].[AfterInsertUpdateOnScalarProperty]'))
	DROP TRIGGER [Core].[AfterInsertUpdateOnScalarProperty]
	
	IF  EXISTS (SELECT 1 FROM [sys].[triggers] 
		WHERE object_id = OBJECT_ID(N'[Core].[AfterInsertUpdateOnNavigationProperty]'))
	DROP TRIGGER [Core].[AfterInsertUpdateOnNavigationProperty]

	IF  EXISTS (SELECT 1 FROM [sys].[indexes] 
		WHERE object_id = OBJECT_ID(N'[Core].[Predicate]') 
		AND name = N'UQ_PredicateName')
	DROP INDEX [UQ_PredicateName] ON [Core].[Predicate]

END


GO

GO

INSERT INTO [Core].[Configuration](ConfigName, ConfigValue) VALUES('ZentityVersion', '2.0.100.500');
INSERT INTO [Core].[Configuration](ConfigName, ConfigValue) VALUES('IsChangeHistoryEnabled', 'False');
INSERT INTO [Core].[Configuration](ConfigName, ConfigValue) VALUES('IsFullTextSearchEnabled', 'False');
INSERT INTO [Core].[Configuration](ConfigName, ConfigValue) VALUES('MaxDiscriminator', 2);
GO
INSERT INTO [Core].[DataModelModule] ([Id], [Namespace], [Uri], [Description],[IsMsShipped]) VALUES ('6b9cdecb-3152-46d0-becb-a23ecc71109f', 'Zentity.Core', 'urn:zentity/module/zentity-core', 'The core data model.', 1);
GO
-- Explicit insert values 1 for Resource and 2 for File. Some of the application 
-- logic for example, the edmx, will fail if these values are different.
INSERT INTO [Core].[ResourceType] ([Id], [DataModelModuleId], [BaseTypeId], [Name], [Uri], [Description], [Discriminator]) VALUES ('d2bd64df-6609-4ea4-ae99-9669da69bf7a', '6b9cdecb-3152-46d0-becb-a23ecc71109f', NULL, 'Resource', 'urn:zentity/module/zentity-core/resource-type/resource', 'The ultimate base resource type for all resource types.', 1);
INSERT INTO [Core].[ResourceType] ([Id], [DataModelModuleId], [BaseTypeId], [Name], [Uri], [Description], [Discriminator]) VALUES ('94c567f8-b3eb-4ccb-9bf1-fc88970f78d7', '6b9cdecb-3152-46d0-becb-a23ecc71109f', 'd2bd64df-6609-4ea4-ae99-9669da69bf7a', 'File', 'urn:zentity/module/zentity-core/resource-type/file', 'Represents a binary file.', 2);
GO
INSERT INTO [Core].[ScalarProperty] ([Id], [ResourceTypeId], [Name], [Uri], [Description], [DataType], [Nullable], [MaxLength], [Scale], [Precision], [TableName], [ColumnName]) VALUES ('9664A465-080D-4CFE-B5D2-450AC24CDE03', 'd2bd64df-6609-4ea4-ae99-9669da69bf7a', 'Id', 'urn:zentity/module/zentity-core/resource-type/resource/property/id', 'Gets the Id value that uniquely identifies the Resource.', 'Guid', 0, NULL, NULL, NULL, 'Resource', 'Id');
INSERT INTO [Core].[ScalarProperty] ([Id], [ResourceTypeId], [Name], [Uri], [Description], [DataType], [Nullable], [MaxLength], [Scale], [Precision], [TableName], [ColumnName]) VALUES ('A8076A7F-DC0F-4B28-AB17-492742813A04', 'd2bd64df-6609-4ea4-ae99-9669da69bf7a', 'DateAdded', 'urn:zentity/module/zentity-core/resource-type/resource/property/date-added', 'Gets the date on which the Resource was added in the repository.', 'DateTime', 1, NULL, NULL, NULL, 'Resource', 'DateAdded');
INSERT INTO [Core].[ScalarProperty] ([Id], [ResourceTypeId], [Name], [Uri], [Description], [DataType], [Nullable], [MaxLength], [Scale], [Precision], [TableName], [ColumnName]) VALUES ('4996CB49-15B3-44ED-B46D-0CE2132DA900', 'd2bd64df-6609-4ea4-ae99-9669da69bf7a', 'DateModified', 'urn:zentity/module/zentity-core/resource-type/resource/property/date-modified', 'Gets the date on which the Resource was last modified.', 'DateTime', 1, NULL, NULL, NULL, 'Resource', 'DateModified');
INSERT INTO [Core].[ScalarProperty] ([Id], [ResourceTypeId], [Name], [Uri], [Description], [DataType], [Nullable], [MaxLength], [Scale], [Precision], [TableName], [ColumnName]) VALUES ('0E671214-CFD7-43C9-8267-8F87D12B6687', 'd2bd64df-6609-4ea4-ae99-9669da69bf7a', 'Description', 'urn:zentity/module/zentity-core/resource-type/resource/property/description', 'Gets or sets the description of the Resource.', 'String', 1, -1, NULL, NULL, 'Resource', 'Description');
INSERT INTO [Core].[ScalarProperty] ([Id], [ResourceTypeId], [Name], [Uri], [Description], [DataType], [Nullable], [MaxLength], [Scale], [Precision], [TableName], [ColumnName]) VALUES ('2F2D690A-7FE1-4A16-BA78-0AD6E49ABF9F', 'd2bd64df-6609-4ea4-ae99-9669da69bf7a', 'Title', 'urn:zentity/module/zentity-core/resource-type/resource/property/title', 'Gets or sets the title of the Resource.', 'String', 1, 425, NULL, NULL, 'Resource', 'Title');
INSERT INTO [Core].[ScalarProperty] ([Id], [ResourceTypeId], [Name], [Uri], [Description], [DataType], [Nullable], [MaxLength], [Scale], [Precision], [TableName], [ColumnName]) VALUES ('EEE5DD55-79AF-4480-B51A-5A7FC360C6C3', 'd2bd64df-6609-4ea4-ae99-9669da69bf7a', 'Uri', 'urn:zentity/module/zentity-core/resource-type/resource/property/uri', 'Gets or sets the URI of the Resource.', 'String', 1, 1024, NULL, NULL, 'Resource', 'Uri');

INSERT INTO [Core].[ScalarProperty] ([Id], [ResourceTypeId], [Name], [Uri], [Description], [DataType], [Nullable], [MaxLength], [Scale], [Precision], [TableName], [ColumnName]) VALUES ('03E621E3-4602-4D29-8F7E-6C00178D95FB', '94c567f8-b3eb-4ccb-9bf1-fc88970f78d7', 'Checksum', 'urn:zentity/module/zentity-core/resource-type/file/property/checksum', 'Gets or sets the hash value of file data.', 'String', 1, 256, NULL, NULL, 'Resource', 'Checksum');
INSERT INTO [Core].[ScalarProperty] ([Id], [ResourceTypeId], [Name], [Uri], [Description], [DataType], [Nullable], [MaxLength], [Scale], [Precision], [TableName], [ColumnName]) VALUES ('720D7515-4FE4-4EFB-AA8A-89558B7FD9D6', '94c567f8-b3eb-4ccb-9bf1-fc88970f78d7', 'FileExtension', 'urn:zentity/module/zentity-core/resource-type/file/property/file-extension', 'Gets or sets the extension of uploaded content file.', 'String', 1, 128, NULL, NULL, 'Resource', 'FileExtension');
INSERT INTO [Core].[ScalarProperty] ([Id], [ResourceTypeId], [Name], [Uri], [Description], [DataType], [Nullable], [MaxLength], [Scale], [Precision], [TableName], [ColumnName]) VALUES ('9FFD4730-1007-46FC-B538-EBE27FBAA992', '94c567f8-b3eb-4ccb-9bf1-fc88970f78d7', 'MimeType', 'urn:zentity/module/zentity-core/resource-type/file/property/mime-type', 'Gets or sets the MIME type of data.', 'String', 1, 128, NULL, NULL, 'Resource', 'MimeType');
INSERT INTO [Core].[ScalarProperty] ([Id], [ResourceTypeId], [Name], [Uri], [Description], [DataType], [Nullable], [MaxLength], [Scale], [Precision], [TableName], [ColumnName]) VALUES ('ABFBEEC4-2A46-4A3F-8B48-6F50EEC10A9F', '94c567f8-b3eb-4ccb-9bf1-fc88970f78d7', 'Size', 'urn:zentity/module/zentity-core/resource-type/file/property/size', 'Gets or sets the size of data file.', 'Int64', 1, NULL, NULL, NULL, 'Resource', 'Size');
GO
INSERT INTO [Core].[NavigationProperty]([Id], [ResourceTypeId], [Name], [Uri], [Description]) VALUES ('054f2a48-2fce-420f-8763-eab97a01bc56', 'd2bd64df-6609-4ea4-ae99-9669da69bf7a', 'Files', 'urn:zentity/module/zentity-core/resource-type/resource/navigation-property/files', 'Gets the associated files with this resource.');
INSERT INTO [Core].[NavigationProperty]([Id], [ResourceTypeId], [Name], [Uri], [Description]) VALUES ('8493213f-1c82-40cb-99d0-d9a0c092c416', '94c567f8-b3eb-4ccb-9bf1-fc88970f78d7', 'Resources', 'urn:zentity/module/zentity-core/resource-type/file/navigation-property/resources', 'Gets the associated resources for this file.');

GO
INSERT INTO [Core].[Predicate] ([Id], [Name], [Uri]) VALUES ('818a93f5-25a9-4149-a8d2-19104a352da0', 'ResourceHasFile', 'urn:zentity/module/zentity-core/predicate/resource-has-file')
INSERT INTO [Core].[Association]([Id], [Name], [Uri], [SubjectNavigationPropertyId], [ObjectNavigationPropertyId], [PredicateId], [SubjectMultiplicity], [ObjectMultiplicity], [ViewName]) VALUES ('10538dc2-1cac-4755-9a34-2938201eb06e', 'ResourceHasFile', 'urn:zentity/module/zentity-core/association/resource-has-file', '054f2a48-2fce-420f-8763-eab97a01bc56', '8493213f-1c82-40cb-99d0-d9a0c092c416', '818a93f5-25a9-4149-a8d2-19104a352da0', 'Many', 'Many', 'ResourceHasFile');
-- To allow advanced options to be changed.
EXEC sp_configure 'show advanced options', 1
GO
-- To update the currently configured value for advanced options.
RECONFIGURE
GO
-- To enable the feature.
EXEC sp_configure 'xp_cmdshell', 1
GO
-- To update the currently configured value for this feature.
RECONFIGURE
GO
------------------------
-- Change History
------------------------
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"ChangeHistory.FileGroup",
"
	ALTER DATABASE [#(DatabaseName)]
	ADD FILEGROUP [ChangeHistory];
"
)

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"ChangeHistory.File",
"
	ALTER DATABASE [#(DatabaseName)]
	ADD FILE 
	(
		NAME = [#(ChangeHistory)],
		FILENAME = N'#(ChangeHistoryFilePath)',
		MAXSIZE = UNLIMITED
	) TO FILEGROUP [ChangeHistory];
"
)

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.PropertyChangeQueryForCaptureInstance",
"
CREATE TABLE [Administration].[PropertyChangeQueryForCaptureInstance]
(
	[CaptureInstance] [nvarchar](256) NOT NULL,
	[ResourceTypeId] [uniqueidentifier] NOT NULL,
	[PropertyChangesQuery] [nvarchar](max) NOT NULL
	CONSTRAINT [PK_PropertyChangeQueryForCaptureInstance] PRIMARY KEY CLUSTERED 
	(
		[CaptureInstance],
		[ResourceTypeId]
	)	
) ON [ChangeHistory]
"
)

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	Creates template queries for property changes per capture instance.
 Parameters: 
	@CaptureInstance	- Capture instance name.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.CreatePropertyChangeQueryForCaptureInstance",
"
CREATE PROCEDURE [Administration].[CreatePropertyChangeQueryForCaptureInstance]
	@CaptureInstance [nvarchar](256)
AS
BEGIN
	DECLARE ResourceTypeCursor CURSOR 
	FOR SELECT [RT].[Id] [ResourceTypeId], 
	[DMM].[Namespace] + '.' + [RT].[Name] [ResourceTypeFullName]
	FROM [Core].[ResourceType] [RT]
	INNER JOIN [Core].[DataModelModule] [DMM]
	ON [RT].[DataModelModuleId] = [DMM].[Id]

	DECLARE @ResourceTypeId [uniqueidentifier];
	DECLARE @ResourceTypeFullName [nvarchar](512);

	OPEN ResourceTypeCursor;
	FETCH NEXT FROM ResourceTypeCursor INTO @ResourceTypeId, @ResourceTypeFullName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE @InsertQuery [nvarchar](max);
		DECLARE @DeleteQuery [nvarchar](max);
		DECLARE @UpdateQuery [nvarchar](max);
		DECLARE @FinalQuery [nvarchar](max);

		SELECT @InsertQuery = N'
			SELECT NEWID()[Id], [__$start_lsn] [ChangeSetId],
			[__$seqval] [SequenceNumber],
			2 [Operation], [__$update_mask] [UpdateMask],
			[Id] [ResourceId],';
		SELECT @DeleteQuery = N'
			SELECT NEWID()[Id], [__$start_lsn] [ChangeSetId],
			[__$seqval] [SequenceNumber],
			1 [Operation], [__$update_mask] [UpdateMask],
			[Id] [ResourceId],';
		SELECT @UpdateQuery = N'
			SELECT NEWID()[Id], [Previous].[__$start_lsn] [ChangeSetId],
			[Previous].[__$seqval] [SequenceNumber],
			3 [Operation], [Previous].[__$update_mask] [UpdateMask],
			[Previous].[Id] [ResourceId],';
		SELECT @FinalQuery = N'SELECT [Id], LOWER(master.dbo.fn_varbintohexstr([ChangeSetId])), 
		LOWER(master.dbo.fn_varbintohexstr([SequenceNumber])), [Operation], [ResourceId], 
		''' + CAST(@ResourceTypeId AS nvarchar(64)) + ''' [ResourceTypeId], 
		N''' + @ResourceTypeFullName + ''' [ResourceTypeFullName], 
		N''<PropertyChanges ResourceId=""'' + CAST([ResourceId] AS [nvarchar](128)) + 
		N''"" ResourceTypeFullName=""' + @ResourceTypeFullName + '"">'' + 
		' ;

		-- Iterate through the scalar properties and prepare the sub-queries.
		SELECT 
			@InsertQuery = CASE WHEN [SP].[Name] = 'Id' THEN @InsertQuery 
			ELSE @InsertQuery + N' NULL [Previous' + Core.EscapeBrackets([SP].[Name]) + N'], [' + 
				Core.EscapeBrackets([ColumnName]) + N'] [Next' + 
				Core.EscapeBrackets([SP].[Name]) + N'] ,' END,
			@DeleteQuery = CASE WHEN [SP].[Name] = 'Id' THEN @DeleteQuery 
			ELSE @DeleteQuery + N' [' + Core.EscapeBrackets([ColumnName]) + N'] [Previous' + 
				Core.EscapeBrackets([SP].[Name]) + N'], NULL [Next' + 
				Core.EscapeBrackets([SP].[Name]) + N'] ,' END,
			@UpdateQuery = CASE WHEN [SP].[Name] = 'Id' THEN @UpdateQuery 
			ELSE @UpdateQuery + N' [Previous].[' + Core.EscapeBrackets([ColumnName]) + N'] [Previous' + 
				Core.EscapeBrackets([SP].[Name]) + N'], [Next].[' + Core.EscapeBrackets([ColumnName]) + 
				N'] [Next' + Core.EscapeBrackets([SP].[Name]) + N'],' END,
			@FinalQuery = CASE WHEN [SP].[Name] = 'Id' THEN @FinalQuery 
			ELSE @FinalQuery + 
				CASE WHEN [SP].[DataType] = N'Binary' 
					THEN N' [Administration].[GetEscapedBinaryPropertyChangeXml]('''
					ELSE N' [Administration].[GetEscapedPropertyChangeXml]('''
				END + 
				Core.EscapeQuotes([SP].[Name]) + N''', [Operation], [Previous' + 
				Core.EscapeBrackets([SP].[Name]) + N'], [Next' + Core.EscapeBrackets([SP].[Name]) + 
				'], ''' + Core.EscapeQuotes(@CaptureInstance) + N''', ''' + 
				Core.EscapeQuotes([SP].[ColumnName]) + N''', [UpdateMask]) + ' 
			END
		FROM [Core].[ScalarProperty] [SP]
		WHERE [SP].[ResourceTypeId] 
			IN (SELECT [Id] FROM [Core].[GetClassHierarchy](@ResourceTypeId))
		
		-- Strip off last commas.
		SELECT @InsertQuery = SUBSTRING(@InsertQuery, 1, LEN(@InsertQuery)-1) + 
		N' FROM [cdc].[' + @CaptureInstance + N'_CT] 
		WHERE  [__$operation] = 2 AND [__$start_lsn] = @VarChangeSetId
		AND [Id] = @VarResourceId';
		SELECT @DeleteQuery = SUBSTRING(@DeleteQuery, 1, LEN(@DeleteQuery)-1) + 
		N' FROM [cdc].[' + @CaptureInstance + N'_CT] 
		WHERE  [__$operation] = 1 
		AND [__$start_lsn] = @VarChangeSetId
		AND [Id] = @VarResourceId';
		SELECT @UpdateQuery = SUBSTRING(@UpdateQuery, 1, LEN(@UpdateQuery)-1) + 
		N' FROM  [cdc].[' + @CaptureInstance + N'_CT]  [Previous] 
		INNER JOIN [cdc].[' + @CaptureInstance + N'_CT] [Next]
		ON  [Previous].[__$seqval] = [Next].[__$seqval] 
		AND [Previous].[__$operation] = 3 
		AND [Next].[__$operation] = 4 
		AND [Previous].[__$start_lsn] = [Next].[__$start_lsn] 
		AND [Previous].[Id] = [Next].[Id] 
		WHERE [Previous].[__$start_lsn] = @VarChangeSetId
		AND [Previous].[Id] = @VarResourceId';

		DECLARE @CommonTable [nvarchar](max);
		SELECT @CommonTable = @InsertQuery;
		SELECT @CommonTable = @CommonTable + N' 
		UNION ALL ';
		SELECT @CommonTable = @CommonTable + @DeleteQuery;
		SELECT @CommonTable = @CommonTable + N' 
		UNION ALL ';
		SELECT @CommonTable = @CommonTable + @UpdateQuery;

		SELECT @FinalQuery = @FinalQuery + ' 
		N''</PropertyChanges>'' [PropertyChanges] 
		FROM (' + @CommonTable + '
		)[T]' ;

		INSERT INTO [Administration].[PropertyChangeQueryForCaptureInstance]
		([CaptureInstance], [ResourceTypeId], [PropertyChangesQuery])
		VALUES(@CaptureInstance, @ResourceTypeId, @FinalQuery);		

		FETCH NEXT FROM ResourceTypeCursor INTO @ResourceTypeId, @ResourceTypeFullName;
	END

	CLOSE ResourceTypeCursor;
	DEALLOCATE ResourceTypeCursor;
END
");

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	This procedure creates capture instances on Core.Resource table if there is none or if 
	Core.Resource has undergone a schema change.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.UpdateCaptureInstances",
"
CREATE PROCEDURE [Administration].[UpdateCaptureInstances]
AS
BEGIN

	DECLARE @ResourceTableId int;
	DECLARE @EarlierCaptureInstanceId int;
	DECLARE @NewCaptureInstanceName nvarchar(256);
	
	-- Initialize.
	SELECT @ResourceTableId = OBJECT_ID('Core.Resource');

	SELECT TOP(1) @EarlierCaptureInstanceId = [object_id]
	FROM [cdc].[change_tables]
	WHERE [source_object_id] = @ResourceTableId
	ORDER BY [create_date] DESC;

	-- Create a unique capture instance name.
	SELECT @NewCaptureInstanceName = N'Core_Resource_' + REPLACE(NEWID(), N'-', N'');

	IF @EarlierCaptureInstanceId IS NULL
	-- Change history not enabled on this table.
	BEGIN
		RAISERROR(N'Enabling CDC on Core.Resource.', 10, 1);

		EXEC sys.sp_cdc_enable_table 
		@source_schema = N'Core'
		,@source_name = N'Resource'
		,@role_name = N'cdcAdmin'
		,@capture_instance = @NewCaptureInstanceName
		,@filegroup_name = N'ChangeHistory' 
		,@supports_net_changes = 1;

		-- Create property change queries for this capture instance.
		EXEC [Administration].[CreatePropertyChangeQueryForCaptureInstance] 
			@NewCaptureInstanceName;
	END -- End of if-change-history-not-enabled.
	ELSE
	-- Change history already enabled on this table.
	BEGIN
		-- Compare all columns of Resource table and the capture instance. If there are differences 
		-- in number, datatype, max length, scale or precision of the columns, create a new capture 
		-- instance and drop all the capture instances on this source object except the latest one.
		-- NOTE: We were observing transaction deadlocks on metadata, so including the NOLOCK hint 
		-- here.
		IF EXISTS
		(
			(
				SELECT [name], [user_type_id], [max_length], [precision], [scale]
				FROM [sys].[columns] WITH(NOLOCK) WHERE [object_id] = @ResourceTableId 
				EXCEPT
				SELECT [name], [user_type_id], [max_length], [precision], [scale]
				FROM [sys].[columns] WITH(NOLOCK) 
				WHERE [object_id] = @EarlierCaptureInstanceId AND [column_id] > 5 
			)
			UNION
			(
				SELECT [name], [user_type_id], [max_length], [precision], [scale]
				FROM [sys].[columns] WITH(NOLOCK) 
				WHERE [object_id] = @EarlierCaptureInstanceId AND [column_id] > 5 
				EXCEPT
				SELECT [name], [user_type_id], [max_length], [precision], [scale]
				FROM [sys].[columns] WITH(NOLOCK) WHERE [object_id] = @ResourceTableId
			)
		)
		BEGIN
			-- Delete all capture instances before the latest instance. Since there are only two 
			-- capture instances allowed on a source table, we need not have any loops here.
			DECLARE @InstanceName nvarchar(256);
			SELECT TOP(1) @InstanceName = [capture_instance] 
			FROM [cdc].[change_tables] WITH(NOLOCK) 
			WHERE [source_object_id] = @ResourceTableId
			AND [object_id] <> @EarlierCaptureInstanceId

			IF(@InstanceName IS NOT NULL)
			BEGIN
				DECLARE @Msg [nvarchar](4000);
				SET @Msg = N'Removing capture instance ' + @InstanceName + N'.';
				RAISERROR(@Msg, 10, 1);

				EXEC sys.sp_cdc_disable_table 
				@source_schema = N'Core' 
				,@source_name = 'Resource'
				,@capture_instance = @InstanceName;
			END
			
			SET @Msg = N'Creating capture instance ' + @NewCaptureInstanceName + N'.';
			RAISERROR(@Msg, 10, 1);

			EXEC sys.sp_cdc_enable_table 
			@source_schema = N'Core'
			,@source_name = 'Resource'
			,@role_name = N'cdcAdmin'
			,@capture_instance = @NewCaptureInstanceName
			,@filegroup_name = N'ChangeHistory' 
			,@supports_net_changes = 1;
				
			-- Create property change queries for this capture instance.
			EXEC [Administration].[CreatePropertyChangeQueryForCaptureInstance] 
				@NewCaptureInstanceName;
		END -- End of if-source-object-not-equals-capture-instance.
	END -- End of if-change-history-enabled-on-table.
END
");

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"CreateCaptureInstances",
"
IF NOT EXISTS(SELECT 1 FROM sys.databases 
	WHERE [database_id] = DB_ID() AND [is_cdc_enabled] = 1)
BEGIN
	RAISERROR('Enabling CDC on the database.', 10, 1);
	EXEC sys.sp_cdc_enable_db;
END

IF NOT EXISTS(SELECT 1 FROM sys.tables [T] 
	INNER JOIN sys.schemas [S] ON [T].schema_id = [S].schema_id 
	WHERE [T].[name]  = 
		N'Predicate'  AND 
		[S].[name]  = 
		N'Core'  AND [is_tracked_by_cdc] = 1)
BEGIN
	RAISERROR(N'Enabling CDC on Core.Predicate.', 10, 1);
	EXEC sys.sp_cdc_enable_table 
		@source_schema = 'Core'
		,@source_name = 'Predicate'
		,@role_name = 'cdcAdmin'
		,@filegroup_name = N'ChangeHistory' 
		,@supports_net_changes = 1;
END

IF NOT EXISTS(SELECT 1 FROM sys.tables [T] 
	INNER JOIN sys.schemas [S] ON [T].schema_id = [S].schema_id 
	WHERE [T].[name]  = 
		N'Property'  AND 
		[S].[name]  = 
		N'Core'  AND [is_tracked_by_cdc] = 1)
BEGIN
	RAISERROR(N'Enabling CDC on Core.Property.', 10, 1);
	EXEC sys.sp_cdc_enable_table 
		@source_schema = 'Core'
		,@source_name = 'Property'
		,@role_name = 'cdcAdmin'
		,@filegroup_name = N'ChangeHistory' 
		,@supports_net_changes = 1;
END

IF NOT EXISTS(SELECT 1 FROM sys.tables [T] 
	INNER JOIN sys.schemas [S] ON [T].schema_id = [S].schema_id 
	WHERE [T].[name]  = 
		N'Relationship'  AND 
		[S].[name]  = 
		N'Core'  AND [is_tracked_by_cdc] = 1)
BEGIN
	RAISERROR(N'Enabling CDC on Core.Relationship.', 10, 1);
	EXEC sys.sp_cdc_enable_table 
		@source_schema = 'Core'
		,@source_name = 'Relationship'
		,@role_name = 'cdcAdmin'
		,@filegroup_name = N'ChangeHistory' 
		,@supports_net_changes = 1;
END

IF NOT EXISTS(SELECT 1 FROM sys.tables [T] 
	INNER JOIN sys.schemas [S] ON [T].schema_id = [S].schema_id 
	WHERE [T].[name]  = 
		N'PredicateProperty'  AND 
		[S].[name]  = 
		N'Core'  AND [is_tracked_by_cdc] = 1)
BEGIN
	RAISERROR(N'Enabling CDC on Core.PredicateProperty.', 10, 1);
	EXEC sys.sp_cdc_enable_table 
		@source_schema = 'Core'
		,@source_name = 'PredicateProperty'
		,@role_name = 'cdcAdmin'
		,@filegroup_name = N'ChangeHistory' 
		,@supports_net_changes = 1; 
END

IF NOT EXISTS(SELECT 1 FROM sys.tables [T] 
	INNER JOIN sys.schemas [S] ON [T].schema_id = [S].schema_id 
	WHERE [T].[name]  = 
		N'RelationshipProperty'  AND 
		[S].[name]  = 
		N'Core'  AND [is_tracked_by_cdc] = 1)
BEGIN
	RAISERROR(N'Enabling CDC on Core.RelationshipProperty.', 10, 1);
	EXEC sys.sp_cdc_enable_table 
		@source_schema = 'Core'
		,@source_name = 'RelationshipProperty'
		,@role_name = 'cdcAdmin'
		,@filegroup_name = N'ChangeHistory' 
		,@supports_net_changes = 1; 
END

EXEC [Administration].[UpdateCaptureInstances];

IF NOT EXISTS(SELECT 1 FROM sys.tables [T] 
	INNER JOIN sys.schemas [S] ON [T].schema_id = [S].schema_id 
	WHERE [T].[name]  = 
		N'ResourceProperty'  AND 
		[S].[name]  = 
		N'Core'  AND [is_tracked_by_cdc] = 1)
BEGIN
	RAISERROR(N'Enabling CDC on Core.ResourceProperty.', 10, 1);
	EXEC sys.sp_cdc_enable_table 
		@source_schema = 'Core'
		,@source_name = 'ResourceProperty'
		,@role_name = 'cdcAdmin'
		,@filegroup_name = N'ChangeHistory' 
		,@supports_net_changes = 1;
END

IF NOT EXISTS(SELECT 1 FROM sys.tables [T] 
	INNER JOIN sys.schemas [S] ON [T].schema_id = [S].schema_id 
	WHERE [T].[name]  = 
		N'DataModelModule'  AND [S].[name]  = N'Core'  AND [is_tracked_by_cdc] = 1)
BEGIN
	RAISERROR(N'Enabling CDC on Core.DataModelModule.', 10, 1);
	EXEC sys.sp_cdc_enable_table 
		@source_schema = 'Core'
		,@source_name = 'DataModelModule'
		,@role_name = 'cdcAdmin'
		,@filegroup_name = N'ChangeHistory' 
		,@supports_net_changes = 1;
END

IF NOT EXISTS(SELECT 1 FROM sys.tables [T] 
	INNER JOIN sys.schemas [S] ON [T].schema_id = [S].schema_id 
	WHERE [T].[name]  = 
		N'Association'  AND [S].[name]  = N'Core'  AND [is_tracked_by_cdc] = 1)
BEGIN
	RAISERROR(N'Enabling CDC on Core.Association.', 10, 1);
	EXEC sys.sp_cdc_enable_table 
		@source_schema = 'Core'
		,@source_name = 'Association'
		,@role_name = 'cdcAdmin'
		,@filegroup_name = N'ChangeHistory' 
		,@supports_net_changes = 1;
END

IF NOT EXISTS(SELECT 1 FROM sys.tables [T] 
	INNER JOIN sys.schemas [S] ON [T].schema_id = [S].schema_id 
	WHERE [T].[name]  = 
		N'ResourceType'  AND [S].[name]  = N'Core'  AND [is_tracked_by_cdc] = 1)
BEGIN
	RAISERROR(N'Enabling CDC on Core.ResourceType.', 10, 1);
	EXEC sys.sp_cdc_enable_table 
		@source_schema = 'Core'
		,@source_name = 'ResourceType'
		,@role_name = 'cdcAdmin'
		,@filegroup_name = N'ChangeHistory' 
		,@supports_net_changes = 1;
END

IF NOT EXISTS(SELECT 1 FROM sys.tables [T] 
	INNER JOIN sys.schemas [S] ON [T].schema_id = [S].schema_id 
	WHERE [T].[name]  = 
		N'ScalarProperty'  AND 
		[S].[name]  = 
		N'Core'  AND [is_tracked_by_cdc] = 1)
BEGIN
	RAISERROR(N'Enabling CDC on Core.ScalarProperty.', 10, 1);
	EXEC sys.sp_cdc_enable_table 
		@source_schema = 'Core'
		,@source_name = 'ScalarProperty'
		,@role_name = 'cdcAdmin'
		,@filegroup_name = N'ChangeHistory' 
		,@supports_net_changes = 1;
END

IF NOT EXISTS(SELECT 1 FROM sys.tables [T] 
	INNER JOIN sys.schemas [S] ON [T].schema_id = [S].schema_id 
	WHERE [T].[name]  = 
		N'NavigationProperty'  AND 
		[S].[name]  = 
		N'Core'  AND [is_tracked_by_cdc] = 1)
BEGIN
	RAISERROR(N'Enabling CDC on Core.NavigationProperty.', 10, 1);
	EXEC sys.sp_cdc_enable_table 
		@source_schema = 'Core'
		,@source_name = 'NavigationProperty'
		,@role_name = 'cdcAdmin'
		,@filegroup_name = N'ChangeHistory' 
		,@supports_net_changes = 1;
END
");

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ChangeSet",
"
CREATE TABLE [Administration].[ChangeSet](
	[Id] [nvarchar](64) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	CONSTRAINT [PK_ChangeSet] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
) ON [ChangeHistory]
"
)

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.Operation",
"
CREATE TABLE [Administration].[Operation](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Description] [nvarchar](4000) NULL,
	CONSTRAINT [PK_Operation] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
) ON [ChangeHistory]
"
)

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.PredicateChange",
"
CREATE TABLE [Administration].[PredicateChange](
	[Id] [uniqueidentifier] NOT NULL,
	[ChangeSetId] [nvarchar](64) NOT NULL,
	[SequenceNumber] [nvarchar](64) NOT NULL,
	[OperationId] [int] NOT NULL,
	[PredicateId] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[PreviousName] [nvarchar](128) NULL,
	[NextName] [nvarchar](128) NULL,
	[PreviousUri] [nvarchar](1024) NULL,
	[NextUri] [nvarchar](1024) NULL,
	CONSTRAINT [PK_PredicateChange] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	),
	CONSTRAINT [UQ_PredicateChange] UNIQUE NONCLUSTERED 
	(
		[ChangeSetId] ASC,
		[SequenceNumber] ASC,
		[OperationId] ASC
	),
	/*PredicateChangeBelongsToChangeSet*/
	CONSTRAINT [PredicateChangeBelongsToChangeSet] FOREIGN KEY ([ChangeSetId])
	REFERENCES [Administration].[ChangeSet] ([Id]),
	/*PredicateChangeIsCausedByOperation*/
	CONSTRAINT [PredicateChangeIsCausedByOperation] FOREIGN KEY([OperationId])
	REFERENCES [Administration].[Operation] ([Id])
) ON [ChangeHistory]
"
)

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.PropertyChange",
"
CREATE TABLE [Administration].[PropertyChange](
	[Id] [uniqueidentifier] NOT NULL,
	[ChangeSetId] [nvarchar](64) NOT NULL,
	[SequenceNumber] [nvarchar](64) NOT NULL,
	[OperationId] [int] NOT NULL,
	[PropertyId] [uniqueidentifier] NOT NULL,
	[PreviousName] [nvarchar](50) NULL,
	[NextName] [nvarchar](50) NULL,
	[PreviousUri] [nvarchar](1024) NULL,
	[NextUri] [nvarchar](1024) NULL,
	CONSTRAINT [PK_PropertyChange] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	),
	CONSTRAINT [UQ_PropertyChange] UNIQUE NONCLUSTERED 
	(
		[ChangeSetId] ASC,
		[SequenceNumber] ASC,
		[OperationId] ASC
	),
	/*PropertyChangeBelongsToChangeSet*/
	CONSTRAINT [PropertyChangeBelongsToChangeSet] FOREIGN KEY ([ChangeSetId])
	REFERENCES [Administration].[ChangeSet] ([Id]),
	/*PropertyChangeIsCausedByOperation*/
	CONSTRAINT [PropertyChangeIsCausedByOperation] FOREIGN KEY([OperationId])
	REFERENCES [Administration].[Operation] ([Id])
) ON [ChangeHistory]
"
)

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.RelationshipChange",
"
CREATE TABLE [Administration].[RelationshipChange](
	[Id] [uniqueidentifier] NOT NULL,
	[ChangeSetId] [nvarchar](64) NOT NULL,
	[SequenceNumber] [nvarchar](64) NOT NULL,
	[OperationId] [int] NOT NULL,
	[RelationshipId] [uniqueidentifier] NOT NULL,
	[PreviousSubjectResourceId] [uniqueidentifier] NULL,
	[NextSubjectResourceId] [uniqueidentifier] NULL,
	[PreviousObjectResourceId] [uniqueidentifier] NULL,
	[NextObjectResourceId] [uniqueidentifier] NULL,
	[PreviousPredicateId] [uniqueidentifier] NULL,
	[NextPredicateId] [uniqueidentifier] NULL,
	[PreviousOrdinalPosition] [int] NULL,
	[NextOrdinalPosition] [int] NULL,
	[PreviousDateAdded] [datetime] NULL,
	[NextDateAdded] [datetime] NULL,
	CONSTRAINT [PK_RelationshipChange] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	),
	CONSTRAINT [UQ_RelationshipChange] UNIQUE NONCLUSTERED 
	(
		[ChangeSetId] ASC,
		[SequenceNumber] ASC,
		[OperationId] ASC
	),
	/*RelationshipChangeBelongsToChangeSet*/
	CONSTRAINT [RelationshipChangeBelongsToChangeSet] FOREIGN KEY ([ChangeSetId])
	REFERENCES [Administration].[ChangeSet] ([Id]),
	/*[RelationshipChangeIsCausedByOperation]*/
	CONSTRAINT [RelationshipChangeIsCausedByOperation] FOREIGN KEY([OperationId])
	REFERENCES [Administration].[Operation] ([Id])
) ON [ChangeHistory]
"
)

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.PredicatePropertyChange",
"
CREATE TABLE [Administration].[PredicatePropertyChange](
	[Id] [uniqueidentifier] NOT NULL,
	[ChangeSetId] [nvarchar](64) NOT NULL,
	[SequenceNumber] [nvarchar](64) NOT NULL,
	[OperationId] [int] NOT NULL,
	[PredicatePropertyId] [uniqueidentifier] NOT NULL,
	[PreviousPredicateId] [uniqueidentifier] NULL,
	[NextPredicateId] [uniqueidentifier] NULL,
	[PreviousPropertyId] [uniqueidentifier]  NULL,
	[NextPropertyId] [uniqueidentifier] NULL,
	[PreviousValue] [nvarchar](max) NULL,
	[NextValue] [nvarchar](max) NULL,
	CONSTRAINT [PK_PredicatePropertyChange] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	),
	CONSTRAINT [UQ_PredicatePropertyChange] UNIQUE NONCLUSTERED 
	(
		[ChangeSetId] ASC,
		[SequenceNumber] ASC,
		[OperationId] ASC
	),
	/*PredicatePropertyChangeBelongsToChangeset*/
	CONSTRAINT [PredicatePropertyChangeBelongsToChangeSet] FOREIGN KEY ([ChangeSetId])
	REFERENCES [Administration].[ChangeSet] ([Id]),
	/*PredicatePropertyChangeIsCausedByOperation*/
	CONSTRAINT [PredicatePropertyChangeIsCausedByOperation] FOREIGN KEY([OperationId])
	REFERENCES [Administration].[Operation] ([Id])
) ON [ChangeHistory]
"
)

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.RelationshipPropertyChange",
"
CREATE TABLE [Administration].[RelationshipPropertyChange](
	[Id] [uniqueidentifier] NOT NULL,
	[ChangeSetId] [nvarchar](64) NOT NULL,
	[SequenceNumber] [nvarchar](64) NOT NULL,
	[OperationId] [int] NOT NULL,
	[RelationshipPropertyId] [uniqueidentifier] NOT NULL,
	[PreviousTripletId] [uniqueidentifier] NULL,
	[NextTripletId] [uniqueidentifier] NULL,
	[PreviousPropertyId] [uniqueidentifier] NULL,
	[NextPropertyId] [uniqueidentifier] NULL,
	[PreviousValue] [nvarchar](max) NULL,
	[NextValue] [nvarchar](max) NULL,
	CONSTRAINT [PK_RelationshipPropertyChange] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	),
	CONSTRAINT [UQ_RelationshipPropertyChange] UNIQUE NONCLUSTERED 
	(
		[ChangeSetId] ASC,
		[SequenceNumber] ASC,
		[OperationId] ASC
	),
	/*[RelationshipPropertyChangeBelongsToChangeSet]*/
	CONSTRAINT [RelationshipPropertyChangeBelongsToChangeSet] FOREIGN KEY ([ChangeSetId])
	REFERENCES [Administration].[ChangeSet] ([Id]),
	/*[RelationshipPropertyChangeIsCausedByOperation]*/
	CONSTRAINT [RelationshipPropertyChangeIsCausedByOperation] FOREIGN KEY([OperationId])
	REFERENCES [Administration].[Operation] ([Id])
) ON [ChangeHistory]
"
)

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ResourceChange",
"
CREATE TABLE [Administration].[ResourceChange](
	[Id] [uniqueidentifier] NOT NULL,
	[ChangeSetId] [nvarchar](64) NOT NULL,
	[SequenceNumber] [nvarchar](64) NOT NULL,
	[OperationId] [int] NOT NULL,
	[ResourceId] [uniqueidentifier] NOT NULL,
	[ResourceTypeId] [uniqueidentifier] NOT NULL,
	[ResourceTypeFullName] [nvarchar](512) NOT NULL,
	[PropertyChanges] [nvarchar](max) NOT NULL,
	CONSTRAINT [PK_ResourceChange] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	),
	CONSTRAINT [UQ_ResourceChange] UNIQUE NONCLUSTERED 
	(
		[ChangeSetId] ASC,
		[SequenceNumber] ASC,
		[OperationId] ASC
	),
	/*[ResourceChangeBelongsToChangeSet]*/
	CONSTRAINT [ResourceChangeBelongsToChangeSet] FOREIGN KEY ([ChangeSetId])
	REFERENCES [Administration].[ChangeSet] ([Id]),
	/*[ResourceChangeIsCausedByOperation]*/
	CONSTRAINT [ResourceChangeIsCausedByOperation] FOREIGN KEY([OperationId])
	REFERENCES [Administration].[Operation] ([Id])
) ON [ChangeHistory]
"
)

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ResourcePropertyChange",
"
CREATE TABLE [Administration].[ResourcePropertyChange](
	[Id] [uniqueidentifier] NOT NULL,
	[ChangeSetId] [nvarchar](64) NOT NULL,
	[SequenceNumber] [nvarchar](64) NOT NULL,
	[OperationId] [int] NOT NULL,
	[ResourcePropertyId] [uniqueidentifier] NOT NULL,
	[PreviousResourceId] [uniqueidentifier] NULL,
	[NextResourceId] [uniqueidentifier] NULL,
	[PreviousPropertyId] [uniqueidentifier] NULL,
	[NextPropertyId] [uniqueidentifier] NULL,
	[PreviousValue] [nvarchar](max) NULL,
	[NextValue] [nvarchar](max) NULL,
	CONSTRAINT [PK_ResourcePropertyChange] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	),
	CONSTRAINT [UQ_ResourcePropertyChange] UNIQUE NONCLUSTERED 
	(
		[ChangeSetId] ASC,
		[SequenceNumber] ASC,
		[OperationId] ASC
	),
	/*[ResourcePropertyChangeBelongsToChangeSet]*/
	CONSTRAINT [ResourcePropertyChangeBelongsToChangeSet] FOREIGN KEY ([ChangeSetId])
	REFERENCES [Administration].[ChangeSet] ([Id]),
	/*[ResourcePropertyChangeIsCausedByOperation]*/
	CONSTRAINT [ResourcePropertyChangeIsCausedByOperation] FOREIGN KEY([OperationId])
	REFERENCES [Administration].[Operation] ([Id])
) ON [ChangeHistory]
"
)

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.DataModelModuleChange",
"
CREATE TABLE [Administration].[DataModelModuleChange](
	[Id] [uniqueidentifier] NOT NULL,
	[ChangeSetId] [nvarchar](64) NOT NULL,
	[SequenceNumber] [nvarchar](64) NOT NULL,
	[OperationId] [int] NOT NULL,
	[DataModelModuleId] [uniqueidentifier] NOT NULL,
	[PreviousNamespace] [nvarchar](150) NULL,
	[NextNamespace] [nvarchar](150) NULL,
	[PreviousUri] [nvarchar](1024) NULL,
	[NextUri] [nvarchar](1024) NULL,
	[PreviousDescription] [nvarchar](max) NULL,
	[NextDescription] [nvarchar](max) NULL,
	[IsMsShipped] [bit] NULL,
	CONSTRAINT [PK_DataModelModuleChange] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	),
	CONSTRAINT [UQ_DataModelModuleChange] UNIQUE NONCLUSTERED 
	(
		[ChangeSetId] ASC,
		[SequenceNumber] ASC,
		[OperationId] ASC
	),
	CONSTRAINT [DataModelModuleChangeBelongsToChangeSet] FOREIGN KEY([ChangeSetId])
	REFERENCES [Administration].[ChangeSet] ([Id]),
	CONSTRAINT [DataModelModuleChangeIsCausedByOperation] FOREIGN KEY([OperationId])
	REFERENCES [Administration].[Operation] ([Id])
)
ON [ChangeHistory]
"
)

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.AssociationChange",
"
CREATE TABLE [Administration].[AssociationChange](
	[Id] [uniqueidentifier] NOT NULL,
	[ChangeSetId] [nvarchar](64) NOT NULL,
	[SequenceNumber] [nvarchar](64) NOT NULL,
	[OperationId] [int] NOT NULL,
	[AssociationId] [uniqueidentifier] NOT NULL,
	[PreviousName] [nvarchar](100) NULL,
	[NextName] [nvarchar](100) NULL,
	[PreviousUri] [nvarchar](1024) NULL,
	[NextUri] [nvarchar](1024) NULL,
	[PreviousSubjectNavigationPropertyId] [uniqueidentifier] NULL,
	[NextSubjectNavigationPropertyId] [uniqueidentifier] NULL,
	[PreviousObjectNavigationPropertyId] [uniqueidentifier] NULL,
	[NextObjectNavigationPropertyId] [uniqueidentifier] NULL,
	[PreviousPredicateId] [uniqueidentifier] NULL,
	[NextPredicateId] [uniqueidentifier] NULL,
	[PreviousSubjectMultiplicity] [nvarchar](32) NULL,
	[NextSubjectMultiplicity] [nvarchar](32) NULL,
	[PreviousObjectMultiplicity] [nvarchar](32) NULL,
	[NextObjectMultiplicity] [nvarchar](32) NULL,
	CONSTRAINT [PK_AssociationChange] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	),
	CONSTRAINT [UQ_AssociationChange] UNIQUE NONCLUSTERED 
	(
		[ChangeSetId] ASC,
		[SequenceNumber] ASC,
		[OperationId] ASC
	),
	/*[AssociationChangeBelongsToChangeSet]*/
	CONSTRAINT [AssociationChangeBelongsToChangeSet] FOREIGN KEY ([ChangeSetId])
	REFERENCES [Administration].[ChangeSet] ([Id]),
	/*[AssociationChangeIsCausedByOperation]*/
	CONSTRAINT [AssociationChangeIsCausedByOperation] FOREIGN KEY([OperationId])
	REFERENCES [Administration].[Operation] ([Id])
) ON [ChangeHistory]
"
)

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ResourceTypeChange",
"
CREATE TABLE [Administration].[ResourceTypeChange](
	[Id] [uniqueidentifier] NOT NULL,
	[ChangeSetId] [nvarchar](64) NOT NULL,
	[SequenceNumber] [nvarchar](64) NOT NULL,
	[OperationId] [int] NOT NULL,
	[ResourceTypeId] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[PreviousDataModelModuleId] [uniqueidentifier] NULL,
	[NextDataModelModuleId] [uniqueidentifier] NULL,
	[PreviousBaseTypeId] [uniqueidentifier] NULL,
	[NextBaseTypeId] [uniqueidentifier] NULL,
	[PreviousName] [nvarchar](100) NULL,
	[NextName] [nvarchar](100) NULL,
	[PreviousUri] [nvarchar](1024) NULL,
	[NextUri] [nvarchar](1024) NULL,
	[PreviousDescription] [nvarchar](4000) NULL,
	[NextDescription] [nvarchar](4000) NULL,
	CONSTRAINT [PK_ResourceTypeChange] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	),
	CONSTRAINT [UQ_ResourceTypeChange] UNIQUE NONCLUSTERED 
	(
		[ChangeSetId] ASC,
		[SequenceNumber] ASC,
		[OperationId] ASC
	),
	/*[ResourceTypeChangeBelongsToChangeSet]*/
	CONSTRAINT [ResourceTypeChangeBelongsToChangeSet] FOREIGN KEY([ChangeSetId])
	REFERENCES [Administration].[ChangeSet] ([Id]),
	/*[ResourceTypeChangeIsCausedByOperation]*/
	CONSTRAINT [ResourceTypeChangeIsCausedByOperation] FOREIGN KEY([OperationId])
	REFERENCES [Administration].[Operation] ([Id])
) ON [ChangeHistory]
"
)

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ScalarPropertyChange",
"
CREATE TABLE [Administration].[ScalarPropertyChange](
	[Id] [uniqueidentifier] NOT NULL,
	[ChangeSetId] [nvarchar](64) NOT NULL,
	[SequenceNumber] [nvarchar](64) NOT NULL,
	[OperationId] [int] NOT NULL,
	[ScalarPropertyId] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[PreviousResourceTypeId] [uniqueidentifier] NULL,
	[NextResourceTypeId] [uniqueidentifier] NULL,
	[PreviousName] [nvarchar](100) NULL,
	[NextName] [nvarchar](100) NULL,
	[PreviousUri] [nvarchar](1024) NULL,
	[NextUri] [nvarchar](1024) NULL,
	[PreviousDescription] [nvarchar](max) NULL,
	[NextDescription] [nvarchar](max) NULL,
	[PreviousDataType] [nvarchar](100) NULL,
	[NextDataType] [nvarchar](100) NULL,
	[PreviousNullable] [bit] NULL,
	[NextNullable] [bit] NULL,
	[PreviousMaxLength] [int] NULL,
	[NextMaxLength] [int] NULL,
	[PreviousScale] [int] NULL,
	[NextScale] [int] NULL,
	[PreviousPrecision] [int] NULL,
	[NextPrecision] [int] NULL,
	CONSTRAINT [PK_ScalarPropertyChange] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	),
	CONSTRAINT [UQ_ScalarPropertyChange] UNIQUE NONCLUSTERED 
	(
		[ChangeSetId] ASC,
		[SequenceNumber] ASC,
		[OperationId] ASC
	),
	/*[ScalarPropertyChangeBelongsToChangeSet]*/
	CONSTRAINT [ScalarPropertyChangeBelongsToChangeSet] FOREIGN KEY([ChangeSetId])
	REFERENCES [Administration].[ChangeSet] ([Id]),
	/*[ScalarPropertyChangeIsCausedByOperation]*/
	CONSTRAINT [ScalarPropertyChangeIsCausedByOperation] FOREIGN KEY([OperationId])
	REFERENCES [Administration].[Operation] ([Id])
) ON [ChangeHistory]
"
)

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.NavigationPropertyChange",
"
CREATE TABLE [Administration].[NavigationPropertyChange](
	[Id] [uniqueidentifier] NOT NULL,
	[ChangeSetId] [nvarchar](64) NOT NULL,
	[SequenceNumber] [nvarchar](64) NOT NULL,
	[OperationId] [int] NOT NULL,
	[NavigationPropertyId] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[PreviousResourceTypeId] [uniqueidentifier] NULL,
	[NextResourceTypeId] [uniqueidentifier] NULL,
	[PreviousName] [nvarchar](100) NULL,
	[NextName] [nvarchar](100) NULL,
	[PreviousUri] [nvarchar](1024) NULL,
	[NextUri] [nvarchar](1024) NULL,
	[PreviousDescription] [nvarchar](max) NULL,
	[NextDescription] [nvarchar](max) NULL,
	CONSTRAINT [PK_NavigationPropertyChange] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	),
	CONSTRAINT [UQ_NavigationPropertyChange] UNIQUE NONCLUSTERED 
	(
		[ChangeSetId] ASC,
		[SequenceNumber] ASC,
		[OperationId] ASC
	),
	/*[NavigationPropertyChangeBelongsToChangeSet]*/
	CONSTRAINT [NavigationPropertyChangeBelongsToChangeSet] FOREIGN KEY([ChangeSetId])
	REFERENCES [Administration].[ChangeSet] ([Id]),
	/*[NavigationPropertyChangeIsCausedByOperation]*/
	CONSTRAINT [NavigationPropertyChangeIsCausedByOperation] FOREIGN KEY([OperationId])
	REFERENCES [Administration].[Operation] ([Id])
) ON [ChangeHistory]
"
)

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	Given the binary property details, creates an XML fragment that has the previous and next 
	binary values as base64 encoded string.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.GetEscapedBinaryPropertyChangeXml",
"
CREATE FUNCTION [Administration].[GetEscapedBinaryPropertyChangeXml]
(
	@PropertyName [nvarchar](256),
	@Operation [int],
	@PreviousValue [varbinary](max),
	@NextValue [varbinary](max),
	@CaptureInstance [nvarchar](256),
	@ColumnName [nvarchar](256),
	@UpdateMask [varbinary](128)
)
RETURNS [nvarchar](max)
AS
BEGIN
	-- Compute 'Changed' flag.
	DECLARE @Changed [bit];
	SELECT @Changed = [sys].[fn_cdc_has_column_changed]( 
		@CaptureInstance, @ColumnName, @UpdateMask);
	
	-- Compute previous and next values. Multiple IF statements are needed to avoid generating
	-- the <PreviousValue/> and <NextValue/> elements in case of NULL values.
	IF @PreviousValue IS NULL AND @NextValue IS NULL
	RETURN
	(
		SELECT [Tag], [Parent], [PropertyChange!1!PropertyName], 
		[PropertyChange!1!Changed]
		FROM
		(
			SELECT 1 [Tag], NULL [Parent], @PropertyName [PropertyChange!1!PropertyName], 
				CASE WHEN @Operation <> 3 THEN NULL
				ELSE 
					CASE WHEN @Changed = 'False' THEN 'False'
					ELSE 'True'
					END
				END [PropertyChange!1!Changed]
		)[T]
		FOR XML EXPLICIT, BINARY BASE64
	)
	
	IF @PreviousValue IS NULL AND @NextValue IS NOT NULL
	RETURN
	(
		SELECT [Tag], [Parent], [PropertyChange!1!PropertyName], 
		[PropertyChange!1!Changed], [NextValue!3!]
		FROM
		(
			SELECT 1 [Tag], NULL [Parent], @PropertyName [PropertyChange!1!PropertyName], 
				CASE WHEN @Operation <> 3 THEN NULL
				ELSE 
					CASE WHEN @Changed = 'False' THEN 'False'
					ELSE 'True'
					END
				END [PropertyChange!1!Changed],
				NULL [NextValue!3!]
			UNION
			SELECT 3 [Tag], 1 [Parent], NULL [PropertyChange!1!PropertyName], 
				NULL [PropertyChange!1!Changed], @NextValue [NextValue!3!]
		)[T]
		FOR XML EXPLICIT, BINARY BASE64
	)
	
	IF @PreviousValue IS NOT NULL AND @NextValue IS NULL
	RETURN
	(
		SELECT [Tag], [Parent], [PropertyChange!1!PropertyName], 
		[PropertyChange!1!Changed], [PreviousValue!2!]
		FROM
		(
			SELECT 1 [Tag], NULL [Parent], @PropertyName [PropertyChange!1!PropertyName], 
				CASE WHEN @Operation <> 3 THEN NULL
				ELSE 
					CASE WHEN @Changed = 'False' THEN 'False'
					ELSE 'True'
					END
				END [PropertyChange!1!Changed],
				NULL [PreviousValue!2!]
			UNION
			SELECT 2 [Tag], 1 [Parent], NULL [PropertyChange!1!PropertyName], 
				NULL [PropertyChange!1!Changed], @PreviousValue [PreviousValue!2!]
		)[T]
		FOR XML EXPLICIT, BINARY BASE64
	)
	
	RETURN
	(
		SELECT [Tag], [Parent], [PropertyChange!1!PropertyName], 
		[PropertyChange!1!Changed], [PreviousValue!2!], [NextValue!3!]
		FROM
		(
			SELECT 1 [Tag], NULL [Parent], @PropertyName [PropertyChange!1!PropertyName], 
				CASE WHEN @Operation <> 3 THEN NULL
				ELSE 
					CASE WHEN @Changed = 'False' THEN 'False'
					ELSE 'True'
					END
				END [PropertyChange!1!Changed],
				NULL [PreviousValue!2!], NULL [NextValue!3!]
			UNION
			SELECT 2 [Tag], 1 [Parent], NULL [PropertyChange!1!PropertyName], 
				NULL [PropertyChange!1!Changed], @PreviousValue [PreviousValue!2!], 
				NULL [NextValue!3!]
			UNION
			SELECT 3 [Tag], 1 [Parent], NULL [PropertyChange!1!PropertyName], 
				NULL [PropertyChange!1!Changed], NULL [PreviousValue!2!], 
				@NextValue [NextValue!3!]
		)[T]
		FOR XML EXPLICIT, BINARY BASE64
	)
END
"
)

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	Given the property details, creates an XML fragment that has escaped special characters.
	
	For example, PropertyName = '<<some Prop&', PreviousValue = '''some<xml<<>"&', Operation
	= 2 and NextValue = '''some<xml<<>"& - new value' would generate the following XML.
	<PropertyChange PropertyName="&lt;&lt;some Prop&amp;">
	  <PreviousValue>'some&lt;xml&lt;&lt;&gt;"&amp;</PreviousValue>
	  <NextValue>'some&lt;xml&lt;&lt;&gt;"&amp; - new value</NextValue>
	</PropertyChange>
	
	Same inputs with Operation = 3 would generate the following Xml.
	<PropertyChange PropertyName="&lt;&lt;some Prop&amp;" Changed="True">
	  <PreviousValue>'some&lt;xml&lt;&lt;&gt;"&amp;</PreviousValue>
	  <NextValue>'some&lt;xml&lt;&lt;&gt;"&amp; - new value</NextValue>
	</PropertyChange>	
 Parameters: 
	@PropertyName	- Property name.
	@Operation		- Operation code, 1=Delete, 2=Insert, 3=Update.
	@PreviousValue	- Previous property value, can be NULL for Inserts.
	@NextValue		- Updated property value, can be NULL for Deletes.
	@CaptureInstance, @ColumnName, @UpdateMask - To compute the 'Changed' flag for this property.
 Return values: 
	The escaped Xml fragment representing property change.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.GetEscapedPropertyChangeXml",
"
CREATE FUNCTION [Administration].[GetEscapedPropertyChangeXml]
(
	@PropertyName [nvarchar](256),
	@Operation [int],
	@PreviousValue [nvarchar](max),
	@NextValue [nvarchar](max),
	@CaptureInstance [nvarchar](256),
	@ColumnName [nvarchar](256),
	@UpdateMask [varbinary](128)
)
RETURNS [nvarchar](max)
AS
BEGIN
	-- Compute 'Changed' flag.
	DECLARE @Changed [bit];
	SELECT @Changed = [sys].[fn_cdc_has_column_changed]( 
		@CaptureInstance, @ColumnName, @UpdateMask);
	
	-- Compute previous and next values. Multiple IF statements are needed to avoid generating
	-- the <PreviousValue/> and <NextValue/> elements in case of NULL values.
	IF @PreviousValue IS NULL AND @NextValue IS NULL
	RETURN
	(
		SELECT [Tag], [Parent], [PropertyChange!1!PropertyName], 
		[PropertyChange!1!Changed]
		FROM
		(
			SELECT 1 [Tag], NULL [Parent], @PropertyName [PropertyChange!1!PropertyName], 
				CASE WHEN @Operation <> 3 THEN NULL
				ELSE 
					CASE WHEN @Changed = 'False' THEN 'False'
					ELSE 'True'
					END
				END [PropertyChange!1!Changed]
		)[T]
		FOR XML EXPLICIT
	)
	
	IF @PreviousValue IS NULL AND @NextValue IS NOT NULL
	RETURN
	(
		SELECT [Tag], [Parent], [PropertyChange!1!PropertyName], 
		[PropertyChange!1!Changed], [NextValue!3!]
		FROM
		(
			SELECT 1 [Tag], NULL [Parent], @PropertyName [PropertyChange!1!PropertyName], 
				CASE WHEN @Operation <> 3 THEN NULL
				ELSE 
					CASE WHEN @Changed = 'False' THEN 'False'
					ELSE 'True'
					END
				END [PropertyChange!1!Changed],
				NULL [NextValue!3!]
			UNION
			SELECT 3 [Tag], 1 [Parent], NULL [PropertyChange!1!PropertyName], 
				NULL [PropertyChange!1!Changed], @NextValue [NextValue!3!]
		)[T]
		FOR XML EXPLICIT
	)
	
	IF @PreviousValue IS NOT NULL AND @NextValue IS NULL
	RETURN
	(
		SELECT [Tag], [Parent], [PropertyChange!1!PropertyName], 
		[PropertyChange!1!Changed], [PreviousValue!2!]
		FROM
		(
			SELECT 1 [Tag], NULL [Parent], @PropertyName [PropertyChange!1!PropertyName], 
				CASE WHEN @Operation <> 3 THEN NULL
				ELSE 
					CASE WHEN @Changed = 'False' THEN 'False'
					ELSE 'True'
					END
				END [PropertyChange!1!Changed],
				NULL [PreviousValue!2!]
			UNION
			SELECT 2 [Tag], 1 [Parent], NULL [PropertyChange!1!PropertyName], 
				NULL [PropertyChange!1!Changed], @PreviousValue [PreviousValue!2!]
		)[T]
		FOR XML EXPLICIT
	)
	
	RETURN
	(
		SELECT [Tag], [Parent], [PropertyChange!1!PropertyName], 
		[PropertyChange!1!Changed], [PreviousValue!2!], [NextValue!3!]
		FROM
		(
			SELECT 1 [Tag], NULL [Parent], @PropertyName [PropertyChange!1!PropertyName], 
				CASE WHEN @Operation <> 3 THEN NULL
				ELSE 
					CASE WHEN @Changed = 'False' THEN 'False'
					ELSE 'True'
					END
				END [PropertyChange!1!Changed],
				NULL [PreviousValue!2!], NULL [NextValue!3!]
			UNION
			SELECT 2 [Tag], 1 [Parent], NULL [PropertyChange!1!PropertyName], 
				NULL [PropertyChange!1!Changed], @PreviousValue [PreviousValue!2!], 
				NULL [NextValue!3!]
			UNION
			SELECT 3 [Tag], 1 [Parent], NULL [PropertyChange!1!PropertyName], 
				NULL [PropertyChange!1!Changed], NULL [PreviousValue!2!], 
				@NextValue [NextValue!3!]
		)[T]
		FOR XML EXPLICIT
	)
END
"
)

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	Given a ChangeSetId, this procedure prepares a list of IDs of all resources that underwent some 
	change in this changeset.A cursor is opened on this list and it is then assigned to an output 
	parameter. The cursor has following columns:
	1. ResourceId - The identifier of the resource that underwent some change in this changeset.
	2. ResourceTypeId - ResourceTypeId of the resource.
 Parameters: 
	@ChangeSetId - Identifier of the changeset to process.
	@ResourceInfoCursor - The cursor for all the resource IDs that underwent some change in this 
		changeset.
 Exceptions: 
	None.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.GetResourceAtChangeSet",
"
CREATE PROCEDURE [Administration].[GetResourceAtChangeSet]
	@ChangeSetId varbinary(64),
	@ResourceInfoCursor  CURSOR VARYING OUTPUT
AS
BEGIN 
	DECLARE @Sql nvarchar(max);
	SET @Sql = N'';

	-- Prepare a query to fetch Ids from each of the capture instances created for Core.Resource.
	-- NOTE: DO NOT use a UNION ALL, we need DISTINCT values of ResourceId here.
	-- There could be multiple capture instances containing values for a resource. For example,
	-- we create a new capture instance when there is change in the schema of Core.Resource table
	-- but do not drop the earlier capture instance until the next schema change.
	-- NOTE: Even though we do not use UNION ALL, we can get duplicate IDs. For example, if there
	-- is a resource update. Two entries will be made in the Core_Resource_CT table and the query 
	-- below would return two rows for the same resource Id. It is only when there is another 
	-- capture instance present the UNION eliminates duplicates. Thus, use a DISTINCT in the final 
	-- query.
	SELECT @Sql = @Sql + 
		N' SELECT [Id] [ResourceId], [ResourceTypeId] ' +
		N' FROM [cdc].[' + [capture_instance] + N'_CT] WHERE [__$start_lsn] = @VarChangeSetId'+
		N'
		UNION '
	FROM [cdc].[change_tables]
	WHERE [source_object_id] = OBJECT_ID('Core.Resource'); 

	-- Strip off the last 'UNION '.	 
	SET @Sql= SUBSTRING(@Sql,0,LEN(@Sql) - LEN(N'UNION '));

	-- Prepare the CURSOR. Note the DISTINCT here.
	-- By making the resource Id read only and system generated, we ensure that for any two resource 
	-- in the database, if the IDs are equal then their ResourceTypeIds are also equal.
	SET @Sql =  N'SET @VarResourceInfoCursor = CURSOR STATIC 
		 FOR  
	   	 SELECT DISTINCT [A].[ResourceId], [A].[ResourceTypeId] 
		 FROM (' + @Sql + N' ) [A] 
		 
		 OPEN @VarResourceInfoCursor;
		 ';
	EXEC sp_executesql @Sql , N'@VarChangeSetId varbinary(64), @VarResourceInfoCursor CURSOR OUTPUT', 
	@VarChangeSetId = @ChangeSetId,
	@VarResourceInfoCursor = @ResourceInfoCursor OUTPUT;
					
	RETURN 0;
END
");

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	This procedure processes the entries in [cdc].Core_Predicate_CT table.
 Parameters: 
	@ChangeSetId - Identifier for the changeset to process.
 Exceptions: 
	None.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ProcessPredicateChanges",
"
CREATE PROCEDURE [Administration].[ProcessPredicateChanges]
	@ChangeSetId varbinary(64)
AS
BEGIN 
	IF EXISTS(SELECT 1 FROM  [cdc].[Core_Predicate_CT] 
		WHERE [__$start_lsn] = @ChangeSetId )

		INSERT INTO [Administration].[PredicateChange]([Id], [ChangeSetId], [SequenceNumber],
		[OperationId], [PredicateId], [PreviousName], [NextName], [PreviousUri], [NextUri])

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			2 [Operation],
			[Id] [PredicateId],
			NULL [PreviousName],
			[Name] [NextName],
			NULL [PreviousUri],
			[Uri] [NextUri] 
		FROM [cdc].[Core_Predicate_CT] 
		WHERE [__$operation] = 2 
			AND [__$start_lsn] = @ChangeSetId 

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			1 [Operation],
			[Id] [PredicateId],
			[Name] [PreviousName],
			NULL [NextName],
			[Uri] [PreviousUri],
			NULL [NextUri] 
		FROM [cdc].[Core_Predicate_CT] 
		WHERE  [__$operation] = 1 
			AND [__$start_lsn] = @ChangeSetId  

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$seqval])) [SequenceNumber],
			3 [Operation],
			[Previous].[Id] [PredicateId],
			[Previous].[Name] [PreviousName],
			[Next].[Name] [NextName],
			[Previous].[Uri] [PreviousUri],
			[Next].[Uri] [NextUri] 
		FROM  [cdc].[Core_Predicate_CT] [Previous] 
		INNER JOIN [cdc].[Core_Predicate_CT] [Next] 
			ON  [Previous].[__$seqval] = [Next].[__$seqval] 
				AND [Previous].[__$operation] = 3 
				AND [Next].[__$operation] = 4 
				AND [Previous].[__$start_lsn] = [Next].[__$start_lsn]  
				AND [Previous].[Id] = [Next].[Id] 
		WHERE [Previous].[__$start_lsn] =  @ChangeSetId;
	RETURN 0;
END
");

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	This procedure processes the entries in [cdc].[Core_Property_CT] table.
 Parameters: 
	@ChangeSetId - Identifier for the changeset to process.
 Exceptions: 
	None.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ProcessPropertyChanges",
"
CREATE PROCEDURE [Administration].[ProcessPropertyChanges]
	@ChangeSetId varbinary(64)
AS
BEGIN 
	IF EXISTS(SELECT [__$start_lsn] FROM [cdc].[Core_Property_CT] 
		WHERE [__$start_lsn] = @ChangeSetId)
		
		INSERT INTO [Administration].[PropertyChange]([Id], [ChangeSetId], [SequenceNumber], 
		[OperationId], [PropertyId], [PreviousName], [NextName], [PreviousUri], [NextUri])
		
		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			2 [Operation],
			[Id] [PropertyId],
			NULL [PreviousName],
			[Name] [NextName] ,
			NULL [PreviousUri],
			[Uri] [NextUri] 
		FROM [cdc].[Core_Property_CT] 
		WHERE  [__$operation] = 2 
			AND [__$start_lsn] = @ChangeSetId  
		
		UNION ALL 
		
		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			1 [Operation],
			[Id] [PropertyId],
			[Name] [PreviousName],
			NULL [NextName] ,
			[Uri] [PreviousUri],
			NULL [NextUri] 
		FROM [cdc].[Core_Property_CT] 
		WHERE  [__$operation] = 1 
			AND [__$start_lsn] = @ChangeSetId 
		
		UNION ALL 
		
		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$seqval])) [SequenceNumber],
			3 [Operation],
			[Previous].[Id] [PropertyId],
			[Previous].[Name] [PreviousName],
			[Next].[Name] [NextName] ,
			[Previous].[Uri] [PreviousUri],
			[Next].[Uri] [NextUri] 
		FROM  [cdc].[Core_Property_CT]  [Previous] 
		INNER JOIN [cdc].[Core_Property_CT] [Next] 
			ON  [Previous].[__$seqval] = [Next].[__$seqval] 
			AND [Previous].[__$operation] = 3 
			AND [Next].[__$operation] = 4 
			AND [Previous].[__$start_lsn] = [Next].[__$start_lsn]  
			AND [Previous].[Id] = [Next].[Id] 
		WHERE  [Previous].[__$start_lsn] = @ChangeSetId    
	RETURN 0;
END
");

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	This procedure processes the entries in [cdc].[Core_Relationship_CT] table.
 Parameters: 
	@ChangeSetId - Identifier for the changeset to process.
 Exceptions: 
	None.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ProcessRelationshipChanges",
"
CREATE PROCEDURE [Administration].[ProcessRelationshipChanges]
@ChangeSetId varbinary(64)
AS
BEGIN 
	IF EXISTS(SELECT [__$start_lsn] FROM  [cdc].[Core_Relationship_CT] 
		WHERE [__$start_lsn] = @ChangeSetId)

		INSERT INTO [Administration].[RelationshipChange]([Id], [ChangeSetId], [SequenceNumber],
		[OperationId], [RelationshipId], [PreviousSubjectResourceId], [NextSubjectResourceId],
		[PreviousObjectResourceId], [NextObjectResourceId], [PreviousPredicateId], 
		[NextPredicateId], [PreviousOrdinalPosition], [NextOrdinalPosition], [PreviousDateAdded],
		[NextDateAdded])

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			2 [Operation],
			[Id] [RelationshipId],
			NULL [PreviousSubjectResourceId],
			[SubjectResourceId] [NextSubjectResourceId],
			NULL [PreviousObjectResourceId],
			[ObjectResourceId] [NextObjectResourceId],
			NULL [PreviousPredicateId],
			[PredicateId] [NextPredicateId],
			NULL [PreviousOrdinalPosition],
			[OrdinalPosition] [NextOrdinalPosition] ,
			NULL [PreviousDateAdded],
			[DateAdded] [NextDateAdded]
		FROM [cdc].[Core_Relationship_CT] 
		WHERE  [__$operation] = 2 
			AND [__$start_lsn] = @ChangeSetId 

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			1 [Operation],
			[Id] [RelationshipId],
			[SubjectResourceId] [PreviousSubjectResourceId],
			NULL [NextSubjectResourceId],
			[ObjectResourceId] [PreviousObjectResourceId],
			NULL [NextObjectResourceId],
			[PredicateId] [PreviousPredicateId],
			NULL [NextPredicateId],
			[OrdinalPosition] [PreviousOrdinalPosition],
			NULL [NextOrdinalPosition] ,
			[DateAdded] [PreviousDateAdded],
			NULL [NextDateAdded]
		FROM [cdc].[Core_Relationship_CT] 
		WHERE  [__$operation] = 1 
			AND [__$start_lsn] = @ChangeSetId  

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$seqval])) [SequenceNumber],
			3 [Operation],
			[Previous].[Id] [RelationshipId],
			[Previous].[SubjectResourceId] [PreviousSubjectResourceId],
			[Next].[SubjectResourceId] [NextSubjectResourceId],
			[Previous].[ObjectResourceId] [PreviousObjectResourceId],
			[Next].[ObjectResourceId] [NextObjectResourceId],
			[Previous].[PredicateId] [PreviousPredicateId],
			[Next].[PredicateId] [NextPredicateId],
			[Previous].[OrdinalPosition] [PreviousOrdinalPosition],
			[Next].[OrdinalPosition] [NextOrdinalPosition] ,
			[Previous].[DateAdded] [PreviousDateAdded],
			[Next].[DateAdded] [NextDateAdded]
		FROM  [cdc].[Core_Relationship_CT] [Previous] 
		INNER JOIN [cdc].[Core_Relationship_CT] [Next] 
			ON  [Previous].[__$seqval] = [Next].[__$seqval] 
			AND [Previous].[__$operation] = 3 
			AND [Next].[__$operation] = 4 
			AND [Previous].[__$start_lsn] = [Next].[__$start_lsn] 
			AND [Previous].[Id] = [Next].[Id] 
		WHERE  [Previous].[__$start_lsn] = @ChangeSetId    
	RETURN 0;
END
");

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	This procedure processes the entries in [cdc].[Core_PredicateProperty_CT] table.
 Parameters: 
	@ChangeSetId - Identifier for the changeset to process.
 Exceptions: 
	None.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ProcessPredicatePropertyChanges",
"
CREATE PROCEDURE [Administration].[ProcessPredicatePropertyChanges]
	@ChangeSetId varbinary(64)
AS
BEGIN 
	IF EXISTS(SELECT [__$start_lsn] FROM  [cdc].[Core_PredicateProperty_CT] 
		WHERE [__$start_lsn] = @ChangeSetId) 

		INSERT INTO [Administration].[PredicatePropertyChange]([Id], [ChangeSetId], [SequenceNumber],
		[OperationId], [PredicatePropertyId], [PreviousPredicateId], [NextPredicateId], 
		[PreviousPropertyId], [NextPropertyId], [PreviousValue], [NextValue])

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			2 [Operation],
			[Id] [PredicatePropertyId],
			NULL [PreviousPredicateId], 
			[PredicateId] [NextPredicateId] ,
			NULL [PreviousPropertyId], 
			[PropertyId] [NextPropertyId] , 
			NULL [PreviousValue],
			[Value] [NextValue]
		FROM [cdc].[Core_PredicateProperty_CT] 
		WHERE  [__$operation] = 2 
			AND [__$start_lsn] = @ChangeSetId  

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			1 [Operation],
			[Id] [PredicatePropertyId],
			[PredicateId] [PreviousPredicateId], 
			NULL [NextPredicateId] ,
			[PropertyId] [PreviousPropertyId], 
			NULL [NextPropertyId] , 
			[Value] [PreviousValue],
			NULL [NextValue] 
		FROM [cdc].[Core_PredicateProperty_CT] 
		WHERE  [__$operation] = 1 
			AND [__$start_lsn] = @ChangeSetId  

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$seqval])) [SequenceNumber],
			3 [Operation],
			[Previous].[Id] [PredicatePropertyId],
			[Previous].[PredicateId] [PreviousPredicateId],
			[Next].[PredicateId] [NextPredicateId], 
			[Previous].[PropertyId] [PreviousPropertyId],
			[Next].[PropertyId] [NextPropertyId], 
			[Previous].[Value] [PreviousValue],
			[Next].[Value] [NextValue] 
		FROM  [cdc].[Core_PredicateProperty_CT]  [Previous] 
		INNER JOIN [cdc].[Core_PredicateProperty_CT] [Next] 
			ON  [Previous].[__$seqval] = [Next].[__$seqval] 
				AND [Previous].[__$operation] = 3 
				AND [Next].[__$operation] = 4 
				AND [Previous].[__$start_lsn] = [Next].[__$start_lsn] 
				AND [Previous].[Id] = [Next].[Id] 
		WHERE  [Previous].[__$start_lsn] = @ChangeSetId    
	RETURN 0;
END
");

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	This procedure processes the entries in [cdc].[Core_RelationshipProperty_CT] table.
 Parameters: 
	@ChangeSetId - Identifier for the changeset to process.
 Exceptions: 
	None.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ProcessRelationshipPropertyChanges",
"
CREATE PROCEDURE [Administration].[ProcessRelationshipPropertyChanges]
	@ChangeSetId varbinary(64)
AS
BEGIN 
	IF EXISTS(SELECT [__$start_lsn] FROM  [cdc].[Core_RelationshipProperty_CT] 
		WHERE [__$start_lsn] = @ChangeSetId)
		
		INSERT INTO [Administration].[RelationshipPropertyChange]([Id], [ChangeSetId], 
		[SequenceNumber], [OperationId], [RelationshipPropertyId], [PreviousTripletId],
		[NextTripletId], [PreviousPropertyId], [NextPropertyId], [PreviousValue],
		[NextValue])

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			2 [Operation],
			[Id] [RelationshipPropertyId],
			NULL [PreviousTripletId],
			[TripletId] [NextTripletId],
			NULL [PreviousPropertyId],
			[PropertyId] [NextPropertyId],
			NULL [PreviousValue],
			[Value] [NextValue] 
		FROM [cdc].[Core_RelationshipProperty_CT] 
		WHERE  [__$operation] = 2 
			AND [__$start_lsn] = @ChangeSetId  

		UNION ALL 
		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			1 [Operation],
			[Id] [RelationshipPropertyId],
			[TripletId] [PreviousTripletId],
			NULL [NextTripletId],
			[PropertyId] [PreviousPropertyId],
			NULL [NextPropertyId] ,
			[Value] [PreviousValue],
			NULL [NextValue]
		FROM [cdc].[Core_RelationshipProperty_CT] 
		WHERE  [__$operation] = 1 
			AND [__$start_lsn] = @ChangeSetId  

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$seqval])) [SequenceNumber],
			3 [Operation],
			[Previous].[Id] [RelationshipPropertyId],
			[Previous].[TripletId] [PreviousTripletId],
			[Next].[TripletId] [NextTripletId],
			[Previous].[PropertyId] [PreviousPropertyId],
			[Next].[PropertyId] [NextPropertyId]  ,
			[Previous].[Value] [PreviousValue],
			[Next].[Value] [NextValue] 
		FROM  [cdc].[Core_RelationshipProperty_CT] [Previous] 
		INNER JOIN [cdc].[Core_RelationshipProperty_CT] [Next] 
		ON  [Previous].[__$seqval] = [Next].[__$seqval] 
			AND [Previous].[__$operation] = 3 
			AND [Next].[__$operation] = 4 
			AND [Previous].[__$start_lsn] = [Next].[__$start_lsn]  
			AND [Previous].[Id] = [Next].[Id] 
		WHERE  [Previous].[__$start_lsn] = @ChangeSetId    
	RETURN 0;
END
");

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	This procedure processes the entries in [cdc].[Core_Resource_CT] and other custom resource type
	tables.
 Parameters: 
	@ChangeSetId - Identifier for the changeset to process.
 Exceptions: 
	None.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ProcessResourceChanges",
"
CREATE PROCEDURE [Administration].[ProcessResourceChanges]
	@ChangeSetId varbinary(64)
AS
BEGIN 
	-- Get a list of resource identifiers to process for this change set.
	DECLARE @ResourceId uniqueidentifier;
	DECLARE @ResourceTypeId nvarchar(128);
	DECLARE @ResourceInfoCursor  CURSOR;
	EXEC [Administration].[GetResourceAtChangeSet] @ChangeSetId, @ResourceInfoCursor OUTPUT;

	FETCH NEXT FROM @ResourceInfoCursor 
	INTO @ResourceId, @ResourceTypeId;

	WHILE @@FETCH_STATUS = 0
	BEGIN
		-- Retrieve the capture instances for this resource. There could be multiple capture
		-- instances associated with a source table. This happens when we do a schema change. 
		-- We do not immediately drop the earlier capture instance, but wait until the maximum
		-- limit of capture instances (2) is reached for Core.Resource. If two capture instances 
		-- host data corresponding to the same change set, then the later one is considered 
		-- and the earlier one is discarded. At this overlapping point, we may choose to drop 
		-- the earlier capture instance.
		DECLARE @InstanceName [nvarchar](256);
		DECLARE @Cmd [nvarchar](max);
		
		DECLARE CaptureInstanceCursor CURSOR
		FOR SELECT [capture_instance]
		FROM [cdc].[change_tables]
		WHERE [source_object_id] = OBJECT_ID(N'Core.Resource')
		ORDER BY [create_date] DESC;
		
		OPEN CaptureInstanceCursor;
		
		FETCH NEXT FROM CaptureInstanceCursor INTO @InstanceName;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			-- Process only the latest capture instance that has entries for this changeset.
			DECLARE @RecordCount int;
			DECLARE @Sql nvarchar(4000);
			SET @Sql= 'SELECT @VarRecordCount = COUNT(1) FROM [cdc].[' + @InstanceName + '_CT]
				WHERE [__$start_lsn] = @VarChangeSetId'
			EXEC sp_executesql @Sql, N'@VarRecordCount int OUT, @VarChangeSetId varbinary(64)',
			@VarRecordCount = @RecordCount OUT, @VarChangeSetId = @ChangeSetId;

			IF(@RecordCount > 0)
			BEGIN
				SELECT @Cmd = [PropertyChangesQuery] 
				FROM [Administration].[PropertyChangeQueryForCaptureInstance]
				WHERE [CaptureInstance] = @InstanceName
				AND [ResourceTypeId] = @ResourceTypeId;
					
				BREAK;
			END
			FETCH NEXT FROM CaptureInstanceCursor INTO @InstanceName;
		END
		CLOSE CaptureInstanceCursor;
		DEALLOCATE CaptureInstanceCursor;
		
		INSERT INTO [Administration].[ResourceChange] 
		(
			[Id], [ChangeSetId], [SequenceNumber], [OperationId], [ResourceId],
			[ResourceTypeId], [ResourceTypeFullName], [PropertyChanges]
		)
		EXECUTE sp_executesql @Cmd, 
		N'@VarResourceId uniqueidentifier, @VarChangeSetId varbinary(64)',
		@VarResourceId = @ResourceId, @VarChangeSetId = @ChangeSetId;

		FETCH NEXT FROM @ResourceInfoCursor 
		INTO @ResourceId, @ResourceTypeId
	END

	CLOSE @ResourceInfoCursor;
	DEALLOCATE @ResourceInfoCursor;
	
	RETURN 0;
END
");

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	This procedure processes the entries in [cdc].[Core_ResourceProperty_CT] table.
 Parameters: 
	@ChangeSetId - Identifier for the changeset to process.
 Exceptions: 
	None.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ProcessResourcePropertyChanges",
"
CREATE PROCEDURE [Administration].[ProcessResourcePropertyChanges]
	@ChangeSetId varbinary(64)
AS
BEGIN 
	IF EXISTS(SELECT [__$start_lsn] FROM  [cdc].[Core_ResourceProperty_CT] 
		WHERE [__$start_lsn] = @ChangeSetId)

		INSERT INTO [Administration].[ResourcePropertyChange]([Id], [ChangeSetId], 
		[SequenceNumber], [OperationId], [ResourcePropertyId], [PreviousResourceId],
		[NextResourceId], [PreviousPropertyId], [NextPropertyId], [PreviousValue],
		[NextValue])

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			2 [Operation],
			[Id] [ResourcePropertyId],
			NULL [PreviousResourceId] ,
			[ResourceId] [NextResourceId] , 
			NULL [PreviousPropertyId] , 
			[PropertyId] [NextPropertyId] ,
			NULL [PreviousValue],
			[Value] [NextValue] 
		FROM [cdc].[Core_ResourceProperty_CT] 
		WHERE  [__$operation] = 2 
			AND [__$start_lsn] = @ChangeSetId 

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			1 [Operation],
			[Id] [ResourcePropertyId],
			[ResourceId] [PreviousResourceId] ,
			NULL [NextResourceId] ,
			[PropertyId] [PreviousPropertyId] ,
			NULL [NextPropertyId],
			[Value] [PreviousValue],
			NULL [NextValue] 
		FROM [cdc].[Core_ResourceProperty_CT] 
		WHERE  [__$operation] = 1 
			AND [__$start_lsn] = @ChangeSetId  

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$seqval])) [SequenceNumber],
			3 [Operation],
			[Previous].[Id] [ResourcePropertyId],
			[Previous].[ResourceId] ,
			[Next].[ResourceId] , 
			[Previous].[PropertyId] ,
			[Next].[PropertyId],
			[Previous].[Value] [PreviousValue],
			[Next].[Value] [NextValue] 
		FROM  [cdc].[Core_ResourceProperty_CT] [Previous] 
		INNER JOIN [cdc].[Core_ResourceProperty_CT] [Next] 
			ON  [Previous].[__$seqval] = [Next].[__$seqval] 
				AND [Previous].[__$operation] = 3 
				AND [Next].[__$operation] = 4 
				AND [Previous].[__$start_lsn] = [Next].[__$start_lsn]  
				AND [Previous].[Id] = [Next].[Id] 
		WHERE  [Previous].[__$start_lsn] = @ChangeSetId    
	RETURN 0;
END
");

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	This procedure processes the entries in [cdc].[Core_DataModelModule_CT] table.
 Parameters: 
	@ChangeSetId - Identifier for the changeset to process.
 Exceptions: 
	None.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ProcessDataModelModuleChanges",
"
CREATE PROCEDURE [Administration].[ProcessDataModelModuleChanges]
	@ChangeSetId varbinary(64)
AS
BEGIN 
	IF EXISTS(SELECT [__$start_lsn] FROM  [cdc].[Core_DataModelModule_CT] 
		WHERE [__$start_lsn] = @ChangeSetId)
		
		INSERT INTO [Administration].[DataModelModuleChange] ([Id], [ChangeSetId], [SequenceNumber], 
		[OperationId], [DataModelModuleId], [PreviousNamespace], [NextNamespace], [PreviousUri], 
		[NextUri], [PreviousDescription], [NextDescription], [IsMsShipped])
		
		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			2 [Operation],
			[Id] [DataModelModuleId],
			NULL [PreviousNamespace],
			[Namespace] [NextNamespace],
			NULL [PreviousUri],
			[Uri] [NextUri],
			NULL [PreviousDescription],
			[Description] [NextDescription],
			[IsMsShipped]
		FROM [cdc].[Core_DataModelModule_CT] 
		WHERE  [__$operation] = 2 
			AND [__$start_lsn] = @ChangeSetId  

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			1 [Operation],
			[Id] [DataModelModuleId],
			[Namespace] [PreviousNamespace],
			 NULL [NextNamespace],
			[Uri] [PreviousUri],
			NULL [NextUri],
			[Description] [PreviousDescription],
			NULL [NextDescription],
			[IsMsShipped]
		FROM [cdc].[Core_DataModelModule_CT] 
		WHERE  [__$operation] = 1 
			AND [__$start_lsn] = @ChangeSetId 

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$seqval])) [SequenceNumber],
			3 [Operation],
			[Previous].[Id] [DataModelModuleId],
			[Previous].[Namespace] [PreviousNamespace],
			[Next].[Namespace] [NextNamespace],
			[Previous].[Uri] [PreviousUri],
			[Next].[Uri] [NextUri],
			[Previous].[Description] [PreviousDescription],
			[Next].[Description] [NextDescription],
			[Previous].[IsMsShipped]
		FROM  [cdc].[Core_DataModelModule_CT] [Previous] 
		INNER JOIN [cdc].[Core_DataModelModule_CT] [Next] 
			ON  [Previous].[__$seqval] = [Next].[__$seqval] 
				AND [Previous].[__$operation] = 3 
				AND [Next].[__$operation] = 4 
				AND [Previous].[__$start_lsn] = [Next].[__$start_lsn]  
				AND [Previous].[Id] = [Next].[Id] 
		WHERE  [Previous].[__$start_lsn] = @ChangeSetId    
	RETURN 0;
END
");

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	This procedure processes the entries in [cdc].[Core_Association_CT] table.
 Parameters: 
	@ChangeSetId - Identifier for the changeset to process.
 Exceptions: 
	None.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ProcessAssociationChanges",
"
CREATE PROCEDURE [Administration].[ProcessAssociationChanges]
	@ChangeSetId varbinary(64)
AS
BEGIN 
	IF EXISTS(SELECT [__$start_lsn] FROM  [cdc].[Core_Association_CT] 
		WHERE [__$start_lsn] = @ChangeSetId)
		
		INSERT INTO [Administration].[AssociationChange]([Id], [ChangeSetId], [SequenceNumber],
		[OperationId], [AssociationId], [PreviousName], [NextName], [PreviousUri], [NextUri],
		[PreviousSubjectNavigationPropertyId], [NextSubjectNavigationPropertyId], 
		[PreviousObjectNavigationPropertyId], [NextObjectNavigationPropertyId], 
		[PreviousPredicateId], [NextPredicateId], [PreviousSubjectMultiplicity],
		[NextSubjectMultiplicity], [PreviousObjectMultiplicity], [NextObjectMultiplicity])

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			2 [Operation],
			[Id] [AssociationId],
			NULL	[PreviousName],
			[Name]	[NextName],
			NULL	[PreviousUri],
			[Uri]	[NextUri],
			NULL	[PreviousSubjectNavigationPropertyId],
			[SubjectNavigationPropertyId]	[NextSubjectNavigationPropertyId],
			NULL	[PreviousObjectNavigationPropertyId],
			[ObjectNavigationPropertyId]	[NextObjectNavigationPropertyId],
			NULL	[PreviousPredicateId],
			[PredicateId]	[NextPredicateId],
			NULL	[PreviousSubjectMultiplicity],
			[SubjectMultiplicity]	[NextSubjectMultiplicity],
			NULL	[PreviousObjectMultiplicity],
			[ObjectMultiplicity] [NextObjectMultiplicity]
		FROM [cdc].[Core_Association_CT] 
		WHERE  [__$operation] = 2 
			AND [__$start_lsn] = @ChangeSetId  

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			1 [Operation],
			[Id] [AssociationId],
			[Name]	[PreviousName],
			NULL	[NextName],
			[Uri]	[PreviousUri],
			NULL	[NextUri],
			[SubjectNavigationPropertyId]	[PreviousSubjectNavigationPropertyId],
			NULL	[NextSubjectNavigationPropertyId],
			[ObjectNavigationPropertyId]	[PreviousObjectNavigationPropertyId],
			NULL	[NextObjectNavigationPropertyId],
			[PredicateId]	[PreviousPredicateId],
			NULL	[NextPredicateId],
			[SubjectMultiplicity]	[PreviousSubjectMultiplicity],
			NULL	[NextSubjectMultiplicity],
			[ObjectMultiplicity]	[PreviousObjectMultiplicity],
			NULL [NextObjectMultiplicity]
		FROM [cdc].[Core_Association_CT] 
		WHERE  [__$operation] = 1 
			AND [__$start_lsn] = @ChangeSetId 

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$seqval])) [SequenceNumber],
			3 [Operation],
			[Previous].[Id] [AssociationId],
			[Previous].[Name]	[PreviousName],
			[Next].[Name]	[NextName],
			[Previous].[Uri]	[PreviousUri],
			[Next].[Uri]	[NextUri],
			[Previous].[SubjectNavigationPropertyId]	[PreviousSubjectNavigationPropertyId],
			[Next].[SubjectNavigationPropertyId]	[NextSubjectNavigationPropertyId],
			[Previous].[ObjectNavigationPropertyId]	[PreviousObjectNavigationPropertyId],
			[Next].[ObjectNavigationPropertyId]	[NextObjectNavigationPropertyId],
			[Previous].[PredicateId]	[PreviousPredicateId],
			[Next].[PredicateId]	[NextPredicateId],
			[Previous].[SubjectMultiplicity]	[PreviousSubjectMultiplicity],
			[Next].[SubjectMultiplicity]	[NextSubjectMultiplicity],
			[Previous].[ObjectMultiplicity]	[PreviousObjectMultiplicity],
			[Next].[ObjectMultiplicity] [NextObjectMultiplicity]
		FROM  [cdc].[Core_Association_CT] [Previous] 
		INNER JOIN [cdc].[Core_Association_CT] [Next] 
			ON  [Previous].[__$seqval] = [Next].[__$seqval] 
				AND [Previous].[__$operation] = 3 
				AND [Next].[__$operation] = 4 
				AND [Previous].[__$start_lsn] = [Next].[__$start_lsn]  
				AND [Previous].[Id] = [Next].[Id] 
		WHERE  [Previous].[__$start_lsn] = @ChangeSetId    
	RETURN 0;
END
");

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	This procedure processes the entries in [cdc].[Core_ResourceType_CT] table.
 Parameters: 
	@ChangeSetId - Identifier for the changeset to process.
 Exceptions: 
	None.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ProcessResourceTypeChanges",
"
CREATE PROCEDURE [Administration].[ProcessResourceTypeChanges]
	@ChangeSetId varbinary(64)
AS
BEGIN 
	IF EXISTS(SELECT [__$start_lsn] FROM  [cdc].[Core_ResourceType_CT] 
		WHERE [__$start_lsn] = @ChangeSetId)
		
		INSERT INTO [Administration].[ResourceTypeChange]([Id], [ChangeSetId], [SequenceNumber],
		[OperationId], [ResourceTypeId], [PreviousDataModelModuleId], [NextDataModelModuleId],
		[PreviousBaseTypeId], [NextBaseTypeId], [PreviousName], [NextName], [PreviousUri],
		[NextUri], [PreviousDescription], [NextDescription])

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			2 [Operation],
			[Id] [ResourceTypeId],
			NULL [PreviousDataModelModuleId],
			[DataModelModuleId] [NextDataModelModuleId],
			NULL [PreviousBaseTypeId],
			[BaseTypeId] [NextBaseTypeId],
			NULL [PreviousName],
			[Name] [NextName],
			NULL [PreviousUri],
			[Uri] [NextUri],
			NULL [PreviousDescription],
			[Description] [NextDescription]
		FROM [cdc].[Core_ResourceType_CT] 
		WHERE  [__$operation] = 2 
			AND [__$start_lsn] = @ChangeSetId  

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			1 [Operation],
			[Id] [ResourceTypeId],
			[DataModelModuleId] [PreviousDataModelModuleId],
			NULL [NextDataModelModuleId],
			[BaseTypeId] [PreviousBaseTypeId],
			NULL [NextBaseTypeId],
			[Name] [PreviousName],
			NULL [NextName],
			[Uri] [PreviousUri],
			NULL [NextUri],
			[Description] [PreviousDescription],
			NULL [NextDescription]
		FROM [cdc].[Core_ResourceType_CT] 
		WHERE  [__$operation] = 1 
			AND [__$start_lsn] = @ChangeSetId 

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$seqval])) [SequenceNumber],
			3 [Operation],
			[Previous].[Id] [ResourceTypeId],
			[Previous].[DataModelModuleId] [PreviousDataModelModuleId],
			[Next].[DataModelModuleId] [NextDataModelModuleId],
			[Previous].[BaseTypeId] [PreviousBaseTypeId],
			[Next].[BaseTypeId] [NextBaseTypeId],
			[Previous].[Name] [PreviousName],
			[Next].[Name] [NextName],
			[Previous].[Uri] [PreviousUri],
			[Next].[Uri] [NextUri],
			[Previous].[Description] [PreviousDescription],
			[Next].[Description] [NextDescription]
		FROM  [cdc].[Core_ResourceType_CT] [Previous] 
		INNER JOIN [cdc].[Core_ResourceType_CT] [Next] 
			ON  [Previous].[__$seqval] = [Next].[__$seqval] 
				AND [Previous].[__$operation] = 3 
				AND [Next].[__$operation] = 4 
				AND [Previous].[__$start_lsn] = [Next].[__$start_lsn]  
				AND [Previous].[Id] = [Next].[Id] 
		WHERE  [Previous].[__$start_lsn] = @ChangeSetId    
	RETURN 0;
END
");

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	This procedure processes the entries in [cdc].[Core_NavigationProperty_CT] table.
 Parameters: 
	@ChangeSetId - Identifier for the changeset to process.
 Exceptions: 
	None.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;


INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ProcessNavigationPropertyChanges",
"
CREATE PROCEDURE [Administration].[ProcessNavigationPropertyChanges]
	@ChangeSetId varbinary(64)
AS
BEGIN 
	IF EXISTS(SELECT [__$start_lsn] FROM  [cdc].[Core_NavigationProperty_CT] 
		WHERE [__$start_lsn] = @ChangeSetId)
		
		INSERT INTO [Administration].[NavigationPropertyChange]([Id], [ChangeSetId], 
		[SequenceNumber], [OperationId], [NavigationPropertyId], [PreviousResourceTypeId], 
		[NextResourceTypeId], [PreviousName], [NextName], [PreviousUri], [NextUri], 
		[PreviousDescription], [NextDescription])
	
		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			2 [Operation],
			[Id] [NavigationPropertyId],
			NULL [PreviousResourceTypeId],
			[ResourceTypeId] [NextResourceTypeId],
			NULL [PreviousName],
			[Name] [NextName],
			NULL [PreviousUri],
			[Uri] [NextUri],
			NULL [PreviousDescription],
			[Description] [NextDescription]
		FROM [cdc].[Core_NavigationProperty_CT] 
		WHERE  [__$operation] = 2 
			AND [__$start_lsn] = @ChangeSetId  

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			1 [Operation],
			[Id] [NavigationPropertyId],
			[ResourceTypeId] [PreviousResourceTypeId],
			 NULL [NextResourceTypeId],
			[Name] [PreviousName],
			NULL [NextName],
			[Uri] [PreviousUri],
			NULL [NextUri],
			[Description] [PreviousDescription],
			NULL [NextDescription]
		FROM [cdc].[Core_NavigationProperty_CT] 
		WHERE  [__$operation] = 1 
			AND [__$start_lsn] = @ChangeSetId  
		
		UNION ALL 
				
		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$seqval])) [SequenceNumber],
			3 [Operation],
			[Previous].[Id] [NavigationPropertyId],
			[Previous].[ResourceTypeId] [PreviousResourceTypeId],
			[Next].[ResourceTypeId] [NextResourceTypeId],
			[Previous].[Name] [PreviousName],
			[Next].[Name] [NextName],
			[Previous].[Uri] [PreviousUri],
			[Next].[Uri] [NextUri],
			[Previous].[Description] [PreviousDescription],
			[Next].[Description] [NextDescription]
		FROM  [cdc].[Core_NavigationProperty_CT] [Previous] 
		INNER JOIN [cdc].[Core_NavigationProperty_CT] [Next] 
			ON  [Previous].[__$seqval] = [Next].[__$seqval] 
				AND [Previous].[__$operation] = 3 
				AND [Next].[__$operation] = 4 
				AND [Previous].[__$start_lsn] = [Next].[__$start_lsn]  
				AND [Previous].[Id] = [Next].[Id] 
		WHERE  [Previous].[__$start_lsn] = @ChangeSetId 

	RETURN 0;
END
");

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	This procedure processes the entries in [cdc].[Core_ScalarProperty_CT] table.
 Parameters: 
	@ChangeSetId - Identifier for the changeset to process.
 Exceptions: 
	None.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ProcessScalarPropertyChanges",
"
CREATE PROCEDURE [Administration].[ProcessScalarPropertyChanges]
	@ChangeSetId varbinary(64)
AS
BEGIN 
	IF EXISTS(SELECT [__$start_lsn] FROM  [cdc].[Core_ScalarProperty_CT] 
		WHERE [__$start_lsn] = @ChangeSetId)
		
		INSERT INTO [Administration].[ScalarPropertyChange]([Id], [ChangeSetId], [SequenceNumber],
		[OperationId], [ScalarPropertyId], [PreviousResourceTypeId], [NextResourceTypeId],
		[PreviousName], [NextName], [PreviousUri], [NextUri], [PreviousDescription], 
		[NextDescription], [PreviousDataType], [NextDataType], [PreviousNullable], [NextNullable],
		[PreviousMaxLength], [NextMaxLength], [PreviousScale], [NextScale], [PreviousPrecision],
		[NextPrecision])
		
		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			2 [Operation],
			[Id] [ScalarPropertyId],
			NULL [PreviousResourceTypeId],
			[ResourceTypeId] [NextResourceTypeId],
			NULL [PreviousName],
			[Name] [NextName],
			NULL [PreviousUri],
			[Uri] [NextUri],
			NULL [PreviousDescription],
			[Description] [NextDescription],
			NULL [PreviousDataType],
			[DataType] [NextDataType],
			NULL [PreviousNullable],
			[Nullable] [NextNullable],
			NULL [PreviousMaxLength],
			[MaxLength] [NextMaxLength],
			NULL [PreviousScale],
			[Scale] [NextScale],
			NULL [PreviousPrecision],
			[Precision] [NextPrecision]
		FROM [cdc].[Core_ScalarProperty_CT] 
		WHERE  [__$operation] = 2 
			AND [__$start_lsn] = @ChangeSetId  

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr([__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr([__$seqval])) [SequenceNumber],
			1 [Operation],
			[Id] [ScalarPropertyId],
			[ResourceTypeId] [PreviousResourceTypeId],
			 NULL [NextResourceTypeId],
			[Name] [PreviousName],
			NULL [NextName],
			[Uri] [PreviousUri],
			NULL [NextUri],
			[Description] [PreviousDescription],
			NULL [NextDescription],
			[DataType] [PreviousDataType],
			NULL [NextDataType],
			[Nullable] [PreviousNullable],
			NULL [NextNullable],
			[MaxLength] [PreviousMaxLength],
			NULL [NextMaxLength],
			[Scale] [PreviousScale],
			NULL [NextScale],
			[Precision] [PreviousPrecision],
			NULL [NextPrecision]
		FROM [cdc].[Core_ScalarProperty_CT] 
		WHERE  [__$operation] = 1 
			AND [__$start_lsn] = @ChangeSetId  

		UNION ALL 

		SELECT NEWID(),
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$start_lsn])) [ChangeSetId],
			LOWER(master.dbo.fn_varbintohexstr( [Previous].[__$seqval])) [SequenceNumber],
			3 [Operation],
			[Previous].[Id] [ScalarPropertyId],
			[Previous].[ResourceTypeId] [PreviousResourceTypeId],
			[Next].[ResourceTypeId] [NextResourceTypeId],
			[Previous].[Name] [PreviousName],
			[Next].[Name] [NextName],
			[Previous].[Uri] [PreviousUri],
			[Next].[Uri] [NextUri],
			[Previous].[Description] [PreviousDescription],
			[Next].[Description] [NextDescription],
			[Previous].[DataType] [PreviousDataType],
			[Next].[DataType] [NextDataType],
			[Previous].[Nullable] [PreviousNullable],
			[Next].[Nullable] [NextNullable],
			[Previous].[MaxLength] [PreviousMaxLength],
			[Next].[MaxLength] [NextMaxLength],
			[Previous].[Scale] [PreviousScale],
			[Next].[Scale] [NextScale],
			[Previous].[Precision] [PreviousPrecision],
			[Next].[Precision] [NextPrecision]
		FROM  [cdc].[Core_ScalarProperty_CT] [Previous] 
		INNER JOIN [cdc].[Core_ScalarProperty_CT] [Next] 
			ON  [Previous].[__$seqval] = [Next].[__$seqval] 
				AND [Previous].[__$operation] = 3 
				AND [Next].[__$operation] = 4 
				AND [Previous].[__$start_lsn] = [Next].[__$start_lsn]  
				AND [Previous].[Id] = [Next].[Id] 
		WHERE  [Previous].[__$start_lsn] = @ChangeSetId    
	RETURN 0;
END
");

SET QUOTED_IDENTIFIER ON;
GO
/*--------------------------------------------------------------------------------------------------

 Summary: 
	This procedure processes the next available LSN in the [cdc].[lsn_time_mapping] table. LSNs are
	processed in the chronological order of their End Times. This procedure orders the entries by 
	End Time and then by the LSNs and picks the top entry to process.
 Parameters: 
	@Count - The number of changesets to process.
 Exceptions:
	None.
--------------------------------------------------------------------------------------------------*/
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"Administration.ProcessNextLSN",
"
CREATE PROCEDURE [Administration].[ProcessNextLSN]
	@Count int
AS
BEGIN
	
	DECLARE @LastLSN varbinary(64);
	DECLARE @LastDateCreated datetime;
	DECLARE @NextLSN varbinary(64);
	DECLARE @NextDateCreated datetime;
	DECLARE @ChangeSetId varbinary(64);
	DECLARE @LSNCursor CURSOR;
	
	-- Get the next set of LSNs to process. We subtract the Administration.ChangeSet
	-- table from cdc.lsn_time_mapping to get the next entries to process.
	SET @LSNCursor = CURSOR FOR
	SELECT TOP(@Count) [start_lsn], [tran_end_time] 
	FROM
	(
		SELECT [start_lsn], [tran_end_time] 
		FROM [cdc].lsn_time_mapping 
		WHERE tran_id <> 0x00
		EXCEPT
		SELECT Administration.fn_hexstrtovarbin([Id]) [start_lsn], [DateCreated] [tran_end_time] 
		FROM [Administration].ChangeSet
	)T
	ORDER BY [tran_end_time] ASC, [start_lsn] ASC;

	-- Start processing.
	OPEN @LSNCursor;
	FETCH NEXT FROM @LSNCursor INTO @NextLSN, @NextDateCreated;
	WHILE 0 = @@FETCH_STATUS
	BEGIN
		BEGIN TRY
			BEGIN TRANSACTION ProcessNextLSN;
				
				-- Treat next LSN as the changeset identifier.
				SET @ChangeSetId = @NextLSN;
				
				-- Create a changeset.
				INSERT INTO [Administration].[ChangeSet] 
				SELECT master.dbo.fn_varbintohexstr(@ChangeSetId), @NextDateCreated;
				
				-- Invoke the changeset processors for each CDC table.
				EXEC [Administration].[ProcessDataModelModuleChanges] @ChangeSetId;
				EXEC [Administration].[ProcessResourceTypeChanges] @ChangeSetId;
				EXEC [Administration].[ProcessAssociationChanges] @ChangeSetId;
				
				EXEC [Administration].[ProcessScalarPropertyChanges] @ChangeSetId;
				EXEC [Administration].[ProcessNavigationPropertyChanges] @ChangeSetId;
				
				EXEC [Administration].[ProcessPredicateChanges] @ChangeSetId;
				EXEC [Administration].[ProcessPropertyChanges] @ChangeSetId;
				EXEC [Administration].[ProcessRelationshipChanges] @ChangeSetId;
				EXEC [Administration].[ProcessPredicatePropertyChanges] @ChangeSetId;
				EXEC [Administration].[ProcessRelationshipPropertyChanges] @ChangeSetId;
				EXEC [Administration].[ProcessResourcePropertyChanges] @ChangeSetId;
				
				EXEC [Administration].[ProcessResourceChanges] @ChangeSetId;
			
			COMMIT TRANSACTION ProcessNextLSN;
		END TRY
		BEGIN CATCH
			IF EXISTS(SELECT 1 FROM sys.dm_tran_active_transactions 
				WHERE name=N'ProcessNextLSN')
					ROLLBACK TRANSACTION ProcessNextLSN;
			DECLARE @Msg nvarchar(4000), @NextLSNHexStr nvarchar(64);
			SELECT @Msg = ERROR_MESSAGE();
			SELECT @NextLSNHexStr = master.dbo.fn_varbintohexstr(@NextLSN);

			-- NOTE: We do not raise a fatal error, just return an informational message.
			RAISERROR(N'Error occurred while processing changeset [%s]. More details: %s', 
				10, 1, @NextLSNHexStr, @Msg);
		END CATCH
		
		FETCH NEXT FROM @LSNCursor INTO @NextLSN, @NextDateCreated;
	END
	
	CLOSE @LSNCursor;
	DEALLOCATE @LSNCursor;
	
	RETURN 0;
END
");

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"PopulateOperations",
"
IF NOT EXISTS(SELECT 1 FROM [Administration].[Operation] WHERE [Id] = 1)
	INSERT [Administration].[Operation] ([Id], [Name], [Description]) VALUES (1, N'Delete', NULL)
IF NOT EXISTS(SELECT 1 FROM [Administration].[Operation] WHERE [Id] = 2)
	INSERT [Administration].[Operation] ([Id], [Name], [Description]) VALUES (2, N'Insert', NULL)
IF NOT EXISTS(SELECT 1 FROM [Administration].[Operation] WHERE [Id] = 3)
	INSERT [Administration].[Operation] ([Id], [Name], [Description]) VALUES (3, N'Update', NULL)
");

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"CreateJobToProcessNextLSN",
"
IF EXISTS(SELECT 1 FROM msdb.dbo.sysjobs WHERE [name] = N'ProcessNextLSN')
BEGIN
	RAISERROR(N'Removing LSN processing job, [ProcessNextLSN].', 10, 1);
	EXEC msdb.dbo.sp_delete_job @job_name = N'ProcessNextLSN', @delete_unused_schedule=1
END

IF NOT EXISTS (SELECT 1 FROM msdb.dbo.syscategories WHERE name=N'Data Collector' AND category_class=1)
BEGIN
	RAISERROR(N'Creating category [Data Collector].', 10, 1);
	EXEC msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'Data Collector'
END

DECLARE @jobId BINARY(16)
RAISERROR(N'Creating LSN processing job, [ProcessNextLSN].', 10, 1);
EXEC  msdb.dbo.sp_add_job @job_name=N'ProcessNextLSN', 
		@enabled=1, 
		@notify_level_eventlog=2, 
		@notify_level_email=2, 
		@notify_level_netsend=2, 
		@notify_level_page=2, 
		@delete_level=0, 
		@category_name=N'Data Collector', 
		@owner_login_name=N'sa', @job_id = @jobId OUTPUT

EXEC msdb.dbo.sp_add_jobserver @job_name=N'ProcessNextLSN'

EXEC msdb.dbo.sp_add_jobstep @job_name=N'ProcessNextLSN', @step_name=N'ExecuteProcedure', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_fail_action=2, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'[Administration].ProcessNextLSN 100', 
		@database_name=N'#(DatabaseName)', 
		@flags=0

EXEC msdb.dbo.sp_update_job @job_name=N'ProcessNextLSN', 
		@enabled=1, 
		@start_step_id=1, 
		@notify_level_eventlog=2, 
		@notify_level_email=2, 
		@notify_level_netsend=2, 
		@notify_level_page=2, 
		@delete_level=0, 
		@description=N'', 
		@category_name=N'Data Collector', 
		@owner_login_name=N'sa', 
		@notify_email_operator_name=N'', 
		@notify_netsend_operator_name=N'', 
		@notify_page_operator_name=N''

DECLARE @schedule_id int
EXEC msdb.dbo.sp_add_jobschedule @job_name=N'ProcessNextLSN', @name=N'LSN Processing Schedule', 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=2, 
		@freq_subday_interval=10, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=1, 
		@active_start_date=20080725, 
		@active_end_date=99991231, 
		@active_start_time=0, 
		@active_end_time=235959, @schedule_id = @schedule_id OUTPUT
");

SET QUOTED_IDENTIFIER ON;
GO
-------------------------
-- Full Text Indexes
-------------------------
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"FullTextSearch.FileGroup",
"
	ALTER DATABASE [#(DatabaseName)]
	ADD FILEGROUP [FullTextSearch];
"
)

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"FullTextSearch.File",
"
	ALTER DATABASE [#(DatabaseName)]
	ADD FILE 
	(
		NAME = [#(FullTextSearch)],
		FILENAME = N'#(FullTextCatalogFilePath)',
		MAXSIZE = UNLIMITED
	) TO FILEGROUP [FullTextSearch];
"
)

SET QUOTED_IDENTIFIER ON;
GO
SET QUOTED_IDENTIFIER OFF;

INSERT INTO [Administration].[Command] (CommandName, CommandValue) VALUES
(
"ZentityCatalog.Catalog",
"
CREATE FULLTEXT CATALOG [ZentityCatalog] ON FILEGROUP [FullTextSearch] AS DEFAULT;
"
)

SET QUOTED_IDENTIFIER ON;
GO

GO

GO


GO
ALTER TABLE [Core].[Resource] ADD [c7a2b2e1-49b3-4ca0-a1a1-6e7b114007e4] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [3d82f5e2-b84a-4422-a0be-82317ad5059f] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [95d6858e-6294-4572-8686-c2c2c8dddd17] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [a23f94c6-6344-4ba6-ae90-63e49743a0d1] nvarchar(max) NULL
GO
ALTER TABLE [Core].[Resource] ADD [bc13df5d-58dc-4fc7-8010-ec7eb04ac0c0] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [ea645ca2-11e7-4746-af99-667274066311] nvarchar(50) NULL
GO
ALTER TABLE [Core].[Resource] ADD [a8f0994e-bc4e-4346-886d-9ffbde0940a8] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [7dc69c44-9efb-46b9-96e5-24b44e7a7f8b] nvarchar(4000) NULL
GO
ALTER TABLE [Core].[Resource] ADD [2b83b84e-b8d2-453a-89be-833e459d6bff] int NULL
GO
ALTER TABLE [Core].[Resource] ADD [49ec0c5e-1c52-4136-9966-9d437413c829] nvarchar(4000) NULL
GO
ALTER TABLE [Core].[Resource] ADD [0bb4e6f6-7d38-4773-a301-2c83810923e8] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [be528997-17b3-405c-a609-58bb560f36b9] nvarchar(4000) NULL
GO
ALTER TABLE [Core].[Resource] ADD [c82db7be-893e-4886-bbaa-63f530d5c960] nvarchar(4000) NULL
GO
ALTER TABLE [Core].[Resource] ADD [7df00974-90e1-4abb-816b-8da453b037e9] nvarchar(1024) NULL
GO
ALTER TABLE [Core].[Resource] ADD [97f83304-bcd9-45f6-9256-9bdc093c52b5] nvarchar(4000) NULL
GO
ALTER TABLE [Core].[Resource] ADD [b597448f-76f8-4663-8b6f-9c04f24194b5] nvarchar(4000) NULL
GO
ALTER TABLE [Core].[Resource] ADD [290fee5b-da0e-42a9-9d80-a10826572733] nvarchar(4000) NULL
GO
ALTER TABLE [Core].[Resource] ADD [4283e5a5-96fe-47fe-94be-de1f0caa9f76] nvarchar(128) NULL
GO
ALTER TABLE [Core].[Resource] ADD [9fd938f9-da4a-4541-bcbe-fc6edb85cc2c] nvarchar(4000) NULL
GO
ALTER TABLE [Core].[Resource] ADD [4b96b254-d594-4576-a6ab-ff90938d6910] datetime NULL
GO
ALTER TABLE [Core].[Resource] ADD [4326eb60-7271-43a5-9436-2d0766c353b9] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [158a2898-edec-47d8-937a-b37269df2aaa] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [eaa610b7-cf35-4393-bf19-0fb7af9a128a] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [2af50485-aa00-4c0a-9d2c-381720b7c7b3] varbinary(max) NULL
GO
ALTER TABLE [Core].[Resource] ADD [25b52405-1f7a-4c9a-b904-591d4e29c73c] nvarchar(4000) NULL
GO
ALTER TABLE [Core].[Resource] ADD [67cbb24d-0c60-4b65-adfa-7f6c60ad5935] nvarchar(1024) NULL
GO
ALTER TABLE [Core].[Resource] ADD [02dbf59e-ff45-41e8-9320-a702ebdadba7] datetime NULL
GO
ALTER TABLE [Core].[Resource] ADD [22e98699-e5c6-4466-af64-be9dc28d7dc5] datetime NULL
GO
ALTER TABLE [Core].[Resource] ADD [5108112d-29f5-4421-8bac-058f2f5bff4e] bit NULL
GO
ALTER TABLE [Core].[Resource] ADD [a2733469-7db1-4a5b-9316-0deec2623e67] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [86f59c02-d498-47d2-81dc-2a52138aadab] decimal(18, 0) NULL
GO
ALTER TABLE [Core].[Resource] ADD [dd35d7a1-7468-4625-9133-2b90b7e07251] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [9fc89788-3c77-43a5-88cc-2d61e65c492d] nvarchar(4000) NULL
GO
ALTER TABLE [Core].[Resource] ADD [07cfd378-9a6a-4c04-93ad-52f243f79c74] int NULL
GO
ALTER TABLE [Core].[Resource] ADD [cbde5c20-64bf-4c43-b354-6fe6a4cf362e] nvarchar(64) NULL
GO
ALTER TABLE [Core].[Resource] ADD [2cc4abc7-b19e-46e1-9438-755742a5946c] int NULL
GO
ALTER TABLE [Core].[Resource] ADD [14c68cc9-d2ef-4d4b-9f79-8c84e27d2813] int NULL
GO
ALTER TABLE [Core].[Resource] ADD [f01210b7-d617-4727-8525-b3f1360f8d78] smallint NULL
GO
ALTER TABLE [Core].[Resource] ADD [d1a6e5d0-0405-441f-b4fb-bd41c2bbe96d] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [2f0f3d7c-8e53-4c74-9418-c53e4b5f3a3a] smallint NULL
GO
ALTER TABLE [Core].[Resource] ADD [51426e9f-5576-4ae9-8bd0-fb954a75eb1c] nvarchar(64) NULL
GO
ALTER TABLE [Core].[Resource] ADD [e7fa1d49-3936-4ec3-9414-058909d19c83] nvarchar(2048) NULL
GO
ALTER TABLE [Core].[Resource] ADD [bdacb3bf-7ba4-499d-a199-9b49e24a022a] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [6b8b39ac-1f83-48d0-8a06-259a6cc55742] nvarchar(4000) NULL
GO
ALTER TABLE [Core].[Resource] ADD [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b] datetime NULL
GO
ALTER TABLE [Core].[Resource] ADD [3e958c5f-bd06-470a-8f90-324eca205160] nvarchar(max) NULL
GO
ALTER TABLE [Core].[Resource] ADD [d68071c0-2560-4368-b41b-6a825000ec07] nvarchar(max) NULL
GO
ALTER TABLE [Core].[Resource] ADD [bec19f51-7fe7-45a8-9a7a-88b008ade630] nvarchar(4000) NULL
GO
ALTER TABLE [Core].[Resource] ADD [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b] datetime NULL
GO
ALTER TABLE [Core].[Resource] ADD [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd] nvarchar(128) NULL
GO
ALTER TABLE [Core].[Resource] ADD [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519] datetime NULL
GO
ALTER TABLE [Core].[Resource] ADD [54104144-2aba-4d7c-af53-f9e06deb66ba] datetime NULL
GO
ALTER TABLE [Core].[Resource] ADD [5e27e1b3-1965-44ee-8787-fbbdbc2f300d] datetime NULL
GO
ALTER TABLE [Core].[Resource] ADD [3851bd83-4ac5-4b9b-8e3c-a16229925ff8] nvarchar(1024) NULL
GO
ALTER TABLE [Core].[Resource] ADD [c051a74d-af00-45f1-8ef8-3c19bd14322c] datetime NULL
GO
ALTER TABLE [Core].[Resource] ADD [a8936969-6719-4639-b914-f271233d02c6] datetime NULL
GO
ALTER TABLE [Core].[Resource] ADD [96302933-01ef-41b4-8399-7f946dd0d07f] nvarchar(1024) NULL
GO
ALTER TABLE [Core].[Resource] ADD [65db5eed-c9fd-452a-a289-a6034f072d9b] datetime NULL
GO
ALTER TABLE [Core].[Resource] ADD [496bd2ef-295d-4801-892d-b38e8338e6ff] nvarchar(4000) NULL
GO
ALTER TABLE [Core].[Resource] ADD [165e8189-f6a3-42c6-ba9f-bddaa81364df] nvarchar(128) NULL
GO
ALTER TABLE [Core].[Resource] ADD [42bed4bc-be0a-4d4a-bb9a-679352689f48] nvarchar(max) NULL
GO
ALTER TABLE [Core].[Resource] ADD [9f4c2262-8882-4a9f-a0bd-875dea047157] nvarchar(4000) NULL
GO
ALTER TABLE [Core].[Resource] ADD [0d977578-3e24-42d8-840f-e52a7e715830] nvarchar(max) NULL
GO
ALTER TABLE [Core].[Resource] ADD [a0d288c5-a9ee-4872-b7f4-f815089037b7] nvarchar(1024) NULL
GO
ALTER TABLE [Core].[Resource] ADD [09212252-409f-43fd-a6e1-3b0213423950] nvarchar(4000) NULL
GO
ALTER TABLE [Core].[Resource] ADD [2b8e690c-44c7-4670-b147-06b7b18d89f3] nvarchar(4000) NULL
GO
ALTER TABLE [Core].[Resource] ADD [3c35c424-0f62-44e5-82fe-2755a9eff1c2] nvarchar(4000) NULL
GO
ALTER TABLE [Core].[Resource] ADD [e94f81aa-1993-4726-9447-5ee237e58654] datetime NULL
GO
ALTER TABLE [Core].[Resource] ADD [56b9acf3-9e10-424e-a9f0-a6f5aab12a50] int NULL
GO
ALTER TABLE [Core].[Resource] ADD [3d773ba6-1955-47c4-9388-baef50b70327] nvarchar(128) NULL
GO
ALTER TABLE [Core].[Resource] ADD [6ac95912-620a-4aeb-83e9-0ced9a61d22a] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [80a0d8df-d9a7-488b-85fe-062bac9418f2] datetime NULL
GO
ALTER TABLE [Core].[Resource] ADD [e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [3f6b345a-d2f2-4cb1-a7b5-2a316854adc6] nvarchar(1024) NULL
GO
ALTER TABLE [Core].[Resource] ADD [e5989779-7dff-46c6-90e1-3a8be8c6d07b] nvarchar(1024) NULL
GO
ALTER TABLE [Core].[Resource] ADD [ecc77bac-77c4-4eb7-80db-3cd6821c8628] int NULL
GO
ALTER TABLE [Core].[Resource] ADD [9d3790de-e0e0-459b-8605-52b9f1470301] nvarchar(1024) NULL
GO
ALTER TABLE [Core].[Resource] ADD [32bc2d19-22ed-4514-bfed-7b85353dc82e] nvarchar(1024) NULL
GO
ALTER TABLE [Core].[Resource] ADD [05d783c7-5517-4fbc-88b9-8e010e3f7bb3] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [6db26d3c-a477-4148-a527-8e331bf46619] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [441aeefc-94b4-4c5a-839f-962ff15a0624] nvarchar(1024) NULL
GO
ALTER TABLE [Core].[Resource] ADD [b888a64d-4555-4d1d-a422-99a1511b1f65] nvarchar(1024) NULL
GO
ALTER TABLE [Core].[Resource] ADD [03163fdf-9d73-40bf-9120-ae22e2c3407c] datetime NULL
GO
ALTER TABLE [Core].[Resource] ADD [b686fec9-0fdf-46f6-b5aa-b1441bad9ea5] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [1cef5926-ef98-4170-94d6-db4326671534] datetime NULL
GO
ALTER TABLE [Core].[Resource] ADD [62f20793-f341-42ad-8980-e358c6037763] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [8db90ced-fd85-4ba7-9a9f-e61bb36cddca] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [95d00191-ec8a-4bef-a24e-ea0d7d132bfb] int NULL
GO
ALTER TABLE [Core].[Resource] ADD [92d45d1c-821b-4869-872b-f4355150a145] int NULL
GO
ALTER TABLE [Core].[Resource] ADD [fd227620-fc28-4539-803e-f9390a8fe329] nvarchar(1024) NULL
GO
ALTER TABLE [Core].[Resource] ADD [08bc5fa6-16e7-4b98-813f-fbe233736d6a] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [22a62585-520b-44e2-9194-80b09e3e5c8d] nvarchar(256) NULL
GO
ALTER TABLE [Core].[Resource] ADD [7ac21049-f4c2-42d5-bd20-21a2563d4eef] datetime NULL
GO
ALTER TABLE [Core].[Resource] ADD [0e2d2490-c627-493f-9a4c-fc9c306e51c5] bigint NULL
GO
ALTER TABLE [Core].[Resource] ADD [54f98dcb-c008-4e66-9c76-353a1a48edac] int NULL
GO
CREATE VIEW [Core].[1915314262f84ef0ad9cc8e9183178ed]
WITH SCHEMABINDING
AS
	SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = '76a9cb80-fb24-4249-8c09-194f0acfc895';
GO
CREATE UNIQUE CLUSTERED INDEX [PK_1915314262f84ef0ad9cc8e9183178ed] ON [Core].[1915314262f84ef0ad9cc8e9183178ed] 
(
	[SubjectResourceId], [ObjectResourceId]
)
GO
CREATE PROCEDURE [Core].[Insert1915314262f84ef0ad9cc8e9183178ed]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO [Core].[Relationship]
	(	
		[Id],
		[SubjectResourceId],
		[ObjectResourceId],
		[PredicateId]
	)
	VALUES
	(
		NEWID(),
		@SubjectResourceId, 
		@ObjectResourceId, 
		'76a9cb80-fb24-4249-8c09-194f0acfc895'
	) 
END
GO
CREATE PROCEDURE [Core].[Delete1915314262f84ef0ad9cc8e9183178ed]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = '76a9cb80-fb24-4249-8c09-194f0acfc895';
END
GO
CREATE VIEW [Core].[0a62762a195841baaa3c8abd8a4a5aee]
WITH SCHEMABINDING
AS
	SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = 'a5781cc2-dd1b-4355-9a9b-4675103a2e7f';
GO
CREATE UNIQUE CLUSTERED INDEX [PK_0a62762a195841baaa3c8abd8a4a5aee] ON [Core].[0a62762a195841baaa3c8abd8a4a5aee] 
(
	[SubjectResourceId], [ObjectResourceId]
)
GO
CREATE PROCEDURE [Core].[Insert0a62762a195841baaa3c8abd8a4a5aee]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO [Core].[Relationship]
	(	
		[Id],
		[SubjectResourceId],
		[ObjectResourceId],
		[PredicateId]
	)
	VALUES
	(
		NEWID(),
		@SubjectResourceId, 
		@ObjectResourceId, 
		'a5781cc2-dd1b-4355-9a9b-4675103a2e7f'
	) 
END
GO
CREATE PROCEDURE [Core].[Delete0a62762a195841baaa3c8abd8a4a5aee]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = 'a5781cc2-dd1b-4355-9a9b-4675103a2e7f';
END
GO
CREATE VIEW [Core].[456369b6732c4b31861898d65f5001c8]
WITH SCHEMABINDING
AS
	SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = '5bab8f81-6171-4fb5-be31-ddc37a97a918';
GO
CREATE UNIQUE CLUSTERED INDEX [PK_456369b6732c4b31861898d65f5001c8] ON [Core].[456369b6732c4b31861898d65f5001c8] 
(
	[SubjectResourceId], [ObjectResourceId]
)
GO
CREATE PROCEDURE [Core].[Insert456369b6732c4b31861898d65f5001c8]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO [Core].[Relationship]
	(	
		[Id],
		[SubjectResourceId],
		[ObjectResourceId],
		[PredicateId]
	)
	VALUES
	(
		NEWID(),
		@SubjectResourceId, 
		@ObjectResourceId, 
		'5bab8f81-6171-4fb5-be31-ddc37a97a918'
	) 
END
GO
CREATE PROCEDURE [Core].[Delete456369b6732c4b31861898d65f5001c8]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = '5bab8f81-6171-4fb5-be31-ddc37a97a918';
END
GO
CREATE VIEW [Core].[fb6e0e136a5949a99c1c12e5157a3a21]
WITH SCHEMABINDING
AS
	SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = '7015f649-1145-4036-a6a2-602a1e0625cb';
GO
CREATE UNIQUE CLUSTERED INDEX [PK_fb6e0e136a5949a99c1c12e5157a3a21] ON [Core].[fb6e0e136a5949a99c1c12e5157a3a21] 
(
	[SubjectResourceId], [ObjectResourceId]
)
GO
CREATE PROCEDURE [Core].[Insertfb6e0e136a5949a99c1c12e5157a3a21]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO [Core].[Relationship]
	(	
		[Id],
		[SubjectResourceId],
		[ObjectResourceId],
		[PredicateId]
	)
	VALUES
	(
		NEWID(),
		@SubjectResourceId, 
		@ObjectResourceId, 
		'7015f649-1145-4036-a6a2-602a1e0625cb'
	) 
END
GO
CREATE PROCEDURE [Core].[Deletefb6e0e136a5949a99c1c12e5157a3a21]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = '7015f649-1145-4036-a6a2-602a1e0625cb';
END
GO
CREATE VIEW [Core].[2664cf484ae544ac97351fba700e3e99]
WITH SCHEMABINDING
AS
	SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = '97cf909f-07fa-4cdf-8d5d-e8a2ed03b6a5';
GO
CREATE UNIQUE CLUSTERED INDEX [PK_2664cf484ae544ac97351fba700e3e99] ON [Core].[2664cf484ae544ac97351fba700e3e99] 
(
	[SubjectResourceId], [ObjectResourceId]
)
GO
CREATE PROCEDURE [Core].[Insert2664cf484ae544ac97351fba700e3e99]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO [Core].[Relationship]
	(	
		[Id],
		[SubjectResourceId],
		[ObjectResourceId],
		[PredicateId]
	)
	VALUES
	(
		NEWID(),
		@SubjectResourceId, 
		@ObjectResourceId, 
		'97cf909f-07fa-4cdf-8d5d-e8a2ed03b6a5'
	) 
END
GO
CREATE PROCEDURE [Core].[Delete2664cf484ae544ac97351fba700e3e99]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = '97cf909f-07fa-4cdf-8d5d-e8a2ed03b6a5';
END
GO
CREATE VIEW [Core].[003876b665834574a53a2069587ac10e]
WITH SCHEMABINDING
AS
	SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = '51255ad0-9daa-4f4a-b804-2eec3c52462f';
GO
CREATE UNIQUE CLUSTERED INDEX [PK_003876b665834574a53a2069587ac10e] ON [Core].[003876b665834574a53a2069587ac10e] 
(
	[SubjectResourceId], [ObjectResourceId]
)
GO
CREATE PROCEDURE [Core].[Insert003876b665834574a53a2069587ac10e]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO [Core].[Relationship]
	(	
		[Id],
		[SubjectResourceId],
		[ObjectResourceId],
		[PredicateId]
	)
	VALUES
	(
		NEWID(),
		@SubjectResourceId, 
		@ObjectResourceId, 
		'51255ad0-9daa-4f4a-b804-2eec3c52462f'
	) 
END
GO
CREATE PROCEDURE [Core].[Delete003876b665834574a53a2069587ac10e]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = '51255ad0-9daa-4f4a-b804-2eec3c52462f';
END
GO
CREATE VIEW [Core].[aab520cc086440e6a4ecb66dfc521c54]
WITH SCHEMABINDING
AS
	SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = 'fba74167-4dad-4cc7-8232-b0f64422c120';
GO
CREATE UNIQUE CLUSTERED INDEX [PK_aab520cc086440e6a4ecb66dfc521c54] ON [Core].[aab520cc086440e6a4ecb66dfc521c54] 
(
	[SubjectResourceId], [ObjectResourceId]
)
GO
CREATE PROCEDURE [Core].[Insertaab520cc086440e6a4ecb66dfc521c54]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO [Core].[Relationship]
	(	
		[Id],
		[SubjectResourceId],
		[ObjectResourceId],
		[PredicateId]
	)
	VALUES
	(
		NEWID(),
		@SubjectResourceId, 
		@ObjectResourceId, 
		'fba74167-4dad-4cc7-8232-b0f64422c120'
	) 
END
GO
CREATE PROCEDURE [Core].[Deleteaab520cc086440e6a4ecb66dfc521c54]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = 'fba74167-4dad-4cc7-8232-b0f64422c120';
END
GO
CREATE VIEW [Core].[8b00027db4f043c9bb32406fb5195a4a]
WITH SCHEMABINDING
AS
	SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = 'e64f1bb5-d8b5-45c7-b3ff-5606c94e10fb';
GO
CREATE UNIQUE CLUSTERED INDEX [PK_8b00027db4f043c9bb32406fb5195a4a] ON [Core].[8b00027db4f043c9bb32406fb5195a4a] 
(
	[SubjectResourceId], [ObjectResourceId]
)
GO
CREATE PROCEDURE [Core].[Insert8b00027db4f043c9bb32406fb5195a4a]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO [Core].[Relationship]
	(	
		[Id],
		[SubjectResourceId],
		[ObjectResourceId],
		[PredicateId]
	)
	VALUES
	(
		NEWID(),
		@SubjectResourceId, 
		@ObjectResourceId, 
		'e64f1bb5-d8b5-45c7-b3ff-5606c94e10fb'
	) 
END
GO
CREATE PROCEDURE [Core].[Delete8b00027db4f043c9bb32406fb5195a4a]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = 'e64f1bb5-d8b5-45c7-b3ff-5606c94e10fb';
END
GO
CREATE VIEW [Core].[62e7627ae3284164b8e68968f18470fa]
WITH SCHEMABINDING
AS
	SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = '919b79c8-fd22-4fa9-a040-a005865cc56b';
GO
CREATE UNIQUE CLUSTERED INDEX [PK_62e7627ae3284164b8e68968f18470fa] ON [Core].[62e7627ae3284164b8e68968f18470fa] 
(
	[SubjectResourceId], [ObjectResourceId]
)
GO
CREATE PROCEDURE [Core].[Insert62e7627ae3284164b8e68968f18470fa]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO [Core].[Relationship]
	(	
		[Id],
		[SubjectResourceId],
		[ObjectResourceId],
		[PredicateId]
	)
	VALUES
	(
		NEWID(),
		@SubjectResourceId, 
		@ObjectResourceId, 
		'919b79c8-fd22-4fa9-a040-a005865cc56b'
	) 
END
GO
CREATE PROCEDURE [Core].[Delete62e7627ae3284164b8e68968f18470fa]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = '919b79c8-fd22-4fa9-a040-a005865cc56b';
END
GO
CREATE VIEW [Core].[bf151136741745deb68c504bd2d71dc0]
WITH SCHEMABINDING
AS
	SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = '19476c3c-af6e-404e-9df2-ac467addbd52';
GO
CREATE UNIQUE CLUSTERED INDEX [PK_bf151136741745deb68c504bd2d71dc0] ON [Core].[bf151136741745deb68c504bd2d71dc0] 
(
	[SubjectResourceId], [ObjectResourceId]
)
GO
CREATE PROCEDURE [Core].[Insertbf151136741745deb68c504bd2d71dc0]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO [Core].[Relationship]
	(	
		[Id],
		[SubjectResourceId],
		[ObjectResourceId],
		[PredicateId]
	)
	VALUES
	(
		NEWID(),
		@SubjectResourceId, 
		@ObjectResourceId, 
		'19476c3c-af6e-404e-9df2-ac467addbd52'
	) 
END
GO
CREATE PROCEDURE [Core].[Deletebf151136741745deb68c504bd2d71dc0]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = '19476c3c-af6e-404e-9df2-ac467addbd52';
END
GO
CREATE VIEW [Core].[81c58166ced94ce18eb3a1f6a0dd3d1c]
WITH SCHEMABINDING
AS
	SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = 'c6f8270c-db09-40d8-90c8-26efccd465fe';
GO
CREATE UNIQUE CLUSTERED INDEX [PK_81c58166ced94ce18eb3a1f6a0dd3d1c] ON [Core].[81c58166ced94ce18eb3a1f6a0dd3d1c] 
(
	[SubjectResourceId], [ObjectResourceId]
)
GO
CREATE PROCEDURE [Core].[Insert81c58166ced94ce18eb3a1f6a0dd3d1c]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO [Core].[Relationship]
	(	
		[Id],
		[SubjectResourceId],
		[ObjectResourceId],
		[PredicateId]
	)
	VALUES
	(
		NEWID(),
		@SubjectResourceId, 
		@ObjectResourceId, 
		'c6f8270c-db09-40d8-90c8-26efccd465fe'
	) 
END
GO
CREATE PROCEDURE [Core].[Delete81c58166ced94ce18eb3a1f6a0dd3d1c]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = 'c6f8270c-db09-40d8-90c8-26efccd465fe';
END
GO
CREATE VIEW [Core].[4377b424756d40a5a207840ae5db5d94]
WITH SCHEMABINDING
AS
	SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = '7fb9e42f-f233-492c-8eb2-f0ee34223ec4';
GO
CREATE UNIQUE CLUSTERED INDEX [PK_4377b424756d40a5a207840ae5db5d94] ON [Core].[4377b424756d40a5a207840ae5db5d94] 
(
	[SubjectResourceId], [ObjectResourceId]
)
GO
CREATE PROCEDURE [Core].[Insert4377b424756d40a5a207840ae5db5d94]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO [Core].[Relationship]
	(	
		[Id],
		[SubjectResourceId],
		[ObjectResourceId],
		[PredicateId]
	)
	VALUES
	(
		NEWID(),
		@SubjectResourceId, 
		@ObjectResourceId, 
		'7fb9e42f-f233-492c-8eb2-f0ee34223ec4'
	) 
END
GO
CREATE PROCEDURE [Core].[Delete4377b424756d40a5a207840ae5db5d94]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = '7fb9e42f-f233-492c-8eb2-f0ee34223ec4';
END
GO
CREATE VIEW [Core].[cd10127c8b574de4a7b7368c66976ef5]
WITH SCHEMABINDING
AS
	SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = '79c66c76-e748-4c9f-8337-59eabf27ab47';
GO
CREATE UNIQUE CLUSTERED INDEX [PK_cd10127c8b574de4a7b7368c66976ef5] ON [Core].[cd10127c8b574de4a7b7368c66976ef5] 
(
	[SubjectResourceId], [ObjectResourceId]
)
GO
CREATE PROCEDURE [Core].[Insertcd10127c8b574de4a7b7368c66976ef5]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO [Core].[Relationship]
	(	
		[Id],
		[SubjectResourceId],
		[ObjectResourceId],
		[PredicateId]
	)
	VALUES
	(
		NEWID(),
		@SubjectResourceId, 
		@ObjectResourceId, 
		'79c66c76-e748-4c9f-8337-59eabf27ab47'
	) 
END
GO
CREATE PROCEDURE [Core].[Deletecd10127c8b574de4a7b7368c66976ef5]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = '79c66c76-e748-4c9f-8337-59eabf27ab47';
END
GO
CREATE VIEW [Core].[0e451d98427849b6ada5fda9c0c922d5]
WITH SCHEMABINDING
AS
	SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = 'b20eb638-a601-4d1b-9c5b-81d3b4ba8095';
GO
CREATE UNIQUE CLUSTERED INDEX [PK_0e451d98427849b6ada5fda9c0c922d5] ON [Core].[0e451d98427849b6ada5fda9c0c922d5] 
(
	[ObjectResourceId]
)
GO
CREATE PROCEDURE [Core].[Insert0e451d98427849b6ada5fda9c0c922d5]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO [Core].[Relationship]
	(	
		[Id],
		[SubjectResourceId],
		[ObjectResourceId],
		[PredicateId]
	)
	VALUES
	(
		NEWID(),
		@SubjectResourceId, 
		@ObjectResourceId, 
		'b20eb638-a601-4d1b-9c5b-81d3b4ba8095'
	) 
END
GO
CREATE PROCEDURE [Core].[Delete0e451d98427849b6ada5fda9c0c922d5]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = 'b20eb638-a601-4d1b-9c5b-81d3b4ba8095';
END
GO
CREATE VIEW [Core].[5c9496a95d824150ab1432b00d6f6869]
WITH SCHEMABINDING
AS
	SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = 'ae40bd66-d25e-408e-be5d-237224e33297';
GO
CREATE UNIQUE CLUSTERED INDEX [PK_5c9496a95d824150ab1432b00d6f6869] ON [Core].[5c9496a95d824150ab1432b00d6f6869] 
(
	[SubjectResourceId], [ObjectResourceId]
)
GO
CREATE PROCEDURE [Core].[Insert5c9496a95d824150ab1432b00d6f6869]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO [Core].[Relationship]
	(	
		[Id],
		[SubjectResourceId],
		[ObjectResourceId],
		[PredicateId]
	)
	VALUES
	(
		NEWID(),
		@SubjectResourceId, 
		@ObjectResourceId, 
		'ae40bd66-d25e-408e-be5d-237224e33297'
	) 
END
GO
CREATE PROCEDURE [Core].[Delete5c9496a95d824150ab1432b00d6f6869]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = 'ae40bd66-d25e-408e-be5d-237224e33297';
END
GO
CREATE VIEW [Core].[e5f7c76f5be5456ab097611295a14d63]
WITH SCHEMABINDING
AS
	SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = '45cc949e-5490-42a8-9b62-9a2f0c17fe93';
GO
CREATE UNIQUE CLUSTERED INDEX [PK_e5f7c76f5be5456ab097611295a14d63] ON [Core].[e5f7c76f5be5456ab097611295a14d63] 
(
	[ObjectResourceId]
)
GO
CREATE PROCEDURE [Core].[Inserte5f7c76f5be5456ab097611295a14d63]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO [Core].[Relationship]
	(	
		[Id],
		[SubjectResourceId],
		[ObjectResourceId],
		[PredicateId]
	)
	VALUES
	(
		NEWID(),
		@SubjectResourceId, 
		@ObjectResourceId, 
		'45cc949e-5490-42a8-9b62-9a2f0c17fe93'
	) 
END
GO
CREATE PROCEDURE [Core].[Deletee5f7c76f5be5456ab097611295a14d63]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = '45cc949e-5490-42a8-9b62-9a2f0c17fe93';
END
GO
CREATE PROCEDURE [Core].[Insert3916037e40ee4eefa4ee0b071217c266]
 @From nvarchar(1024)
, @DateExchanged datetime
, @To nvarchar(4000)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[96302933-01ef-41b4-8399-7f946dd0d07f], 
[65db5eed-c9fd-452a-a289-a6034f072d9b], 
[496bd2ef-295d-4801-892d-b38e8338e6ff], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@From, 
@DateExchanged, 
@To, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'3916037e-40ee-4eef-a4ee-0b071217c266', 
3

);


END
GO
CREATE PROCEDURE [Core].[Update3916037e40ee4eefa4ee0b071217c266]
 @From nvarchar(1024)
, @DateExchanged datetime
, @To nvarchar(4000)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [96302933-01ef-41b4-8399-7f946dd0d07f]  =  @From
, [65db5eed-c9fd-452a-a289-a6034f072d9b]  =  @DateExchanged
, [496bd2ef-295d-4801-892d-b38e8338e6ff]  =  @To
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete3916037e40ee4eefa4ee0b071217c266]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Inserte82b8970583f42b583c40ff7135bb0e7]
 @License nvarchar(4000)
, @Copyright nvarchar(4000)
, @DateCopyrighted datetime
, @Duration int
, @Language nvarchar(128)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[2b8e690c-44c7-4670-b147-06b7b18d89f3], 
[3c35c424-0f62-44e5-82fe-2755a9eff1c2], 
[e94f81aa-1993-4726-9447-5ee237e58654], 
[56b9acf3-9e10-424e-a9f0-a6f5aab12a50], 
[3d773ba6-1955-47c4-9388-baef50b70327], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@License, 
@Copyright, 
@DateCopyrighted, 
@Duration, 
@Language, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'e82b8970-583f-42b5-83c4-0ff7135bb0e7', 
4

);


END
GO
CREATE PROCEDURE [Core].[Updatee82b8970583f42b583c40ff7135bb0e7]
 @License nvarchar(4000)
, @Copyright nvarchar(4000)
, @DateCopyrighted datetime
, @Duration int
, @Language nvarchar(128)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [2b8e690c-44c7-4670-b147-06b7b18d89f3]  =  @License
, [3c35c424-0f62-44e5-82fe-2755a9eff1c2]  =  @Copyright
, [e94f81aa-1993-4726-9447-5ee237e58654]  =  @DateCopyrighted
, [56b9acf3-9e10-424e-a9f0-a6f5aab12a50]  =  @Duration
, [3d773ba6-1955-47c4-9388-baef50b70327]  =  @Language
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deletee82b8970583f42b583c40ff7135bb0e7]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert346958a32ccb4bdda8101085c2ce3e65]
 @VersionInformation nvarchar(256)
, @Copyright nvarchar(4000)
, @HardwareRequirements nvarchar(4000)
, @EULA nvarchar(1024)
, @OperatingSystem nvarchar(4000)
, @DownloadRequirements nvarchar(4000)
, @SystemRequirements nvarchar(4000)
, @Language nvarchar(128)
, @License nvarchar(4000)
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[0bb4e6f6-7d38-4773-a301-2c83810923e8], 
[be528997-17b3-405c-a609-58bb560f36b9], 
[c82db7be-893e-4886-bbaa-63f530d5c960], 
[7df00974-90e1-4abb-816b-8da453b037e9], 
[97f83304-bcd9-45f6-9256-9bdc093c52b5], 
[b597448f-76f8-4663-8b6f-9c04f24194b5], 
[290fee5b-da0e-42a9-9d80-a10826572733], 
[4283e5a5-96fe-47fe-94be-de1f0caa9f76], 
[9fd938f9-da4a-4541-bcbe-fc6edb85cc2c], 
[4b96b254-d594-4576-a6ab-ff90938d6910], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@VersionInformation, 
@Copyright, 
@HardwareRequirements, 
@EULA, 
@OperatingSystem, 
@DownloadRequirements, 
@SystemRequirements, 
@Language, 
@License, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'346958a3-2ccb-4bdd-a810-1085c2ce3e65', 
5

);


END
GO
CREATE PROCEDURE [Core].[Update346958a32ccb4bdda8101085c2ce3e65]
 @VersionInformation nvarchar(256)
, @Copyright nvarchar(4000)
, @HardwareRequirements nvarchar(4000)
, @EULA nvarchar(1024)
, @OperatingSystem nvarchar(4000)
, @DownloadRequirements nvarchar(4000)
, @SystemRequirements nvarchar(4000)
, @Language nvarchar(128)
, @License nvarchar(4000)
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [0bb4e6f6-7d38-4773-a301-2c83810923e8]  =  @VersionInformation
, [be528997-17b3-405c-a609-58bb560f36b9]  =  @Copyright
, [c82db7be-893e-4886-bbaa-63f530d5c960]  =  @HardwareRequirements
, [7df00974-90e1-4abb-816b-8da453b037e9]  =  @EULA
, [97f83304-bcd9-45f6-9256-9bdc093c52b5]  =  @OperatingSystem
, [b597448f-76f8-4663-8b6f-9c04f24194b5]  =  @DownloadRequirements
, [290fee5b-da0e-42a9-9d80-a10826572733]  =  @SystemRequirements
, [4283e5a5-96fe-47fe-94be-de1f0caa9f76]  =  @Language
, [9fd938f9-da4a-4541-bcbe-fc6edb85cc2c]  =  @License
, [4b96b254-d594-4576-a6ab-ff90938d6910]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete346958a32ccb4bdda8101085c2ce3e65]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert479452ef4aac4f1f8e0012bf54cce9a9]
 @Email nvarchar(2048)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[e7fa1d49-3936-4ec3-9414-058909d19c83], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@Email, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'479452ef-4aac-4f1f-8e00-12bf54cce9a9', 
6

);


END
GO
CREATE PROCEDURE [Core].[Update479452ef4aac4f1f8e0012bf54cce9a9]
 @Email nvarchar(2048)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [e7fa1d49-3936-4ec3-9414-058909d19c83]  =  @Email
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete479452ef4aac4f1f8e0012bf54cce9a9]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert0e3efa4a7bfa471cb7bd17685cdef487]
 @LastName nvarchar(256)
, @FirstName nvarchar(256)
, @MiddleName nvarchar(256)
, @Email nvarchar(2048)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[c7a2b2e1-49b3-4ca0-a1a1-6e7b114007e4], 
[3d82f5e2-b84a-4422-a0be-82317ad5059f], 
[95d6858e-6294-4572-8686-c2c2c8dddd17], 
[e7fa1d49-3936-4ec3-9414-058909d19c83], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@LastName, 
@FirstName, 
@MiddleName, 
@Email, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'0e3efa4a-7bfa-471c-b7bd-17685cdef487', 
7

);


END
GO
CREATE PROCEDURE [Core].[Update0e3efa4a7bfa471cb7bd17685cdef487]
 @LastName nvarchar(256)
, @FirstName nvarchar(256)
, @MiddleName nvarchar(256)
, @Email nvarchar(2048)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [c7a2b2e1-49b3-4ca0-a1a1-6e7b114007e4]  =  @LastName
, [3d82f5e2-b84a-4422-a0be-82317ad5059f]  =  @FirstName
, [95d6858e-6294-4572-8686-c2c2c8dddd17]  =  @MiddleName
, [e7fa1d49-3936-4ec3-9414-058909d19c83]  =  @Email
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete0e3efa4a7bfa471cb7bd17685cdef487]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert23d6f2a9381944de9cf01eb683141dfe]
 @ChangeHistory nvarchar(max)
, @ISBN nvarchar(256)
, @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[a23f94c6-6344-4ba6-ae90-63e49743a0d1], 
[bc13df5d-58dc-4fc7-8010-ec7eb04ac0c0], 
[80a0d8df-d9a7-488b-85fe-062bac9418f2], 
[e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6], 
[3f6b345a-d2f2-4cb1-a7b5-2a316854adc6], 
[e5989779-7dff-46c6-90e1-3a8be8c6d07b], 
[ecc77bac-77c4-4eb7-80db-3cd6821c8628], 
[9d3790de-e0e0-459b-8605-52b9f1470301], 
[32bc2d19-22ed-4514-bfed-7b85353dc82e], 
[05d783c7-5517-4fbc-88b9-8e010e3f7bb3], 
[6db26d3c-a477-4148-a527-8e331bf46619], 
[441aeefc-94b4-4c5a-839f-962ff15a0624], 
[b888a64d-4555-4d1d-a422-99a1511b1f65], 
[03163fdf-9d73-40bf-9120-ae22e2c3407c], 
[b686fec9-0fdf-46f6-b5aa-b1441bad9ea5], 
[9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2], 
[1cef5926-ef98-4170-94d6-db4326671534], 
[62f20793-f341-42ad-8980-e358c6037763], 
[8db90ced-fd85-4ba7-9a9f-e61bb36cddca], 
[95d00191-ec8a-4bef-a24e-ea0d7d132bfb], 
[92d45d1c-821b-4869-872b-f4355150a145], 
[fd227620-fc28-4539-803e-f9390a8fe329], 
[08bc5fa6-16e7-4b98-813f-fbe233736d6a], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@ChangeHistory, 
@ISBN, 
@DatePublished, 
@DOI, 
@Pages, 
@Institution, 
@DayPublished, 
@PublisherAddress, 
@Location, 
@BookTitle, 
@Volume, 
@CatalogNumber, 
@PublisherUri, 
@DateSubmitted, 
@Chapter, 
@Number, 
@DateAccepted, 
@Publisher, 
@Edition, 
@MonthPublished, 
@YearPublished, 
@Organization, 
@Series, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'23d6f2a9-3819-44de-9cf0-1eb683141dfe', 
8

);


END
GO
CREATE PROCEDURE [Core].[Update23d6f2a9381944de9cf01eb683141dfe]
 @ChangeHistory nvarchar(max)
, @ISBN nvarchar(256)
, @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [a23f94c6-6344-4ba6-ae90-63e49743a0d1]  =  @ChangeHistory
, [bc13df5d-58dc-4fc7-8010-ec7eb04ac0c0]  =  @ISBN
, [80a0d8df-d9a7-488b-85fe-062bac9418f2]  =  @DatePublished
, [e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6]  =  @DOI
, [3f6b345a-d2f2-4cb1-a7b5-2a316854adc6]  =  @Pages
, [e5989779-7dff-46c6-90e1-3a8be8c6d07b]  =  @Institution
, [ecc77bac-77c4-4eb7-80db-3cd6821c8628]  =  @DayPublished
, [9d3790de-e0e0-459b-8605-52b9f1470301]  =  @PublisherAddress
, [32bc2d19-22ed-4514-bfed-7b85353dc82e]  =  @Location
, [05d783c7-5517-4fbc-88b9-8e010e3f7bb3]  =  @BookTitle
, [6db26d3c-a477-4148-a527-8e331bf46619]  =  @Volume
, [441aeefc-94b4-4c5a-839f-962ff15a0624]  =  @CatalogNumber
, [b888a64d-4555-4d1d-a422-99a1511b1f65]  =  @PublisherUri
, [03163fdf-9d73-40bf-9120-ae22e2c3407c]  =  @DateSubmitted
, [b686fec9-0fdf-46f6-b5aa-b1441bad9ea5]  =  @Chapter
, [9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2]  =  @Number
, [1cef5926-ef98-4170-94d6-db4326671534]  =  @DateAccepted
, [62f20793-f341-42ad-8980-e358c6037763]  =  @Publisher
, [8db90ced-fd85-4ba7-9a9f-e61bb36cddca]  =  @Edition
, [95d00191-ec8a-4bef-a24e-ea0d7d132bfb]  =  @MonthPublished
, [92d45d1c-821b-4869-872b-f4355150a145]  =  @YearPublished
, [fd227620-fc28-4539-803e-f9390a8fe329]  =  @Organization
, [08bc5fa6-16e7-4b98-813f-fbe233736d6a]  =  @Series
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete23d6f2a9381944de9cf01eb683141dfe]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insertc73f8d6f03044575a614219dbf52fc89]
 @VersionInformation nvarchar(256)
, @Copyright nvarchar(4000)
, @HardwareRequirements nvarchar(4000)
, @EULA nvarchar(1024)
, @OperatingSystem nvarchar(4000)
, @DownloadRequirements nvarchar(4000)
, @SystemRequirements nvarchar(4000)
, @Language nvarchar(128)
, @License nvarchar(4000)
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[0bb4e6f6-7d38-4773-a301-2c83810923e8], 
[be528997-17b3-405c-a609-58bb560f36b9], 
[c82db7be-893e-4886-bbaa-63f530d5c960], 
[7df00974-90e1-4abb-816b-8da453b037e9], 
[97f83304-bcd9-45f6-9256-9bdc093c52b5], 
[b597448f-76f8-4663-8b6f-9c04f24194b5], 
[290fee5b-da0e-42a9-9d80-a10826572733], 
[4283e5a5-96fe-47fe-94be-de1f0caa9f76], 
[9fd938f9-da4a-4541-bcbe-fc6edb85cc2c], 
[4b96b254-d594-4576-a6ab-ff90938d6910], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@VersionInformation, 
@Copyright, 
@HardwareRequirements, 
@EULA, 
@OperatingSystem, 
@DownloadRequirements, 
@SystemRequirements, 
@Language, 
@License, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'c73f8d6f-0304-4575-a614-219dbf52fc89', 
9

);


END
GO
CREATE PROCEDURE [Core].[Updatec73f8d6f03044575a614219dbf52fc89]
 @VersionInformation nvarchar(256)
, @Copyright nvarchar(4000)
, @HardwareRequirements nvarchar(4000)
, @EULA nvarchar(1024)
, @OperatingSystem nvarchar(4000)
, @DownloadRequirements nvarchar(4000)
, @SystemRequirements nvarchar(4000)
, @Language nvarchar(128)
, @License nvarchar(4000)
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [0bb4e6f6-7d38-4773-a301-2c83810923e8]  =  @VersionInformation
, [be528997-17b3-405c-a609-58bb560f36b9]  =  @Copyright
, [c82db7be-893e-4886-bbaa-63f530d5c960]  =  @HardwareRequirements
, [7df00974-90e1-4abb-816b-8da453b037e9]  =  @EULA
, [97f83304-bcd9-45f6-9256-9bdc093c52b5]  =  @OperatingSystem
, [b597448f-76f8-4663-8b6f-9c04f24194b5]  =  @DownloadRequirements
, [290fee5b-da0e-42a9-9d80-a10826572733]  =  @SystemRequirements
, [4283e5a5-96fe-47fe-94be-de1f0caa9f76]  =  @Language
, [9fd938f9-da4a-4541-bcbe-fc6edb85cc2c]  =  @License
, [4b96b254-d594-4576-a6ab-ff90938d6910]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deletec73f8d6f03044575a614219dbf52fc89]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insertb21eec569b7a4f199c6123520fd674b0]
 @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[80a0d8df-d9a7-488b-85fe-062bac9418f2], 
[e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6], 
[3f6b345a-d2f2-4cb1-a7b5-2a316854adc6], 
[e5989779-7dff-46c6-90e1-3a8be8c6d07b], 
[ecc77bac-77c4-4eb7-80db-3cd6821c8628], 
[9d3790de-e0e0-459b-8605-52b9f1470301], 
[32bc2d19-22ed-4514-bfed-7b85353dc82e], 
[05d783c7-5517-4fbc-88b9-8e010e3f7bb3], 
[6db26d3c-a477-4148-a527-8e331bf46619], 
[441aeefc-94b4-4c5a-839f-962ff15a0624], 
[b888a64d-4555-4d1d-a422-99a1511b1f65], 
[03163fdf-9d73-40bf-9120-ae22e2c3407c], 
[b686fec9-0fdf-46f6-b5aa-b1441bad9ea5], 
[9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2], 
[1cef5926-ef98-4170-94d6-db4326671534], 
[62f20793-f341-42ad-8980-e358c6037763], 
[8db90ced-fd85-4ba7-9a9f-e61bb36cddca], 
[95d00191-ec8a-4bef-a24e-ea0d7d132bfb], 
[92d45d1c-821b-4869-872b-f4355150a145], 
[fd227620-fc28-4539-803e-f9390a8fe329], 
[08bc5fa6-16e7-4b98-813f-fbe233736d6a], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@DatePublished, 
@DOI, 
@Pages, 
@Institution, 
@DayPublished, 
@PublisherAddress, 
@Location, 
@BookTitle, 
@Volume, 
@CatalogNumber, 
@PublisherUri, 
@DateSubmitted, 
@Chapter, 
@Number, 
@DateAccepted, 
@Publisher, 
@Edition, 
@MonthPublished, 
@YearPublished, 
@Organization, 
@Series, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'b21eec56-9b7a-4f19-9c61-23520fd674b0', 
10

);


END
GO
CREATE PROCEDURE [Core].[Updateb21eec569b7a4f199c6123520fd674b0]
 @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [80a0d8df-d9a7-488b-85fe-062bac9418f2]  =  @DatePublished
, [e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6]  =  @DOI
, [3f6b345a-d2f2-4cb1-a7b5-2a316854adc6]  =  @Pages
, [e5989779-7dff-46c6-90e1-3a8be8c6d07b]  =  @Institution
, [ecc77bac-77c4-4eb7-80db-3cd6821c8628]  =  @DayPublished
, [9d3790de-e0e0-459b-8605-52b9f1470301]  =  @PublisherAddress
, [32bc2d19-22ed-4514-bfed-7b85353dc82e]  =  @Location
, [05d783c7-5517-4fbc-88b9-8e010e3f7bb3]  =  @BookTitle
, [6db26d3c-a477-4148-a527-8e331bf46619]  =  @Volume
, [441aeefc-94b4-4c5a-839f-962ff15a0624]  =  @CatalogNumber
, [b888a64d-4555-4d1d-a422-99a1511b1f65]  =  @PublisherUri
, [03163fdf-9d73-40bf-9120-ae22e2c3407c]  =  @DateSubmitted
, [b686fec9-0fdf-46f6-b5aa-b1441bad9ea5]  =  @Chapter
, [9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2]  =  @Number
, [1cef5926-ef98-4170-94d6-db4326671534]  =  @DateAccepted
, [62f20793-f341-42ad-8980-e358c6037763]  =  @Publisher
, [8db90ced-fd85-4ba7-9a9f-e61bb36cddca]  =  @Edition
, [95d00191-ec8a-4bef-a24e-ea0d7d132bfb]  =  @MonthPublished
, [92d45d1c-821b-4869-872b-f4355150a145]  =  @YearPublished
, [fd227620-fc28-4539-803e-f9390a8fe329]  =  @Organization
, [08bc5fa6-16e7-4b98-813f-fbe233736d6a]  =  @Series
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deleteb21eec569b7a4f199c6123520fd674b0]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert3064535f9989489ca7d024a40674494a]
 @Name nvarchar(50)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Name IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Name].', 16, 1);
	RETURN -1;
END
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[ea645ca2-11e7-4746-af99-667274066311], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@Name, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'3064535f-9989-489c-a7d0-24a40674494a', 
11

);


END
GO
CREATE PROCEDURE [Core].[Update3064535f9989489ca7d024a40674494a]
 @Name nvarchar(50)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Name IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Name].', 16, 1);
	RETURN -1;
END
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [ea645ca2-11e7-4746-af99-667274066311]  =  @Name
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete3064535f9989489ca7d024a40674494a]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insertfbad8cd768d0465fb6162d4f2f1964b5]
 @Subject nvarchar(256)
, @From nvarchar(1024)
, @DateExchanged datetime
, @To nvarchar(4000)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[a8f0994e-bc4e-4346-886d-9ffbde0940a8], 
[96302933-01ef-41b4-8399-7f946dd0d07f], 
[65db5eed-c9fd-452a-a289-a6034f072d9b], 
[496bd2ef-295d-4801-892d-b38e8338e6ff], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@Subject, 
@From, 
@DateExchanged, 
@To, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'fbad8cd7-68d0-465f-b616-2d4f2f1964b5', 
12

);


END
GO
CREATE PROCEDURE [Core].[Updatefbad8cd768d0465fb6162d4f2f1964b5]
 @Subject nvarchar(256)
, @From nvarchar(1024)
, @DateExchanged datetime
, @To nvarchar(4000)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [a8f0994e-bc4e-4346-886d-9ffbde0940a8]  =  @Subject
, [96302933-01ef-41b4-8399-7f946dd0d07f]  =  @From
, [65db5eed-c9fd-452a-a289-a6034f072d9b]  =  @DateExchanged
, [496bd2ef-295d-4801-892d-b38e8338e6ff]  =  @To
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deletefbad8cd768d0465fb6162d4f2f1964b5]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert8a3e790bec774ab3abbc30008fdb9ebc]
 @Codec nvarchar(4000)
, @BitRate int
, @Mode nvarchar(4000)
, @License nvarchar(4000)
, @Copyright nvarchar(4000)
, @DateCopyrighted datetime
, @Duration int
, @Language nvarchar(128)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[7dc69c44-9efb-46b9-96e5-24b44e7a7f8b], 
[2b83b84e-b8d2-453a-89be-833e459d6bff], 
[49ec0c5e-1c52-4136-9966-9d437413c829], 
[2b8e690c-44c7-4670-b147-06b7b18d89f3], 
[3c35c424-0f62-44e5-82fe-2755a9eff1c2], 
[e94f81aa-1993-4726-9447-5ee237e58654], 
[56b9acf3-9e10-424e-a9f0-a6f5aab12a50], 
[3d773ba6-1955-47c4-9388-baef50b70327], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@Codec, 
@BitRate, 
@Mode, 
@License, 
@Copyright, 
@DateCopyrighted, 
@Duration, 
@Language, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'8a3e790b-ec77-4ab3-abbc-30008fdb9ebc', 
13

);


END
GO
CREATE PROCEDURE [Core].[Update8a3e790bec774ab3abbc30008fdb9ebc]
 @Codec nvarchar(4000)
, @BitRate int
, @Mode nvarchar(4000)
, @License nvarchar(4000)
, @Copyright nvarchar(4000)
, @DateCopyrighted datetime
, @Duration int
, @Language nvarchar(128)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [7dc69c44-9efb-46b9-96e5-24b44e7a7f8b]  =  @Codec
, [2b83b84e-b8d2-453a-89be-833e459d6bff]  =  @BitRate
, [49ec0c5e-1c52-4136-9966-9d437413c829]  =  @Mode
, [2b8e690c-44c7-4670-b147-06b7b18d89f3]  =  @License
, [3c35c424-0f62-44e5-82fe-2755a9eff1c2]  =  @Copyright
, [e94f81aa-1993-4726-9447-5ee237e58654]  =  @DateCopyrighted
, [56b9acf3-9e10-424e-a9f0-a6f5aab12a50]  =  @Duration
, [3d773ba6-1955-47c4-9388-baef50b70327]  =  @Language
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete8a3e790bec774ab3abbc30008fdb9ebc]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insertb0eb981eb5aa48c0bf975622abef1e6e]
 @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[80a0d8df-d9a7-488b-85fe-062bac9418f2], 
[e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6], 
[3f6b345a-d2f2-4cb1-a7b5-2a316854adc6], 
[e5989779-7dff-46c6-90e1-3a8be8c6d07b], 
[ecc77bac-77c4-4eb7-80db-3cd6821c8628], 
[9d3790de-e0e0-459b-8605-52b9f1470301], 
[32bc2d19-22ed-4514-bfed-7b85353dc82e], 
[05d783c7-5517-4fbc-88b9-8e010e3f7bb3], 
[6db26d3c-a477-4148-a527-8e331bf46619], 
[441aeefc-94b4-4c5a-839f-962ff15a0624], 
[b888a64d-4555-4d1d-a422-99a1511b1f65], 
[03163fdf-9d73-40bf-9120-ae22e2c3407c], 
[b686fec9-0fdf-46f6-b5aa-b1441bad9ea5], 
[9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2], 
[1cef5926-ef98-4170-94d6-db4326671534], 
[62f20793-f341-42ad-8980-e358c6037763], 
[8db90ced-fd85-4ba7-9a9f-e61bb36cddca], 
[95d00191-ec8a-4bef-a24e-ea0d7d132bfb], 
[92d45d1c-821b-4869-872b-f4355150a145], 
[fd227620-fc28-4539-803e-f9390a8fe329], 
[08bc5fa6-16e7-4b98-813f-fbe233736d6a], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@DatePublished, 
@DOI, 
@Pages, 
@Institution, 
@DayPublished, 
@PublisherAddress, 
@Location, 
@BookTitle, 
@Volume, 
@CatalogNumber, 
@PublisherUri, 
@DateSubmitted, 
@Chapter, 
@Number, 
@DateAccepted, 
@Publisher, 
@Edition, 
@MonthPublished, 
@YearPublished, 
@Organization, 
@Series, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'b0eb981e-b5aa-48c0-bf97-5622abef1e6e', 
14

);


END
GO
CREATE PROCEDURE [Core].[Updateb0eb981eb5aa48c0bf975622abef1e6e]
 @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [80a0d8df-d9a7-488b-85fe-062bac9418f2]  =  @DatePublished
, [e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6]  =  @DOI
, [3f6b345a-d2f2-4cb1-a7b5-2a316854adc6]  =  @Pages
, [e5989779-7dff-46c6-90e1-3a8be8c6d07b]  =  @Institution
, [ecc77bac-77c4-4eb7-80db-3cd6821c8628]  =  @DayPublished
, [9d3790de-e0e0-459b-8605-52b9f1470301]  =  @PublisherAddress
, [32bc2d19-22ed-4514-bfed-7b85353dc82e]  =  @Location
, [05d783c7-5517-4fbc-88b9-8e010e3f7bb3]  =  @BookTitle
, [6db26d3c-a477-4148-a527-8e331bf46619]  =  @Volume
, [441aeefc-94b4-4c5a-839f-962ff15a0624]  =  @CatalogNumber
, [b888a64d-4555-4d1d-a422-99a1511b1f65]  =  @PublisherUri
, [03163fdf-9d73-40bf-9120-ae22e2c3407c]  =  @DateSubmitted
, [b686fec9-0fdf-46f6-b5aa-b1441bad9ea5]  =  @Chapter
, [9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2]  =  @Number
, [1cef5926-ef98-4170-94d6-db4326671534]  =  @DateAccepted
, [62f20793-f341-42ad-8980-e358c6037763]  =  @Publisher
, [8db90ced-fd85-4ba7-9a9f-e61bb36cddca]  =  @Edition
, [95d00191-ec8a-4bef-a24e-ea0d7d132bfb]  =  @MonthPublished
, [92d45d1c-821b-4869-872b-f4355150a145]  =  @YearPublished
, [fd227620-fc28-4539-803e-f9390a8fe329]  =  @Organization
, [08bc5fa6-16e7-4b98-813f-fbe233736d6a]  =  @Series
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deleteb0eb981eb5aa48c0bf975622abef1e6e]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insertf46d9cb111be445b90675d558d8714c4]
 @VersionInformation nvarchar(256)
, @Copyright nvarchar(4000)
, @HardwareRequirements nvarchar(4000)
, @EULA nvarchar(1024)
, @OperatingSystem nvarchar(4000)
, @DownloadRequirements nvarchar(4000)
, @SystemRequirements nvarchar(4000)
, @Language nvarchar(128)
, @License nvarchar(4000)
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[0bb4e6f6-7d38-4773-a301-2c83810923e8], 
[be528997-17b3-405c-a609-58bb560f36b9], 
[c82db7be-893e-4886-bbaa-63f530d5c960], 
[7df00974-90e1-4abb-816b-8da453b037e9], 
[97f83304-bcd9-45f6-9256-9bdc093c52b5], 
[b597448f-76f8-4663-8b6f-9c04f24194b5], 
[290fee5b-da0e-42a9-9d80-a10826572733], 
[4283e5a5-96fe-47fe-94be-de1f0caa9f76], 
[9fd938f9-da4a-4541-bcbe-fc6edb85cc2c], 
[4b96b254-d594-4576-a6ab-ff90938d6910], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@VersionInformation, 
@Copyright, 
@HardwareRequirements, 
@EULA, 
@OperatingSystem, 
@DownloadRequirements, 
@SystemRequirements, 
@Language, 
@License, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'f46d9cb1-11be-445b-9067-5d558d8714c4', 
15

);


END
GO
CREATE PROCEDURE [Core].[Updatef46d9cb111be445b90675d558d8714c4]
 @VersionInformation nvarchar(256)
, @Copyright nvarchar(4000)
, @HardwareRequirements nvarchar(4000)
, @EULA nvarchar(1024)
, @OperatingSystem nvarchar(4000)
, @DownloadRequirements nvarchar(4000)
, @SystemRequirements nvarchar(4000)
, @Language nvarchar(128)
, @License nvarchar(4000)
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [0bb4e6f6-7d38-4773-a301-2c83810923e8]  =  @VersionInformation
, [be528997-17b3-405c-a609-58bb560f36b9]  =  @Copyright
, [c82db7be-893e-4886-bbaa-63f530d5c960]  =  @HardwareRequirements
, [7df00974-90e1-4abb-816b-8da453b037e9]  =  @EULA
, [97f83304-bcd9-45f6-9256-9bdc093c52b5]  =  @OperatingSystem
, [b597448f-76f8-4663-8b6f-9c04f24194b5]  =  @DownloadRequirements
, [290fee5b-da0e-42a9-9d80-a10826572733]  =  @SystemRequirements
, [4283e5a5-96fe-47fe-94be-de1f0caa9f76]  =  @Language
, [9fd938f9-da4a-4541-bcbe-fc6edb85cc2c]  =  @License
, [4b96b254-d594-4576-a6ab-ff90938d6910]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deletef46d9cb111be445b90675d558d8714c4]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert2961c44e725248149ca15ebc584d4932]
 @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[80a0d8df-d9a7-488b-85fe-062bac9418f2], 
[e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6], 
[3f6b345a-d2f2-4cb1-a7b5-2a316854adc6], 
[e5989779-7dff-46c6-90e1-3a8be8c6d07b], 
[ecc77bac-77c4-4eb7-80db-3cd6821c8628], 
[9d3790de-e0e0-459b-8605-52b9f1470301], 
[32bc2d19-22ed-4514-bfed-7b85353dc82e], 
[05d783c7-5517-4fbc-88b9-8e010e3f7bb3], 
[6db26d3c-a477-4148-a527-8e331bf46619], 
[441aeefc-94b4-4c5a-839f-962ff15a0624], 
[b888a64d-4555-4d1d-a422-99a1511b1f65], 
[03163fdf-9d73-40bf-9120-ae22e2c3407c], 
[b686fec9-0fdf-46f6-b5aa-b1441bad9ea5], 
[9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2], 
[1cef5926-ef98-4170-94d6-db4326671534], 
[62f20793-f341-42ad-8980-e358c6037763], 
[8db90ced-fd85-4ba7-9a9f-e61bb36cddca], 
[95d00191-ec8a-4bef-a24e-ea0d7d132bfb], 
[92d45d1c-821b-4869-872b-f4355150a145], 
[fd227620-fc28-4539-803e-f9390a8fe329], 
[08bc5fa6-16e7-4b98-813f-fbe233736d6a], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@DatePublished, 
@DOI, 
@Pages, 
@Institution, 
@DayPublished, 
@PublisherAddress, 
@Location, 
@BookTitle, 
@Volume, 
@CatalogNumber, 
@PublisherUri, 
@DateSubmitted, 
@Chapter, 
@Number, 
@DateAccepted, 
@Publisher, 
@Edition, 
@MonthPublished, 
@YearPublished, 
@Organization, 
@Series, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'2961c44e-7252-4814-9ca1-5ebc584d4932', 
16

);


END
GO
CREATE PROCEDURE [Core].[Update2961c44e725248149ca15ebc584d4932]
 @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [80a0d8df-d9a7-488b-85fe-062bac9418f2]  =  @DatePublished
, [e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6]  =  @DOI
, [3f6b345a-d2f2-4cb1-a7b5-2a316854adc6]  =  @Pages
, [e5989779-7dff-46c6-90e1-3a8be8c6d07b]  =  @Institution
, [ecc77bac-77c4-4eb7-80db-3cd6821c8628]  =  @DayPublished
, [9d3790de-e0e0-459b-8605-52b9f1470301]  =  @PublisherAddress
, [32bc2d19-22ed-4514-bfed-7b85353dc82e]  =  @Location
, [05d783c7-5517-4fbc-88b9-8e010e3f7bb3]  =  @BookTitle
, [6db26d3c-a477-4148-a527-8e331bf46619]  =  @Volume
, [441aeefc-94b4-4c5a-839f-962ff15a0624]  =  @CatalogNumber
, [b888a64d-4555-4d1d-a422-99a1511b1f65]  =  @PublisherUri
, [03163fdf-9d73-40bf-9120-ae22e2c3407c]  =  @DateSubmitted
, [b686fec9-0fdf-46f6-b5aa-b1441bad9ea5]  =  @Chapter
, [9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2]  =  @Number
, [1cef5926-ef98-4170-94d6-db4326671534]  =  @DateAccepted
, [62f20793-f341-42ad-8980-e358c6037763]  =  @Publisher
, [8db90ced-fd85-4ba7-9a9f-e61bb36cddca]  =  @Edition
, [95d00191-ec8a-4bef-a24e-ea0d7d132bfb]  =  @MonthPublished
, [92d45d1c-821b-4869-872b-f4355150a145]  =  @YearPublished
, [fd227620-fc28-4539-803e-f9390a8fe329]  =  @Organization
, [08bc5fa6-16e7-4b98-813f-fbe233736d6a]  =  @Series
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete2961c44e725248149ca15ebc584d4932]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insertc91643ffe8704a12b1406040a989fd6d]
 @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[80a0d8df-d9a7-488b-85fe-062bac9418f2], 
[e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6], 
[3f6b345a-d2f2-4cb1-a7b5-2a316854adc6], 
[e5989779-7dff-46c6-90e1-3a8be8c6d07b], 
[ecc77bac-77c4-4eb7-80db-3cd6821c8628], 
[9d3790de-e0e0-459b-8605-52b9f1470301], 
[32bc2d19-22ed-4514-bfed-7b85353dc82e], 
[05d783c7-5517-4fbc-88b9-8e010e3f7bb3], 
[6db26d3c-a477-4148-a527-8e331bf46619], 
[441aeefc-94b4-4c5a-839f-962ff15a0624], 
[b888a64d-4555-4d1d-a422-99a1511b1f65], 
[03163fdf-9d73-40bf-9120-ae22e2c3407c], 
[b686fec9-0fdf-46f6-b5aa-b1441bad9ea5], 
[9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2], 
[1cef5926-ef98-4170-94d6-db4326671534], 
[62f20793-f341-42ad-8980-e358c6037763], 
[8db90ced-fd85-4ba7-9a9f-e61bb36cddca], 
[95d00191-ec8a-4bef-a24e-ea0d7d132bfb], 
[92d45d1c-821b-4869-872b-f4355150a145], 
[fd227620-fc28-4539-803e-f9390a8fe329], 
[08bc5fa6-16e7-4b98-813f-fbe233736d6a], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@DatePublished, 
@DOI, 
@Pages, 
@Institution, 
@DayPublished, 
@PublisherAddress, 
@Location, 
@BookTitle, 
@Volume, 
@CatalogNumber, 
@PublisherUri, 
@DateSubmitted, 
@Chapter, 
@Number, 
@DateAccepted, 
@Publisher, 
@Edition, 
@MonthPublished, 
@YearPublished, 
@Organization, 
@Series, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'c91643ff-e870-4a12-b140-6040a989fd6d', 
17

);


END
GO
CREATE PROCEDURE [Core].[Updatec91643ffe8704a12b1406040a989fd6d]
 @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [80a0d8df-d9a7-488b-85fe-062bac9418f2]  =  @DatePublished
, [e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6]  =  @DOI
, [3f6b345a-d2f2-4cb1-a7b5-2a316854adc6]  =  @Pages
, [e5989779-7dff-46c6-90e1-3a8be8c6d07b]  =  @Institution
, [ecc77bac-77c4-4eb7-80db-3cd6821c8628]  =  @DayPublished
, [9d3790de-e0e0-459b-8605-52b9f1470301]  =  @PublisherAddress
, [32bc2d19-22ed-4514-bfed-7b85353dc82e]  =  @Location
, [05d783c7-5517-4fbc-88b9-8e010e3f7bb3]  =  @BookTitle
, [6db26d3c-a477-4148-a527-8e331bf46619]  =  @Volume
, [441aeefc-94b4-4c5a-839f-962ff15a0624]  =  @CatalogNumber
, [b888a64d-4555-4d1d-a422-99a1511b1f65]  =  @PublisherUri
, [03163fdf-9d73-40bf-9120-ae22e2c3407c]  =  @DateSubmitted
, [b686fec9-0fdf-46f6-b5aa-b1441bad9ea5]  =  @Chapter
, [9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2]  =  @Number
, [1cef5926-ef98-4170-94d6-db4326671534]  =  @DateAccepted
, [62f20793-f341-42ad-8980-e358c6037763]  =  @Publisher
, [8db90ced-fd85-4ba7-9a9f-e61bb36cddca]  =  @Edition
, [95d00191-ec8a-4bef-a24e-ea0d7d132bfb]  =  @MonthPublished
, [92d45d1c-821b-4869-872b-f4355150a145]  =  @YearPublished
, [fd227620-fc28-4539-803e-f9390a8fe329]  =  @Organization
, [08bc5fa6-16e7-4b98-813f-fbe233736d6a]  =  @Series
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deletec91643ffe8704a12b1406040a989fd6d]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert4c89074ef0d6470687006d2e0d1afe98]
 @ProgrammingLanguage nvarchar(256)
, @Technology nvarchar(256)
, @VersionInformation nvarchar(256)
, @Copyright nvarchar(4000)
, @HardwareRequirements nvarchar(4000)
, @EULA nvarchar(1024)
, @OperatingSystem nvarchar(4000)
, @DownloadRequirements nvarchar(4000)
, @SystemRequirements nvarchar(4000)
, @Language nvarchar(128)
, @License nvarchar(4000)
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[4326eb60-7271-43a5-9436-2d0766c353b9], 
[158a2898-edec-47d8-937a-b37269df2aaa], 
[0bb4e6f6-7d38-4773-a301-2c83810923e8], 
[be528997-17b3-405c-a609-58bb560f36b9], 
[c82db7be-893e-4886-bbaa-63f530d5c960], 
[7df00974-90e1-4abb-816b-8da453b037e9], 
[97f83304-bcd9-45f6-9256-9bdc093c52b5], 
[b597448f-76f8-4663-8b6f-9c04f24194b5], 
[290fee5b-da0e-42a9-9d80-a10826572733], 
[4283e5a5-96fe-47fe-94be-de1f0caa9f76], 
[9fd938f9-da4a-4541-bcbe-fc6edb85cc2c], 
[4b96b254-d594-4576-a6ab-ff90938d6910], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@ProgrammingLanguage, 
@Technology, 
@VersionInformation, 
@Copyright, 
@HardwareRequirements, 
@EULA, 
@OperatingSystem, 
@DownloadRequirements, 
@SystemRequirements, 
@Language, 
@License, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'4c89074e-f0d6-4706-8700-6d2e0d1afe98', 
18

);


END
GO
CREATE PROCEDURE [Core].[Update4c89074ef0d6470687006d2e0d1afe98]
 @ProgrammingLanguage nvarchar(256)
, @Technology nvarchar(256)
, @VersionInformation nvarchar(256)
, @Copyright nvarchar(4000)
, @HardwareRequirements nvarchar(4000)
, @EULA nvarchar(1024)
, @OperatingSystem nvarchar(4000)
, @DownloadRequirements nvarchar(4000)
, @SystemRequirements nvarchar(4000)
, @Language nvarchar(128)
, @License nvarchar(4000)
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [4326eb60-7271-43a5-9436-2d0766c353b9]  =  @ProgrammingLanguage
, [158a2898-edec-47d8-937a-b37269df2aaa]  =  @Technology
, [0bb4e6f6-7d38-4773-a301-2c83810923e8]  =  @VersionInformation
, [be528997-17b3-405c-a609-58bb560f36b9]  =  @Copyright
, [c82db7be-893e-4886-bbaa-63f530d5c960]  =  @HardwareRequirements
, [7df00974-90e1-4abb-816b-8da453b037e9]  =  @EULA
, [97f83304-bcd9-45f6-9256-9bdc093c52b5]  =  @OperatingSystem
, [b597448f-76f8-4663-8b6f-9c04f24194b5]  =  @DownloadRequirements
, [290fee5b-da0e-42a9-9d80-a10826572733]  =  @SystemRequirements
, [4283e5a5-96fe-47fe-94be-de1f0caa9f76]  =  @Language
, [9fd938f9-da4a-4541-bcbe-fc6edb85cc2c]  =  @License
, [4b96b254-d594-4576-a6ab-ff90938d6910]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete4c89074ef0d6470687006d2e0d1afe98]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert750bd9f2aa9d4a0382c0739fceafe7b2]
 @Series nvarchar(256)
, @Image varbinary(max)
, @Audience nvarchar(4000)
, @Venue nvarchar(1024)
, @DateEnd datetime
, @DateStart datetime
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[eaa610b7-cf35-4393-bf19-0fb7af9a128a], 
[2af50485-aa00-4c0a-9d2c-381720b7c7b3], 
[25b52405-1f7a-4c9a-b904-591d4e29c73c], 
[67cbb24d-0c60-4b65-adfa-7f6c60ad5935], 
[02dbf59e-ff45-41e8-9320-a702ebdadba7], 
[22e98699-e5c6-4466-af64-be9dc28d7dc5], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@Series, 
@Image, 
@Audience, 
@Venue, 
@DateEnd, 
@DateStart, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'750bd9f2-aa9d-4a03-82c0-739fceafe7b2', 
19

);


END
GO
CREATE PROCEDURE [Core].[Update750bd9f2aa9d4a0382c0739fceafe7b2]
 @Series nvarchar(256)
, @Image varbinary(max)
, @Audience nvarchar(4000)
, @Venue nvarchar(1024)
, @DateEnd datetime
, @DateStart datetime
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [eaa610b7-cf35-4393-bf19-0fb7af9a128a]  =  @Series
, [2af50485-aa00-4c0a-9d2c-381720b7c7b3]  =  @Image
, [25b52405-1f7a-4c9a-b904-591d4e29c73c]  =  @Audience
, [67cbb24d-0c60-4b65-adfa-7f6c60ad5935]  =  @Venue
, [02dbf59e-ff45-41e8-9320-a702ebdadba7]  =  @DateEnd
, [22e98699-e5c6-4466-af64-be9dc28d7dc5]  =  @DateStart
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete750bd9f2aa9d4a0382c0739fceafe7b2]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert807b649a83284caaa29d7a61048fcef2]
 @Stereoscopic bit
, @Resolution nvarchar(256)
, @PSNR decimal(18, 0)
, @Director nvarchar(256)
, @Codec nvarchar(4000)
, @BitRate int
, @AspectRatio nvarchar(64)
, @FrameWidth int
, @FrameHeight int
, @FramesPerSecond smallint
, @ScanningMethod nvarchar(256)
, @BitsPerPixel smallint
, @ColorModel nvarchar(64)
, @License nvarchar(4000)
, @Copyright nvarchar(4000)
, @DateCopyrighted datetime
, @Duration int
, @Language nvarchar(128)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[5108112d-29f5-4421-8bac-058f2f5bff4e], 
[a2733469-7db1-4a5b-9316-0deec2623e67], 
[86f59c02-d498-47d2-81dc-2a52138aadab], 
[dd35d7a1-7468-4625-9133-2b90b7e07251], 
[9fc89788-3c77-43a5-88cc-2d61e65c492d], 
[07cfd378-9a6a-4c04-93ad-52f243f79c74], 
[cbde5c20-64bf-4c43-b354-6fe6a4cf362e], 
[2cc4abc7-b19e-46e1-9438-755742a5946c], 
[14c68cc9-d2ef-4d4b-9f79-8c84e27d2813], 
[f01210b7-d617-4727-8525-b3f1360f8d78], 
[d1a6e5d0-0405-441f-b4fb-bd41c2bbe96d], 
[2f0f3d7c-8e53-4c74-9418-c53e4b5f3a3a], 
[51426e9f-5576-4ae9-8bd0-fb954a75eb1c], 
[2b8e690c-44c7-4670-b147-06b7b18d89f3], 
[3c35c424-0f62-44e5-82fe-2755a9eff1c2], 
[e94f81aa-1993-4726-9447-5ee237e58654], 
[56b9acf3-9e10-424e-a9f0-a6f5aab12a50], 
[3d773ba6-1955-47c4-9388-baef50b70327], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@Stereoscopic, 
@Resolution, 
@PSNR, 
@Director, 
@Codec, 
@BitRate, 
@AspectRatio, 
@FrameWidth, 
@FrameHeight, 
@FramesPerSecond, 
@ScanningMethod, 
@BitsPerPixel, 
@ColorModel, 
@License, 
@Copyright, 
@DateCopyrighted, 
@Duration, 
@Language, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'807b649a-8328-4caa-a29d-7a61048fcef2', 
20

);


END
GO
CREATE PROCEDURE [Core].[Update807b649a83284caaa29d7a61048fcef2]
 @Stereoscopic bit
, @Resolution nvarchar(256)
, @PSNR decimal(18, 0)
, @Director nvarchar(256)
, @Codec nvarchar(4000)
, @BitRate int
, @AspectRatio nvarchar(64)
, @FrameWidth int
, @FrameHeight int
, @FramesPerSecond smallint
, @ScanningMethod nvarchar(256)
, @BitsPerPixel smallint
, @ColorModel nvarchar(64)
, @License nvarchar(4000)
, @Copyright nvarchar(4000)
, @DateCopyrighted datetime
, @Duration int
, @Language nvarchar(128)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [5108112d-29f5-4421-8bac-058f2f5bff4e]  =  @Stereoscopic
, [a2733469-7db1-4a5b-9316-0deec2623e67]  =  @Resolution
, [86f59c02-d498-47d2-81dc-2a52138aadab]  =  @PSNR
, [dd35d7a1-7468-4625-9133-2b90b7e07251]  =  @Director
, [9fc89788-3c77-43a5-88cc-2d61e65c492d]  =  @Codec
, [07cfd378-9a6a-4c04-93ad-52f243f79c74]  =  @BitRate
, [cbde5c20-64bf-4c43-b354-6fe6a4cf362e]  =  @AspectRatio
, [2cc4abc7-b19e-46e1-9438-755742a5946c]  =  @FrameWidth
, [14c68cc9-d2ef-4d4b-9f79-8c84e27d2813]  =  @FrameHeight
, [f01210b7-d617-4727-8525-b3f1360f8d78]  =  @FramesPerSecond
, [d1a6e5d0-0405-441f-b4fb-bd41c2bbe96d]  =  @ScanningMethod
, [2f0f3d7c-8e53-4c74-9418-c53e4b5f3a3a]  =  @BitsPerPixel
, [51426e9f-5576-4ae9-8bd0-fb954a75eb1c]  =  @ColorModel
, [2b8e690c-44c7-4670-b147-06b7b18d89f3]  =  @License
, [3c35c424-0f62-44e5-82fe-2755a9eff1c2]  =  @Copyright
, [e94f81aa-1993-4726-9447-5ee237e58654]  =  @DateCopyrighted
, [56b9acf3-9e10-424e-a9f0-a6f5aab12a50]  =  @Duration
, [3d773ba6-1955-47c4-9388-baef50b70327]  =  @Language
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete807b649a83284caaa29d7a61048fcef2]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert1f2c6537db1a42f884da7fba29803174]
 @Email nvarchar(2048)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[e7fa1d49-3936-4ec3-9414-058909d19c83], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@Email, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'1f2c6537-db1a-42f8-84da-7fba29803174', 
21

);


END
GO
CREATE PROCEDURE [Core].[Update1f2c6537db1a42f884da7fba29803174]
 @Email nvarchar(2048)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [e7fa1d49-3936-4ec3-9414-058909d19c83]  =  @Email
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete1f2c6537db1a42f884da7fba29803174]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert798fc6e5d57d4027b0da8b39e1a9dfe3]
 @EventName nvarchar(1024)
, @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[3851bd83-4ac5-4b9b-8e3c-a16229925ff8], 
[80a0d8df-d9a7-488b-85fe-062bac9418f2], 
[e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6], 
[3f6b345a-d2f2-4cb1-a7b5-2a316854adc6], 
[e5989779-7dff-46c6-90e1-3a8be8c6d07b], 
[ecc77bac-77c4-4eb7-80db-3cd6821c8628], 
[9d3790de-e0e0-459b-8605-52b9f1470301], 
[32bc2d19-22ed-4514-bfed-7b85353dc82e], 
[05d783c7-5517-4fbc-88b9-8e010e3f7bb3], 
[6db26d3c-a477-4148-a527-8e331bf46619], 
[441aeefc-94b4-4c5a-839f-962ff15a0624], 
[b888a64d-4555-4d1d-a422-99a1511b1f65], 
[03163fdf-9d73-40bf-9120-ae22e2c3407c], 
[b686fec9-0fdf-46f6-b5aa-b1441bad9ea5], 
[9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2], 
[1cef5926-ef98-4170-94d6-db4326671534], 
[62f20793-f341-42ad-8980-e358c6037763], 
[8db90ced-fd85-4ba7-9a9f-e61bb36cddca], 
[95d00191-ec8a-4bef-a24e-ea0d7d132bfb], 
[92d45d1c-821b-4869-872b-f4355150a145], 
[fd227620-fc28-4539-803e-f9390a8fe329], 
[08bc5fa6-16e7-4b98-813f-fbe233736d6a], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@EventName, 
@DatePublished, 
@DOI, 
@Pages, 
@Institution, 
@DayPublished, 
@PublisherAddress, 
@Location, 
@BookTitle, 
@Volume, 
@CatalogNumber, 
@PublisherUri, 
@DateSubmitted, 
@Chapter, 
@Number, 
@DateAccepted, 
@Publisher, 
@Edition, 
@MonthPublished, 
@YearPublished, 
@Organization, 
@Series, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'798fc6e5-d57d-4027-b0da-8b39e1a9dfe3', 
22

);


END
GO
CREATE PROCEDURE [Core].[Update798fc6e5d57d4027b0da8b39e1a9dfe3]
 @EventName nvarchar(1024)
, @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [3851bd83-4ac5-4b9b-8e3c-a16229925ff8]  =  @EventName
, [80a0d8df-d9a7-488b-85fe-062bac9418f2]  =  @DatePublished
, [e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6]  =  @DOI
, [3f6b345a-d2f2-4cb1-a7b5-2a316854adc6]  =  @Pages
, [e5989779-7dff-46c6-90e1-3a8be8c6d07b]  =  @Institution
, [ecc77bac-77c4-4eb7-80db-3cd6821c8628]  =  @DayPublished
, [9d3790de-e0e0-459b-8605-52b9f1470301]  =  @PublisherAddress
, [32bc2d19-22ed-4514-bfed-7b85353dc82e]  =  @Location
, [05d783c7-5517-4fbc-88b9-8e010e3f7bb3]  =  @BookTitle
, [6db26d3c-a477-4148-a527-8e331bf46619]  =  @Volume
, [441aeefc-94b4-4c5a-839f-962ff15a0624]  =  @CatalogNumber
, [b888a64d-4555-4d1d-a422-99a1511b1f65]  =  @PublisherUri
, [03163fdf-9d73-40bf-9120-ae22e2c3407c]  =  @DateSubmitted
, [b686fec9-0fdf-46f6-b5aa-b1441bad9ea5]  =  @Chapter
, [9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2]  =  @Number
, [1cef5926-ef98-4170-94d6-db4326671534]  =  @DateAccepted
, [62f20793-f341-42ad-8980-e358c6037763]  =  @Publisher
, [8db90ced-fd85-4ba7-9a9f-e61bb36cddca]  =  @Edition
, [95d00191-ec8a-4bef-a24e-ea0d7d132bfb]  =  @MonthPublished
, [92d45d1c-821b-4869-872b-f4355150a145]  =  @YearPublished
, [fd227620-fc28-4539-803e-f9390a8fe329]  =  @Organization
, [08bc5fa6-16e7-4b98-813f-fbe233736d6a]  =  @Series
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete798fc6e5d57d4027b0da8b39e1a9dfe3]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert44c2b18fa1ec4dedabb09c4ef1b1e740]
 @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[80a0d8df-d9a7-488b-85fe-062bac9418f2], 
[e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6], 
[3f6b345a-d2f2-4cb1-a7b5-2a316854adc6], 
[e5989779-7dff-46c6-90e1-3a8be8c6d07b], 
[ecc77bac-77c4-4eb7-80db-3cd6821c8628], 
[9d3790de-e0e0-459b-8605-52b9f1470301], 
[32bc2d19-22ed-4514-bfed-7b85353dc82e], 
[05d783c7-5517-4fbc-88b9-8e010e3f7bb3], 
[6db26d3c-a477-4148-a527-8e331bf46619], 
[441aeefc-94b4-4c5a-839f-962ff15a0624], 
[b888a64d-4555-4d1d-a422-99a1511b1f65], 
[03163fdf-9d73-40bf-9120-ae22e2c3407c], 
[b686fec9-0fdf-46f6-b5aa-b1441bad9ea5], 
[9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2], 
[1cef5926-ef98-4170-94d6-db4326671534], 
[62f20793-f341-42ad-8980-e358c6037763], 
[8db90ced-fd85-4ba7-9a9f-e61bb36cddca], 
[95d00191-ec8a-4bef-a24e-ea0d7d132bfb], 
[92d45d1c-821b-4869-872b-f4355150a145], 
[fd227620-fc28-4539-803e-f9390a8fe329], 
[08bc5fa6-16e7-4b98-813f-fbe233736d6a], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@DatePublished, 
@DOI, 
@Pages, 
@Institution, 
@DayPublished, 
@PublisherAddress, 
@Location, 
@BookTitle, 
@Volume, 
@CatalogNumber, 
@PublisherUri, 
@DateSubmitted, 
@Chapter, 
@Number, 
@DateAccepted, 
@Publisher, 
@Edition, 
@MonthPublished, 
@YearPublished, 
@Organization, 
@Series, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'44c2b18f-a1ec-4ded-abb0-9c4ef1b1e740', 
23

);


END
GO
CREATE PROCEDURE [Core].[Update44c2b18fa1ec4dedabb09c4ef1b1e740]
 @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [80a0d8df-d9a7-488b-85fe-062bac9418f2]  =  @DatePublished
, [e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6]  =  @DOI
, [3f6b345a-d2f2-4cb1-a7b5-2a316854adc6]  =  @Pages
, [e5989779-7dff-46c6-90e1-3a8be8c6d07b]  =  @Institution
, [ecc77bac-77c4-4eb7-80db-3cd6821c8628]  =  @DayPublished
, [9d3790de-e0e0-459b-8605-52b9f1470301]  =  @PublisherAddress
, [32bc2d19-22ed-4514-bfed-7b85353dc82e]  =  @Location
, [05d783c7-5517-4fbc-88b9-8e010e3f7bb3]  =  @BookTitle
, [6db26d3c-a477-4148-a527-8e331bf46619]  =  @Volume
, [441aeefc-94b4-4c5a-839f-962ff15a0624]  =  @CatalogNumber
, [b888a64d-4555-4d1d-a422-99a1511b1f65]  =  @PublisherUri
, [03163fdf-9d73-40bf-9120-ae22e2c3407c]  =  @DateSubmitted
, [b686fec9-0fdf-46f6-b5aa-b1441bad9ea5]  =  @Chapter
, [9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2]  =  @Number
, [1cef5926-ef98-4170-94d6-db4326671534]  =  @DateAccepted
, [62f20793-f341-42ad-8980-e358c6037763]  =  @Publisher
, [8db90ced-fd85-4ba7-9a9f-e61bb36cddca]  =  @Edition
, [95d00191-ec8a-4bef-a24e-ea0d7d132bfb]  =  @MonthPublished
, [92d45d1c-821b-4869-872b-f4355150a145]  =  @YearPublished
, [fd227620-fc28-4539-803e-f9390a8fe329]  =  @Organization
, [08bc5fa6-16e7-4b98-813f-fbe233736d6a]  =  @Series
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete44c2b18fa1ec4dedabb09c4ef1b1e740]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert1b6989307cc14535888ea489eece878d]
 @JournalName nvarchar(256)
, @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[bdacb3bf-7ba4-499d-a199-9b49e24a022a], 
[80a0d8df-d9a7-488b-85fe-062bac9418f2], 
[e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6], 
[3f6b345a-d2f2-4cb1-a7b5-2a316854adc6], 
[e5989779-7dff-46c6-90e1-3a8be8c6d07b], 
[ecc77bac-77c4-4eb7-80db-3cd6821c8628], 
[9d3790de-e0e0-459b-8605-52b9f1470301], 
[32bc2d19-22ed-4514-bfed-7b85353dc82e], 
[05d783c7-5517-4fbc-88b9-8e010e3f7bb3], 
[6db26d3c-a477-4148-a527-8e331bf46619], 
[441aeefc-94b4-4c5a-839f-962ff15a0624], 
[b888a64d-4555-4d1d-a422-99a1511b1f65], 
[03163fdf-9d73-40bf-9120-ae22e2c3407c], 
[b686fec9-0fdf-46f6-b5aa-b1441bad9ea5], 
[9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2], 
[1cef5926-ef98-4170-94d6-db4326671534], 
[62f20793-f341-42ad-8980-e358c6037763], 
[8db90ced-fd85-4ba7-9a9f-e61bb36cddca], 
[95d00191-ec8a-4bef-a24e-ea0d7d132bfb], 
[92d45d1c-821b-4869-872b-f4355150a145], 
[fd227620-fc28-4539-803e-f9390a8fe329], 
[08bc5fa6-16e7-4b98-813f-fbe233736d6a], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@JournalName, 
@DatePublished, 
@DOI, 
@Pages, 
@Institution, 
@DayPublished, 
@PublisherAddress, 
@Location, 
@BookTitle, 
@Volume, 
@CatalogNumber, 
@PublisherUri, 
@DateSubmitted, 
@Chapter, 
@Number, 
@DateAccepted, 
@Publisher, 
@Edition, 
@MonthPublished, 
@YearPublished, 
@Organization, 
@Series, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'1b698930-7cc1-4535-888e-a489eece878d', 
24

);


END
GO
CREATE PROCEDURE [Core].[Update1b6989307cc14535888ea489eece878d]
 @JournalName nvarchar(256)
, @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [bdacb3bf-7ba4-499d-a199-9b49e24a022a]  =  @JournalName
, [80a0d8df-d9a7-488b-85fe-062bac9418f2]  =  @DatePublished
, [e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6]  =  @DOI
, [3f6b345a-d2f2-4cb1-a7b5-2a316854adc6]  =  @Pages
, [e5989779-7dff-46c6-90e1-3a8be8c6d07b]  =  @Institution
, [ecc77bac-77c4-4eb7-80db-3cd6821c8628]  =  @DayPublished
, [9d3790de-e0e0-459b-8605-52b9f1470301]  =  @PublisherAddress
, [32bc2d19-22ed-4514-bfed-7b85353dc82e]  =  @Location
, [05d783c7-5517-4fbc-88b9-8e010e3f7bb3]  =  @BookTitle
, [6db26d3c-a477-4148-a527-8e331bf46619]  =  @Volume
, [441aeefc-94b4-4c5a-839f-962ff15a0624]  =  @CatalogNumber
, [b888a64d-4555-4d1d-a422-99a1511b1f65]  =  @PublisherUri
, [03163fdf-9d73-40bf-9120-ae22e2c3407c]  =  @DateSubmitted
, [b686fec9-0fdf-46f6-b5aa-b1441bad9ea5]  =  @Chapter
, [9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2]  =  @Number
, [1cef5926-ef98-4170-94d6-db4326671534]  =  @DateAccepted
, [62f20793-f341-42ad-8980-e358c6037763]  =  @Publisher
, [8db90ced-fd85-4ba7-9a9f-e61bb36cddca]  =  @Edition
, [95d00191-ec8a-4bef-a24e-ea0d7d132bfb]  =  @MonthPublished
, [92d45d1c-821b-4869-872b-f4355150a145]  =  @YearPublished
, [fd227620-fc28-4539-803e-f9390a8fe329]  =  @Organization
, [08bc5fa6-16e7-4b98-813f-fbe233736d6a]  =  @Series
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete1b6989307cc14535888ea489eece878d]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insertc9528c0e2ca74dc8908fadfdaa1f0b7f]
 @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f', 
25

);


END
GO
CREATE PROCEDURE [Core].[Updatec9528c0e2ca74dc8908fadfdaa1f0b7f]
 @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deletec9528c0e2ca74dc8908fadfdaa1f0b7f]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insertbd23247c26f441ef9ad4b1e677ec19ce]
 @EventName nvarchar(1024)
, @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[3851bd83-4ac5-4b9b-8e3c-a16229925ff8], 
[80a0d8df-d9a7-488b-85fe-062bac9418f2], 
[e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6], 
[3f6b345a-d2f2-4cb1-a7b5-2a316854adc6], 
[e5989779-7dff-46c6-90e1-3a8be8c6d07b], 
[ecc77bac-77c4-4eb7-80db-3cd6821c8628], 
[9d3790de-e0e0-459b-8605-52b9f1470301], 
[32bc2d19-22ed-4514-bfed-7b85353dc82e], 
[05d783c7-5517-4fbc-88b9-8e010e3f7bb3], 
[6db26d3c-a477-4148-a527-8e331bf46619], 
[441aeefc-94b4-4c5a-839f-962ff15a0624], 
[b888a64d-4555-4d1d-a422-99a1511b1f65], 
[03163fdf-9d73-40bf-9120-ae22e2c3407c], 
[b686fec9-0fdf-46f6-b5aa-b1441bad9ea5], 
[9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2], 
[1cef5926-ef98-4170-94d6-db4326671534], 
[62f20793-f341-42ad-8980-e358c6037763], 
[8db90ced-fd85-4ba7-9a9f-e61bb36cddca], 
[95d00191-ec8a-4bef-a24e-ea0d7d132bfb], 
[92d45d1c-821b-4869-872b-f4355150a145], 
[fd227620-fc28-4539-803e-f9390a8fe329], 
[08bc5fa6-16e7-4b98-813f-fbe233736d6a], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@EventName, 
@DatePublished, 
@DOI, 
@Pages, 
@Institution, 
@DayPublished, 
@PublisherAddress, 
@Location, 
@BookTitle, 
@Volume, 
@CatalogNumber, 
@PublisherUri, 
@DateSubmitted, 
@Chapter, 
@Number, 
@DateAccepted, 
@Publisher, 
@Edition, 
@MonthPublished, 
@YearPublished, 
@Organization, 
@Series, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'bd23247c-26f4-41ef-9ad4-b1e677ec19ce', 
26

);


END
GO
CREATE PROCEDURE [Core].[Updatebd23247c26f441ef9ad4b1e677ec19ce]
 @EventName nvarchar(1024)
, @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [3851bd83-4ac5-4b9b-8e3c-a16229925ff8]  =  @EventName
, [80a0d8df-d9a7-488b-85fe-062bac9418f2]  =  @DatePublished
, [e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6]  =  @DOI
, [3f6b345a-d2f2-4cb1-a7b5-2a316854adc6]  =  @Pages
, [e5989779-7dff-46c6-90e1-3a8be8c6d07b]  =  @Institution
, [ecc77bac-77c4-4eb7-80db-3cd6821c8628]  =  @DayPublished
, [9d3790de-e0e0-459b-8605-52b9f1470301]  =  @PublisherAddress
, [32bc2d19-22ed-4514-bfed-7b85353dc82e]  =  @Location
, [05d783c7-5517-4fbc-88b9-8e010e3f7bb3]  =  @BookTitle
, [6db26d3c-a477-4148-a527-8e331bf46619]  =  @Volume
, [441aeefc-94b4-4c5a-839f-962ff15a0624]  =  @CatalogNumber
, [b888a64d-4555-4d1d-a422-99a1511b1f65]  =  @PublisherUri
, [03163fdf-9d73-40bf-9120-ae22e2c3407c]  =  @DateSubmitted
, [b686fec9-0fdf-46f6-b5aa-b1441bad9ea5]  =  @Chapter
, [9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2]  =  @Number
, [1cef5926-ef98-4170-94d6-db4326671534]  =  @DateAccepted
, [62f20793-f341-42ad-8980-e358c6037763]  =  @Publisher
, [8db90ced-fd85-4ba7-9a9f-e61bb36cddca]  =  @Edition
, [95d00191-ec8a-4bef-a24e-ea0d7d132bfb]  =  @MonthPublished
, [92d45d1c-821b-4869-872b-f4355150a145]  =  @YearPublished
, [fd227620-fc28-4539-803e-f9390a8fe329]  =  @Organization
, [08bc5fa6-16e7-4b98-813f-fbe233736d6a]  =  @Series
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deletebd23247c26f441ef9ad4b1e677ec19ce]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insertba2ecb5d12814ba3bfbdb53fc3eec02c]
 @DateApproved datetime
, @DateRejected datetime
, @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[c051a74d-af00-45f1-8ef8-3c19bd14322c], 
[a8936969-6719-4639-b914-f271233d02c6], 
[80a0d8df-d9a7-488b-85fe-062bac9418f2], 
[e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6], 
[3f6b345a-d2f2-4cb1-a7b5-2a316854adc6], 
[e5989779-7dff-46c6-90e1-3a8be8c6d07b], 
[ecc77bac-77c4-4eb7-80db-3cd6821c8628], 
[9d3790de-e0e0-459b-8605-52b9f1470301], 
[32bc2d19-22ed-4514-bfed-7b85353dc82e], 
[05d783c7-5517-4fbc-88b9-8e010e3f7bb3], 
[6db26d3c-a477-4148-a527-8e331bf46619], 
[441aeefc-94b4-4c5a-839f-962ff15a0624], 
[b888a64d-4555-4d1d-a422-99a1511b1f65], 
[03163fdf-9d73-40bf-9120-ae22e2c3407c], 
[b686fec9-0fdf-46f6-b5aa-b1441bad9ea5], 
[9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2], 
[1cef5926-ef98-4170-94d6-db4326671534], 
[62f20793-f341-42ad-8980-e358c6037763], 
[8db90ced-fd85-4ba7-9a9f-e61bb36cddca], 
[95d00191-ec8a-4bef-a24e-ea0d7d132bfb], 
[92d45d1c-821b-4869-872b-f4355150a145], 
[fd227620-fc28-4539-803e-f9390a8fe329], 
[08bc5fa6-16e7-4b98-813f-fbe233736d6a], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@DateApproved, 
@DateRejected, 
@DatePublished, 
@DOI, 
@Pages, 
@Institution, 
@DayPublished, 
@PublisherAddress, 
@Location, 
@BookTitle, 
@Volume, 
@CatalogNumber, 
@PublisherUri, 
@DateSubmitted, 
@Chapter, 
@Number, 
@DateAccepted, 
@Publisher, 
@Edition, 
@MonthPublished, 
@YearPublished, 
@Organization, 
@Series, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'ba2ecb5d-1281-4ba3-bfbd-b53fc3eec02c', 
27

);


END
GO
CREATE PROCEDURE [Core].[Updateba2ecb5d12814ba3bfbdb53fc3eec02c]
 @DateApproved datetime
, @DateRejected datetime
, @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [c051a74d-af00-45f1-8ef8-3c19bd14322c]  =  @DateApproved
, [a8936969-6719-4639-b914-f271233d02c6]  =  @DateRejected
, [80a0d8df-d9a7-488b-85fe-062bac9418f2]  =  @DatePublished
, [e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6]  =  @DOI
, [3f6b345a-d2f2-4cb1-a7b5-2a316854adc6]  =  @Pages
, [e5989779-7dff-46c6-90e1-3a8be8c6d07b]  =  @Institution
, [ecc77bac-77c4-4eb7-80db-3cd6821c8628]  =  @DayPublished
, [9d3790de-e0e0-459b-8605-52b9f1470301]  =  @PublisherAddress
, [32bc2d19-22ed-4514-bfed-7b85353dc82e]  =  @Location
, [05d783c7-5517-4fbc-88b9-8e010e3f7bb3]  =  @BookTitle
, [6db26d3c-a477-4148-a527-8e331bf46619]  =  @Volume
, [441aeefc-94b4-4c5a-839f-962ff15a0624]  =  @CatalogNumber
, [b888a64d-4555-4d1d-a422-99a1511b1f65]  =  @PublisherUri
, [03163fdf-9d73-40bf-9120-ae22e2c3407c]  =  @DateSubmitted
, [b686fec9-0fdf-46f6-b5aa-b1441bad9ea5]  =  @Chapter
, [9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2]  =  @Number
, [1cef5926-ef98-4170-94d6-db4326671534]  =  @DateAccepted
, [62f20793-f341-42ad-8980-e358c6037763]  =  @Publisher
, [8db90ced-fd85-4ba7-9a9f-e61bb36cddca]  =  @Edition
, [95d00191-ec8a-4bef-a24e-ea0d7d132bfb]  =  @MonthPublished
, [92d45d1c-821b-4869-872b-f4355150a145]  =  @YearPublished
, [fd227620-fc28-4539-803e-f9390a8fe329]  =  @Organization
, [08bc5fa6-16e7-4b98-813f-fbe233736d6a]  =  @Series
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deleteba2ecb5d12814ba3bfbdb53fc3eec02c]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Inserta58aa716641a41dc9deebcd8a323c401]
 @From nvarchar(1024)
, @DateExchanged datetime
, @To nvarchar(4000)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[96302933-01ef-41b4-8399-7f946dd0d07f], 
[65db5eed-c9fd-452a-a289-a6034f072d9b], 
[496bd2ef-295d-4801-892d-b38e8338e6ff], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@From, 
@DateExchanged, 
@To, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'a58aa716-641a-41dc-9dee-bcd8a323c401', 
28

);


END
GO
CREATE PROCEDURE [Core].[Updatea58aa716641a41dc9deebcd8a323c401]
 @From nvarchar(1024)
, @DateExchanged datetime
, @To nvarchar(4000)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [96302933-01ef-41b4-8399-7f946dd0d07f]  =  @From
, [65db5eed-c9fd-452a-a289-a6034f072d9b]  =  @DateExchanged
, [496bd2ef-295d-4801-892d-b38e8338e6ff]  =  @To
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deletea58aa716641a41dc9deebcd8a323c401]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insertf0b6249c94f748d3a8cdc70a390b7403]
 @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'f0b6249c-94f7-48d3-a8cd-c70a390b7403', 
29

);


END
GO
CREATE PROCEDURE [Core].[Updatef0b6249c94f748d3a8cdc70a390b7403]
 @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deletef0b6249c94f748d3a8cdc70a390b7403]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Inserte314b72c021d48719403c8af9d929b10]
 @Report nvarchar(max)
, @Status nvarchar(4000)
, @Plan nvarchar(max)
, @Name nvarchar(1024)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[42bed4bc-be0a-4d4a-bb9a-679352689f48], 
[9f4c2262-8882-4a9f-a0bd-875dea047157], 
[0d977578-3e24-42d8-840f-e52a7e715830], 
[a0d288c5-a9ee-4872-b7f4-f815089037b7], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@Report, 
@Status, 
@Plan, 
@Name, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'e314b72c-021d-4871-9403-c8af9d929b10', 
30

);


END
GO
CREATE PROCEDURE [Core].[Updatee314b72c021d48719403c8af9d929b10]
 @Report nvarchar(max)
, @Status nvarchar(4000)
, @Plan nvarchar(max)
, @Name nvarchar(1024)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [42bed4bc-be0a-4d4a-bb9a-679352689f48]  =  @Report
, [9f4c2262-8882-4a9f-a0bd-875dea047157]  =  @Status
, [0d977578-3e24-42d8-840f-e52a7e715830]  =  @Plan
, [a0d288c5-a9ee-4872-b7f4-f815089037b7]  =  @Name
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deletee314b72c021d48719403c8af9d929b10]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Inserted6a2f1fa36e4a3fa828c972929ded7e]
 @Reference nvarchar(4000)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[09212252-409f-43fd-a6e1-3b0213423950], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@Reference, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'ed6a2f1f-a36e-4a3f-a828-c972929ded7e', 
31

);


END
GO
CREATE PROCEDURE [Core].[Updateed6a2f1fa36e4a3fa828c972929ded7e]
 @Reference nvarchar(4000)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [09212252-409f-43fd-a6e1-3b0213423950]  =  @Reference
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deleteed6a2f1fa36e4a3fa828c972929ded7e]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert73bf716d3f2845d98426cd31f7b3552a]
 @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[80a0d8df-d9a7-488b-85fe-062bac9418f2], 
[e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6], 
[3f6b345a-d2f2-4cb1-a7b5-2a316854adc6], 
[e5989779-7dff-46c6-90e1-3a8be8c6d07b], 
[ecc77bac-77c4-4eb7-80db-3cd6821c8628], 
[9d3790de-e0e0-459b-8605-52b9f1470301], 
[32bc2d19-22ed-4514-bfed-7b85353dc82e], 
[05d783c7-5517-4fbc-88b9-8e010e3f7bb3], 
[6db26d3c-a477-4148-a527-8e331bf46619], 
[441aeefc-94b4-4c5a-839f-962ff15a0624], 
[b888a64d-4555-4d1d-a422-99a1511b1f65], 
[03163fdf-9d73-40bf-9120-ae22e2c3407c], 
[b686fec9-0fdf-46f6-b5aa-b1441bad9ea5], 
[9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2], 
[1cef5926-ef98-4170-94d6-db4326671534], 
[62f20793-f341-42ad-8980-e358c6037763], 
[8db90ced-fd85-4ba7-9a9f-e61bb36cddca], 
[95d00191-ec8a-4bef-a24e-ea0d7d132bfb], 
[92d45d1c-821b-4869-872b-f4355150a145], 
[fd227620-fc28-4539-803e-f9390a8fe329], 
[08bc5fa6-16e7-4b98-813f-fbe233736d6a], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@DatePublished, 
@DOI, 
@Pages, 
@Institution, 
@DayPublished, 
@PublisherAddress, 
@Location, 
@BookTitle, 
@Volume, 
@CatalogNumber, 
@PublisherUri, 
@DateSubmitted, 
@Chapter, 
@Number, 
@DateAccepted, 
@Publisher, 
@Edition, 
@MonthPublished, 
@YearPublished, 
@Organization, 
@Series, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'73bf716d-3f28-45d9-8426-cd31f7b3552a', 
32

);


END
GO
CREATE PROCEDURE [Core].[Update73bf716d3f2845d98426cd31f7b3552a]
 @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [80a0d8df-d9a7-488b-85fe-062bac9418f2]  =  @DatePublished
, [e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6]  =  @DOI
, [3f6b345a-d2f2-4cb1-a7b5-2a316854adc6]  =  @Pages
, [e5989779-7dff-46c6-90e1-3a8be8c6d07b]  =  @Institution
, [ecc77bac-77c4-4eb7-80db-3cd6821c8628]  =  @DayPublished
, [9d3790de-e0e0-459b-8605-52b9f1470301]  =  @PublisherAddress
, [32bc2d19-22ed-4514-bfed-7b85353dc82e]  =  @Location
, [05d783c7-5517-4fbc-88b9-8e010e3f7bb3]  =  @BookTitle
, [6db26d3c-a477-4148-a527-8e331bf46619]  =  @Volume
, [441aeefc-94b4-4c5a-839f-962ff15a0624]  =  @CatalogNumber
, [b888a64d-4555-4d1d-a422-99a1511b1f65]  =  @PublisherUri
, [03163fdf-9d73-40bf-9120-ae22e2c3407c]  =  @DateSubmitted
, [b686fec9-0fdf-46f6-b5aa-b1441bad9ea5]  =  @Chapter
, [9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2]  =  @Number
, [1cef5926-ef98-4170-94d6-db4326671534]  =  @DateAccepted
, [62f20793-f341-42ad-8980-e358c6037763]  =  @Publisher
, [8db90ced-fd85-4ba7-9a9f-e61bb36cddca]  =  @Edition
, [95d00191-ec8a-4bef-a24e-ea0d7d132bfb]  =  @MonthPublished
, [92d45d1c-821b-4869-872b-f4355150a145]  =  @YearPublished
, [fd227620-fc28-4539-803e-f9390a8fe329]  =  @Organization
, [08bc5fa6-16e7-4b98-813f-fbe233736d6a]  =  @Series
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete73bf716d3f2845d98426cd31f7b3552a]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insertd2d6d8f750f74c048456d03c22534eb9]
 @License nvarchar(4000)
, @Copyright nvarchar(4000)
, @DateCopyrighted datetime
, @Duration int
, @Language nvarchar(128)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[2b8e690c-44c7-4670-b147-06b7b18d89f3], 
[3c35c424-0f62-44e5-82fe-2755a9eff1c2], 
[e94f81aa-1993-4726-9447-5ee237e58654], 
[56b9acf3-9e10-424e-a9f0-a6f5aab12a50], 
[3d773ba6-1955-47c4-9388-baef50b70327], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@License, 
@Copyright, 
@DateCopyrighted, 
@Duration, 
@Language, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'd2d6d8f7-50f7-4c04-8456-d03c22534eb9', 
33

);


END
GO
CREATE PROCEDURE [Core].[Updated2d6d8f750f74c048456d03c22534eb9]
 @License nvarchar(4000)
, @Copyright nvarchar(4000)
, @DateCopyrighted datetime
, @Duration int
, @Language nvarchar(128)
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [2b8e690c-44c7-4670-b147-06b7b18d89f3]  =  @License
, [3c35c424-0f62-44e5-82fe-2755a9eff1c2]  =  @Copyright
, [e94f81aa-1993-4726-9447-5ee237e58654]  =  @DateCopyrighted
, [56b9acf3-9e10-424e-a9f0-a6f5aab12a50]  =  @Duration
, [3d773ba6-1955-47c4-9388-baef50b70327]  =  @Language
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deleted2d6d8f750f74c048456d03c22534eb9]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert78d47d6099e643168e81d7317c70667f]
 @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'78d47d60-99e6-4316-8e81-d7317c70667f', 
34

);


END
GO
CREATE PROCEDURE [Core].[Update78d47d6099e643168e81d7317c70667f]
 @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete78d47d6099e643168e81d7317c70667f]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert367057b41bde448689c0d7b49956662c]
 @Journal nvarchar(256)
, @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[6ac95912-620a-4aeb-83e9-0ced9a61d22a], 
[80a0d8df-d9a7-488b-85fe-062bac9418f2], 
[e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6], 
[3f6b345a-d2f2-4cb1-a7b5-2a316854adc6], 
[e5989779-7dff-46c6-90e1-3a8be8c6d07b], 
[ecc77bac-77c4-4eb7-80db-3cd6821c8628], 
[9d3790de-e0e0-459b-8605-52b9f1470301], 
[32bc2d19-22ed-4514-bfed-7b85353dc82e], 
[05d783c7-5517-4fbc-88b9-8e010e3f7bb3], 
[6db26d3c-a477-4148-a527-8e331bf46619], 
[441aeefc-94b4-4c5a-839f-962ff15a0624], 
[b888a64d-4555-4d1d-a422-99a1511b1f65], 
[03163fdf-9d73-40bf-9120-ae22e2c3407c], 
[b686fec9-0fdf-46f6-b5aa-b1441bad9ea5], 
[9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2], 
[1cef5926-ef98-4170-94d6-db4326671534], 
[62f20793-f341-42ad-8980-e358c6037763], 
[8db90ced-fd85-4ba7-9a9f-e61bb36cddca], 
[95d00191-ec8a-4bef-a24e-ea0d7d132bfb], 
[92d45d1c-821b-4869-872b-f4355150a145], 
[fd227620-fc28-4539-803e-f9390a8fe329], 
[08bc5fa6-16e7-4b98-813f-fbe233736d6a], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@Journal, 
@DatePublished, 
@DOI, 
@Pages, 
@Institution, 
@DayPublished, 
@PublisherAddress, 
@Location, 
@BookTitle, 
@Volume, 
@CatalogNumber, 
@PublisherUri, 
@DateSubmitted, 
@Chapter, 
@Number, 
@DateAccepted, 
@Publisher, 
@Edition, 
@MonthPublished, 
@YearPublished, 
@Organization, 
@Series, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'367057b4-1bde-4486-89c0-d7b49956662c', 
35

);


END
GO
CREATE PROCEDURE [Core].[Update367057b41bde448689c0d7b49956662c]
 @Journal nvarchar(256)
, @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [6ac95912-620a-4aeb-83e9-0ced9a61d22a]  =  @Journal
, [80a0d8df-d9a7-488b-85fe-062bac9418f2]  =  @DatePublished
, [e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6]  =  @DOI
, [3f6b345a-d2f2-4cb1-a7b5-2a316854adc6]  =  @Pages
, [e5989779-7dff-46c6-90e1-3a8be8c6d07b]  =  @Institution
, [ecc77bac-77c4-4eb7-80db-3cd6821c8628]  =  @DayPublished
, [9d3790de-e0e0-459b-8605-52b9f1470301]  =  @PublisherAddress
, [32bc2d19-22ed-4514-bfed-7b85353dc82e]  =  @Location
, [05d783c7-5517-4fbc-88b9-8e010e3f7bb3]  =  @BookTitle
, [6db26d3c-a477-4148-a527-8e331bf46619]  =  @Volume
, [441aeefc-94b4-4c5a-839f-962ff15a0624]  =  @CatalogNumber
, [b888a64d-4555-4d1d-a422-99a1511b1f65]  =  @PublisherUri
, [03163fdf-9d73-40bf-9120-ae22e2c3407c]  =  @DateSubmitted
, [b686fec9-0fdf-46f6-b5aa-b1441bad9ea5]  =  @Chapter
, [9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2]  =  @Number
, [1cef5926-ef98-4170-94d6-db4326671534]  =  @DateAccepted
, [62f20793-f341-42ad-8980-e358c6037763]  =  @Publisher
, [8db90ced-fd85-4ba7-9a9f-e61bb36cddca]  =  @Edition
, [95d00191-ec8a-4bef-a24e-ea0d7d132bfb]  =  @MonthPublished
, [92d45d1c-821b-4869-872b-f4355150a145]  =  @YearPublished
, [fd227620-fc28-4539-803e-f9390a8fe329]  =  @Organization
, [08bc5fa6-16e7-4b98-813f-fbe233736d6a]  =  @Series
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete367057b41bde448689c0d7b49956662c]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Inserta2fda58832b842b0b35fdd9ecaa21a57]
 @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[80a0d8df-d9a7-488b-85fe-062bac9418f2], 
[e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6], 
[3f6b345a-d2f2-4cb1-a7b5-2a316854adc6], 
[e5989779-7dff-46c6-90e1-3a8be8c6d07b], 
[ecc77bac-77c4-4eb7-80db-3cd6821c8628], 
[9d3790de-e0e0-459b-8605-52b9f1470301], 
[32bc2d19-22ed-4514-bfed-7b85353dc82e], 
[05d783c7-5517-4fbc-88b9-8e010e3f7bb3], 
[6db26d3c-a477-4148-a527-8e331bf46619], 
[441aeefc-94b4-4c5a-839f-962ff15a0624], 
[b888a64d-4555-4d1d-a422-99a1511b1f65], 
[03163fdf-9d73-40bf-9120-ae22e2c3407c], 
[b686fec9-0fdf-46f6-b5aa-b1441bad9ea5], 
[9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2], 
[1cef5926-ef98-4170-94d6-db4326671534], 
[62f20793-f341-42ad-8980-e358c6037763], 
[8db90ced-fd85-4ba7-9a9f-e61bb36cddca], 
[95d00191-ec8a-4bef-a24e-ea0d7d132bfb], 
[92d45d1c-821b-4869-872b-f4355150a145], 
[fd227620-fc28-4539-803e-f9390a8fe329], 
[08bc5fa6-16e7-4b98-813f-fbe233736d6a], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@DatePublished, 
@DOI, 
@Pages, 
@Institution, 
@DayPublished, 
@PublisherAddress, 
@Location, 
@BookTitle, 
@Volume, 
@CatalogNumber, 
@PublisherUri, 
@DateSubmitted, 
@Chapter, 
@Number, 
@DateAccepted, 
@Publisher, 
@Edition, 
@MonthPublished, 
@YearPublished, 
@Organization, 
@Series, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'a2fda588-32b8-42b0-b35f-dd9ecaa21a57', 
36

);


END
GO
CREATE PROCEDURE [Core].[Updatea2fda58832b842b0b35fdd9ecaa21a57]
 @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [80a0d8df-d9a7-488b-85fe-062bac9418f2]  =  @DatePublished
, [e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6]  =  @DOI
, [3f6b345a-d2f2-4cb1-a7b5-2a316854adc6]  =  @Pages
, [e5989779-7dff-46c6-90e1-3a8be8c6d07b]  =  @Institution
, [ecc77bac-77c4-4eb7-80db-3cd6821c8628]  =  @DayPublished
, [9d3790de-e0e0-459b-8605-52b9f1470301]  =  @PublisherAddress
, [32bc2d19-22ed-4514-bfed-7b85353dc82e]  =  @Location
, [05d783c7-5517-4fbc-88b9-8e010e3f7bb3]  =  @BookTitle
, [6db26d3c-a477-4148-a527-8e331bf46619]  =  @Volume
, [441aeefc-94b4-4c5a-839f-962ff15a0624]  =  @CatalogNumber
, [b888a64d-4555-4d1d-a422-99a1511b1f65]  =  @PublisherUri
, [03163fdf-9d73-40bf-9120-ae22e2c3407c]  =  @DateSubmitted
, [b686fec9-0fdf-46f6-b5aa-b1441bad9ea5]  =  @Chapter
, [9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2]  =  @Number
, [1cef5926-ef98-4170-94d6-db4326671534]  =  @DateAccepted
, [62f20793-f341-42ad-8980-e358c6037763]  =  @Publisher
, [8db90ced-fd85-4ba7-9a9f-e61bb36cddca]  =  @Edition
, [95d00191-ec8a-4bef-a24e-ea0d7d132bfb]  =  @MonthPublished
, [92d45d1c-821b-4869-872b-f4355150a145]  =  @YearPublished
, [fd227620-fc28-4539-803e-f9390a8fe329]  =  @Organization
, [08bc5fa6-16e7-4b98-813f-fbe233736d6a]  =  @Series
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deletea2fda58832b842b0b35fdd9ecaa21a57]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insertcd4a41de8b5f4144aa9be4535c6ac949]
 @Category nvarchar(256)
, @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[22a62585-520b-44e2-9194-80b09e3e5c8d], 
[80a0d8df-d9a7-488b-85fe-062bac9418f2], 
[e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6], 
[3f6b345a-d2f2-4cb1-a7b5-2a316854adc6], 
[e5989779-7dff-46c6-90e1-3a8be8c6d07b], 
[ecc77bac-77c4-4eb7-80db-3cd6821c8628], 
[9d3790de-e0e0-459b-8605-52b9f1470301], 
[32bc2d19-22ed-4514-bfed-7b85353dc82e], 
[05d783c7-5517-4fbc-88b9-8e010e3f7bb3], 
[6db26d3c-a477-4148-a527-8e331bf46619], 
[441aeefc-94b4-4c5a-839f-962ff15a0624], 
[b888a64d-4555-4d1d-a422-99a1511b1f65], 
[03163fdf-9d73-40bf-9120-ae22e2c3407c], 
[b686fec9-0fdf-46f6-b5aa-b1441bad9ea5], 
[9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2], 
[1cef5926-ef98-4170-94d6-db4326671534], 
[62f20793-f341-42ad-8980-e358c6037763], 
[8db90ced-fd85-4ba7-9a9f-e61bb36cddca], 
[95d00191-ec8a-4bef-a24e-ea0d7d132bfb], 
[92d45d1c-821b-4869-872b-f4355150a145], 
[fd227620-fc28-4539-803e-f9390a8fe329], 
[08bc5fa6-16e7-4b98-813f-fbe233736d6a], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@Category, 
@DatePublished, 
@DOI, 
@Pages, 
@Institution, 
@DayPublished, 
@PublisherAddress, 
@Location, 
@BookTitle, 
@Volume, 
@CatalogNumber, 
@PublisherUri, 
@DateSubmitted, 
@Chapter, 
@Number, 
@DateAccepted, 
@Publisher, 
@Edition, 
@MonthPublished, 
@YearPublished, 
@Organization, 
@Series, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'cd4a41de-8b5f-4144-aa9b-e4535c6ac949', 
37

);


END
GO
CREATE PROCEDURE [Core].[Updatecd4a41de8b5f4144aa9be4535c6ac949]
 @Category nvarchar(256)
, @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [22a62585-520b-44e2-9194-80b09e3e5c8d]  =  @Category
, [80a0d8df-d9a7-488b-85fe-062bac9418f2]  =  @DatePublished
, [e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6]  =  @DOI
, [3f6b345a-d2f2-4cb1-a7b5-2a316854adc6]  =  @Pages
, [e5989779-7dff-46c6-90e1-3a8be8c6d07b]  =  @Institution
, [ecc77bac-77c4-4eb7-80db-3cd6821c8628]  =  @DayPublished
, [9d3790de-e0e0-459b-8605-52b9f1470301]  =  @PublisherAddress
, [32bc2d19-22ed-4514-bfed-7b85353dc82e]  =  @Location
, [05d783c7-5517-4fbc-88b9-8e010e3f7bb3]  =  @BookTitle
, [6db26d3c-a477-4148-a527-8e331bf46619]  =  @Volume
, [441aeefc-94b4-4c5a-839f-962ff15a0624]  =  @CatalogNumber
, [b888a64d-4555-4d1d-a422-99a1511b1f65]  =  @PublisherUri
, [03163fdf-9d73-40bf-9120-ae22e2c3407c]  =  @DateSubmitted
, [b686fec9-0fdf-46f6-b5aa-b1441bad9ea5]  =  @Chapter
, [9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2]  =  @Number
, [1cef5926-ef98-4170-94d6-db4326671534]  =  @DateAccepted
, [62f20793-f341-42ad-8980-e358c6037763]  =  @Publisher
, [8db90ced-fd85-4ba7-9a9f-e61bb36cddca]  =  @Edition
, [95d00191-ec8a-4bef-a24e-ea0d7d132bfb]  =  @MonthPublished
, [92d45d1c-821b-4869-872b-f4355150a145]  =  @YearPublished
, [fd227620-fc28-4539-803e-f9390a8fe329]  =  @Organization
, [08bc5fa6-16e7-4b98-813f-fbe233736d6a]  =  @Series
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deletecd4a41de8b5f4144aa9be4535c6ac949]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert8047506b8c7d432093b3e515c868cf48]
 @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[80a0d8df-d9a7-488b-85fe-062bac9418f2], 
[e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6], 
[3f6b345a-d2f2-4cb1-a7b5-2a316854adc6], 
[e5989779-7dff-46c6-90e1-3a8be8c6d07b], 
[ecc77bac-77c4-4eb7-80db-3cd6821c8628], 
[9d3790de-e0e0-459b-8605-52b9f1470301], 
[32bc2d19-22ed-4514-bfed-7b85353dc82e], 
[05d783c7-5517-4fbc-88b9-8e010e3f7bb3], 
[6db26d3c-a477-4148-a527-8e331bf46619], 
[441aeefc-94b4-4c5a-839f-962ff15a0624], 
[b888a64d-4555-4d1d-a422-99a1511b1f65], 
[03163fdf-9d73-40bf-9120-ae22e2c3407c], 
[b686fec9-0fdf-46f6-b5aa-b1441bad9ea5], 
[9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2], 
[1cef5926-ef98-4170-94d6-db4326671534], 
[62f20793-f341-42ad-8980-e358c6037763], 
[8db90ced-fd85-4ba7-9a9f-e61bb36cddca], 
[95d00191-ec8a-4bef-a24e-ea0d7d132bfb], 
[92d45d1c-821b-4869-872b-f4355150a145], 
[fd227620-fc28-4539-803e-f9390a8fe329], 
[08bc5fa6-16e7-4b98-813f-fbe233736d6a], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@DatePublished, 
@DOI, 
@Pages, 
@Institution, 
@DayPublished, 
@PublisherAddress, 
@Location, 
@BookTitle, 
@Volume, 
@CatalogNumber, 
@PublisherUri, 
@DateSubmitted, 
@Chapter, 
@Number, 
@DateAccepted, 
@Publisher, 
@Edition, 
@MonthPublished, 
@YearPublished, 
@Organization, 
@Series, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'8047506b-8c7d-4320-93b3-e515c868cf48', 
38

);


END
GO
CREATE PROCEDURE [Core].[Update8047506b8c7d432093b3e515c868cf48]
 @DatePublished datetime
, @DOI nvarchar(256)
, @Pages nvarchar(1024)
, @Institution nvarchar(1024)
, @DayPublished int
, @PublisherAddress nvarchar(1024)
, @Location nvarchar(1024)
, @BookTitle nvarchar(256)
, @Volume nvarchar(256)
, @CatalogNumber nvarchar(1024)
, @PublisherUri nvarchar(1024)
, @DateSubmitted datetime
, @Chapter nvarchar(256)
, @Number nvarchar(256)
, @DateAccepted datetime
, @Publisher nvarchar(256)
, @Edition nvarchar(256)
, @MonthPublished int
, @YearPublished int
, @Organization nvarchar(1024)
, @Series nvarchar(256)
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [80a0d8df-d9a7-488b-85fe-062bac9418f2]  =  @DatePublished
, [e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6]  =  @DOI
, [3f6b345a-d2f2-4cb1-a7b5-2a316854adc6]  =  @Pages
, [e5989779-7dff-46c6-90e1-3a8be8c6d07b]  =  @Institution
, [ecc77bac-77c4-4eb7-80db-3cd6821c8628]  =  @DayPublished
, [9d3790de-e0e0-459b-8605-52b9f1470301]  =  @PublisherAddress
, [32bc2d19-22ed-4514-bfed-7b85353dc82e]  =  @Location
, [05d783c7-5517-4fbc-88b9-8e010e3f7bb3]  =  @BookTitle
, [6db26d3c-a477-4148-a527-8e331bf46619]  =  @Volume
, [441aeefc-94b4-4c5a-839f-962ff15a0624]  =  @CatalogNumber
, [b888a64d-4555-4d1d-a422-99a1511b1f65]  =  @PublisherUri
, [03163fdf-9d73-40bf-9120-ae22e2c3407c]  =  @DateSubmitted
, [b686fec9-0fdf-46f6-b5aa-b1441bad9ea5]  =  @Chapter
, [9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2]  =  @Number
, [1cef5926-ef98-4170-94d6-db4326671534]  =  @DateAccepted
, [62f20793-f341-42ad-8980-e358c6037763]  =  @Publisher
, [8db90ced-fd85-4ba7-9a9f-e61bb36cddca]  =  @Edition
, [95d00191-ec8a-4bef-a24e-ea0d7d132bfb]  =  @MonthPublished
, [92d45d1c-821b-4869-872b-f4355150a145]  =  @YearPublished
, [fd227620-fc28-4539-803e-f9390a8fe329]  =  @Organization
, [08bc5fa6-16e7-4b98-813f-fbe233736d6a]  =  @Series
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete8047506b8c7d432093b3e515c868cf48]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Inserte47c360d3a1e4d618e3de979b1265adb]
 @DatePresented datetime
, @Length bigint
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[7ac21049-f4c2-42d5-bd20-21a2563d4eef], 
[0e2d2490-c627-493f-9a4c-fc9c306e51c5], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@DatePresented, 
@Length, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'e47c360d-3a1e-4d61-8e3d-e979b1265adb', 
39

);


END
GO
CREATE PROCEDURE [Core].[Updatee47c360d3a1e4d618e3de979b1265adb]
 @DatePresented datetime
, @Length bigint
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [7ac21049-f4c2-42d5-bd20-21a2563d4eef]  =  @DatePresented
, [0e2d2490-c627-493f-9a4c-fc9c306e51c5]  =  @Length
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Deletee47c360d3a1e4d618e3de979b1265adb]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Insert7b65d54a4e78440995057e3e09d23c68]
 @Count int
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END


INSERT INTO [Core].[Resource]
( 
[54f98dcb-c008-4e66-9c76-353a1a48edac], 
[6b8b39ac-1f83-48d0-8a06-259a6cc55742], 
[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b], 
[3e958c5f-bd06-470a-8f90-324eca205160], 
[d68071c0-2560-4368-b41b-6a825000ec07], 
[bec19f51-7fe7-45a8-9a7a-88b008ade630], 
[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b], 
[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd], 
[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519], 
[54104144-2aba-4d7c-af53-f9e06deb66ba], 
[5e27e1b3-1965-44ee-8787-fbbdbc2f300d], 
[165e8189-f6a3-42c6-ba9f-bddaa81364df], 
[Title], 
[DateModified], 
[Id], 
[DateAdded], 
[Uri], 
[Description], 
[ResourceTypeId], 
[Discriminator]

)
VALUES
(
@Count, 
@License, 
@DateValidUntil, 
@Notes, 
@Abstract, 
@Copyright, 
@DateValidFrom, 
@Language, 
@DateAvailableUntil, 
@DateAvailableFrom, 
@DateCopyrighted, 
@Scope, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'7b65d54a-4e78-4409-9505-7e3e09d23c68', 
40

);


END
GO
CREATE PROCEDURE [Core].[Update7b65d54a4e78440995057e3e09d23c68]
 @Count int
, @License nvarchar(4000)
, @DateValidUntil datetime
, @Notes nvarchar(max)
, @Abstract nvarchar(max)
, @Copyright nvarchar(4000)
, @DateValidFrom datetime
, @Language nvarchar(128)
, @DateAvailableUntil datetime
, @DateAvailableFrom datetime
, @DateCopyrighted datetime
, @Scope nvarchar(128)
, @Title nvarchar(425)
, @DateModified datetime
, @Id uniqueidentifier
, @DateAdded datetime
, @Uri nvarchar(1024)
, @Description nvarchar(max)

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 
IF @Id IS NULL
BEGIN
	RAISERROR (N'Cannot insert the value NULL into column [Id].', 16, 1);
	RETURN -1;
END



UPDATE [Core].[Resource] 
SET	
 [54f98dcb-c008-4e66-9c76-353a1a48edac]  =  @Count
, [6b8b39ac-1f83-48d0-8a06-259a6cc55742]  =  @License
, [54cbe4dd-9b7e-4bde-8fac-3206274dbc8b]  =  @DateValidUntil
, [3e958c5f-bd06-470a-8f90-324eca205160]  =  @Notes
, [d68071c0-2560-4368-b41b-6a825000ec07]  =  @Abstract
, [bec19f51-7fe7-45a8-9a7a-88b008ade630]  =  @Copyright
, [e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b]  =  @DateValidFrom
, [8ccbc846-afd9-4a5f-9f4c-a06ed93668fd]  =  @Language
, [13a5164e-cdc5-4fd2-b2ca-cf047a3f2519]  =  @DateAvailableUntil
, [54104144-2aba-4d7c-af53-f9e06deb66ba]  =  @DateAvailableFrom
, [5e27e1b3-1965-44ee-8787-fbbdbc2f300d]  =  @DateCopyrighted
, [165e8189-f6a3-42c6-ba9f-bddaa81364df]  =  @Scope
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END
GO
CREATE PROCEDURE [Core].[Delete7b65d54a4e78440995057e3e09d23c68]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END
GO
EXEC [Core].[DropUniqueIndexesFromMetadata]
GO
INSERT INTO [Core].[DataModelModule]([Id],[Namespace],[Uri],[Description],[IsMsShipped]) 
    VALUES('6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','Zentity.ScholarlyWorks','urn:zentity/module/zentity-scholarly-work','The scholarly works data model.',1)
GO
EXEC [Core].[CreateOrUpdateResourceType] 'f0b6249c-94f7-48d3-a8cd-c70a390b7403','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','d2bd64df-6609-4ea4-ae99-9669da69bf7a','ScholarlyWorkItem','urn:zentity/module/zentity-scholarly-works/resource-type/scholarly-work-item','Represents the base resource type for all the types in ScholarlyWorks module.',29
GO
EXEC [Core].[CreateOrUpdateResourceType] '3064535f-9989-489c-a7d0-24a40674494a','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','f0b6249c-94f7-48d3-a8cd-c70a390b7403','Tag','urn:zentity/module/zentity-scholarly-works/resource-type/tag','Represents a tag.',11
GO
EXEC [Core].[CreateOrUpdateResourceType] 'f46d9cb1-11be-445b-9067-5d558d8714c4','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','f0b6249c-94f7-48d3-a8cd-c70a390b7403','Download','urn:zentity/module/zentity-scholarly-works/resource-type/download','Represents a download.',15
GO
EXEC [Core].[CreateOrUpdateResourceType] '1f2c6537-db1a-42f8-84da-7fba29803174','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','f0b6249c-94f7-48d3-a8cd-c70a390b7403','Contact','urn:zentity/module/zentity-scholarly-works/resource-type/contact','Represents a contact.',21
GO
EXEC [Core].[CreateOrUpdateResourceType] 'c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','f0b6249c-94f7-48d3-a8cd-c70a390b7403','ScholarlyWork','urn:zentity/module/zentity-scholarly-works/resource-type/scholarly-work','Represents a scholarly work.',25
GO
EXEC [Core].[CreateOrUpdateResourceType] 'a58aa716-641a-41dc-9dee-bcd8a323c401','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','f0b6249c-94f7-48d3-a8cd-c70a390b7403','PersonalCommunication','urn:zentity/module/zentity-scholarly-works/resource-type/personal-communication','Represents a personal communication.',28
GO
EXEC [Core].[CreateOrUpdateResourceType] 'ed6a2f1f-a36e-4a3f-a828-c972929ded7e','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','f0b6249c-94f7-48d3-a8cd-c70a390b7403','ElectronicSource','urn:zentity/module/zentity-scholarly-works/resource-type/electronic-source','Represents an elecronic source external to the repository.',31
GO
EXEC [Core].[CreateOrUpdateResourceType] 'd2d6d8f7-50f7-4c04-8456-d03c22534eb9','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','f0b6249c-94f7-48d3-a8cd-c70a390b7403','Media','urn:zentity/module/zentity-scholarly-works/resource-type/media','Represents a media.',33
GO
EXEC [Core].[CreateOrUpdateResourceType] '78d47d60-99e6-4316-8e81-d7317c70667f','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','f0b6249c-94f7-48d3-a8cd-c70a390b7403','CategoryNode','urn:zentity/module/zentity-scholarly-works/resource-type/category-node','Represents a category node. Nodes can be used to formulate hierarchies based on subject area, departments etc.',34
GO
EXEC [Core].[CreateOrUpdateResourceType] '346958a3-2ccb-4bdd-a810-1085c2ce3e65','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','f46d9cb1-11be-445b-9067-5d558d8714c4','Data','urn:zentity/module/zentity-scholarly-works/resource-type/data','Represents some binary data.',5
GO
EXEC [Core].[CreateOrUpdateResourceType] 'c73f8d6f-0304-4575-a614-219dbf52fc89','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','f46d9cb1-11be-445b-9067-5d558d8714c4','Software','urn:zentity/module/zentity-scholarly-works/resource-type/software','Represents a software.',9
GO
EXEC [Core].[CreateOrUpdateResourceType] '4c89074e-f0d6-4706-8700-6d2e0d1afe98','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','f46d9cb1-11be-445b-9067-5d558d8714c4','Code','urn:zentity/module/zentity-scholarly-works/resource-type/code','Represents a piece of code.',18
GO
EXEC [Core].[CreateOrUpdateResourceType] '479452ef-4aac-4f1f-8e00-12bf54cce9a9','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','1f2c6537-db1a-42f8-84da-7fba29803174','Organization','urn:zentity/module/zentity-scholarly-works/resource-type/organization','Represents an organization.',6
GO
EXEC [Core].[CreateOrUpdateResourceType] '0e3efa4a-7bfa-471c-b7bd-17685cdef487','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','1f2c6537-db1a-42f8-84da-7fba29803174','Person','urn:zentity/module/zentity-scholarly-works/resource-type/person','Represents a person.',7
GO
EXEC [Core].[CreateOrUpdateResourceType] '750bd9f2-aa9d-4a03-82c0-739fceafe7b2','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Lecture','urn:zentity/module/zentity-scholarly-works/resource-type/lecture','Represents a lecture.',19
GO
EXEC [Core].[CreateOrUpdateResourceType] 'e314b72c-021d-4871-9403-c8af9d929b10','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Experiment','urn:zentity/module/zentity-scholarly-works/resource-type/experiment','Represents an experiment.',30
GO
EXEC [Core].[CreateOrUpdateResourceType] 'a2fda588-32b8-42b0-b35f-dd9ecaa21a57','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Publication','urn:zentity/module/zentity-scholarly-works/resource-type/publication','Represents a publication.',36
GO
EXEC [Core].[CreateOrUpdateResourceType] 'e47c360d-3a1e-4d61-8e3d-e979b1265adb','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Tutorial','urn:zentity/module/zentity-scholarly-works/resource-type/tutorial','Represents a tutorial.',39
GO
EXEC [Core].[CreateOrUpdateResourceType] '7b65d54a-4e78-4409-9505-7e3e09d23c68','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','ScholarlyWorkContainer','urn:zentity/module/zentity-scholarly-works/resource-type/scholarly-work-container','Represents a container for scholarly works.',40
GO
EXEC [Core].[CreateOrUpdateResourceType] '3916037e-40ee-4eef-a4ee-0b071217c266','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','a58aa716-641a-41dc-9dee-bcd8a323c401','Letter','urn:zentity/module/zentity-scholarly-works/resource-type/letter','Represents a letter.',3
GO
EXEC [Core].[CreateOrUpdateResourceType] 'fbad8cd7-68d0-465f-b616-2d4f2f1964b5','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','a58aa716-641a-41dc-9dee-bcd8a323c401','Email','urn:zentity/module/zentity-scholarly-works/resource-type/email','Represents an e-mail.',12
GO
EXEC [Core].[CreateOrUpdateResourceType] 'e82b8970-583f-42b5-83c4-0ff7135bb0e7','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','d2d6d8f7-50f7-4c04-8456-d03c22534eb9','Image','urn:zentity/module/zentity-scholarly-works/resource-type/image','Represents an image.',4
GO
EXEC [Core].[CreateOrUpdateResourceType] '8a3e790b-ec77-4ab3-abbc-30008fdb9ebc','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','d2d6d8f7-50f7-4c04-8456-d03c22534eb9','Audio','urn:zentity/module/zentity-scholarly-works/resource-type/audio','Represents an audio resource.',13
GO
EXEC [Core].[CreateOrUpdateResourceType] '807b649a-8328-4caa-a29d-7a61048fcef2','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','d2d6d8f7-50f7-4c04-8456-d03c22534eb9','Video','urn:zentity/module/zentity-scholarly-works/resource-type/video','Represents a video.',20
GO
EXEC [Core].[CreateOrUpdateResourceType] '23d6f2a9-3819-44de-9cf0-1eb683141dfe','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Book','urn:zentity/module/zentity-scholarly-works/resource-type/book','Represents a book.',8
GO
EXEC [Core].[CreateOrUpdateResourceType] 'b0eb981e-b5aa-48c0-bf97-5622abef1e6e','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Thesis','urn:zentity/module/zentity-scholarly-works/resource-type/thesis','Represents a thesis.',14
GO
EXEC [Core].[CreateOrUpdateResourceType] '2961c44e-7252-4814-9ca1-5ebc584d4932','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Booklet','urn:zentity/module/zentity-scholarly-works/resource-type/booklet','Represents a booklet.',16
GO
EXEC [Core].[CreateOrUpdateResourceType] 'c91643ff-e870-4a12-b140-6040a989fd6d','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Chapter','urn:zentity/module/zentity-scholarly-works/resource-type/chapter','Represents a chapter.',17
GO
EXEC [Core].[CreateOrUpdateResourceType] '1b698930-7cc1-4535-888e-a489eece878d','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Journal','urn:zentity/module/zentity-scholarly-works/resource-type/journal','Represents a journal.',24
GO
EXEC [Core].[CreateOrUpdateResourceType] 'bd23247c-26f4-41ef-9ad4-b1e677ec19ce','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Proceedings','urn:zentity/module/zentity-scholarly-works/resource-type/proceedings','Represents a proceedings.',26
GO
EXEC [Core].[CreateOrUpdateResourceType] 'ba2ecb5d-1281-4ba3-bfbd-b53fc3eec02c','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Patent','urn:zentity/module/zentity-scholarly-works/resource-type/patent','Represents a patent.',27
GO
EXEC [Core].[CreateOrUpdateResourceType] '73bf716d-3f28-45d9-8426-cd31f7b3552a','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Unpublished','urn:zentity/module/zentity-scholarly-works/resource-type/unpublished','Represents an unpublished work.',32
GO
EXEC [Core].[CreateOrUpdateResourceType] '367057b4-1bde-4486-89c0-d7b49956662c','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','JournalArticle','urn:zentity/module/zentity-scholarly-works/resource-type/journal-article','Represents a journal article.',35
GO
EXEC [Core].[CreateOrUpdateResourceType] 'cd4a41de-8b5f-4144-aa9b-e4535c6ac949','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','TechnicalReport','urn:zentity/module/zentity-scholarly-works/resource-type/technical-report','Represents a technical report.',37
GO
EXEC [Core].[CreateOrUpdateResourceType] '8047506b-8c7d-4320-93b3-e515c868cf48','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Manual','urn:zentity/module/zentity-scholarly-works/resource-type/manual','Represents a manual.',38
GO
EXEC [Core].[CreateOrUpdateResourceType] 'b21eec56-9b7a-4f19-9c61-23520fd674b0','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','b0eb981e-b5aa-48c0-bf97-5622abef1e6e','ThesisPhD','urn:zentity/module/zentity-scholarly-works/resource-type/thesisphd','Represents a PhD thesis.',10
GO
EXEC [Core].[CreateOrUpdateResourceType] '44c2b18f-a1ec-4ded-abb0-9c4ef1b1e740','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','b0eb981e-b5aa-48c0-bf97-5622abef1e6e','ThesisMsc','urn:zentity/module/zentity-scholarly-works/resource-type/thesismsc','Represents an Msc thesis.',23
GO
EXEC [Core].[CreateOrUpdateResourceType] '798fc6e5-d57d-4027-b0da-8b39e1a9dfe3','6fe29bdb-eaf0-430b-a6d8-0e16a13b82c3','bd23247c-26f4-41ef-9ad4-b1e677ec19ce','ProceedingsArticle','urn:zentity/module/zentity-scholarly-works/resource-type/proceedings-article','Represents a proceedings article.',22
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'c7a2b2e1-49b3-4ca0-a1a1-6e7b114007e4','0e3efa4a-7bfa-471c-b7bd-17685cdef487','LastName','urn:zentity/module/zentity-scholarly-works/resource-type/person/property/lastname','Gets or sets the last name of this person.','String',1,256,0,0,'Resource','c7a2b2e1-49b3-4ca0-a1a1-6e7b114007e4'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '3d82f5e2-b84a-4422-a0be-82317ad5059f','0e3efa4a-7bfa-471c-b7bd-17685cdef487','FirstName','urn:zentity/module/zentity-scholarly-works/resource-type/person/property/firstname','Gets or sets the first name of this person.','String',1,256,0,0,'Resource','3d82f5e2-b84a-4422-a0be-82317ad5059f'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '95d6858e-6294-4572-8686-c2c2c8dddd17','0e3efa4a-7bfa-471c-b7bd-17685cdef487','MiddleName','urn:zentity/module/zentity-scholarly-works/resource-type/person/property/middlename','Gets or sets the middle name of this person.','String',1,256,0,0,'Resource','95d6858e-6294-4572-8686-c2c2c8dddd17'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'a23f94c6-6344-4ba6-ae90-63e49743a0d1','23d6f2a9-3819-44de-9cf0-1eb683141dfe','ChangeHistory','urn:zentity/module/zentity-scholarly-works/resource-type/book/property/changehistory','Gets or sets the change history of this book. E.g. the datetime information of when this book was created, edited etc.','String',1,-1,0,0,'Resource','a23f94c6-6344-4ba6-ae90-63e49743a0d1'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'bc13df5d-58dc-4fc7-8010-ec7eb04ac0c0','23d6f2a9-3819-44de-9cf0-1eb683141dfe','ISBN','urn:zentity/module/zentity-scholarly-works/resource-type/book/property/isbn','Gets or sets the International Standard Book Number of this book.','String',1,256,0,0,'Resource','bc13df5d-58dc-4fc7-8010-ec7eb04ac0c0'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'ea645ca2-11e7-4746-af99-667274066311','3064535f-9989-489c-a7d0-24a40674494a','Name','urn:zentity/module/zentity-scholarly-works/resource-type/tag/property/name','Gets or sets the name of this tag.','String',0,50,0,0,'Resource','ea645ca2-11e7-4746-af99-667274066311'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'a8f0994e-bc4e-4346-886d-9ffbde0940a8','fbad8cd7-68d0-465f-b616-2d4f2f1964b5','Subject','urn:zentity/module/zentity-scholarly-works/resource-type/email/property/subject','Gets or sets the subject of this e-mail.','String',1,256,0,0,'Resource','a8f0994e-bc4e-4346-886d-9ffbde0940a8'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '7dc69c44-9efb-46b9-96e5-24b44e7a7f8b','8a3e790b-ec77-4ab3-abbc-30008fdb9ebc','Codec','urn:zentity/module/zentity-scholarly-works/resource-type/audio/property/codec','Gets or sets the codec information for the audio.','String',1,4000,0,0,'Resource','7dc69c44-9efb-46b9-96e5-24b44e7a7f8b'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '2b83b84e-b8d2-453a-89be-833e459d6bff','8a3e790b-ec77-4ab3-abbc-30008fdb9ebc','BitRate','urn:zentity/module/zentity-scholarly-works/resource-type/audio/property/bitrate','Gets or sets the number of bits transmitted or received per second. E.g. 1.4 Mbit/s for CD quality.','Int32',1,0,0,0,'Resource','2b83b84e-b8d2-453a-89be-833e459d6bff'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '49ec0c5e-1c52-4136-9966-9d437413c829','8a3e790b-ec77-4ab3-abbc-30008fdb9ebc','Mode','urn:zentity/module/zentity-scholarly-works/resource-type/audio/property/mode','Gets or sets the mode of this audio. E.g stereo, 5.1 audio etc.','String',1,4000,0,0,'Resource','49ec0c5e-1c52-4136-9966-9d437413c829'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '0bb4e6f6-7d38-4773-a301-2c83810923e8','f46d9cb1-11be-445b-9067-5d558d8714c4','VersionInformation','urn:zentity/module/zentity-scholarly-works/resource-type/download/property/versioninformation','Gets or sets the version information of this download.','String',1,256,0,0,'Resource','0bb4e6f6-7d38-4773-a301-2c83810923e8'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'be528997-17b3-405c-a609-58bb560f36b9','f46d9cb1-11be-445b-9067-5d558d8714c4','Copyright','urn:zentity/module/zentity-scholarly-works/resource-type/download/property/copyright','Gets or sets the copyright of this download.','String',1,4000,0,0,'Resource','be528997-17b3-405c-a609-58bb560f36b9'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'c82db7be-893e-4886-bbaa-63f530d5c960','f46d9cb1-11be-445b-9067-5d558d8714c4','HardwareRequirements','urn:zentity/module/zentity-scholarly-works/resource-type/download/property/hardwarerequirements','Gets or sets the hardware requirements of this download.','String',1,4000,0,0,'Resource','c82db7be-893e-4886-bbaa-63f530d5c960'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '7df00974-90e1-4abb-816b-8da453b037e9','f46d9cb1-11be-445b-9067-5d558d8714c4','EULA','urn:zentity/module/zentity-scholarly-works/resource-type/download/property/eula','Gets or sets the end user license agreement for this download.','String',1,1024,0,0,'Resource','7df00974-90e1-4abb-816b-8da453b037e9'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '97f83304-bcd9-45f6-9256-9bdc093c52b5','f46d9cb1-11be-445b-9067-5d558d8714c4','OperatingSystem','urn:zentity/module/zentity-scholarly-works/resource-type/download/property/operatingsystem','Gets or sets the operating system requirements for this download.','String',1,4000,0,0,'Resource','97f83304-bcd9-45f6-9256-9bdc093c52b5'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'b597448f-76f8-4663-8b6f-9c04f24194b5','f46d9cb1-11be-445b-9067-5d558d8714c4','DownloadRequirements','urn:zentity/module/zentity-scholarly-works/resource-type/download/property/downloadrequirements','Gets or sets other requirements for this download.','String',1,4000,0,0,'Resource','b597448f-76f8-4663-8b6f-9c04f24194b5'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '290fee5b-da0e-42a9-9d80-a10826572733','f46d9cb1-11be-445b-9067-5d558d8714c4','SystemRequirements','urn:zentity/module/zentity-scholarly-works/resource-type/download/property/systemrequirements','Gets or sets the environment details for this download.','String',1,4000,0,0,'Resource','290fee5b-da0e-42a9-9d80-a10826572733'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '4283e5a5-96fe-47fe-94be-de1f0caa9f76','f46d9cb1-11be-445b-9067-5d558d8714c4','Language','urn:zentity/module/zentity-scholarly-works/resource-type/download/property/language','Gets or sets the language of this download.','String',1,128,0,0,'Resource','4283e5a5-96fe-47fe-94be-de1f0caa9f76'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '9fd938f9-da4a-4541-bcbe-fc6edb85cc2c','f46d9cb1-11be-445b-9067-5d558d8714c4','License','urn:zentity/module/zentity-scholarly-works/resource-type/download/property/license','Gets or sets the license of this download.','String',1,4000,0,0,'Resource','9fd938f9-da4a-4541-bcbe-fc6edb85cc2c'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '4b96b254-d594-4576-a6ab-ff90938d6910','f46d9cb1-11be-445b-9067-5d558d8714c4','DateCopyrighted','urn:zentity/module/zentity-scholarly-works/resource-type/download/property/datecopyrighted','Gets or sets the copyright date for this download.','DateTime',1,0,0,0,'Resource','4b96b254-d594-4576-a6ab-ff90938d6910'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '4326eb60-7271-43a5-9436-2d0766c353b9','4c89074e-f0d6-4706-8700-6d2e0d1afe98','ProgrammingLanguage','urn:zentity/module/zentity-scholarly-works/resource-type/code/property/programminglanguage','Gets or sets the programming language for this piece of code.','String',1,256,0,0,'Resource','4326eb60-7271-43a5-9436-2d0766c353b9'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '158a2898-edec-47d8-937a-b37269df2aaa','4c89074e-f0d6-4706-8700-6d2e0d1afe98','Technology','urn:zentity/module/zentity-scholarly-works/resource-type/code/property/technology','Gets or sets the technology required for this code.','String',1,256,0,0,'Resource','158a2898-edec-47d8-937a-b37269df2aaa'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'eaa610b7-cf35-4393-bf19-0fb7af9a128a','750bd9f2-aa9d-4a03-82c0-739fceafe7b2','Series','urn:zentity/module/zentity-scholarly-works/resource-type/lecture/property/series','Gets or sets the lecture series.','String',1,256,0,0,'Resource','eaa610b7-cf35-4393-bf19-0fb7af9a128a'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '2af50485-aa00-4c0a-9d2c-381720b7c7b3','750bd9f2-aa9d-4a03-82c0-739fceafe7b2','Image','urn:zentity/module/zentity-scholarly-works/resource-type/lecture/property/image','Gets or sets an image associated with the lecture.','Binary',1,-1,0,0,'Resource','2af50485-aa00-4c0a-9d2c-381720b7c7b3'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '25b52405-1f7a-4c9a-b904-591d4e29c73c','750bd9f2-aa9d-4a03-82c0-739fceafe7b2','Audience','urn:zentity/module/zentity-scholarly-works/resource-type/lecture/property/audience','Gets or sets the lecture audience.','String',1,4000,0,0,'Resource','25b52405-1f7a-4c9a-b904-591d4e29c73c'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '67cbb24d-0c60-4b65-adfa-7f6c60ad5935','750bd9f2-aa9d-4a03-82c0-739fceafe7b2','Venue','urn:zentity/module/zentity-scholarly-works/resource-type/lecture/property/venue','Gets or sets the lecture venue.','String',1,1024,0,0,'Resource','67cbb24d-0c60-4b65-adfa-7f6c60ad5935'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '02dbf59e-ff45-41e8-9320-a702ebdadba7','750bd9f2-aa9d-4a03-82c0-739fceafe7b2','DateEnd','urn:zentity/module/zentity-scholarly-works/resource-type/lecture/property/dateend','Gets or sets the end datetime of the lecture.','DateTime',1,0,0,0,'Resource','02dbf59e-ff45-41e8-9320-a702ebdadba7'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '22e98699-e5c6-4466-af64-be9dc28d7dc5','750bd9f2-aa9d-4a03-82c0-739fceafe7b2','DateStart','urn:zentity/module/zentity-scholarly-works/resource-type/lecture/property/datestart','Gets or sets the start datetime of the lecture.','DateTime',1,0,0,0,'Resource','22e98699-e5c6-4466-af64-be9dc28d7dc5'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '5108112d-29f5-4421-8bac-058f2f5bff4e','807b649a-8328-4caa-a29d-7a61048fcef2','Stereoscopic','urn:zentity/module/zentity-scholarly-works/resource-type/video/property/stereoscopic','Gets or sets the stereoscopic capabilities of this video. It is a technique of recording three-dimensional visual information.','Boolean',1,0,0,0,'Resource','5108112d-29f5-4421-8bac-058f2f5bff4e'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'a2733469-7db1-4a5b-9316-0deec2623e67','807b649a-8328-4caa-a29d-7a61048fcef2','Resolution','urn:zentity/module/zentity-scholarly-works/resource-type/video/property/resolution','Gets or sets the resolution of this video generally expressed in pixels, horizontal and vertical scan lines, voxels etc.','String',1,256,0,0,'Resource','a2733469-7db1-4a5b-9316-0deec2623e67'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '86f59c02-d498-47d2-81dc-2a52138aadab','807b649a-8328-4caa-a29d-7a61048fcef2','PSNR','urn:zentity/module/zentity-scholarly-works/resource-type/video/property/psnr','Gets or sets the peak signal-to-noise ratio. This is used to judge the video quality.','Decimal',1,0,0,18,'Resource','86f59c02-d498-47d2-81dc-2a52138aadab'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'dd35d7a1-7468-4625-9133-2b90b7e07251','807b649a-8328-4caa-a29d-7a61048fcef2','Director','urn:zentity/module/zentity-scholarly-works/resource-type/video/property/director','Gets or sets the director of this video.','String',1,256,0,0,'Resource','dd35d7a1-7468-4625-9133-2b90b7e07251'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '9fc89788-3c77-43a5-88cc-2d61e65c492d','807b649a-8328-4caa-a29d-7a61048fcef2','Codec','urn:zentity/module/zentity-scholarly-works/resource-type/video/property/codec','Gets or sets the codec information of this video.','String',1,4000,0,0,'Resource','9fc89788-3c77-43a5-88cc-2d61e65c492d'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '07cfd378-9a6a-4c04-93ad-52f243f79c74','807b649a-8328-4caa-a29d-7a61048fcef2','BitRate','urn:zentity/module/zentity-scholarly-works/resource-type/video/property/bitrate','Gets or sets the rate of information content in a video stream. For example VideoCD, with a bit rate of about 1 Mbit/s, is lower quality than DVD, with a bit rate of about 5 Mbit/s.','Int32',1,0,0,0,'Resource','07cfd378-9a6a-4c04-93ad-52f243f79c74'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'cbde5c20-64bf-4c43-b354-6fe6a4cf362e','807b649a-8328-4caa-a29d-7a61048fcef2','AspectRatio','urn:zentity/module/zentity-scholarly-works/resource-type/video/property/aspectratio','Gets or sets the aspect ratio of this video.','String',1,64,0,0,'Resource','cbde5c20-64bf-4c43-b354-6fe6a4cf362e'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '2cc4abc7-b19e-46e1-9438-755742a5946c','807b649a-8328-4caa-a29d-7a61048fcef2','FrameWidth','urn:zentity/module/zentity-scholarly-works/resource-type/video/property/framewidth','Gets or sets the frame width of the video stream.','Int32',1,0,0,0,'Resource','2cc4abc7-b19e-46e1-9438-755742a5946c'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '14c68cc9-d2ef-4d4b-9f79-8c84e27d2813','807b649a-8328-4caa-a29d-7a61048fcef2','FrameHeight','urn:zentity/module/zentity-scholarly-works/resource-type/video/property/frameheight','Gets or sets the frame height of the video stream.','Int32',1,0,0,0,'Resource','14c68cc9-d2ef-4d4b-9f79-8c84e27d2813'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'f01210b7-d617-4727-8525-b3f1360f8d78','807b649a-8328-4caa-a29d-7a61048fcef2','FramesPerSecond','urn:zentity/module/zentity-scholarly-works/resource-type/video/property/framespersecond','Gets or sets the number of still pictures per second of video.','Int16',1,0,0,0,'Resource','f01210b7-d617-4727-8525-b3f1360f8d78'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'd1a6e5d0-0405-441f-b4fb-bd41c2bbe96d','807b649a-8328-4caa-a29d-7a61048fcef2','ScanningMethod','urn:zentity/module/zentity-scholarly-works/resource-type/video/property/scanningmethod','Gets or sets the scanning method of the video stream. E.g. Interlaced or Progressive etc.','String',1,256,0,0,'Resource','d1a6e5d0-0405-441f-b4fb-bd41c2bbe96d'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '2f0f3d7c-8e53-4c74-9418-c53e4b5f3a3a','807b649a-8328-4caa-a29d-7a61048fcef2','BitsPerPixel','urn:zentity/module/zentity-scholarly-works/resource-type/video/property/bitsperpixel','Gets or sets the number of bits per pixel.','Int16',1,0,0,0,'Resource','2f0f3d7c-8e53-4c74-9418-c53e4b5f3a3a'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '51426e9f-5576-4ae9-8bd0-fb954a75eb1c','807b649a-8328-4caa-a29d-7a61048fcef2','ColorModel','urn:zentity/module/zentity-scholarly-works/resource-type/video/property/colormodel','Gets or sets the video color representation. E.g. YIQ, YUV etc.','String',1,64,0,0,'Resource','51426e9f-5576-4ae9-8bd0-fb954a75eb1c'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'e7fa1d49-3936-4ec3-9414-058909d19c83','1f2c6537-db1a-42f8-84da-7fba29803174','Email','urn:zentity/module/zentity-scholarly-works/resource-type/contact/property/email','Gets or sets the e-mail of this contact.','String',1,2048,0,0,'Resource','e7fa1d49-3936-4ec3-9414-058909d19c83'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'bdacb3bf-7ba4-499d-a199-9b49e24a022a','1b698930-7cc1-4535-888e-a489eece878d','JournalName','urn:zentity/module/zentity-scholarly-works/resource-type/journal/property/journalname','Gets or sets the journal name of this journal.','String',1,256,0,0,'Resource','bdacb3bf-7ba4-499d-a199-9b49e24a022a'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '6b8b39ac-1f83-48d0-8a06-259a6cc55742','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','License','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/property/license','Gets or sets the license information of this work.','String',1,4000,0,0,'Resource','6b8b39ac-1f83-48d0-8a06-259a6cc55742'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '54cbe4dd-9b7e-4bde-8fac-3206274dbc8b','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','DateValidUntil','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/property/datevaliduntil','Gets or sets the date until this work is valid.','DateTime',1,0,0,0,'Resource','54cbe4dd-9b7e-4bde-8fac-3206274dbc8b'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '3e958c5f-bd06-470a-8f90-324eca205160','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Notes','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/property/notes','Gets or sets additional notes on the scholarly work.','String',1,-1,0,0,'Resource','3e958c5f-bd06-470a-8f90-324eca205160'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'd68071c0-2560-4368-b41b-6a825000ec07','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Abstract','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/property/abstract','Gets or sets the abstract of this work.','String',1,-1,0,0,'Resource','d68071c0-2560-4368-b41b-6a825000ec07'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'bec19f51-7fe7-45a8-9a7a-88b008ade630','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Copyright','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/property/copyright','Gets or sets the copyright information of this work.','String',1,4000,0,0,'Resource','bec19f51-7fe7-45a8-9a7a-88b008ade630'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','DateValidFrom','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/property/datevalidfrom','Gets or sets the date from which this work is valid.','DateTime',1,0,0,0,'Resource','e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '8ccbc846-afd9-4a5f-9f4c-a06ed93668fd','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Language','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/property/language','Gets or sets the language of this work.','String',1,128,0,0,'Resource','8ccbc846-afd9-4a5f-9f4c-a06ed93668fd'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '13a5164e-cdc5-4fd2-b2ca-cf047a3f2519','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','DateAvailableUntil','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/property/dateavailableuntil','Gets or sets the date until this resource is available.','DateTime',1,0,0,0,'Resource','13a5164e-cdc5-4fd2-b2ca-cf047a3f2519'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '54104144-2aba-4d7c-af53-f9e06deb66ba','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','DateAvailableFrom','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/property/dateavailablefrom','Gets or sets the date from which this work is available.','DateTime',1,0,0,0,'Resource','54104144-2aba-4d7c-af53-f9e06deb66ba'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '5e27e1b3-1965-44ee-8787-fbbdbc2f300d','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','DateCopyrighted','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/property/datecopyrighted','Gets or sets the copyright date of this work.','DateTime',1,0,0,0,'Resource','5e27e1b3-1965-44ee-8787-fbbdbc2f300d'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '3851bd83-4ac5-4b9b-8e3c-a16229925ff8','bd23247c-26f4-41ef-9ad4-b1e677ec19ce','EventName','urn:zentity/module/zentity-scholarly-works/resource-type/proceedings/property/eventname','Gets or sets the event name of this proceedings.','String',1,1024,0,0,'Resource','3851bd83-4ac5-4b9b-8e3c-a16229925ff8'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'c051a74d-af00-45f1-8ef8-3c19bd14322c','ba2ecb5d-1281-4ba3-bfbd-b53fc3eec02c','DateApproved','urn:zentity/module/zentity-scholarly-works/resource-type/patent/property/dateapproved','Gets or sets the date on which this patent was approved.','DateTime',1,0,0,0,'Resource','c051a74d-af00-45f1-8ef8-3c19bd14322c'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'a8936969-6719-4639-b914-f271233d02c6','ba2ecb5d-1281-4ba3-bfbd-b53fc3eec02c','DateRejected','urn:zentity/module/zentity-scholarly-works/resource-type/patent/property/daterejected','Gets or sets the date on which this patent was rejected.','DateTime',1,0,0,0,'Resource','a8936969-6719-4639-b914-f271233d02c6'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '96302933-01ef-41b4-8399-7f946dd0d07f','a58aa716-641a-41dc-9dee-bcd8a323c401','From','urn:zentity/module/zentity-scholarly-works/resource-type/personalcommunication/property/from','Gets or sets the source of the communication.','String',1,1024,0,0,'Resource','96302933-01ef-41b4-8399-7f946dd0d07f'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '65db5eed-c9fd-452a-a289-a6034f072d9b','a58aa716-641a-41dc-9dee-bcd8a323c401','DateExchanged','urn:zentity/module/zentity-scholarly-works/resource-type/personalcommunication/property/dateexchanged','Gets or sets the datetime of communication exchange.','DateTime',1,0,0,0,'Resource','65db5eed-c9fd-452a-a289-a6034f072d9b'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '496bd2ef-295d-4801-892d-b38e8338e6ff','a58aa716-641a-41dc-9dee-bcd8a323c401','To','urn:zentity/module/zentity-scholarly-works/resource-type/personalcommunication/property/to','Gets or sets the target of the communication.','String',1,4000,0,0,'Resource','496bd2ef-295d-4801-892d-b38e8338e6ff'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '165e8189-f6a3-42c6-ba9f-bddaa81364df','f0b6249c-94f7-48d3-a8cd-c70a390b7403','Scope','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlyworkitem/property/scope','Gets or sets the scope of this item. E.g. internal, external, public etc.','String',1,128,0,0,'Resource','165e8189-f6a3-42c6-ba9f-bddaa81364df'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '42bed4bc-be0a-4d4a-bb9a-679352689f48','e314b72c-021d-4871-9403-c8af9d929b10','Report','urn:zentity/module/zentity-scholarly-works/resource-type/experiment/property/report','Gets or sets the experiment report.','String',1,-1,0,0,'Resource','42bed4bc-be0a-4d4a-bb9a-679352689f48'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '9f4c2262-8882-4a9f-a0bd-875dea047157','e314b72c-021d-4871-9403-c8af9d929b10','Status','urn:zentity/module/zentity-scholarly-works/resource-type/experiment/property/status','Gets or sets the status of this experiment.','String',1,4000,0,0,'Resource','9f4c2262-8882-4a9f-a0bd-875dea047157'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '0d977578-3e24-42d8-840f-e52a7e715830','e314b72c-021d-4871-9403-c8af9d929b10','Plan','urn:zentity/module/zentity-scholarly-works/resource-type/experiment/property/plan','Gets or sets the experiment plan.','String',1,-1,0,0,'Resource','0d977578-3e24-42d8-840f-e52a7e715830'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'a0d288c5-a9ee-4872-b7f4-f815089037b7','e314b72c-021d-4871-9403-c8af9d929b10','Name','urn:zentity/module/zentity-scholarly-works/resource-type/experiment/property/name','Gets or sets the experiment name.','String',1,1024,0,0,'Resource','a0d288c5-a9ee-4872-b7f4-f815089037b7'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '09212252-409f-43fd-a6e1-3b0213423950','ed6a2f1f-a36e-4a3f-a828-c972929ded7e','Reference','urn:zentity/module/zentity-scholarly-works/resource-type/electronicsource/property/reference','Gets or sets the reference (e.g. URL) of the electronic source.','String',1,4000,0,0,'Resource','09212252-409f-43fd-a6e1-3b0213423950'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '2b8e690c-44c7-4670-b147-06b7b18d89f3','d2d6d8f7-50f7-4c04-8456-d03c22534eb9','License','urn:zentity/module/zentity-scholarly-works/resource-type/media/property/license','Gets or sets the license information of the media.','String',1,4000,0,0,'Resource','2b8e690c-44c7-4670-b147-06b7b18d89f3'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '3c35c424-0f62-44e5-82fe-2755a9eff1c2','d2d6d8f7-50f7-4c04-8456-d03c22534eb9','Copyright','urn:zentity/module/zentity-scholarly-works/resource-type/media/property/copyright','Gets or sets the copyright information of this media.','String',1,4000,0,0,'Resource','3c35c424-0f62-44e5-82fe-2755a9eff1c2'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'e94f81aa-1993-4726-9447-5ee237e58654','d2d6d8f7-50f7-4c04-8456-d03c22534eb9','DateCopyrighted','urn:zentity/module/zentity-scholarly-works/resource-type/media/property/datecopyrighted','Gets or sets the copyright datetime of this media.','DateTime',1,0,0,0,'Resource','e94f81aa-1993-4726-9447-5ee237e58654'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '56b9acf3-9e10-424e-a9f0-a6f5aab12a50','d2d6d8f7-50f7-4c04-8456-d03c22534eb9','Duration','urn:zentity/module/zentity-scholarly-works/resource-type/media/property/duration','Gets or sets the duration of this media.','Int32',1,0,0,0,'Resource','56b9acf3-9e10-424e-a9f0-a6f5aab12a50'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '3d773ba6-1955-47c4-9388-baef50b70327','d2d6d8f7-50f7-4c04-8456-d03c22534eb9','Language','urn:zentity/module/zentity-scholarly-works/resource-type/media/property/language','Gets or sets the language of this media.','String',1,128,0,0,'Resource','3d773ba6-1955-47c4-9388-baef50b70327'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '6ac95912-620a-4aeb-83e9-0ced9a61d22a','367057b4-1bde-4486-89c0-d7b49956662c','Journal','urn:zentity/module/zentity-scholarly-works/resource-type/journalarticle/property/journal','Gets or sets the journal hosting this article.','String',1,256,0,0,'Resource','6ac95912-620a-4aeb-83e9-0ced9a61d22a'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '80a0d8df-d9a7-488b-85fe-062bac9418f2','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','DatePublished','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/datepublished','Gets or sets the date on which this resource was published.','DateTime',1,0,0,0,'Resource','80a0d8df-d9a7-488b-85fe-062bac9418f2'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','DOI','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/doi','Gets or sets the permanent digital object identifier (e.g. issued by CrossRef) of this publication.','String',1,256,0,0,'Resource','e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '3f6b345a-d2f2-4cb1-a7b5-2a316854adc6','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Pages','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/pages','Gets or sets the page numbers. E.g. 234-400.','String',1,1024,0,0,'Resource','3f6b345a-d2f2-4cb1-a7b5-2a316854adc6'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'e5989779-7dff-46c6-90e1-3a8be8c6d07b','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Institution','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/institution','Gets or sets the institution that was involved in publishing, but not necessarily the publisher.','String',1,1024,0,0,'Resource','e5989779-7dff-46c6-90e1-3a8be8c6d07b'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'ecc77bac-77c4-4eb7-80db-3cd6821c8628','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','DayPublished','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/daypublished','Gets or sets the date part of DatePublished.','Int32',1,0,0,0,'Resource','ecc77bac-77c4-4eb7-80db-3cd6821c8628'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '9d3790de-e0e0-459b-8605-52b9f1470301','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','PublisherAddress','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/publisheraddress','Gets or sets the publisher''s address.','String',1,1024,0,0,'Resource','9d3790de-e0e0-459b-8605-52b9f1470301'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '32bc2d19-22ed-4514-bfed-7b85353dc82e','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Location','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/location','Gets or sets the location where the resource is published.','String',1,1024,0,0,'Resource','32bc2d19-22ed-4514-bfed-7b85353dc82e'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '05d783c7-5517-4fbc-88b9-8e010e3f7bb3','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','BookTitle','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/booktitle','Gets or sets the title of this publication.','String',1,256,0,0,'Resource','05d783c7-5517-4fbc-88b9-8e010e3f7bb3'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '6db26d3c-a477-4148-a527-8e331bf46619','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Volume','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/volume','Gets or sets the volume of a journal or multi-volume book etc.','String',1,256,0,0,'Resource','6db26d3c-a477-4148-a527-8e331bf46619'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '441aeefc-94b4-4c5a-839f-962ff15a0624','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','CatalogNumber','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/catalognumber','Gets or sets the library of congress catalog number.','String',1,1024,0,0,'Resource','441aeefc-94b4-4c5a-839f-962ff15a0624'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'b888a64d-4555-4d1d-a422-99a1511b1f65','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','PublisherUri','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/publisheruri','Gets or sets an identifier to locate the publisher, generally the web-site URL.','String',1,1024,0,0,'Resource','b888a64d-4555-4d1d-a422-99a1511b1f65'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '03163fdf-9d73-40bf-9120-ae22e2c3407c','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','DateSubmitted','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/datesubmitted','Gets or sets the submission date of this publication.','DateTime',1,0,0,0,'Resource','03163fdf-9d73-40bf-9120-ae22e2c3407c'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'b686fec9-0fdf-46f6-b5aa-b1441bad9ea5','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Chapter','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/chapter','Gets or sets the chapter number.','String',1,256,0,0,'Resource','b686fec9-0fdf-46f6-b5aa-b1441bad9ea5'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Number','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/number','Gets or sets the number of a journal, magazine, tech-report etc.','String',1,256,0,0,'Resource','9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '1cef5926-ef98-4170-94d6-db4326671534','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','DateAccepted','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/dateaccepted','Gets or sets the date of acceptance of the publication.','DateTime',1,0,0,0,'Resource','1cef5926-ef98-4170-94d6-db4326671534'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '62f20793-f341-42ad-8980-e358c6037763','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Publisher','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/publisher','Gets or sets the publisher?s name.','String',1,256,0,0,'Resource','62f20793-f341-42ad-8980-e358c6037763'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '8db90ced-fd85-4ba7-9a9f-e61bb36cddca','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Edition','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/edition','Gets or sets the edition of this publication.','String',1,256,0,0,'Resource','8db90ced-fd85-4ba7-9a9f-e61bb36cddca'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '95d00191-ec8a-4bef-a24e-ea0d7d132bfb','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','MonthPublished','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/monthpublished','Gets or sets the month part of DatePublished.','Int32',1,0,0,0,'Resource','95d00191-ec8a-4bef-a24e-ea0d7d132bfb'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '92d45d1c-821b-4869-872b-f4355150a145','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','YearPublished','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/yearpublished','Gets or sets the year part of DatePublished.','Int32',1,0,0,0,'Resource','92d45d1c-821b-4869-872b-f4355150a145'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] 'fd227620-fc28-4539-803e-f9390a8fe329','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Organization','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/organization','Gets or sets the publication sponsor information.','String',1,1024,0,0,'Resource','fd227620-fc28-4539-803e-f9390a8fe329'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '08bc5fa6-16e7-4b98-813f-fbe233736d6a','a2fda588-32b8-42b0-b35f-dd9ecaa21a57','Series','urn:zentity/module/zentity-scholarly-works/resource-type/publication/property/series','Gets or sets the publication series.','String',1,256,0,0,'Resource','08bc5fa6-16e7-4b98-813f-fbe233736d6a'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '22a62585-520b-44e2-9194-80b09e3e5c8d','cd4a41de-8b5f-4144-aa9b-e4535c6ac949','Category','urn:zentity/module/zentity-scholarly-works/resource-type/technicalreport/property/category','Gets or sets the category of this report.','String',1,256,0,0,'Resource','22a62585-520b-44e2-9194-80b09e3e5c8d'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '7ac21049-f4c2-42d5-bd20-21a2563d4eef','e47c360d-3a1e-4d61-8e3d-e979b1265adb','DatePresented','urn:zentity/module/zentity-scholarly-works/resource-type/tutorial/property/datepresented','Gets or sets the date when this tutorial was presented.','DateTime',1,0,0,0,'Resource','7ac21049-f4c2-42d5-bd20-21a2563d4eef'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '0e2d2490-c627-493f-9a4c-fc9c306e51c5','e47c360d-3a1e-4d61-8e3d-e979b1265adb','Length','urn:zentity/module/zentity-scholarly-works/resource-type/tutorial/property/length','Gets or sets the length of the tutorial.','Int64',1,0,0,0,'Resource','0e2d2490-c627-493f-9a4c-fc9c306e51c5'
GO
EXEC [Core].[CreateOrUpdateScalarProperty] '54f98dcb-c008-4e66-9c76-353a1a48edac','7b65d54a-4e78-4409-9505-7e3e09d23c68','Count','urn:zentity/module/zentity-scholarly-works/resource-type/scholarly-work-container/property/count','Gets or sets the count of contained items.','Int32',1,0,0,0,'Resource','54f98dcb-c008-4e66-9c76-353a1a48edac'
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '8386cea7-ae68-4792-9917-51d429093c8f','3064535f-9989-489c-a7d0-24a40674494a','ScholarlyWorkItems','urn:zentity/module/zentity-scholarly-works/resource-type/tag/navigation-property/scholarlyworkitems','Gets a collection of related ScholarlyWorkItem objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] 'f4f9f9a0-7cd7-4267-a647-ef595910fc25','f46d9cb1-11be-445b-9067-5d558d8714c4','ScholarlyWorks','urn:zentity/module/zentity-scholarly-works/resource-type/download/navigation-property/scholarlyworks','Gets a collection of related ScholarlyWork objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '830a7ee3-9ccb-45c1-9f26-0409e7ea3335','1f2c6537-db1a-42f8-84da-7fba29803174','ContributionInWorks','urn:zentity/module/zentity-scholarly-works/resource-type/contact/navigation-property/contributioninworks','Gets a collection of related ScholarlyWork objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '2b34beed-ee33-4241-878e-0dfbddb6a846','1f2c6537-db1a-42f8-84da-7fba29803174','PresentedWorks','urn:zentity/module/zentity-scholarly-works/resource-type/contact/navigation-property/presentedworks','Gets a collection of related ScholarlyWork objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '47ae7643-7fe8-4b62-a6c3-1575efe19e5a','1f2c6537-db1a-42f8-84da-7fba29803174','EditedWorks','urn:zentity/module/zentity-scholarly-works/resource-type/contact/navigation-property/editedworks','Gets a collection of related ScholarlyWork objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '77fee07f-3a0b-430c-8a39-bc5afb780557','1f2c6537-db1a-42f8-84da-7fba29803174','AddedItems','urn:zentity/module/zentity-scholarly-works/resource-type/contact/navigation-property/addeditems','Gets a collection of related ScholarlyWorkItem objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '569197b2-5501-404b-899b-f0f15a8b91f3','1f2c6537-db1a-42f8-84da-7fba29803174','AuthoredWorks','urn:zentity/module/zentity-scholarly-works/resource-type/contact/navigation-property/authoredworks','Gets a collection of related ScholarlyWork objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '0e96f444-ab1e-4548-b52c-15d890d0a766','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','ElectronicSources','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/navigation-property/electronicsources','Gets a collection of related ElectronicSource objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] 'fe4aa654-c233-40a3-ab3a-1ecf9a1113be','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Medias','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/navigation-property/medias','Gets a collection of related Media objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '6da4c329-92dd-4e48-9f91-48fa8c014d96','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Editors','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/navigation-property/editors','Gets a collection of related Contact objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '1d242d11-41fb-45b7-90ab-585b88d00669','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','VersionOf','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/navigation-property/versionof','Gets a collection of related ScholarlyWork objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '6eb5afbd-7db1-4dea-8acb-65fd1b2defe0','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Contributors','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/navigation-property/contributors','Gets a collection of related Contact objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '18135894-7380-48e1-b6c6-6f12d1d6c157','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Cites','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/navigation-property/cites','Gets a collection of related ScholarlyWork objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] 'd1272877-d605-4de7-80ee-7b7db46839d0','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','CitedBy','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/navigation-property/citedby','Gets a collection of related ScholarlyWork objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] 'a76d1de5-4886-47a3-86f3-92a0d2f0e2f9','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Versions','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/navigation-property/versions','Gets a collection of related ScholarlyWork objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '07606ecb-3505-4942-be5a-b0029902fbf9','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Downloads','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/navigation-property/downloads','Gets a collection of related Download objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] 'c54e7a5f-34bc-4bd4-a35a-c41169529ae4','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','RepresentationOf','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/navigation-property/representationof','Gets a collection of related ScholarlyWork objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '99bef6f7-ef56-4f86-8b7a-d455e1dc3d04','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Authors','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/navigation-property/authors','Gets a collection of related Contact objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '42d90e7f-46b9-4e3c-b079-d6a28fb7339d','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','PersonalCommunications','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/navigation-property/personalcommunications','Gets a collection of related PersonalCommunication objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '0912e3d4-6761-4f4f-abd9-da930a9ed51c','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Representations','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/navigation-property/representations','Gets a collection of related ScholarlyWork objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '08262338-675d-4a21-bc36-f544748833e4','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Presenters','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/navigation-property/presenters','Gets a collection of related Contact objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '6a81351f-d593-4b92-bd6c-2faf3e3f9009','c9528c0e-2ca7-4dc8-908f-adfdaa1f0b7f','Container','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlywork/navigation-property/container','Gets or sets the container of this scholarly work.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] 'cc61814d-1201-461e-b394-3b89d986bc12','a58aa716-641a-41dc-9dee-bcd8a323c401','ScholarlyWorks','urn:zentity/module/zentity-scholarly-works/resource-type/personalcommunication/navigation-property/scholarlyworks','Gets a collection of related ScholarlyWork objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] 'd21cf151-cd37-4b65-9bea-362143878ffb','f0b6249c-94f7-48d3-a8cd-c70a390b7403','CategoryNodes','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlyworkitem/navigation-property/categorynodes','Gets a collection of related CategoryNode objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] 'f032d144-3774-46e7-a0c3-b8d42841741e','f0b6249c-94f7-48d3-a8cd-c70a390b7403','Tags','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlyworkitem/navigation-property/tags','Gets a collection of related Tag objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '3da16424-c4d6-44c3-92b4-ef17e6d18847','f0b6249c-94f7-48d3-a8cd-c70a390b7403','AddedBy','urn:zentity/module/zentity-scholarly-works/resource-type/scholarlyworkitem/navigation-property/addedby','Gets a collection of related Contact objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] 'd1ad7731-1304-4894-98b4-921c2ce31690','ed6a2f1f-a36e-4a3f-a828-c972929ded7e','ScholarlyWorks','urn:zentity/module/zentity-scholarly-works/resource-type/electronicsource/navigation-property/scholarlyworks','Gets a collection of related ScholarlyWork objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] 'e4bdbbd0-15cb-4f8d-a6de-0ea8794efb36','d2d6d8f7-50f7-4c04-8456-d03c22534eb9','ScholarlyWorks','urn:zentity/module/zentity-scholarly-works/resource-type/media/navigation-property/scholarlyworks','Gets a collection of related ScholarlyWork objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '0502afb6-6158-4126-8f72-20c8c5c01e2f','78d47d60-99e6-4316-8e81-d7317c70667f','ScholarlyWorkItems','urn:zentity/module/zentity-scholarly-works/resource-type/categorynode/navigation-property/scholarlyworkitems','Gets a collection of related ScholarlyWorkItem objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '60953d38-d605-4cc5-a3d4-60fd5ceb044d','78d47d60-99e6-4316-8e81-d7317c70667f','Parent','urn:zentity/module/zentity-scholarly-works/resource-type/categorynode/navigation-property/parent','Gets a collection of related CategoryNode objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '00f067de-6c9e-42ca-a394-df96bad19ecf','78d47d60-99e6-4316-8e81-d7317c70667f','Children','urn:zentity/module/zentity-scholarly-works/resource-type/categorynode/navigation-property/children','Gets a collection of related CategoryNode objects.',NULL,NULL
GO
EXEC [Core].[CreateOrUpdateNavigationProperty] '836fc343-cffb-486e-8f6c-0d0eca06106a','7b65d54a-4e78-4409-9505-7e3e09d23c68','ContainedWorks','urn:zentity/module/zentity-scholarly-works/resource-type/scholarly-work-container/navigation-property/contained-works','Gets a collection of contained ScholarlyWork objects.',NULL,NULL
GO
EXEC [Core].[CreatePredicate] '76a9cb80-fb24-4249-8c09-194f0acfc895', 'ScholarlyWorkItemHasTag', 'urn:zentity/module/zentity-scholarly-works/association/scholarly-work-item-has-tag';
GO
EXEC [Core].[CreateOrUpdateAssociation] '19153142-62f8-4ef0-ad9c-c8e9183178ed','ScholarlyWorkItemHasTag','urn:zentity/module/zentity-scholarly-works/association/scholarly-work-item-has-tag','f032d144-3774-46e7-a0c3-b8d42841741e','8386cea7-ae68-4792-9917-51d429093c8f','76a9cb80-fb24-4249-8c09-194f0acfc895','Many','Many','1915314262f84ef0ad9cc8e9183178ed'
GO
EXEC [Core].[CreatePredicate] 'a5781cc2-dd1b-4355-9a9b-4675103a2e7f', 'ScholarlyWorkIsAssociatedWithDownload', 'urn:zentity/module/zentity-scholarly-works/association/scholarly-work-is-associated-with-download';
GO
EXEC [Core].[CreateOrUpdateAssociation] '0a62762a-1958-41ba-aa3c-8abd8a4a5aee','ScholarlyWorkIsAssociatedWithDownload','urn:zentity/module/zentity-scholarly-works/association/scholarly-work-is-associated-with-download','07606ecb-3505-4942-be5a-b0029902fbf9','f4f9f9a0-7cd7-4267-a647-ef595910fc25','a5781cc2-dd1b-4355-9a9b-4675103a2e7f','Many','Many','0a62762a195841baaa3c8abd8a4a5aee'
GO
EXEC [Core].[CreatePredicate] '5bab8f81-6171-4fb5-be31-ddc37a97a918', 'ScholarlyWorkHasContributionBy', 'urn:zentity/module/zentity-scholarly-works/association/scholarly-work-has-contribution-by';
GO
EXEC [Core].[CreateOrUpdateAssociation] '456369b6-732c-4b31-8618-98d65f5001c8','ScholarlyWorkHasContributionBy','urn:zentity/module/zentity-scholarly-works/association/scholarly-work-has-contribution-by','6eb5afbd-7db1-4dea-8acb-65fd1b2defe0','830a7ee3-9ccb-45c1-9f26-0409e7ea3335','5bab8f81-6171-4fb5-be31-ddc37a97a918','Many','Many','456369b6732c4b31861898d65f5001c8'
GO
EXEC [Core].[CreatePredicate] '7015f649-1145-4036-a6a2-602a1e0625cb', 'ScholarlyWorkIsPresentedBy', 'urn:zentity/module/zentity-scholarly-works/association/scholarly-work-is-presented-by';
GO
EXEC [Core].[CreateOrUpdateAssociation] 'fb6e0e13-6a59-49a9-9c1c-12e5157a3a21','ScholarlyWorkIsPresentedBy','urn:zentity/module/zentity-scholarly-works/association/scholarly-work-is-presented-by','08262338-675d-4a21-bc36-f544748833e4','2b34beed-ee33-4241-878e-0dfbddb6a846','7015f649-1145-4036-a6a2-602a1e0625cb','Many','Many','fb6e0e136a5949a99c1c12e5157a3a21'
GO
EXEC [Core].[CreatePredicate] '97cf909f-07fa-4cdf-8d5d-e8a2ed03b6a5', 'ScholarlyWorkIsEditedBy', 'urn:zentity/module/zentity-scholarly-works/association/scholarly-work-is-edited-by';
GO
EXEC [Core].[CreateOrUpdateAssociation] '2664cf48-4ae5-44ac-9735-1fba700e3e99','ScholarlyWorkIsEditedBy','urn:zentity/module/zentity-scholarly-works/association/scholarly-work-is-edited-by','6da4c329-92dd-4e48-9f91-48fa8c014d96','47ae7643-7fe8-4b62-a6c3-1575efe19e5a','97cf909f-07fa-4cdf-8d5d-e8a2ed03b6a5','Many','Many','2664cf484ae544ac97351fba700e3e99'
GO
EXEC [Core].[CreatePredicate] '51255ad0-9daa-4f4a-b804-2eec3c52462f', 'ScholarlyWorkItemIsAddedBy', 'urn:zentity/module/zentity-scholarly-works/association/scholarly-work-item-is-added-by';
GO
EXEC [Core].[CreateOrUpdateAssociation] '003876b6-6583-4574-a53a-2069587ac10e','ScholarlyWorkItemIsAddedBy','urn:zentity/module/zentity-scholarly-works/association/scholarly-work-item-is-added-by','3da16424-c4d6-44c3-92b4-ef17e6d18847','77fee07f-3a0b-430c-8a39-bc5afb780557','51255ad0-9daa-4f4a-b804-2eec3c52462f','Many','Many','003876b665834574a53a2069587ac10e'
GO
EXEC [Core].[CreatePredicate] 'fba74167-4dad-4cc7-8232-b0f64422c120', 'ScholarlyWorkIsAuthoredBy', 'urn:zentity/module/zentity-scholarly-works/association/scholarly-work-is-authored-by';
GO
EXEC [Core].[CreateOrUpdateAssociation] 'aab520cc-0864-40e6-a4ec-b66dfc521c54','ScholarlyWorkIsAuthoredBy','urn:zentity/module/zentity-scholarly-works/association/scholarly-work-is-authored-by','99bef6f7-ef56-4f86-8b7a-d455e1dc3d04','569197b2-5501-404b-899b-f0f15a8b91f3','fba74167-4dad-4cc7-8232-b0f64422c120','Many','Many','aab520cc086440e6a4ecb66dfc521c54'
GO
EXEC [Core].[CreatePredicate] 'e64f1bb5-d8b5-45c7-b3ff-5606c94e10fb', 'ScholarlyWorkIsAssociatedWithElectronicSource', 'urn:zentity/module/zentity-scholarly-works/association/scholarly-work-is-associated-with-electronic-source';
GO
EXEC [Core].[CreateOrUpdateAssociation] '8b00027d-b4f0-43c9-bb32-406fb5195a4a','ScholarlyWorkIsAssociatedWithElectronicSource','urn:zentity/module/zentity-scholarly-works/association/scholarly-work-is-associated-with-electronic-source','0e96f444-ab1e-4548-b52c-15d890d0a766','d1ad7731-1304-4894-98b4-921c2ce31690','e64f1bb5-d8b5-45c7-b3ff-5606c94e10fb','Many','Many','8b00027db4f043c9bb32406fb5195a4a'
GO
EXEC [Core].[CreatePredicate] '919b79c8-fd22-4fa9-a040-a005865cc56b', 'ScholarlyWorkIsAssociatedWithMedia', 'urn:zentity/module/zentity-scholarly-works/association/scholarly-work-is-associated-with-media';
GO
EXEC [Core].[CreateOrUpdateAssociation] '62e7627a-e328-4164-b8e6-8968f18470fa','ScholarlyWorkIsAssociatedWithMedia','urn:zentity/module/zentity-scholarly-works/association/scholarly-work-is-associated-with-media','fe4aa654-c233-40a3-ab3a-1ecf9a1113be','e4bdbbd0-15cb-4f8d-a6de-0ea8794efb36','919b79c8-fd22-4fa9-a040-a005865cc56b','Many','Many','62e7627ae3284164b8e68968f18470fa'
GO
EXEC [Core].[CreatePredicate] '19476c3c-af6e-404e-9df2-ac467addbd52', 'ScholarlyWorkHasVersion', 'urn:zentity/module/zentity-scholarly-works/association/scholarly-work-has-version';
GO
EXEC [Core].[CreateOrUpdateAssociation] 'bf151136-7417-45de-b68c-504bd2d71dc0','ScholarlyWorkHasVersion','urn:zentity/module/zentity-scholarly-works/association/scholarly-work-has-version','a76d1de5-4886-47a3-86f3-92a0d2f0e2f9','1d242d11-41fb-45b7-90ab-585b88d00669','19476c3c-af6e-404e-9df2-ac467addbd52','Many','Many','bf151136741745deb68c504bd2d71dc0'
GO
EXEC [Core].[CreatePredicate] 'c6f8270c-db09-40d8-90c8-26efccd465fe', 'ScholarlyWorkIsCitedBy', 'urn:zentity/module/zentity-scholarly-works/association/scholarly-work-is-cited-by';
GO
EXEC [Core].[CreateOrUpdateAssociation] '81c58166-ced9-4ce1-8eb3-a1f6a0dd3d1c','ScholarlyWorkIsCitedBy','urn:zentity/module/zentity-scholarly-works/association/scholarly-work-is-cited-by','d1272877-d605-4de7-80ee-7b7db46839d0','18135894-7380-48e1-b6c6-6f12d1d6c157','c6f8270c-db09-40d8-90c8-26efccd465fe','Many','Many','81c58166ced94ce18eb3a1f6a0dd3d1c'
GO
EXEC [Core].[CreatePredicate] '7fb9e42f-f233-492c-8eb2-f0ee34223ec4', 'ScholarlyWorkHasRepresentation', 'urn:zentity/module/zentity-scholarly-works/association/scholarly-work-has-representation';
GO
EXEC [Core].[CreateOrUpdateAssociation] '4377b424-756d-40a5-a207-840ae5db5d94','ScholarlyWorkHasRepresentation','urn:zentity/module/zentity-scholarly-works/association/scholarly-work-has-representation','0912e3d4-6761-4f4f-abd9-da930a9ed51c','c54e7a5f-34bc-4bd4-a35a-c41169529ae4','7fb9e42f-f233-492c-8eb2-f0ee34223ec4','Many','Many','4377b424756d40a5a207840ae5db5d94'
GO
EXEC [Core].[CreatePredicate] '79c66c76-e748-4c9f-8337-59eabf27ab47', 'ScholarlyWorkIsAssociatedWithPersonalCommunication', 'urn:zentity/module/zentity-scholarly-works/association/scholarly-work-is-associated-with-personal-communication';
GO
EXEC [Core].[CreateOrUpdateAssociation] 'cd10127c-8b57-4de4-a7b7-368c66976ef5','ScholarlyWorkIsAssociatedWithPersonalCommunication','urn:zentity/module/zentity-scholarly-works/association/scholarly-work-is-associated-with-personal-communication','42d90e7f-46b9-4e3c-b079-d6a28fb7339d','cc61814d-1201-461e-b394-3b89d986bc12','79c66c76-e748-4c9f-8337-59eabf27ab47','Many','Many','cd10127c8b574de4a7b7368c66976ef5'
GO
EXEC [Core].[CreatePredicate] 'b20eb638-a601-4d1b-9c5b-81d3b4ba8095', 'ScholarlyWorkContainerContainsWorks', 'urn:zentity/module/zentity-scholarly-works/association/scholarly-work-container-contains-works';
GO
EXEC [Core].[CreateOrUpdateAssociation] '0e451d98-4278-49b6-ada5-fda9c0c922d5','ScholarlyWorkContainerContainsWorks','urn:zentity/module/zentity-scholarly-works/association/scholarly-work-container-contains-works','836fc343-cffb-486e-8f6c-0d0eca06106a','6a81351f-d593-4b92-bd6c-2faf3e3f9009','b20eb638-a601-4d1b-9c5b-81d3b4ba8095','ZeroOrOne','Many','0e451d98427849b6ada5fda9c0c922d5'
GO
EXEC [Core].[CreatePredicate] 'ae40bd66-d25e-408e-be5d-237224e33297', 'CategoryNodeHasScholarlyWorkItem', 'urn:zentity/module/zentity-scholarly-works/association/category-node-has-scholarly-work-item';
GO
EXEC [Core].[CreateOrUpdateAssociation] '5c9496a9-5d82-4150-ab14-32b00d6f6869','CategoryNodeHasScholarlyWorkItem','urn:zentity/module/zentity-scholarly-works/association/category-node-has-scholarly-work-item','0502afb6-6158-4126-8f72-20c8c5c01e2f','d21cf151-cd37-4b65-9bea-362143878ffb','ae40bd66-d25e-408e-be5d-237224e33297','Many','Many','5c9496a95d824150ab1432b00d6f6869'
GO
EXEC [Core].[CreatePredicate] '45cc949e-5490-42a8-9b62-9a2f0c17fe93', 'CategoryNodeHasChildren', 'urn:zentity/module/zentity-scholarly-works/association/category-node-has-children';
GO
EXEC [Core].[CreateOrUpdateAssociation] 'e5f7c76f-5be5-456a-b097-611295a14d63','CategoryNodeHasChildren','urn:zentity/module/zentity-scholarly-works/association/category-node-has-children','00f067de-6c9e-42ca-a394-df96bad19ecf','60953d38-d605-4cc5-a3d4-60fd5ceb044d','45cc949e-5490-42a8-9b62-9a2f0c17fe93','ZeroOrOne','Many','e5f7c76f5be5456ab097611295a14d63'
GO
EXEC [Core].[CreateUniqueIndexesOnMetadata]
GO
EXEC [Core].[AfterSchemaChanges];
GO
----------------------------------------------
CREATE NONCLUSTERED INDEX [IX_Book_ISBN] ON [Core].[Resource] 
(
	[bc13df5d-58dc-4fc7-8010-ec7eb04ac0c0] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Code_ProgrammingLanguage] ON [Core].[Resource] 
(
	[4326eb60-7271-43a5-9436-2d0766c353b9] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Code_Technology] ON [Core].[Resource] 
(
	[158a2898-edec-47d8-937a-b37269df2aaa] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Download_VersionInformation] ON [Core].[Resource] 
(
	[0bb4e6f6-7d38-4773-a301-2c83810923e8] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Email_Subject] ON [Core].[Resource] 
(
	[a8f0994e-bc4e-4346-886d-9ffbde0940a8] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Journal_JournalName] ON [Core].[Resource] 
(
	[bdacb3bf-7ba4-499d-a199-9b49e24a022a] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_JournalArticle_Journal] ON [Core].[Resource] 
(
	[6ac95912-620a-4aeb-83e9-0ced9a61d22a] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Lecture_Dates] ON [Core].[Resource] 
(
	[02dbf59e-ff45-41e8-9320-a702ebdadba7] ASC,
	[22e98699-e5c6-4466-af64-be9dc28d7dc5] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Lecture_Series] ON [Core].[Resource] 
(
	[eaa610b7-cf35-4393-bf19-0fb7af9a128a] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Media_Language] ON [Core].[Resource] 
(
	[3d773ba6-1955-47c4-9388-baef50b70327] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Patent_Dates] ON [Core].[Resource] 
(
	[c051a74d-af00-45f1-8ef8-3c19bd14322c] ASC,
	[a8936969-6719-4639-b914-f271233d02c6] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Person_FirstName] ON [Core].[Resource] 
(
	[3d82f5e2-b84a-4422-a0be-82317ad5059f] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Person_LastName] ON [Core].[Resource] 
(
	[c7a2b2e1-49b3-4ca0-a1a1-6e7b114007e4] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Person_MiddleName] ON [Core].[Resource] 
(
	[95d6858e-6294-4572-8686-c2c2c8dddd17] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Publication_DOI] ON [Core].[Resource] 
(
	[e7d9fd2e-8113-4bdd-9c96-0f4e6a3415e6] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
INCLUDE
(
	[441aeefc-94b4-4c5a-839f-962ff15a0624]
)
GO
CREATE NONCLUSTERED INDEX [IX_Publication_Dates] ON [Core].[Resource] 
(
	[80a0d8df-d9a7-488b-85fe-062bac9418f2] ASC,
	[92d45d1c-821b-4869-872b-f4355150a145] ASC,
	[03163fdf-9d73-40bf-9120-ae22e2c3407c] ASC,
	[1cef5926-ef98-4170-94d6-db4326671534] ASC,
	[ecc77bac-77c4-4eb7-80db-3cd6821c8628] ASC,
	[95d00191-ec8a-4bef-a24e-ea0d7d132bfb] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Publication_Edition] ON [Core].[Resource] 
(
	[8db90ced-fd85-4ba7-9a9f-e61bb36cddca] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Publication_Number] ON [Core].[Resource] 
(
	[9ecdaa0b-45a6-4b03-bcb8-d7f589b07db2] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Publication_Publisher] ON [Core].[Resource] 
(
	[62f20793-f341-42ad-8980-e358c6037763] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
INCLUDE
(
	[9d3790de-e0e0-459b-8605-52b9f1470301]
)
GO
CREATE NONCLUSTERED INDEX [IX_Publication_Series] ON [Core].[Resource] 
(
	[08bc5fa6-16e7-4b98-813f-fbe233736d6a] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Publication_Volume] ON [Core].[Resource] 
(
	[6db26d3c-a477-4148-a527-8e331bf46619] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_ScholarlyWork_Dates] ON [Core].[Resource] 
(
	[54104144-2aba-4d7c-af53-f9e06deb66ba] ASC, -- DateAvailableFrom
	[13a5164e-cdc5-4fd2-b2ca-cf047a3f2519] ASC, -- DateAvailableUntil
	[5e27e1b3-1965-44ee-8787-fbbdbc2f300d] ASC, -- DateCopyrighted
	[e2f5ae39-6d70-458e-bdff-9bcfb27f9a5b] ASC, -- DateValidFrom
	[54cbe4dd-9b7e-4bde-8fac-3206274dbc8b] ASC, -- DateValidUntil
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_ScholarlyWork_Language] ON [Core].[Resource] 
(
	[8ccbc846-afd9-4a5f-9f4c-a06ed93668fd] ASC, -- Language
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_ScholarlyWorkItem_Scope] ON [Core].[Resource] 
(
	[165e8189-f6a3-42c6-ba9f-bddaa81364df] ASC, 
	[Id] ASC,
	[Discriminator] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Tag_Name] ON [Core].[Resource] 
(
	[ea645ca2-11e7-4746-af99-667274066311] ASC,
	[Id] ASC,
	[Discriminator] ASC
)
GO

