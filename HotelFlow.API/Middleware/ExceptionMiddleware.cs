using HotelFlow.Application.Common;
using HotelFlow.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace HotelFlow.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
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
        catch (NotFoundException ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.NotFound,
                ex.Message
            );
        }
        catch (UnauthorizedException ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.Unauthorized,
                ex.Message
            );
        }
        catch (ForbiddenException ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.Forbidden,
                ex.Message
            );
        }
        catch (BadRequestException ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.BadRequest,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            await HandleExceptionAsync(
                context,
                HttpStatusCode.InternalServerError,
                ex.Message
            );
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ErrorResponse
        {
            StatusCode = context.Response.StatusCode,
            Message = message
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }


}

