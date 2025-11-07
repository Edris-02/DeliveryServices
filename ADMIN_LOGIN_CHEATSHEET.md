# ?? Admin Login - Quick Reference

```
??????????????????????????????????????????????
?   ?
?DEFAULT ADMIN CREDENTIALS?
?          ?
?  Email/Username:  admin@deliveryservices.com  ?
?  Password:        Admin123!@#              ?
?              ?
?  ??  CHANGE PASSWORD IN PRODUCTION!        ?
?        ?
??????????????????????????????????????????????
```

---

## ?? Quick Start (3 Steps)

```bash
# Step 1: Install Package
dotnet add DeliveryServices.Models\DeliveryServices.Models.csproj package Microsoft.Extensions.Identity.Stores

# Step 2: Run Migration
dotnet ef migrations add AddIdentityAndUserMerchantLink --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web
dotnet ef database update --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web

# Step 3: Start Application
dotnet run --project DeliveryServices.Web
```

---

## ?? Login Process

```
1. Open browser
   ?
2. Navigate to: https://localhost:PORT/Identity/Account/Login
   ?
3. Enter credentials:
   - Email: admin@deliveryservices.com
   - Password: Admin123!@#
   ?
4. Click "Sign In"
   ?
5. ? Redirected to /Admin/Dashboard
```

---

## ? What Happens Automatically

```
On First Run:
??? Creates Roles
?   ??? Admin
?   ??? Merchant
?   ??? Customer
??? Creates Admin User
?   ??? Username: admin@deliveryservices.com
?   ??? Email: admin@deliveryservices.com
?   ??? Password: Admin123!@#
??? Assigns Admin Role
??? Logs Success Message

On Subsequent Runs:
??? Checks if Admin exists
??? Finds existing user
??? Skips creation
```

---

## ?? Database Tables Created

```
AspNetUsers
??? Admin user (Username: admin@deliveryservices.com)

AspNetRoles
??? Admin
??? Merchant
??? Customer

AspNetUserRoles
??? Admin ? Admin role link

Merchants
??? (Empty initially)

Orders
??? (Empty initially)
```

---

## ?? Test Checklist

```
? Run migrations
? Start application
? Check logs for "Default admin user created successfully"
? Navigate to login page
? Enter admin@deliveryservices.com / Admin123!@#
? Verify redirected to dashboard
? Check admin menu accessible
? Create test order
? Create test merchant
? Test logout
? Login again (should work)
```

---

## ?? Expected Log Output

```
? SUCCESS:
info: Created role: Admin
info: Created role: Merchant
info: Created role: Customer
info: Default admin user created successfully - Username: admin@deliveryservices.com

? ALREADY EXISTS:
info: Default admin user already exists

? ERROR:
error: Failed to create default admin user: [details]
error: An error occurred while seeding the database
```

---

## ?? Troubleshooting

```
PROBLEM: Can't login
SOLUTION: Use email "admin@deliveryservices.com" not "Admin"

PROBLEM: "Access Denied" after login
SOLUTION: Check AspNetUserRoles table for Admin role assignment

PROBLEM: Admin user not created
SOLUTION: Check logs, verify migrations applied, check database connection

PROBLEM: Password doesn't work
SOLUTION: Ensure password is exactly "Admin123!@#" (case-sensitive)
```

---

## ??? Security Reminder

```
??  DEVELOPMENT:
? Default credentials OK
? Easy testing
? Quick demos

??  PRODUCTION:
? Change default password IMMEDIATELY
? Update email address
? Enable 2FA
? Consider disabling auto-seed
```

---

## ?? Documentation Reference

```
Quick Start           ? QUICK_START.md
Admin Seeding Guide   ? ADMIN_USER_SEEDING.md
Full Implementation   ? AUTHENTICATION_IMPLEMENTATION_GUIDE.md
Seeding Summary       ? AUTO_ADMIN_SEEDING_SUMMARY.md
This Cheat Sheet      ? ADMIN_LOGIN_CHEATSHEET.md
```

---

## ?? Login URLs

```
ADMIN LOGIN:
https://localhost:PORT/Identity/Account/Login

ADMIN DASHBOARD:
https://localhost:PORT/Admin/Dashboard

MERCHANT REGISTER:
https://localhost:PORT/Identity/Account/Register

MERCHANT DASHBOARD:
https://localhost:PORT/Merchant/Home
```

---

## ?? Pro Tips

```
? Use private/incognito mode to test different roles
? Keep a browser tab open with admin and another with merchant
? Check application console for startup logs
? Verify database tables after first run
? Copy email from this doc to avoid typos
? Clear cookies if login issues occur
? Check AspNetUsers table to confirm admin exists
```

---

## ?? Password Requirements

```
Minimum Length:        6 characters  ?
Uppercase Required:    Yes (A)       ?
Lowercase Required:    Yes (dmin)    ?
Digit Required:   Yes (123)     ?
Special Char Required: Yes (!@#)     ?

Default Password:      Admin123!@#   ? MEETS ALL
```

---

## ?? Need Help?

```
1. Check logs for error messages
2. Verify database connection
3. Ensure migrations are applied
4. Review ADMIN_USER_SEEDING.md
5. Check SQL Server is running
6. Verify email and password are correct
```

---

**COPY THIS FOR QUICK ACCESS:**

```
Email: admin@deliveryservices.com
Password: Admin123!@#
URL: https://localhost:PORT/Identity/Account/Login
```

---

**Last Updated**: 2024  
**Feature**: Automatic Admin Seeding  
**Status**: ? Active and Ready  
**Login Field**: Email (Username and Email are the same)
