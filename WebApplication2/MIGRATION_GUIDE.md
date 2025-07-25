# ??? **Database Migration Guide for Microsoft SQL Server**

## ?? **Migration Overview**

### **Current Database Structure**
Your Eid Al-Adha Farm API now uses a clean migration approach with:
- ? **No Static Seed Data** in migrations
- ? **Dynamic Demo Data Service** for testing
- ? **Microsoft SQL Server** optimized schema
- ? **Production-Ready** database structure

---

## ?? **Database Setup Instructions**

### **1. Prerequisites**
- Microsoft SQL Server (LocalDB, Express, or Full)
- .NET 9 SDK installed
- Connection string configured in `appsettings.json`

### **2. Connection String Configuration**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=abdallah;Database=Farm;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;"
  }
}
```

### **3. Database Creation & Migration**
```bash
# Navigate to project directory
cd WebApplication2

# Create database and apply migrations
dotnet ef database update

# Or run the application (migrations applied automatically)
dotnet run
```

---

## ??? **Database Schema**

### **Core Identity Tables**
- `AspNetUsers` - Extended user management
- `AspNetRoles` - Role-based access control  
- `AspNetUserRoles` - User-role relationships
- `AspNetUserClaims` - User claims
- `AspNetRoleClaims` - Role claims
- `AspNetUserLogins` - External login providers
- `AspNetUserTokens` - User tokens

### **Business Tables**
- `Categories` - Animal categories (Livestock, Dairy, Meat, etc.)
- `Farms` - User farms (one-to-one with users)
- `Animals` - Farm animals with pricing and Islamic compliance
- `Offers` - DataEntry created offers with buying/selling prices
- `Stocks` - Inventory management with quantities
- `Invoices` - Customer purchases with profit tracking
- `AuditLogs` - Complete activity tracking
- `UserOtps` - Email verification system
- `PendingRegistrations` - Pre-verification user data

---

## ?? **Demo Data Management**

### **Automatic Demo Data (Development)**
- Demo data automatically seeded in development environment
- Includes realistic users, animals, offers, and transactions
- No manual intervention needed

### **Manual Demo Data Control (SuperAdmin)**
```bash
# Check demo data status
GET /api/demodata/status

# Seed demo data
POST /api/demodata/seed

# Force seed (clears existing first)
POST /api/demodata/seed/force

# Clear all demo data
DELETE /api/demodata/clear

# Get demo data information
GET /api/demodata/info
```

### **Demo Data Includes**
- **5 Categories**: Livestock, Dairy Animals, Meat Animals, Breeding Stock, Sacrifice Animals
- **8 Users**: 3 DataEntry managers + 5 Customers
- **32 Animals**: Sheep, Goats, Cows, Camels with Islamic compliance
- **20 Offers**: Created by DataEntry users with pricing
- **8 Invoices**: Sample purchases with various statuses

---

## ??? **Migration Security Features**

### **Data Protection**
- No sensitive data in migration files
- Dynamic data generation for testing
- Production-safe migrations
- Role-based data access

### **Migration Files**
- `20250725094247_fiest.cs` - Initial database schema
- `20250725102800_RemoveStaticSeedData.cs` - Removes static seed data

---

## ?? **Migration Commands**

### **Basic Migration Commands**
```bash
# Check migration status
dotnet ef migrations list

# Create new migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Rollback to specific migration
dotnet ef database update PreviousMigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove
```

### **Advanced Migration Commands**
```bash
# Generate SQL script for migrations
dotnet ef migrations script

# Create migration bundle for deployment
dotnet ef migrations bundle

# Reset database (development only)
dotnet ef database drop --force
dotnet ef database update
```

---

## ?? **Production Deployment**

### **1. Migration Bundle Approach**
```bash
# Create migration bundle
dotnet ef migrations bundle --configuration Release

# Deploy to production
./efbundle.exe --connection "YourProductionConnectionString"
```

### **2. SQL Script Approach**
```bash
# Generate SQL scripts
dotnet ef migrations script --output migrations.sql

# Execute on production database
sqlcmd -S ProductionServer -d ProductionDB -i migrations.sql
```

### **3. Application Startup Approach**
```csharp
// Already configured in Program.cs
await context.Database.MigrateAsync();
```

---

## ?? **Database Performance**

### **Optimized Indexes**
- `IX_AspNetUsers_Email` (Unique)
- `IX_Categories_Name` (Unique)
- `IX_Invoices_InvoiceNumber` (Unique)
- `IX_Farms_OwnerId` (Unique)
- `IX_UserOtps_UserId_OtpCode_Purpose` (Composite)

### **Efficient Relationships**
- Proper foreign key constraints
- Optimized cascade delete behaviors
- Performance-oriented column types

---

## ?? **Troubleshooting**

### **Common Migration Issues**
```bash
# Issue: Migration pending
# Solution: Apply migrations
dotnet ef database update

# Issue: Connection string error
# Solution: Check appsettings.json configuration

# Issue: Permission denied
# Solution: Check SQL Server permissions

# Issue: Database already exists
# Solution: Drop and recreate (development only)
dotnet ef database drop --force
dotnet ef database update
```

### **Demo Data Issues**
```bash
# Issue: Demo data not seeding
# Solution: Check development environment
# Or manually seed via API: POST /api/demodata/seed

# Issue: Duplicate data errors
# Solution: Clear and reseed
# DELETE /api/demodata/clear
# POST /api/demodata/seed
```

---

## ? **Migration Checklist**

### **Development Environment**
- [x] Database connection configured
- [x] Migrations applied automatically
- [x] Demo data seeded automatically
- [x] All tables created with proper schema
- [x] SuperAdmin user created

### **Production Environment**
- [ ] Connection string configured for production
- [ ] Migration bundle created
- [ ] Database backup taken
- [ ] Migrations applied successfully
- [ ] SuperAdmin account secured
- [ ] Demo data cleared (if applicable)

---

## ?? **Support**

### **Migration Support**
- Check Entity Framework logs for detailed error information
- Verify database connectivity and permissions
- Ensure .NET 9 compatibility

### **Demo Data Support**  
- Use demo data controller endpoints for management
- Check application logs for seeding errors
- Verify user permissions for demo data operations

---

**?? Your database is now ready for production with clean migrations and dynamic demo data!**