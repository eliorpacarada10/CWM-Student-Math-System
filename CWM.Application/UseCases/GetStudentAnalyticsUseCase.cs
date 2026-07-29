using CWM.Application.Contracts;
using CWM.Application.Exceptions;
using CWM.Application.Ports.Driven;
using CWM.Application.Ports.Driving;

namespace CWM.Application.UseCases;

public sealed class GetStudentAnalyticsUseCase : IGetStudentAnalyticsUseCase
{
    private readonly IExamRepository _repository;

    public GetStudentAnalyticsUseCase(IExamRepository repository)
    {
        _repository = repository;
    }

    public async Task<StudentAnalyticsResult> HandleAsync(string studentExternalId, CancellationToken cancellationToken)
    {
        var student = await _repository.FindStudentWithExamsAsync(studentExternalId, cancellationToken)
            ?? throw new StudentNotFoundException(studentExternalId);

        var examResults = student.Exams
            .Select(exam => new ExamGradeResult(
                exam.ExternalId,
                exam.TotalTasks,
                exam.CorrectTasks,
                exam.Tasks
                    .Select(task => new TaskGradeResult(
                        task.ExternalId,
                        task.Expression,
                        task.ClaimedResult,
                        task.ComputedResult,
                        task.IsCorrect,
                        task.GradingError))
                    .ToList()))
            .ToList();

        return new StudentAnalyticsResult(student.ExternalId, examResults);
    }
}
