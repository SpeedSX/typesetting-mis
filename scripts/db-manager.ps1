# Database Management Script for Development
# This script provides various database operations
# Usage: .\scripts/db-manager.ps1 [command] [options]

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("drop", "add", "update", "reset", "status", "list")]
    [string]$Command,
    
    [string]$MigrationName = "",
    [switch]$Force
)

Write-Host "🗄️  Database Manager" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan

# Change to backend directory
Set-Location -Path "backend"

try {
    switch ($Command) {
        "drop" {
            Write-Host "🗑️  Dropping database..." -ForegroundColor Yellow
            $forceFlag = if ($Force) { "--force" } else { "" }
            dotnet ef database drop $forceFlag --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Database dropped successfully" -ForegroundColor Green
            } else {
                Write-Host "❌ Failed to drop database" -ForegroundColor Red
                exit 1
            }
        }
        
        "add" {
            if ([string]::IsNullOrEmpty($MigrationName)) {
                Write-Host "❌ Migration name is required for 'add' command" -ForegroundColor Red
                Write-Host "Usage: .\scripts\db-manager.ps1 add -MigrationName 'YourMigrationName'" -ForegroundColor Yellow
                exit 1
            }
            
            Write-Host "📝 Adding migration: $MigrationName" -ForegroundColor Yellow
            dotnet ef migrations add $MigrationName --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Migration added successfully" -ForegroundColor Green
            } else {
                Write-Host "❌ Failed to add migration" -ForegroundColor Red
                exit 1
            }
        }
        
        "update" {
            Write-Host "🚀 Updating database..." -ForegroundColor Yellow
            dotnet ef database update --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Database updated successfully" -ForegroundColor Green
            } else {
                Write-Host "❌ Failed to update database" -ForegroundColor Red
                exit 1
            }
        }
        
        "reset" {
            if ([string]::IsNullOrEmpty($MigrationName)) {
                $MigrationName = "ResetDatabase_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
            }
            
            Write-Host "🔄 Resetting database..." -ForegroundColor Yellow
            Write-Host "  1. Dropping database..." -ForegroundColor Gray
            dotnet ef database drop --force --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API
            
            if ($LASTEXITCODE -ne 0) {
                Write-Host "❌ Failed to drop database" -ForegroundColor Red
                exit 1
            }
            
            Write-Host "  2. Adding migration: $MigrationName..." -ForegroundColor Gray
            dotnet ef migrations add $MigrationName --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API
            
            if ($LASTEXITCODE -ne 0) {
                Write-Host "❌ Failed to add migration" -ForegroundColor Red
                exit 1
            }
            
            Write-Host "  3. Updating database..." -ForegroundColor Gray
            dotnet ef database update --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Database reset completed!" -ForegroundColor Green
            } else {
                Write-Host "❌ Failed to update database" -ForegroundColor Red
                exit 1
            }
        }
        
        "status" {
            Write-Host "📊 Database status..." -ForegroundColor Yellow
            dotnet ef database update --dry-run --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API
        }
        
        "list" {
            Write-Host "📋 Available migrations..." -ForegroundColor Yellow
            dotnet ef migrations list --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API
        }
    }
    
} catch {
    Write-Host "❌ An error occurred: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    # Return to original directory
    Set-Location -Path ".."
}

Write-Host "`n💡 Available commands:" -ForegroundColor Magenta
Write-Host "  .\scripts\db-manager.ps1 drop                    # Drop database"
Write-Host "  .\scripts\db-manager.ps1 add -MigrationName 'X'  # Add migration"
Write-Host "  .\scripts\db-manager.ps1 update                  # Update database"
Write-Host "  .\scripts\db-manager.ps1 reset                  # Drop + Add + Update"
Write-Host "  .\scripts\db-manager.ps1 status                 # Check pending migrations"
Write-Host "  .\scripts\db-manager.ps1 list                   # List all migrations"
