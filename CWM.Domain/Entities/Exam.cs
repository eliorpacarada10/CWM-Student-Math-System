using CWM.Domain.Exceptions;

namespace CWM.Domain.Entities;

/// <summary>
/// One exam submission for a student, containing the graded tasks from that submission.
/// </summary>
public class Exam
{
    private readonly List<TaskItem> _tasks = new();

    public int Id { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;
    public int StudentId { get; private set; }
    public DateTime SubmittedAtUtc { get; private set; }
    public IReadOnlyCollection<TaskItem> Tasks => _tasks.AsReadOnly();

    public int TotalTasks => _tasks.Count;
    public int CorrectTasks => _tasks.Count(t => t.IsCorrect == true);

    private Exam()
    {
    }

    public Exam(string externalId)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new MathTestDomainException("Exam external id is required.");
        }

        ExternalId = externalId;
        SubmittedAtUtc = DateTime.UtcNow;
    }

    public void AddTask(TaskItem task)
    {
        if (_tasks.Any(t => t.ExternalId == task.ExternalId))
        {
            throw new MathTestDomainException(
                $"Exam '{ExternalId}' already contains a task with id '{task.ExternalId}'.");
        }

        _tasks.Add(task);
    }
}
