# ? MERCHANT LOGIN REDIRECT - FIXED

## ?? Problem
Merchants were redirected to `/Admin/Dashboard/Index` instead of `/Merchant/Home/Index`, causing "Access Denied" error.

## ? Solution Applied

### Code Fix (AccountController.cs)
Added missing line to update user's MerchantId after creating merchant:
```csharp
_unitOfWork.Merchant.Add(merchant);
_unitOfWork.Save();

// ? NEW: Update user with MerchantId
user.MerchantId = merchant.Id;
await _userManager.UpdateAsync(user);
```

### For Existing Merchants
Run the SQL script: `FixMerchantLogin.sql`

Or run this quick fix:
```sql
-- Update all merchants
UPDATE u
SET u.MerchantId = m.Id
FROM AspNetUsers u
INNER JOIN Merchants m ON m.UserId = u.Id
WHERE u.MerchantId IS NULL;

-- Ensure they have Merchant role
DECLARE @RoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Merchant');

INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, @RoleId
FROM AspNetUsers u
INNER JOIN Merchants m ON m.UserId = u.Id
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles ur 
    WHERE ur.UserId = u.Id AND ur.RoleId = @RoleId
);
```

## ?? Testing

### 1. For New Merchants:
- Register as merchant
- Login
- ? Should redirect to `/Merchant/Home/Index`

### 2. For Existing Merchants:
- Run SQL fix
- Logout completely
- Clear browser cookies
- Login again
- ? Should redirect to `/Merchant/Home/Index`

## ?? Checklist

- [x] Updated AccountController.cs
- [ ] Run SQL fix for existing merchants
- [ ] Test merchant login
- [ ] Verify redirect works

## ?? Documentation

See `MERCHANT_LOGIN_REDIRECT_FIX.md` for complete details.
