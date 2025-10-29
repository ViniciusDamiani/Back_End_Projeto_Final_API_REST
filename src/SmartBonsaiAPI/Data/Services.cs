using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public interface IMeasurementService
{
	Task<MeasurementDto?> GetLatestByDeviceAsync(Guid deviceId);
	Task<MeasurementDto> CreateAsync(Guid deviceId, MeasurementCreateDto dto);
}

public class MeasurementService : IMeasurementService
{
	private readonly SmartRoomContext _db;

	public MeasurementService(SmartRoomContext db)
	{
		_db = db;
	}

	public async Task<MeasurementDto?> GetLatestByDeviceAsync(Guid deviceId)
	{
		var entity = await _db.Measurements
			.Where(m => m.DeviceId == deviceId)
			.OrderByDescending(m => m.CreatedAt)
			.FirstOrDefaultAsync();

		if (entity == null) return null;

		return new MeasurementDto
		{
			Id = entity.Id,
			DeviceId = entity.DeviceId,
			TemperatureC = entity.TemperatureC,
			HumidityPct = entity.HumidityPct,
			CreatedAt = entity.CreatedAt
		};
	}

	public async Task<MeasurementDto> CreateAsync(Guid deviceId, MeasurementCreateDto dto)
	{
		var entity = new Measurement
		{
			DeviceId = deviceId,
			TemperatureC = dto.TemperatureC,
			HumidityPct = dto.HumidityPct,
			CreatedAt = DateTime.UtcNow
		};

		_db.Measurements.Add(entity);
		await _db.SaveChangesAsync();

		return new MeasurementDto
		{
			Id = entity.Id,
			DeviceId = entity.DeviceId,
			TemperatureC = entity.TemperatureC,
			HumidityPct = entity.HumidityPct,
			CreatedAt = entity.CreatedAt
		};
	}
}

public interface IActuatorService
{
	Task<bool> ExecuteCommandAsync(Guid actuatorId, ActionCommandDto command);
	Task<ActuatorStatusDto?> GetStatusAsync(Guid actuatorId);
}

public class ActuatorService : IActuatorService
{
	private readonly SmartRoomContext _db;

	public ActuatorService(SmartRoomContext db)
	{
		_db = db;
	}

	public async Task<bool> ExecuteCommandAsync(Guid actuatorId, ActionCommandDto command)
	{
		var actuator = await _db.Actuators.FindAsync(actuatorId);
		if (actuator == null) return false;

		switch (command.Action.ToLower())
		{
			case "on":
				actuator.IsActive = true;
				break;
			case "off":
				actuator.IsActive = false;
				break;
			case "set_speed":
				if (command.Value.HasValue)
					actuator.CurrentValue = command.Value.Value;
				break;
		}

		actuator.LastUpdated = DateTime.UtcNow;
		await _db.SaveChangesAsync();
		return true;
	}

	public async Task<ActuatorStatusDto?> GetStatusAsync(Guid actuatorId)
	{
		var actuator = await _db.Actuators.FindAsync(actuatorId);
		if (actuator == null) return null;

		return new ActuatorStatusDto
		{
			Id = actuator.Id,
			Name = actuator.Name,
			Type = actuator.Type,
			IsActive = actuator.IsActive,
			CurrentValue = actuator.CurrentValue,
			LastUpdated = actuator.LastUpdated
		};
	}
}

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
			// Criar configuração padrão
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

		// Buscar última medição (assumindo deviceId fixo para simplicidade)
		var latestMeasurement = await _db.Measurements
			.OrderByDescending(m => m.CreatedAt)
			.FirstOrDefaultAsync();

		if (latestMeasurement == null) return;

		// Buscar atuadores disponíveis
		var actuators = await _db.Actuators.ToListAsync();

		// Regras de automação
        if (latestMeasurement.TemperatureC < automationStatus.TargetTemperatureMin)
		{
			// Ligar aquecedor
			var heater = actuators.FirstOrDefault(a => a.Type == "heater");
			if (heater != null)
			{
                var executed = await _actuatorService.ExecuteCommandAsync(heater.Id, new ActionCommandDto { Action = "on" });
                if (executed)
                {
                    var subject = "Automação: Aquecedor ligado";
                    var body =
                        $"Ação automática executada.\n" +
                        $"Atuador: {heater.Name} (heater)\n" +
                        $"Motivo: Temperatura {latestMeasurement.TemperatureC:F1}°C < alvo mínimo {automationStatus.TargetTemperatureMin:F1}°C\n" +
                        $"Data/Hora (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                    await _emailSender.SendAsync(subject, body);
                }
			}
		}
		else if (latestMeasurement.TemperatureC > automationStatus.TargetTemperatureMax)
		{
			// Ligar ventilador
			var fan = actuators.FirstOrDefault(a => a.Type == "fan");
			if (fan != null)
			{
                var executed = await _actuatorService.ExecuteCommandAsync(fan.Id, new ActionCommandDto { Action = "on" });
                if (executed)
                {
                    var subject = "Automação: Ventilador ligado";
                    var body =
                        $"Ação automática executada.\n" +
                        $"Atuador: {fan.Name} (fan)\n" +
                        $"Motivo: Temperatura {latestMeasurement.TemperatureC:F1}°C > alvo máximo {automationStatus.TargetTemperatureMax:F1}°C\n" +
                        $"Data/Hora (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                    await _emailSender.SendAsync(subject, body);
                }
			}
		}

		if (latestMeasurement.HumidityPct < automationStatus.TargetHumidityMin)
		{
			// Ligar bomba de água
			var pump = actuators.FirstOrDefault(a => a.Type == "pump");
			if (pump != null)
			{
                var executed = await _actuatorService.ExecuteCommandAsync(pump.Id, new ActionCommandDto { Action = "on" });
                if (executed)
                {
                    var subject = "Automação: Bomba de água ligada";
                    var body =
                        $"Ação automática executada.\n" +
                        $"Atuador: {pump.Name} (pump)\n" +
                        $"Motivo: Umidade {latestMeasurement.HumidityPct:F1}% < alvo mínimo {automationStatus.TargetHumidityMin:F1}%\n" +
                        $"Data/Hora (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                    await _emailSender.SendAsync(subject, body);
                }
			}
		}

		automationStatus.LastEvaluation = DateTime.UtcNow;
		await _db.SaveChangesAsync();
	}
}


