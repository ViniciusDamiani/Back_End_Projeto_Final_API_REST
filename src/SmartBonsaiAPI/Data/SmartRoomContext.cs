using Microsoft.EntityFrameworkCore;

public class SmartRoomContext : DbContext
{
	public SmartRoomContext(DbContextOptions<SmartRoomContext> options) : base(options)
	{
	}

	public DbSet<Measurement> Measurements { get; set; } = null!;
	public DbSet<Actuator> Actuators { get; set; } = null!;
	public DbSet<AutomationStatus> AutomationStatus { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Measurement>().HasKey(m => m.Id);
		modelBuilder.Entity<Measurement>()
			.HasIndex(m => new { m.DeviceId, m.CreatedAt });

		modelBuilder.Entity<Actuator>().HasKey(a => a.Id);
		modelBuilder.Entity<Actuator>()
			.HasIndex(a => a.Type);

		modelBuilder.Entity<AutomationStatus>().HasKey(a => a.Id);
	}
}



