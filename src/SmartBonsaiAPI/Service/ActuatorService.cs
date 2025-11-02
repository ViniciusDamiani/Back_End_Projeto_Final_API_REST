using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
