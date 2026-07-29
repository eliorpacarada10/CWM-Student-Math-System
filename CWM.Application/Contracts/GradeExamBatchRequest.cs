namespace CWM.Application.Contracts;

/// <summary>
/// Application's own request shape for grading a batch of exams. A single upload with one
/// student is just a list of length one -- there is no separate "mass upload" contract.
/// </summary>
public sealed record GradeExamBatchRequest(string TeacherExternalId, IReadOnlyList<StudentSubmission> Students);

public sealed record StudentSubmission(string StudentExternalId, IReadOnlyList<ExamSubmission> Exams);

public sealed record ExamSubmission(string ExamExternalId, IReadOnlyList<TaskSubmission> Tasks);

public sealed record TaskSubmission(string TaskExternalId, string Expression, decimal ClaimedResult);
