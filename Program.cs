using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrackerWebApp.Data;

var builder = WebApplication.CreateBuilder(args);

// 1) EF Core + SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 2) Identity core services + default UI (Razor Pages)
builder.Services
    .AddDefaultIdentity<IdentityUser>(opts => opts.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 3) MVC controllers + views
builder.Services.AddControllersWithViews();

// 4) Razor Pages needed for Identity UI
builder.Services.AddRazorPages();

var app = builder.Build();


//Seed on startup
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.SeedAsync(scope.ServiceProvider);
}


// 5) Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// 6) Map both MVC controllers and Razor Pages (for Identity UI)
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
