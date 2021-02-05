SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [Core].[Resource] ADD [0baae1dc-1c7b-4552-a80d-2e0d4bd6818f] nvarchar(max) NULL;
GO

ALTER TABLE [Core].[Resource] ADD [206d926f-520c-4e7d-9d7c-f726f858ecff] nvarchar(max) NULL;
GO

CREATE VIEW [Core].[72eb3abe871b4137958404c3d5c93826]
WITH SCHEMABINDING
AS
	SELECT [SubjectResourceId], [ObjectResourceId]
	FROM [Core].[Relationship] T
	WHERE [T].[PredicateId] = '471e0800-105f-4428-a613-8cdd41e6013d';
GO

CREATE UNIQUE CLUSTERED INDEX [PK_72eb3abe871b4137958404c3d5c93826] ON [Core].[72eb3abe871b4137958404c3d5c93826] 
(
	[SubjectResourceId], [ObjectResourceId]
);
GO

CREATE PROCEDURE [Core].[Insert72eb3abe871b4137958404c3d5c93826]
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
		'471e0800-105f-4428-a613-8cdd41e6013d'
	) 
END;
GO

CREATE PROCEDURE [Core].[Delete72eb3abe871b4137958404c3d5c93826]
	@SubjectResourceId [uniqueidentifier], 
	@ObjectResourceId [uniqueidentifier]
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [Core].[Relationship]
	WHERE [SubjectResourceId] = @SubjectResourceId
	AND [ObjectResourceId] = @ObjectResourceId
	AND [PredicateId] = '471e0800-105f-4428-a613-8cdd41e6013d';
END;
GO

CREATE PROCEDURE [Core].[Insert48db03e2f07e434a92563177a56e1059]
 @IdentityName nvarchar(max)
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
[0baae1dc-1c7b-4552-a80d-2e0d4bd6818f], 
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
@IdentityName, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'48db03e2-f07e-434a-9256-3177a56e1059', 
43

);


END;
GO

CREATE PROCEDURE [Core].[Update48db03e2f07e434a92563177a56e1059]
 @IdentityName nvarchar(max)
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
 [0baae1dc-1c7b-4552-a80d-2e0d4bd6818f]  =  @IdentityName
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END;
GO

CREATE PROCEDURE [Core].[Delete48db03e2f07e434a92563177a56e1059]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END;
GO

CREATE PROCEDURE [Core].[Insertf1dc8dd59b444fccb95413bf6304c24d]
 @GroupName nvarchar(max)
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
[206d926f-520c-4e7d-9d7c-f726f858ecff], 
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
@GroupName, 
@Title, 
@DateModified, 
@Id, 
@DateAdded, 
@Uri, 
@Description, 
'f1dc8dd5-9b44-4fcc-b954-13bf6304c24d', 
44

);


END;
GO

CREATE PROCEDURE [Core].[Updatef1dc8dd59b444fccb95413bf6304c24d]
 @GroupName nvarchar(max)
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
 [206d926f-520c-4e7d-9d7c-f726f858ecff]  =  @GroupName
, [Title]  =  @Title
, [DateModified]  =  @DateModified
, [DateAdded]  =  @DateAdded
, [Uri]  =  @Uri
, [Description]  =  @Description

WHERE 
[Id] = @Id;

END;
GO

CREATE PROCEDURE [Core].[Deletef1dc8dd59b444fccb95413bf6304c24d]
@Id [uniqueidentifier]

AS
BEGIN
SET NOCOUNT ON;
-- Not NULL value check 



DELETE FROM [Core].[Resource] 
WHERE [Id] = @Id;

END;
GO

EXEC [Core].[DropUniqueIndexesFromMetadata];
GO

EXEC [Core].[CreateOrUpdateDataModelModule] '8962c847-e7dd-42a0-a2fc-ef0a12803a57','Zentity.Security.Authorization',NULL,NULL;
GO

EXEC [Core].[CreateOrUpdateResourceType] '48db03e2-f07e-434a-9256-3177a56e1059','8962c847-e7dd-42a0-a2fc-ef0a12803a57','d2bd64df-6609-4ea4-ae99-9669da69bf7a','Identity','urn:zentity/module/zentity-authorization/resource-type/identity','Represents an Identity.',43;
GO

EXEC [Core].[CreateOrUpdateResourceType] 'f1dc8dd5-9b44-4fcc-b954-13bf6304c24d','8962c847-e7dd-42a0-a2fc-ef0a12803a57','d2bd64df-6609-4ea4-ae99-9669da69bf7a','Group','urn:zentity/module/zentity-authorization/resource-type/group','Represents a Group.',44;
GO

EXEC [Core].[CreateOrUpdateScalarProperty] '0baae1dc-1c7b-4552-a80d-2e0d4bd6818f','48db03e2-f07e-434a-9256-3177a56e1059','IdentityName',NULL,NULL,'String',1,-1,0,0,'Resource','0baae1dc-1c7b-4552-a80d-2e0d4bd6818f';
GO

EXEC [Core].[CreateOrUpdateScalarProperty] '206d926f-520c-4e7d-9d7c-f726f858ecff','f1dc8dd5-9b44-4fcc-b954-13bf6304c24d','GroupName',NULL,NULL,'String',1,-1,0,0,'Resource','206d926f-520c-4e7d-9d7c-f726f858ecff';
GO

EXEC [Core].[CreateOrUpdateNavigationProperty] '1095d210-8b31-4e4a-921f-6a5b3a2fdc11','48db03e2-f07e-434a-9256-3177a56e1059','Groups','urn:zentity/module/zentity-authorization/resource-type/identity/navigation-property/groups','Gets a collection of related Group objects.',NULL,NULL;
GO

EXEC [Core].[CreateOrUpdateNavigationProperty] 'f1aa74c3-083b-4ce9-8955-e59ff79662b4','f1dc8dd5-9b44-4fcc-b954-13bf6304c24d','Identities','urn:zentity/module/zentity-authorization/resource-type/group/navigation-property/identities','Gets a collection of related Identity objects.',NULL,NULL;
GO

EXEC [Core].[CreatePredicate] '471e0800-105f-4428-a613-8cdd41e6013d', 'IdentityBelongsToGroups', 'urn:zentity/module/zentity-authorization/association/identity-belongs-to-groups';
GO

EXEC [Core].[CreateOrUpdateAssociation] '72eb3abe-871b-4137-9584-04c3d5c93826','IdentityBelongsToGroups','urn:zentity/module/zentity-authorization/association/identity-belongs-to-groups','1095d210-8b31-4e4a-921f-6a5b3a2fdc11','f1aa74c3-083b-4ce9-8955-e59ff79662b4','471e0800-105f-4428-a613-8cdd41e6013d','Many','Many','72eb3abe871b4137958404c3d5c93826';
GO

EXEC [Core].[CreateUniqueIndexesOnMetadata];
GO

EXEC [Core].[AfterSchemaChanges];
GO
