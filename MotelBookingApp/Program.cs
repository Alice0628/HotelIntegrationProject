using Microsoft.AspNetCore.Identity;
using MotelBookingApp.Data;
using MotelBookingApp.Models;
using Microsoft.EntityFrameworkCore;
using MotelBookingApp.Iservice;
using MotelBookingApp.Service;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.S3;
using Amazon;
using Amazon.DynamoDBv2.DataModel;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

//builder.Services.AddDbContext<MotelDbContext>();

//user Secrets
var secretEmail = builder.Configuration["MyGmail"];
var secretSMTP = builder.Configuration["SMTP"];
builder.Services.AddTransient<IAdminMotelService, AdminMotelService>();
builder.Services.AddTransient<IAdminRoomTypeService, AdminRoomTypeService>();
builder.Services.AddTransient<IStaffRoomService, StaffRoomService>();
builder.Services.AddScoped<IAdminMotelService, AdminMotelService>();
builder.Services.AddScoped<IAdminRoomTypeService, AdminRoomTypeService>();
builder.Services.AddScoped<IStaffRoomService, StaffRoomService>();
var awsAccessKey = builder.Configuration.GetValue<string>("AWS:AccessKey");
var awsSecretKey = builder.Configuration.GetValue<string>("AWS:SecretKey");
var credentials = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
var config = new AmazonDynamoDBConfig { RegionEndpoint = RegionEndpoint.USEast2 };
var client = new AmazonDynamoDBClient(credentials, config);
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddSingleton(new MotelDbContext(client, new DynamoDBContextConfig()));
 


//builder.Services.AddIdentity<AppUser, AppRole>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<MotelDbContext>()
//    .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider)
//    .AddRoles<AppRole>();

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
//using (var scope = app.Services.CreateAsyncScope())
//{
//    var services = scope.ServiceProvider;
//    var context = services.GetRequiredService<MotelDbContext>();

//    //context.Database.EnsureCreated();

//    //context.Database.Migrate();

//    var userManager = services.GetRequiredService<UserManager<AppUser>>();
//    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
//    IdentityDataSeed.InitializeAsync(userManager, roleManager);
//}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
