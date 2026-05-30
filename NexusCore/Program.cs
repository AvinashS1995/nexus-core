using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using NexusCore.Common;
using NexusCore.Repositories;
using NexusCore.Repositories.Interfaces;
using NexusCore.Services;
using NexusCore.Services.Interfaces;
using NexusPlatform.Services;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container.

/* --------------------------------------------------
 * 1️⃣ Controllers + Custom Model Binder
 * --------------------------------------------------*/
builder.Services.AddControllers(options =>
{
    // 🔥 Custom JSON / Encrypted Model Binder
    options.ModelBinderProviders
        .Insert(0, new JsonBodyModelBinderProvider());
})
.AddNewtonsoftJson();

/* --------------------------------------------------
 * 2️⃣ CORS
 * --------------------------------------------------*/
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder
            .WithOrigins(configuration["AllowedHosts"])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

/* --------------------------------------------------
 * 3️⃣ DbContext (🔥 IMPORTANT FIX)
 * --------------------------------------------------*/
builder.Services.AddScoped<DbContext>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var writeConnection = config.GetConnectionString("DefaultConnection");
    var readConnection = config.GetConnectionString("ReadConnection");

    return new DbContext(writeConnection, readConnection);
});

/* --------------------------------------------------
 * 4️⃣ JWT Authentication
 * --------------------------------------------------*/
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();

            return context.Response.WriteAsync(
                JsonConvert.SerializeObject(new
                {
                    StatusCode = NexusCore.Common.StatusCodes.UnAuthorised,
                    message = "UnAuthorized Access",
                    data = string.Empty
                }));
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
    };
});

/* --------------------------------------------------
 * 5️⃣ Authorization
 * --------------------------------------------------*/
builder.Services.AddAuthorization();

/* --------------------------------------------------
 * 6️⃣ Swagger
 * --------------------------------------------------*/
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swagger =>
{
    swagger.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = configuration["version"],
        Title = "Nexus Core JWT Token Authentication API",
        Description = "ASP.NET Core Web API"
    });

    swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description =
            "Enter 'Bearer {token}'"
    });

    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

/* --------------------------------------------------
 * 7️⃣ Dependency Injection (Repositories / Services)
 * --------------------------------------------------*/
builder.Services.AddTransient<ILoginRepository, LoginRepository>();
builder.Services.AddTransient<ILoginService, LoginService>();
builder.Services.AddTransient<ICommonRepository, CommonRepository>();
builder.Services.AddTransient<ICommonService, CommonService>();
// 🔥 REQUIRED for HttpContext
builder.Services.AddHttpContextAccessor();

// 🔥 REQUIRED for HttpClient
builder.Services.AddHttpClient();
builder.Services.AddScoped<IDeviceInfoService, DeviceInfoService>();



// Load error messages
ErrorMessages.LoadErrorMessages();

/* --------------------------------------------------
 * BUILD APP
 * --------------------------------------------------*/
var app = builder.Build();

/* --------------------------------------------------
 * MIDDLEWARE PIPELINE (ORDER MATTERS!)
 * --------------------------------------------------*/
app.ConfigureExceptionHandler(); // Global exception

app.UseCors("CorsPolicy");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

/* --------------------------------------------------
 * Swagger UI
 * --------------------------------------------------*/
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
    });
}

app.Run();
