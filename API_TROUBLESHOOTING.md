# API Troubleshooting Guide

## Common "Bad Request" Issues and Solutions

### 1. Missing Content-Type Header

**Problem:** API expects JSON but request doesn't specify content type.

**Solution:** Always include `Content-Type: application/json` header:

```bash
curl -X POST "http://localhost:5000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!","firstName":"Test","lastName":"User"}'
```

### 2. Invalid JSON Format

**Problem:** Malformed JSON in request body.

**Solution:** Ensure JSON is valid:
- All strings must be in double quotes: `"email"` not `'email'`
- No trailing commas
- Proper escaping of special characters

**Valid:**
```json
{
  "email": "test@example.com",
  "password": "Test123!",
  "firstName": "Test",
  "lastName": "User"
}
```

**Invalid:**
```json
{
  'email': 'test@example.com',  // ❌ Single quotes
  "password": "Test123!",        // ❌ Trailing comma
}
```

### 3. Missing Required Fields

**Problem:** Required fields are missing from request.

**Solution:** Check which fields are required:

**Register Request:**
- `email` (required, must be valid email)
- `password` (required, min 6 characters)
- `firstName` (required)
- `lastName` (required)
- `phoneNumber` (optional)
- `role` (optional, defaults to Buyer)

**Login Request:**
- `email` (required)
- `password` (required)

**Create Product Request:**
- `name` (required)
- `description` (required)
- `pricePerKg` (required, must be > 0)
- `availableQuantity` (required, must be >= 1)
- `categoryId` (required)
- `imageUrl` (optional)
- `location` (optional)
- `harvestDate` (optional)
- `expiryDate` (optional)

### 4. Property Name Casing

**Problem:** Property names don't match (camelCase vs PascalCase).

**Solution:** API uses camelCase. Use these property names:

**Correct (camelCase):**
```json
{
  "email": "test@example.com",
  "firstName": "Test",
  "lastName": "User"
}
```

**Incorrect (PascalCase):**
```json
{
  "Email": "test@example.com",  // ❌ Wrong
  "FirstName": "Test"           // ❌ Wrong
}
```

### 5. Empty Request Body

**Problem:** Sending empty body or null.

**Solution:** Always include a valid JSON body, even if minimal:

```json
{}
```

### 6. Testing with Swagger UI

**Best Practice:** Use Swagger UI to test - it handles all formatting automatically:

1. Go to: `http://localhost:5000/swagger`
2. Click on endpoint (e.g., `/api/auth/register`)
3. Click "Try it out"
4. Fill in the form
5. Click "Execute"

Swagger will show you:
- Exact request format
- Required vs optional fields
- Example responses
- Error messages

### 7. Common Error Responses

**400 Bad Request - Validation Failed:**
```json
{
  "message": "Validation failed",
  "errors": [
    {
      "field": "email",
      "errors": ["Email is required"]
    }
  ]
}
```

**400 Bad Request - Request Body Null:**
```json
{
  "message": "Request body is required",
  "errors": ["Request body cannot be null"]
}
```

**400 Bad Request - Email Already Registered:**
```json
{
  "message": "Email already registered"
}
```

### 8. Testing with PowerShell

**Example - Register:**
```powershell
$body = @{
    email = "test@example.com"
    password = "Test123!"
    firstName = "Test"
    lastName = "User"
    role = "Buyer"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/auth/register" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

**Example - Login:**
```powershell
$body = @{
    email = "test@example.com"
    password = "Test123!"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"

$token = $response.token
Write-Host "Token: $token"
```

### 9. Testing with cURL

**Register:**
```bash
curl -X POST "http://localhost:5000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "firstName": "Test",
    "lastName": "User",
    "role": "Buyer"
  }'
```

**Login:**
```bash
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }'
```

**Get Products:**
```bash
curl -X GET "http://localhost:5000/api/products" \
  -H "Content-Type: application/json"
```

### 10. Check Application Logs

If you're still getting errors, check the application console output for detailed error messages. The API now provides more detailed error information.

### 11. Verify API is Running

Make sure your application is running:
```bash
dotnet run
```

Then test a simple endpoint:
```bash
curl http://localhost:5000/api/products
```

If this works, the API is running. If not, check:
- Port number (might be 5000 or 7000)
- Application is actually running
- No firewall blocking the port

### 12. CORS Issues (for Mobile Apps)

If calling from a mobile app or different origin, ensure CORS is configured. The API is already configured to allow mobile apps.

### Quick Diagnostic Checklist

- [ ] Application is running (`dotnet run`)
- [ ] Using correct URL (`http://localhost:5000/api/...`)
- [ ] Content-Type header is set (`application/json`)
- [ ] JSON is valid (check with JSON validator)
- [ ] Property names are camelCase
- [ ] All required fields are included
- [ ] Email format is valid
- [ ] Password meets requirements (min 6 characters)
- [ ] No trailing commas in JSON
- [ ] All strings use double quotes

### Still Having Issues?

1. **Test with Swagger UI first** - This eliminates formatting issues
2. **Check the exact error message** - The API now returns detailed errors
3. **Verify your JSON** - Use a JSON validator online
4. **Check application logs** - Look for detailed error messages in console




