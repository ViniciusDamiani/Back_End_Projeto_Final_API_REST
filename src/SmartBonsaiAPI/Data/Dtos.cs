using System;

public class MeasurementCreateDto
{
	public double TemperatureC { get; set; }
	public double HumidityPct { get; set; }
}

public class MeasurementDto
{
	public Guid Id { get; set; }
	public Guid DeviceId { get; set; }
	public double TemperatureC { get; set; }
	public double HumidityPct { get; set; }
	public DateTime CreatedAt { get; set; }
}


