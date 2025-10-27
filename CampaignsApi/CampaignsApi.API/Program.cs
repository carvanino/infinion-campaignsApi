using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using CampaignsApi.Infrastructure.Data;
using CampaignsApi.Application.Interfaces;
using CampaignsApi.Application.Services;
using CampaignsApi.Application.Validators;
using CampaignsApi.Infrastructure.Data.Repositories;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;



var builder = WebApplication.CreateBuilder(args);

//DB Config
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Authentication configurations
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateCampaignValidator>();
builder.Services.AddFluentValidationAutoValidation();

// JSON configurations
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    // Override the default validation response
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => char.ToLowerInvariant(kvp.Key[0]) + kvp.Key.Substring(1), // Convert to camelCase
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var response = new
        {
            status = 400,
            message = "Validation failed",
            errors = errors
        };

        return new BadRequestObjectResult(response);
    };
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;

    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});


// Swagger/OpenAPI configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Campaigns API",
        Version = "v1",
        Description = "Simple RESTful API for managing marketing campaigns with AzureAD authentication",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "akinolatofunmi.tech@gmail.com"
        }

    });

    // Configure OAuth2 with Azure AD
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            // Using Authorization Code flow with PKCE (most secure for SPAs)
            AuthorizationCode = new OpenApiOAuthFlow
            {
                // Azure AD authorization endpoint (multitenant)
                AuthorizationUrl = new Uri("https://login.microsoftonline.com/common/oauth2/v2.0/authorize"),
                
                // Azure AD token endpoint (multitenant)
                TokenUrl = new Uri("https://login.microsoftonline.com/common/oauth2/v2.0/token"),
                
                // Scopes - what permissions the token will have
                Scopes = new Dictionary<string, string>
                {
                    { "api://f50fb135-a878-46ae-a516-fd167f8183ec/access_as_user", "Access Campaigns API as user" }
                }
            }
        }
    });
    
    // Make all endpoints require authentication
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { "api://f50fb135-a878-46ae-a516-fd167f8183ec/access_as_user" }
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5069", "https://localhost:7137")
              .AllowAnyMethod()
              .AllowAnyHeader() 
              .AllowCredentials(); 
    });
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Campaigns API v1");
        options.RoutePrefix = "swagger";

        options.OAuthClientId("f50fb135-a878-46ae-a516-fd167f8183ec");  // Your Client ID
        options.OAuthUsePkce();  // Use PKCE for security (no client secret needed)
        options.OAuthScopeSeparator(" ");  // Space-separated scopes
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .AllowAnonymous();

// Root endpoint
app.MapGet("/", () => Results.Redirect("/swagger"))
   .AllowAnonymous();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        dbContext.Database.Migrate();
        app.Logger.LogInformation("Database migration completed successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while migrating the database");
    }
}

app.Logger.LogInformation("Starting Campaigns API");
app.Run();

public partial class Program { }
