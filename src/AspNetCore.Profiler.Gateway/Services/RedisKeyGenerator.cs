using System.Text;
using System.Text.RegularExpressions;
using Ocelot.Cache;
using Ocelot.Configuration;
using Ocelot.Request.Middleware;

namespace AspNetCore.Profiler.Gateway.Services;
public class RedisKeyGenerator : ICacheKeyGenerator
{
    private const char Delimiter = '-';
    private const string RedisKeyHeaderName = "X-Redis-Key";

    public async ValueTask<string> GenerateRequestCacheKey(DownstreamRequest downstreamRequest, DownstreamRoute downstreamRoute)
    {
        #region Generate custom Redis Key

        // 1. Try to generate Redis key by URL parameter.
        // StringBuilder customRedisKey = await this.TryGenRedisKeyByUrlParameter(downstreamRequest);
        // 2. Try to generate Redis key by HTTP header "X-Redis-Key".
        StringBuilder customRedisKey = await this.TryGenRedisKeyByHttpHeader(downstreamRequest);

        if (customRedisKey.Length > 0)
        {
            string cacheKey = customRedisKey.ToString();
            return cacheKey;
        }
        #endregion

        #region Official implementation
        var builder = new StringBuilder()
            .Append(downstreamRequest.Method)
            .Append(Delimiter)
            .Append(downstreamRequest.OriginalString);

        var options = downstreamRoute?.CacheOptions ?? new CacheOptions(ttlSeconds: 600, region: "ocelot", header: string.Empty);
        if (!string.IsNullOrEmpty(options.Header))
        {
            var header = downstreamRequest.Headers
                .FirstOrDefault(r => r.Key.Equals(options.Header, StringComparison.OrdinalIgnoreCase))
                .Value?.FirstOrDefault();

            if (!string.IsNullOrEmpty(header))
            {
                builder.Append(Delimiter)
                    .Append(header);
            }
        }

        if (!options.EnableContentHashing || !downstreamRequest.HasContent)
        {
            string originalCacheKey = builder.ToString();
            string md5Hash = MD5Helper.GenerateMd5(originalCacheKey);
            return md5Hash;
        }

        var requestContentString = await ReadContentAsync(downstreamRequest);
        builder.Append(Delimiter)
            .Append(requestContentString);

        return MD5Helper.GenerateMd5(builder.ToString());
        #endregion
    }

    private static Task<string> ReadContentAsync(DownstreamRequest downstream) => downstream.HasContent && downstream.Request?.Content != null
        ? downstream.Request.Content.ReadAsStringAsync() ?? Task.FromResult(string.Empty)
        : Task.FromResult(string.Empty);

    private Task<StringBuilder> TryGenRedisKeyByUrlParameter(DownstreamRequest downStreamRequest)
    {
        StringBuilder sbRedisKey = new();
        string requestUrl = downStreamRequest.OriginalString;

        // Dictionary of URL patterns
        Dictionary<string, string> patterns = new()
        {
            { "PaymentGet", @"^https?:\/\/[^\/]+\/Payment\/([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})$" }
        };

        foreach (var kvp in patterns)
        {
            Regex regex = new(kvp.Value, RegexOptions.IgnoreCase);
            Match match = regex.Match(requestUrl);

            if (match.Success)
            {
                switch (kvp.Key)
                {
                    case "PaymentGet":
                        string guid = match.Groups[1].Value;
                        sbRedisKey.Append(kvp.Key).Append(":").Append(guid);
                        break;
                    default:
                        break;
                }
            }
        }

        return Task.FromResult(sbRedisKey);
    }

    private Task<StringBuilder> TryGenRedisKeyByHttpHeader(DownstreamRequest downstreamRequest)
    {
        StringBuilder sbRedisKey = new();

        var httpHeaderValues = downstreamRequest.Headers.FirstOrDefault(x => x.Key.Equals(RedisKeyHeaderName));
        if (httpHeaderValues.Value != null && httpHeaderValues.Value.Any())
        {
            string headerValue = httpHeaderValues.Value.FirstOrDefault();
            if (!string.IsNullOrEmpty(headerValue))
            {
                sbRedisKey.Append(headerValue);
            }
        }
        return Task.FromResult(sbRedisKey);
    }
}
