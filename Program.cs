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


// 1. Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/petapi_log.txt", rollingInterval: RollingInterval.Day) 
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
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
        Description = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsImV4cCI6MTc4MDk5MjUxMSwiaXNzIjoiUGV0SGVhbHRoQVBJIiwiYXVkIjoiUGV0UHVsc2VVc2VycyJ9.SedN1sqxLyspGXwcxokGcvhfL-6t3eKcRiWGlI799Ck"
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
var app = builder.Build();
app.UseMiddleware<PetHealthAPI.Middleware.CorrelationIdMiddleware>();

// Middleware Order
app.UseMiddleware<PetHealthAPI.Middleware.ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
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
app.MapControllers();
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