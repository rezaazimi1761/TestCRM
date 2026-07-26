using Blazored.LocalStorage;
using ModernCRM.Web.Auth;
using ModernCRM.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthStateProvider>());
builder.Services.AddAuthorizationCore();

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
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();