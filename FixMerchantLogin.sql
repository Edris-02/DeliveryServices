-- ================================================
-- FIX MERCHANT LOGIN REDIRECT ISSUE
-- This script fixes existing merchants that were
-- created before the MerchantId update fix
-- ================================================

USE [YourDatabaseName]; -- Replace with your actual database name
GO

PRINT '========================================';
PRINT 'Starting Merchant Login Fix';
PRINT '========================================';
PRINT '';

-- ================================================
-- STEP 1: Check Current State
-- ================================================
PRINT 'STEP 1: Checking current merchant status...';
PRINT '';

SELECT 
    u.Email,
    u.MerchantId,
    m.Id as ActualMerchantId,
    CASE 
   WHEN u.MerchantId IS NULL THEN 'NEEDS FIX'
        WHEN u.MerchantId = m.Id THEN 'OK'
        ELSE 'MISMATCH'
    END as Status
FROM AspNetUsers u
INNER JOIN Merchants m ON m.UserId = u.Id
WHERE u.MerchantId IS NULL OR u.MerchantId != m.Id;

PRINT '';
PRINT '========================================';

-- ================================================
-- STEP 2: Update MerchantId for All Merchants
-- ================================================
PRINT 'STEP 2: Updating MerchantId for all merchants...';
PRINT '';

UPDATE u
SET u.MerchantId = m.Id
FROM AspNetUsers u
INNER JOIN Merchants m ON m.UserId = u.Id
WHERE u.MerchantId IS NULL OR u.MerchantId != m.Id;

PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' user(s) updated with MerchantId';
PRINT '';
PRINT '========================================';

-- ================================================
-- STEP 3: Ensure Merchant Role Exists
-- ================================================
PRINT 'STEP 3: Ensuring Merchant role exists...';
PRINT '';

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Merchant')
BEGIN
INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (
        NEWID(),
      'Merchant',
   'MERCHANT',
        NEWID()
    );
    PRINT 'Merchant role created';
END
ELSE
BEGIN
    PRINT 'Merchant role already exists';
END

PRINT '';
PRINT '========================================';

-- ================================================
-- STEP 4: Assign Merchant Role to All Merchants
-- ================================================
PRINT 'STEP 4: Assigning Merchant role to all merchant users...';
PRINT '';

DECLARE @MerchantRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Merchant');

INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, @MerchantRoleId
FROM AspNetUsers u
INNER JOIN Merchants m ON m.UserId = u.Id
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles ur 
    WHERE ur.UserId = u.Id AND ur.RoleId = @MerchantRoleId
);

PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' merchant(s) assigned Merchant role';
PRINT '';
PRINT '========================================';

-- ================================================
-- STEP 5: Verify Fix
-- ================================================
PRINT 'STEP 5: Verifying all merchants are fixed...';
PRINT '';

SELECT 
    u.Email,
    u.FullName,
  u.MerchantId,
    m.Id as MerchantTableId,
    m.Name as BusinessName,
    r.Name as AssignedRole,
    CASE 
        WHEN u.MerchantId = m.Id AND r.Name = 'Merchant' THEN '? FIXED'
        WHEN u.MerchantId IS NULL THEN '? Missing MerchantId'
        WHEN r.Name IS NULL THEN '? Missing Role'
  ELSE '? OTHER ISSUE'
 END as FixStatus
FROM AspNetUsers u
INNER JOIN Merchants m ON m.UserId = u.Id
LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id
LEFT JOIN AspNetRoles r ON r.Id = ur.RoleId AND r.Name = 'Merchant'
ORDER BY u.Email;

PRINT '';
PRINT '========================================';
PRINT 'Merchant Login Fix Complete!';
PRINT '========================================';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Review the results above';
PRINT '2. Ask merchants to logout and login again';
PRINT '3. Clear browser cookies if issues persist';
PRINT '4. Check that merchants redirect to /Merchant/Home/Index';
PRINT '';

-- ================================================
-- OPTIONAL: Test Specific Merchant
-- ================================================
PRINT '========================================';
PRINT 'Optional: Test Specific Merchant';
PRINT '========================================';
PRINT '';
PRINT 'To test a specific merchant, run this query:';
PRINT '';
PRINT 'DECLARE @TestEmail NVARCHAR(256) = ''merchant@example.com'';';
PRINT '';
PRINT 'SELECT ';
PRINT '    u.Email,';
PRINT '    u.MerchantId,';
PRINT '  m.Id as MerchantTableId,';
PRINT '    r.Name as Role';
PRINT 'FROM AspNetUsers u';
PRINT 'LEFT JOIN Merchants m ON m.UserId = u.Id';
PRINT 'LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id';
PRINT 'LEFT JOIN AspNetRoles r ON r.Id = ur.RoleId';
PRINT 'WHERE u.Email = @TestEmail;';
PRINT '';
