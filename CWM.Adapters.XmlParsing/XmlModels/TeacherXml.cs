using System.Xml.Serialization;

namespace CWM.Adapters.XmlParsing.XmlModels;

/// <summary>
/// Wire models for the (corrected) exam-upload XML schema -- see CLAUDE.md's "Assumptions"
/// section for why this differs from the assignment's literal (invalid) sample:
///
///   &lt;Teacher Id="11111"&gt;
///     &lt;Students&gt;
///       &lt;Student Id="12345"&gt;
///         &lt;Exam Id="1"&gt;
///           &lt;Task Id="1"&gt;2+3/6-4 = 74&lt;/Task&gt;
///         &lt;/Exam&gt;
///       &lt;/Student&gt;
///     &lt;/Students&gt;
///   &lt;/Teacher&gt;
///
/// Kept deliberately separate from Application.Contracts: if the real schema turns out to
/// differ slightly (an attribute renamed, casing changed), only this file and the mapping in
/// XmlExamDocumentParser need to change -- Application never notices.
/// </summary>
[XmlRoot("Teacher")]
public sealed class TeacherXml
{
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    [XmlArray("Students")]
    [XmlArrayItem("Student")]
    public List<StudentXml> Students { get; set; } = new();
}

public sealed class StudentXml
{
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    [XmlElement("Exam")]
    public List<ExamXml> Exams { get; set; } = new();
}

public sealed class ExamXml
{
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    [XmlElement("Task")]
    public List<TaskXml> Tasks { get; set; } = new();
}

public sealed class TaskXml
{
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Raw text content, e.g. "2+3/6-4 = 74" -- split on the last '=' during mapping.</summary>
    [XmlText]
    public string Text { get; set; } = string.Empty;
}
