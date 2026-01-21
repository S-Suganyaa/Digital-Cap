using DigitalCap.Core.Models;
using DigitalCap.Infrastructure.DbContext;
using DigitalCap.Persistence.Extensions;
using DigitalCap.WebApi.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddPersistenceServices(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// Add Identity FIRST (this registers Identity.Application, Identity.External, etc.)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add Azure AD Authentication
// Identity.Application is already registered by AddIdentity, so don't add it again
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Identity.Application"; // Use Identity's default
    options.DefaultChallengeScheme = "AzureAD"; // Challenge with Azure AD
})
.AddOpenIdConnect("AzureAD", options =>
{
    // Read Azure AD settings from configuration
    var adInstance = configuration["Instance"] ?? "https://login.microsoftonline.com/";
    var adTenantId = configuration["TenantId"];
    var adClientId = configuration["ClientId"];
    var adClientSecret = configuration["ClientSecret"];
    var callbackPath = configuration["CallbackPath"] ?? "/signin-oidc";
    
    //// Get base URL for redirect URI
    //var baseUrl = configuration["BaseUrl"] ?? 
    //              (builder.Environment.IsDevelopment() 
    //                  ? "https://localhost:7192" 
    //                  : configuration["URL"] ?? "https://localhost:7192");

    // Validate required settings
    if (string.IsNullOrEmpty(adTenantId) || string.IsNullOrEmpty(adClientId))
    {
        throw new InvalidOperationException(
            "Azure AD configuration is missing. Please ensure ClientId and TenantId are set in appsettings.json.");
    }

    options.Authority = $"{adInstance.TrimEnd('/')}/{adTenantId}";
    options.ClientId = adClientId;
    
  
    options.ResponseType = "code";
    options.CallbackPath = callbackPath;
    options.SignedOutCallbackPath = "/signout-oidc";
    options.SaveTokens = true;

    // Add scopes
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");

    // Map claims
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "role"
    };

    // Log the redirect URI for debugging (so you know what to register in Azure AD)
  //  var redirectUri = $"{baseUrl.TrimEnd('/')}{callbackPath}";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowReact");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// IMPORTANT: UseAuthentication must be before UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();