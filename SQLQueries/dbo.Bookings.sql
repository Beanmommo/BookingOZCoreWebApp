CREATE TABLE [dbo].[Bookings] (
    [Id]         INT IDENTITY (1,1) NOT NULL,
    [Date]       DATETIME       NOT NULL,
    [CustomerId] NVARCHAR (450) NOT NULL,
    [StaffId]    NVARCHAR (450) NOT NULL,
    [LocationId] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    FOREIGN KEY ([LocationId]) REFERENCES [dbo].[Locations] ([Id])
);

