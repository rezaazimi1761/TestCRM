using ModernCRM.Crm.Api.UserSync;
using ModernCRM.Crm.Api.Consumers;
using MassTransit;
using ModernCRM.Crm.Api.Frontend;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModernCRM.Crm.Application.Handlers;
using ModernCRM.Crm.Domain.Repositories;
using ModernCRM.Crm.Infrastructure.Persistence;
using ModernCRM.Crm.Infrastructure.Repositories;
using ModernCRM.Crm.Infrastructure;
using ModernCRM.Crm.Infrastructure.Integration;

var builder = WebApplication.CreateBuilder(args);

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
if (jwtSecret.Length < 32) throw new InvalidOperationException("Jwt:Secret must contain at least 32 characters.");

builder.Services.AddCrmPersistence(builder.Configuration);
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IOpportunityRepository, OpportunityRepository>();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AuthUserSyncedConsumer>();
    x.AddConsumer<AuthUserSyncFailedConsumer>();
    x.AddSagaStateMachine<UserSyncStateMachine, UserSyncSagaState>()
        .EntityFrameworkRepository(r => r.UseCrmSagaRepository());
    x.AddCrmEfOutbox();
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbit = builder.Configuration.GetSection("RabbitMQ");
        var rabbitPassword = rabbit["Password"];
        if (string.IsNullOrWhiteSpace(rabbitPassword))
            throw new InvalidOperationException("RabbitMQ:Password must be provided through a secure configuration source.");
        cfg.Host(rabbit["Host"] ?? "rabbitmq://localhost", h =>
        {
            h.Username(rabbit["Username"] ?? "guest");
            h.Password(rabbitPassword);
        });
        cfg.ReceiveEndpoint("moderncrm-user-sync-saga", e =>
        {
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
            e.UseCrmEfOutbox(context);
            e.ConfigureSaga<UserSyncSagaState>(context);
        });
        cfg.ReceiveEndpoint("moderncrm-auth-sync-results", e =>
        {
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
            e.UseCrmEfOutbox(context);
            e.ConfigureConsumer<AuthUserSyncedConsumer>(context);
            e.ConfigureConsumer<AuthUserSyncFailedConsumer>(context);
        });
    });
});

builder.Services.AddScoped<CreateAccountHandler>();
builder.Services.AddScoped<UpdateAccountHandler>();
builder.Services.AddScoped<DeleteAccountHandler>();
builder.Services.AddScoped<GetAccountsHandler>();
builder.Services.AddScoped<GetAccountByIdHandler>();
builder.Services.AddScoped<CreateContactHandler>();
builder.Services.AddScoped<UpdateContactHandler>();
builder.Services.AddScoped<DeleteContactHandler>();
builder.Services.AddScoped<GetContactsHandler>();
builder.Services.AddScoped<GetContactByIdHandler>();
builder.Services.AddScoped<CreateTicketHandler>();
builder.Services.AddScoped<UpdateTicketHandler>();
builder.Services.AddScoped<DeleteTicketHandler>();
builder.Services.AddScoped<GetTicketsHandler>();
builder.Services.AddScoped<GetTicketByIdHandler>();
builder.Services.AddScoped<CreateOpportunityHandler>();
builder.Services.AddScoped<UpdateOpportunityHandler>();
builder.Services.AddScoped<DeleteOpportunityHandler>();
builder.Services.AddScoped<GetOpportunitiesHandler>();
builder.Services.AddScoped<GetOpportunityByIdHandler>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization(options =>
    options.AddPolicy("Tenant", policy => policy.RequireClaim("tenant_id")));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ModernCRM.Crm.Api.ExceptionHandling.DomainExceptionHandler>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name));

var app = builder.Build();
if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<CrmDatabaseInitializer>().InitializeAsync();
}
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireAuthorization("Tenant");
app.Run();
