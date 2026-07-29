using System.Net;
using System.Net.Http.Json;
using CWM.Application.Contracts;
using CWM.Application.Ports.Driven;
using CWM.Domain.Entities;
using CWM.Tests.TestSupport;
using Moq;
using Xunit;

namespace CWM.Tests.ComponentTests;

public class ExamsControllerTests : IClassFixture<ComponentWebApplicationFactory>
{
    private readonly ComponentWebApplicationFactory _factory;

    public ExamsControllerTests(ComponentWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Grade_returns_200_with_graded_results_for_a_valid_upload()
    {
        var teacher = new Teacher("11111");
        var student = new Student("12345");

        _factory.RepositoryMock
            .Setup(r => r.GetOrCreateTeacherAsync("11111", It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);
        _factory.RepositoryMock
            .Setup(r => r.GetOrCreateStudentsAsync(
                teacher, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, Student> { ["12345"] = student }
                as IReadOnlyDictionary<string, Student>);
        _factory.RepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var client = _factory.CreateAuthenticatedClient();
        var response = await client.PostAsync(
            "/api/v1/exams/grade", SampleXml.AsUploadContent(SampleXml.SingleStudentTwoTasks));

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GradeExamBatchResult>(JsonOptions.Web);

        Assert.NotNull(result);
        Assert.Equal("11111", result!.TeacherExternalId);
        var exam = result.Students.Single().Exams.Single();
        Assert.Equal(2, exam.TotalTasks);
        // Both sample tasks claim a wrong answer ("2+3/6-4" is really -1.5, not 74) --
        // this asserts real MathEngine evaluation ran, not a stub.
        Assert.Equal(0, exam.CorrectTasks);

        _factory.RepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Grade_returns_401_without_an_api_key()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync(
            "/api/v1/exams/grade", SampleXml.AsUploadContent(SampleXml.SingleStudentTwoTasks));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Grade_returns_400_for_malformed_xml()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsync(
            "/api/v1/exams/grade", SampleXml.AsUploadContent("<Teacher><Students>"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
