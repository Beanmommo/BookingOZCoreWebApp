CREATE TABLE [dbo].[Locations] (
    [Id]   INT IDENTITY (1,1) NOT NULL,
    [Name] NVARCHAR (256) NOT NULL,
    [Lat]  REAL           NULL,
    [Long] REAL           NULL,
    CONSTRAINT [PK_Locations] PRIMARY KEY CLUSTERED ([Id] ASC)
);

