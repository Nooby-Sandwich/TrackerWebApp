using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrackerWebApp.Data;
using TrackerWebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// 1) Add DB context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// 2) Add Identity
builder.Services.AddDefaultIdentity<IdentityUser>(opts =>
    opts.SignIn.RequireConfirmedAccount = false
)
.AddEntityFrameworkStores<ApplicationDbContext>();

// 3) Add controllers + views
builder.Services.AddControllersWithViews();

// 4) Register CurrencyService with typed HttpClient + config
//builder.Services.AddHttpClient<ICurrencyService, CurrencyService>((sp, client) =>
//{
//    var config = sp.GetRequiredService<IConfiguration>();
//    var baseUrl = config["ExchangeRateHost:BaseUrl"];
//    client.BaseAddress = new Uri(baseUrl);
//});


var app = builder.Build();

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

// 6) Endpoint mapping
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
app.MapRazorPages();

app.Run();
