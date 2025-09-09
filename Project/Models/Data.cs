using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Project.Models
{
    public class MaintenanceTask
    {
        public int Id { get; set; }
        
        public DateTime DueDate { get; set; }

        public string Text { get; set; }
        
        [JsonPropertyName("type")]
        public int MaintenanceTypeId { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        public List<MaintenanceTaskItem> TaskItems { get; set; }
        
        public MaintenanceTask? Next { get; set; }
        
        public MaintenanceTask()
        {
            TaskItems = new List<MaintenanceTaskItem>();
        }
    }

    public class MaintenanceType
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Period { get; set; }

        public string? Color { get; set; }
        
        public ICollection<MaintenanceTypeItem> TypeItems { get; set; }
    }

    public class MaintenanceTypeItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int MaintenanceTypeId { get; set; }
        
        public MaintenanceType MaintenanceType { get; set; }
        
        
    }

    public class MaintenanceTaskItem
    {
        public int Id { get; set; }
        
        public int MaintenanceTypeItemId { get; set; }
        public MaintenanceTypeItem MaintenanceTypeItem { get; set; }
        public bool Checked { get; set; }
    }

    public class MaintenanceDbContext : DbContext
    {
        public DbSet<MaintenanceTask> Tasks { get; set; }
        public DbSet<MaintenanceTaskItem> TaskItems { get; set; }
        public DbSet<MaintenanceType> Types { get; set; }
        public DbSet<MaintenanceTypeItem> TypeItems { get; set; }

        public MaintenanceDbContext(DbContextOptions<MaintenanceDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MaintenanceType>().HasData(new MaintenanceType
                { Id = 1, Name = "Basic Cleanup", Period = "1w", Color = "#6aa84f"});
            modelBuilder.Entity<MaintenanceType>().HasData(new MaintenanceType
                { Id = 2, Name = "Safety Inspection", Period = "1m", Color = "#f1c232"});
            modelBuilder.Entity<MaintenanceType>().HasData(new MaintenanceType
                { Id = 3, Name = "Machine Calibration", Period = "3m", Color = "#4a86e8" });
            modelBuilder.Entity<MaintenanceType>().HasData(new MaintenanceType
                { Id = 4, Name = "Preventive Maintenance", Period = "6m", Color = "#e06666" });

            modelBuilder.Entity<MaintenanceTypeItem>().HasData(new MaintenanceTypeItem
                { Id = 1, Name = "Clean and sanitize work surfaces", MaintenanceTypeId = 1 });
            modelBuilder.Entity<MaintenanceTypeItem>().HasData(new MaintenanceTypeItem
                { Id = 2, Name = "Remove waste material", MaintenanceTypeId = 1 });

            modelBuilder.Entity<MaintenanceTypeItem>().HasData(new MaintenanceTypeItem
                { Id = 3, Name = "Inspect safety equipment", MaintenanceTypeId = 2 });
            modelBuilder.Entity<MaintenanceTypeItem>().HasData(new MaintenanceTypeItem
                { Id = 4, Name = "Check emergency exits", MaintenanceTypeId = 2 });
            modelBuilder.Entity<MaintenanceTypeItem>().HasData(new MaintenanceTypeItem
                { Id = 5, Name = "Test fire alarms", MaintenanceTypeId = 2 });
            modelBuilder.Entity<MaintenanceTypeItem>().HasData(new MaintenanceTypeItem
                { Id = 6, Name = "Inspect first aid kits", MaintenanceTypeId = 2 });

            modelBuilder.Entity<MaintenanceTypeItem>().HasData(new MaintenanceTypeItem
                { Id = 7, Name = "Calibrate machine sensors", MaintenanceTypeId = 3 });
            modelBuilder.Entity<MaintenanceTypeItem>().HasData(new MaintenanceTypeItem
                { Id = 8, Name = "Check machine alignment", MaintenanceTypeId = 3 });
            modelBuilder.Entity<MaintenanceTypeItem>().HasData(new MaintenanceTypeItem
                { Id = 9, Name = "Check for any abnormal sounds", MaintenanceTypeId = 3 });

            modelBuilder.Entity<MaintenanceTypeItem>().HasData(new MaintenanceTypeItem
                { Id = 10, Name = "Inspect for wear and tear", MaintenanceTypeId = 4 });
            modelBuilder.Entity<MaintenanceTypeItem>().HasData(new MaintenanceTypeItem
                { Id = 11, Name = "Replace worn parts", MaintenanceTypeId = 4 });
            modelBuilder.Entity<MaintenanceTypeItem>().HasData(new MaintenanceTypeItem
                { Id = 12, Name = "Check for leaks", MaintenanceTypeId = 4 });
            modelBuilder.Entity<MaintenanceTypeItem>().HasData(new MaintenanceTypeItem
                { Id = 13, Name = "Lubricate moving parts", MaintenanceTypeId = 4 });
        }
    }
}