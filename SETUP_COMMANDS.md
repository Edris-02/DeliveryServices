# Quick Setup Commands

## Step 1: Install Required Package

```bash
dotnet add DeliveryServices.Models\DeliveryServices.Models.csproj package Microsoft.Extensions.Identity.Stores
```

## Step 2: Create Database Migration

```bash
dotnet ef migrations add AddIdentityAndUserMerchantLink --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web --context ApplicationDbContext
```

## Step 3: Update Database

```bash
dotnet ef database update --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web --context ApplicationDbContext
```

## Step 4: Create First Admin User (Optional - Add to Program.cs temporarily)

Add this code before `app.Run();` in Program.cs:

```csharp
// Seed admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Ensure roles exist
    string[] roles = { "Admin", "Merchant", "Customer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
 }
    }

    // Create admin user
    var adminEmail = "admin@delivery.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
{
       UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Administrator",
            EmailConfirmed = true
        };
        
        var result = await userManager.CreateAsync(admin, "Admin@123");
        if (result.Succeeded)
        {
     await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
```

## Step 5: Build and Run

```bash
dotnet build
dotnet run --project DeliveryServices.Web
```

## Step 6: Test

1. Navigate to: `https://localhost:xxxx/Identity/Account/Login`
2. Login with:
   - Email: `admin@delivery.com`
   - Password: `Admin@123`

3. Or register a new merchant account at: `https://localhost:xxxx/Identity/Account/Register`

---

## Troubleshooting

### If migration fails:
```bash
# Remove last migration
dotnet ef migrations remove --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web

# Try again
dotnet ef migrations add AddIdentityAndUserMerchantLink --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web
```

### If database update fails:
```bash
# Check existing migrations
dotnet ef migrations list --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web

# Drop database and recreate (WARNING: Deletes all data)
dotnet ef database drop --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web
dotnet ef database update --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web
```

---

## Quick Test Checklist

- [ ] Package installed successfully
- [ ] Migration created without errors
- [ ] Database updated successfully
- [ ] Application builds without errors
- [ ] Can access login page
- [ ] Can register merchant account
- [ ] Can login with merchant account
- [ ] Redirected to merchant dashboard
- [ ] Merchant can view their orders
- [ ] Admin can login (if seeded)
- [ ] Admin can access admin dashboard
