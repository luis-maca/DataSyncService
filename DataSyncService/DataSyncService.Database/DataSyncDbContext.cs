using DataSyncService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataSyncService.Database
{
	public class DataSyncDbContext : DbContext
	{

		public DataSyncDbContext(DbContextOptions<DataSyncDbContext> options) : base(options) { }

		public DbSet<Configuration> Configuration { get; set; } = null!;
		public DbSet<SyncLog> SyncLog { get; set; } = null!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataSyncDbContext).Assembly);

			modelBuilder.Entity<Configuration>(entity =>
			{
				entity.ToTable("Configuration");
				entity.HasKey(x => x.Id);

				entity.Property(x => x.Name)
					.IsRequired()
					.HasMaxLength(128);

				entity.Property(x => x.Value)
					.IsRequired()
					.HasMaxLength(256);
			});

			modelBuilder.Entity<SyncLog>(entity =>
			{
				entity.ToTable("SyncLog");
				entity.HasKey(x => x.Id);

				entity.Property(x => x.Name)
					.IsRequired()
					.HasMaxLength(128);

				entity.Property(x => x.Data)
					.IsRequired();
			});
		}

	}
}
