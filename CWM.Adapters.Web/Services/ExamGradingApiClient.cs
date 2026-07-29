using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CWM.Adapters.Web.Models;

namespace CWM.Adapters.Web.Services;

public sealed class ExamGradingApiException : Exception
{
    public ExamGradingApiException(string message) : base(message)
    {
    }
}

/// <summary>
/// The Web app's only contact with the backend -- a plain HttpClient wrapper calling the
/// exact same public contract a third-party integrator would use. No shortcuts, no
/// internal-only route.
/// </summary>
public sealed class ExamGradingApiClient
{
    // The Api serializes with camelCase property names (ASP.NET Core MVC's default);
    // HttpClient's JSON helpers don't assume that convention unless told to.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public ExamGradingApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GradeExamBatchResult> GradeAsync(
        Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
        content.Add(fileContent, "file", fileName);

        var response = await _httpClient.PostAsync("api/v1/exams/grade", content, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<GradeExamBatchResult>(JsonOptions, cancellationToken)
            ?? throw new ExamGradingApiException("The grading response was empty.");
    }

    public async Task<StudentAnalyticsResult?> GetAnalyticsAsync(string studentExternalId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(
            $"api/v1/students/{Uri.EscapeDataString(studentExternalId)}/analytics", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<StudentAnalyticsResult>(JsonOptions, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(JsonOptions, cancellationToken);
        var message = problem?.Title ?? $"Request failed with status {(int)response.StatusCode}.";
        if (problem?.Errors is { Count: > 0 })
        {
            message = $"{message} {string.Join(" ", problem.Errors)}";
        }

        throw new ExamGradingApiException(message);
    }

    private sealed record ProblemResponse(string? Title, int Status, IReadOnlyList<string>? Errors);
}
