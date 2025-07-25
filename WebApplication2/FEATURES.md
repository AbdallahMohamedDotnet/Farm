# Eid Al-Adha Sacrifice Farm API - Complete Feature Documentation

A comprehensive .NET 9 Web API for managing farms and animals during Eid Al-Adha with role-based business operations, email verification, and complete sales management system.

## ?? **Complete Feature Set**

### ?? **Authentication & User Management**
- ? **3-Step Registration**: Email ? OTP ? Account Creation
- ? **Email Verification**: OTP-based confirmation
- ? **Role-Based Access**: SuperAdmin, DataEntry, Customer
- ? **JWT Authentication**: Secure token-based auth
- ? **Name-Only Login**: Returns user's full name only

### ?? **Role-Based Business Operations**

#### **????? SuperAdmin Features**
- ? **Sales Analytics**: Total sales, profits, monthly reports
- ? **Employee Management**: Assign DataEntry role by email
- ? **User Management**: View all users with roles and status
- ? **Invoice Oversight**: View all invoices with profit tracking
- ? **Offer Monitoring**: Monitor all offers with sales data

#### **?? DataEntry Features**
- ? **Animal Management**: Add animals to system with pricing
- ? **Offer Creation**: Create offers with buying/selling prices
- ? **Stock Management**: Manage inventory with quantities
- ? **Price Setting**: Set buying and selling prices
- ? **Offer Control**: Activate/deactivate offers

#### **?? Customer Features**
- ? **Browse Offers**: View all available offers
- ? **Purchase System**: Buy animals through invoice creation
- ? **Invoice Management**: View and manage purchase invoices
- ? **Payment Processing**: Pay or cancel invoices
- ? **Purchase History**: Track all purchases

### ?? **Animal & Farm Management**
- ? **Islamic Compliance**: Sacrifice eligibility validation
- ? **Enhanced Animal Properties**: Weight, breed, pricing
- ? **Category System**: Organize animals by categories
- ? **Stock Tracking**: Quantity management
- ? **Auto Farm Creation**: Automatic farm creation on registration

### ?? **Business & Financial Features**
- ? **Buying/Selling Prices**: Track costs and revenues
- ? **Profit Calculation**: Automatic profit margin calculation
- ? **Invoice System**: Complete invoicing with auto-numbering
- ? **Sales Analytics**: Comprehensive reporting
- ? **Stock Management**: Inventory tracking with quantities

### ?? **Email & Communication**
- ? **SMTP Integration**: MailKit for email sending
- ? **OTP System**: 6-digit codes with 15-min expiry
- ? **Email Templates**: Professional HTML emails
- ? **Resend Functionality**: OTP resend capability

### ?? **Audit & Logging**
- ? **Activity Tracking**: Complete user action logging
- ? **Security Monitoring**: Track all system changes
- ? **Comprehensive Logs**: Action, entity, and detail tracking

---

## ??? **Database Schema**

### **Core Tables**
- **Users**: Extended IdentityUser with custom fields
- **Farms**: One-to-one with Users
- **Animals**: Enhanced with pricing and business data
- **Categories**: Animal categorization
- **UserOtps**: Email verification system
- **PendingRegistrations**: Pre-verification user data

### **Business Tables**
- **Offers**: DataEntry created offers with pricing
- **Stock**: Inventory management with quantities
- **Invoices**: Customer purchases with profit tracking
- **AuditLogs**: Complete activity tracking

---

## ?? **API Endpoints Summary**

### **Authentication** (`/api/auth/`)
- `POST /register` - Initiate registration
- `POST /confirm-email` - Verify OTP and create account
- `POST /login` - Login and get user name
- `POST /resend-otp` - Resend verification code

### **DataEntry** (`/api/dataentry/`) [DataEntry Role]
- `POST /offers` - Create new offer
- `GET /offers` - Get my offers
- `PUT /offers/{id}` - Update offer
- `DELETE /offers/{id}` - Delete offer
- `PUT /offers/{id}/activate` - Activate offer
- `PUT /offers/{id}/deactivate` - Deactivate offer
- `POST /animals` - Add new animal

### **Customer** (`/api/customer/`) [Customer Role]
- `GET /offers` - Browse available offers
- `GET /offers/{id}` - View offer details
- `POST /purchase` - Purchase offer (create invoice)
- `GET /invoices` - View my invoices
- `GET /invoices/{id}` - View invoice details
- `PUT /invoices/{id}/pay` - Pay invoice
- `PUT /invoices/{id}/cancel` - Cancel invoice

### **Admin** (`/api/admin/`) [SuperAdmin Role]
- `GET /analytics` - Sales analytics and profits
- `GET /invoices` - All invoices with profit data
- `GET /offers` - All offers with sales data
- `POST /assign-dataentry` - Assign DataEntry role
- `GET /users` - View all users with roles

### **Animals** (`/api/animals/`) [Authenticated]
- `POST /` - Create animal
- `GET /` - Get user's animals
- `PUT /{id}/feed` - Feed animal
- `PUT /{id}/groom` - Groom animal
- `PUT /{id}/sacrifice` - Sacrifice animal
- `DELETE /{id}` - Delete animal

---

## ?? **Complete Usage Workflow**

### **1. User Registration & Setup**
```bash
# Step 1: Register
POST /api/auth/register
{
  "username": "farm_manager",
  "email": "manager@farm.com",
  "password": "Password123!",
  "firstName": "Farm",
  "lastName": "Manager"
}

# Step 2: Confirm Email
POST /api/auth/confirm-email
{
  "email": "manager@farm.com",
  "otpCode": "123456"
}

# Step 3: Login
POST /api/auth/login
{
  "email": "manager@farm.com",
  "password": "Password123!"
}
# Returns: {"userName": "Farm Manager"}
```

### **2. SuperAdmin Operations**
```bash
# Assign DataEntry role
POST /api/admin/assign-dataentry
{
  "email": "dataentry@farm.com"
}

# View sales analytics
GET /api/admin/analytics?startDate=2024-01-01&endDate=2024-12-31

# Monitor all business operations
GET /api/admin/invoices
GET /api/admin/offers
```

### **3. DataEntry Operations**
```bash
# Add animal with pricing
POST /api/dataentry/animals
{
  "name": "Premium Sheep",
  "type": "Sheep",
  "age": 12,
  "weight": 45.5,
  "buyingPrice": 1500,
  "sellingPrice": 2000
}

# Create offer
POST /api/dataentry/offers
{
  "title": "Premium Sheep for Eid",
  "description": "High-quality sheep ready for sacrifice",
  "animalId": "animal-guid",
  "sellingPrice": 2000,
  "buyingPrice": 1500
}
```

### **4. Customer Operations**
```bash
# Browse offers
GET /api/customer/offers

# Purchase animal
POST /api/customer/purchase
{
  "offerId": "offer-guid",
  "quantity": 1,
  "notes": "For Eid Al-Adha sacrifice"
}

# Pay invoice
PUT /api/customer/invoices/{invoice-id}/pay
```

---

## ?? **Business Intelligence Features**

### **Sales Analytics** (SuperAdmin)
- **Total Sales**: Revenue from all paid invoices
- **Total Profits**: Sales minus buying costs
- **Profit Margins**: Percentage calculations
- **Monthly Reports**: Sales trends over time
- **Top Animals**: Best-selling animal types
- **Invoice Status**: Paid vs pending tracking

### **Example Analytics Response**
```json
{
  "totalSales": 25000.00,
  "totalBuyingCost": 18000.00,
  "totalProfit": 7000.00,
  "profitMargin": 28.0,
  "totalInvoices": 15,
  "paidInvoices": 12,
  "pendingInvoices": 3,
  "monthlySales": [
    {
      "year": 2024,
      "month": 3,
      "monthName": "March",
      "sales": 5000.00,
      "profit": 1200.00,
      "invoiceCount": 3
    }
  ],
  "topSellingAnimals": [
    {
      "animalType": "Sheep",
      "totalSold": 8,
      "totalRevenue": 16000.00,
      "totalProfit": 4000.00
    }
  ]
}
```

---

## ??? **Security Features**

### **Authentication Security**
- Email-first registration approach
- OTP verification with expiry
- JWT tokens with role claims
- Account activation/deactivation

### **Authorization Security**
- Role-based access control
- User isolation (own data only)
- Resource ownership validation
- Audit trail for all actions

### **Data Security**
- Password hashing before storage
- Secure connection strings
- CSRF protection headers
- Input validation and sanitization

---

## ?? **Technical Features**

### **Database Features**
- Entity Framework Core 9.0
- Code-first migrations
- Relationship management
- Index optimization
- Seed data initialization

### **Email System**
- MailKit SMTP integration
- HTML email templates
- OTP generation and validation
- Automatic cleanup of expired OTPs

### **Business Logic**
- Islamic sacrifice eligibility
- Automatic profit calculations
- Stock quantity management
- Invoice auto-numbering
- Comprehensive error handling

---

## ?? **Deployment Ready**

### **Production Checklist**
- ? Connection string configuration
- ? Email SMTP setup
- ? JWT secret key security
- ? Role seeding
- ? Database initialization
- ? Audit logging
- ? Error handling
- ? API documentation

### **Default Accounts**
- **SuperAdmin**: admin@farm.com / Admin123!
- **Roles**: SuperAdmin, DataEntry, Customer
- **Auto-created**: Farms for all users

---

## ?? **Support & Maintenance**

### **Monitoring**
- Comprehensive audit logs
- Error tracking and logging
- User activity monitoring
- Business metrics tracking

### **Maintenance**
- Automatic database creation
- Seed data initialization
- Role management
- User account management

---

**?? All Features Successfully Implemented and Tested!**

The API provides a complete business solution for farm management during Eid Al-Adha with role-based operations, financial tracking, and Islamic compliance validation.