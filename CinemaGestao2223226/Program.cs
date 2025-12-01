using System;
using System.Globalization;
using System.Threading.Tasks;
using CinemaGestao2223226.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()                                   // <-- add this
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Configure localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Configure supported cultures
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("pt-PT")
    };

    options.DefaultRequestCulture = new RequestCulture("pt-PT");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Use cookie to persist language preference
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Add localization middleware
app.UseRequestLocalization();

app.UseRouting();

// IMPORTANT: auth then authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

await SeedDataAsync(app);
app.Run();

static async Task SeedDataAsync(IHost host)
{
    using var scope = host.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        Console.WriteLine("Seeding started");  // <--- add this

        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        // Roles
        string[] roles = { "Administrador", "Cliente" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                Console.WriteLine($"Creating role {role}");  // <---
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Admin user
        // Admin user
        var adminEmail = "admin@cinema.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser != null)
        {
            // Delete existing admin so we can recreate with known password
            Console.WriteLine("Deleting existing admin user...");
            await userManager.DeleteAsync(adminUser);
            adminUser = null;
        }

        if (adminUser == null)
        {
            Console.WriteLine("Creating admin user...");
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrador");
                Console.WriteLine("Admin user created and added to role");
            }
            else
            {
                Console.WriteLine("Failed to create admin user:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"{error.Code}: {error.Description}");
                }
            }
        }
        else
        {
            Console.WriteLine("Admin user already exists");  // <---
            if (!await userManager.IsInRoleAsync(adminUser, "Administrador"))
            {
                await userManager.AddToRoleAsync(adminUser, "Administrador");
                Console.WriteLine("Admin added to role Administrador");  // <---
            }
        }

        Console.WriteLine("Seeding finished");  // <---
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding data: {ex}");
    }
}
