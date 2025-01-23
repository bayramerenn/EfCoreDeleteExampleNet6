using System.Text.RegularExpressions;

namespace EfCoreDeleteExample;

/// <summary>
/// Extension methods for handling Gemini AI content generation results
/// </summary>
public static class VertexGenerateContentResultExtensions
{
    private static readonly Regex jsonRegex = new Regex(
        @"```json\s*(\[.*?\])\s*```",
        RegexOptions.Compiled | RegexOptions.Singleline
    );

    /// <summary>
    /// Extracts JSON content from Gemini's response that is wrapped in JSON code blocks
    /// </summary>
    /// <param name="result">Gemini's generation result containing JSON response</param>
    /// <returns>Extracted JSON string from the response</returns>
    /// <exception cref="ArgumentNullException">When result is null</exception>
    /// <exception cref="ArgumentException">When no valid JSON content is found in the response</exception>
    public static string ToJsonContent(this string result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var match = jsonRegex.Match(result);

        return match.Success ? match.Groups[1].Value : result;
    }
}