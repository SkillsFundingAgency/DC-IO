CREATE TABLE [dbo].[DataExchange] (
    [DataExchange_Id] BIGINT         IDENTITY (1, 1) NOT NULL,
    [Job_Id]          BIGINT         NOT NULL,
    [Item]            INT            NOT NULL,
    [ActorId]         INT            NOT NULL,
    [Value]           NVARCHAR (MAX) NOT NULL,
    [Created_On]      DATETIME       CONSTRAINT [def_dbo_DataExchange_CreatedOn] DEFAULT (getutcdate()) NOT NULL,
    [Created_By]      NVARCHAR (256) CONSTRAINT [def_dbo_DataExchange_Createdby] DEFAULT (suser_sname()) NOT NULL,
    [Modified_On]     DATETIME       CONSTRAINT [def_dbo_DataExchange_ModifiedOn] DEFAULT (getutcdate()) NOT NULL,
    [Modified_By]     NVARCHAR (256) CONSTRAINT [def_dbo_DataExchange_ModifiedBy] DEFAULT (suser_sname()) NOT NULL,
    CONSTRAINT [PK_dbo_DataExchange] PRIMARY KEY CLUSTERED ([DataExchange_Id] DESC)
);

