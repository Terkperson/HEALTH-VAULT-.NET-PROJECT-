# IMMEDIATE FIX - Login Not Working

## What to Do Right Now

### STEP 1: Add Azure Configuration (5 minutes) - DO THIS FIRST!

1. Open [Azure Portal](https://portal.azure.com)
2. Search for `healthvault-web-app-hgd5bwc4e8e5dkf8`
3. Click on your Web App
4. In the left menu, click **Configuration**
5. Click **Application settings** tab
6. Click **+ New application setting**
7. Add this setting:
   ```
   Name: ApiBaseUrl
   Value: https://healthvault-api-app-h3hkd3bcf4exh2bz.ukwest-01.azurewebsites.net/
   ```
8. Click **OK**
9. Click **Save** at the top
10. Click **Continue** when prompted
11. Click **Restart** in the Overview page

### STEP 2: Check API Configuration (3 minutes)

1. In Azure Portal, search for `healthvault-api-app-h3hkd3bcf4exh2bz`
2. Click on your API App
3. Click **Configuration** → **Application settings**
4. Verify these settings exist (add if missing):

**Application Settings:**
```
Jwt__Key = HealthVault-MVP-ChangeThisKey-MustBe32CharsMin!
Jwt__Issuer = HealthVault
Jwt__Audience = HealthVault.Clients
Jwt__ExpiresMinutes = 480
```

5. Click **Connection strings** tab
6. Verify this exists (add if missing):

**Connection String:**
```
Name: DefaultConnection
Value: Server=tcp:healthvault-sql-server.database.windows.net,1433;Initial Catalog=HealthVaultDb;Persist Security Info=False;User ID=CloudSAC84a40e8;Password=Terkperson@830;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
Type: SQLAzure
```

7. Click **Save** → **Continue** → **Restart**

### STEP 3: Redeploy API with CORS Fix (10 minutes)

The code change we made needs to be deployed to Azure:

```powershell
# Open PowerShell and run:
cd "c:\Users\terkp\OneDrive\Desktop\HEALTH_VAULT\HealthVault\src\HealthVault.Api"

# Build and publish
dotnet publish -c Release -o ./bin/Release/publish
```

**Now deploy using Azure Portal:**

1. Go to your API App in Azure Portal
2. Click **Deployment Center** in left menu
3. Under "Manual Deployment (Push/Sync)", click **FTPS credentials** tab
4. Note the FTP endpoint and username/password
5. Use FileZilla or WinSCP to connect
6. Upload all files from `./bin/Release/publish` to `/site/wwwroot/`
7. Delete old files in `/site/wwwroot/` first
8. Upload new files
9. Restart the API app

**OR use GitHub Actions (if set up):**
Just push your changes - it will auto-deploy:
```powershell
cd "c:\Users\terkp\OneDrive\Desktop\HEALTH_VAULT"
git push origin main
```

### STEP 4: Test Login (1 minute)

1. Go to: https://healthvault-web-app-hgd5bwc4e8e5dkf8.ukwest-01.azurewebsites.net
2. Open Browser DevTools (Press F12)
3. Go to **Console** tab
4. Try logging in with:
   - Email: `admin@healthvault.local`
   - Password: `Admin@123`
5. Watch for errors in the console

## Most Likely Issue

Based on typical scenarios, the issue is probably:

**Issue:** Web App is trying to connect to `localhost:5080` instead of your Azure API

**Why:** The `ApiBaseUrl` setting is not configured in Azure

**Fix:** Complete STEP 1 above

## Check if Fix Worked

After completing steps 1-3, test in browser console:

```javascript
// Test if API is reachable
fetch('https://healthvault-api-app-h3hkd3bcf4exh2bz.ukwest-01.azurewebsites.net/api/auth/login', {
  method: 'POST',
  headers: {'Content-Type': 'application/json'},
  body: JSON.stringify({email: 'admin@healthvault.local', password: 'Admin@123'})
})
.then(r => r.json())
.then(data => {
  if (data.succeeded) {
    console.log('✓ LOGIN WORKS!', data);
  } else {
    console.error('✗ Login failed:', data);
  }
})
.catch(err => console.error('✗ Network error:', err));
```

**Expected output:** `✓ LOGIN WORKS!` with a token

## Still Not Working?

### Check Browser Network Tab

1. F12 → Network tab
2. Try logging in
3. Look for the `/api/auth/login` request
4. Click on it
5. Check:
   - **Request URL** - Should be `https://healthvault-api-app...`, NOT `localhost`
   - **Status** - Should be `200`, not `404` or `500`
   - **Response** - What's the error message?

### Common Errors & Solutions

| Error in Console | Cause | Solution |
|-----------------|-------|----------|
| "Failed to fetch" | CORS or API offline | Redeploy API with CORS fix |
| "404 Not Found" | Wrong API URL | Add `ApiBaseUrl` in Web App config |
| "500 Internal Server Error" | Database issue | Check connection string in API config |
| "401 Unauthorized" | Wrong credentials | Try `admin@healthvault.local` / `Admin@123` |
| Request goes to localhost | Missing config | Add `ApiBaseUrl` setting |

### Check Azure Logs

1. Go to API App in Azure Portal
2. Click **Log stream**
3. Try logging in
4. Watch for errors
5. Common errors:
   - "Cannot open server" = DB connection issue
   - "Table does not exist" = Run migrations
   - "Invalid token" = JWT config issue

## Quick Checklist

After completing all steps, verify:

- [ ] `ApiBaseUrl` added to Web App configuration
- [ ] JWT settings added to API App configuration
- [ ] Connection string exists in API App
- [ ] Both apps restarted
- [ ] API redeployed with CORS changes
- [ ] Can access Swagger: https://healthvault-api-app-h3hkd3bcf4exh2bz.ukwest-01.azurewebsites.net/swagger
- [ ] Login test in browser console returns success

## Alternative: Deploy Both Apps Fresh

If nothing works, redeploy everything:

```powershell
# API
cd "c:\Users\terkp\OneDrive\Desktop\HEALTH_VAULT\HealthVault\src\HealthVault.Api"
dotnet publish -c Release

# Web
cd "c:\Users\terkp\OneDrive\Desktop\HEALTH_VAULT\HealthVault\src\HealthVault.Web"
dotnet publish -c Release
```

Then upload both to Azure.

## Need Help Now?

1. Take a screenshot of:
   - Browser DevTools → Console (F12)
   - Browser DevTools → Network tab showing the login request
   - Azure Portal → API App → Log Stream

2. Check these URLs:
   - API: https://healthvault-api-app-h3hkd3bcf4exh2bz.ukwest-01.azurewebsites.net/swagger
   - Web: https://healthvault-web-app-hgd5bwc4e8e5dkf8.ukwest-01.azurewebsites.net

Both should load without errors.

---

**START WITH STEP 1 - That's usually the fix!**
