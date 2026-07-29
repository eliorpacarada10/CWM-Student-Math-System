namespace CWM.Adapters.Web.Models;

public sealed record StudentAnalyticsResult(string StudentExternalId, IReadOnlyList<ExamGradeResult> Exams);
