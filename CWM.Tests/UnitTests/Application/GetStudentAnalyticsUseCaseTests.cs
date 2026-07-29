using CWM.Application.Exceptions;
using CWM.Application.UseCases;
using CWM.Domain.Entities;
using CWM.Tests.UnitTests.Application.TestDoubles;
using Xunit;

namespace CWM.Tests.UnitTests.Application;

public class GetStudentAnalyticsUseCaseTests
{
    [Fact]
    public async Task HandleAsync_throws_StudentNotFoundException_when_the_student_does_not_exist()
    {
        var useCase = new GetStudentAnalyticsUseCase(new InMemoryExamRepository());

        await Assert.ThrowsAsync<StudentNotFoundException>(
            () => useCase.HandleAsync("does-not-exist", CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_maps_a_students_persisted_exams_without_re_grading()
    {
        var repository = new InMemoryExamRepository();
        var teacher = await repository.GetOrCreateTeacherAsync("11111", CancellationToken.None);
        var students = await repository.GetOrCreateStudentsAsync(teacher, new[] { "12345" }, CancellationToken.None);
        var student = students["12345"];

        var exam = new Exam("1");
        var task = new TaskItem("1", "2+2", 4m);
        task.Grade(4m);
        exam.AddTask(task);
        student.AddExam(exam);

        var useCase = new GetStudentAnalyticsUseCase(repository);

        var result = await useCase.HandleAsync("12345", CancellationToken.None);

        Assert.Equal("12345", result.StudentExternalId);
        var examResult = Assert.Single(result.Exams);
        Assert.Equal(1, examResult.TotalTasks);
        Assert.Equal(1, examResult.CorrectTasks);
        Assert.True(examResult.Tasks.Single().IsCorrect);
    }
}
