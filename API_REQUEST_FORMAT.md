# API Request Format Guide

## ❌ Common Mistake: Wrapping in "request" object

**WRONG:**
```json
{
  "request": {
    "email": "newuser@example.com",
    "password": "Test123!",
    "firstName": "New",
    "lastName": "User"
  }
}
```

**CORRECT:**
```json
{
  "email": "newuser@example.com",
  "password": "Test123!",
  "firstName": "New",
  "lastName": "User",
  "role": "Buyer"
}
```

---

## ✅ Correct Request Formats

### Register User

**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "newuser@example.com",
  "password": "Test123!",
  "firstName": "New",
  "lastName": "User",
  "phoneNumber": "+27 82 123 4567",
  "role": "Buyer"
}
```

**cURL:**
```bash
curl -X POST "http://localhost:5000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "password": "Test123!",
    "firstName": "New",
    "lastName": "User",
    "role": "Buyer"
  }'
```

**PowerShell:**
```powershell
$body = @{
    email = "newuser@example.com"
    password = "Test123!"
    firstName = "New"
    lastName = "User"
    role = "Buyer"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/auth/register" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

---

### Login

**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "newuser@example.com",
  "password": "Test123!"
}
```

**cURL:**
```bash
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "password": "Test123!"
  }'
```

---

### Create Product (Requires Auth)

**Endpoint:** `POST /api/products`

**Headers:**
```
Authorization: Bearer YOUR_JWT_TOKEN_HERE
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Fresh Organic Tomatoes",
  "description": "Premium organic tomatoes",
  "pricePerKg": 45.00,
  "availableQuantity": 50,
  "imageUrl": "https://example.com/tomatoes.jpg",
  "location": "Cape Town, Western Cape",
  "harvestDate": "2024-12-10T00:00:00Z",
  "expiryDate": "2024-12-17T00:00:00Z",
  "categoryId": 1
}
```

---

## 🔍 How to Use Swagger UI Correctly

1. **Go to:** `http://localhost:5000/swagger`
2. **Click on** `POST /api/auth/register`
3. **Click "Try it out"**
4. **In the Request body field, paste this (NO "request" wrapper):**
   ```json
   {
     "email": "newuser@example.com",
     "password": "Test123!",
     "firstName": "New",
     "lastName": "User",
     "role": "Buyer"
   }
   ```
5. **Click "Execute"**

**Important:** Do NOT add `"request": { ... }` around the data. The API expects the properties directly in the JSON root.

---

## 📝 Property Names (camelCase)

All property names must be in **camelCase**:

- ✅ `email` (not `Email`)
- ✅ `firstName` (not `FirstName`)
- ✅ `lastName` (not `LastName`)
- ✅ `phoneNumber` (not `PhoneNumber`)
- ✅ `pricePerKg` (not `PricePerKg`)
- ✅ `availableQuantity` (not `AvailableQuantity`)
- ✅ `categoryId` (not `CategoryId`)

---

## 🚨 Common Errors

### Error: "The request field is required"
**Cause:** You wrapped the data in a `"request"` object  
**Fix:** Remove the `"request"` wrapper, send data directly

### Error: "Validation failed"
**Cause:** Missing required fields or invalid format  
**Fix:** Check that all required fields are present and correctly formatted

### Error: "Email already registered"
**Cause:** Email already exists in database  
**Fix:** Use a different email or check existing users

---

## ✅ Quick Test

**Test Registration:**
```bash
curl -X POST "http://localhost:5000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!","firstName":"Test","lastName":"User"}'
```

**Expected Response:**
```json
{
  "message": "User registered successfully",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "test@example.com",
    "firstName": "Test",
    "lastName": "User",
    "role": "Buyer"
  }
}
```




