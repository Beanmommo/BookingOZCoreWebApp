CREATE TABLE [dbo].[Locations] (
    [Id]   NVARCHAR (450) NOT NULL,
    [Name] NVARCHAR (256) NOT NULL,
    [Lat]  FLOAT           NULL,
    [Long] FLOAT           NULL,
    CONSTRAINT [PK_Locations] PRIMARY KEY CLUSTERED ([Id] ASC)
);

