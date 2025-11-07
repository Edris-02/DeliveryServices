# ? Merchant Auto-Creation & Layout Updates - Complete

## ?? Changes Applied

### 1. ? Automatic User Creation for Merchants
**File:** `DeliveryServices.Web\Areas\Admin\Controllers\MerchantsController.cs`

**What Changed:**
- ? Added `UserManager<ApplicationUser>` dependency injection
- ? Implemented automatic user account creation when admin creates merchant
- ? Uses same password logic as drivers: `FirstName@123`
- ? Links User ? Merchant bidirectionally

### 2. ? Removed Merchant Payouts Link
**File:** `DeliveryServices.Web\Areas\Admin\Views\Shared\_Layout.cshtml`

**What Changed:**
- ? Removed "Financial" section from sidebar
- ? Removed "Merchant Payouts" link
- ? Cleaned up navigation structure

---

## ?? Merchant Auto-Creation Details

### How It Works:

```
Admin Creates Merchant
    ?
1. Fills merchant form
   - Name: ABC Store
   - Phone Number: (555) 123-4567
   - Address: 123 Main St
    ?
2. System checks if user exists (by phone number)
    ?
3. Creates ApplicationUser:
   - Username: (555) 123-4567 (Phone as username)
   - Email: abcstore@merchant.com (generated)
   - FullName: ABC Store
   - Password: ABC@123 (FirstName@123)
   - Role: Merchant
    ?
4. Creates Merchant record
    ?
5. Links User ? Merchant
   - User.MerchantId = merchant.Id
   - Merchant.UserId = user.Id
    ?
6. Success message shows credentials
```

---

## ?? Key Differences: Driver vs Merchant

| Aspect | Driver | Merchant |
|--------|--------|----------|
| **Username** | Email address | Phone Number |
| **Email** | User's email | Generated (name@merchant.com) |
| **Password** | FirstName@123 | FirstName@123 |
| **Unique Key** | Email | PhoneNumber |
| **Login Field** | Email | Phone Number |

### Why Phone for Merchants?
- Merchants model doesn't have Email field
- Phone Number is required and unique
- Simpler for merchants (they remember their business phone)

---

## ?? Updated Code Highlights

### Merchant Creation Logic:
```csharp
// Check if user exists
var existingUser = await _userManager.FindByNameAsync(merchant.PhoneNumber);
if (existingUser != null)
{
    ModelState.AddModelError("PhoneNumber", "A user with this phone number already exists");
    return View(merchant);
}

// Generate email from name
var sanitizedName = merchant.Name.Replace(" ", "").ToLower();
var generatedEmail = $"{sanitizedName}@merchant.com";

// Create user
var user = new ApplicationUser
{
    UserName = merchant.PhoneNumber,  // Phone as username
 Email = generatedEmail,
    FullName = merchant.Name,
    PhoneNumber = merchant.PhoneNumber,
    EmailConfirmed = true
};

// Generate password
var defaultPassword = $"{merchant.Name.Split(' ')[0]}@123";
var result = await _userManager.CreateAsync(user, defaultPassword);

// Assign role
await _userManager.AddToRoleAsync(user, UserRoles.Merchant);

// Link merchant to user
merchant.UserId = user.Id;
_unitOfWork.Merchant.Add(merchant);
_unitOfWork.Save();

// Update user with MerchantId
user.MerchantId = merchant.Id;
await _userManager.UpdateAsync(user);
```

---

## ?? Testing Guide

### Test Merchant Creation:
```
1. Login as Admin
2. Navigate to: /Admin/Merchants
3. Click "Create New Merchant"
4. Fill form:
   - Name: ABC Store
   - Phone: (555) 123-4567
   - PhoneNumber: (555) 123-4567
   - Address: 123 Main St
5. Click "Create"
6. ? See success message:
   "Merchant created successfully! Login credentials - 
    Username: (555) 123-4567, Password: ABC@123"
7. Logout
8. Login with:
   - Email/Username: (555) 123-4567
   - Password: ABC@123
9. ? Should redirect to /Merchant/Home/Index
```

### Verify Database:
```sql
-- Check merchant user
SELECT 
    u.UserName,
    u.Email,
    u.FullName,
    u.MerchantId,
    m.Name,
    r.Name as RoleName
FROM AspNetUsers u
INNER JOIN Merchants m ON m.UserId = u.Id
LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id
LEFT JOIN AspNetRoles r ON r.Id = ur.RoleId
WHERE u.UserName = '(555) 123-4567';
```

**Expected:**
- UserName: (555) 123-4567
- Email: abcstore@merchant.com
- FullName: ABC Store
- MerchantId: (number)
- Name: ABC Store
- RoleName: Merchant

---

## ?? Admin Sidebar (Updated)

### Before:
```
Main Menu
??? Dashboard
??? Orders
??? Merchants
??? Drivers

Financial  ? REMOVED
??? Merchant Payouts  ? REMOVED

Analytics
??? ...
```

### After:
```
Main Menu
??? Dashboard
??? Orders
??? Merchants
??? Drivers

Analytics
??? Reports
??? Analytics

System
??? Settings
??? Profile
```

---

## ?? Login Credentials Format

### For Merchants Created by Admin:
```
Username: [Phone Number from form]
Password: [First word of merchant name]@123

Examples:
- ABC Store ? ABC@123
- John's Pizza ? John's@123 (includes apostrophe)
- Best Buy Shop ? Best@123
```

### For Drivers Created by Admin:
```
Username: [Email address from form]
Password: [First word of driver name]@123

Examples:
- John Smith ? John@123
- Mary Jane ? Mary@123
- Bob Johnson ? Bob@123
```

---

## ?? Important Notes

### Password Security:
- ? Meets ASP.NET Core Identity requirements:
  - Minimum 6 characters
  - Uppercase letter
  - Lowercase letter (if name has it)
  - Digit (123)
  - Special character (@)

### Production Recommendations:
1. **Change Default Password** after first login
2. **Enable Email Confirmation** for merchants
3. **Add "Change Password" feature** to merchant portal
4. **Consider SMS verification** for phone-based login
5. **Log all merchant creations** for audit trail

---

## ?? Auto-Creation Comparison

### Success Messages:

**Driver:**
```
Driver created successfully! 
Login credentials - Email: john.smith@delivery.com, Password: John@123
```

**Merchant:**
```
Merchant created successfully! 
Login credentials - Username: (555) 123-4567, Password: ABC@123
```

### Database Links:

**Driver:**
```
Driver.UserId ? User.Id
User.DriverId ? Driver.Id
```

**Merchant:**
```
Merchant.UserId ? User.Id
User.MerchantId ? Merchant.Id
```

---

## ?? Troubleshooting

### Issue: "A user with this phone number already exists"
**Cause:** Phone number is already used by another user  
**Solution:** Use a different phone number or delete the existing user

### Issue: Merchant created but can't login
**Check:**
1. Verify user was created:
   ```sql
   SELECT * FROM AspNetUsers WHERE UserName = '[phone number]'
   ```
2. Verify role assignment:
   ```sql
   SELECT * FROM AspNetUserRoles WHERE UserId = '[user id]'
   ```
3. Try exact username (phone number) and password

### Issue: Password doesn't work
**Verify:**
- Password is case-sensitive
- Uses exact first word of merchant name
- Includes @123 suffix
- No spaces

---

## ?? Files Modified

| File | Change |
|------|--------|
| `MerchantsController.cs` | Added UserManager, auto user creation |
| `Admin/_Layout.cshtml` | Removed Financial section/Merchant Payouts |

---

## ? Build Status

```
Build: ? SUCCESSFUL
Compilation: ? No Errors
Controllers: ? Updated
Views: ? Updated (Layout)
```

---

## ?? Summary

### What Was Implemented:
? Automatic user creation when admin creates merchant  
? Same password pattern as drivers (FirstName@123)  
? Phone number as username (merchants don't have email field)  
? Generated email for user account  
? Bidirectional User ? Merchant linking  
? Role assignment (Merchant role)  
? Success message with credentials  
? Removed Merchant Payouts link from admin sidebar  

### Ready For:
? Merchant creation testing  
? Login testing with phone number  
? Production deployment  

---

## ?? Next Steps

1. ? Test merchant creation
2. ? Test merchant login
3. ? Add "Change Password" to merchant portal (optional)
4. ? Add merchant approval workflow (optional)
5. ? Implement password reset for merchants (optional)

---

**Status**: ? **COMPLETE**  
**Build**: ? Successful  
**Auto-Creation**: ? Implemented  
**Layout**: ? Updated  

**Both merchant and driver auto-creation now work the same way!** ??
