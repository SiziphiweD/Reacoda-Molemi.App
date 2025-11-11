# Deployment Guide - ReacodeApp

This guide will help you deploy the ReacodeApp web API to share with your Kotlin mobile app.

## 🚀 Hosting Options

### Option 1: Azure App Service (Recommended)
- **Easy deployment** from Visual Studio or GitHub
- **Built-in PostgreSQL** support
- **Auto-scaling** and SSL certificates
- **Free tier available** for testing

### Option 2: AWS Elastic Beanstalk
- **Simple deployment** for .NET applications
- **RDS PostgreSQL** for database
- **Load balancing** included

### Option 3: DigitalOcean / Linode / VPS
- **Full control** over the server
- **Cost-effective** for small to medium apps
- **Requires manual setup** of Nginx, PostgreSQL, etc.

### Option 4: Railway / Render / Fly.io
- **Easy deployment** with Git push
- **PostgreSQL included**
- **Good for startups**

---

## 📋 Pre-Deployment Checklist

### 1. Update Production Configuration

Edit `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-production-db-host;Database=Reacoda_MolemiDB;Username=your_user;Password=your_secure_password;Port=5432"
  },
  "Jwt": {
    "Key": "GENERATE_A_SECURE_32_CHARACTER_KEY_HERE",
    "Issuer": "ReacodeApp",
    "Audience": "ReacodeAppUsers",
    "ExpiryMinutes": 1440
  }
}
```

### 2. Generate Secure JWT Key

Run this PowerShell command to generate a secure key:
```powershell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

Replace `CHANGE_THIS_TO_A_SECURE_RANDOM_KEY` in `appsettings.Production.json` with the generated key.

### 3. Database Setup

**Option A: Use the same PostgreSQL database**
- Your web app and Kotlin app will connect to the same database
- Ensure the database is accessible from both applications
- Use connection pooling for better performance

**Option B: Use separate databases (not recommended)**
- Requires data synchronization
- More complex to maintain

### 4. Environment Variables (Recommended for Production)

Instead of storing secrets in `appsettings.Production.json`, use environment variables:

**Azure App Service:**
- Go to Configuration → Application Settings
- Add:
  - `ConnectionStrings__DefaultConnection`
  - `Jwt__Key`
  - `Jwt__Issuer`
  - `Jwt__Audience`
  - `Stripe__PublishableKey`
  - `Stripe__SecretKey`

**Linux/VPS:**
```bash
export ConnectionStrings__DefaultConnection="Host=..."
export Jwt__Key="your-secure-key"
```

---

## 🔧 Deployment Steps

### For Azure App Service:

1. **Create App Service:**
   ```bash
   az webapp create --resource-group MyResourceGroup --plan MyAppServicePlan --name MyAppName --runtime "DOTNET|8.0"
   ```

2. **Deploy:**
   ```bash
   dotnet publish -c Release
   az webapp deploy --resource-group MyResourceGroup --name MyAppName --src-path ./bin/Release/net8.0/publish
   ```

3. **Set Environment Variables:**
   - Azure Portal → Your App → Configuration → Application Settings

4. **Enable HTTPS:**
   - Azure automatically provides SSL certificates

### For Linux VPS (Ubuntu/Debian):

1. **Install .NET 8 Runtime:**
   ```bash
   wget https://dot.net/v1/dotnet-install.sh
   chmod +x dotnet-install.sh
   ./dotnet-install.sh --channel 8.0
   ```

2. **Install Nginx:**
   ```bash
   sudo apt update
   sudo apt install nginx
   ```

3. **Create Systemd Service:**
   Create `/etc/systemd/system/reacodeapp.service`:
   ```ini
   [Unit]
   Description=ReacodeApp
   After=network.target

   [Service]
   Type=notify
   ExecStart=/usr/bin/dotnet /var/www/reacodeapp/ReacodeApp.dll
   Restart=always
   RestartSec=10
   Environment=ASPNETCORE_ENVIRONMENT=Production
   Environment=ASPNETCORE_URLS=http://localhost:5000

   [Install]
   WantedBy=multi-user.target
   ```

4. **Configure Nginx:**
   Create `/etc/nginx/sites-available/reacodeapp`:
   ```nginx
   server {
       listen 80;
       server_name your-domain.com;

       location / {
           proxy_pass http://localhost:5000;
           proxy_http_version 1.1;
           proxy_set_header Upgrade $http_upgrade;
           proxy_set_header Connection keep-alive;
           proxy_set_header Host $host;
           proxy_cache_bypass $http_upgrade;
           proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
           proxy_set_header X-Forwarded-Proto $scheme;
       }
   }
   ```

5. **Enable and Start:**
   ```bash
   sudo ln -s /etc/nginx/sites-available/reacodeapp /etc/nginx/sites-enabled/
   sudo nginx -t
   sudo systemctl restart nginx
   sudo systemctl enable reacodeapp
   sudo systemctl start reacodeapp
   ```

---

## 📱 Kotlin Mobile App Configuration

### API Base URL

After deployment, your API will be available at:
- **Production:** `https://your-domain.com/api`
- **Development:** `http://localhost:5000/api` (for testing)

### Example API Calls in Kotlin

**1. Register User:**
```kotlin
val client = OkHttpClient()
val json = JSONObject().apply {
    put("email", "user@example.com")
    put("password", "SecurePassword123!")
    put("firstName", "John")
    put("lastName", "Doe")
    put("role", "Buyer")
}

val requestBody = json.toString().toRequestBody("application/json".toMediaType())
val request = Request.Builder()
    .url("https://your-domain.com/api/auth/register")
    .post(requestBody)
    .build()

val response = client.newCall(request).execute()
```

**2. Login:**
```kotlin
val json = JSONObject().apply {
    put("email", "user@example.com")
    put("password", "SecurePassword123!")
}

val requestBody = json.toString().toRequestBody("application/json".toMediaType())
val request = Request.Builder()
    .url("https://your-domain.com/api/auth/login")
    .post(requestBody)
    .build()

val response = client.newCall(request).execute()
val responseBody = response.body?.string()
// Parse JSON to get token
val token = JSONObject(responseBody).getString("token")
```

**3. Get Products (with Auth):**
```kotlin
val request = Request.Builder()
    .url("https://your-domain.com/api/products")
    .addHeader("Authorization", "Bearer $token")
    .get()
    .build()

val response = client.newCall(request).execute()
```

---

## 🔒 Security Considerations

1. **HTTPS Only:** Always use HTTPS in production
2. **JWT Key:** Keep your JWT key secret and never commit it to Git
3. **Database:** Use strong passwords and restrict access
4. **CORS:** Already configured to allow mobile apps
5. **Rate Limiting:** Consider adding rate limiting for API endpoints

---

## 🧪 Testing After Deployment

1. **Test API Endpoints:**
   ```bash
   # Register
   curl -X POST "https://your-domain.com/api/auth/register" \
     -H "Content-Type: application/json" \
     -d '{"email":"test@example.com","password":"Test123!","firstName":"Test","lastName":"User","role":"Buyer"}'

   # Login
   curl -X POST "https://your-domain.com/api/auth/login" \
     -H "Content-Type: application/json" \
     -d '{"email":"test@example.com","password":"Test123!"}'

   # Get Products
   curl -X GET "https://your-domain.com/api/products"
   ```

2. **Test Swagger UI:**
   - Visit: `https://your-domain.com/swagger`

3. **Test from Kotlin App:**
   - Update base URL in your Kotlin app
   - Test login and API calls

---

## 📊 Database Connection for Kotlin

Your Kotlin app can connect to the same PostgreSQL database:

**Option 1: Direct Database Connection (Not Recommended)**
- Expose PostgreSQL port (5432) to internet
- Security risk - use VPN or private network

**Option 2: Use API Only (Recommended)**
- Kotlin app only calls the REST API
- Web app handles all database operations
- More secure and maintainable

---

## 🐛 Troubleshooting

### CORS Issues:
- Ensure `AllowMobileApp` policy is active in production
- Check that CORS middleware is before authentication

### Database Connection Issues:
- Verify connection string format
- Check firewall rules
- Ensure PostgreSQL allows remote connections

### JWT Token Issues:
- Verify JWT key matches in all environments
- Check token expiry time
- Ensure `Bearer` prefix in Authorization header

---

## 📞 Support

For issues or questions:
1. Check application logs
2. Review Swagger documentation at `/swagger`
3. Test API endpoints with curl or Postman




