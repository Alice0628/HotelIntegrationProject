using Microsoft.AspNetCore.Identity;
using MotelBkApp.Data;
using MotelBkApp.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
//var connectionString = builder.Configuration.GetConnectionString("MotelDbContextConnection") ?? throw new InvalidOperationException("Connection string 'MotelDbContextConnection' not found.");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<MotelDbContext>();

//builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<MotelDbContext>();


builder.Services.AddIdentity<AppUser, AppRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<MotelDbContext>()
    .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider)
     .AddDefaultUI()
    .AddRoles<AppRole>();

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

app.UseAuthentication();
app.UseAuthorization();
using (var scope = app.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<MotelDbContext>();

    context.Database.EnsureCreated();
    //context.Database.Migrate();

    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    IdentityDataSeed.InitializeAsync(userManager, roleManager);
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
