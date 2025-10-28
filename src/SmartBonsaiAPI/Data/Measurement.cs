using System;

public class Measurement
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public Guid DeviceId { get; set; }
	public double TemperatureC { get; set; }
	public double HumidityPct { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


