using CWM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CWM.Adapters.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ExternalId)
            .HasMaxLength(50)
            .IsRequired();

        // Assumption (see CLAUDE.md): student external ids are globally unique, not just
        // unique per teacher -- this is what lets GET /students/{id}/analytics look a student
        // up without a teacher id in the route.
        builder.HasIndex(s => s.ExternalId).IsUnique();

        builder.HasMany(s => s.Exams)
            .WithOne()
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Exams)
            .HasField("_exams")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
