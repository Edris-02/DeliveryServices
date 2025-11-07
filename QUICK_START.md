# ? Quick Reference Card

## ?? What You Need To Do Right Now

### 1. Install Package (REQUIRED)
```bash
dotnet add DeliveryServices.Models\DeliveryServices.Models.csproj package Microsoft.Extensions.Identity.Stores
```

### 2. Create & Run Migration (REQUIRED)
```bash
# Create migration
dotnet ef migrations add AddIdentityAndUserMerchantLink --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web

# Apply to database
dotnet ef database update --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web
```

### 3. Run the Application
```bash
dotnet run --project DeliveryServices.Web
```

**?? That's it! Admin user is automatically created on first run!**

---

## ?? Default Login Credentials

### Admin Account (Auto-Created)
- **Email**: `admin@deliveryservices.com`
- **Password**: `Admin123!@#`

**?? IMPORTANT**: 
- Login field accepts **Email** (Username and Email are the same)
- Change this password after first login in production!

---

## ?? Important URLs

| Page | URL | Who Can Access |
|------|-----|----------------|
| Login | `/Identity/Account/Login` | Everyone |
| Register | `/Identity/Account/Register` | Everyone |
| Admin Dashboard | `/Admin/Dashboard` | Admin only |
| Merchant Dashboard | `/Merchant/Home` | Merchant only |
| Admin Orders | `/Admin/Orders` | Admin only |
| Merchant Orders | `/Merchant/Home/Orders` | Merchant only |

---

## ?? What Was Created

### Identity System
? Login page with gradient design
? Register page with account type toggle
? Access denied page
? Role-based authentication
? Automatic merchant profile creation
? **Automatic admin user creation** ? NEW

### Merchant Portal
? Dark sidebar (green accent)
? Dashboard with statistics
? Orders management
? Payout history
? Business profile
? User avatar & profile

### Admin Portal
? Authorization added to all controllers
? Existing dark sidebar
? Full management capabilities
? **Auto-seeded admin account** ? NEW

---

## ? Quick Test

### Test Admin Login
1. Run migrations
2. Start application
3. Go to `/Identity/Account/Login`
4. Login with:
   - **Email**: `admin@deliveryservices.com`
   - **Password**: `Admin123!@#`
5. Should see admin dashboard ?

### Test Merchant Registration
1. Go to `/Identity/Account/Register`
2. Select "Merchant"
3. Enter business details
4. Register
5. Login
6. Should see merchant dashboard ?

---

## ?? If Something Goes Wrong

### Build Errors
- Install the package: `Microsoft.Extensions.Identity.Stores`
- Clean and rebuild: `dotnet clean && dotnet build`

### Migration Errors
- Check connection string in `appsettings.json`
- Ensure SQL Server is running
- Remove bad migration: `dotnet ef migrations remove`

### Admin User Not Created
- Check application logs for errors
- Verify migrations were applied
- Check `AspNetUsers` table in database
- See `ADMIN_USER_SEEDING.md` for troubleshooting

### Login Not Working
- **Use email**: `admin@deliveryservices.com` (not "Admin")
- Ensure password is exactly: `Admin123!@#` (case-sensitive)
- Check roles were created (AspNetRoles table)
- Verify user has role (AspNetUserRoles table)
- Clear browser cookies

### Merchant Dashboard Empty
- Ensure merchant account was created during registration
- Check Merchants table has UserId linked
- Try creating test orders in admin panel

---

## ?? Database Tables to Check

After migration, verify these tables exist:
- ? AspNetUsers (should contain admin@deliveryservices.com)
- ? AspNetRoles (should contain Admin, Merchant, Customer)
- ? AspNetUserRoles (should link admin user to Admin role)
- ? Merchants (with UserId column)
- ? Orders
- ? OrderItems
- ? MerchantPayouts

---

## ?? Success Criteria

You'll know it works when:
1. ? Can login with email: admin@deliveryservices.com
2. ? Admin login redirects to `/Admin/Dashboard`
3. ? Can register a merchant account
4. ? Merchant login redirects to `/Merchant/Home`
5. ? Merchant dashboard shows statistics
6. ? Admin can access all admin pages
7. ? Unauthorized access shows Access Denied

---

## ?? What Happens on First Run

### Automatic Setup:
1. ? Creates all roles (Admin, Merchant, Customer)
2. ? Creates admin user (admin@deliveryservices.com / Admin123!@#)
3. ? Assigns Admin role to admin user
4. ? Logs all actions for verification

### Check Logs:
Look for these messages in console:
```
Created role: Admin
Created role: Merchant
Created role: Customer
Default admin user created successfully - Username: admin@deliveryservices.com
```

---

## ?? Next Steps After Testing

1. Login as Admin
2. Create some test merchants
3. Create some test orders
4. Test payout creation
5. Register a merchant account
6. Login as merchant
7. Verify merchant sees only their data
8. Test logout
9. **Change admin password** (important!)

---

## ?? Tips

- Use private/incognito mode to test different roles
- Check browser console for JavaScript errors
- Check application logs for authentication errors
- Test on different browsers
- Test responsive design on mobile
- **Login with EMAIL, not a separate username**
- **Don't forget to change admin password in production!**

---

## ?? Security Reminder

### For Production:
1. ?? **Change admin password immediately**
2. ?? Change admin email to real address
3. ?? Consider disabling auto-seed in production
4. ?? Enable email confirmation
5. ?? Enable two-factor authentication
6. ?? Monitor admin access logs

### See `ADMIN_USER_SEEDING.md` for:
- Security best practices
- Production deployment guide
- Password change instructions
- Advanced configuration options

---

**Need Help?** Check:
- `ADMIN_USER_SEEDING.md` - Admin user details ? NEW
- `AUTHENTICATION_IMPLEMENTATION_GUIDE.md` - Full details
- `SETUP_COMMANDS.md` - Command reference
- `IMPLEMENTATION_SUMMARY.md` - What was built

**Ready to Go!** ??
Just run the three commands above!
No manual admin creation needed!

**Important:** Login with **email** `admin@deliveryservices.com`, not username "Admin"
