using CWM.Application.Contracts;
using CWM.Application.Exceptions;
using CWM.Application.Ports.Driven;
using CWM.Application.Ports.Driving;
using CWM.Domain.Entities;
using FluentValidation;

namespace CWM.Application.UseCases;

/// <summary>
/// Orchestrates grading a batch (one student or a full mass upload -- same code path).
/// This class does not compute anything itself: arithmetic goes through IArithmeticEvaluator,
/// the correctness rule lives on TaskItem (Domain), persistence goes through IExamRepository.
/// What this class *does* own: sequencing those calls, batching the student lookup so a mass
/// upload costs one query instead of one-per-student, and deciding that one ungradable task
/// must not fail the rest of the batch.
/// </summary>
public sealed class GradeExamBatchUseCase : IGradeExamBatchUseCase
{
    private readonly IArithmeticEvaluator _evaluator;
    private readonly IExamRepository _repository;
    private readonly IValidator<GradeExamBatchRequest> _validator;

    public GradeExamBatchUseCase(
        IArithmeticEvaluator evaluator,
        IExamRepository repository,
        IValidator<GradeExamBatchRequest> validator)
    {
        _evaluator = evaluator;
        _repository = repository;
        _validator = validator;
    }

    public async Task<GradeExamBatchResult> HandleAsync(GradeExamBatchRequest request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var teacher = await _repository.GetOrCreateTeacherAsync(request.TeacherExternalId, cancellationToken);

        var studentExternalIds = request.Students
            .Select(s => s.StudentExternalId)
            .Distinct()
            .ToList();
        
        var studentsByExternalId = await _repository.GetOrCreateStudentsAsync(
            teacher, studentExternalIds, cancellationToken);

        var studentResults = request.Students
            .Select(studentSubmission => GradeStudent(
                studentsByExternalId[studentSubmission.StudentExternalId], studentSubmission))
            .ToList();

        // One commit for the entire aggregated upload: all students/exams in this batch
        // persist together, or none do.
        await _repository.SaveChangesAsync(cancellationToken);

        return new GradeExamBatchResult(teacher.ExternalId, studentResults);
    }

    private StudentGradeResult GradeStudent(Student student, StudentSubmission studentSubmission)
    {
        var examResults = studentSubmission.Exams
            .Select(examSubmission => GradeExam(student, examSubmission))
            .ToList();

        return new StudentGradeResult(student.ExternalId, examResults);
    }

    private ExamGradeResult GradeExam(Student student, ExamSubmission examSubmission)
    {
        var exam = new Exam(examSubmission.ExamExternalId);
        var taskResults = examSubmission.Tasks
            .Select(taskSubmission => GradeTask(exam, taskSubmission))
            .ToList();

        student.AddExam(exam);
        _repository.AddExam(student, exam);

        return new ExamGradeResult(exam.ExternalId, exam.TotalTasks, exam.CorrectTasks, taskResults);
    }

    private TaskGradeResult GradeTask(Exam exam, TaskSubmission taskSubmission)
    {
        var task = new TaskItem(taskSubmission.TaskExternalId, taskSubmission.Expression, taskSubmission.ClaimedResult);

        try
        {
            var computed = _evaluator.Evaluate(taskSubmission.Expression);
            task.Grade(computed);
        }
        catch (ArithmeticEvaluationException ex)
        {
            task.MarkGradingFailed(ex.Message);
        }

        exam.AddTask(task);

        return new TaskGradeResult(
            task.ExternalId, task.Expression, task.ClaimedResult, task.ComputedResult, task.IsCorrect, task.GradingError);
    }
}
