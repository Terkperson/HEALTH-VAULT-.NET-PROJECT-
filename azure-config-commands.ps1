# Azure Configuration Helper Script
# Run these commands in Azure Cloud Shell or with Azure CLI installed

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Azure Configuration Commands" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Copy and paste these commands in Azure Portal Cloud Shell" -ForegroundColor Yellow
Write-Host "or run with Azure CLI installed locally" -ForegroundColor Yellow
Write-Host ""

$resourceGroup = Read-Host "Enter your Resource Group name (e.g., healthvault-rg)"
$webAppName = "healthvault-web-app-hgd5bwc4e8e5dkf8"
$apiAppName = "healthvault-api-app-h3hkd3bcf4exh2bz"

Write-Host ""
Write-Host "=== Web App Configuration ===" -ForegroundColor Green
Write-Host ""
Write-Host "Setting ApiBaseUrl for Web App..." -ForegroundColor Cyan

$webCommands = @"
# Configure Web App
az webapp config appsettings set \
  --resource-group $resourceGroup \
  --name $webAppName \
  --settings ApiBaseUrl="https://$apiAppName.ukwest-01.azurewebsites.net/"

# Restart Web App
az webapp restart \
  --resource-group $resourceGroup \
  --name $webAppName
"@

Write-Host $webCommands -ForegroundColor White
Write-Host ""

Write-Host "=== API App Configuration ===" -ForegroundColor Green
Write-Host ""
Write-Host "Configuring JWT and Connection String for API App..." -ForegroundColor Cyan

$apiCommands = @"
# Configure API App
az webapp config appsettings set \
  --resource-group $resourceGroup \
  --name $apiAppName \
  --settings \
    Jwt__Key="HealthVault-MVP-ChangeThisKey-MustBe32CharsMin!" \
    Jwt__Issuer="HealthVault" \
    Jwt__Audience="HealthVault.Clients" \
    Jwt__ExpiresMinutes="480"

# Configure Connection String
az webapp config connection-string set \
  --resource-group $resourceGroup \
  --name $apiAppName \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:healthvault-sql-server.database.windows.net,1433;Initial Catalog=HealthVaultDb;Persist Security Info=False;User ID=CloudSAC84a40e8;Password=Terkperson@830;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Restart API App
az webapp restart \
  --resource-group $resourceGroup \
  --name $apiAppName
"@

Write-Host $apiCommands -ForegroundColor White
Write-Host ""

Write-Host "=== Manual Configuration (Azure Portal) ===" -ForegroundColor Green
Write-Host ""
Write-Host "If you prefer using Azure Portal:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Web App Configuration:" -ForegroundColor Cyan
Write-Host "   - Navigate to: $webAppName → Configuration → Application Settings" -ForegroundColor White
Write-Host "   - Add new setting:" -ForegroundColor White
Write-Host "     Name: ApiBaseUrl" -ForegroundColor Gray
Write-Host "     Value: https://$apiAppName.ukwest-01.azurewebsites.net/" -ForegroundColor Gray
Write-Host ""
Write-Host "2. API App Configuration:" -ForegroundColor Cyan
Write-Host "   - Navigate to: $apiAppName → Configuration → Application Settings" -ForegroundColor White
Write-Host "   - Add these settings:" -ForegroundColor White
Write-Host "     Jwt__Key = HealthVault-MVP-ChangeThisKey-MustBe32CharsMin!" -ForegroundColor Gray
Write-Host "     Jwt__Issuer = HealthVault" -ForegroundColor Gray
Write-Host "     Jwt__Audience = HealthVault.Clients" -ForegroundColor Gray
Write-Host "     Jwt__ExpiresMinutes = 480" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Restart both apps after configuration" -ForegroundColor Yellow
Write-Host ""

Write-Host "=== Testing Commands ===" -ForegroundColor Green
Write-Host ""

$testCommands = @"
# Test API Health
curl https://$apiAppName.ukwest-01.azurewebsites.net/swagger/index.html

# Test Login Endpoint
curl -X POST https://$apiAppName.ukwest-01.azurewebsites.net/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{\"email\":\"admin@healthvault.local\",\"password\":\"Admin@123\"}'

# View Web App Logs
az webapp log tail \
  --resource-group $resourceGroup \
  --name $webAppName

# View API App Logs
az webapp log tail \
  --resource-group $resourceGroup \
  --name $apiAppName
"@

Write-Host $testCommands -ForegroundColor White
Write-Host ""

Write-Host "=== Quick Reference ===" -ForegroundColor Green
Write-Host ""
Write-Host "Web App URL: https://$webAppName.ukwest-01.azurewebsites.net" -ForegroundColor Cyan
Write-Host "API App URL: https://$apiAppName.ukwest-01.azurewebsites.net" -ForegroundColor Cyan
Write-Host "Swagger UI: https://$apiAppName.ukwest-01.azurewebsites.net/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "Default Admin Login:" -ForegroundColor Yellow
Write-Host "  Email: admin@healthvault.local" -ForegroundColor Gray
Write-Host "  Password: Admin@123" -ForegroundColor Gray
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Configuration Ready!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Copy commands above and run in Azure CLI or Cloud Shell" -ForegroundColor White
Write-Host "2. Wait for apps to restart (2-3 minutes)" -ForegroundColor White
Write-Host "3. Test login with provided credentials" -ForegroundColor White
Write-Host "4. Check Azure Log Stream if issues persist" -ForegroundColor White
Write-Host ""
