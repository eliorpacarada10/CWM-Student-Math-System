using CWM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CWM.Adapters.Persistence.Configurations;

public sealed class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ExternalId)
            .HasMaxLength(50)
            .IsRequired();
        builder.HasIndex(t => t.ExternalId).IsUnique();

        // Student has no back-navigation to Teacher (only the TeacherId FK scalar), so
        // WithOne() takes no expression.
        builder.HasMany(t => t.Students)
            .WithOne()
            .HasForeignKey(s => s.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);

        // Students is exposed as IReadOnlyCollection<Student> with no public setter, backed by
        // a private List<Student> field -- EF Core must be told to materialize via the field.
        builder.Navigation(t => t.Students)
            .HasField("_students")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
