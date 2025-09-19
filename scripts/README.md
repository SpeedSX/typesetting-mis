# Database Management Scripts

This directory contains PowerShell and Batch scripts to help manage the database during development.

## Scripts Overview

### PowerShell Scripts (Recommended)

| Script | Purpose | Usage |
|--------|---------|-------|
| `reset-database.ps1` | Complete database reset (drop + add migration + update) | `.\scripts\reset-database.ps1 [migration-name]` |
| `update-database.ps1` | Add migration and update database | `.\scripts\update-database.ps1 [migration-name]` |
| `drop-database.ps1` | Drop the database only | `.\scripts\drop-database.ps1` |
| `db-manager.ps1` | Comprehensive database management tool | `.\scripts\db-manager.ps1 [command] [options]` |

### Batch Scripts (Windows)

| Script | Purpose | Usage |
|--------|---------|-------|
| `reset-db.bat` | Complete database reset | `scripts\reset-db.bat [migration-name]` |

## Quick Start

### Most Common Usage (PowerShell)
```powershell
# Complete database reset with custom migration name
.\scripts\reset-database.ps1 -MigrationName "AddUserCompanyNullable"

# Quick database reset with default name
.\scripts\reset-database.ps1

# Just add migration and update
.\scripts\update-database.ps1 -MigrationName "FixIndexConstraints"
```

### Most Common Usage (Batch)
```cmd
# Complete database reset
scripts\reset-db.bat AddUserCompanyNullable

# Quick database reset with default name
scripts\reset-db.bat
```

## Advanced Usage with db-manager.ps1

The `db-manager.ps1` script provides all database operations in one tool:

```powershell
# Drop database
.\scripts\db-manager.ps1 drop

# Add migration
.\scripts\db-manager.ps1 add -MigrationName "YourMigrationName"

# Update database
.\scripts\db-manager.ps1 update

# Complete reset
.\scripts\db-manager.ps1 reset -MigrationName "YourMigrationName"

# Check database status
.\scripts\db-manager.ps1 status

# List all migrations
.\scripts\db-manager.ps1 list
```

## Prerequisites

- .NET 8 SDK installed
- Entity Framework Core tools installed globally:
  ```bash
  dotnet tool install --global dotnet-ef
  ```
- PostgreSQL running and accessible
- Connection string configured in `appsettings.Development.json`

## Development Workflow

1. **Make changes to entities** in `TypesettingMIS.Core/Entities/`
2. **Update DbContext** in `TypesettingMIS.Infrastructure/Data/ApplicationDbContext.cs`
3. **Reset database** with your changes:
   ```powershell
   .\scripts\reset-database.ps1 -MigrationName "DescribeYourChanges"
   ```
4. **Test your changes** by running the API
5. **Repeat as needed** until database structure is stable

## Troubleshooting

### Common Issues

1. **"dotnet ef not found"**
   - Install EF Core tools: `dotnet tool install --global dotnet-ef`

2. **"Database connection failed"**
   - Check PostgreSQL is running
   - Verify connection string in `appsettings.Development.json`

3. **"Migration already exists"**
   - Use a different migration name
   - Or drop database first: `.\scripts\drop-database.ps1`

4. **"PowerShell execution policy"**
   - Run: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser`

### Manual Commands

If scripts fail, you can run commands manually:

```bash
cd backend

# Drop database
dotnet ef database drop --force --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API

# Add migration
dotnet ef migrations add YourMigrationName --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API

# Update database
dotnet ef database update --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API
```

## Notes

- These scripts are designed for **development only**
- Never use these scripts in production
- Always backup your data before running reset scripts
- The scripts automatically change to the `backend` directory and return to the root
