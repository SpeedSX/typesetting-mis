# Database Update Script for Development
# This script adds a migration and updates the database
# Usage: .\scripts\update-database.ps1 [migration-name]

param(
    [string]$MigrationName = "UpdateDatabase"
)

Write-Host "🔄 Starting database update process..." -ForegroundColor Cyan

# Change to backend directory
Set-Location -Path "backend"

try {
    Write-Host "📝 Adding migration: $MigrationName" -ForegroundColor Yellow
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
    Write-Host "🎉 Database update completed!" -ForegroundColor Cyan
    
} catch {
    Write-Host "❌ An error occurred: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    # Return to original directory
    Set-Location -Path ".."
}

Write-Host "`n💡 Usage examples:" -ForegroundColor Magenta
Write-Host "  .\scripts\update-database.ps1                    # Uses default migration name 'UpdateDatabase'"
Write-Host "  .\scripts\update-database.ps1 -MigrationName 'AddUserCompanyNullable'"
Write-Host "  .\scripts\update-database.ps1 -MigrationName 'FixIndexConstraints'"
