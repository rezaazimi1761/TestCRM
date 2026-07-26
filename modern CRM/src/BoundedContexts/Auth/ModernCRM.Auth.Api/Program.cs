using ModernCRM.Auth.Application.Handlers;
using ModernCRM.Auth.Domain.Users;
using ModernCRM.Auth.Infrastructure.Identity;
using ModernCRM.Auth.Infrastructure.Persistence;
using ModernCRM.Auth.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<AuthDbContext>();
builder.Services.AddSingleton<IAuthUserRepository, AuthUserRepository>();
builder.Services.AddSingleton<ITenantRepository, TenantRepository>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<UpdateUserHandler>();
builder.Services.AddScoped<DeleteUserHandler>();
builder.Services.AddScoped<GetUsersHandler>();
builder.Services.AddScoped<GetUserByIdHandler>();
builder.Services.AddScoped<CreateTenantHandler>();
builder.Services.AddScoped<UpdateTenantHandler>();
builder.Services.AddScoped<GetTenantsHandler>();
builder.Services.AddScoped<GetTenantByIdHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();