using AuthProvider.Utils;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AuthProvider.Exceptions;
public class UuidExceptionHandler(ILogger<UuidExceptionHandler> Logger) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionDetails = new ExceptionDetails(exception, httpContext);
        Logger.LogError(exceptionDetails.ToString());
        string guid = exceptionDetails.Guid.ToString();
        // exception.Data.Add("GUID", guid);
        httpContext.Response.Clear();
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        HeaderUtils.AddHeader(httpContext.Response, HeaderUtils.XExceptionCode, guid);
        return ValueTask.FromResult(true);
    }
}
