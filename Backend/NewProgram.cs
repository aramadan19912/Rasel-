using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Backend.Application.Interfaces;
using Backend.Domain.Entities.Identity;
using Backend.Domain.Enums;
using Backend.Infrastructure.Authorization;
using Backend.Infrastructure.Configuration;
using Backend.Infrastructure.Data;
using Backend.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

builder.Services.AddDbContext<Backend.Infrastructure.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("OutlookInboxManagement")));

builder.Services.AddIdentity<ApplicationUser, Role>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<Backend.Infrastructure.Data.ApplicationDbContext>()
.AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

var secretKey = jwtSettings.Get<JwtSettings>()?.SecretKey ?? throw new InvalidOperationException("JWT Secret Key not configured");
var key = Encoding.UTF8.GetBytes(secretKey);

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

builder.Services.AddAuthorization(options =>
{
    var permissionFields = typeof(SystemPermission).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
    foreach (var field in permissionFields)
    {
        var permissionValue = field.GetValue(null)?.ToString();
        if (!string.IsNullOrEmpty(permissionValue))
        {
            options.AddPolicy(permissionValue, policy => policy.Requirements.Add(new PermissionRequirement(permissionValue)));
        }
    }
});

builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

// ===== Authentication & Authorization Services =====
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

// ===== Feature Services =====
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<ICalendarService, CalendarService>();
builder.Services.AddScoped<IContactsService, ContactsService>();
builder.Services.AddScoped<IVideoConferenceService, VideoConferenceService>();

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:4201").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Rasel Outlook Management API", Version = "v1", Description = "Clean Architecture with JWT & Permissions" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization using Bearer scheme. Enter 'Bearer' [space] and your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rasel API V1"); c.RoutePrefix = string.Empty; });
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<Backend.Infrastructure.Data.ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<Role>>();
        var permissionService = services.GetRequiredService<IPermissionService>();

        context.Database.Migrate();
        await permissionService.SeedPermissionsAsync();

        var roles = new[] {
            new { Name = SystemRole.SuperAdmin, Description = "Super Administrator", IsSystem = true },
            new { Name = SystemRole.Admin, Description = "Administrator", IsSystem = true },
            new { Name = SystemRole.Manager, Description = "Manager", IsSystem = true },
            new { Name = SystemRole.User, Description = "Regular User", IsSystem = true },
            new { Name = SystemRole.Guest, Description = "Guest User", IsSystem = true }
        };

        foreach (var roleData in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleData.Name))
            {
                await roleManager.CreateAsync(new Role { Name = roleData.Name, Description = roleData.Description, IsSystemRole = roleData.IsSystem });
            }
        }

        var superAdminRole = await roleManager.FindByNameAsync(SystemRole.SuperAdmin);
        if (superAdminRole != null)
        {
            var allPermissions = await context.Permissions.ToListAsync();
            foreach (var permission in allPermissions)
            {
                var exists = await context.RolePermissions.AnyAsync(rp => rp.RoleId == superAdminRole.Id && rp.PermissionId == permission.Id);
                if (!exists)
                {
                    context.RolePermissions.Add(new RolePermission { RoleId = superAdminRole.Id, PermissionId = permission.Id });
                }
            }
            await context.SaveChangesAsync();
        }

        var adminEmail = "admin@rasel.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin@123456");
            if (result.Succeeded) await userManager.AddToRoleAsync(adminUser, SystemRole.SuperAdmin);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database error");
    }
}

app.Run();
