# PostgreSQL Setup Guide for Reacoda Molemi

## 🐘 PostgreSQL Installation & Setup

### Step 1: Install PostgreSQL

1. **Download PostgreSQL:**
   - Visit [postgresql.org/download](https://www.postgresql.org/download/)
   - Download PostgreSQL for Windows
   - Run the installer with default settings
   - **Remember your postgres user password!**

### Step 2: Install pgAdmin (Optional but Recommended)

1. **Download pgAdmin:**
   - Visit [pgadmin.org](https://www.pgadmin.org/download/)
   - Download and install pgAdmin 4
   - This provides a GUI for database management

### Step 3: Create Database and User

#### Option A: Using pgAdmin (GUI)
1. Open pgAdmin
2. Connect to PostgreSQL server
3. Right-click on "Databases" → "Create" → "Database"
4. Name: `Reacoda_MolemiDB`
5. Click "Save"

#### Option B: Using SQL Script
1. Open Command Prompt or pgAdmin Query Tool
2. Run the `database_setup.sql` script included in the project
3. This will create the database and dedicated user

#### Option C: Using psql Command Line
```bash
# Connect to PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE "Reacoda_MolemiDB";

# Create user
CREATE USER molemi_user WITH PASSWORD 'molemi_password123';

# Grant permissions
GRANT ALL PRIVILEGES ON DATABASE "Reacoda_MolemiDB" TO molemi_user;
```

### Step 4: Verify Setup

1. **Test Connection:**
   ```bash
   psql -h localhost -U molemi_user -d "Reacoda_MolemiDB"
   ```

2. **Check Database:**
   ```sql
   \l
   \c "Reacoda_MolemiDB"
   \dt
   ```

## 🔧 Application Setup

### Step 1: Update Connection String (if needed)

The connection string is already configured in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=Reacoda_MolemiDB;Username=molemi_user;Password=molemi_password123;Port=5432"
  }
}
```

### Step 2: Run Database Migration

```bash
# Navigate to project directory
cd Molemi.app

# Restore packages
dotnet restore

# Create database tables
dotnet ef database update

# Run the application
dotnet run
```

## 📱 Android App Integration

### Database Connection Details for Android Studio:

- **Host:** `localhost` (or your server IP)
- **Port:** `5432`
- **Database:** `Reacoda_MolemiDB`
- **Username:** `molemi_user`
- **Password:** `molemi_password123`

### Recommended Android Libraries:
- **Room Database** (for local caching)
- **Retrofit** (for API calls)
- **OkHttp** (for HTTP client)

### API Endpoints (Future):
- `GET /api/products` - Get all products
- `POST /api/orders` - Create new order
- `GET /api/users/{id}` - Get user details
- `POST /api/auth/login` - User authentication

## 🔍 Troubleshooting

### Common Issues:

1. **Connection Refused:**
   - Ensure PostgreSQL service is running
   - Check if port 5432 is open
   - Verify firewall settings

2. **Authentication Failed:**
   - Double-check username and password
   - Ensure user has proper permissions
   - Try connecting with postgres user first

3. **Database Not Found:**
   - Verify database name spelling
   - Check if database was created successfully
   - Ensure you're connecting to the right server

4. **Migration Errors:**
   - Delete `Migrations` folder and recreate
   - Check connection string format
   - Ensure Entity Framework tools are installed

### Useful Commands:

```bash
# Check PostgreSQL status
pg_ctl status

# Start PostgreSQL service
pg_ctl start

# Stop PostgreSQL service
pg_ctl stop

# List all databases
psql -U postgres -c "\l"

# Connect to specific database
psql -U molemi_user -d "Reacoda_MolemiDB"
```

## 🚀 Next Steps

1. **Test the web application** at `https://localhost:7000`
2. **Create sample data** through the admin interface
3. **Set up Android Studio project** with database connection
4. **Implement REST API** for mobile app integration
5. **Deploy to production server** when ready

## 📞 Support

If you encounter any issues:
1. Check the troubleshooting section above
2. Verify PostgreSQL installation
3. Test database connection manually
4. Check application logs for specific errors

