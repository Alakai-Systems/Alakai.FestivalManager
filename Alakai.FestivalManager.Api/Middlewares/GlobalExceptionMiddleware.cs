using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Alakai.FestivalManager.Api.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.BadRequest,
                "Validation failed.",
                ex.Errors.Select(e => e.ErrorMessage).ToList());
        }
        catch (NotFoundException ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.NotFound,
                ex.Message,
                []);
        }
        catch (BusinessRuleException ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.Conflict,
                ex.Message,
                []);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is SqlException sqlEx &&
                (sqlEx.Number == 2601 || sqlEx.Number == 2627))
            {
                await HandleExceptionAsync(
                    context,
                    HttpStatusCode.Conflict,
                    "A record with the same unique value already exists.",
                    []);
                return;
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");

            await HandleExceptionAsync(
                context,
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred.",
                []);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message,
        List<string> errors)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        ApiResponse<object> response = new()
        {
            Success = false,
            Message = message,
            Data = null,
            Errors = errors
        };

        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string json = JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsync(json);
    }
}