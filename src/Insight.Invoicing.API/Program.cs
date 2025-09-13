using System.Security.Claims;
using System.Text;
using Insight.Invoicing.Infrastructure.Persistence;
using Insight.Invoicing.API.Swagger;
using Insight.Invoicing.API.Hubs;
using Insight.Invoicing.API.Authorization;
using Insight.Invoicing.API.Middleware;
using Insight.Invoicing.Application.Services;
using Insight.Invoicing.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
using SendGrid;
using Insight.Invoicing.Domain.Repositories;
using Insight.Invoicing.Domain.Entities;
using Microsoft.AspNetCore.SignalR;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

// Redis Configuration (optional for development)
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
var useRedis = builder.Configuration.GetValue<bool>("UseRedis", false);

if (useRedis)
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        return ConnectionMultiplexer.Connect(redisConnectionString);
    });
    builder.Services.AddSignalR().AddStackExchangeRedis(redisConnectionString);
}
else
{
    builder.Services.AddSignalR();
}

// Hangfire with PostgreSQL (optional for development)
var useHangfire = builder.Configuration.GetValue<bool>("UseHangfire", false);
if (useHangfire)
{
    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("HangfireConnection")));

    builder.Services.AddHangfireServer();
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser<int>, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Insight Invoicing API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
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
            new string[] {}
        }
    });

    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});


var jwtKey = builder.Configuration["JwtSettings:SecretKey"];
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
var jwtAudience = builder.Configuration["JwtSettings:Audience"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT key is not configured");
}



builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            NameClaimType = ClaimTypes.NameIdentifier
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

builder.Services.AddAuthorization();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Insight.Invoicing.Application.Commands.Authentication.LoginCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Insight.Invoicing.Infrastructure.Handlers.Authentication.LoginCommandHandler).Assembly);
});

builder.Services.AddScoped(typeof(Insight.Invoicing.Domain.Repositories.IRepository<>), typeof(Insight.Invoicing.Infrastructure.Persistence.Repositories.Repository<>));
builder.Services.AddScoped(typeof(Insight.Invoicing.Domain.Repositories.IBaseRepository<>), typeof(Insight.Invoicing.Infrastructure.Persistence.Repositories.BaseRepository<>));

builder.Services.AddScoped<Insight.Invoicing.Domain.Repositories.IContractRepository, Insight.Invoicing.Infrastructure.Persistence.Repositories.ContractRepository>();
builder.Services.AddScoped<Insight.Invoicing.Domain.Repositories.IPaymentReceiptRepository, Insight.Invoicing.Infrastructure.Persistence.Repositories.PaymentReceiptRepository>();

builder.Services.AddScoped<Insight.Invoicing.Domain.Services.IInstallmentCalculatorService, Insight.Invoicing.Infrastructure.Services.InstallmentCalculatorService>();

// MassTransit for message bus (optional for development)
var useRabbitMQ = builder.Configuration.GetValue<bool>("UseRabbitMQ", false);
if (useRabbitMQ)
{
    builder.Services.AddMassTransit(x =>
    {
        x.SetKebabCaseEndpointNameFormatter();

        x.AddConsumer<Insight.Invoicing.Infrastructure.EventHandlers.IntegrationEventConsumers.ContractApprovedConsumer>();
        x.AddConsumer<Insight.Invoicing.Infrastructure.EventHandlers.IntegrationEventConsumers.PaymentReceiptUploadedConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            var rabbitMqHost = builder.Configuration.GetConnectionString("RabbitMQ") ?? "localhost";
            cfg.Host(rabbitMqHost, "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });

            cfg.ConfigureEndpoints(context);
        });
    });
}
else
{
    builder.Services.AddMassTransit(x =>
    {
        x.SetKebabCaseEndpointNameFormatter();
        x.UsingInMemory((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        });
    });
}

builder.Services.AddScoped<INotificationService>(sp =>
{
    var hubContext = sp.GetRequiredService<IHubContext<NotificationHub>>();
    var notificationRepository = sp.GetRequiredService<IRepository<Notification>>();
    var logger = sp.GetRequiredService<ILogger<NotificationService>>();

    var genericHubContext = (IHubContext<Hub>)hubContext;
    return new NotificationService(genericHubContext, notificationRepository, logger);
});
builder.Services.AddScoped<IEventBus, MassTransitEventBus>();
builder.Services.AddScoped<IOutboxService, OutboxService>();
builder.Services.AddScoped<IInstallmentService, InstallmentService>();
builder.Services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

var emailProvider = builder.Configuration["EmailProvider"] ?? "Mock";
if (emailProvider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<ISendGridClient>(sp =>
    {
        var apiKey = builder.Configuration["SendGrid:ApiKey"];
        return new SendGridClient(apiKey);
    });
    builder.Services.AddScoped<IEmailService, SendGridEmailService>();
}
else
{
    builder.Services.AddScoped<IEmailService, MockEmailService>();
}

var smsProvider = builder.Configuration["SmsProvider"] ?? "Mock";
if (smsProvider.Equals("Twilio", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<ISmsService, TwilioSmsService>();
}
else
{
    builder.Services.AddScoped<ISmsService, MockSmsService>();
}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DtsTask API V1");
    c.DocumentTitle = "Insight Invoicing API - DTC Task";
});

app.UseCors("AllowFrontend");

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/hubs/notifications");

// Map Hangfire Dashboard (if enabled)
if (useHangfire)
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });
}

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
}

app.Run();
