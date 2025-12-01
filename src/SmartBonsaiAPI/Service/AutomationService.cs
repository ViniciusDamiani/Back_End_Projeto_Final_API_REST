using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public interface IAutomationService
{
    Task<AutomationStatusDto> GetStatusAsync();
    Task<AutomationStatusDto> EnableAutomaticAsync();
    Task<AutomationStatusDto> DisableAutomaticAsync();
    Task<AutomationStatusDto> UpdateTargetsAsync(AutomationStatusDto targets);
    Task EvaluateRulesAsync();
}

public class AutomationService : IAutomationService
{
    private readonly SmartRoomContext _db;
    private readonly IActuatorService _actuatorService;
    private readonly IEmailSender _emailSender;

    public AutomationService(SmartRoomContext db, IActuatorService actuatorService, IEmailSender emailSender)
    {
        _db = db;
        _actuatorService = actuatorService;
        _emailSender = emailSender;
    }

    public async Task<AutomationStatusDto> GetStatusAsync()
    {
        var status = await _db.AutomationStatus.FirstOrDefaultAsync();
        if (status == null)
        {
            status = new AutomationStatus
            {
                IsEnabled = false,
                TargetTemperatureMin = 20.0,
                TargetTemperatureMax = 25.0,
                TargetHumidityMin = 40.0,
                TargetHumidityMax = 60.0
            };
            _db.AutomationStatus.Add(status);
            await _db.SaveChangesAsync();
        }

        return new AutomationStatusDto
        {
            IsEnabled = status.IsEnabled,
            TargetTemperatureMin = status.TargetTemperatureMin,
            TargetTemperatureMax = status.TargetTemperatureMax,
            TargetHumidityMin = status.TargetHumidityMin,
            TargetHumidityMax = status.TargetHumidityMax
        };
    }

    public async Task<AutomationStatusDto> EnableAutomaticAsync()
    {
        var status = await _db.AutomationStatus.FirstOrDefaultAsync();
        if (status == null)
        {
            status = new AutomationStatus { IsEnabled = true };
            _db.AutomationStatus.Add(status);
        }
        else
        {
            status.IsEnabled = true;
        }

        await _db.SaveChangesAsync();
        return await GetStatusAsync();
    }

    public async Task<AutomationStatusDto> DisableAutomaticAsync()
    {
        var status = await _db.AutomationStatus.FirstOrDefaultAsync();
        if (status != null)
        {
            status.IsEnabled = false;
            await _db.SaveChangesAsync();
        }

        return await GetStatusAsync();
    }

    public async Task<AutomationStatusDto> UpdateTargetsAsync(AutomationStatusDto targets)
    {
        var status = await _db.AutomationStatus.FirstOrDefaultAsync();
        if (status == null)
        {
            status = new AutomationStatus();
            _db.AutomationStatus.Add(status);
        }

        status.TargetTemperatureMin = targets.TargetTemperatureMin;
        status.TargetTemperatureMax = targets.TargetTemperatureMax;
        status.TargetHumidityMin = targets.TargetHumidityMin;
        status.TargetHumidityMax = targets.TargetHumidityMax;

        await _db.SaveChangesAsync();
        return await GetStatusAsync();
    }

    public async Task EvaluateRulesAsync()
    {
        var automationStatus = await _db.AutomationStatus.FirstOrDefaultAsync();
        if (automationStatus?.IsEnabled != true) return;

        var latestMeasurement = await _db.Measurements
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync();

        if (latestMeasurement == null) return;

        var emailConfigured = _emailSender.IsConfigured();
        var actuators = await _db.Actuators.ToListAsync();

        if (latestMeasurement.TemperatureC < automationStatus.TargetTemperatureMin)
        {
            var heater = actuators.FirstOrDefault(a => a.Type == "heater");
            bool executed = false;
            
            if (heater != null)
            {
                executed = await _actuatorService.ExecuteCommandAsync(heater.Id, new ActionCommandDto { Action = "on" });
            }

            if (emailConfigured)
            {
                await SendAutomationEmailAsync(
                    "⚠️ Alerta: Temperatura abaixo do mínimo",
                    $"A temperatura do ambiente está abaixo do limite configurado.\n\n" +
                    $"📊 Dados atuais:\n" +
                    $"  • Temperatura: {latestMeasurement.TemperatureC:F1}°C\n" +
                    $"  • Limite mínimo: {automationStatus.TargetTemperatureMin:F1}°C\n" +
                    $"  • Diferença: {automationStatus.TargetTemperatureMin - latestMeasurement.TemperatureC:F1}°C abaixo\n\n" +
                    $"🔧 Ação tomada: {(heater != null ? (executed ? $"Aquecedor '{heater.Name}' foi ligado" : "Tentativa de ligar aquecedor falhou") : "Nenhum aquecedor configurado")}\n\n" +
                    $"🕐 Data/Hora: {TimeZoneHelper.GetBrazilTimeFormatted()}"
                );
            }
        }
        else if (latestMeasurement.TemperatureC > automationStatus.TargetTemperatureMax)
        {
            var fan = actuators.FirstOrDefault(a => a.Type == "fan");
            bool executed = false;
            
            if (fan != null)
            {
                executed = await _actuatorService.ExecuteCommandAsync(fan.Id, new ActionCommandDto { Action = "on" });
            }

            if (emailConfigured)
            {
                await SendAutomationEmailAsync(
                    "⚠️ Alerta: Temperatura acima do máximo",
                    $"A temperatura do ambiente está acima do limite configurado.\n\n" +
                    $"📊 Dados atuais:\n" +
                    $"  • Temperatura: {latestMeasurement.TemperatureC:F1}°C\n" +
                    $"  • Limite máximo: {automationStatus.TargetTemperatureMax:F1}°C\n" +
                    $"  • Diferença: {latestMeasurement.TemperatureC - automationStatus.TargetTemperatureMax:F1}°C acima\n\n" +
                    $"🔧 Ação tomada: {(fan != null ? (executed ? $"Ventilador '{fan.Name}' foi ligado" : "Tentativa de ligar ventilador falhou") : "Nenhum ventilador configurado")}\n\n" +
                    $"🕐 Data/Hora: {TimeZoneHelper.GetBrazilTimeFormatted()}"
                );
            }
        }

        if (latestMeasurement.SoilHumidityPct < automationStatus.TargetHumidityMin)
        {
            var pump = actuators.FirstOrDefault(a => a.Type == "pump");
            bool executed = false;
            
            if (pump != null)
            {
                executed = await _actuatorService.ExecuteCommandAsync(pump.Id, new ActionCommandDto { Action = "on" });
            }

            if (emailConfigured)
            {
                await SendAutomationEmailAsync(
                    "💧 Alerta: Umidade do solo abaixo do mínimo",
                    $"A umidade do solo está abaixo do limite configurado.\n\n" +
                    $"📊 Dados atuais:\n" +
                    $"  • Umidade do solo: {latestMeasurement.SoilHumidityPct:F1}%\n" +
                    $"  • Limite mínimo: {automationStatus.TargetHumidityMin:F1}%\n" +
                    $"  • Diferença: {automationStatus.TargetHumidityMin - latestMeasurement.SoilHumidityPct:F1}% abaixo\n\n" +
                    $"🔧 Ação tomada: {(pump != null ? (executed ? $"Bomba de água '{pump.Name}' foi ligada" : "Tentativa de ligar bomba falhou") : "Nenhuma bomba configurada")}\n\n" +
                    $"🕐 Data/Hora: {TimeZoneHelper.GetBrazilTimeFormatted()}"
                );
            }
        }
        else if (latestMeasurement.SoilHumidityPct > automationStatus.TargetHumidityMax)
        {
            var fan = actuators.FirstOrDefault(a => a.Type == "fan");
            bool executed = false;
            
            if (fan != null)
            {
                executed = await _actuatorService.ExecuteCommandAsync(fan.Id, new ActionCommandDto { Action = "on" });
            }

            if (emailConfigured)
            {
                await SendAutomationEmailAsync(
                    "🌊 Alerta: Umidade do solo acima do máximo",
                    $"A umidade do solo está acima do limite configurado.\n\n" +
                    $"📊 Dados atuais:\n" +
                    $"  • Umidade do solo: {latestMeasurement.SoilHumidityPct:F1}%\n" +
                    $"  • Limite máximo: {automationStatus.TargetHumidityMax:F1}%\n" +
                    $"  • Diferença: {latestMeasurement.SoilHumidityPct - automationStatus.TargetHumidityMax:F1}% acima\n\n" +
                    $"⚠️ Atenção: Excesso de água pode prejudicar a planta!\n\n" +
                    $"🔧 Ação tomada: {(fan != null ? (executed ? $"Ventilador '{fan.Name}' foi ligado para secar" : "Tentativa de ligar ventilador falhou") : "Nenhum ventilador configurado")}\n\n" +
                    $"🕐 Data/Hora: {TimeZoneHelper.GetBrazilTimeFormatted()}"
                );
            }
        }

        automationStatus.LastEvaluation = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private async Task SendAutomationEmailAsync(string subject, string bodyText)
    {
        try
        {
            if (!_emailSender.IsConfigured())
            {
                Console.WriteLine("[AutomationService] Email não configurado. Não foi possível enviar notificação.");
                return;
            }

            string headerColor = "#ff9800";
            string icon = "⚠️";
            
            if (subject.Contains("Temperatura abaixo") || subject.Contains("Umidade do solo abaixo"))
            {
                headerColor = "#2196F3";
                icon = "📉";
            }
            else if (subject.Contains("Temperatura acima") || subject.Contains("Umidade do solo acima"))
            {
                headerColor = "#f44336";
                icon = "📈";
            }
            var lines = bodyText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var bodyHtml = string.Join("", lines.Select(line => 
            {
                line = line.Trim();
                if (string.IsNullOrWhiteSpace(line))
                    return "";
                
                if ((line.Contains("📊") || line.Contains("🔧") || line.Contains("🕐") || line.Contains("⚠️") || line.Contains("💧") || line.Contains("🌊")) && line.Contains(":"))
                {
                    return $"<p><strong>{line}</strong></p>";
                }
                if (line.StartsWith("•"))
                {
                    return $"<p>{line}</p>";
                }
                return $"<p>{line}</p>";
            }));
            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: {headerColor}; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 20px; border-radius: 0 0 5px 5px; }}
        .success {{ color: #4CAF50; font-weight: bold; }}
        .info {{ background-color: #e7f3ff; padding: 15px; border-left: 4px solid #2196F3; margin: 15px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{icon} SmartBonsai API</h1>
        </div>
        <div class='content'>
            <p class='success'>{subject}</p>
            <div class='info'>
                {bodyHtml}
            </div>
            <p>Este é um email automático do sistema SmartBonsai. As ações foram executadas automaticamente com base nas configurações de automação.</p>
        </div>
        <div class='footer'>
            <p>SmartBonsai API - Sistema de Monitoramento de Bonsai</p>
        </div>
    </div>
</body>
</html>";

            await _emailSender.SendAsync(subject, htmlBody, isHtml: true);
            Console.WriteLine($"[AutomationService] Email de automação enviado: {subject}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomationService] Erro ao enviar email de automação: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[AutomationService] Detalhes: {ex.InnerException.Message}");
            }
        }
    }
}
