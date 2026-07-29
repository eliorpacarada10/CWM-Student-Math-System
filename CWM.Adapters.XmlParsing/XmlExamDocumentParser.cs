using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using CWM.Adapters.XmlParsing.XmlModels;
using CWM.Application.Contracts;
using CWM.Application.Exceptions;
using CWM.Application.Ports.Driven;

namespace CWM.Adapters.XmlParsing;

/// <summary>
/// Implements the IExamDocumentParser port for the XML schema. Owns two distinct steps:
/// deserializing raw XML into schema-shaped wire models (XmlModels/), then hand-mapping those
/// onto Application's Contracts. Kept as its own adapter (rather than inline in Api) so Api
/// stays pure transport, and so a future JSON source is "write a new adapter", not "add an if
/// branch to the controller".
/// </summary>
public sealed class XmlExamDocumentParser : IExamDocumentParser
{
    private static readonly XmlSerializer Serializer = new(typeof(TeacherXml));

    public string SupportedFormat => "xml";

    public async Task<GradeExamBatchRequest> ParseAsync(Stream content, CancellationToken cancellationToken)
    {
        // XmlSerializer.Deserialize has no async overload -- there's nothing to await there
        // regardless of library choice. The genuine I/O is receiving the upload itself, so
        // that's the part that actually gets awaited: buffer it into memory first, then run
        // the synchronous XmlSerializer call against the now-fully-local MemoryStream, which
        // involves no further I/O at all.
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        var teacherXml = Deserialize(buffer);
        return MapToRequest(teacherXml);
    }

    private static TeacherXml Deserialize(Stream content)
    {
        var settings = new XmlReaderSettings
        {
            // Disable DTD/external entity resolution -- this endpoint accepts XML uploads
            // from arbitrary callers, so XXE injection is a real risk to close off here.
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        try
        {
            using var reader = XmlReader.Create(content, settings);
            return (TeacherXml?)Serializer.Deserialize(reader)
                ?? throw new DocumentParsingException("The uploaded document did not contain a <Teacher> root element.");
        }
        catch (InvalidOperationException ex)
        {
            // XmlSerializer wraps malformed-XML/schema-mismatch failures in InvalidOperationException;
            // the real reason is usually in the inner exception.
            throw new DocumentParsingException(
                $"The uploaded XML does not match the expected schema: {ex.InnerException?.Message ?? ex.Message}", ex);
        }
        catch (XmlException ex)
        {
            throw new DocumentParsingException($"The uploaded content is not well-formed XML: {ex.Message}", ex);
        }
    }

    private static GradeExamBatchRequest MapToRequest(TeacherXml teacherXml)
    {
        if (string.IsNullOrWhiteSpace(teacherXml.Id))
        {
            throw new DocumentParsingException("The <Teacher> element is missing its Id attribute.");
        }

        var students = teacherXml.Students
            .Select(studentXml => new StudentSubmission(
                studentXml.Id,
                studentXml.Exams
                    .Select(examXml => new ExamSubmission(
                        examXml.Id,
                        examXml.Tasks.Select(MapTask).ToList()))
                    .ToList()))
            .ToList();

        return new GradeExamBatchRequest(teacherXml.Id, students);
    }

    private static TaskSubmission MapTask(TaskXml taskXml)
    {
        var text = taskXml.Text.Trim();
        var equalsIndex = text.LastIndexOf('=');

        if (equalsIndex < 0)
        {
            throw new DocumentParsingException(
                $"Task '{taskXml.Id}' is missing '=' separating the expression from the claimed result: '{text}'.");
        }

        var expression = text[..equalsIndex].Trim();
        var claimedResultText = text[(equalsIndex + 1)..].Trim();

        if (string.IsNullOrEmpty(expression))
        {
            throw new DocumentParsingException($"Task '{taskXml.Id}' has an empty expression.");
        }

        if (!decimal.TryParse(claimedResultText, NumberStyles.Number, CultureInfo.InvariantCulture, out var claimedResult))
        {
            throw new DocumentParsingException($"Task '{taskXml.Id}' has a non-numeric claimed result: '{claimedResultText}'.");
        }

        return new TaskSubmission(taskXml.Id, expression, claimedResult);
    }
}
