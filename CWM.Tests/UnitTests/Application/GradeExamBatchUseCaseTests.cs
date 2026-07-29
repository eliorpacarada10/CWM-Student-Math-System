using CWM.Application.Contracts;
using CWM.Application.UseCases;
using CWM.Application.Validation;
using CWM.Tests.UnitTests.Application.TestDoubles;
using FluentValidation;
using Xunit;

namespace CWM.Tests.UnitTests.Application;

public class GradeExamBatchUseCaseTests
{
    private static GradeExamBatchUseCase CreateUseCase(
        FakeArithmeticEvaluator evaluator, InMemoryExamRepository repository) =>
        new(evaluator, repository, new GradeExamBatchRequestValidator());

    private static TaskSubmission Task(string id, string expression, decimal claimed) =>
        new(id, expression, claimed);

    [Fact]
    public async Task HandleAsync_grades_correct_and_incorrect_tasks()
    {
        var evaluator = new FakeArithmeticEvaluator()
            .WithResult("2+2", 4m)
            .WithResult("2+3", 999m); // student claimed the wrong answer
        var repository = new InMemoryExamRepository();
        var useCase = CreateUseCase(evaluator, repository);

        var request = new GradeExamBatchRequest("11111", new[]
        {
            new StudentSubmission("12345", new[]
            {
                new ExamSubmission("1", new[]
                {
                    Task("1", "2+2", 4m),
                    Task("2", "2+3", 5m),
                })
            })
        });

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        var exam = result.Students.Single().Exams.Single();
        Assert.Equal(2, exam.TotalTasks);
        Assert.Equal(1, exam.CorrectTasks);
        Assert.True(exam.Tasks[0].IsCorrect);
        Assert.False(exam.Tasks[1].IsCorrect);
    }

    [Fact]
    public async Task HandleAsync_records_an_ungradable_task_without_failing_the_rest_of_the_batch()
    {
        var evaluator = new FakeArithmeticEvaluator()
            .WithFailure("2+*")
            .WithResult("6*2+3-4", 22m);
        var repository = new InMemoryExamRepository();
        var useCase = CreateUseCase(evaluator, repository);

        var request = new GradeExamBatchRequest("11111", new[]
        {
            new StudentSubmission("12345", new[]
            {
                new ExamSubmission("1", new[]
                {
                    Task("1", "2+*", 4m),
                    Task("2", "6*2+3-4", 22m),
                })
            })
        });

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        var tasks = result.Students.Single().Exams.Single().Tasks;
        Assert.False(tasks[0].IsCorrect);
        Assert.NotNull(tasks[0].GradingError);
        Assert.Null(tasks[0].ComputedResult);

        Assert.True(tasks[1].IsCorrect);
    }

    [Fact]
    public async Task HandleAsync_batches_student_lookup_into_a_single_call_regardless_of_batch_size()
    {
        // The N+1 fix: GetOrCreateStudentsAsync must be called exactly once for the whole
        // upload, not once per student, no matter how many students are in it.
        var evaluator = new FakeArithmeticEvaluator().WithResult("1+1", 2m);
        var repository = new InMemoryExamRepository();
        var useCase = CreateUseCase(evaluator, repository);

        var students = Enumerable.Range(1, 50)
            .Select(i => new StudentSubmission(
                $"student-{i}",
                new[] { new ExamSubmission("1", new[] { Task("1", "1+1", 2m) }) }))
            .ToList();

        await useCase.HandleAsync(new GradeExamBatchRequest("11111", students), CancellationToken.None);

        Assert.Equal(1, repository.GetOrCreateStudentsCallCount);
    }

    [Fact]
    public async Task HandleAsync_commits_the_whole_batch_in_a_single_SaveChangesAsync_call()
    {
        var evaluator = new FakeArithmeticEvaluator().WithResult("1+1", 2m);
        var repository = new InMemoryExamRepository();
        var useCase = CreateUseCase(evaluator, repository);

        var students = Enumerable.Range(1, 10)
            .Select(i => new StudentSubmission(
                $"student-{i}",
                new[] { new ExamSubmission("1", new[] { Task("1", "1+1", 2m) }) }))
            .ToList();

        await useCase.HandleAsync(new GradeExamBatchRequest("11111", students), CancellationToken.None);

        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_throws_ValidationException_when_the_batch_has_no_students()
    {
        var useCase = CreateUseCase(new FakeArithmeticEvaluator(), new InMemoryExamRepository());
        var request = new GradeExamBatchRequest("11111", Array.Empty<StudentSubmission>());

        await Assert.ThrowsAsync<ValidationException>(() => useCase.HandleAsync(request, CancellationToken.None));
    }
}
