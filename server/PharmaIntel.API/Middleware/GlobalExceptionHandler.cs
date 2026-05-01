// =============================================================================
// ExceptionHandler: GlobalExceptionHandler
// Chuc nang: Bat moi exception trong pipeline va tra ProblemDetails (RFC 7807).
//   - AppException -> dung StatusCode/ErrorType cua exception
//   - Khac -> 500 Internal Server Error (an chi tiet o moi truong Production)
// Dang ky bang services.AddExceptionHandler<GlobalExceptionHandler>()
//             + app.UseExceptionHandler() (dat sau dau pipeline).
// =============================================================================
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.Core.Exceptions;

namespace PharmaIntel.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, problem) = BuildProblem(exception, httpContext);

        if (status >= 500)
            _logger.LogError(exception, "Unhandled exception at {Path}", httpContext.Request.Path);
        else
            _logger.LogWarning("Handled {Type} ({Status}) at {Path}: {Message}",
                exception.GetType().Name, status, httpContext.Request.Path, exception.Message);

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";
        // Serialize theo runtime type de ValidationProblemDetails.Errors khong bi mat
        await httpContext.Response.WriteAsJsonAsync(problem, problem.GetType(), cancellationToken: cancellationToken);
        return true;
    }

    private (int Status, ProblemDetails Problem) BuildProblem(Exception ex, HttpContext ctx)
    {
        var traceId = ctx.TraceIdentifier;

        switch (ex)
        {
            case ValidationException ve:
                {
                    var p = new ValidationProblemDetails(ve.Errors.ToDictionary(k => k.Key, v => v.Value))
                    {
                        Title = "Du lieu khong hop le",
                        Status = ve.StatusCode,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        Detail = ve.Message,
                        Instance = ctx.Request.Path
                    };
                    p.Extensions["errorType"] = ve.ErrorType;
                    p.Extensions["traceId"] = traceId;
                    return (ve.StatusCode, p);
                }

            case AppException ae:
                {
                    var p = new ProblemDetails
                    {
                        Title = TitleFor(ae.StatusCode),
                        Status = ae.StatusCode,
                        Detail = ae.Message,
                        Instance = ctx.Request.Path
                    };
                    p.Extensions["errorType"] = ae.ErrorType;
                    p.Extensions["traceId"] = traceId;
                    return (ae.StatusCode, p);
                }

            default:
                {
                    var p = new ProblemDetails
                    {
                        Title = "Loi he thong",
                        Status = StatusCodes.Status500InternalServerError,
                        Detail = _env.IsDevelopment() ? ex.ToString() : "Da xay ra loi khong mong muon. Vui long thu lai.",
                        Instance = ctx.Request.Path,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                    };
                    p.Extensions["errorType"] = "internal_error";
                    p.Extensions["traceId"] = traceId;
                    return (StatusCodes.Status500InternalServerError, p);
                }
        }
    }

    private static string TitleFor(int status) => status switch
    {
        400 => "Yeu cau khong hop le",
        401 => "Chua xac thuc",
        403 => "Khong du quyen",
        404 => "Khong tim thay",
        409 => "Xung dot du lieu",
        _ => "Loi"
    };
}
