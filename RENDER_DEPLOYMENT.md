# Deploy ReacodeApp to Render

This guide will walk you through deploying your ReacodeApp to Render and connecting it with your Kotlin mobile app.

## 🚀 Prerequisites

1. **GitHub/GitLab/Bitbucket account** (for code repository)
2. **Render account** (sign up at https://render.com - free tier available)
3. **PostgreSQL database** (Render provides this)

---

## 📋 Step-by-Step Deployment

### Step 1: Prepare Your Code

1. **Push your code to GitHub:**
   ```bash
   git add .
   git commit -m "Prepare for Render deployment"
   git push origin main
   ```

2. **Create `.gitignore` if you don't have one:**
   Make sure these are ignored:
   ```
   bin/
   obj/
   *.user
   appsettings.Production.json
   ```

---

### Step 2: Create PostgreSQL Database on Render

1. **Go to Render Dashboard:** https://dashboard.render.com
2. **Click "New +" → "PostgreSQL"**
3. **Configure:**
   - **Name:** `reacoda-molemi-db`
   - **Database:** `Reacoda_MolemiDB`
   - **User:** (auto-generated)
   - **Region:** Choose closest to your users
   - **PostgreSQL Version:** 15 or 16
   - **Plan:** Free (for testing) or Starter ($7/month for production)
4. **Click "Create Database"**
5. **Save the connection details:**
   - Internal Database URL (for Render services)
   - External Connection String (for local access if needed)

---

### Step 3: Create Web Service on Render

1. **In Render Dashboard, click "New +" → "Web Service"**
2. **Connect your repository:**
   - Select your GitHub/GitLab account
   - Choose the `Molemi.app` repository
   - Click "Connect"

3. **Configure the service:**
   - **Name:** `reacoda-molemi-app` (or your preferred name)
   - **Region:** Same as your database
   - **Branch:** `main` (or your default branch)
   - **Root Directory:** Leave empty (or `./` if needed)
   - **Runtime:** `dotnet`
   - **Build Command:** `dotnet publish -c Release -o ./publish`
   - **Start Command:** `dotnet ./publish/ReacodeApp.dll`
   - **Plan:** Free (for testing) or Starter ($7/month for production)

4. **Environment Variables:**
   Click "Advanced" and add these environment variables:

   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://0.0.0.0:10000
   ```

   **Database Connection:**
   ```
   ConnectionStrings__DefaultConnection=<Your PostgreSQL Internal Database URL>
   ```
   (Get this from your PostgreSQL service → "Connections" → "Internal Database URL")

   **JWT Configuration:**
   ```
   Jwt__Key=<Generate a secure 32+ character key>
   Jwt__Issuer=ReacodeApp
   Jwt__Audience=ReacodeAppUsers
   Jwt__ExpiryMinutes=1440
   ```

   **Stripe (if using):**
   ```
   Stripe__PublishableKey=<Your Stripe Publishable Key>
   Stripe__SecretKey=<Your Stripe Secret Key>
   ```

5. **Click "Create Web Service"**

---

### Step 4: Generate Secure JWT Key

Run this PowerShell command to generate a secure key:
```powershell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

Or use this online tool: https://www.grc.com/passwords.htm (generate 64 random characters)

Copy the generated key and paste it as the `Jwt__Key` environment variable.

---

### Step 5: Run Database Migrations

After your web service is deployed:

1. **Go to your Web Service → "Shell" tab**
2. **Run migration command:**
   ```bash
   dotnet ef database update
   ```

   **OR** use Render's one-off command:
   - Go to your Web Service
   - Click "Manual Deploy" → "Run Command"
   - Enter: `dotnet ef database update`
   - Click "Run"

---

### Step 6: Get Your API URL

After deployment, Render will provide you with a URL like:
```
https://reacoda-molemi-app.onrender.com
```

Your API endpoints will be at:
```
https://reacoda-molemi-app.onrender.com/api
```

Swagger documentation:
```
https://reacoda-molemi-app.onrender.com/swagger
```

---

## 🔧 Render-Specific Configuration

### Update appsettings.json for Render

Render uses environment variables, but you can also update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Jwt": {
    "Key": "",
    "Issuer": "ReacodeApp",
    "Audience": "ReacodeAppUsers",
    "ExpiryMinutes": 1440
  },
  "AllowedHosts": "*"
}
```

**Note:** Leave connection strings empty in code - use environment variables instead.

---

## 📱 Kotlin Mobile App Configuration

### Update Base URL in Kotlin App

In your Kotlin app, update the API base URL:

```kotlin
object ApiConfig {
    const val BASE_URL = "https://reacoda-molemi-app.onrender.com/api"
    // Or use BuildConfig for different environments
    // const val BASE_URL = BuildConfig.API_BASE_URL
}
```

### Example API Calls

**1. Register:**
```kotlin
val client = OkHttpClient()
val json = JSONObject().apply {
    put("email", "user@example.com")
    put("password", "SecurePassword123!")
    put("firstName", "John")
    put("lastName", "Doe")
    put("role", "Buyer")
}

val request = Request.Builder()
    .url("${ApiConfig.BASE_URL}/auth/register")
    .post(json.toString().toRequestBody("application/json".toMediaType()))
    .build()

val response = client.newCall(request).execute()
```

**2. Login:**
```kotlin
val json = JSONObject().apply {
    put("email", "user@example.com")
    put("password", "SecurePassword123!")
}

val request = Request.Builder()
    .url("${ApiConfig.BASE_URL}/auth/login")
    .post(json.toString().toRequestBody("application/json".toMediaType()))
    .build()

val response = client.newCall(request).execute()
val responseBody = response.body?.string()
val token = JSONObject(responseBody).getString("token")
```

**3. Get Products:**
```kotlin
val request = Request.Builder()
    .url("${ApiConfig.BASE_URL}/products")
    .addHeader("Authorization", "Bearer $token")
    .get()
    .build()

val response = client.newCall(request).execute()
```

---

## 🔒 Security Best Practices

1. **Never commit secrets to Git:**
   - Use environment variables for all sensitive data
   - Add `appsettings.Production.json` to `.gitignore`

2. **Use HTTPS:**
   - Render automatically provides SSL certificates
   - Always use `https://` in your API calls

3. **JWT Key:**
   - Generate a strong, random key
   - Store it only in Render environment variables
   - Never expose it in code or logs

4. **Database:**
   - Use Internal Database URL (not external) for Render services
   - External URL is only for local development/testing

---

## 🐛 Troubleshooting

### Issue: Database connection fails

**Solution:**
- Verify you're using the **Internal Database URL** (not external)
- Check that environment variable name is exactly: `ConnectionStrings__DefaultConnection`
- Ensure database service is running

### Issue: Application won't start

**Solution:**
- Check build logs in Render dashboard
- Verify build command: `dotnet publish -c Release -o ./publish`
- Verify start command: `dotnet ./publish/ReacodeApp.dll`
- Check that `ASPNETCORE_URLS=http://0.0.0.0:10000` is set

### Issue: 502 Bad Gateway

**Solution:**
- Check application logs
- Verify port is set to `10000` (Render's default)
- Ensure `ASPNETCORE_URLS` environment variable is set correctly

### Issue: CORS errors from mobile app

**Solution:**
- CORS is already configured to allow mobile apps
- Verify you're using HTTPS in API calls
- Check that `AllowMobileApp` policy is active (it is by default in production)

### Issue: Migrations not running

**Solution:**
- Run migrations manually via Render Shell
- Or add migration to build process (not recommended for production)

---

## 📊 Render Plans

### Free Tier (Good for Testing)
- **Web Service:** Spins down after 15 minutes of inactivity
- **Database:** 90-day retention, 1GB storage
- **Perfect for:** Development and testing

### Starter Plan ($7/month)
- **Web Service:** Always on, no spin-down
- **Database:** Persistent, 1GB storage
- **Perfect for:** Production use

### Professional Plan ($25/month)
- **Web Service:** Always on, better performance
- **Database:** 10GB storage, automated backups
- **Perfect for:** High-traffic production

---

## 🔄 Continuous Deployment

Render automatically deploys when you push to your connected branch:

1. **Make changes to your code**
2. **Commit and push:**
   ```bash
   git add .
   git commit -m "Your changes"
   git push origin main
   ```
3. **Render automatically:**
   - Detects the push
   - Builds your application
   - Deploys the new version
   - Shows deployment status in dashboard

---

## 📝 Environment Variables Reference

Here's a complete list of environment variables you should set in Render:

```
# Application
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:10000

# Database
ConnectionStrings__DefaultConnection=<Internal Database URL from Render>

# JWT
Jwt__Key=<Your secure 32+ character key>
Jwt__Issuer=ReacodeApp
Jwt__Audience=ReacodeAppUsers
Jwt__ExpiryMinutes=1440

# Stripe (if using)
Stripe__PublishableKey=<Your key>
Stripe__SecretKey=<Your key>
```

---

## ✅ Post-Deployment Checklist

- [ ] Database created and running
- [ ] Web service deployed successfully
- [ ] Environment variables set
- [ ] Database migrations run
- [ ] API accessible at `/api` endpoints
- [ ] Swagger UI accessible at `/swagger`
- [ ] Test login endpoint works
- [ ] Test products endpoint works
- [ ] Kotlin app base URL updated
- [ ] HTTPS working (SSL certificate active)

---

## 🎉 You're Done!

Your API is now live and ready to use with your Kotlin mobile app!

**API Base URL:** `https://your-app-name.onrender.com/api`

**Test it:**
```bash
curl https://your-app-name.onrender.com/api/products
```

---

## 📞 Need Help?

- **Render Docs:** https://render.com/docs
- **Render Support:** support@render.com
- **Check Logs:** Render Dashboard → Your Service → Logs




