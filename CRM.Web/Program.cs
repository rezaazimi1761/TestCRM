using Blazored.LocalStorage;
using CRM.Web.Auth;
using CRM.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

// Auth state
builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthStateProvider>());
builder.Services.AddAuthorizationCore();

// HTTP clients
builder.Services.AddHttpClient<AuthApiClient>(c =>
    c.BaseAddress = new Uri(builder.Configuration["AuthService:Url"] ?? "http://localhost:9041"));

builder.Services.AddScoped<CrmApiClient>();
builder.Services.AddScoped<AuthAdminClient>();
builder.Services.AddHttpClient("auth-admin");
builder.Services.AddHttpClient("crm");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_host");
app.Run();
