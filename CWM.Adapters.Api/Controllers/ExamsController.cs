using CWM.Adapters.Api.Parsing;
using CWM.Application.Contracts;
using CWM.Application.Ports.Driving;
using Microsoft.AspNetCore.Mvc;

namespace CWM.Adapters.Api.Controllers;

/// <summary>
/// Requirement: single grading contract for one student's exam or a full mass/aggregated
/// upload -- same schema, same endpoint, no separate "batch" mode. Also the one integration
/// point third parties use -- there is no separate internal-only endpoint.
/// </summary>
[ApiController]
[Route("api/v1/exams")]
public sealed class ExamsController : ControllerBase
{
    private readonly IGradeExamBatchUseCase _gradeExamBatchUseCase;
    private readonly IExamDocumentParserResolver _parserResolver;

    public ExamsController(IGradeExamBatchUseCase gradeExamBatchUseCase, IExamDocumentParserResolver parserResolver)
    {
        _gradeExamBatchUseCase = gradeExamBatchUseCase;
        _parserResolver = parserResolver;
    }

    [HttpPost("grade")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<GradeExamBatchResult>> Grade(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("An exam document file is required.");
        }

        var parser = _parserResolver.Resolve(file);

        await using var stream = file.OpenReadStream();
        var request = await parser.ParseAsync(stream, cancellationToken);

        var result = await _gradeExamBatchUseCase.HandleAsync(request, cancellationToken);
        return Ok(result);
    }
}
