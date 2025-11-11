# Complete Hosting Preparation Guide
## ASP.NET MVC + API to Render.com with Android App Integration

This comprehensive guide will walk you through preparing and deploying your ASP.NET MVC web application with API endpoints to Render.com and connecting it to your Android mobile app.

---

## Table of Contents

1. [Project Preparation](#1-project-preparation)
2. [Production Build Configuration](#2-production-build-configuration)
3. [Database Hosting Setup](#3-database-hosting-setup)
4. [Render.com Deployment](#4-rendercom-deployment)
5. [Android App Integration](#5-android-app-integration)
6. [Security Configuration](#6-security-configuration)
7. [Testing & Verification](#7-testing--verification)
8. [Troubleshooting](#8-troubleshooting)

---

## 1. Project Preparation

### 1.1 Verify Project Structure

Ensure your project has the following structure:
```
Molemi.app/
├── Controllers/
│   ├── HomeController.cs          # MVC controllers for web
│   ├── BuyerController.cs         # MVC controllers for web
│   ├── FarmerController.cs        # MVC controllers for web
│   └── Api/
│       ├── AuthController.cs      # API controllers for mobile
│       ├── BuyersController.cs    # API controllers for mobile
│       ├── FarmersController.cs   # API controllers for mobile
│       └── ProductsController.cs  # API controllers for mobile
├── Program.cs                     # Main configuration
├── appsettings.json               # Base configuration (no secrets)
├── appsettings.Production.json    # Production template (no secrets)
├── .env                           # Local development secrets (gitignored)
├── .env.example                   # Template for environment variables
└── .gitignore                     # Should exclude secrets
```

### 1.2 Remove Secrets from Source Code

**✅ Verify these files have NO secrets:**

1. **Check `appsettings.json`:**
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
     "Stripe": {
       "PublishableKey": "",
       "SecretKey": ""
     }
   }
   ```

2. **Check `appsettings.Production.json`:**
   - Should only contain placeholders like `YOUR_DB_HOST`, `CHANGE_THIS_TO_A_SECURE_RANDOM_KEY`

3. **Verify `.gitignore` includes:**
   ```
   .env
   .env.local
   .env.production
   appsettings.Production.json
   bin/
   obj/
   ```

### 1.3 Configure Environment Variables Support

Your `Program.cs` should already load `.env` files using `DotNetEnv`:

```csharp
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);
```

**✅ Verify this is in your `Program.cs`** (it should already be there).

### 1.4 Verify CORS Configuration

Your `Program.cs` should have CORS configured for mobile apps:

```csharp
// Add CORS for mobile app (Kotlin/Android) and web app
builder.Services.AddCors(options =>
{
    // Production policy - allows mobile app from any origin
    options.AddPolicy("AllowMobileApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    // Development policy
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// In middleware pipeline:
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("AllowMobileApp");
}
```

**✅ Verify CORS is configured correctly** (should already be in your `Program.cs`).

### 1.5 Test Local Build

Before deploying, test the production build locally:

```bash
# Clean previous builds
dotnet clean

# Publish for production
dotnet publish ReacodeApp.csproj -c Release -o ./publish

# Test the published build (optional, for local testing)
cd publish
dotnet ReacodeApp.dll
```

**Expected output:** Application should start without errors.

---

## 2. Production Build Configuration

### 2.1 Build Command for Render.com

**Build Command:**
```bash
dotnet publish ReacodeApp.csproj -c Release -o ./publish
```

**Alternative (if in project root directory):**
```bash
dotnet publish -c Release -o ./publish
```

This command:
- Compiles in Release mode (optimized)
- Outputs to `./publish` directory
- Includes all necessary dependencies
- Explicitly specifies the project file for reliability

### 2.2 Start Command for Render.com

**Start Command:**
```bash
dotnet ./publish/ReacodeApp.dll
```

**Important:** Render.com uses port `10000` by default. Your application should listen on `0.0.0.0:10000`.

### 2.3 Verify Port Configuration

Your `Program.cs` should handle the port from environment variables:

```csharp
// This is already handled by ASP.NET Core
// Set via environment variable: ASPNETCORE_URLS=http://0.0.0.0:10000
```

**✅ No code changes needed** - ASP.NET Core reads `ASPNETCORE_URLS` automatically.

### 2.4 Create `render.yaml` (Optional but Recommended)

Create a `render.yaml` file in your project root for Infrastructure as Code:

```yaml
services:
  - type: web
    name: reacoda-molemi-app
    env: dotnet
    buildCommand: dotnet publish ReacodeApp.csproj -c Release -o ./publish
    startCommand: dotnet ./publish/ReacodeApp.dll
    envVars:
      - key: ASPNETCORE_ENVIRONMENT
        value: Production
      - key: ASPNETCORE_URLS
        value: http://0.0.0.0:10000
    plan: free  # Change to 'starter' for production ($7/month)

databases:
  - name: reacoda-molemi-db
    databaseName: Reacoda_MolemiDB
    user: molemi_user
    plan: free  # Change to 'starter' for production ($7/month)
```

**Note:** You can also configure everything via Render Dashboard (recommended for first-time setup).

---

## 3. Database Hosting Setup

### 3.1 Create PostgreSQL Database on Render.com

1. **Go to Render Dashboard:** https://dashboard.render.com
2. **Click "New +" → "PostgreSQL"**
3. **Configure Database:**
   - **Name:** `reacoda-molemi-db` (or your preferred name)
   - **Database:** `Reacoda_MolemiDB`
   - **User:** (auto-generated, or specify custom)
   - **Region:** Choose closest to your users (e.g., `Oregon (US West)`)
   - **PostgreSQL Version:** 15 or 16 (latest stable)
   - **Plan:** 
     - **Free** (for testing - spins down after inactivity)
     - **Starter** ($7/month - always on, recommended for production)
4. **Click "Create Database"**

### 3.2 Get Database Connection String

After creating the database:

1. **Go to your PostgreSQL service** in Render Dashboard
2. **Click on "Connections" tab**
3. **Copy the "Internal Database URL"** (for Render services)
   - Format: `postgresql://user:password@host:port/database`
   - Example: `postgresql://molemi_user:abc123@dpg-xxxxx-a/reacoda_molemidb`

**⚠️ Important:** Use **Internal Database URL** for Render services, not External URL.

### 3.3 Convert to Connection String Format

The Internal Database URL needs to be converted to ASP.NET Core format:

**From Render URL:**
```
postgresql://user:password@host:port/database
```

**To ASP.NET Core format:**
```
Host=host;Database=database;Username=user;Password=password;Port=port
```

**Example:**
```
postgresql://molemi_user:abc123@dpg-xxxxx-a/reacoda_molemidb
```

**Becomes:**
```
Host=dpg-xxxxx-a;Database=reacoda_molemidb;Username=molemi_user;Password=abc123;Port=5432
```

**💡 Tip:** You can use this PowerShell script to convert:
```powershell
$renderUrl = "postgresql://user:password@host:port/database"
$parts = $renderUrl -replace "postgresql://", "" -split "@"
$credentials = $parts[0] -split ":"
$hostDb = $parts[1] -split "/"
$hostPort = $hostDb[0] -split ":"

$connectionString = "Host=$($hostPort[0]);Database=$($hostDb[1]);Username=$($credentials[0]);Password=$($credentials[1]);Port=$($hostPort[1])"
Write-Host $connectionString
```

---

## 4. Render.com Deployment

### 4.1 Prepare GitHub Repository

1. **Ensure all code is committed:**
   ```bash
   git status
   git add .
   git commit -m "Prepare for production deployment"
   ```

2. **Push to GitHub:**
   ```bash
   git push origin main
   ```

3. **Verify `.gitignore` excludes secrets:**
   - `.env` files
   - `appsettings.Production.json` (if it contains secrets)
   - `bin/` and `obj/` folders

### 4.2 Create Web Service on Render.com

1. **Go to Render Dashboard:** https://dashboard.render.com
2. **Click "New +" → "Web Service"**
3. **Connect Repository:**
   - Select your GitHub account
   - Choose the `Molemi.app` repository
   - Select branch: `main` (or your default branch)
   - Click "Connect"

4. **Configure Service:**
   - **Name:** `reacoda-molemi-app` (or your preferred name)
   - **Region:** Same region as your database (for lower latency)
   - **Root Directory:** Leave empty (or `./` if needed)
   - **Runtime:** `dotnet`
   - **Build Command:** `dotnet publish ReacodeApp.csproj -c Release -o ./publish`
   - **Start Command:** `dotnet ./publish/ReacodeApp.dll`
   - **Plan:** 
     - **Free** (for testing - spins down after 15 min inactivity)
     - **Starter** ($7/month - always on, recommended for production)

### 4.3 Configure Environment Variables

Click "Advanced" → "Environment Variables" and add:

#### Application Settings
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:10000
```

#### Database Connection
```
ConnectionStrings__DefaultConnection=Host=dpg-xxxxx-a;Database=reacoda_molemidb;Username=molemi_user;Password=abc123;Port=5432
```
*(Replace with your actual database connection string from Step 3.3)*

#### JWT Configuration
```
Jwt__Key=YOUR_SECURE_32_PLUS_CHARACTER_KEY_HERE
Jwt__Issuer=ReacodeApp
Jwt__Audience=ReacodeAppUsers
Jwt__ExpiryMinutes=1440
```

**Generate Secure JWT Key:**

**PowerShell:**
```powershell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

**Online Tool:** https://www.grc.com/passwords.htm (generate 64 random characters)

**Bash:**
```bash
openssl rand -base64 32
```

#### Stripe Keys (if using payment)
```
Stripe__PublishableKey=pk_live_YOUR_PUBLISHABLE_KEY
Stripe__SecretKey=sk_live_YOUR_SECRET_KEY
```

**⚠️ Important:** Use **production Stripe keys** (starting with `pk_live_` and `sk_live_`), not test keys.

### 4.4 Deploy the Service

1. **Click "Create Web Service"**
2. **Render will:**
   - Clone your repository
   - Run the build command
   - Start your application
   - Provide a URL (e.g., `https://reacoda-molemi-app.onrender.com`)

3. **Monitor the deployment:**
   - Go to "Logs" tab to see build and startup logs
   - Wait for "Your service is live" message

### 4.5 Run Database Migrations

After deployment, run Entity Framework migrations:

1. **Option 1: Using Render Shell**
   - Go to your Web Service → "Shell" tab
   - Run: `dotnet ef database update`
   - Wait for migrations to complete

2. **Option 2: Using One-Off Command**
   - Go to your Web Service
   - Click "Manual Deploy" → "Run Command"
   - Enter: `dotnet ef database update`
   - Click "Run"

**Expected output:** Migrations applied successfully.

### 4.6 Verify Deployment

1. **Check Application URL:**
   - Your app should be live at: `https://reacoda-molemi-app.onrender.com`
   - Web UI: `https://reacoda-molemi-app.onrender.com`
   - API Base: `https://reacoda-molemi-app.onrender.com/api`
   - Swagger: `https://reacoda-molemi-app.onrender.com/swagger`

2. **Test API Endpoint:**
   ```bash
   curl https://reacoda-molemi-app.onrender.com/api/products
   ```

3. **Test Swagger UI:**
   - Open: `https://reacoda-molemi-app.onrender.com/swagger`
   - Should show all API endpoints

---

## 5. Android App Integration

### 5.1 Update API Base URL

**In your Android/Kotlin project:**

**Option 1: Using BuildConfig (Recommended)**

1. **Create `ApiConfig.kt`:**
   ```kotlin
   object ApiConfig {
       // Development (for emulator)
       const val BASE_URL_DEV = "http://10.0.2.2:5000/api/"
       
       // Production (your Render URL)
       const val BASE_URL_PROD = "https://reacoda-molemi-app.onrender.com/api/"
       
       // Switch based on build type
       const val BASE_URL = if (BuildConfig.DEBUG) {
           BASE_URL_DEV
       } else {
           BASE_URL_PROD
       }
   }
   ```

2. **Or use `strings.xml` for different build variants:**
   
   **`res/values/strings.xml` (production):**
   ```xml
   <string name="api_base_url">https://reacoda-molemi-app.onrender.com/api/</string>
   ```
   
   **`res/values/strings.xml` (debug):**
   ```xml
   <string name="api_base_url">http://10.0.2.2:5000/api/</string>
   ```

### 5.2 Configure Retrofit

**Example Retrofit Setup:**

```kotlin
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import okhttp3.OkHttpClient
import okhttp3.Interceptor
import okhttp3.Response
import okhttp3.logging.HttpLoggingInterceptor

// JWT Token Manager
class TokenManager {
    private val prefs = context.getSharedPreferences("auth_prefs", Context.MODE_PRIVATE)
    
    fun saveToken(token: String) {
        prefs.edit().putString("jwt_token", token).apply()
    }
    
    fun getToken(): String? {
        return prefs.getString("jwt_token", null)
    }
    
    fun clearToken() {
        prefs.edit().remove("jwt_token").apply()
    }
}

// Auth Interceptor
class AuthInterceptor(private val tokenManager: TokenManager) : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val token = tokenManager.getToken()
        val requestBuilder = chain.request().newBuilder()
            .addHeader("Content-Type", "application/json")
        
        token?.let {
            requestBuilder.addHeader("Authorization", "Bearer $it")
        }
        
        return chain.proceed(requestBuilder.build())
    }
}

// Retrofit Client
object RetrofitClient {
    private val tokenManager = TokenManager()
    
    private val loggingInterceptor = HttpLoggingInterceptor().apply {
        level = if (BuildConfig.DEBUG) {
            HttpLoggingInterceptor.Level.BODY
        } else {
            HttpLoggingInterceptor.Level.NONE
        }
    }
    
    private val okHttpClient = OkHttpClient.Builder()
        .addInterceptor(AuthInterceptor(tokenManager))
        .addInterceptor(loggingInterceptor)
        .build()
    
    val retrofit: Retrofit = Retrofit.Builder()
        .baseUrl(ApiConfig.BASE_URL)
        .client(okHttpClient)
        .addConverterFactory(GsonConverterFactory.create())
        .build()
    
    val apiService: ApiService = retrofit.create(ApiService::class.java)
}
```

### 5.3 Create API Service Interface

```kotlin
import retrofit2.http.*

interface ApiService {
    // Authentication
    @POST("auth/register")
    suspend fun register(@Body request: RegisterRequest): Response<AuthResponse>
    
    @POST("auth/login")
    suspend fun login(@Body request: LoginRequest): Response<AuthResponse>
    
    // Products (Public endpoints)
    @GET("products")
    suspend fun getProducts(): Response<List<ProductDto>>
    
    @GET("products/{id}")
    suspend fun getProduct(@Path("id") id: Int): Response<ProductDto>
    
    // Buyer endpoints (require authentication)
    @GET("buyers/products")
    suspend fun getBuyerProducts(
        @Query("searchTerm") searchTerm: String? = null,
        @Query("categoryId") categoryId: Int? = null,
        @Query("minPrice") minPrice: Double? = null,
        @Query("maxPrice") maxPrice: Double? = null,
        @Query("sortBy") sortBy: String? = null
    ): Response<List<ProductDto>>
    
    @POST("buyers/cart")
    suspend fun addToCart(@Body request: AddToCartRequest): Response<CartResponseDto>
    
    @GET("buyers/cart")
    suspend fun getCart(): Response<CartDto>
    
    @POST("buyers/checkout")
    suspend fun checkout(@Body request: CheckoutRequest): Response<OrderDto>
    
    @GET("buyers/orders")
    suspend fun getOrders(): Response<List<OrderDto>>
    
    // Farmer endpoints (require authentication)
    @GET("farmers/dashboard")
    suspend fun getFarmerDashboard(): Response<FarmerDashboardDto>
    
    @GET("farmers/products")
    suspend fun getFarmerProducts(): Response<List<ProductDto>>
    
    @POST("farmers/products")
    suspend fun createProduct(@Body request: CreateProductRequest): Response<ProductDto>
    
    @GET("farmers/orders")
    suspend fun getFarmerOrders(): Response<List<OrderDto>>
}
```

### 5.4 Test API Calls

**1. Test Registration:**
```kotlin
suspend fun testRegistration() {
    try {
        val request = RegisterRequest(
            email = "test@example.com",
            password = "SecurePass123!",
            firstName = "John",
            lastName = "Doe",
            role = "Buyer"
        )
        
        val response = RetrofitClient.apiService.register(request)
        if (response.isSuccessful) {
            val authResponse = response.body()
            authResponse?.token?.let { token ->
                TokenManager().saveToken(token)
                Log.d("API", "Registration successful, token saved")
            }
        } else {
            Log.e("API", "Registration failed: ${response.message()}")
        }
    } catch (e: Exception) {
        Log.e("API", "Error: ${e.message}")
    }
}
```

**2. Test Login:**
```kotlin
suspend fun testLogin() {
    try {
        val request = LoginRequest(
            email = "test@example.com",
            password = "SecurePass123!"
        )
        
        val response = RetrofitClient.apiService.login(request)
        if (response.isSuccessful) {
            val authResponse = response.body()
            authResponse?.token?.let { token ->
                TokenManager().saveToken(token)
                Log.d("API", "Login successful, token saved")
            }
        }
    } catch (e: Exception) {
        Log.e("API", "Error: ${e.message}")
    }
}
```

**3. Test Get Products:**
```kotlin
suspend fun testGetProducts() {
    try {
        val response = RetrofitClient.apiService.getProducts()
        if (response.isSuccessful) {
            val products = response.body()
            Log.d("API", "Products count: ${products?.size}")
        }
    } catch (e: Exception) {
        Log.e("API", "Error: ${e.message}")
    }
}
```

### 5.5 Handle Authentication Errors

```kotlin
class ApiErrorHandler {
    fun handleError(response: Response<*>): String {
        return when (response.code()) {
            401 -> {
                // Token expired or invalid
                TokenManager().clearToken()
                "Session expired. Please login again."
            }
            403 -> "You don't have permission to access this resource."
            404 -> "Resource not found."
            500 -> "Server error. Please try again later."
            else -> "An error occurred: ${response.message()}"
        }
    }
}
```

---

## 6. Security Configuration

### 6.1 Environment Variables Best Practices

✅ **DO:**
- Store all secrets in Render environment variables
- Use strong, randomly generated keys
- Rotate keys periodically
- Use different keys for development and production

❌ **DON'T:**
- Commit secrets to Git
- Share secrets in chat/email
- Use weak or predictable keys
- Reuse keys across environments

### 6.2 JWT Token Security

**JWT Configuration:**
- **Key Length:** Minimum 32 characters (recommended: 64+)
- **Algorithm:** HS256 (symmetric)
- **Expiry:** 1440 minutes (24 hours) - adjust as needed
- **Storage:** Store securely in mobile app (EncryptedSharedPreferences)

**Mobile App Token Storage:**
```kotlin
// Use EncryptedSharedPreferences for secure storage
val masterKey = MasterKey.Builder(context)
    .setKeyScheme(MasterKey.KeyScheme.AES256_GCM)
    .build()

val encryptedPrefs = EncryptedSharedPreferences.create(
    context,
    "auth_prefs",
    masterKey,
    EncryptedSharedPreferences.PrefKeyEncryptionScheme.AES256_SIV,
    EncryptedSharedPreferences.PrefValueEncryptionScheme.AES256_GCM
)
```

### 6.3 HTTPS Configuration

✅ **Render.com automatically provides:**
- SSL/TLS certificates (Let's Encrypt)
- HTTPS enabled by default
- Certificate auto-renewal

**Always use HTTPS in production:**
- API calls: `https://your-app.onrender.com/api`
- Never use `http://` in production

### 6.4 CORS Security

Your CORS is already configured for mobile apps:

```csharp
// Production: Allows mobile apps from any origin
options.AddPolicy("AllowMobileApp", policy =>
{
    policy.AllowAnyOrigin()  // Safe for mobile apps (they use Bearer tokens)
          .AllowAnyMethod()
          .AllowAnyHeader();
});
```

**✅ This is correct** - Mobile apps don't have same-origin restrictions like browsers.

### 6.5 Database Security

✅ **Best Practices:**
- Use Internal Database URL (not external) for Render services
- Never expose database credentials
- Use strong database passwords
- Enable SSL connections (Render does this automatically)
- Regular backups (Starter plan includes this)

---

## 7. Testing & Verification

### 7.1 Test Web Application

1. **Open in browser:**
   ```
   https://reacoda-molemi-app.onrender.com
   ```

2. **Verify:**
   - Home page loads
   - Login/Register pages work
   - Navigation works
   - No console errors

### 7.2 Test API Endpoints

**Using cURL:**

1. **Test Public Endpoint (Products):**
   ```bash
   curl https://reacoda-molemi-app.onrender.com/api/products
   ```

2. **Test Registration:**
   ```bash
   curl -X POST https://reacoda-molemi-app.onrender.com/api/auth/register \
     -H "Content-Type: application/json" \
     -d '{
       "email": "test@example.com",
       "password": "SecurePass123!",
       "firstName": "John",
       "lastName": "Doe",
       "role": "Buyer"
     }'
   ```

3. **Test Login:**
   ```bash
   curl -X POST https://reacoda-molemi-app.onrender.com/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{
       "email": "test@example.com",
       "password": "SecurePass123!"
     }'
   ```

4. **Test Authenticated Endpoint (with token):**
   ```bash
   curl https://reacoda-molemi-app.onrender.com/api/buyers/cart \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"
   ```

### 7.3 Test Swagger UI

1. **Open Swagger:**
   ```
   https://reacoda-molemi-app.onrender.com/swagger
   ```

2. **Test endpoints:**
   - Click "Authorize" button
   - Enter JWT token (from login response)
   - Test various endpoints

### 7.4 Test Android App Connection

1. **Update API URL** in Android app
2. **Run app on emulator/device**
3. **Test flows:**
   - ✅ Registration
   - ✅ Login
   - ✅ Fetch products
   - ✅ Add to cart
   - ✅ Place order
   - ✅ View orders

### 7.5 Post-Deployment Checklist

- [ ] Database created and running
- [ ] Web service deployed successfully
- [ ] All environment variables set correctly
- [ ] Database migrations run successfully
- [ ] Web UI accessible at root URL
- [ ] API accessible at `/api` endpoints
- [ ] Swagger UI accessible at `/swagger`
- [ ] Test registration endpoint works
- [ ] Test login endpoint works
- [ ] Test products endpoint works
- [ ] JWT token authentication works
- [ ] Android app can connect to API
- [ ] HTTPS working (SSL certificate active)
- [ ] No secrets in source code
- [ ] CORS configured correctly

---

## 8. Troubleshooting

### 8.1 Build Failures

**Issue:** Build command fails

**Solutions:**
- Check build logs in Render Dashboard
- Verify .NET SDK version (should be 8.0)
- Ensure all NuGet packages are restored
- Check for compilation errors in logs

### 8.2 Application Won't Start

**Issue:** 502 Bad Gateway or application not starting

**Solutions:**
- Verify start command: `dotnet ./publish/ReacodeApp.dll`
- Check `ASPNETCORE_URLS=http://0.0.0.0:10000` is set
- Review application logs in Render Dashboard
- Ensure port 10000 is used (Render's default)

### 8.3 Database Connection Errors

**Issue:** Cannot connect to database

**Solutions:**
- Verify using **Internal Database URL** (not external)
- Check environment variable name: `ConnectionStrings__DefaultConnection`
- Ensure database service is running
- Verify connection string format is correct
- Check database credentials are correct

### 8.4 CORS Errors from Mobile App

**Issue:** CORS errors when calling API from Android app

**Solutions:**
- Verify using HTTPS (not HTTP)
- Check CORS policy is active in production
- Ensure `AllowMobileApp` policy is used in production
- Mobile apps shouldn't have CORS issues - check if using correct URL

### 8.5 JWT Token Issues

**Issue:** 401 Unauthorized errors

**Solutions:**
- Verify JWT key is set correctly in environment variables
- Check token is being sent in Authorization header: `Bearer {token}`
- Verify token hasn't expired (check expiry time)
- Ensure token format is correct (no extra spaces)

### 8.6 Migration Errors

**Issue:** Database migrations fail

**Solutions:**
- Run migrations manually via Render Shell
- Check database connection is working
- Verify Entity Framework tools are available
- Review migration logs for specific errors

### 8.7 Slow Response Times

**Issue:** API responses are slow

**Solutions:**
- Free tier spins down after inactivity - use Starter plan for always-on
- Check database and web service are in same region
- Review application logs for performance issues
- Consider database connection pooling

---

## 9. Continuous Deployment

### 9.1 Automatic Deployments

Render automatically deploys when you push to your connected branch:

1. **Make changes to code**
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

### 9.2 Manual Deployments

To deploy a specific commit:

1. Go to Render Dashboard → Your Service
2. Click "Manual Deploy"
3. Select branch and commit
4. Click "Deploy"

---

## 10. Production Checklist

Before going live:

- [ ] All secrets moved to environment variables
- [ ] Production database created (Starter plan recommended)
- [ ] Strong JWT key generated and set
- [ ] Production Stripe keys configured (if using)
- [ ] HTTPS enabled and working
- [ ] Database migrations run
- [ ] All API endpoints tested
- [ ] Android app tested with production URL
- [ ] Error handling implemented
- [ ] Logging configured
- [ ] Monitoring set up (optional)
- [ ] Backup strategy in place

---

## 11. Useful Commands

### Local Testing

```bash
# Clean build
dotnet clean

# Publish locally
dotnet publish ReacodeApp.csproj -c Release -o ./publish

# Test published build
cd publish
dotnet ReacodeApp.dll

# Run migrations locally (if needed)
dotnet ef database update
```

### Render Shell Commands

```bash
# Check .NET version
dotnet --version

# List files
ls -la

# Check environment variables
env | grep ASPNETCORE

# Run migrations
dotnet ef database update

# Check application logs
# (View in Render Dashboard → Logs tab)
```

---

## 12. Support & Resources

### Render.com Resources
- **Documentation:** https://render.com/docs
- **Support:** support@render.com
- **Status Page:** https://status.render.com
- **Community:** https://community.render.com

### Your Application URLs
After deployment, you'll have:

- **Web Application:** `https://reacoda-molemi-app.onrender.com`
- **API Base URL:** `https://reacoda-molemi-app.onrender.com/api`
- **Swagger UI:** `https://reacoda-molemi-app.onrender.com/swagger`

### Quick Test Commands

```bash
# Test API health
curl https://reacoda-molemi-app.onrender.com/api/products

# Test registration
curl -X POST https://reacoda-molemi-app.onrender.com/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!","firstName":"Test","lastName":"User","role":"Buyer"}'
```

---

## Summary

You now have:
- ✅ Production-ready ASP.NET MVC + API application
- ✅ PostgreSQL database hosted on Render.com
- ✅ Web service deployed and accessible
- ✅ Android app configured to use production API
- ✅ Secure authentication with JWT tokens
- ✅ All secrets stored in environment variables

**Your API is live and ready for your Android app!** 🎉

---

**Last Updated:** 2024-01-15  
**Version:** 1.0

