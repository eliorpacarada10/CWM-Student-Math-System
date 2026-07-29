using CWM.Application.Ports.Driven;
using CWM.Domain.Entities;

namespace CWM.Tests.UnitTests.Application.TestDoubles;

/// <summary>
/// A fully in-memory IExamRepository -- no EF Core, no I/O. Also tracks call counts so tests
/// can assert on the *shape* of the interaction (e.g. "one batch lookup, one save"), not just
/// the end result.
/// </summary>
public sealed class InMemoryExamRepository : IExamRepository
{
    private readonly List<Teacher> _teachers = new();
    private readonly List<Student> _students = new();

    public int GetOrCreateStudentsCallCount { get; private set; }
    public int SaveChangesCallCount { get; private set; }

    public Task<Teacher> GetOrCreateTeacherAsync(string teacherExternalId, CancellationToken cancellationToken)
    {
        var teacher = _teachers.FirstOrDefault(t => t.ExternalId == teacherExternalId);
        if (teacher is null)
        {
            teacher = new Teacher(teacherExternalId);
            _teachers.Add(teacher);
        }

        return Task.FromResult(teacher);
    }

    public Task<IReadOnlyDictionary<string, Student>> GetOrCreateStudentsAsync(
        Teacher teacher, IReadOnlyCollection<string> studentExternalIds, CancellationToken cancellationToken)
    {
        GetOrCreateStudentsCallCount++;

        var result = new Dictionary<string, Student>();
        foreach (var externalId in studentExternalIds)
        {
            var student = _students.FirstOrDefault(s => s.ExternalId == externalId);
            if (student is null)
            {
                student = new Student(externalId);
                teacher.AddStudent(student);
                _students.Add(student);
            }

            result[externalId] = student;
        }

        return Task.FromResult<IReadOnlyDictionary<string, Student>>(result);
    }

    public void AddExam(Student student, Exam exam)
    {
        // No-op: the use case already links the exam via student.AddExam before calling this;
        // there's no separate in-memory "store" to add it to.
    }

    public Task<Student?> FindStudentWithExamsAsync(string studentExternalId, CancellationToken cancellationToken) =>
        Task.FromResult(_students.FirstOrDefault(s => s.ExternalId == studentExternalId));

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }
}
