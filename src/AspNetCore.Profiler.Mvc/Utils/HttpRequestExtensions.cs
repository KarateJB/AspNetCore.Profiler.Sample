namespace AspNetCore.Profiler.Mvc.Utils;

/// <summary>
/// HttpRequest Extensions
/// </summary>
public static class HttpRequestExtensions
{
    /// <summary>
    /// Check if the request is authorized to access MiniProfiler
    /// </summary>
    /// <param name="request">HttpRequest</param>
    /// <returns>True(OK)/False(Not allowed)</returns>
    public static bool IsAuthorizedToMiniProfiler(this HttpRequest request)
    {
        if (!request.Path.ToString().StartsWith("/profiler", StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }

        var bearerTokenPrefix = "Bearer";
        var accessToken = string.Empty;

        var authorizationHeader = request.Headers["Authorization"].ToString();

        // Get Access token from header
        if (!string.IsNullOrEmpty(authorizationHeader) &&
            authorizationHeader.StartsWith(bearerTokenPrefix, StringComparison.OrdinalIgnoreCase))
        {
            accessToken = authorizationHeader.Replace(bearerTokenPrefix, string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
        }
        // Get Access token from cookie
        else if (request.Cookies.TryGetValue("access-token", out accessToken))
        {
        }
        else
        {
            return false;
        }

# if DEBUG
        // Skip validation
        return true;
# else
        // Validate JWT (OAuth2 server is required)
        return AccessTokenValidator.ValidateAsync(accessToken).Result;
# endif
    }
}

