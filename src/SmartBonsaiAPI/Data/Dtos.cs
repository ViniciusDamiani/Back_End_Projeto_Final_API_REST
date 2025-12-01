using System;

public class MeasurementCreateDto
{
	public double LightPct { get; set; }
	public double SoilHumidityPct { get; set; }   // usado pelo deviceId 1
    public double AirHumidityPct { get; set; }    // usado pelo deviceId 2
    public double TemperatureC { get; set; }      // usado pelo deviceId 2
	public double WaterFlowLpm { get; set; }       // usado pelo sensor de vazão
	public double WaterVolumeMl { get; set; }      // volume medido na janela de amostragem
}

public class MeasurementDto
{
	public Guid Id { get; set; }
	public int DeviceId { get; set; }
	public double LightPct { get; set; }
	public double SoilHumidityPct { get; set; }
	public double TemperatureC { get; set; }
	public double HumidityPct { get; set; }
	public double WaterFlowLpm { get; set; }
	public double WaterVolumeMl { get; set; }
	public DateTime CreatedAt { get; set; }
}

public class ActionCommandDto
{
	public string Action { get; set; } = string.Empty; // "on", "off", "set_speed"
	public double? Value { get; set; } // Para comandos com valor (ex: velocidade)
}

public class ActuatorStatusDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public bool IsActive { get; set; }
	public double? CurrentValue { get; set; }
	public DateTime LastUpdated { get; set; }
}

public class AutomationStatusDto
{
	//Usado para configurar metas da automação
	public bool IsEnabled { get; set; }
	public double TargetTemperatureMin { get; set; }
	public double TargetTemperatureMax { get; set; }
	public double TargetHumidityMin { get; set; }
	public double TargetHumidityMax { get; set; }
}


