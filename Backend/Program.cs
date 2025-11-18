using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OutlookInboxManagement.Data;
using OutlookInboxManagement.Helpers;
using OutlookInboxManagement.Hubs;
using OutlookInboxManagement.Models;
using OutlookInboxManagement.Services;
using Backend.Application.Interfaces;
using Backend.Infrastructure.Services;
using Backend.Infrastructure.Data;
using Backend.Infrastructure.Configuration;
using Backend.Application.Interfaces.Archive;
using Backend.Infrastructure.Services.Archive;
using Backend.Application.Interfaces.DMS;
using Backend.Infrastructure.Services.DMS;
using OutlookInboxManagement.Services.Admin;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// Configure Database - Register both DbContexts for compatibility
builder.Services.AddDbContext<OutlookInboxManagement.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("OutlookInboxManagement")));

builder.Services.AddDbContext<Backend.Infrastructure.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("OutlookInboxManagement")));

// Alias for default ApplicationDbContext
builder.Services.AddScoped<ApplicationDbContext>(provider =>
    provider.GetRequiredService<OutlookInboxManagement.Data.ApplicationDbContext>());

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<OutlookInboxManagement.Data.ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Register Application Services
builder.Services.AddScoped<IInboxService, InboxService>();
builder.Services.AddScoped<IMessageRuleEngine, MessageRuleEngine>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<OutlookInboxManagement.Services.ICalendarService, CalendarService>();
builder.Services.AddScoped<Backend.Application.Interfaces.IContactsService, Backend.Infrastructure.Services.ContactsService>();
builder.Services.AddScoped<Backend.Application.Interfaces.IUserService, Backend.Infrastructure.Services.UserService>();
builder.Services.AddScoped<Backend.Application.Interfaces.IJwtService, Backend.Infrastructure.Services.JwtService>();
builder.Services.AddScoped<Backend.Services.IVideoConferenceService, Backend.Services.VideoConferenceService>();

// Register Organization Services
builder.Services.AddScoped<OutlookInboxManagement.Services.Admin.IDepartmentService, OutlookInboxManagement.Services.Admin.DepartmentService>();
builder.Services.AddScoped<OutlookInboxManagement.Services.Admin.IPositionService, OutlookInboxManagement.Services.Admin.PositionService>();
builder.Services.AddScoped<OutlookInboxManagement.Services.Admin.IEmployeeService, OutlookInboxManagement.Services.Admin.EmployeeService>();
builder.Services.AddScoped<OutlookInboxManagement.Services.Admin.IOrgChartService, OutlookInboxManagement.Services.Admin.OrgChartService>();

// Register Archive Services
builder.Services.AddScoped<IArchiveCategoryService, ArchiveCategoryService>();
builder.Services.AddScoped<ICorrespondenceService, CorrespondenceService>();
builder.Services.AddScoped<IPdfConversionService, PdfConversionService>();

// Register Admin Services
builder.Services.AddScoped<OutlookInboxManagement.Services.Admin.IDashboardService, OutlookInboxManagement.Services.Admin.DashboardService>();
builder.Services.AddScoped<OutlookInboxManagement.Services.Admin.IReportService, OutlookInboxManagement.Services.Admin.ReportService>();

// Register DMS Services
builder.Services.AddScoped<IDocumentService, DocumentService>();

// Add SignalR
builder.Services.AddSignalR();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:4201")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Outlook Inbox Management API",
        Version = "v1",
        Description = "Comprehensive Inbox Management System with all Outlook features"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
});

// Configure JWT Settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

var secretKey = jwtSettings.Get<JwtSettings>()?.SecretKey
    ?? throw new InvalidOperationException("JWT Secret Key not configured");
var key = Encoding.UTF8.GetBytes(secretKey);

// Configure Authentication with JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Get<JwtSettings>()?.Issuer,
        ValidAudience = jwtSettings.Get<JwtSettings>()?.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };

    // Support JWT in SignalR connections via query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Add Response Caching
builder.Services.AddResponseCaching();

// Add Memory Cache
builder.Services.AddMemoryCache();

// Configure Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Outlook Inbox Management API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at root
    });
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAngular");

// Enable Response Caching
app.UseResponseCaching();

// Enable Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Map Controllers
app.MapControllers();

// Map SignalR Hubs
app.MapHub<InboxHub>("/hubs/inbox");
app.MapHub<Backend.Hubs.VideoConferenceHub>("/hubs/video-conference");

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Apply migrations
        context.Database.Migrate();

        // Seed default roles
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }

        // Create default admin user
        var adminEmail = "admin@outlookinbox.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");

                // Initialize folders for admin user
                var inboxService = services.GetRequiredService<IInboxService>();
                await inboxService.InitializeUserFoldersAsync(adminUser.Id);
            }
        }

        // Seed organizational data
        var orgSeeder = new OrganizationSeeder(context);
        await orgSeeder.SeedAsync();

        // Seed archive categories
        var archiveSeeder = new ArchiveSeeder(context);
        await archiveSeeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.Run();
