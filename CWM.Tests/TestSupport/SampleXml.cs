using System.Text;

namespace CWM.Tests.TestSupport;

/// <summary>
/// Builds sample exam-upload XML (the corrected schema -- see CLAUDE.md's Assumptions
/// section) and wraps it as multipart form-data for HTTP-level tests against
/// POST /api/v1/exams/grade.
/// </summary>
public static class SampleXml
{
    public const string SingleStudentTwoTasks = """
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

    // Mirrors the assignment's own example: two students under one teacher, same tasks.
    public const string TwoStudentsTwoTasksEach = """
        <Teacher Id="11111">
          <Students>
            <Student Id="12345">
              <Exam Id="1">
                <Task Id="1">2+3/6-4 = 74</Task>
                <Task Id="2">6*2+3-4 = 22</Task>
              </Exam>
            </Student>
            <Student Id="54321">
              <Exam Id="1">
                <Task Id="1">2+3/6-4 = 74</Task>
                <Task Id="2">6*2+3-4 = 22</Task>
              </Exam>
            </Student>
          </Students>
        </Teacher>
        """;

    public static MultipartFormDataContent AsUploadContent(string xml, string fileName = "exam.xml")
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(xml));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/xml");
        content.Add(fileContent, "file", fileName);
        return content;
    }
}
