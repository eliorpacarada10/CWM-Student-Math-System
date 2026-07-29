using CWM.Application.Ports.Driven;

namespace CWM.Adapters.Api.Parsing;

public sealed class ExamDocumentParserResolver : IExamDocumentParserResolver
{
    private readonly Dictionary<string, IExamDocumentParser> _parsersByFormat;

    public ExamDocumentParserResolver(IEnumerable<IExamDocumentParser> parsers)
    {
        _parsersByFormat = parsers.ToDictionary(p => p.SupportedFormat, StringComparer.OrdinalIgnoreCase);
    }

    public IExamDocumentParser Resolve(IFormFile file)
    {
        var format = ResolveFormat(file);

        if (_parsersByFormat.TryGetValue(format, out var parser))
        {
            return parser;
        }

        throw new NotSupportedException(
            $"No parser is registered for format '{format}'. Supported formats: {string.Join(", ", _parsersByFormat.Keys)}.");
    }

    private static string ResolveFormat(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).TrimStart('.');
        if (!string.IsNullOrEmpty(extension))
        {
            return extension;
        }

        // Fall back to the content-type's subtype, e.g. "text/xml" -> "xml".
        return file.ContentType.Split('/').LastOrDefault() ?? string.Empty;
    }
}
