using System.Text;
using System.Text.RegularExpressions;
using Ocelot.Cache;
using Ocelot.Configuration;
using Ocelot.Request.Middleware;

namespace AspNetCore.Profiler.Gateway.Services;
public class RedisKeyGenerator : ICacheKeyGenerator
{
    private const char Delimiter = '-';

    public async ValueTask<string> GenerateRequestCacheKey(DownstreamRequest downstreamRequest, DownstreamRoute downstreamRoute)
    {
        #region Custom implementation

        // Use the regex, but better if client side can put the "Redis Key" in the request header.
        StringBuilder customRedisKey = await this.TryGenRedisKeyIfMatched(downstreamRequest);
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

    private static Task<string> ReadContentAsync(DownstreamRequest downstream) => downstream.HasContent
        ? downstream?.Request?.Content?.ReadAsStringAsync() ?? Task.FromResult(string.Empty)
        : Task.FromResult(string.Empty);

    private Task<StringBuilder> TryGenRedisKeyIfMatched(DownstreamRequest downStreamRequest)
    {
        string requestUrl = downStreamRequest.OriginalString;
        string pattern = @"^https?:\/\/[^\/]+\/Demo\?xxx=([^&]*)&yyy=(.*)$";
        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

        Match match = regex.Match(requestUrl);
        if (match.Success)
        {
            // TODO: Generate the custom Redis Key
        }

        return Task.FromResult(new StringBuilder());
    }
}
