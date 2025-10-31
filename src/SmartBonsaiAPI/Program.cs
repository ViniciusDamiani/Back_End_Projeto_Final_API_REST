using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Carregar variáveis do .env (se existir)
EnvLoader.Load();

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infra/Domain registrations
builder.Services.AddDbContext<SmartRoomContext>(options => options.UseInMemoryDatabase("SmartRoomDb"));
builder.Services.AddScoped<IMeasurementService, MeasurementService>();
builder.Services.AddScoped<IActuatorService, ActuatorService>();
builder.Services.AddScoped<IAutomationService, AutomationService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();


