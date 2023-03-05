using Microsoft.AspNetCore.Identity;
using MotelBookingApp.Data;
using MotelBookingApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<MotelDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found."));
});

//user Secrets
var secretEmail = builder.Configuration["MyGmail"];
var secretSMTP = builder.Configuration["SMTP"];
 

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services
    .AddAuthentication()
    .AddCookie()
    .AddGoogle(o =>
    {
        o.ClientId = "41243224755-9etbgr6l5asi9vvh2h4rmjqg4b618crg.apps.googleusercontent.com";
        o.ClientSecret = "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";
        // o.ClientId = "373602987509-ju7nf1ln5m18mhef8k43m4aehmaeu2ad.apps.googleusercontent.com";
        // o.ClientSecret = "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";
        o.SignInScheme = IdentityConstants.ExternalScheme;
        // o.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;;
    });

builder.Services.AddIdentity<AppUser, AppRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<MotelDbContext>()
    .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider)
    .AddRoles<AppRole>();


// Stripe Settings
//read Values from appsettings.json
var secretKeyValue = builder.Configuration["SecretKey"];
// Debug.WriteLine("SecretKey:" + secretKeyValue);
StripeConfiguration.ApiKey = secretKeyValue;




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<MotelDbContext>();

    context.Database.Migrate();

    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    IdentityDataSeed.InitializeAsync(userManager, roleManager);
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.UseWebSockets();

app.Run();
