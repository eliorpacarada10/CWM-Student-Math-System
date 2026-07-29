namespace CWM.Application.Contracts;

public sealed record GradeExamBatchResult(string TeacherExternalId, IReadOnlyList<StudentGradeResult> Students);

public sealed record StudentGradeResult(string StudentExternalId, IReadOnlyList<ExamGradeResult> Exams);

public sealed record ExamGradeResult(
    string ExamExternalId,
    int TotalTasks,
    int CorrectTasks,
    IReadOnlyList<TaskGradeResult> Tasks);

/// <summary>
/// GradingError is set (and IsCorrect/ComputedResult are null) when the expression itself
/// could not be evaluated -- an ungradable task, not a thrown exception. See
/// TaskItem.MarkGradingFailed for why that distinction matters for mass uploads.
/// </summary>
public sealed record TaskGradeResult(
    string TaskExternalId,
    string Expression,
    decimal ClaimedResult,
    decimal? ComputedResult,
    bool? IsCorrect,
    string? GradingError);
