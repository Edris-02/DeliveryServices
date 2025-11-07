# Authentication & Multi-Area Implementation Guide

## Overview
This implementation adds complete authentication with ASP.NET Core Identity, role-based authorization, and separate portals for Admin and Merchant users.

---

## ?? Required Package Installation

Run these commands in the Package Manager Console or terminal:

```bash
# Add Identity package to Models project
dotnet add DeliveryServices.Models package Microsoft.Extensions.Identity.Stores

# Update database with new migration
dotnet ef migrations add AddIdentityAndUserMerchantLink --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web
dotnet ef database update --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web
```

---

## ?? Files Created

### Models Layer

1. **ApplicationUser.cs** - Custom user extending IdentityUser
   - `FullName` property
   - Optional `MerchantId` linking to Merchant entity
   
2. **UserRoles.cs** - Role constants
   - Admin
   - Merchant
- Customer

3. **ViewModels/LoginViewModel.cs** - Login form model
4. **ViewModels/RegisterViewModel.cs** - Registration form with merchant support

### Updated Files

1. **Merchants.cs** - Added `UserId` property to link to ApplicationUser
2. **ApplicationDbContext.cs** - Updated to use `IdentityDbContext<ApplicationUser>`
3. **Program.cs** - Configured Identity services and authentication

### Identity Area

**Controllers:**
- `AccountController.cs` - Handles login, register, logout, access denied

**Views:**
- `Login.cshtml` - Modern login page with gradient background
- `Register.cshtml` - Registration page with account type selection (Customer/Merchant)
- `AccessDenied.cshtml` - Access denied page
- `_ViewImports.cshtml` - Tag helpers and using statements

### Merchant Area

**Controllers:**
- `HomeController.cs` - Merchant dashboard, orders, payouts, profile

**Views:**
- `Index.cshtml` - Merchant dashboard with statistics
- `Orders.cshtml` - List of all merchant orders
- `Shared/_Layout.cshtml` - Dark sidebar layout for merchant portal
- `_ViewImports.cshtml`
- `_ViewStart.cshtml`

### Admin Area Updates

**Controllers** (Added `[Authorize(Roles = UserRoles.Admin)]`):
- `DashboardController.cs`
- `OrdersController.cs`
- `MerchantsController.cs`
- `MerchantPayoutsController.cs`

---

## ?? Authentication Flow

### Registration

1. User visits `/Identity/Account/Register`
2. Selects account type:
   - **Customer**: Standard user account
   - **Merchant**: Creates user + merchant profile
3. On submit:
   - Creates `ApplicationUser`
   - If Merchant: Creates `Merchants` record linked to user
   - Assigns appropriate role
   - Redirects to login

### Login

1. User visits `/Identity/Account/Login`
2. Enters email and password
3. On success, redirects based on role:
   - **Admin** ? `/Admin/Dashboard`
   - **Merchant** ? `/Merchant/Home`
   - **Customer** ? Default home

### Authorization

```csharp
// Admin controllers
[Authorize(Roles = UserRoles.Admin)]

// Merchant controllers
[Authorize(Roles = UserRoles.Merchant)]
```

---

## ?? Design Features

### Login/Register Pages

- **Gradient Background**: Purple to violet gradient
- **Modern Card Design**: Rounded corners, shadows
- **Brand Icon**: Centered logo with gradient background
- **Responsive**: Works on all devices
- **Toast Notifications**: Success/error messages

### Merchant Portal

**Dark Sidebar:**
- Color scheme: `#1a1d29` gradient
- Active link: Green (`#10b981`)
- Smooth animations
- User profile at bottom
- Navigation: Dashboard, Orders, Payouts, Profile

**Dashboard:**
- Statistics cards: Balance, Orders, Delivered, In Transit
- Revenue & Payouts summary
- Recent orders table
- Quick action cards

---

## ??? Database Changes

### ApplicationUser (AspNetUsers table)
```sql
- Id (string)
- UserName (string)
- Email (string)
- FullName (string) -- NEW
- MerchantId (int, nullable) -- NEW
- [Standard Identity fields...]
```

### Merchants Table
```sql
- UserId (string, nullable) -- NEW
  Foreign Key to AspNetUsers.Id
```

---

## ?? First-Time Setup

### 1. Create Admin User (Manual)

After running migrations, create an admin user manually:

```csharp
// Option A: Add this code temporarily to Program.cs (after app.Run())
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

// Ensure roles exist
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    // Create admin user
    var adminEmail = "admin@deliveryservices.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
     var admin = new ApplicationUser
        {
   UserName = adminEmail,
 Email = adminEmail,
        FullName = "System Administrator",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(admin, "Admin@123");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}
```

### 2. Test Registration

1. Navigate to `/Identity/Account/Register`
2. Create a merchant account
3. Login and verify access to merchant portal

---

## ?? Key Features

### Merchant Portal Features

? **Dashboard**
- Current balance display
- Order statistics
- Recent orders
- Revenue summary
- Payout history

? **Orders Management**
- View all orders for their business
- Filter by status
- View order details
- Track delivery status
- See item-level status (delivered/cancelled/pending)

? **Payouts**
- View payout history
- Track received payments
- See payout details

? **Profile**
- View business information
- Update contact details

### Admin Portal Features

? **Full Access**
- All merchants
- All orders
- Create payouts
- Manage users
- View analytics

---

## ??? Security Features

1. **Password Requirements**
   - Minimum 6 characters
- Requires uppercase
   - Requires lowercase
   - Requires digit

2. **Account Lockout**
   - 5 failed attempts
   - 5-minute lockout period

3. **Cookie Security**
   - 7-day expiration
   - Sliding expiration
   - Secure paths

4. **Role-Based Access**
   - Admin-only areas protected
   - Merchant-only areas protected
   - Redirect to appropriate portal on login

---

## ?? Navigation Routes

### Identity Area
- `/Identity/Account/Login` - Login page
- `/Identity/Account/Register` - Registration
- `/Identity/Account/Logout` - Logout (POST)
- `/Identity/Account/AccessDenied` - Access denied

### Merchant Area
- `/Merchant/Home/Index` - Dashboard
- `/Merchant/Home/Orders` - Orders list
- `/Merchant/Home/OrderDetails/{id}` - Order details
- `/Merchant/Home/Payouts` - Payout history
- `/Merchant/Home/Profile` - Business profile

### Admin Area
- `/Admin/Dashboard` - Dashboard
- `/Admin/Orders` - Orders management
- `/Admin/Merchants` - Merchants management
- `/Admin/MerchantPayouts` - Payouts management

---

## ?? Troubleshooting

### "Type 'IdentityUser' could not be found"
**Solution**: Install `Microsoft.Extensions.Identity.Stores` in Models project

### "Login redirects to wrong area"
**Solution**: Check role assignment in database (AspNetUserRoles table)

### "Merchant dashboard shows no data"
**Solution**: Ensure user has MerchantId set and linked correctly

### "Access Denied after login"
**Solution**: 
- Check user has correct role
- Verify `[Authorize]` attributes
- Clear browser cookies and re-login

---

## ?? Testing Checklist

- [ ] Admin can login and access all admin pages
- [ ] Merchant can register and create account
- [ ] Merchant can login and see their dashboard
- [ ] Merchant sees only their own orders
- [ ] Merchant sees correct balance
- [ ] Unauthorized users redirected to login
- [ ] Logout works correctly
- [ ] Password validation works
- [ ] Account lockout after failed attempts
- [ ] Role-based redirection works

---

## ?? Migration Commands

```bash
# Create migration
dotnet ef migrations add AddIdentityAndUserMerchantLink --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web

# Apply migration
dotnet ef database update --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web

# Rollback if needed
dotnet ef database update PreviousMigrationName --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web
```

---

## ?? Next Steps

1. **Install Package**: `Microsoft.Extensions.Identity.Stores` in Models project
2. **Run Migration**: Create and apply database migration
3. **Create Admin**: Use code snippet to create first admin user
4. **Test Registration**: Create a merchant account
5. **Test Login**: Verify role-based redirection
6. **Test Permissions**: Try accessing admin/merchant areas

---

## ?? Notes

- Merchants are automatically linked to their user account on registration
- Only delivered items count toward merchant balance
- Admins can create merchants without user accounts (UserId will be null)
- Merchants created via admin panel cannot login until a user account is linked
- Email confirmation is disabled by default (set to true for production)

---

## ?? Security Recommendations for Production

1. Enable email confirmation
2. Implement two-factor authentication
3. Use stronger password requirements
4. Implement CAPTCHA on registration
5. Add password reset functionality
6. Implement email verification
7. Add activity logging
8. Implement session timeout
9. Use HTTPS only
10. Implement rate limiting

---

**Status**: ? Implementation Complete
**Build Status**: ?? Requires package installation
**Migration Status**: ?? Requires database migration
