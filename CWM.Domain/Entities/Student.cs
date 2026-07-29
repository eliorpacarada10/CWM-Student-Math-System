using CWM.Domain.Exceptions;

namespace CWM.Domain.Entities;

public class Student
{
    private readonly List<Exam> _exams = new();

    public int Id { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;
    public int TeacherId { get; private set; }
    public IReadOnlyCollection<Exam> Exams => _exams.AsReadOnly();

    private Student()
    {
    }

    public Student(string externalId)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new MathTestDomainException("Student external id is required.");
        }

        ExternalId = externalId;
    }

    public void AddExam(Exam exam)
    {
        _exams.Add(exam);
    }
}
