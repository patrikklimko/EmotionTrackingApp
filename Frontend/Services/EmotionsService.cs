using System.Text.Json;
using DTO;
using Frontend.Services.Interfaces;
using Frontend.Utils;
using Microsoft.AspNetCore.WebUtilities;

namespace Frontend.Services;

public class EmotionsService(NonAuthedClient httpClient) : IEmotionsService
{
  public async Task<List<EmotionDto>?> GetAll(string? emotionQuery, string? emotionColor)
  {
    var response = await httpClient.GetAsync(_buildEmotionUrl(emotionQuery, emotionColor));
    response.EnsureSuccessStatusCode();

    return await new ApiParsingUtils<List<EmotionDto>>().Process(response);
  }

  private static string _buildEmotionUrl(string? emotionQuery = null, string? emotionColor = null)
  {
    var path = "/Emotions";
    var queryParameters = new Dictionary<string, string>();

    // Add parameters conditionally
    if (!string.IsNullOrWhiteSpace(emotionQuery))
    {
      queryParameters["EmotionQuery"] = emotionQuery;
    }

    if (!string.IsNullOrWhiteSpace(emotionColor))
    {
      queryParameters["EmotionColor"] = emotionColor;
    }

    // Return the built URL
    return queryParameters.Count > 0
      ? QueryHelpers.AddQueryString(path, queryParameters)
      : path;
  }
}