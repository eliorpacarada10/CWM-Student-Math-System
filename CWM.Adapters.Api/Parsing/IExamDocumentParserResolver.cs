using CWM.Application.Ports.Driven;

namespace CWM.Adapters.Api.Parsing;

/// <summary>
/// Picks which registered IExamDocumentParser handles a given upload, based on its
/// file extension / content-type. This dispatch logic is a transport concern (it's about
/// an HTTP upload's metadata), so it lives in Api, not in Application -- Application never
/// needs to know a resolver exists, it just receives whichever parser's output.
/// </summary>
public interface IExamDocumentParserResolver
{
    IExamDocumentParser Resolve(IFormFile file);
}
