# Azure Login Issue - Immediate Diagnosis & Fix

## The Problem

Your login isn't working because:

1. **API may not be deployed** with the new CORS changes yet
2. **Azure configuration** is missing the required settings
3. **Web App** doesn't know where the API is

## Quick Diagnosis Steps

### Step 1: Check if API is Running

Open this URL in your browser:
```
https://healthvault-api-app-h3hkd3bcf4exh2bz.ukwest-01.azurewebsites.net/swagger
```

**Expected:** Swagger documentation page loads  
**If it doesn't load:** API app is down or not deployed

### Step 2: Test Login Endpoint Directly

Open browser DevTools (F12) → Console, paste this:

```javascript
fetch('https://healthvault-api-app-h3hkd3bcf4exh2bz.ukwest-01.azurewebsites.net/api/auth/login', {
  method: 'POST',
  headers: {'Content-Type': 'application/json'},
  body: JSON.stringify({
    email: 'admin@healthvault.local',
    password: 'Admin@123'
  })
})
.then(r => r.json())
.then(d => console.log('Response:', d))
.catch(e => console.error('Error:', e));
```

**Expected:** JSON response with `succeeded: true` and a token  
**If CORS error:** Need to redeploy API with CORS fix  
**If 500 error:** Database connection issue  
**If 401 error:** Wrong credentials or JWT config issue

### Step 3: Check Network Tab

1. Open your web app: https://healthvault-web-app-hgd5bwc4e8e5dkf8.ukwest-01.azurewebsites.net
2. Open DevTools (F12) → Network tab
3. Try logging in
4. Look for `/api/auth/login` request
5. Check:
   - **Request URL** - Should point to API, not localhost
   - **Status Code** - 200 = good, 404 = wrong URL, 401 = wrong password, 500 = server error
   - **Response** - What error message?

## Most Likely Issues & Fixes

### Issue #1: API Not Redeployed with CORS Fix ⚠️

**The Problem:** We changed Program.cs but didn't redeploy the API to Azure

**The Fix:**

```powershell
# Build and publish the API
cd "c:\Users\terkp\OneDrive\Desktop\HEALTH_VAULT\HealthVault\src\HealthVault.Api"
dotnet publish -c Release -o ./bin/Release/publish

# The publish folder is now ready to deploy to Azure
```

**Then deploy using one of these methods:**

**Method A: Azure Portal (Easiest)**
1. Go to Azure Portal → Your API App Service
2. Click "Deployment Center" → "FTPS credentials"
3. Use FileZilla or any FTP client to upload files from `./bin/Release/publish`

**Method B: VS Code**
1. Install "Azure App Service" extension
2. Right-click the API project → "Deploy to Web App"
3. Select your API app

**Method C: Azure CLI**
```powershell
# Compress the publish folder
Compress-Archive -Path ./bin/Release/publish/* -DestinationPath ./api-deploy.zip -Force

# Deploy (replace <resource-group> with your actual resource group name)
az webapp deployment source config-zip `
  --resource-group <your-resource-group> `
  --name healthvault-api-app-h3hkd3bcf4exh2bz `
  --src ./api-deploy.zip
```

### Issue #2: Missing Azure Configuration ⚠️

**The Problem:** Azure doesn't have the environment variables set

**The Fix:** Configure in Azure Portal

1. Go to Azure Portal
2. Find your **Web App**: `healthvault-web-app-hgd5bwc4e8e5dkf8`
3. Click **Configuration** → **Application settings**
4. Click **+ New application setting**
5. Add:
   ```
   Name: ApiBaseUrl
   Value: https://healthvault-api-app-h3hkd3bcf4exh2bz.ukwest-01.azurewebsites.net/
   ```
6. Click **Save** → **Continue**
7. **Restart** the web app

Then do the same for your **API App**: `healthvault-api-app-h3hkd3bcf4exh2bz`

Add these settings (if missing):
```
Jwt__Key = HealthVault-MVP-ChangeThisKey-MustBe32CharsMin!
Jwt__Issuer = HealthVault
Jwt__Audience = HealthVault.Clients
Jwt__ExpiresMinutes = 480
```

And verify the connection string exists:
```
ConnectionStrings__DefaultConnection = Server=tcp:healthvault-sql-server.database.windows.net,1433;Initial Catalog=HealthVaultDb;Persist Security Info=False;User ID=CloudSAC84a40e8;Password=Terkperson@830;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### Issue #3: Database Not Seeded ⚠️

**The Problem:** Admin account doesn't exist in Azure database

**The Fix:** Check Azure SQL Database

1. Go to Azure Portal → Your SQL Database
2. Click **Query editor**
3. Login with your SQL credentials
4. Run this query:

```sql
SELECT Email, UserName FROM AspNetUsers;
```

**If empty:** Database not seeded. The API should seed it on startup, but if it didn't:

**Option A:** Restart the API app (it seeds on startup)  
**Option B:** Manually insert admin user (not recommended)  
**Option C:** Run migrations from local machine connected to Azure DB

### Issue #4: Web App Can't Reach API ⚠️

**The Problem:** CORS or network configuration

**Quick Test:** Open browser console on your web app and run:

```javascript
fetch('https://healthvault-api-app-h3hkd3bcf4exh2bz.ukwest-01.azurewebsites.net/swagger/index.html')
  .then(r => console.log('API reachable:', r.ok))
  .catch(e => console.log('API unreachable:', e));
```

**If unreachable:** 
- Check API app is running in Azure Portal
- Check App Service logs for errors

## Priority Action Plan

**Do this right now in order:**

### 1️⃣ CHECK API IS RUNNING (2 minutes)

Open: https://healthvault-api-app-h3hkd3bcf4exh2bz.ukwest-01.azurewebsites.net/swagger

✅ **If it loads:** API is running, skip to step 2  
❌ **If it doesn't:** API needs to be restarted or redeployed

### 2️⃣ CONFIGURE AZURE SETTINGS (5 minutes)

Go to Azure Portal and add the settings listed in "Issue #2" above

### 3️⃣ REDEPLOY API WITH CORS FIX (10 minutes)

Follow "Issue #1" instructions to redeploy the API

### 4️⃣ TEST AGAIN (1 minute)

Try logging in at: https://healthvault-web-app-hgd5bwc4e8e5dkf8.ukwest-01.azurewebsites.net

Login with:
- Email: `admin@healthvault.local`
- Password: `Admin@123`

## Check Azure Logs

If still not working, check logs:

**Azure Portal Method:**
1. Go to your API App Service
2. Click **Log stream** in left menu
3. Try logging in again
4. Watch for error messages

**Look for these common errors:**

| Error Message | Cause | Fix |
|--------------|-------|-----|
| "Cannot open server" | Database connection | Check connection string |
| "No such host" | DNS issue | Check API URL in Web App config |
| "CORS policy" | CORS not configured | Redeploy API with CORS fix |
| "Invalid credentials" | Wrong password | Check password in DbSeeder.cs |
| "JWT validation failed" | JWT config mismatch | Check Jwt__Key matches in both apps |

## Test Credentials

Make sure you're using the exact credentials from DbSeeder:

From your code, the default accounts are:

**Admin:**
- Email: `admin@healthvault.local`
- Password: `Admin@123`

**Doctor:**
- Email: `doctor@healthvault.local`
- Password: `Doctor@123`

**Patient:**
- Email: `patient@healthvault.local`
- Password: `Patient@123`

## Still Not Working?

If you've done all the above and it still doesn't work:

1. **Check exact error in browser console** (F12 → Console)
2. **Check Network tab** for the actual error response
3. **Check Azure Log Stream** for server-side errors
4. **Run the test script below**

## Quick Test Script

Run this in PowerShell to test everything:

```powershell
Write-Host "Testing HealthVault Azure Deployment" -ForegroundColor Cyan

# Test API
Write-Host "`n1. Testing API health..."
try {
    $api = Invoke-WebRequest -Uri "https://healthvault-api-app-h3hkd3bcf4exh2bz.ukwest-01.azurewebsites.net/swagger/index.html" -Method Head -TimeoutSec 10
    Write-Host "   ✓ API is online" -ForegroundColor Green
} catch {
    Write-Host "   ✗ API is offline or unreachable" -ForegroundColor Red
}

# Test Web App
Write-Host "`n2. Testing Web App..."
try {
    $web = Invoke-WebRequest -Uri "https://healthvault-web-app-hgd5bwc4e8e5dkf8.ukwest-01.azurewebsites.net" -Method Head -TimeoutSec 10
    Write-Host "   ✓ Web App is online" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Web App is offline or unreachable" -ForegroundColor Red
}

# Test Login
Write-Host "`n3. Testing login endpoint..."
$body = @{email="admin@healthvault.local";password="Admin@123"} | ConvertTo-Json
try {
    $login = Invoke-RestMethod -Uri "https://healthvault-api-app-h3hkd3bcf4exh2bz.ukwest-01.azurewebsites.net/api/auth/login" -Method Post -Body $body -ContentType "application/json" -TimeoutSec 15
    if ($login.succeeded) {
        Write-Host "   ✓ Login successful!" -ForegroundColor Green
    } else {
        Write-Host "   ✗ Login failed: $($login.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "   ✗ Login endpoint error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nDone!" -ForegroundColor Cyan
```

## Next Steps After Fix

Once login works:
1. Test all three user types (admin, doctor, patient)
2. Test creating appointments
3. Test uploading medical records
4. Check Azure costs and set budget alerts
5. Change JWT secret to something more secure
6. Move database password to Azure Key Vault

---

**Created:** 2026-09-25  
**Priority:** URGENT - Blocking live app usage
