using CWM.Application.Contracts;

namespace CWM.Application.Ports.Driving;

/// <summary>
/// The one entry point for grading -- called identically whether the caller is the Blazor
/// UI, a mobile app, or a third-party integrator. None of them get a special code path.
/// </summary>
public interface IGradeExamBatchUseCase
{
    Task<GradeExamBatchResult> HandleAsync(GradeExamBatchRequest request, CancellationToken cancellationToken);
}
