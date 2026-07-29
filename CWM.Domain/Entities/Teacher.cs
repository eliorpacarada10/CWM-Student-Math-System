using CWM.Domain.Exceptions;

namespace CWM.Domain.Entities;

public class Teacher
{
    private readonly List<Student> _students = new();

    public int Id { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;
    public IReadOnlyCollection<Student> Students => _students.AsReadOnly();

    private Teacher()
    {
    }

    public Teacher(string externalId)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new MathTestDomainException("Teacher external id is required.");
        }

        ExternalId = externalId;
    }

    public void AddStudent(Student student)
    {
        _students.Add(student);
    }
}
