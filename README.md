# Entity Framework Core Performance Test

![Runninghill Logo](https://github.com/Runninghill/ef-core-performance/blob/main/.images/runninghill.png?raw=true "Runninghill")

A solution used to run performance tests against Entity Framework Core (using different strategies and databases) to determine the load that could be catered for when **inserting records into a database** that has an auto increment column, using API endpoints.

This is as a result of experiencing **terrible performance** and **ZombieCheck()** issues when using the default implementation of Entity Framework Core with a Microsoft SQL server that has an auto increment column.

**TLDR:**  If you are planning to use Entity Framework Core with Microsoft SQL Server and an auto increment column in a Production environment it is highly recommended that you consider changing some of the default values. Either by disabling auto transactions in your Entity Framework solution, by changing your lock isolation behavior in code or the database or setting the auto increment column as the Primary Key.

- [Entity Framework Core Performance Test](#entity-framework-core-performance-test)
  - [Features](#features)
  - [Prerequisites](#prerequisites)
  - [Stress Test](#stress-test)
    - [Default Implementation](#default-implementation)
    - [Suggested Retry Policy](#suggested-retry-policy)
    - [Custom Execution Strategy](#custom-execution-strategy)
    - [Changing Transaction Isolation Level](#changing-transaction-isolation-level)
    - [Guid as Id](#guid-as-id)
    - [Stored Procedure](#stored-procedure)
    - [EF Core with MySql](#ef-core-with-mysql)
    - [Conclusion](#conclusion)
  - [Visual Studio Quickstart](#visual-studio-quickstart)
    - [Running Locally in Visual Studio - With Docker](#running-locally-in-visual-studio---with-docker)
    - [Running Locally in Visual Studio - Without Docker](#running-locally-in-visual-studio---without-docker)
    - [Application Settings](#application-settings)
  - [Resources](#resources)

## Features

This is a [ASP .Net Core App](https://github.com/dotnet/aspnetcore). The solution has the following features:

- Basic API to insert records into a MS SQL database using 3 different techniques.
  - Inserting using EF Core with a table that has an auto increment column.
  - Inserting using EF Core with a table that has a Guid column.
  - Inserting using a Stored Procedure that inserts into a table that has an auto increment column.
- Basic API to insert records into a MySql database using a table that has an auto increment primary key.
- Using the [artillery npm](https://www.npmjs.com/package/artillery) package to stress test the API's.

![Swagger](https://github.com/Runninghill/ef-core-performance/blob/main/.images/swaggeroverview.PNG)

## Prerequisites

- [Visual Studio](https://visualstudio.microsoft.com/downloads/) or any C# Code Editor
- [Node.js](https://nodejs.org/en/download/)
- [Artillery](https://www.npmjs.com/package/artillery)
- [Docker](https://www.docker.com/products/docker-desktop) (Optional)

## Stress Test

[Artillery](https://www.npmjs.com/package/artillery) was used to test the load that can be handled by EF Core using different strategies and technologies.

### Default Implementation

Simple API that inserts records into a MS SQL database using EF Core. The table has an **auto increment Id** column.

**Test:** 100 inserts per second for 5 seconds (**Note:** This load can be greatly reduced when connecting to a network based SQL server)

**Test Results:** Only 4 records created successfully.

**Error:**

```code
 System.InvalidOperationException: An exception has been raised that is likely due to a transient failure. Consider enabling transient error resiliency by adding 'EnableRetryOnFailure' to the 'UseSqlServer' call
```

### Suggested Retry Policy

Simple API that inserts records into a MS SQL database using EF Core. The table has an **auto increment Id** column. Based on the results above and as suggested by Microsoft the **EnableRetryOnFailure** execution policy was added.

```c#
 services.AddDbContext<TestDbContext>(builder => builder
                .UseSqlServer(Configuration.GetConnectionString("CRUD"), sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                         maxRetryCount: 2,
                         maxRetryDelay: TimeSpan.FromSeconds(30),
                         errorNumbersToAdd: null);
                }));
```

**Test:** 100 inserts per second for 5 seconds (**Note:** This load can be greatly reduced when connecting to a network based SQL server)

**Test Results:** Only 3 records created successfully.

**Error:**

```code
 System.InvalidOperationException: This SqlTransaction has completed; it is no longer usable.
         at Microsoft.Data.SqlClient.SqlTransaction.ZombieCheck()

```

### Custom Execution Strategy

Simple API that inserts records into a MS SQL database using EF Core. The table has an **auto increment Id** column. Based on the results above and online research it was decided to test with a **Custom Execution Strategy**.

**Note** the error received with regards to **ZombieCheck** is either an error in the way Dependency Injection is handled in conjunction with EnableRetryOnFailure or a false positive in the error received. This becomes clear when adding a Custom Execution Strategy and receiving the correct error.

```c#
 services.AddDbContext<TestDbContext>(builder => builder
                .UseSqlServer(Configuration.GetConnectionString("CRUD"), sqlOptions =>
                {
                    sqlOptions.ExecutionStrategy(c =>
                        new CustomExecutionStrategy(c, 2, TimeSpan.FromSeconds(30)));
                }));
```

**Test:** 100 inserts per second for 5 seconds (**Note:** This load can be greatly reduced when connecting to a network based SQL server)

**Test Results:** Only 2 records created successfully.

**Error:**

```code
  Microsoft.Data.SqlClient.SqlException (0x80131904): Transaction (Process ID 110) was deadlocked on lock resources with another process and has been chosen as the deadlock victim

```

### Changing Transaction Isolation Level

Simple API that inserts records into a MS SQL database using EF Core. The table has an **auto increment Id** column.

After discussions with Microsoft and further research the issue is as follows:

The deadlock is caused by the SELECT statement in Entity Framework to get the generated ID value. By default, it takes a shared lock which can deadlock with the exclusive locks held by concurrent UPDATE statements.

This can be avoided in 4 ways:

1. Creating the Auto Increment Column as a Primary Key solves the issue as this means the value is read from the PK's Index and a lock isn't created. **Note** Databases like MySql only allow you to create an Auto Increment Field as a Primary Key.
  
    ```sql
    use test
    go
    create table AutoGenId
    (
      ID int identity PRIMARY KEY,
      FirstName nvarchar(50),
      Surname nvarchar(50),
      IDNumber nvarchar(20)
    )
    go
    ```

2. Running SaveChanges in a ReadUncommitted Transaction (Would have to be applied per transaction or Database interaction).
  
    ```c#
    context.Database.CreateExecutionStrategy().Execute(context, c =>
    {
        using var transaction = c.Database.BeginTransaction(IsolationLevel.ReadUncommitted);
        c.SaveChanges();
        transaction.Commit();
    });
    ```

3. Setting AutoTransactionsEnabled to false (This can be done on a global context level or per transaction but means transactions won't be created automatically).

    ```c#
    _context.Database.AutoTransactionsEnabled = false;
    ```

4. Enabling Read Committed Snapshot on the Database (Database change). This is a recommendation from [Scaling .Net Core Applications](https://www.carlrippon.com/scalable-and-performant-asp-net-core-web-apis-sql-server-isolation-level/) for general performance that could assist on reading from the database as well.

    **Note:** This solution will have an impact on your database's temp DB Storage.

    ```sql
    ALTER DATABASE test
    SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE
    GO
    ```

**Test:** 100 inserts per second for 5 seconds

**Test Results:** All 500 records created successfully.

### Guid as Id

Simple API that inserts records into a MS SQL database using EF Core. The table has a **Guid Id** column. Based on the deadlock issue above it was decided to test with a Guid Id that is generated in code thus not creating a deadlock.

**Test:** 100 inserts per second for 5 seconds

**Test Results:** All 500 records created successfully.

### Stored Procedure

Simple API that inserts records into a MS SQL database using EF Core. The table has an **auto increment Id** column. Based on the deadlock issue above it was decided to test the insert with a **Stored Procedure** hoping it would handle the deadlock issue better.

**Test:** 100 inserts per second for 5 seconds

**Test Results:** All 500 records created successfully.

### EF Core with MySql

Simple API that inserts records into a MySql database using EF Core (Pomelo.EntityFrameworkCore.MySql). The table has an **auto increment Id** as a **Primary Key**. MySql only allows 1 auto increment column and it has to be a Key. 

**Test:** 100 inserts per second for 5 seconds

**Test Results:** All 500 records created successfully.

### Conclusion

If a table with an **Auto Incremented Id** column is used the load that the default implementation of EF Core and MS SQL can handle is **extremely disappointing**.It isn't close to the load that can be handled by a Stored Procedure unless the **default transaction isolation levels are changed**.

## Visual Studio Quickstart

The following tutorial shows how to run locally.

### Running Locally in Visual Studio - With Docker

1. Clone this repo and open the project in Visual Studio.
2. Open `deployment` folder in your terminal (Command Prompt) and run the following command:

    ```bash
    docker-compose up
    ```

3. Run the app with F5
4. Open `testing/artillery` folder in your terminal (Command Prompt) and run the following commands:

    ```bash
    npm install -g artillery
    artillery run ./MsSqlAutoGenId.yml
    artillery run ./MsSqlGuid.yml
    artillery run ./MsSqlStoredProcedure.yml
    artillery run ./MySqlAutoGenId.yml
    ```

### Running Locally in Visual Studio - Without Docker

1. Clone this repo and open the project in Visual Studio.
2. Setup an MS SQL database and run the scripts in `deployment/mssql-scripts` in order.
3. Setup an MySql database and run the scripts in `deployment/mysql-scripts` in order.
4. Open **appsettings.json** and update the settings. Refer to the [application settings table](#Application-Settings) for details.
5. Run the app with F5
6. Open `testing/artillery` folder in your terminal (Command Prompt) and run the following commands:

    ```bash
    npm install -g artillery
    artillery run ./MsSqlAutoGenId.yml
    artillery run ./MsSqlGuid.yml
    artillery run ./MsSqlStoredProcedure.yml
    artillery run ./MySqlAutoGenId.yml
    ```

### Application Settings

| Setting      | Description                                |
| ------------ | -------------------------------------------------- |
| **ConnectionStrings.MsSql** | Connection string of the MS SQL Database used to store inserted records |
| **ConnectionStrings.MySql** | Connection string of the MySQL Database used to store inserted records |

## Resources

- [Transient Error](https://stackoverflow.com/questions/29840282/error-when-connect-database-continuously)
- [Connection Resiliency](https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency)
- [Custom Execution Strategy](https://www.middleway.eu/azure-ef-core-and-ef-6-sql-database-connection-resiliency/)
- [Scaling .Net Core Applications](https://www.carlrippon.com/scalable-and-performant-asp-net-core-web-apis-sql-server-isolation-level/)
- [Enabling Read Committed Snapshot Isolation in MS SQL server](https://pitstop.manageengine.com/portal/en/kb/articles/enabling-read-committed-snapshot-isolation-in-ms-sql-server)
