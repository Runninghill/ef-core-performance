use test
go
CREATE PROCEDURE sp_InsertTestModel
    @FirstName nvarchar(50),
    @LastName nvarchar(50),
    @IdNumber nvarchar(50)
AS
    SET NOCOUNT ON;
    insert into AutoGenId (FirstName, Surname, IdNumber)
    values (@FirstName, @LastName, @IdNumber);
go
