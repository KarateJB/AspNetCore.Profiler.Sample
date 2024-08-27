using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace AspNetCore.Profiler.Mvc.Utils;

public class HttpRequestLogFilter : ActionFilterAttribute
{
    private readonly ILogger logger;

    public HttpRequestLogFilter(ILogger<HttpRequestLogFilter> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        string requestDetails = $"[{context.HttpContext.Request.Path}][{context.HttpContext.Request.Method}] {JsonConvert.SerializeObject(context.ActionArguments)}";
        try
        {
            this.logger.LogDebug($"Request: {requestDetails}");
            base.OnActionExecuting(context);
        }
        catch (System.Exception ex)
        {
            this.logger.LogError(ex, $"{requestDetails} error");
        }
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        try
        {
            string responseBody = context.Result switch
            {
                ObjectResult result when result.Value != null => JsonConvert.SerializeObject(result.Value),
                JsonResult result when result.Value != null => JsonConvert.SerializeObject(result.Value),
                ContentResult result when !string.IsNullOrEmpty(result.Content) => result.Content,
                _ => string.Empty,
            };
            this.logger.LogDebug($"Response: {responseBody}");
            base.OnActionExecuted(context);
        }
        catch (System.Exception ex)
        {
            this.logger.LogError(ex, $"Response error");
        }
    }
}
