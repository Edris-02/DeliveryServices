# Automatic Admin User Seeding

## Overview
The application now automatically creates a default admin user on the first run, eliminating the need for manual user creation.

---

## Default Admin Credentials

### Login Information
- **Username**: `Admin`
- **Email**: `admin@deliveryservices.com`
- **Password**: `Admin123!@#`

?? **IMPORTANT**: Change this password immediately after first login in production!

---

## How It Works

### Automatic Seeding Process

1. **Application Startup**: When the app starts, the `SeedDefaultAdminUser()` method is called
2. **Role Creation**: Ensures all roles exist (Admin, Merchant, Customer)
3. **Admin Check**: Checks if user "Admin" already exists
4. **User Creation**: If not found, creates the admin user with specified credentials
5. **Role Assignment**: Assigns the "Admin" role to the user
6. **Logging**: Logs all actions for debugging

### Code Location
**File**: `DeliveryServices.Web\Program.cs`

```csharp
// Seed default admin user on first run
await SeedDefaultAdminUser(app);
```

---

## Password Requirements

The admin password `Admin123!@#` meets all security requirements:
- ? Minimum 6 characters
- ? Contains uppercase letters (A)
- ? Contains lowercase letters (dmin)
- ? Contains digits (123)
- ? Contains special characters (!@#)

---

## First Run Behavior

### When Application Starts First Time:

1. **Database Migration**: Ensure migrations are applied
   ```bash
   dotnet ef database update --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web
   ```

2. **Auto-Seeding**: Application automatically:
   - Creates roles: Admin, Merchant, Customer
   - Creates admin user
- Logs success message

3. **Login**: Navigate to `/Identity/Account/Login`
- Username: `Admin`
   - Password: `Admin123!@#`

### When Application Starts Subsequently:

- Checks if admin exists
- If exists: Logs "Default admin user already exists"
- If missing: Recreates the admin user
- No duplicate users created

---

## Logging

Check application logs for seeding status:

### Success Messages:
```
Created role: Admin
Created role: Merchant
Created role: Customer
Default admin user created successfully - Username: Admin
```

### If User Exists:
```
Default admin user already exists
```

### If Error Occurs:
```
Failed to create default admin user: [error details]
An error occurred while seeding the database
```

---

## Security Considerations

### Development
? Default credentials are acceptable for development
? Easy testing and demonstration

### Production
?? **CRITICAL SECURITY ACTIONS REQUIRED:**

1. **Change Default Password Immediately**
   - Login as Admin
   - Navigate to Profile ? Change Password
   - Use a strong, unique password

2. **Update Email Address**
   - Change to actual admin email
   - Enable email confirmation

3. **Enable Two-Factor Authentication**
   - Add 2FA for admin accounts
   - Require for sensitive operations

4. **Monitor Admin Access**
   - Log all admin login attempts
   - Alert on suspicious activity

5. **Consider Removing Auto-Seed in Production**
   - Comment out the seeding call
   - Or use environment-based logic:
   ```csharp
 if (app.Environment.IsDevelopment())
   {
 await SeedDefaultAdminUser(app);
}
   ```

---

## Customization

### Change Default Credentials

Edit `Program.cs` line 97-98:

```csharp
const string adminUsername = "Admin";        // Change username
const string adminPassword = "Admin123!@#";     // Change password
```

### Change Email

Edit `Program.cs` line 107:

```csharp
Email = "admin@deliveryservices.com",  // Change email
```

### Change Display Name

Edit `Program.cs` line 108:

```csharp
FullName = "System Administrator",     // Change full name
```

---

## Troubleshooting

### Admin User Not Created

**Check:**
1. Database connection is working
2. Migrations are applied
3. Check application logs for errors
4. Verify `AspNetUsers` table exists

**Solution:**
```bash
# Check migrations
dotnet ef migrations list --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web

# Apply migrations
dotnet ef database update --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web
```

### Password Doesn't Meet Requirements

If you change the password, ensure it meets requirements set in `Program.cs`:

```csharp
options.Password.RequireDigit = true;
options.Password.RequireLowercase = true;
options.Password.RequireUppercase = true;
options.Password.RequireNonAlphanumeric = true;
options.Password.RequiredLength = 6;
```

### Cannot Login

1. **Check Database**: Verify user exists in `AspNetUsers` table
2. **Check Role**: Verify user has Admin role in `AspNetUserRoles` table
3. **Check Password**: Ensure you're using exact password (case-sensitive)
4. **Clear Cookies**: Clear browser cookies and try again

### Multiple Admin Users Created

This should not happen due to the existence check. If it does:
1. Check logs for duplicate seeding calls
2. Verify `FindByNameAsync()` is working correctly
3. Delete duplicate users from database manually

---

## Database Verification

### Check Admin User Exists

```sql
-- Query AspNetUsers table
SELECT * FROM AspNetUsers WHERE UserName = 'Admin';

-- Check admin role assignment
SELECT u.UserName, u.Email, r.Name as RoleName
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.UserName = 'Admin';
```

---

## Testing

### Test Admin Login

1. Start application
2. Navigate to `/Identity/Account/Login`
3. Enter credentials:
   - Username: `Admin`
   - Password: `Admin123!@#`
4. Should redirect to `/Admin/Dashboard`
5. Verify admin menu is accessible

### Test Admin Permissions

After login, verify access to:
- ? `/Admin/Dashboard`
- ? `/Admin/Orders`
- ? `/Admin/Merchants`
- ? `/Admin/MerchantPayouts`

---

## Best Practices

### Development Environment
```csharp
// Keep auto-seeding enabled
await SeedDefaultAdminUser(app);
```

### Staging Environment
```csharp
// Keep enabled, use different credentials
if (app.Environment.IsStaging())
{
    await SeedDefaultAdminUser(app);
}
```

### Production Environment
```csharp
// Disable or use secure credentials
if (app.Environment.IsDevelopment())
{
    await SeedDefaultAdminUser(app);
}
// Or create admin manually through secure process
```

---

## Alternative Approaches

### Environment Variables

```csharp
var adminUsername = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "Admin";
var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin123!@#";
```

### Configuration File

```csharp
var adminUsername = builder.Configuration["AdminUser:Username"];
var adminPassword = builder.Configuration["AdminUser:Password"];
```

Add to `appsettings.json`:
```json
{
  "AdminUser": {
    "Username": "Admin",
    "Password": "Admin123!@#",
    "Email": "admin@deliveryservices.com"
  }
}
```

---

## Summary

? **Automatic Admin Creation**: No manual setup required
? **First Run Ready**: Works immediately after migration
? **Secure by Default**: Meets password complexity requirements
? **Logged**: All actions logged for audit trail
? **Safe**: Won't create duplicates
? **Customizable**: Easy to change credentials

?? **Remember**: Change default password in production!

---

**Quick Start:**
1. Run migrations
2. Start application
3. Login with `Admin` / `Admin123!@#`
4. Start managing your delivery service!

**Login URL**: `https://localhost:PORT/Identity/Account/Login`
