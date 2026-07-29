using CWM.Application.Contracts;
using CWM.Application.Ports.Driving;
using Microsoft.AspNetCore.Mvc;

namespace CWM.Adapters.Api.Controllers;

[ApiController]
[Route("api/v1/students")]
public sealed class StudentsController : ControllerBase
{
    private readonly IGetStudentAnalyticsUseCase _getStudentAnalyticsUseCase;

    public StudentsController(IGetStudentAnalyticsUseCase getStudentAnalyticsUseCase)
    {
        _getStudentAnalyticsUseCase = getStudentAnalyticsUseCase;
    }

    [HttpGet("{studentExternalId}/analytics")]
    public async Task<ActionResult<StudentAnalyticsResult>> GetAnalytics(
        string studentExternalId, CancellationToken cancellationToken)
    {
        var result = await _getStudentAnalyticsUseCase.HandleAsync(studentExternalId, cancellationToken);
        return Ok(result);
    }
}
