using System;

public class Measurement
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public Guid DeviceId { get; set; }
	public double TemperatureC { get; set; }
	public double HumidityPct { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Actuator
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public string Name { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty; // "pump", "fan", "heater", "light"
	public bool IsActive { get; set; }
	public double? CurrentValue { get; set; } // Para atuadores com valores (ex: velocidade do ventilador)
	public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class AutomationStatus
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public bool IsEnabled { get; set; }
	public double TargetTemperatureMin { get; set; }
	public double TargetTemperatureMax { get; set; }
	public double TargetHumidityMin { get; set; }
	public double TargetHumidityMax { get; set; }
	public DateTime LastEvaluation { get; set; } = DateTime.UtcNow;
}



