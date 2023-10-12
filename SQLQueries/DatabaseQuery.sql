--Create Locations
CREATE TABLE [dbo].[Locations] (
	[Id]         INT IDENTITY (1,1) NOT NULL,
	[Name] NVARCHAR (256) NOT NULL,
	[Lat] FLOAT,
	[Long] FLOAT,
	CONSTRAINT [PK_Locations] PRIMARY KEY CLUSTERED ([Id] ASC)
)

--Create LocationStaff
CREATE TABLE [dbo].[StaffLocations] (
	[StaffId] NVARCHAR (450),
	[LocationId] NVARCHAR (450)
	CONSTRAINT [PK_StaffLocations] PRIMARY KEY CLUSTERED ([StaffId] ASC, [LocationId] ASC),
	CONSTRAINT [FK_StaffLocations_AspNetUsers_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [dbo].[AspNetUsers] (Id) ON DELETE CASCADE,
	CONSTRAINT [FK_StaffLocations_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [dbo].[Locations] (Id) ON DELETE CASCADE
)

--Create Bookings Table
CREATE TABLE [dbo].[Bookings] (
	[Id]         INT IDENTITY (1,1) NOT NULL,
	[Date] datetime NOT NULL,
	[CustomerId] NVARCHAR (450) NOT NULL,
	[StaffId] NVARCHAR (450) NOT NULL,
	[LocationId] NVARCHAR (450) NOT NULL,

	PRIMARY KEY (Id),
	FOREIGN KEY (CustomerId) REFERENCES AspNetUsers(Id),
	FOREIGN KEY (LocationId) REFERENCES Locations(Id)
)




