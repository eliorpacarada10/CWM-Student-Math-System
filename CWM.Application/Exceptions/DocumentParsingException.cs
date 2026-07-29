namespace CWM.Application.Exceptions;

/// <summary>
/// The IExamDocumentParser port's failure contract -- a document that is structurally
/// malformed or doesn't match the expected schema. Unlike MathEngine, parser adapters
/// already depend on Application (they produce its Contracts), so they can throw this
/// type directly rather than needing their own translated exception.
/// </summary>
public sealed class DocumentParsingException : Exception
{
    public DocumentParsingException(string message) : base(message)
    {
    }

    public DocumentParsingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
