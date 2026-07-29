using System.Text.Json;

namespace CWM.Tests.TestSupport;

/// <summary>
/// ASP.NET Core's MVC output formatter serializes with camelCase property names by default.
/// HttpContent.ReadFromJsonAsync&lt;T&gt;() does NOT use that convention unless told to --
/// without this, deserializing records back on the test side would silently leave every
/// property at its default value instead of throwing, which is worse than a compile error.
/// </summary>
public static class JsonOptions
{
    public static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
}
