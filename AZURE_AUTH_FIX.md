# Azure Authentication Issue - Diagnosis & Fix

## Problem Analysis

Based on the browser console errors:
1. **404 Error** on `healthcare-hero.jpg` - missing static file
2. **WebSocket Connection** successful to Azure
3. **Browser Extension Error** - "message channel closed before response" (Chrome PDF viewer extension interference)
4. **Login Failure** - likely CORS or API connection issue

## Root Causes

### 1. **API Base URL Configuration**
Your web app has a localhost fallback in `appsettings.json`:
```json
"ApiBaseUrl": "http://localhost:5080/"
```

When deployed to Azure, this should point to your API URL.

### 2. **CORS Configuration**
The API allows any origin (`AllowAnyOrigin()`), but Azure may require explicit configuration.

### 3. **HTTPS vs HTTP**
- API URL in Program.cs uses HTTPS: `https://healthvault-api-app-h3hkd3bcf4exh2bz.ukwest-01.azurewebsites.net/`
- But the server binds to HTTP: `http://0.0.0.0:{port}`

Azure App Service handles HTTPS termination, so this should work, but JWT validation may be affected.

### 4. **JWT Configuration**
Your JWT settings use a basic key. Ensure the same JWT settings exist in Azure App Configuration.

## Fixes

### Fix 1: Update Web App Configuration (Azure Portal)

Go to your Web App in Azure Portal → Configuration → Application Settings and add:

```
ApiBaseUrl = https://healthvault-api-app-h3hkd3bcf4exh2bz.ukwest-01.azurewebsites.net/
```

### Fix 2: Update API CORS Settings

Update your API's CORS policy to explicitly allow your web app domain:

**File:** `HealthVault/src/HealthVault.Api/Program.cs`

Change:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Client", policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin());
});
```

To:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Client", policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .WithOrigins(
                  "https://healthvault-web-app-hgd5bwc4e8e5dkf8.ukwest-01.azurewebsites.net",
                  "http://localhost:5081" // for local development
              )
              .AllowCredentials());
});
```

### Fix 3: Ensure JWT Key Match

Verify in Azure Portal (both API and Web App) → Configuration → Application Settings:

```
Jwt__Key = HealthVault-MVP-ChangeThisKey-MustBe32CharsMin!
Jwt__Issuer = HealthVault
Jwt__Audience = HealthVault.Clients
Jwt__ExpiresMinutes = 480
```

⚠️ **Note:** Azure uses double underscore `__` for nested configuration sections.

### Fix 4: Enable Detailed Logging

Add to both API and Web App in Azure Portal → Configuration → Application Settings:

```
ASPNETCORE_ENVIRONMENT = Development
Logging__LogLevel__Default = Information
Logging__LogLevel__Microsoft.AspNetCore = Warning
```

Then check the Log Stream in Azure Portal to see actual errors.

### Fix 5: Database Connection String

Verify your API has the connection string in Azure Configuration:

```
ConnectionStrings__DefaultConnection = Server=tcp:healthvault-sql-server.database.windows.net,1433;Initial Catalog=HealthVaultDb;Persist Security Info=False;User ID=CloudSAC84a40e8;Password=Terkperson@830;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## Testing Steps

1. **Test API Directly**
   ```bash
   curl -X POST https://healthvault-api-app-h3hkd3bcf4exh2bz.ukwest-01.azurewebsites.net/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"admin@healthvault.local","password":"Admin@123"}'
   ```

2. **Check Browser Console**
   - Open DevTools (F12)
   - Go to Network tab
   - Try logging in
   - Check the `/api/auth/login` request:
     - Status code
     - Response body
     - Request headers

3. **Check Azure Logs**
   - Go to Azure Portal
   - Select your API App Service
   - Go to "Log stream"
   - Watch logs while attempting login

## Quick Deploy Script

Run this to update and redeploy with fixes:

```powershell
# Redeploy API with updated CORS
cd "c:\Users\terkp\OneDrive\Desktop\HEALTH_VAULT\HealthVault\src\HealthVault.Api"
dotnet publish -c Release -o ./publish
# Then deploy the publish folder to Azure

# Restart both apps in Azure Portal after configuration changes
```

## Common Issues

### Issue: "Origin not allowed by CORS"
**Solution:** Use Fix 2 above with explicit origins

### Issue: "401 Unauthorized" on dashboard
**Solution:** JWT key mismatch. Verify Fix 3

### Issue: "500 Internal Server Error"
**Solution:** Database connection issue. Verify Fix 5

### Issue: Login succeeds but dashboard fails
**Solution:** Check JWT token in browser storage (F12 → Application → Session Storage)

## Next Steps

1. Apply Fix 1 (Azure configuration) immediately - no redeployment needed
2. Apply Fix 2 (CORS changes) and redeploy API
3. Test with provided curl command
4. Check Azure logs for specific errors
5. Update this document with findings

## Security Note

⚠️ **Before going to production:**
1. Change JWT key to a strong, random value
2. Remove database password from code - use Azure Key Vault
3. Restrict CORS to specific domains only
4. Enable HTTPS-only traffic in Azure App Service
