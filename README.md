# For development

The app only talks to PostgreSQL. There is no SQLite / in-memory option.

## 1. Prepare the dev database
On the dev PostgreSQL server, as a superuser:

```sql
CREATE DATABASE payments;
CREATE ROLE paymentsapi WITH LOGIN PASSWORD '<password>';
```

Then, connected to the `payments` database, still as a superuser:

```sql
CREATE SCHEMA IF NOT EXISTS main AUTHORIZATION paymentsapi;
-- The migrations do NOT create this extension and the people search needs it
-- (EF.Functions.Unaccent in PeopleRepository / PeopleGroupCourseRepository).
CREATE EXTENSION unaccent SCHEMA main;
ALTER ROLE paymentsapi SET search_path = main, public;
```

## 2. Add the connection string
Inside the project folder:

`dotnet user-secrets set "PostgreSqlConnectionString" "Host=<host>;Port=5432;Database=payments;Username=paymentsapi;Password=<password>"`

It is stored in the user-secrets store, outside the repo, so it is never committed.
If it is missing the app fails on startup with `Configure PostgreSqlConnectionString`.

## 3. Create the tables
Never run `dotnet ef database update`. Migrations are **always** applied through a SQL script
that we execute ourselves, in every environment. Generate the whole schema from scratch:

`dotnet ef migrations script --idempotent -o schema.sql`

With no migration names it goes from zero to the latest migration, so it builds the full
database. `--idempotent` guards every step, so re-running it is safe.

Then execute it as `paymentsapi`:

`psql -h <host> -U paymentsapi -d payments -f schema.sql`

No `GRANT` is needed in dev: the script runs as `paymentsapi`, so that role owns the tables.

## 4. Seed the minimum rows
The app needs a current course, an app config row and an admin user to be usable:

```sql
INSERT INTO main.course("Name", "StartDate", "EndDate", "Active") VALUES('25-26', '2025-09-01', '2026-07-30', true);
INSERT INTO main.app_config("DisplayEnrollment") VALUES(false);
INSERT INTO main.user("Username", "HashedPassword", "Firstname", "Lastname") VALUES('admin', '', 'Administrador', '');
```

# For migrations
## Requisites
### Install ef globally
`dotnet tool update --global dotnet-ef`

Global tools live in `~/.dotnet/tools`, which is not on the PATH by default. If `dotnet ef`
says "command not found", add it:

`export PATH="$PATH:$HOME/.dotnet/tools"`

### Design package
`Microsoft.EntityFrameworkCore.Design` is already referenced in the csproj. Nothing to add.

## Migrate and update

1. `dotnet ef migrations add MigrationName`

2. `dotnet ef database update`

## Production update

### Generate the script for the migrations
1. `dotnet ef migrations script -o update.db`
#### To specify migrations
#### DESDE darrera migracio feta HASTA nom de la migracio que es vol fer
2. `dotnet ef migrations script DESDE HAST -o update.db`

2. Execute the script into production db.

3. Grant permissions
    - GRANT ALL ON ALL TABLES IN SCHEMA main TO paymentsapi;
    - GRANT USAGE ON SCHEMA main to paymentsapi;

4. Add course
INSERT INTO main.course("Name", "StartDate", "EndDate", "Active") VALUES('22-23', '2022-09-01', '2023-07-30', true);

5. Add appConfig
insert into main.app_config("DisplayEnrollment") values (false);

6. Add AdminUser withHash
insert into main.user("Username", "HashedPassword", "Firstname", "Lastname") values ('admin','','Administrador', '');

7. Add unaccent extension
create extension unaccent schema main; 
ALTER ROLE paymentsapi SET search_path = main, public;

### Config OAuth2.0 with Google
