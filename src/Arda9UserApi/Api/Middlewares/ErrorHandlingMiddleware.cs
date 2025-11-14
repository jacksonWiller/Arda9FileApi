//using System.Net.Mime;
//using Arda9UserApi.Api.Extensions;

//namespace Arda9UserApi.Api.Middlewares;

//public class ErrorHandlingMiddleware(
//    RequestDelegate next,
//    ILogger<ErrorHandlingMiddleware> logger,
//    IHostEnvironment environment)
//{
//    private readonly RequestDelegate _next = next;
//    private readonly ILogger<ErrorHandlingMiddleware> _logger = logger;
//    private readonly IHostEnvironment _environment = environment;

//    public async Task Invoke(HttpContext context)
//    {
//        try
//        {
//            await _next(context);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "An unexpected exception was thrown: {Message}", ex.Message);

//            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

//            if (_environment.IsDevelopment())
//            {
//                context.Response.ContentType = MediaTypeNames.Text.Plain;
//                await context.Response.WriteAsync(ex.ToString());
//            }
//            else
//            {
//                context.Response.ContentType = MediaTypeNames.Application.Json;
//                await context.Response.WriteAsync(ApiResponse.InternalServerError("An internal error occurred.").ToJson());
//            }
//        }
//    }
//}