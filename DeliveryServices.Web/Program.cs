using DeliveryServices.DataAccess.Data;
using DeliveryServices.DataAccess.Repository;
using DeliveryServices.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DeliveryService")));

// Register UnitOfWork for dependency injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Configure culture for no currency symbol and yyyy/MM/dd date format
var cultureInfo = new CultureInfo("en-US");

// Remove currency symbol - just show numbers
cultureInfo.NumberFormat.CurrencySymbol = "";
cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;

// Set date format to yyyy/MM/dd
cultureInfo.DateTimeFormat.ShortDatePattern = "yyyy/MM/dd";
cultureInfo.DateTimeFormat.LongDatePattern = "yyyy/MM/dd";

var supportedCultures = new[] { cultureInfo };

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(cultureInfo);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

// Use request localization
app.UseRequestLocalization();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Admin}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
