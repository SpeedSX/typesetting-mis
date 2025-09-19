# Database Reset Script for Development
# This script drops, recreates, and updates the database
# Usage: .\scripts\reset-database.ps1 [migration-name]

param(
    [string]$MigrationName = "ResetDatabase"
)

Write-Host "🔄 Starting database reset process..." -ForegroundColor Cyan

# Change to backend directory
Set-Location -Path "backend"

try {
    Write-Host "📦 Dropping existing database..." -ForegroundColor Yellow
    dotnet ef database drop --force --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to drop database" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Database dropped successfully" -ForegroundColor Green
    
    Write-Host "📝 Adding new migration: $MigrationName" -ForegroundColor Yellow
    dotnet ef migrations add $MigrationName --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to add migration" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Migration added successfully" -ForegroundColor Green
    
    Write-Host "🚀 Updating database..." -ForegroundColor Yellow
    dotnet ef database update --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to update database" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Database updated successfully" -ForegroundColor Green
    Write-Host "🎉 Database reset completed!" -ForegroundColor Cyan
    
} catch {
    Write-Host "❌ An error occurred: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    # Return to original directory
    Set-Location -Path ".."
}

Write-Host "`n💡 Usage examples:" -ForegroundColor Magenta
Write-Host "  .\scripts\reset-database.ps1                    # Uses default migration name 'ResetDatabase'"
Write-Host "  .\scripts\reset-database.ps1 -MigrationName 'AddAdminUsers'"
Write-Host "  .\scripts\reset-database.ps1 -MigrationName 'UpdateUserCompanyBinding'"
