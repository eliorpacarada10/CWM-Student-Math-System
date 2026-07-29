using CWM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CWM.Adapters.Persistence.Configurations;

public sealed class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.ToTable("Exams");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ExternalId)
            .HasMaxLength(50)
            .IsRequired();

        // An exam id (e.g. "1") is only unique within one student's own submissions, unlike
        // Teacher/Student external ids which are assumed globally unique.
        builder.HasIndex(e => new { e.StudentId, e.ExternalId }).IsUnique();

        builder.HasMany(e => e.Tasks)
            .WithOne()
            .HasForeignKey(t => t.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Tasks)
            .HasField("_tasks")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Computed in-memory from the Tasks collection, not persisted columns.
        builder.Ignore(e => e.TotalTasks);
        builder.Ignore(e => e.CorrectTasks);
    }
}
