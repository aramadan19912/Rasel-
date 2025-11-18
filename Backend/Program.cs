using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OutlookInboxManagement.Data;
using OutlookInboxManagement.Helpers;
using OutlookInboxManagement.Hubs;
using OutlookInboxManagement.Models;
using OutlookInboxManagement.Services;
using Application.Interfaces;
using Infrastructure.Services;
using Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("OutlookInboxManagement")));

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
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Register Application Services
builder.Services.AddScoped<IInboxService, InboxService>();
builder.Services.AddScoped<IMessageRuleEngine, MessageRuleEngine>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICalendarService, CalendarService>();
builder.Services.AddScoped<IContactsService, ContactsService>();
builder.Services.AddScoped<Backend.Services.IVideoConferenceService, Backend.Services.VideoConferenceService>();

// Register Organization Services
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IOrgChartService, OrgChartService>();

// Register Archive Services
builder.Services.AddScoped<Application.Interfaces.Archive.IArchiveCategoryService, Infrastructure.Services.Archive.ArchiveCategoryService>();
builder.Services.AddScoped<Application.Interfaces.Archive.ICorrespondenceService, Infrastructure.Services.Archive.CorrespondenceService>();
builder.Services.AddScoped<Application.Interfaces.Archive.IPdfConversionService, Infrastructure.Services.Archive.PdfConversionService>();

// Register Admin Services
builder.Services.AddScoped<OutlookInboxManagement.Services.Admin.IDashboardService, OutlookInboxManagement.Services.Admin.DashboardService>();
builder.Services.AddScoped<OutlookInboxManagement.Services.Admin.IReportService, OutlookInboxManagement.Services.Admin.ReportService>();

// Register DMS Services
builder.Services.AddScoped<Application.Interfaces.DMS.IDocumentService, Infrastructure.Services.DMS.DocumentService>();

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

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
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
