using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebApplication2.Data;
using WebApplication2.Models;
using WebApplication2.Services;
using WebApplication2.Middleware;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Configure enhanced security logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddEventSourceLogger();

// Add services to the container.
builder.Services.AddControllers();

// Add Memory Cache for rate limiting and security
builder.Services.AddMemoryCache();

// Configure Entity Framework with connection pooling and warning suppression
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        });
        
    // Suppress the pending model changes warning temporarily
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// Configure Enhanced Identity with security policies
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Password policies
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 4;
    
    // User policies
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    
    // Lockout policies
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // Sign in policies
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Enhanced JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("SecuritySettings:RequireHttps", true);
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true,
        RequireSignedTokens = true
    };
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("JWT Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogDebug("JWT Token validated for user: {User}", context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        }
    };
});

// Register Enhanced Services with Dependency Injection
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAnimalService, AnimalService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IOfferService, OfferService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<ITokenEncryptionService, TokenEncryptionService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<IDemoDataService, DemoDataService>(); // Add demo data service

// Configure Enhanced CORS with security
builder.Services.AddCors(options =>
{
    options.AddPolicy("SecurePolicy", policy =>
    {
        policy.WithOrigins("https://localhost:7000", "https://yourdomain.com") // Specify allowed origins
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "Accept", "Origin")
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// Configure Forwarded Headers for proxy scenarios
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configure Enhanced Swagger/OpenAPI with security
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Eid Al-Adha Sacrifice Farm API", 
        Version = "v1",
        Description = "Secure API for managing farms and animals during Eid Al-Adha with role-based business operations",
        Contact = new OpenApiContact
        {
            Name = "Farm Management Team",
            Email = "support@farm.com"
        }
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Note: The token will be automatically encrypted. Enter 'Bearer' [space] and then your encrypted token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Configure Enhanced Antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = false;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.HttpOnly = true;
});

var app = builder.Build();

// Configure Forwarded Headers
app.UseForwardedHeaders();

// Configure the HTTP request pipeline with enhanced security
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Eid Al-Adha Sacrifice Farm API v1");
        c.RoutePrefix = string.Empty;
        c.DocumentTitle = "Farm API - Secure Documentation";
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts(); // HTTP Strict Transport Security
}

// Security Middleware Pipeline (Order is important!)
app.UseMiddleware<SecurityMiddleware>(); // Custom security validation
app.UseHttpsRedirection(); // Force HTTPS
app.UseMiddleware<TokenDecryptionMiddleware>(); // Decrypt JWT tokens

app.UseCors("SecurePolicy");
app.UseAuthentication();
app.UseAuthorization();

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.MapControllers();

// Initialize database and seed data with enhanced error handling
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var demoDataService = services.GetRequiredService<IDemoDataService>();
        
        logger.LogInformation("Initializing database...");
        
        // Use EnsureCreated instead of migrations to avoid the model mismatch issue
        if (await context.Database.EnsureCreatedAsync())
        {
            logger.LogInformation("Database created successfully");
        }
        else
        {
            logger.LogInformation("Database already exists");
            
            // Try to apply pending migrations if they exist
            try
            {
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation($"Applying {pendingMigrations.Count()} pending migrations...");
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Database migrations applied successfully");
                }
            }
            catch (Exception migrationEx)
            {
                logger.LogWarning(migrationEx, "Migration issue detected, but database is operational");
            }
        }
        
        // Seed roles
        await SeedRolesAsync(roleManager, logger);
        
        // Seed super admin user
        await SeedSuperAdminAsync(userManager, context, logger);
        
        // Seed demo data if environment allows it and data doesn't exist
        if (app.Environment.IsDevelopment())
        {
            try
            {
                if (!await demoDataService.HasDemoDataAsync())
                {
                    logger.LogInformation("Seeding demo data for development environment...");
                    await demoDataService.SeedDemoDataAsync();
                    logger.LogInformation("Demo data seeded successfully");
                }
                else
                {
                    logger.LogInformation("Demo data already exists, skipping seeding");
                }
            }
            catch (Exception demoEx)
            {
                logger.LogWarning(demoEx, "Demo data seeding failed, but application will continue");
            }
        }
        
        logger.LogInformation("Application started successfully with enhanced security");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the application");
        // Don't throw in production to allow the app to start
        if (app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

app.Run();

// Helper methods for seeding with enhanced logging
static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
{
    var roles = new[] { UserRoles.SuperAdmin, UserRoles.DataEntry, UserRoles.Customer };
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (result.Succeeded)
            {
                logger.LogInformation("Role {Role} created successfully", role);
            }
            else
            {
                logger.LogError("Failed to create role {Role}: {Errors}", role, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}

static async Task SeedSuperAdminAsync(UserManager<User> userManager, ApplicationDbContext context, ILogger logger)
{
    const string superAdminEmail = "admin@farm.com";
    const string superAdminPassword = "Admin123!@#";
    
    var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
    if (superAdmin == null)
    {
        superAdmin = new User
        {
            UserName = "SuperAdmin",
            Email = superAdminEmail,
            FirstName = "Super",
            LastName = "Admin",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var result = await userManager.CreateAsync(superAdmin, superAdminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(superAdmin, UserRoles.SuperAdmin);
            
            // Create farm for super admin
            var farm = new Farm
            {
                Id = Guid.NewGuid(),
                Name = "Admin Farm",
                OwnerId = superAdmin.Id
            };
            
            context.Farms.Add(farm);
            await context.SaveChangesAsync();
            
            logger.LogInformation("SuperAdmin user created successfully");
        }
        else
        {
            logger.LogError("Failed to create SuperAdmin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
    else
    {
        logger.LogInformation("SuperAdmin user already exists");
    }
}
