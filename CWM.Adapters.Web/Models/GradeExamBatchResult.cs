namespace CWM.Adapters.Web.Models;

/// <summary>
/// Mirrors CWM.Application.Contracts.GradeExamBatchResult's JSON shape. Deliberately
/// duplicated rather than referencing Application directly -- Web has zero project
/// references into the hexagon core, and calls the Api exactly like any other HTTP client
/// would. The cost of that decoupling is this file staying in sync with the wire contract.
/// </summary>
public sealed record GradeExamBatchResult(string TeacherExternalId, IReadOnlyList<StudentGradeResult> Students);

public sealed record StudentGradeResult(string StudentExternalId, IReadOnlyList<ExamGradeResult> Exams);

public sealed record ExamGradeResult(
    string ExamExternalId,
    int TotalTasks,
    int CorrectTasks,
    IReadOnlyList<TaskGradeResult> Tasks);

public sealed record TaskGradeResult(
    string TaskExternalId,
    string Expression,
    decimal ClaimedResult,
    decimal? ComputedResult,
    bool? IsCorrect,
    string? GradingError);
