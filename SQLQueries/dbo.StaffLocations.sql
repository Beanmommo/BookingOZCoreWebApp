CREATE TABLE [dbo].[StaffLocations] (
    [StaffId]    NVARCHAR (450) NOT NULL,
    [LocationId] INT NOT NULL,
    CONSTRAINT [PK_StaffLocations] PRIMARY KEY CLUSTERED ([StaffId] ASC, [LocationId] ASC),
    CONSTRAINT [FK_StaffLocations_AspNetUsers_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_StaffLocations_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [dbo].[Locations] ([Id]) ON DELETE CASCADE
);

