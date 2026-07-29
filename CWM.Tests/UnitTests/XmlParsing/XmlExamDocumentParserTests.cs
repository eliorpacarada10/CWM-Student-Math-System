using System.Text;
using CWM.Adapters.XmlParsing;
using CWM.Application.Exceptions;
using CWM.Tests.TestSupport;
using Xunit;

namespace CWM.Tests.UnitTests.XmlParsing;

public class XmlExamDocumentParserTests
{
    private readonly XmlExamDocumentParser _parser = new();

    private static Stream ToStream(string xml) => new MemoryStream(Encoding.UTF8.GetBytes(xml));

    [Fact]
    public void SupportedFormat_is_xml()
    {
        Assert.Equal("xml", _parser.SupportedFormat);
    }

    [Fact]
    public async Task Parse_maps_the_corrected_schema_into_a_grade_exam_batch_request()
    {
        const string xml = """
            <Teacher Id="11111">
              <Students>
                <Student Id="12345">
                  <Exam Id="1">
                    <Task Id="1">2+3/6-4 = 74</Task>
                    <Task Id="2">6*2+3-4 = 22</Task>
                  </Exam>
                </Student>
              </Students>
            </Teacher>
            """;

        var request = await _parser.ParseAsync(ToStream(xml), CancellationToken.None);

        Assert.Equal("11111", request.TeacherExternalId);
        var student = Assert.Single(request.Students);
        Assert.Equal("12345", student.StudentExternalId);
        var exam = Assert.Single(student.Exams);
        Assert.Equal("1", exam.ExamExternalId);
        Assert.Equal(2, exam.Tasks.Count);

        Assert.Equal("2+3/6-4", exam.Tasks[0].Expression);
        Assert.Equal(74m, exam.Tasks[0].ClaimedResult);
        Assert.Equal("6*2+3-4", exam.Tasks[1].Expression);
        Assert.Equal(22m, exam.Tasks[1].ClaimedResult);
    }

    [Fact]
    public async Task Parse_captures_every_student_in_a_mass_upload_not_just_the_first()
    {
        var request = await _parser.ParseAsync(ToStream(SampleXml.TwoStudentsTwoTasksEach), CancellationToken.None);

        Assert.Equal(2, request.Students.Count);
        Assert.Contains(request.Students, s => s.StudentExternalId == "12345");
        Assert.Contains(request.Students, s => s.StudentExternalId == "54321");
    }

    [Fact]
    public async Task Parse_throws_DocumentParsingException_when_root_element_does_not_match()
    {
        const string xml = "<NotATeacher Id=\"1\"></NotATeacher>";

        await Assert.ThrowsAsync<DocumentParsingException>(
            () => _parser.ParseAsync(ToStream(xml), CancellationToken.None));
    }

    [Fact]
    public async Task Parse_throws_DocumentParsingException_when_task_text_is_missing_equals()
    {
        const string xml = """
            <Teacher Id="11111">
              <Students>
                <Student Id="12345">
                  <Exam Id="1">
                    <Task Id="1">2+3/6-4</Task>
                  </Exam>
                </Student>
              </Students>
            </Teacher>
            """;

        await Assert.ThrowsAsync<DocumentParsingException>(
            () => _parser.ParseAsync(ToStream(xml), CancellationToken.None));
    }

    [Fact]
    public async Task Parse_throws_DocumentParsingException_when_claimed_result_is_not_numeric()
    {
        const string xml = """
            <Teacher Id="11111">
              <Students>
                <Student Id="12345">
                  <Exam Id="1">
                    <Task Id="1">2+2 = not-a-number</Task>
                  </Exam>
                </Student>
              </Students>
            </Teacher>
            """;

        await Assert.ThrowsAsync<DocumentParsingException>(
            () => _parser.ParseAsync(ToStream(xml), CancellationToken.None));
    }

    [Fact]
    public async Task Parse_throws_DocumentParsingException_on_malformed_xml()
    {
        const string xml = "<Teacher Id=\"11111\"><Students>";

        await Assert.ThrowsAsync<DocumentParsingException>(
            () => _parser.ParseAsync(ToStream(xml), CancellationToken.None));
    }
}
