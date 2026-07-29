using CWM.Application.Contracts;

namespace CWM.Application.Ports.Driving;

/// <summary>
/// Requirement: "analytics UI for students to review their exams and see which tasks are
/// correct". Purely a read of already-persisted grading results.
/// </summary>
public interface IGetStudentAnalyticsUseCase
{
    Task<StudentAnalyticsResult> HandleAsync(string studentExternalId, CancellationToken cancellationToken);
}
