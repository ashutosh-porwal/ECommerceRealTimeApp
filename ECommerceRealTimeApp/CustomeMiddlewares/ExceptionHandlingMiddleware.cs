using System.Text.Json;

namespace ECommerceRealTimeApp.CustomeMiddlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate requestDelegate, IHostEnvironment hostEnvironment,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _requestDelegate = requestDelegate;
            _hostEnvironment = hostEnvironment;
            _logger = logger;
        }

        public async Task TaskAsync(HttpContext context)
        {
            try
            {
                await _requestDelegate(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while processing request.");
                await HandleExceptionAsync(context, ex, _hostEnvironment);
            }
        }

        private static Task HandleExceptionAsync(HttpContext httpContext, Exception exception, IHostEnvironment hostEnvironment)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var errorResponse = new
            {
                StatusCode = httpContext.Response.StatusCode,
                Success = false,
                Details = hostEnvironment.IsDevelopment() ? exception.StackTrace : "Internal Server Error"
            };

            var json = JsonSerializer.Serialize(errorResponse);

            return httpContext.Response.WriteAsync(json);
        }
    }
}
