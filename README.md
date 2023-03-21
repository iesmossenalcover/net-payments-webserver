# For development
## Add Database connection string
Inside de project folder, execute:

`dotnet user-secrets set "PostgreSqlConnectionString" "<connection-string-value>"`

# For migrations
## Requisites
### Install ef globally
`dotnet tool update --global dotnet-ef`

### Add this package to the project
`dotnet add package Microsoft.EntityFrameworkCore.Design`

## Migrate and update

1. `dotnet ef migrations add MigrationName`

2. `dotnet ef database update`

## Production update

### Generate the script for the migrations
1. `dotnet ef migrations script -o update.db`

2. Execute the script into production db.

3. Grant permissions
    - GRANT ALL ON ALL TABLES IN SCHEMA main TO paymentsapi;
    - GRANT USAGE ON SCHEMA main to paymentsapi;


# DDBB model in docs folder