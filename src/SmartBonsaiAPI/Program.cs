using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Permitir que o servidor aceite conexões de qualquer IP na rede
builder.WebHost.UseUrls("http://0.0.0.0:5000");

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

// Habilitar CORS (para o ESP32 e páginas web)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SmartRoomContext>();
    if (!db.Actuators.Any(a => a.Id == Guid.Parse("00000000-0000-0000-0000-000000000001")))
    {
        db.Actuators.Add(new Actuator {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "Water Pump",
            Type = "pump",
            IsActive = false
        });
        db.SaveChanges();
    }
}


// Ativar Swagger SEM restrição de ambiente
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartBonsai API v1");
    c.RoutePrefix = "swagger"; // Swagger em /swagger
});

app.UseCors("AllowAll");
//app.UseHttpsRedirection();

// Servir arquivos estáticos (dashboard)
app.UseStaticFiles();

// Servir arquivos padrão (index.html)
app.UseDefaultFiles();

app.MapControllers();

app.Run();


