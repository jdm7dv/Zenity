
/****** Object:  Schema [OaiPmh]    Script Date: 03/22/2008 16:56:30 ******/
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'OaiPmh')
EXEC sys.sp_executesql N'CREATE SCHEMA [OaiPmh] AUTHORIZATION [dbo]'
GO
/****** Object:  StoredProcedure [OaiPmh].[DeleteMetaData]    Script Date: 03/22/2008 16:56:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[OaiPmh].[DeleteMetaData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [OaiPmh].[DeleteMetaData]
	-- Add the parameters for the stored procedure here
	@Id uniqueidentifier
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    
	--DELETE FROM  WHERE id=@ID
	DELETE FROM [OaiPmh].[MetaDataProvider] WHERE id=@Id
END
' 
END
GO
/****** Object:  Table [OaiPmh].[MetaDataProvider]    Script Date: 03/22/2008 16:56:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[OaiPmh].[MetaDataProvider]') AND type in (N'U'))
BEGIN
CREATE TABLE [OaiPmh].[MetaDataProvider](
	[id] [uniqueidentifier] NOT NULL,
	[QueryExecutionDateTime] [datetime] NOT NULL,
	[TotalRecords] [int] NOT NULL,
	[ActualTotalRecords] [int] NOT NULL,
	[PendingRecords] [int] NOT NULL,
	[ActualHarvestedRecords] [int] NOT NULL,
	[QueryString] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_MetaDataProvider] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  StoredProcedure [OaiPmh].[PurgeExpiredResumptionTokens]    Script Date: 03/22/2008 16:56:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[OaiPmh].[PurgeExpiredResumptionTokens]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [OaiPmh].[PurgeExpiredResumptionTokens] 		
AS
BEGIN
	SET NOCOUNT ON;		
	DELETE from [OaiPmh].[MetaDataProvider] WHERE DATEADD(hh,24,QueryExecutionDateTime)  > GETDATE()   	
END
' 
END
GO
/****** Object:  StoredProcedure [OaiPmh].[InsertResumptionToken]    Script Date: 03/22/2008 16:56:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[OaiPmh].[InsertResumptionToken]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [OaiPmh].[InsertResumptionToken]
	-- Add the parameters for the stored procedure here
	@QueryExecutionDateTime datetime,
	@TotalRecords int,
	@ActualTotalRecords int,
	@PendingRecords int,
	@ActualHarvestedRecords int,
	@QueryString nvarchar(256),
	@NewResumptionToken uniqueidentifier output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    SET @NewResumptionToken=NEWID()
	INSERT INTO [OaiPmh].[MetaDataProvider] 
				(id, 
				QueryExecutionDateTime, 
				TotalRecords, 
				ActualTotalRecords, 
				PendingRecords, 
				ActualHarvestedRecords, 
				QueryString)	
	VALUES		(@NewResumptionToken,
				@QueryExecutionDateTime,
				@TotalRecords,
				@ActualTotalRecords,
				@PendingRecords,
				@ActualHarvestedRecords,
				@QueryString)
END
' 
END
GO
/****** Object:  StoredProcedure [OaiPmh].[GetMetadataTokenRecord]    Script Date: 03/22/2008 16:56:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[OaiPmh].[GetMetadataTokenRecord]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [OaiPmh].[GetMetadataTokenRecord]
	@Id uniqueidentifier
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here       
		       
	SELECT	id, 
			QueryExecutionDateTime, 
			TotalRecords, 
			ActualTotalRecords, 
			PendingRecords, 
			ActualHarvestedRecords, 
			QueryString
	FROM [OaiPmh].[MetaDataProvider] 
	WHERE id=@Id
END
' 
END
GO
/****** Object:  StoredProcedure [OaiPmh].[UpdateResumptionToken]    Script Date: 03/22/2008 16:56:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[OaiPmh].[UpdateResumptionToken]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [OaiPmh].[UpdateResumptionToken] 
	@ResumptionToken uniqueidentifier,
	@MaxHarvestCount int,
	@ActualHarvestedRecords int,
	@NewResumptionToken uniqueidentifier output	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
			
	DECLARE @PendingRecords int;
	DECLARE @QueryExecutionDateTime datetime
	DECLARE @TotalRecords int
	DECLARE @ActualTotalRecords int		
	DECLARE @QueryString nvarchar(256)
					
	SELECT	@PendingRecords=PendingRecords,
			@QueryExecutionDateTime=QueryExecutionDateTime, 
			@ActualTotalRecords = ActualTotalRecords,
			@TotalRecords=TotalRecords,
			@QueryString=QueryString
	FROM [OaiPmh].[MetaDataProvider]
	WHERE id=@ResumptionToken;
	
	IF (@PendingRecords>0)
	BEGIN
		IF (@PendingRecords > @MaxHarvestCount)
		BEGIN
			SET @NewResumptionToken=NEWID();
					
			INSERT INTO [OaiPmh].[MetaDataProvider] 
					(id, 
					QueryExecutionDateTime, 
					TotalRecords, 
					ActualTotalRecords, 
					PendingRecords, 
					ActualHarvestedRecords, 
					QueryString)
			VALUES
					(@NewResumptionToken,
					@QueryExecutionDateTime,					
					@TotalRecords,
					@ActualTotalRecords,
					@PendingRecords - @MaxHarvestCount,
					@ActualHarvestedRecords,
					@QueryString)										
					
			DELETE FROM [OaiPmh].[MetaDataProvider] WHERE id=@ResumptionToken															
		END																							
	END	
END
' 
END
GO
-- Get the database name.
DECLARE @DBName nvarchar(128);
SELECT @DBName = REPLACE(DB_NAME(),']',']]');

DECLARE @Cmd nvarchar(4000);
SET @Cmd = N'
USE master;
DECLARE @Count int;
--check if the sql agent is running. A job can be scheduled only if the agent is running
SELECT @Count=COUNT(*) FROM master.dbo.sysprocesses 
WHERE program_name LIKE ''SQLAgent%'';
IF (@Count <> 0) -- means the agent is running
BEGIN
--check if the job already exists
SELECT @count = COUNT(*) FROM msdb.dbo.sysjobs 
WHERE NAME LIKE ''%PurgeResumptionTokens%'';
IF (@count = 0) --means the job does not exist
BEGIN

BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0

IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N''[Uncategorized (Local)]'' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N''JOB'', @type=N''LOCAL'', @name=N''[Uncategorized (Local)]''
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
DECLARE @login nvarchar(500);
select @login = SUSER_NAME();
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N''PurgeResumptionTokens'', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N''This job periodically deletes the resumption tokens, from the Platform specific database'', 
		@category_name=N''[Uncategorized (Local)]'', 
		@owner_login_name=@login, @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N''Remove the resumption token ids which are older than 24 hrs'', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=1, 
		@retry_interval=1440, 
		@os_run_priority=0, @subsystem=N''TSQL'', 
		@command=N''DECLARE @RC int
EXECUTE @RC = ['+@DBName+'].[OaiPmh].[PurgeExpiredResumptionTokens]'', 
		@database_name=N'''+@DBName+''', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N''(local)''
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
END
ELSE
PRINT ''The job for remvoving resumption token already exists''
END
ELSE
PRINT ''SQL agent is not running. Cannot schedule the job for removal of resumption tokens.''
'
EXEC(@Cmd);
GO
