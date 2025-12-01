using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

// Inicializar dados padrão no banco (atuadores)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SmartRoomContext>();
    
    // Verificar se já existem atuadores
    if (!db.Actuators.Any())
    {
        db.Actuators.AddRange(new[]
        {
            new Actuator
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "Bomba de Água",
                Type = "pump",
                IsActive = false,
                LastUpdated = DateTime.UtcNow
            },
            new Actuator
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name = "Iluminação UV",
                Type = "light",
                IsActive = false,
                LastUpdated = DateTime.UtcNow
            },
            new Actuator
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "Ventilador",
                Type = "fan",
                IsActive = false,
                LastUpdated = DateTime.UtcNow
            },
            new Actuator
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                Name = "Aquecedor",
                Type = "heater",
                IsActive = false,
                LastUpdated = DateTime.UtcNow
            }
        });
        db.SaveChanges();
        Console.WriteLine("[Startup] Atuadores padrão inicializados.");
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


