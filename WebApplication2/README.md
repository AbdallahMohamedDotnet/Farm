# Eid Al-Adha Sacrifice Farm API

A comprehensive .NET 9 Web API for managing farms and animals during Eid Al-Adha, implementing Islamic sacrifice eligibility rules with a secure 3-step registration process and email verification.

## ?? New Registration & Authentication Flow

### ?? **3-Step Registration Process**

#### **Step 1: Registration Request** 
- User fills: Name, Email, Password
- System stores as pending registration (NOT in user database yet)
- Sends OTP to email
- **No user account created yet**

#### **Step 2: Email Confirmation**
- User enters OTP from email
- System validates OTP
- **Creates user account in database**
- **Creates farm automatically**
- Account is now active

#### **Step 3: Login**
- User logs in with email/password
- **Returns only user's full name** (no token)
- Simple name-based authentication response

---

## ?? API Endpoints

### **Step 1: POST** `/api/auth/register`
Initiate registration - collect user details and send OTP.

**Request:**
```json
{
  "username": "abdallah_farm",
  "email": "abdallah@example.com",
  "password": "Password123!",
  "firstName": "Abdallah",
  "lastName": "Mohamed"
}
```

**Response:**
```json
{
  "message": "Registration initiated. Please check your email for the verification code to complete your registration.",
  "success": true,
  "requiresEmailConfirmation": true
}
```

### **Step 2: POST** `/api/auth/confirm-email`
Validate OTP and create user account in database.

**Request:**
```json
{
  "email": "abdallah@example.com",
  "otpCode": "123456"
}
```

**Response:**
```json
{
  "message": "Email confirmed successfully! Your account has been created.",
  "success": true,
  "userName": "Abdallah Mohamed"
}
```

### **Step 3: POST** `/api/auth/login`
Login and get user's name only.

**Request:**
```json
{
  "email": "abdallah@example.com",
  "password": "Password123!"
}
```

**Response:**
```json
{
  "message": "Login successful",
  "success": true,
  "userName": "Abdallah Mohamed"
}
```

### **POST** `/api/auth/resend-otp`
Resend OTP if needed.

**Request:**
```json
{
  "email": "abdallah@example.com",
  "purpose": "EmailConfirmation"
}
```

---

## ??? Tech Stack

- **Backend**: ASP.NET Core 9 Web API
- **Database**: Microsoft SQL Server
- **Authentication**: ASP.NET Identity (Name-only response)
- **Email**: MailKit/MimeKit for SMTP
- **Documentation**: Swagger/OpenAPI

## ?? Database Connection

```
Server=abdallah;Database=Farm;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;
```

## ??? Enhanced Database Schema

### **New Table: PendingRegistrations**
Stores registration data before email confirmation:
- Id, Email, Username, FirstName, LastName
- PasswordHash, CreatedAt, ExpiresAt (30 min)
- IsConfirmed flag

### **Updated UserOtps Table**
Handles OTP for pending registrations:
- Uses temporary UserId format: `pending_{email}_{purpose}`
- 15-minute expiry for security
- Automatic cleanup of expired entries

### **Users Table** (Created only after email confirmation)
- Standard ASP.NET Identity
- FirstName, LastName, IsActive
- EmailConfirmed = true (since confirmed before creation)

### **Farms Table**
- Automatically created after email confirmation
- Named: "{FirstName} {LastName}'s Farm"

---

## ?? Registration Flow Details

### **1. User Registration Request**
```
User submits form ? Data stored in PendingRegistrations ? OTP generated ? Email sent
```

### **2. Email Verification**
```
User enters OTP ? OTP validated ? User created in database ? Farm created ? Success response
```

### **3. Login Process**
```
User logs in ? Credentials validated ? Return full name only
```

### **4. Security Features**
- **30-minute expiry** for pending registrations
- **15-minute expiry** for OTP codes
- **Automatic cleanup** of expired records
- **Duplicate prevention** for pending registrations
- **Password hashing** before storage

---

## ??? Islamic Sacrifice Rules

### Eligible Animals & Minimum Ages
- **Camel**: 5 years (60 months)
- **Cow**: 2 years (24 months)
- **Goat**: 1 year (12 months)
- **Sheep**: 6 months

---

## ?? Email Configuration

Configure SMTP settings in `appsettings.json`:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password",
    "SenderName": "Farm Management System"
  }
}
```

---

## ?? Setup Instructions

### 1. Prerequisites
- .NET 9 SDK
- SQL Server with Windows Authentication
- SMTP server access (Gmail recommended)

### 2. Database Setup
```bash
# Database will be created automatically on first run
# Tables: Users, Farms, Animals, Categories, UserOtps, AuditLogs, PendingRegistrations
```

### 3. Email Configuration
- Enable 2FA on Gmail
- Generate App Password
- Update EmailSettings in appsettings.json

### 4. Run Application
```bash
cd WebApplication2
dotnet run
```

### 5. Default Admin Account
- **Email**: admin@farm.com
- **Password**: Admin123!
- **Role**: SuperAdmin

---

## ?? Testing the New Flow

### **Complete Registration Test**

1. **Register User**
```bash
POST /api/auth/register
{
  "username": "test_user",
  "email": "test@example.com", 
  "password": "Test123!",
  "firstName": "Test",
  "lastName": "User"
}
```

2. **Check Email for OTP**
- Look for 6-digit code
- Valid for 15 minutes

3. **Confirm Email**
```bash
POST /api/auth/confirm-email
{
  "email": "test@example.com",
  "otpCode": "123456"
}
```

4. **Login**
```bash
POST /api/auth/login
{
  "email": "test@example.com",
  "password": "Test123!"
}
# Returns: {"userName": "Test User"}
```

---

## ?? Error Handling

### **Registration Errors**
- `"User already exists"` - Email already registered
- `"A registration for this email is already pending"` - Wait or let expire

### **Confirmation Errors**
- `"Invalid or expired OTP"` - Request new OTP
- `"Registration has expired"` - Start registration again
- `"No pending registration found"` - Register first

### **Login Errors**
- `"Invalid email or password"` - Check credentials
- `"Email not confirmed"` - Complete email confirmation
- `"Account is deactivated"` - Contact admin

---

## ?? Future Enhancements

- [ ] SMS-based OTP as alternative
- [ ] Social media login integration
- [ ] Multi-factor authentication
- [ ] Password reset functionality
- [ ] Account recovery options

---

## ?? Support

For technical support:
- Check application logs for detailed error information
- Verify email configuration for OTP delivery
- Ensure database connectivity

---

**Note**: This implementation follows a secure email-first registration approach where user accounts are only created after successful email verification.