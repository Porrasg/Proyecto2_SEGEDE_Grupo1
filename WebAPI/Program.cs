using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WebAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Y1-C: filtro global que exige autenticacion (cualquier JWT valido) por defecto
// en TODOS los controllers/acciones. Las acciones que deben seguir siendo
// publicas (Login, Register, Activate, RecoverPassword, etc.) se marcan
// explicitamente con [AllowAnonymous] en su propio controller.
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Autenticacion JWT (Y1-A): solo se configura el esquema y se emiten/validan
// tokens en este commit. Todavia no hay ningun [Authorize] en los controllers
// -- eso se agrega recien en Y1-C, una vez que el frontend (Y1-B) ya mande el
// header Authorization en cada peticion. Agregarlo antes rompería toda la app.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = JwtTokenHelper.Issuer,
            ValidateAudience = true,
            ValidAudience = JwtTokenHelper.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(JwtTokenHelper.SigningKeyBytes),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

var app = builder.Build();

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
