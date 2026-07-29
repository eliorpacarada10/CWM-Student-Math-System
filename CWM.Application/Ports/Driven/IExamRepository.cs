using CWM.Domain.Entities;

namespace CWM.Application.Ports.Driven;

/// <summary>
/// Persistence port. CWM.Adapters.Persistence implements this with EF Core + SQL Server;
/// Application never references EF Core directly. SaveChangesAsync is a deliberate,
/// explicit Unit-of-Work seam: the use case stages every student/exam in an aggregated
/// upload via GetOrCreate*/AddExam, then calls SaveChangesAsync exactly once so the whole
/// batch commits atomically -- one bad student's data does not get partially persisted
/// alongside the good ones.
/// </summary>
public interface IExamRepository
{
    Task<Teacher> GetOrCreateTeacherAsync(string teacherExternalId, CancellationToken cancellationToken);

    /// <summary>
    /// Batch get-or-create for every student in one aggregated upload -- a single query for
    /// the whole external-id list, not one query per student. A mass upload of 500 students
    /// previously meant 500 round-trips; this is the fix.
    /// </summary>
    Task<IReadOnlyDictionary<string, Student>> GetOrCreateStudentsAsync(
        Teacher teacher, IReadOnlyCollection<string> studentExternalIds, CancellationToken cancellationToken);

    void AddExam(Student student, Exam exam);

    Task<Student?> FindStudentWithExamsAsync(string studentExternalId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
