# API Debugging Guide - User Registration/Login

## Issues Fixed

### 1. ✅ Async/Await Issues
- **Fixed:** Changed `Any()` to `AnyAsync()` for database queries
- **Fixed:** Changed `FirstOrDefault()` to `FirstOrDefaultAsync()` for async operations
- **Why:** Prevents blocking the thread and ensures proper async database access

### 2. ✅ Case-Insensitive Email Comparison
- **Fixed:** Email comparison now uses `.ToLower()` for case-insensitive matching
- **Why:** Prevents duplicate registrations with different cases (e.g., `Test@Email.com` vs `test@email.com`)

### 3. ✅ Comprehensive Logging Added
- **Added:** Detailed logging for registration and login attempts
- **Added:** EF Core SQL query logging in Development mode
- **Why:** Helps identify exactly where failures occur

### 4. ✅ Better Error Messages
- **Added:** More detailed error responses showing User ID when email exists
- **Why:** Easier debugging and user feedback

---

## How to Debug

### Step 1: Check Database Connection

Verify your connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=Reacoda_MolemiDB;Username=postgres;Password=12345;Port=5432"
  }
}
```

**Test Connection:**
1. Open pgAdmin
2. Connect to your PostgreSQL server
3. Navigate to `Reacoda_MolemiDB` database
4. Check if `Users` table exists
5. Query: `SELECT * FROM "Users" ORDER BY "Id" DESC LIMIT 10;`

### Step 2: Check Application Logs

When you run the application, you'll now see detailed logs:

**Registration Logs:**
```
[Information] Checking if email exists: test@example.com (normalized: test@example.com)
[Information] Email test@example.com is available for registration
[Information] Password hashed successfully for user: test@example.com
[Information] User registered successfully. User ID: 1, Email: test@example.com
```

**Login Logs:**
```
[Information] Login attempt for email: test@example.com (normalized: test@example.com)
[Information] User found. User ID: 1, Email: test@example.com, IsActive: True
[Information] Password verification result: Success for User ID: 1
```

**Error Logs:**
```
[Warning] Registration failed: Email already exists. Email: test@example.com, Existing User ID: 1
[Warning] Login failed: User not found. Email: test@example.com
[Warning] Login failed: Invalid password for User ID: 1, Email: test@example.com
```

### Step 3: View EF Core SQL Queries

In Development mode, EF Core will log all SQL queries to the console:

```
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (XXms) [Parameters=[@__normalizedEmail_0='test@example.com' (Size = 4000)], CommandType='Text', CommandTimeout='30']
      SELECT u."Id", u."Email", u."PasswordHash", ...
      FROM "Users" AS u
      WHERE LOWER(u."Email") = @__normalizedEmail_0
      LIMIT 1
```

This shows you:
- The exact SQL query being executed
- Parameters being passed
- Execution time
- Results returned

### Step 4: Test Registration

**Using Swagger:**
1. Go to `http://localhost:5000/swagger`
2. Click `POST /api/auth/register`
3. Click "Try it out"
4. Enter:
   ```json
   {
     "email": "newuser@example.com",
     "password": "Test123!",
     "firstName": "New",
     "lastName": "User",
     "role": "Buyer"
   }
   ```
5. Click "Execute"
6. Check the console logs for detailed information

**Using curl:**
```bash
curl -X POST "http://localhost:5000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "password": "Test123!",
    "firstName": "New",
    "lastName": "User"
  }'
```

**What to Check:**
- ✅ Does the log show "Email is available"?
- ✅ Does it show "Password hashed successfully"?
- ✅ Does it show "User registered successfully" with User ID?
- ✅ Check pgAdmin - does the user appear in the database?

### Step 5: Test Login

**Using Swagger:**
1. Click `POST /api/auth/login`
2. Enter:
   ```json
   {
     "email": "newuser@example.com",
     "password": "Test123!"
   }
   ```
3. Click "Execute"
4. Check console logs

**What to Check:**
- ✅ Does the log show "User found"?
- ✅ Does it show "Password verification result: Success"?
- ✅ Do you receive a JWT token in the response?

### Step 6: Common Issues and Solutions

#### Issue: "Email already registered" but email doesn't exist in database

**Possible Causes:**
1. **Case sensitivity:** Email exists with different case
   - **Solution:** Check database with: `SELECT * FROM "Users" WHERE LOWER("Email") = LOWER('test@example.com');`

2. **Different database:** Connection string points to wrong database
   - **Solution:** Verify connection string matches the database you're checking

3. **Transaction not committed:** Previous registration failed but email check passed
   - **Solution:** Check database for incomplete records

#### Issue: "Invalid email or password" on login

**Possible Causes:**
1. **User doesn't exist:**
   - **Check logs:** Does it say "User not found"?
   - **Solution:** Register the user first

2. **Wrong password:**
   - **Check logs:** Does it say "Invalid password"?
   - **Solution:** Use the correct password or reset it

3. **Password hash mismatch:**
   - **Check logs:** What is the "Password verification result"?
   - **Possible cause:** Password was hashed with different method
   - **Solution:** Check if existing users were created with different hashing method

4. **User is inactive:**
   - **Check logs:** Does it show `IsActive: False`?
   - **Solution:** Activate the user in database: `UPDATE "Users" SET "IsActive" = true WHERE "Email" = 'test@example.com';`

#### Issue: Password verification fails

**Debug Steps:**
1. Check the password hash in database:
   ```sql
   SELECT "Id", "Email", "PasswordHash" FROM "Users" WHERE "Email" = 'test@example.com';
   ```

2. Verify hash format:
   - ASP.NET Identity PasswordHasher creates hashes starting with specific prefixes
   - Should look like: `AQAAAAIAAYagAAAAE...` (base64 encoded)

3. Check if password was hashed with different method:
   - If existing users use BCrypt, you'll need to migrate them
   - Or create a compatibility layer

---

## Database Queries for Debugging

### Check All Users
```sql
SELECT 
    "Id", 
    "Email", 
    "FirstName", 
    "LastName", 
    "IsActive", 
    "IsVerified",
    "Role",
    "CreatedAt",
    LEFT("PasswordHash", 20) || '...' AS "PasswordHashPreview"
FROM "Users"
ORDER BY "Id" DESC;
```

### Check Specific Email (Case-Insensitive)
```sql
SELECT * FROM "Users" 
WHERE LOWER("Email") = LOWER('test@example.com');
```

### Check for Duplicate Emails
```sql
SELECT "Email", COUNT(*) as "Count"
FROM "Users"
GROUP BY LOWER("Email")
HAVING COUNT(*) > 1;
```

### Check Inactive Users
```sql
SELECT "Id", "Email", "IsActive", "IsVerified"
FROM "Users"
WHERE "IsActive" = false;
```

---

## Testing Checklist

- [ ] Application starts without errors
- [ ] Database connection successful (check logs)
- [ ] Can register new user with unique email
- [ ] Registration logs show success
- [ ] User appears in database after registration
- [ ] Can login with registered credentials
- [ ] Login logs show success
- [ ] JWT token is returned
- [ ] Token can be used for authenticated endpoints

---

## Next Steps if Still Having Issues

1. **Check Application Logs:** Look for detailed error messages
2. **Check Database:** Verify data actually exists
3. **Check Connection String:** Ensure it's correct
4. **Check EF Core Logs:** See the actual SQL queries
5. **Test with Swagger:** Use Swagger UI for easiest testing
6. **Compare with Existing Users:** Check how existing users were created (if any)

---

## Enable More Detailed Logging

To see even more details, update `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information",
      "ReacodeApp.Controllers.Api": "Debug"
    }
  }
}
```

This will show:
- All EF Core queries
- Detailed controller logs
- Request/response information




