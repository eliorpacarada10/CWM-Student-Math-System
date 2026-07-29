using System.Net;
using System.Net.Http.Json;
using CWM.Application.Contracts;
using CWM.Domain.Entities;
using CWM.Tests.TestSupport;
using Moq;
using Xunit;

namespace CWM.Tests.ComponentTests;

public class StudentsControllerTests : IClassFixture<ComponentWebApplicationFactory>
{
    private readonly ComponentWebApplicationFactory _factory;

    public StudentsControllerTests(ComponentWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAnalytics_returns_200_with_the_students_graded_exams()
    {
        var student = new Student("12345");
        var exam = new Exam("1");
        var task = new TaskItem("1", "2+2", 4m);
        task.Grade(4m);
        exam.AddTask(task);
        student.AddExam(exam);

        _factory.RepositoryMock
            .Setup(r => r.FindStudentWithExamsAsync("12345", It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        var client = _factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/v1/students/12345/analytics");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<StudentAnalyticsResult>(JsonOptions.Web);

        Assert.NotNull(result);
        Assert.Equal("12345", result!.StudentExternalId);
        Assert.True(result.Exams.Single().Tasks.Single().IsCorrect);
    }

    [Fact]
    public async Task GetAnalytics_returns_404_for_an_unknown_student()
    {
        _factory.RepositoryMock
            .Setup(r => r.FindStudentWithExamsAsync("unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        var client = _factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/v1/students/unknown/analytics");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
