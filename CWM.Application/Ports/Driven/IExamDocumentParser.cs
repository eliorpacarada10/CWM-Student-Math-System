using CWM.Application.Contracts;

namespace CWM.Application.Ports.Driven;

/// <summary>
/// Translates an external document format into Application's own request shape. Today
/// CWM.Adapters.XmlParsing is the only implementation; adding a JSON source later means
/// writing a new IExamDocumentParser and registering it -- Application and Api's routing
/// logic never change. SupportedFormat is a plain label (e.g. "xml") the hosting adapter
/// uses to pick which parser to call for a given upload -- it carries no HTTP/transport
/// meaning, just "what format does this implementation understand."
/// </summary>
public interface IExamDocumentParser
{
    string SupportedFormat { get; }

    Task<GradeExamBatchRequest> ParseAsync(Stream content, CancellationToken cancellationToken);
}
