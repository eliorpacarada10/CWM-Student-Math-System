using System.Net;
using System.Net.Http.Json;
using CWM.Application.Contracts;
using CWM.Tests.TestSupport;
using Xunit;

namespace CWM.Tests.IntegrationTests;

/// <summary>
/// Full stack, real SQLite-backed persistence, real MathEngine, real XmlParsing -- driven
/// entirely through HTTP, the same way any real caller (UI or third party) would use it.
/// Each test owns its own IntegrationWebApplicationFactory (a fresh in-memory database) --
/// see that class for why sharing one via IClassFixture would leak state between tests.
/// </summary>
public class ExamGradingFlowTests
{
    [Fact]
    public async Task Uploading_an_exam_persists_it_so_analytics_reflects_the_same_grade()
    {
        using var factory = new IntegrationWebApplicationFactory();
        var client = factory.CreateAuthenticatedClient();

        var gradeResponse = await client.PostAsync(
            "/api/v1/exams/grade", SampleXml.AsUploadContent(SampleXml.SingleStudentTwoTasks));
        gradeResponse.EnsureSuccessStatusCode();

        var gradeResult = await gradeResponse.Content.ReadFromJsonAsync<GradeExamBatchResult>(JsonOptions.Web);
        Assert.NotNull(gradeResult);
        var gradedExam = gradeResult!.Students.Single().Exams.Single();
        Assert.Equal(2, gradedExam.TotalTasks);
        Assert.Equal(0, gradedExam.CorrectTasks); // both sample tasks claim a wrong answer

        var analyticsResponse = await client.GetAsync("/api/v1/students/12345/analytics");
        analyticsResponse.EnsureSuccessStatusCode();

        var analytics = await analyticsResponse.Content.ReadFromJsonAsync<StudentAnalyticsResult>(JsonOptions.Web);
        Assert.NotNull(analytics);
        Assert.Equal("12345", analytics!.StudentExternalId);
        var persistedExam = analytics!.Exams.Single();
        Assert.Equal(gradedExam.TotalTasks, persistedExam.TotalTasks);
        Assert.Equal(gradedExam.CorrectTasks, persistedExam.CorrectTasks);
    }

    [Fact]
    public async Task Uploading_a_mass_batch_grades_every_student_and_keeps_their_data_separate()
    {
        using var factory = new IntegrationWebApplicationFactory();
        var client = factory.CreateAuthenticatedClient();

        var gradeResponse = await client.PostAsync(
            "/api/v1/exams/grade", SampleXml.AsUploadContent(SampleXml.TwoStudentsTwoTasksEach));
        gradeResponse.EnsureSuccessStatusCode();

        var gradeResult = await gradeResponse.Content.ReadFromJsonAsync<GradeExamBatchResult>(JsonOptions.Web);
        Assert.NotNull(gradeResult);
        Assert.Equal(2, gradeResult!.Students.Count);

        foreach (var studentExternalId in new[] { "12345", "54321" })
        {
            var gradedStudent = gradeResult.Students.Single(s => s.StudentExternalId == studentExternalId);
            var gradedExam = gradedStudent.Exams.Single();
            Assert.Equal(2, gradedExam.TotalTasks);
            Assert.Equal(0, gradedExam.CorrectTasks); // both sample tasks claim a wrong answer

            // Each student's own analytics must reflect only their own exam -- not the other
            // student's, and not a merge of both.
            var analyticsResponse = await client.GetAsync($"/api/v1/students/{studentExternalId}/analytics");
            analyticsResponse.EnsureSuccessStatusCode();
            var analytics = await analyticsResponse.Content.ReadFromJsonAsync<StudentAnalyticsResult>(JsonOptions.Web);

            Assert.Equal(studentExternalId, analytics!.StudentExternalId);
            var persistedExam = analytics.Exams.Single();
            Assert.Equal(gradedExam.TotalTasks, persistedExam.TotalTasks);
            Assert.Equal(gradedExam.CorrectTasks, persistedExam.CorrectTasks);
        }
    }

    [Fact]
    public async Task Uploading_the_same_teacher_and_student_twice_reuses_the_same_records()
    {
        using var factory = new IntegrationWebApplicationFactory();
        var client = factory.CreateAuthenticatedClient();

        await client.PostAsync("/api/v1/exams/grade", SampleXml.AsUploadContent(SampleXml.SingleStudentTwoTasks));

        const string secondUploadXml = """
            <Teacher Id="11111">
              <Students>
                <Student Id="12345">
                  <Exam Id="2">
                    <Task Id="1">2+2 = 4</Task>
                  </Exam>
                </Student>
              </Students>
            </Teacher>
            """;
        var secondResponse = await client.PostAsync(
            "/api/v1/exams/grade", SampleXml.AsUploadContent(secondUploadXml));
        secondResponse.EnsureSuccessStatusCode();

        var analyticsResponse = await client.GetAsync("/api/v1/students/12345/analytics");
        var analytics = await analyticsResponse.Content.ReadFromJsonAsync<StudentAnalyticsResult>(JsonOptions.Web);

        // Same student now has two exams -- proves get-or-create reused the existing student
        // record across separate uploads instead of erroring on (or duplicating) it.
        Assert.Equal(2, analytics!.Exams.Count);
    }

    [Fact]
    public async Task Uploading_the_same_student_and_exam_id_twice_returns_409_instead_of_crashing()
    {
        using var factory = new IntegrationWebApplicationFactory();
        var client = factory.CreateAuthenticatedClient();

        var firstResponse = await client.PostAsync(
            "/api/v1/exams/grade", SampleXml.AsUploadContent(SampleXml.SingleStudentTwoTasks));
        firstResponse.EnsureSuccessStatusCode();

        // Same Teacher/Student/Exam id as above -- a resubmission of the same exam.
        var secondResponse = await client.PostAsync(
            "/api/v1/exams/grade", SampleXml.AsUploadContent(SampleXml.SingleStudentTwoTasks));

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task GetAnalytics_returns_404_for_a_student_that_was_never_uploaded()
    {
        using var factory = new IntegrationWebApplicationFactory();
        var client = factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1/students/never-existed/analytics");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
