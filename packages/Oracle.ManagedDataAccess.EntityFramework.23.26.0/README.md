![Oracle Logo](https://raw.githubusercontent.com/oracle/dotnet-db-samples/master/images/oracle-nuget.png)
# Oracle.ManagedDataAccess.EntityFramework 23.26.0
Release Notes for Oracle Data Provider for .NET, Managed Driver for Entity Framework 6 NuGet Package

October 2025

This document provides information that supplements the [Oracle Data Provider for .NET (ODP.NET) documentation](https://docs.oracle.com/en/database/oracle/oracle-database/23/odpnt/index.html).

## Oracle .NET Links
* [Oracle .NET Home Page](https://www.oracle.com/database/technologies/appdev/dotnet.html)
* [GitHub - Sample Code](https://github.com/oracle/dotnet-db-samples)
* [ODP.NET Discussion Forum](https://forums.oracle.com/ords/apexds/domain/dev-community/category/odp-dot-net)
* [YouTube](https://www.youtube.com/user/OracleDOTNETTeam)
* [X (Twitter)](https://twitter.com/oracledotnet)
* [Email Newsletter Sign Up](https://go.oracle.com/LP=28277?elqCampaignId=124071&nsl=onetdev)

## New Features
* None

## Bug Fixes / Changes since Oracle.ManagedDataAccess.EntityFramework 23.9.0
* Upgrade dependency to ODP.NET 23.26.0 or higher

## Installation Changes
The following app/web.config entries are added when installing this NuGet package:

1) Entity Framework

The Oracle Entity Framework 6 "provider" entry is added to the "entityFramework" section to enable Entity Framework to use Oracle.ManagedDataAccess.dll.

2) Connection String

A sample ODP.NET connection string entry is added to enable the classes that are derived from DbContext to be associated with a connection string instead of associating the derived class with a connection string programmatically via its constructor. The name "OracleDbContext" should be changed to the class name that derives from DbContext. The connectionString attribute should be modified properly to set the "User Id", "Password", and 
"Data Source" appropriately with valid values.

## Tips, Limitations, and Known Issues
This section contains Entity Framework related information that pertains to both ODP.NET, Managed Driver and ODP.NET, Unmanaged Driver. 

1. Interval Day to Second and Interval Year to Month column values cannot be compared to literals in a WHERE clause of a LINQ to Entities or an Entity SQL query.

2. LINQ to Entities and Entity SQL (ESQL) queries that require the usage of SQL APPLY in the generated queries will cause SQL syntax error(s) if the Oracle Database being used does not support APPLY. In such cases, the inner exception message will indicate that APPLY is not supported by the database.

3. ODP.NET does not currently support wildcards that accept character ranges for the LIKE operator in Entity SQL (i.e. [] and [^]). [Bug 11683837]

4. Executing LINQ or ESQL query against tables with one or more column names that are close to or equal to the maximum length of identifiers (30 bytes) may encounter "ORA-00972: identifier is too long" error, due to the usage of alias identifier(s) in the generated SQL that exceed the limit.

5. An "ORA-00932: inconsistent datatypes: expected - got NCLOB" error will be encountered when trying to bind a string that is equal to or greater than 2,000 characters in length to an XMLType column or parameter. [Bug 12630958]

6. An "ORA-00932 : inconsistent datatypes" error can be encountered if a string of 2,000 or more characters, or a byte array with 4,000 bytes or more in length, is bound in a WHERE clause of a LINQ/ESQL query. The same error can be encountered if an entity property that maps to a BLOB, CLOB, NCLOB, LONG, LONG RAW, XMLTYPE column is used in a WHERE clause of a LINQ/ESQL query.

7. An "Arithmetic operation resulted in an overflow" exception can be encountered when fetching numeric values that have more precision than what the .NET type can support. In such cases, the LINQ or ESQL query can "cast" the value to a particular .NET or EDM type to limit the precision and avoid the exception. This approach can be useful if the LINQ/ESQL query has computed/calculated columns which will store up to 38 precision in Oracle, which cannot be represented as .NET decimal unless the value is casted. 

8. Oracle Database treats NULLs and empty strings the same. When executing string related operations on NULLS or empty strings, the result will be NULL. When comparing strings with NULLs, use the equals operator (i.e. "x == NULL") in the LINQ query, which will in turn use the "IS NULL" condition in the generated SQL that will appropriately detect NULL-ness.

9. If an exception message of "The store provider factory type 'Oracle.ManagedDataAccess.Client.OracleClientFactory' does not implement the IServiceProvider interface." is encountered when executing an Entity Framework application with ODP.NET, the machine.config requires and entry for ODP.NET under the "DbProviderFactories" section. To resolve this issue by adding an entry in the machine.config, reinstall ODP.NET

10. Creating a second instance of the context that derives from DbContext within an application and executing methods within the scope of that context that result in an interaction with the database may result in unexpected recreation of the database objects if the DropCreateDatabaseAlways database initializer is used.

Known workarounds:
- Use a different database initializer,
- Use an operating system authenticated user for the connection, or 
- Include "Persist Security Info=true" in the connection string (Warning: Turning on "Persist Security Info" will cause 
the password to remain as part of the connection string).

 Copyright (c) 2024, 2025, Oracle and/or its affiliates.

