use test
go
create table AutoGenId
(
	ID int identity,
	FirstName nvarchar(50),
	Surname nvarchar(50),
	IDNumber nvarchar(20)
)
go

-- use test
-- go
-- create table AutoGenId
-- (
-- 	ID int identity PRIMARY KEY,
-- 	FirstName nvarchar(50),
-- 	Surname nvarchar(50),
-- 	IDNumber nvarchar(20)
-- )
-- go
