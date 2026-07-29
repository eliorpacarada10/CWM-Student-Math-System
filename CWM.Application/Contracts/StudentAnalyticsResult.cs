namespace CWM.Application.Contracts;

/// <summary>
/// The student analytics view: every exam the student has submitted, with per-task
/// correctness already computed -- this is a read of persisted grading results, no
/// re-evaluation happens here. Reuses ExamGradeResult/TaskGradeResult so a "just graded"
/// response and a "historical analytics" response share one vocabulary.
/// </summary>
public sealed record StudentAnalyticsResult(string StudentExternalId, IReadOnlyList<ExamGradeResult> Exams);
