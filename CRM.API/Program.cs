using System.Text;
using System.Text.Json;
using CRM.API.Hubs;
using CRM.API.Middleware;
using CRM.API.Services;
using CRM.Application.Services.Classes;
using CRM.Application.Services.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories.Classes;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ─── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy        = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// ─── Swagger with SoftOnCloud JWT bearer ──────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "CRM API",
        Version = "v1",
        Description = "SoftOnCloud CRM — React → .NET → PostgreSQL fn_* only"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "SoftOnCloud JWT from POST https://api.softoncloud.com/api/auth/login. Example: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ─── SoftOnCloud HTTP client (product-connection, session) ────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("SoftOnCloud", client =>
{
    var baseUrl = builder.Configuration["SoftOnCloud:ApiBaseUrl"] ?? "https://api.softoncloud.com";
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<ITenantConnectionFactory, SoftOnCloudTenantConnectionFactory>();
builder.Services.AddSingleton<SoftOnCloudSessionValidator>();
builder.Services.AddSingleton<SessionRevocationStore>();
builder.Services.AddSingleton<ISessionForceLogoutNotifier, SessionForceLogoutNotifier>();
builder.Services.AddSignalR();

// Product DB helper — repositories call PostgreSQL fn_* only (no inline SQL).
builder.Services.AddScoped<ProductDatabaseHelper>();

// SoftOnCloud login / countries (HTTP to platform — no product SQL).
builder.Services.AddScoped<ISoftOnCloudAuthService, SoftOnCloudAuthService>();

// ─── Module DI ────────────────────────────────────────────────────────────────
builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<ILeadService, LeadService>();
builder.Services.AddScoped<IOpportunityRepository, OpportunityRepository>();
builder.Services.AddScoped<IOpportunityService, OpportunityService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IOwnerRepository, OwnerRepository>();
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

// ─── SoftOnCloud JWT validation ───────────────────────────────────────────────
var jwtSecret = builder.Configuration["SoftOnCloud:Jwt:Secret"]
                ?? throw new InvalidOperationException("SoftOnCloud:Jwt:Secret is not configured.");
var jwtIssuer = builder.Configuration["SoftOnCloud:Jwt:Issuer"] ?? "SoftOnCloud";
var jwtAudience = builder.Configuration["SoftOnCloud:Jwt:Audience"] ?? "SoftOnCloud";

if (string.Equals(jwtSecret, "REPLACE_WITH_SOFTONCLOUD_JWT_SECRET", StringComparison.Ordinal))
{
    if (!builder.Environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "SoftOnCloud:Jwt:Secret is still the placeholder. Copy the SoftOnCloud JWT signing secret (same as Appointment).");
    }

    // Dev scaffold: API boots; set real SoftOnCloud JWT secret before testing auth.
    Console.WriteLine(
        "WARNING: SoftOnCloud:Jwt:Secret is placeholder — copy from SoftOnCloud / Appointment appsettings before JWT auth.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtIssuer,
        ValidAudience            = jwtAudience,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew                = TimeSpan.FromMinutes(1),
        NameClaimType            = "user_id",
        RoleClaimType            = "role"
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var path = context.HttpContext.Request.Path;
            if (path.StartsWithSegments("/hubs/session"))
            {
                var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                if (string.IsNullOrEmpty(accessToken))
                {
                    var auth = context.Request.Headers.Authorization.FirstOrDefault();
                    if (!string.IsNullOrEmpty(auth)
                        && auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        accessToken = auth["Bearer ".Length..].Trim();
                    }
                }
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
            }
            return Task.CompletedTask;
        },
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode  = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new
                {
                    message = "Your session has ended. Please sign in again.",
                    code = "SESSION_ENDED"
                }));
        }
    };
});

// ─── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("CrmCors", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "https://crm.softoncloud.com",
                "http://crm.softoncloud.com",
                "https://www.crm.softoncloud.com",
                "http://www.crm.softoncloud.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
startupLogger.LogInformation(
    "CRM API starting. Environment={Environment}; SoftOnCloud={SoftOnCloudBase}; Frontend={Frontend}; AppId={AppId}",
    app.Environment.EnvironmentName,
    builder.Configuration["SoftOnCloud:ApiBaseUrl"],
    builder.Configuration["CRM:FrontendBaseUrl"],
    builder.Configuration["CRM:AppId"]);

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var ex = feature?.Error;
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("UnhandledException");
        logger.LogError(ex, "Unhandled API exception Path={Path}", context.Request.Path);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = ex switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            InvalidOperationException => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };

        var message = ex switch
        {
            UnauthorizedAccessException => "Your session has ended. Please sign in again.",
            InvalidOperationException ioe => ioe.Message,
            _ => "Unexpected server error. Please try again."
        };

        object payload = ex is UnauthorizedAccessException
            ? new { message, code = "SESSION_ENDED" }
            : new { message };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    });
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM API v1");
    options.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseCors("CrmCors");
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SoftOnCloudSessionMiddleware>();

app.MapGet("/", () => "CRM API is running");
app.MapControllers();
app.MapHub<SessionHub>("/hubs/session");

app.Run();
