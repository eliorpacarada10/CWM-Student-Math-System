using CWM.Adapters.Persistence.Configurations;
using CWM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CWM.Adapters.Persistence;

public sealed class CwmDbContext : DbContext
{
    public CwmDbContext(DbContextOptions<CwmDbContext> options) : base(options)
    {
    }

    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TeacherConfiguration());
        modelBuilder.ApplyConfiguration(new StudentConfiguration());
        modelBuilder.ApplyConfiguration(new ExamConfiguration());
        modelBuilder.ApplyConfiguration(new TaskItemConfiguration());
    }
}
