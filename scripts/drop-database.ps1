# Database Drop Script for Development
# This script drops the database
# Usage: .\scripts/drop-database.ps1

Write-Host "🗑️  Dropping database..." -ForegroundColor Yellow

# Change to backend directory
Set-Location -Path "backend"

try {
    dotnet ef database drop --force --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to drop database" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Database dropped successfully" -ForegroundColor Green
    
} catch {
    Write-Host "❌ An error occurred: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    # Return to original directory
    Set-Location -Path ".."
}
