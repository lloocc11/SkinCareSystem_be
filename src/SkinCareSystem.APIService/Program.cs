
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CloudinaryDotNet;
using Swashbuckle.AspNetCore.Annotations;
using SkinCareSystem.Repositories.DBContext;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.ExternalServices.IServices;
using SkinCareSystem.Services.ExternalServices.Services;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.InternalServices.Services;
using SkinCareSystem.Services.Options;
using SkinCareSystem.Services.Rag;


var builder = WebApplication.CreateBuilder(args);

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<SkinCareSystemDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<IChatSessionService, ChatSessionService>();
builder.Services.AddScoped<IChatMessageService, ChatMessageService>();
builder.Services.AddScoped<IAIAnalysisService, AIAnalysisService>();
builder.Services.AddScoped<IAIResponseService, AIResponseService>();

// Question Services
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IUserAnswerService, UserAnswerService>();
// Medical Document Services
builder.Services.AddScoped<IMedicalDocumentService, MedicalDocumentService>();
builder.Services.AddScoped<IDocumentChunkService, DocumentChunkService>();
builder.Services.AddScoped<IMedicalDocumentAssetService, MedicalDocumentAssetService>();
// Consent Record Service
builder.Services.AddScoped<IConsentRecordService, ConsentRecordService>();

builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

builder.Services.AddRagServices();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JWT"));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CloudinarySettings>>().Value;
    if (string.IsNullOrWhiteSpace(settings.CloudName) ||
        string.IsNullOrWhiteSpace(settings.ApiKey) ||
        string.IsNullOrWhiteSpace(settings.ApiSecret))
    {
        throw new InvalidOperationException("Cloudinary settings are missing or incomplete.");
    }

    var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
    var cloudinary = new Cloudinary(account)
    {
        Api = { Secure = true }
    };

    return cloudinary;
});

var jwtSettings = builder.Configuration.GetSection("JWT").Get<JwtSettings>()
                  ?? throw new InvalidOperationException("JWT settings are missing from configuration.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(authHeader) && !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Fail("Authorization header must start with 'Bearer '.");
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c=>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SkinCareSystem.APIService",
        Version = "v1",
        Description = "Skin Care System API Service"
    });
    
    // Add JWT Authentication with proper Bearer scheme
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // Enable annotations for better Swagger documentation
    c.EnableAnnotations();
    
    // Include XML comments if file exists
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
    policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
    });
});
var app = builder.Build();

// Seed admin user on startup
await SeedAdminUserAsync(app.Services);

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SkinCareSystem.APIService v1");
    c.RoutePrefix = string.Empty; // Make Swagger UI available at root
});
app.UseDeveloperExceptionPage();


app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

await app.RunAsync();

// Method to seed admin user from environment variables
static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<SkinCareSystemDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Get admin email from environment variable
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        
        // Skip if no admin email provided
        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            logger.LogInformation("ADMIN_EMAIL not set. Skipping admin seed.");
            return;
        }
        
        // Get admin role
        var adminRole = await context.Roles
            .FirstOrDefaultAsync(r => r.name.ToLower() == "admin");
            
        if (adminRole == null)
        {
            logger.LogWarning("Admin role not found in database. Please ensure roles are seeded first.");
            return;
        }
        
        // Check if admin user already exists by email
        var existingAdmin = await context.Users
            .FirstOrDefaultAsync(u => u.email.ToLower() == adminEmail.ToLower());
        
        if (existingAdmin != null)
        {
            // Update existing user to admin role
            if (existingAdmin.role_id != adminRole.role_id)
            {
                existingAdmin.role_id = adminRole.role_id;
                existingAdmin.status = "active";
                existingAdmin.updated_at = DateTime.UtcNow;
                await context.SaveChangesAsync();
                logger.LogInformation("Promoted user {Email} to admin role", existingAdmin.email);
            }
            else
            {
                logger.LogInformation("User {Email} already has admin role", existingAdmin.email);
            }
        }
        else
        {
            // Admin doesn't exist yet - will be created when they first login via Google
            logger.LogInformation("Admin email {Email} set. User will be promoted to admin on first Google login.", adminEmail);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error seeding admin user");
    }
}
        
    
