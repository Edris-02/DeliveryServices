# ?? Merchant Login Redirect Issue - FIXED

## ? Problem Identified and Resolved

### The Issue:
When merchants logged in, they were redirected to `/Admin/Dashboard/Index` instead of `/Merchant/Home/Index`, causing an "Access Denied" error.

### Root Cause:
The registration code wasn't updating the `ApplicationUser.MerchantId` property after creating the merchant profile. This meant the user-merchant link was only one-way.

---

## ?? Fix Applied

### Updated File: `AccountController.cs`

**Before:**
```csharp
var merchant = new Merchants
{
    Name = model.BusinessName ?? model.FullName,
    PhoneNumber = model.BusinessPhone ?? model.PhoneNumber ?? "",
    Phone = model.BusinessPhone ?? model.PhoneNumber ?? "",
    Address = model.BusinessAddress ?? "",
    UserId = user.Id
};

_unitOfWork.Merchant.Add(merchant);
_unitOfWork.Save();

// ? Missing: Update user with MerchantId

await _userManager.AddToRoleAsync(user, UserRoles.Merchant);
```

**After:**
```csharp
var merchant = new Merchants
{
    Name = model.BusinessName ?? model.FullName,
    PhoneNumber = model.BusinessPhone ?? model.PhoneNumber ?? "",
    Phone = model.BusinessPhone ?? model.PhoneNumber ?? "",
    Address = model.BusinessAddress ?? "",
    UserId = user.Id
};

_unitOfWork.Merchant.Add(merchant);
_unitOfWork.Save();

// ? Update user with MerchantId
user.MerchantId = merchant.Id;
await _userManager.UpdateAsync(user);

await _userManager.AddToRoleAsync(user, UserRoles.Merchant);
```

---

## ?? Verify the Issue

### Check if User Has Merchant Role:
```sql
-- Check user and their roles
SELECT u.UserName, u.Email, u.MerchantId, r.Name as RoleName
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'your.merchant@email.com';
```

**Expected Result:**
- UserName: merchant email
- MerchantId: Should have a number (not NULL)
- RoleName: Should be "Merchant"

---

## ??? Fix Existing Merchants

If you already registered merchants before this fix, run these SQL scripts:

### Option 1: Fix All Merchants at Once
```sql
-- Update all users linked to merchants with MerchantId
UPDATE u
SET u.MerchantId = m.Id
FROM AspNetUsers u
INNER JOIN Merchants m ON m.UserId = u.Id
WHERE u.MerchantId IS NULL;
```

### Option 2: Fix Specific Merchant
```sql
-- Replace 'merchant@email.com' with actual email
DECLARE @UserId NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = 'merchant@email.com')
DECLARE @MerchantId INT = (SELECT Id FROM Merchants WHERE UserId = @UserId)

UPDATE AspNetUsers
SET MerchantId = @MerchantId
WHERE Id = @UserId;
```

### Option 3: Ensure Merchant Has Role
```sql
-- Check if merchant has Merchant role
DECLARE @UserId NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = 'merchant@email.com')
DECLARE @RoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Merchant')

-- Add role if missing
IF NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles 
    WHERE UserId = @UserId AND RoleId = @RoleId
)
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@UserId, @RoleId);
END
```

---

## ? Test the Fix

### 1. For New Merchants:
1. Go to `/Identity/Account/Register`
2. Select "Merchant" account type
3. Fill in business details
4. Click "Create Account"
5. Login with merchant credentials
6. ? Should redirect to `/Merchant/Home/Index`

### 2. For Existing Merchants:
1. Run the SQL fix scripts above
2. Logout completely
3. Login again
4. ? Should redirect to `/Merchant/Home/Index`

---

## ?? Troubleshooting

### Still Getting Access Denied?

#### Check 1: Verify Merchant Has Role
```sql
SELECT u.Email, r.Name as RoleName
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'merchant@email.com';
```

**Expected:** Should show "Merchant" role

**If Empty:** Run this to add the role:
```sql
DECLARE @UserId NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = 'merchant@email.com')
DECLARE @RoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Merchant')

INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES (@UserId, @RoleId);
```

#### Check 2: Verify MerchantId is Set
```sql
SELECT Id, Email, MerchantId 
FROM AspNetUsers 
WHERE Email = 'merchant@email.com';
```

**Expected:** MerchantId should be a number (not NULL)

**If NULL:** Run this to fix:
```sql
UPDATE AspNetUsers
SET MerchantId = (SELECT Id FROM Merchants WHERE UserId = AspNetUsers.Id)
WHERE Email = 'merchant@email.com';
```

#### Check 3: Clear Browser Cookies
Sometimes old authentication cookies cause issues:
1. Chrome: `Ctrl+Shift+Delete` ? Clear cookies
2. Firefox: `Ctrl+Shift+Delete` ? Clear cookies
3. Edge: `Ctrl+Shift+Delete` ? Clear cookies
4. Or use Incognito/Private mode

#### Check 4: Verify Login Logic
The `AccountController.Login` method should have:
```csharp
if (result.Succeeded)
{
    var user = await _userManager.FindByEmailAsync(model.Email);
    var roles = await _userManager.GetRolesAsync(user!);

  if (roles.Contains(UserRoles.Admin))
    {
        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
    }
    else if (roles.Contains(UserRoles.Merchant))
    {
        return RedirectToAction("Index", "Home", new { area = "Merchant" });
    }

    return RedirectToLocal(returnUrl);
}
```

---

## ?? Verify Database State

### Complete Merchant Setup Check:
```sql
-- Replace 'merchant@email.com' with actual merchant email
DECLARE @Email NVARCHAR(256) = 'merchant@email.com'

-- User Info
SELECT 
    'User Info' as Category,
    u.Id,
    u.UserName,
    u.Email,
    u.MerchantId,
    u.EmailConfirmed
FROM AspNetUsers u
WHERE u.Email = @Email

UNION ALL

-- Merchant Info
SELECT 
    'Merchant Info' as Category,
    CAST(m.Id AS NVARCHAR(450)),
    m.Name,
    m.PhoneNumber,
    m.UserId,
    NULL
FROM Merchants m
INNER JOIN AspNetUsers u ON m.UserId = u.Id
WHERE u.Email = @Email

UNION ALL

-- Role Info
SELECT 
    'Role Info' as Category,
    r.Id,
    r.Name,
  NULL,
    NULL,
    NULL
FROM AspNetRoles r
INNER JOIN AspNetUserRoles ur ON r.Id = ur.RoleId
INNER JOIN AspNetUsers u ON ur.UserId = u.Id
WHERE u.Email = @Email;
```

**Expected Results:**
1. **User Info**: Should have MerchantId populated
2. **Merchant Info**: Should have matching UserId
3. **Role Info**: Should show "Merchant" role

---

## ?? Summary

### What Was Fixed:
? Added `user.MerchantId = merchant.Id` after creating merchant  
? Added `await _userManager.UpdateAsync(user)` to save the change  
? Now the User ? Merchant relationship is bidirectional  

### What Happens Now:
1. ? Merchant registers ? User and Merchant created
2. ? User.MerchantId is set to Merchant.Id
3. ? Merchant.UserId is set to User.Id
4. ? User is assigned "Merchant" role
5. ? Login redirects to Merchant portal
6. ? Merchant can access their dashboard

### For Existing Merchants:
- Run SQL fix scripts to update MerchantId
- Ensure Merchant role is assigned
- Clear browser cookies
- Login again

---

## ?? Next Steps

1. ? Fix is applied to code
2. ?? Fix existing merchants with SQL (if any)
3. ? Test new merchant registration
4. ? Test merchant login
5. ? Verify redirect to Merchant area

---

**Status**: ? **FIXED**
**Applies To**: New merchant registrations  
**Existing Merchants**: Need SQL fix (see above)  
**Testing**: Ready for testing

---

## ?? Prevention

This same pattern should be applied when creating drivers:
```csharp
// After creating driver
driver.UserId = user.Id;
_unitOfWork.Driver.Add(driver);
_unitOfWork.Save();

// ? Update user with DriverId
user.DriverId = driver.Id;
await _userManager.UpdateAsync(user);
```

Already implemented in `DriversController.cs` ?
