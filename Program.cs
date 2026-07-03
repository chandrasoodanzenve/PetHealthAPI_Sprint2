using SwaggerModels = Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Microsoft.EntityFrameworkCore;
using PetHealthAPI.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using PetHealthAPI.Models;
using Asp.Versioning;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.AspNetCore.RateLimiting; 
using System.Threading.RateLimiting;
using Polly;
using Polly.Extensions.Http;
// 1. Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/petapi_log.txt", rollingInterval: RollingInterval.Day) 
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
var serviceName = "PetHealthAPI";
var serviceVersion = "1.0.0";
// 1. Retry Policy with Logging
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, 
        retryAttempt => TimeSpan.FromSeconds(2),
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            Console.WriteLine($"\n[POLLY DEBUG] ---> RETRY ATTEMPT: {retryCount} <---");
            Log.Warning("!!! POLLY RETRY TRIGGERED !!! Attempt: {RetryCount} | Reason: {Reason}", 
                retryCount, outcome.Result?.StatusCode);
        });

// 2. Circuit Breaker
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

// 3. Timeout
var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(5);

// 4. Bulkhead
var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(2, 10);

// 5. Fallback 
var fallbackPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .FallbackAsync(new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
    {
        Content = new StringContent("The service is currently busy. Please try again later (Polly Fallback).")
    });

// --- Register ResilientClient ONLY ONCE ---
builder.Services.AddHttpClient("ResilientClient")
    .AddPolicyHandler(fallbackPolicy) // 1. Outer: Catch failures
    .AddPolicyHandler(retryPolicy)    // 2. Middle: Try again
    .AddPolicyHandler(circuitBreakerPolicy) // 3. Inner: Break if too many errors
    .AddPolicyHandler(timeoutPolicy)   // 4. Inner: Kill slow requests
    .AddPolicyHandler(bulkheadPolicy); // 5. Inner: Limit resources

builder.Services.AddHostedService<PetHealthAPI.BackgroundServices.OutboxProcessor>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("fixed", httpContext =>
    {
        var username = httpContext.User.Identity?.Name;
        var key = username ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromSeconds(10),
            QueueLimit = 0
        });
    });
});
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 1024 * 1024; 
});

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource(serviceName)
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion))
            .AddAspNetCoreInstrumentation() 
            .AddHttpClientInstrumentation()
            .AddSqlClientInstrumentation()  
            .AddConsoleExporter()  
            .AddOtlpExporter(options =>
                {
                     options.Endpoint = new Uri("http://localhost:4317"); 
                });    
    })
    .WithMetrics(metricsProviderBuilder =>
    {
        metricsProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddMeter("PetHealthAPI.Metrics")
            .AddPrometheusExporter();
            // .AddConsoleExporter();
    });
builder.Host.UseSerilog(); 

// 1. Database Configuration
var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider");

if (dbProvider == "Sqlite")
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));
}
builder.Services.AddScoped<PetHealthAPI.Repositories.IPetRepository, PetHealthAPI.Repositories.PetRepository>();
builder.Services.AddScoped<PetHealthAPI.Services.IPetService, PetHealthAPI.Services.PetService>();
builder.Services.AddDistributedMemoryCache();
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
//     options.InstanceName = "PetHealth_";
// });
// 1. API Versioning Setup
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true; 
    options.ReportApiVersions = true; 
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(), 
        new HeaderApiVersionReader("x-api-version") 
    );
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; 
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddControllers();
builder.Services.AddHealthChecks()
    .AddCheck("API-Self-Check", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("SqlServerConnection")!, 
        name: "Database-Check"
    );
builder.Services.AddHostedService<PetHealthAPI.BackgroundServices.PetHealthReminderService>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();
            var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString() ?? "N/A";
            Log.Warning("Validation failed! CorrelationID: {CorrelationId}. Errors: {Errors}", correlationId, string.Join(", ", errors));

        var response = ApiResponse<object>.Failure("Validation failed", errors);
        return new BadRequestObjectResult(response);
    };
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters(); 
builder.Services.AddValidatorsFromAssemblyContaining<PetHealthAPI.Validators.PetValidator>();
// 2. Swagger Configuration with JWT
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new SwaggerModels.OpenApiInfo { Title = "Pet API v1", Version = "v1" });
    options.SwaggerDoc("v2", new SwaggerModels.OpenApiInfo { Title = "Pet API v2", Version = "v2" });    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    // JWT Security Definition
    options.AddSecurityDefinition("Bearer", new SwaggerModels.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SwaggerModels.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = SwaggerModels.ParameterLocation.Header,
        Description = "Enter the below token in the format:{your JWT token}. Example:eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });
    options.AddSecurityRequirement(new SwaggerModels.OpenApiSecurityRequirement
    {
        {
            new SwaggerModels.OpenApiSecurityScheme
            {
                Reference = new SwaggerModels.OpenApiReference { Type = SwaggerModels.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    } );
});
// 3. JWT Authentication Setup
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"];
if (string.IsNullOrEmpty(secretKey))
{
    throw new Exception("JWT Key is missing in appsettings.json!");
}
var key = Encoding.ASCII.GetBytes(secretKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString() ?? "N/A";
            Log.Error("Authentication failed! CorrelationID: {CorrelationId}. Error: {Message}", correlationId, context.Exception.Message);
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();
// 1. Response Compression 
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// 2. Output Caching 
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("PetCachePolicy", builder => 
        builder.Expire(TimeSpan.FromMinutes(5)).Tag("pets_tag"));
});
var app = builder.Build();
// Middleware Order
app.UseMiddleware<PetHealthAPI.Middleware.ExceptionMiddleware>();
app.UseMiddleware<PetHealthAPI.Middleware.CorrelationIdMiddleware>();

// 2. Security Headers 
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");

    if (context.Request.Path.StartsWithSegments("/swagger"))
    {
        context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'");
    }
    else
    {
        context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
    }
    await next();
});
app.UseResponseCompression();
app.UseRateLimiter();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pet API - Version 1.0");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Pet API - Version 2.0");
        options.DocumentTitle = "Pet Pulse - Health Tracker Pro API"; 
    });
}
app.UseAuthentication(); 
app.UseAuthorization();  
app.UseOutputCache(); 
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.MapControllers();
app.MapHealthChecks("/health");
try
{
    Log.Information("API Starting Up...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API failed to start correctly!");
}
finally
{
    Log.CloseAndFlush();
}
public partial class Program { }