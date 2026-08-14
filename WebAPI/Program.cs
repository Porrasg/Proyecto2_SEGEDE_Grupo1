using WebAPI;
using WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<FlushAutomaticService>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Y4: reutiliza el pipeline de logging real de ASP.NET Core (en vez de que
// AppLogger arranque con su propio logger standalone) para que los
// AppLogger.LogError(...) de los controllers y el manejador global de abajo
// escriban por el mismo canal.
AppLogger.Configure(app.Services.GetRequiredService<ILoggerFactory>());

// Y4: red de seguridad para cualquier excepcion que se escape sin pasar por un
// try/catch de accion (los ~100 catch existentes en los controllers siguen
// devolviendo su propio mensaje al cliente tal cual; esto solo cubre lo que
// NO quedo atrapado ahi). Nunca se devuelve el detalle interno/stack trace.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (feature?.Error != null)
        {
            AppLogger.LogError("UnhandledException", feature.Error, context.Request.Path);
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"message\":\"Ocurrió un error inesperado en el servidor.\"}");
    });
});

app.UseCors(cors => cors.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("Content-Disposition"));

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
