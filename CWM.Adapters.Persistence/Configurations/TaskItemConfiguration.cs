using CWM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CWM.Adapters.Persistence.Configurations;

public sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ExternalId)
            .HasMaxLength(50)
            .IsRequired();
        builder.HasIndex(t => new { t.ExamId, t.ExternalId }).IsUnique();

        builder.Property(t => t.Expression)
            .HasMaxLength(500)
            .IsRequired();

        // decimal(18,4): plenty of headroom for exam-arithmetic results while keeping a fixed,
        // predictable scale for the tolerance-based comparison in TaskItem.Grade.
        builder.Property(t => t.ClaimedResult).HasColumnType("decimal(18,4)");
        builder.Property(t => t.ComputedResult).HasColumnType("decimal(18,4)");

        builder.Property(t => t.GradingError).HasMaxLength(500);
    }
}
