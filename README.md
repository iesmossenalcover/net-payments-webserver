# For migrations

## Requisites
### Install ef globally
`dotnet tool update --global dotnet-ef`

### Add this package to the project
`dotnet add package Microsoft.EntityFrameworkCore.Design`

## Migrate and update

1. `dotnet ef migrations add MigrationName`

2. `dotnet ef database update`

## 