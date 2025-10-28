using Microsoft.EntityFrameworkCore;

public class SmartRoomContext : DbContext
{
	public SmartRoomContext(DbContextOptions<SmartRoomContext> options) : base(options)
	{
	}

	public DbSet<Measurement> Measurements { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Measurement>().HasKey(m => m.Id);
		modelBuilder.Entity<Measurement>()
			.HasIndex(m => new { m.DeviceId, m.CreatedAt });
	}
}


