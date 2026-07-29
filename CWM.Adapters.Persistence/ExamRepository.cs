using CWM.Application.Exceptions;
using CWM.Application.Ports.Driven;
using CWM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CWM.Adapters.Persistence;

public sealed class ExamRepository : IExamRepository
{
    private readonly CwmDbContext _context;

    public ExamRepository(CwmDbContext context)
    {
        _context = context;
    }

    public async Task<Teacher> GetOrCreateTeacherAsync(string teacherExternalId, CancellationToken cancellationToken)
    {
        var teacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.ExternalId == teacherExternalId, cancellationToken);

        if (teacher is not null)
        {
            return teacher;
        }

        teacher = new Teacher(teacherExternalId);
        _context.Teachers.Add(teacher);
        return teacher;
    }

    public async Task<IReadOnlyDictionary<string, Student>> GetOrCreateStudentsAsync(
        Teacher teacher, IReadOnlyCollection<string> studentExternalIds, CancellationToken cancellationToken)
    {
        var existingStudents = await _context.Students
            .Where(s => studentExternalIds.Contains(s.ExternalId))
            .ToListAsync(cancellationToken);

        var studentsByExternalId = existingStudents.ToDictionary(s => s.ExternalId);

        foreach (var externalId in studentExternalIds)
        {
            if (studentsByExternalId.ContainsKey(externalId))
            {
                continue;
            }

            var student = new Student(externalId);
            teacher.AddStudent(student);
            _context.Students.Add(student);
            studentsByExternalId[externalId] = student;
        }

        return studentsByExternalId;
    }

    public void AddExam(Student student, Exam exam)
    {
        _context.Exams.Add(exam);
    }

    public async Task<Student?> FindStudentWithExamsAsync(string studentExternalId, CancellationToken cancellationToken)
    {
        // Read-only view for analytics -- AsNoTracking avoids paying for change-tracking on
        // data that's only ever being displayed back, never mutated.
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.Exams)
            .ThenInclude(e => e.Tasks)
            .FirstOrDefaultAsync(s => s.ExternalId == studentExternalId, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // Most likely cause: an exam already exists for (StudentId, ExternalId) --
            // a teacher re-submitting an exam that was already graded for that student.
            throw new DuplicateExamSubmissionException(
                "One or more exams in this upload were already submitted for their student.", ex);
        }
    }
}
