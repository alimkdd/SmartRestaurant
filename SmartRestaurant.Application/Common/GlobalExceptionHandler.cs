using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartRestaurant.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace SmartRestaurant.Application.Common;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IConfiguration config) : IExceptionHandler
{
    private readonly RequestDelegate _next;

    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        object response;
        HttpStatusCode statusCode;

        switch (exception)
        {
            case ValidationException ve:
                statusCode = HttpStatusCode.BadRequest;
                response = new
                {
                    Status = (int)statusCode,
                    Error = ve.Message,
                    Details = ve.Errors
                };
                break;

            case NotFoundException nfe:
                statusCode = HttpStatusCode.NotFound;
                response = new
                {
                    Status = (int)statusCode,
                    Error = nfe.Message
                };
                break;

            case UnauthorizedException ue:
                statusCode = HttpStatusCode.Unauthorized;
                response = new
                {
                    Status = (int)statusCode,
                    Error = ue.Message
                };
                break;

            case ForbiddenException fe:
                statusCode = HttpStatusCode.Forbidden;
                response = new
                {
                    Status = (int)statusCode,
                    Error = fe.Message
                };
                break;

            case ConflictException ce:
                statusCode = HttpStatusCode.Conflict;
                response = new
                {
                    Status = (int)statusCode,
                    Error = ce.Message
                };
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                response = new
                {
                    Status = (int)statusCode,
                    Error = "An unexpected error occurred."
                };
                break;
        }


        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return new ValueTask<bool>(httpContext.Response.WriteAsync(json, cancellationToken)
                                                       .ContinueWith(_ => true, cancellationToken));
    }
}