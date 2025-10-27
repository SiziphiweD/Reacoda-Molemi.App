# Reacoda Molemi - Agritech Marketplace

A comprehensive web-based Agritech marketplace built with ASP.NET MVC 8.0, designed to connect farmers and buyers with role-based dashboards for farmers, buyers, admins, and super admins.

## 🌟 Features

### 🧑‍🌾 **Farmer Features**
- Dashboard with sales analytics and notifications
- Product management (add, edit, delete products)
- Order management (accept, reject, ship orders)
- Revenue tracking and statistics

### 🛒 **Buyer Features**
- Product browsing with filters and search
- Shopping cart functionality
- Order placement and tracking
- Product reviews and ratings

### 🧑‍💼 **Admin Features**
- User management and verification
- Product approval system
- Order monitoring and analytics
- Platform reports and insights

### 👑 **Super Admin Features**
- System health monitoring
- Admin account management
- Platform configuration
- Advanced analytics and audit logs

## 🛠️ Technology Stack

- **Backend:** ASP.NET MVC 8.0
- **Database:** PostgreSQL (shared with Android app)
- **ORM:** Entity Framework Core
- **Frontend:** Bootstrap 5, Poppins Font, Custom CSS
- **Authentication:** Session-based with role management
- **UI Theme:** Green (#1FAA59) and Red (#E74C3C) color scheme

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK
- PostgreSQL Server
- pgAdmin (for database management)
- Visual Studio 2022 or VS Code

### Database Setup

1. **Install PostgreSQL:**
   - Download from [postgresql.org](https://www.postgresql.org/download/)
   - Install with default settings
   - Remember your postgres user password

2. **Create Database:**
   ```sql
   -- Connect to PostgreSQL and run:
   CREATE DATABASE "Reacoda_MolemiDB";
   ```

3. **Update Connection String:**
   - Open `appsettings.json`
   - Update the connection string with your PostgreSQL credentials:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=Reacoda_MolemiDB;Username=postgres;Password=YOUR_PASSWORD;Port=5432"
     }
   }
   ```

### Running the Application

1. **Clone the project:**
   ```bash
   git clone <repository-url>
   cd Molemi.app
   ```

2. **Restore packages:**
   ```bash
   dotnet restore
   ```

3. **Create database:**
   ```bash
   dotnet ef database update
   ```

4. **Run the application:**
   ```bash
   dotnet run
   ```

5. **Access the application:**
   - **HTTPS:** `https://localhost:7000`
   - **HTTP:** `http://localhost:5000`

### Default Login Credentials

- **Super Admin:** `admin@reacoda.com`
- **Password:** (Set during first login)

## 📱 Android App Integration

This database is designed to be shared with an Android Studio application:

- **Database Name:** `Reacoda_MolemiDB`
- **Tables:** Users, Products, Orders, Categories, Notifications, Reviews
- **API Endpoints:** Ready for REST API implementation
- **Authentication:** Session-based (can be extended to JWT for mobile)

## 🗂️ Project Structure

```
Reacoda Molemi/
├── Controllers/          # MVC Controllers (Auth, Shop, Farmer, Admin, SuperAdmin)
├── Models/              # Data Models and ViewModels
├── Views/               # Razor Views for all roles
├── Data/                # Entity Framework DbContext
├── Services/            # Authentication and Session services
├── wwwroot/             # Static files (CSS, JS, images)
├── Migrations/          # Database migrations
└── Properties/          # Launch settings
```

## 🔧 Development

### Adding New Features

1. **Models:** Add to `Models/` folder
2. **Controllers:** Create in `Controllers/` folder
3. **Views:** Add to appropriate `Views/` subfolder
4. **Database:** Update `Data/ApplicationDbContext.cs`

### Database Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## 🎨 UI/UX Design

- **Color Scheme:** Green (#1FAA59) and Red (#E74C3C)
- **Typography:** Poppins font family
- **Layout:** Card-based responsive design
- **Animations:** Hover effects and smooth transitions
- **Mobile:** Fully responsive for all screen sizes

## 📊 Database Schema

### Key Tables:
- **Users:** Farmers, Buyers, Admins, SuperAdmins
- **Products:** Farm produce with categories
- **Orders:** Order management with status tracking
- **Categories:** Product categorization
- **Notifications:** User notifications system
- **Reviews:** Product reviews and ratings

## 🔐 Security Features

- Role-based authentication
- Session management
- Password hashing (SHA256)
- Input validation and sanitization
- SQL injection protection via EF Core

## 📈 Future Enhancements

- REST API for mobile app
- Real-time notifications
- Payment gateway integration
- Advanced analytics dashboard
- Multi-language support

## 📄 License

This project is developed for Reacoda PTY LTD.

## 🤝 Support

For support and questions, contact the development team.
