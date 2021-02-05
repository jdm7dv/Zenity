
IF EXISTS(SELECT * FROM sys.objects WHERE type = 'FN' AND name = 'GetAuthorizationCondition')
BEGIN
DROP FUNCTION Core.GetAuthorizationCondition;
END
GO

CREATE FUNCTION Core.GetAuthorizationCondition(@identityName NVARCHAR(100))
RETURNS NVARCHAR(max) AS
BEGIN

--The condition string to return.
declare @authorizationCondition nvarchar(max);

--The subqueries to be nested into the criteria.
declare @predicateIdQuery nvarchar(1000);
declare @resourceIdQuery nvarchar(1000);
declare @identityIdQuery nvarchar(1000);
declare @adminGroupIdQuery nvarchar(1000);
declare @allUsersGroupIdQuery nvarchar(1000);
declare @identityGroupsQuery nvarchar(1000);

--Predicate ids and uri's.
declare @denyReadPredicateId uniqueidentifier;
declare @denyReadUri nvarchar(500);
declare @memberOfPredicateId uniqueidentifier;
declare @memberOfUri nvarchar(500);
declare @ownerPredicateId nvarchar(500);
declare @ownerUri nvarchar(500);
declare @denyOwnerPredicateId nvarchar(500);
declare @denyOwnerUri nvarchar(500);

--Identity id, group names and group ids.
declare @identityId uniqueidentifier;
declare @adminGroupId uniqueidentifier;
declare @adminGroupName nvarchar(500);
declare @allUsersGroupId uniqueidentifier;
declare @allUsersGroupName nvarchar(500);

declare @paramDef nvarchar(1000);

--Identity type
declare @identityNameColumnName nvarchar(100);
declare @identityNamePropertyName nvarchar(100);
declare @identityTypeName nvarchar(100);

--Group type
declare @groupNameColumnName nvarchar(100);
declare @groupNamePropertyName nvarchar(100);
declare @groupTypeName nvarchar(100);

--Predicate uri's.
set @denyReadUri = N'urn:zentity/module/zentity-authorization/predicate/deny-read-access';
set @memberOfUri = N'urn:zentity/module/zentity-authorization/association/identity-belongs-to-groups';
set @ownerUri = N'urn:zentity/module/zentity-authorization/predicate/is-owner-of';
set @denyOwnerUri = N'urn:zentity/module/zentity-authorization/predicate/deny-ownership-of';

--Identity and group type names.
set @adminGroupName = 'Administrators';
set @allUsersGroupName = 'AllUsers';
set @identityNamePropertyName = 'IdentityName';
set @identityTypeName = 'Identity';
set @groupNamePropertyName = 'GroupName';
set @groupTypeName = 'Group';

--Find out the column name in core.resource which corresponds to the IdentityName property.
select @identityNameColumnName = columnname from Core.ScalarProperty
where name =  @identityNamePropertyName and ResourceTypeId = (select Core.ResourceType.Id from Core.ResourceType
where Name = @identityTypeName);

-- Find out column name for GroupName property in Core.Resource.
select @groupNameColumnName = columnname from Core.ScalarProperty
where name =  @groupNamePropertyName and ResourceTypeId = (select Core.ResourceType.Id from Core.ResourceType
where Name = @groupTypeName);

--Find out predicate id for deny read predicate
select @denyReadPredicateId = id from Core.Predicate where uri = @denyReadUri;

-- Retrieve predicate Id for IdentityBelongsToGroups predicate.
select @memberOfPredicateId = id from Core.Predicate where uri = @memberOfUri;

--Find out predicate id for owner predicate
select @ownerPredicateId = id from Core.Predicate where uri = @ownerUri;

-- Retrieve predicate Id for deny owner predicate.
select @denyOwnerPredicateId = id from Core.Predicate where uri = @denyOwnerUri;

--Find out resource id of the identity.
select @identityIdQuery = N'select id from Core.Resource where [' + @identityNameColumnName
 + '] = ''' + @identityName + '''';

-- Find out id for Administrators group from Core.Resource.
select @adminGroupIdQuery = N'select id from Core.Resource where [' + @groupNameColumnName
 + '] = ''' + @adminGroupName + '''';

--Find out the group id for AllUsers group
select @allUsersGroupIdQuery = N'select id from Core.Resource where [' + @groupNameColumnName
 + '] = ''' + @allUsersGroupName + '''';

--Select groups to which an identity belongs
select @identityGroupsQuery = N'(select ObjectResourceId from Core.Relationship 
where SubjectResourceId = (' + @identityIdQuery + ') and PredicateId = cast(''{' 
+ cast(@memberOfPredicateId as nvarchar(100)) + '}'' as uniqueidentifier))';

-- Create authorization criteria
-- The conditions in the clause formed below are as follows - 
-- user belongs to administrators group 

-- OR resource id does not participate in relationships 
-- with predicate = DenyRead and 
-- Subject = Identity OR Subject = AllUsersGroup OR Subject = Any of the groups of which the identity is a member.

select @authorizationCondition = N' 
((exists(select id from Core.Relationship where SubjectResourceId = (' + @identityIdQuery 
+ ') and ObjectResourceId = (' + @adminGroupIdQuery + ') 
and predicateId = cast(''{' + cast(@memberOfPredicateId as nvarchar(100)) + '}'' as uniqueidentifier))) 

or (''' + @identityName  + ''' <> ''Guest'' and ((id in (select ObjectResourceId from Core.Relationship
where PredicateId = cast(''{' + cast(@ownerPredicateId as nvarchar(100)) + '}'' as uniqueidentifier) 
and (SubjectResourceId = (' + @identityIdQuery + '))))

or (id in (select ObjectResourceId from Core.Relationship
where PredicateId = cast(''{' + cast(@ownerPredicateId as nvarchar(100)) + '}'' as uniqueidentifier) 
and (SubjectResourceId = (' + @allUsersGroupIdQuery + ') 
or SubjectResourceId in (' + @identityGroupsQuery + ')))
and (id not in (select ObjectResourceId from Core.Relationship
where PredicateId = cast(''{' + cast(@denyOwnerPredicateId as nvarchar(100)) + '}'' as uniqueidentifier) 
and (SubjectResourceId = (' + @allUsersGroupIdQuery + ') 
or SubjectResourceId in (' + @identityGroupsQuery + ')))) )))

or id not in (select ObjectResourceId from Core.Relationship
where PredicateId = cast(''{' + cast(@denyReadPredicateId as nvarchar(100)) + '}'' as uniqueidentifier) 
and (SubjectResourceId = (' + @identityIdQuery + ') 
or SubjectResourceId = (' + @allUsersGroupIdQuery + ') 
or SubjectResourceId in (' + @identityGroupsQuery + '))))';

return @authorizationCondition;
END