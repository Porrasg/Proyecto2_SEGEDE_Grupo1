using System;
using Microsoft.Extensions.Logging;

namespace WebAPI
{
    // Logging estatico minimo (sin contenedor de DI, misma convencion que AuditHelper
    // y JwtTokenHelper). Antes de este helper, ninguna excepcion capturada en los
    // controllers quedaba registrada en ningun lado -- el try/catch de cada accion
    // solo le devolvia el mensaje al cliente y lo descartaba. Program.cs llama a
    // Configure() una sola vez al arrancar para reutilizar el pipeline de logging
    // real de ASP.NET Core (consola + lo que se agregue a futuro) en vez de crear
    // uno aparte.
    public static class AppLogger
    {
        private static ILoggerFactory _factory = LoggerFactory.Create(builder => builder.AddConsole());

        public static void Configure(ILoggerFactory factory)
        {
            _factory = factory;
        }

        public static void LogError(string controllerName, Exception ex, string? context = null)
        {
            _factory.CreateLogger(controllerName).LogError(ex, "{Context}: {Message}", context ?? "Error no controlado", ex.Message);
        }
    }
}
