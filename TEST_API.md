# Testing the API

## Quick Test Guide

### 1. Start the Application

Make sure your application is running:
```bash
dotnet run
```

The API will be available at:
- **HTTP:** `http://localhost:5000/api`
- **HTTPS:** `https://localhost:7000/api`
- **Swagger UI:** `http://localhost:5000/swagger` or `https://localhost:7000/swagger`

---

## Test API Endpoints

### Test 1: Get All Products (Public - No Auth Required)

**Using curl:**
```bash
curl -X GET "http://localhost:5000/api/products" -H "Content-Type: application/json"
```

**Using PowerShell:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/products" -Method Get -ContentType "application/json"
```

**Expected Response:**
```json
[
  {
    "id": 1,
    "name": "Fresh Organic Tomatoes",
    "description": "Premium organic tomatoes...",
    "pricePerKg": 45.00,
    "availableQuantity": 50,
    ...
  }
]
```

---

### Test 2: Get Single Product (Public - No Auth Required)

```bash
curl -X GET "http://localhost:5000/api/products/1" -H "Content-Type: application/json"
```

---

### Test 3: Register New User

```bash
curl -X POST "http://localhost:5000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "TestPassword123!",
    "firstName": "Test",
    "lastName": "User",
    "role": "Buyer"
  }'
```

**Expected Response:**
```json
{
  "message": "User registered successfully",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "testuser@example.com",
    "firstName": "Test",
    "lastName": "User",
    "role": "Buyer"
  }
}
```

**Save the token** from the response for the next test!

---

### Test 4: Login

```bash
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "TestPassword123!"
  }'
```

**Expected Response:**
```json
{
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "testuser@example.com",
    "firstName": "Test",
    "lastName": "User",
    "role": "Buyer"
  }
}
```

---

### Test 5: Create Product (Requires Auth - Farmer Only)

Replace `YOUR_TOKEN_HERE` with the token from login/register:

```bash
curl -X POST "http://localhost:5000/api/products" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "name": "Fresh Carrots",
    "description": "Sweet, crunchy carrots",
    "pricePerKg": 25.00,
    "availableQuantity": 60,
    "imageUrl": "https://example.com/carrots.jpg",
    "location": "Johannesburg, Gauteng",
    "harvestDate": "2024-12-10T00:00:00Z",
    "expiryDate": "2024-12-20T00:00:00Z",
    "categoryId": 1
  }'
```

**Note:** Only users with `Role = Farmer` can create products.

---

## Using Swagger UI

1. **Navigate to:** `http://localhost:5000/swagger`
2. **Test endpoints directly in the browser:**
   - Click on any endpoint
   - Click "Try it out"
   - Fill in the parameters
   - Click "Execute"
   - See the response

3. **For authenticated endpoints:**
   - Click the "Authorize" button (lock icon) at the top
   - Enter: `Bearer YOUR_TOKEN_HERE`
   - Click "Authorize"
   - Now you can test protected endpoints

---

## PowerShell Test Script

Save this as `test-api.ps1`:

```powershell
$baseUrl = "http://localhost:5000/api"

Write-Host "Testing API Endpoints..." -ForegroundColor Green
Write-Host ""

# Test 1: Get Products
Write-Host "1. Testing GET /api/products..." -ForegroundColor Yellow
try {
    $products = Invoke-RestMethod -Uri "$baseUrl/products" -Method Get
    Write-Host "✓ Success! Found $($products.Count) products" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# Test 2: Register User
Write-Host "2. Testing POST /api/auth/register..." -ForegroundColor Yellow
$registerData = @{
    email = "testuser$(Get-Random)@example.com"
    password = "TestPassword123!"
    firstName = "Test"
    lastName = "User"
    role = "Buyer"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "$baseUrl/auth/register" -Method Post -Body $registerData -ContentType "application/json"
    $token = $registerResponse.token
    Write-Host "✓ Success! User registered. Token received." -ForegroundColor Green
    Write-Host "  User ID: $($registerResponse.user.id)" -ForegroundColor Cyan
    Write-Host "  Email: $($registerResponse.user.email)" -ForegroundColor Cyan
    Write-Host ""
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    $token = $null
}

# Test 3: Login (if register failed, try with existing user)
if (-not $token) {
    Write-Host "3. Testing POST /api/auth/login..." -ForegroundColor Yellow
    $loginData = @{
        email = "testuser@example.com"
        password = "TestPassword123!"
    } | ConvertTo-Json
    
    try {
        $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginData -ContentType "application/json"
        $token = $loginResponse.token
        Write-Host "✓ Success! Login successful. Token received." -ForegroundColor Green
        Write-Host ""
    } catch {
        Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
    }
}

# Test 4: Get Products with Auth (optional)
if ($token) {
    Write-Host "4. Testing authenticated request..." -ForegroundColor Yellow
    $headers = @{
        "Authorization" = "Bearer $token"
    }
    try {
        $products = Invoke-RestMethod -Uri "$baseUrl/products" -Method Get -Headers $headers
        Write-Host "✓ Success! Authenticated request works." -ForegroundColor Green
        Write-Host ""
    } catch {
        Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
    }
}

Write-Host "API Testing Complete!" -ForegroundColor Green
```

Run it:
```powershell
.\test-api.ps1
```

---

## Common Issues

### Issue: "Connection refused" or "Cannot connect"
**Solution:** Make sure the application is running (`dotnet run`)

### Issue: "404 Not Found"
**Solution:** 
- Check the URL is correct: `http://localhost:5000/api/...`
- Make sure you're using the correct port (check `Properties/launchSettings.json`)

### Issue: "401 Unauthorized"
**Solution:**
- Make sure you're including the `Authorization: Bearer TOKEN` header
- Verify the token is valid (not expired)
- Check that the user has the correct role (e.g., Farmer for creating products)

### Issue: "500 Internal Server Error"
**Solution:**
- Check application logs
- Verify database connection
- Make sure database migrations have been run

---

## Verify API is Working

✅ **API is working if:**
- You can access Swagger UI at `/swagger`
- GET `/api/products` returns a list of products
- POST `/api/auth/register` creates a new user
- POST `/api/auth/login` returns a JWT token
- You can see API documentation in Swagger

---

## Next Steps

Once the API is working locally:
1. Test all endpoints in Swagger UI
2. Test from your Kotlin mobile app
3. Deploy to Render (see `RENDER_DEPLOYMENT.md`)
4. Update Kotlin app base URL to production URL




