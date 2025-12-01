using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public interface IMeasurementService
{
    Task<MeasurementDto?> GetLatestByDeviceAsync(int deviceId);
    Task<MeasurementDto> CreateAsync(int deviceId, MeasurementCreateDto dto);
    Task<List<MeasurementDto>> GetHistoryAsync(int deviceId, int limit = 100);
}

public class MeasurementService : IMeasurementService
{
    private readonly SmartRoomContext _db;
    private readonly IServiceScopeFactory? _serviceScopeFactory;

    public MeasurementService(SmartRoomContext db, IServiceScopeFactory? serviceScopeFactory = null)
    {
        _db = db;
        _serviceScopeFactory = serviceScopeFactory;
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
            WaterFlowLpm = entity.WaterFlowLpm,
            WaterVolumeMl = entity.WaterVolumeMl,
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
            WaterFlowLpm = dto.WaterFlowLpm,
            WaterVolumeMl = dto.WaterVolumeMl,
            CreatedAt = DateTime.UtcNow
        };

        _db.Measurements.Add(entity);
        await _db.SaveChangesAsync();

        // Avaliar regras de automação automaticamente após criar medição
        // (executar em background para não bloquear a resposta)
        if (_serviceScopeFactory != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var automationService = scope.ServiceProvider.GetRequiredService<IAutomationService>();
                    await automationService.EvaluateRulesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MeasurementService] Erro ao avaliar regras de automação: {ex.Message}");
                }
            });
        }

        return new MeasurementDto
        {
            Id = entity.Id,
            DeviceId = entity.DeviceId,
            LightPct = entity.LightPct,
            SoilHumidityPct = entity.SoilHumidityPct,
            TemperatureC = entity.TemperatureC,
            HumidityPct = entity.HumidityPct,
            WaterFlowLpm = entity.WaterFlowLpm,
            WaterVolumeMl = entity.WaterVolumeMl,
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
            WaterFlowLpm = e.WaterFlowLpm,
            WaterVolumeMl = e.WaterVolumeMl,
            CreatedAt = e.CreatedAt
        }).OrderBy(m => m.CreatedAt).ToList();
    }
}
