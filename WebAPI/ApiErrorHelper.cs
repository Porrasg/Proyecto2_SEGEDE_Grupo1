using CoreApp;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI
{
    public static class ApiErrorHelper
    {
        public static ObjectResult Handle(string controllerName, Exception exception, string? operation = null)
        {
            if (exception is BusinessException or ArgumentException)
            {
                return new BadRequestObjectResult(new { message = exception.Message });
            }

            AppLogger.LogError(controllerName, exception, operation);
            return new ObjectResult(new { message = "Ocurrió un error interno al procesar la solicitud." })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
