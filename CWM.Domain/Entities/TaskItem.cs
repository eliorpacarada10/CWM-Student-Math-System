using CWM.Domain.Exceptions;

namespace CWM.Domain.Entities;

/// <summary>
/// A single graded arithmetic question, e.g. "2+3/6-4 = 74".
/// </summary>
public class TaskItem
{
    // Division can produce non-terminating decimals (e.g. 1/3), so grading compares
    // within a small tolerance rather than requiring exact decimal equality.
    private const decimal ToleranceEpsilon = 0.0001m;

    public int Id { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;
    public int ExamId { get; private set; }
    public string Expression { get; private set; } = string.Empty;
    public decimal ClaimedResult { get; private set; }
    public decimal? ComputedResult { get; private set; }
    public bool? IsCorrect { get; private set; }
    public DateTime? GradedAtUtc { get; private set; }

    private TaskItem()
    {
    }

    public TaskItem(string externalId, string expression, decimal claimedResult)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new MathTestDomainException("Task external id is required.");
        }

        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new MathTestDomainException($"Task '{externalId}' has an empty expression.");
        }

        ExternalId = externalId;
        Expression = expression;
        ClaimedResult = claimedResult;
    }

    public void Grade(decimal computedResult)
    {
        ComputedResult = computedResult;
        IsCorrect = Math.Abs(computedResult - ClaimedResult) < ToleranceEpsilon;
        GradedAtUtc = DateTime.UtcNow;
    }
}
