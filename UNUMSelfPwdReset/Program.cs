using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using UNUMSelfPwdReset;
using UNUMSelfPwdReset.Managers;
using Microsoft.AspNetCore.Session;

var builder = WebApplication.CreateBuilder(args);

var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ') ?? builder.Configuration["MicrosoftGraph:Scopes"]?.Split(' ');

var obj = builder.Configuration.GetSection("AzureAd");
// Add services to the container.
IConfigurationSection azureAdSection = builder.Configuration.GetSection("AzureAd");

//azureAdSection.GetSection("Instance").Value = "https://login.microsoftonline.com/";
//azureAdSection.GetSection("Domain").Value = "adventglobal.com";
//azureAdSection.GetSection("TenantId").Value = "0951bc8d-d7bc-40d0-9668-9119a55ad78c";
azureAdSection.GetSection("ClientId").Value = Environment.GetEnvironmentVariable("CLIENT_ID");
azureAdSection.GetSection("ClientSecret").Value = Environment.GetEnvironmentVariable("CLIENT_SECRET");
builder.Configuration.GetSection("AdminCreds:UserName").Value = Environment.GetEnvironmentVariable("ADMIN_USERNAME");
builder.Configuration.GetSection("AdminCreds:Password").Value = Environment.GetEnvironmentVariable("ADMIN_PWD");

//services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd");
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration, "AzureAd")
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
            .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
            .AddInMemoryTokenCaches();

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));

   
});
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();
builder.Services.AddScoped<LoginsManager>();
builder.Services.AddScoped<PasswordResetService>();
builder.Services.AddScoped<AzureAdminActionManager>();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "UNUM.AdventureWorks.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(7200);
    options.Cookie.IsEssential = true;
});


var app = builder.Build();
app.UseExceptionHandler("/Home/Error");
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
