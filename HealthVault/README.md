# 🏥 HealthVault - Digital Health Records Management System

## 📋 Project Overview

HealthVault is a comprehensive web-based healthcare management system that enables patients to manage their medical records, book appointments, and interact with healthcare providers. The system provides role-based access for Patients, Staff, and Administrators.

## ✨ Key Features

### 👤 Patient Features
- User registration and authentication
- Book and manage appointments
- View and upload medical records
- Update personal profile
- View appointment history
- Dashboard with quick statistics

### 👨‍⚕️ Staff Features
- View and manage patient appointments
- Access patient medical records
- Update appointment statuses
- View patient list
- Staff dashboard with metrics

### 👨‍💼 Admin Features
- User management (create, update, delete users)
- Assign roles to users
- View system-wide statistics
- Manage staff accounts
- Monitor system activity

## 🛠️ Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0 Web API
- **ORM**: Entity Framework Core
- **Database**: SQL Server
- **Authentication**: JWT (JSON Web Tokens)
- **Architecture**: Clean Architecture / Layered Architecture

### Frontend
- **Framework**: Blazor Server (.NET 8)
- **CSS**: Custom CSS with Glassmorphism effects
- **UI Components**: Bootstrap 5
- **Icons**: SVG Icons
- **Notifications**: Custom Toast Service

### Additional Technologies
- Swagger/OpenAPI for API documentation
- CORS enabled for cross-origin requests
- Dependency Injection
- Data validation with Data Annotations

## 📁 Project Structure

```
HealthVault/
├── src/
│   ├── HealthVault.Api/          # Web API project
│   │   ├── Controllers/          # API endpoints
│   │   ├── Data/                 # DbContext & configurations
│   │   ├── Models/               # Data models
│   │   └── Services/             # Business logic
│   │
│   └── HealthVault.Web/          # Blazor Web UI
│       ├── Components/
│       │   ├── Layout/           # Layout components
│       │   ├── Pages/            # Page components
│       │   └── Shared/           # Shared components
│       ├── Services/             # Client services
│       └── wwwroot/              # Static files (CSS, images)
│
└── docs/                         # Documentation
```

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- SQL Server (LocalDB, Express, or Full)
- Visual Studio 2022 or VS Code
- Git (optional)

### Installation Steps

1. **Clone the Repository**
   ```bash
   git clone <repository-url>
   cd HealthVault
   ```

2. **Update Connection String**
   
   Edit `src/HealthVault.Api/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=.;Database=HealthVault;Trusted_Connection=true;TrustServerCertificate=true;"
     }
   }
   ```

3. **Create Database**
   ```bash
   cd src/HealthVault.Api
   dotnet ef database update
   ```

4. **Run the Application**
   
   **Terminal 1** (API):
   ```bash
   cd src/HealthVault.Api
   dotnet run
   ```
   
   **Terminal 2** (Web UI):
   ```bash
   cd src/HealthVault.Web
   dotnet run
   ```

5. **Access the Application**
   - Web UI: http://localhost:5081
   - API: http://localhost:5080
   - Swagger: http://localhost:5080/swagger

## 👥 Default User Accounts

### Patient
- Email: `patient@healthvault.com`
- Password: `Patient@12345`

### Staff
- Email: `staff@healthvault.com`
- Password: `Staff@12345`

### Admin
- Email: `admin@healthvault.com`
- Password: `Admin@12345`

## 📸 Screenshots

### Login Page
Clean authentication interface with social login options.

### Patient Dashboard
Overview of appointments, medical records, and quick actions.

### Appointment Booking
Simple interface to book appointments with healthcare providers.

### Admin Panel
Comprehensive user management and system statistics.

## 🎨 UI/UX Features

- ✨ **Glassmorphism Design** - Modern blur effects and transparency
- 🎨 **Gradient Accents** - Professional color schemes
- 📱 **Responsive Design** - Works on desktop, tablet, and mobile
- 🔔 **Toast Notifications** - Non-intrusive user feedback
- 🎭 **Empty States** - Helpful messages when no data exists
- ⏳ **Loading States** - Visual feedback during operations
- 🎯 **Role-Based UI** - Different interfaces for different user types

## 🔐 Security Features

- JWT-based authentication
- Password hashing (built-in ASP.NET Identity)
- Role-based authorization
- Input validation
- SQL injection protection (EF Core parameterized queries)
- CORS configuration
- Secure session management

## 🗄️ Database Schema

### Main Tables
- **Users** - User accounts and authentication
- **Patients** - Patient-specific information
- **Appointments** - Appointment bookings
- **MedicalRecords** - Patient medical history
- **Roles** - User role definitions
- **UserRoles** - User-role relationships

## 📝 API Endpoints

### Authentication
- `POST /api/Auth/register` - Register new user
- `POST /api/Auth/login` - User login

### Appointments
- `GET /api/Appointments` - Get all appointments
- `GET /api/Appointments/{id}` - Get appointment by ID
- `POST /api/Appointments` - Create appointment
- `PUT /api/Appointments/{id}` - Update appointment
- `DELETE /api/Appointments/{id}` - Delete appointment

### Medical Records
- `GET /api/MedicalRecords` - Get all records
- `GET /api/MedicalRecords/{id}` - Get record by ID
- `POST /api/MedicalRecords` - Create record
- `PUT /api/MedicalRecords/{id}` - Update record

### Users (Admin Only)
- `GET /api/Users` - Get all users
- `GET /api/Users/{id}` - Get user by ID
- `PUT /api/Users/{id}` - Update user
- `DELETE /api/Users/{id}` - Delete user

## 🧪 Testing

### Manual Testing
1. Test user registration and login
2. Test appointment booking as patient
3. Test appointment management as staff
4. Test user management as admin
5. Test responsive design on different devices

### Test Scenarios
- ✅ User can register with valid credentials
- ✅ User cannot register with existing email
- ✅ User can login with correct credentials
- ✅ Patient can book appointments
- ✅ Staff can view and manage appointments
- ✅ Admin can create/update/delete users
- ✅ Role-based access control works correctly

## 📦 Deployment

### Option 1: Azure
1. Create Azure App Service
2. Create Azure SQL Database
3. Update connection string
4. Publish using Visual Studio or CLI

### Option 2: Local IIS
1. Publish application
2. Create IIS website
3. Configure application pool
4. Set up database connection

### Option 3: Docker (Future)
Docker support can be added for containerized deployment.

## 🔧 Configuration

### JWT Settings
Edit `appsettings.json`:
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-chars",
    "Issuer": "HealthVault",
    "Audience": "HealthVaultUsers",
    "ExpiryMinutes": 60
  }
}
```

### CORS Settings
Edit `Program.cs` to configure allowed origins.

## 🐛 Known Issues

- Social login buttons are visual only (not functional)
- Email verification not implemented
- Password reset via email not implemented
- File upload size limited to 10MB

## 🚧 Future Enhancements

- [ ] Email verification system
- [ ] Password reset via email
- [ ] Two-factor authentication
- [ ] Real-time notifications (SignalR)
- [ ] Calendar view for appointments
- [ ] Export reports to PDF
- [ ] Dashboard charts and analytics
- [ ] Mobile app (Xamarin/MAUI)
- [ ] Integration with payment gateway
- [ ] Telemedicine video calls

## 📚 Documentation

- [Deployment Checklist](DEPLOYMENT_CHECKLIST.md) - Complete guide for production deployment
- [Quick Start Hosting](QUICK_START_HOSTING.md) - Fast hosting options
- [API Documentation](http://localhost:5080/swagger) - Interactive API docs (when running)

## 👨‍💻 Development

### Prerequisites for Development
- .NET 8.0 SDK
- Visual Studio 2022 / VS Code
- SQL Server Management Studio (optional)
- Postman (for API testing)

### Running in Development Mode
```bash
# Run API with hot reload
cd src/HealthVault.Api
dotnet watch run

# Run Web UI with hot reload
cd src/HealthVault.Web
dotnet watch run
```

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add MigrationName --project src/HealthVault.Api

# Update database
dotnet ef database update --project src/HealthVault.Api

<<<<<<< HEAD
# Remove last migration
dotnet ef migrations remove --project src/HealthVault.Api
```
=======
## 4. Deploy the API and Web app

This is a separate frontend and backend project, so submit one GitHub link and one deployment link for each app.

For Render, create two Docker web services from the repository root. Leave **Root Directory** empty, set the API service Dockerfile path to `Dockerfile.api`, and set the Web service Dockerfile path to `Dockerfile.web`. The Dockerfiles build the .NET 8 projects from the repository root.

The hosting platform must provide a reachable SQL Server database. Run the database scripts against that database before opening the API URL; the API then creates the demo Identity accounts on first start.

Set these environment variables on the API service:

```
ConnectionStrings__DefaultConnection=<SQL Server connection string>
Jwt__Key=<long random production secret>
Jwt__Issuer=HealthVault
Jwt__Audience=HealthVault.Clients
```

Set this environment variable on the Web service, using the public API URL and a trailing slash:

```
ApiBaseUrl=https://<your-api-domain>/
```

The apps use the hosting platform's `PORT` variable when it is supplied, and keep ports 5080 and 5081 for local development. Do not commit production connection strings, JWT keys, or real patient data.

## Demo accounts
>>>>>>> 960b12810b17516b39cef21ad8ba29fcce5b106a

## 🤝 Contributing

This is an academic project. Contributions are welcome for learning purposes.

## 📄 License

This project is created for educational purposes as an end-of-semester project.

## 👤 Author

**Your Name**
- Student ID: [Your ID]
- Program: [Your Program]
- Institution: [Your Institution]
- Email: [Your Email]
- GitHub: [Your GitHub]

## 🙏 Acknowledgments

- ASP.NET Core documentation
- Blazor documentation
- Bootstrap framework
- Stack Overflow community
- [Your Instructor's Name]

## 📞 Support

For issues and questions:
- Create an issue on GitHub
- Email: [your-email]

---

## 🎓 Academic Information

**Course**: [Course Name]
**Semester**: [Semester/Year]
**Instructor**: [Instructor Name]
**Submission Date**: [Date]

---

**Built with ❤️ using .NET 8.0 and Blazor**

Last Updated: December 2024
