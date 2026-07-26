using ModernCRM.Crm.Application.Handlers;
using ModernCRM.Crm.Domain.Repositories;
using ModernCRM.Crm.Infrastructure.Persistence;
using ModernCRM.Crm.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<CrmDbContext>();
builder.Services.AddSingleton<IAccountRepository, AccountRepository>();
builder.Services.AddSingleton<IContactRepository, ContactRepository>();
builder.Services.AddSingleton<ITicketRepository, TicketRepository>();
builder.Services.AddSingleton<IOpportunityRepository, OpportunityRepository>();

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