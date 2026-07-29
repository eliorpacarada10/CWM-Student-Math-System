using System.Net;
using CWM.Application.Exceptions;
using CWM.Domain.Exceptions;
using FluentValidation;

namespace CWM.Adapters.Api.Middleware;

/// <summary>
/// Centralizes translating exceptions from every layer into HTTP responses, so controllers
/// don't each need their own try/catch -- keeps them limited to "call a port, return the
/// result", per the "thin Api" goal.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            // FluentValidation -- "is this a sensible request" failures.
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, "Validation failed.",
                ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (DocumentParsingException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, "Could not parse the uploaded document.",
                new[] { ex.Message });
        }
        catch (ArithmeticEvaluationException ex)
        {
            // Normally caught per-task inside GradeExamBatchUseCase; this is a defensive
            // fallback so any unexpected evaluator failure still returns a clean 400.
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, "Could not evaluate an expression.",
                new[] { ex.Message });
        }
        catch (StudentNotFoundException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, ex.Message, Array.Empty<string>());
        }
        catch (DuplicateExamSubmissionException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.Conflict, ex.Message, Array.Empty<string>());
        }
        catch (MathTestDomainException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, ex.Message, Array.Empty<string>());
        }
        catch (NotSupportedException ex)
        {
            // e.g. an uploaded file whose format has no registered parser.
            await WriteProblemAsync(context, HttpStatusCode.UnsupportedMediaType, ex.Message, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing {Path}", context.Request.Path);
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.",
                Array.Empty<string>());
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context, HttpStatusCode statusCode, string title, IEnumerable<string> details)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new
        {
            title,
            status = (int)statusCode,
            errors = details
        });
    }
}
