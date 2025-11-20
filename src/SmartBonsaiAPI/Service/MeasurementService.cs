using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public interface IMeasurementService
{
    Task<MeasurementDto?> GetLatestByDeviceAsync(int deviceId);
    Task<MeasurementDto> CreateAsync(int deviceId, MeasurementCreateDto dto);
    Task<List<MeasurementDto>> GetHistoryAsync(int deviceId, int limit = 100);
}

public class MeasurementService : IMeasurementService
{
    private readonly SmartRoomContext _db;

    public MeasurementService(SmartRoomContext db)
    {
        _db = db;
    }

    public async Task<MeasurementDto?> GetLatestByDeviceAsync(int deviceId)
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
            LightPct = entity.LightPct,
            SoilHumidityPct = entity.SoilHumidityPct,
            TemperatureC = entity.TemperatureC,
            HumidityPct = entity.HumidityPct,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task<MeasurementDto> CreateAsync(int deviceId, MeasurementCreateDto dto)
    {
        var entity = new Measurement
        {
            DeviceId = deviceId,
            LightPct = dto.LightPct,
            SoilHumidityPct = dto.SoilHumidityPct,
            TemperatureC = dto.TemperatureC,
            HumidityPct = dto.AirHumidityPct,
            CreatedAt = DateTime.UtcNow
        };

        _db.Measurements.Add(entity);
        await _db.SaveChangesAsync();

        return new MeasurementDto
        {
            Id = entity.Id,
            DeviceId = entity.DeviceId,
            LightPct = dto.LightPct,
            SoilHumidityPct = entity.SoilHumidityPct,
            TemperatureC = entity.TemperatureC,
            HumidityPct = entity.HumidityPct,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task<List<MeasurementDto>> GetHistoryAsync(int deviceId, int limit = 100)
    {
        var entities = await _db.Measurements
            .Where(m => m.DeviceId == deviceId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return entities.Select(e => new MeasurementDto
        {
            Id = e.Id,
            DeviceId = e.DeviceId,
            LightPct = e.LightPct,
            SoilHumidityPct = e.SoilHumidityPct,
            TemperatureC = e.TemperatureC,
            HumidityPct = e.HumidityPct,
            CreatedAt = e.CreatedAt
        }).OrderBy(m => m.CreatedAt).ToList(); // Ordenar por data crescente para gráficos
    }
}
