using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Entities_DTOs;
using Microsoft.IdentityModel.Tokens;

namespace WebAPI
{
    // Emite los JWT de sesion. Sin contenedor de DI (regla del proyecto): la clave
    // de firma se resuelve una sola vez a nivel de tipo, y Program.cs reutiliza la
    // misma clave para configurar la validacion de tokens entrantes.
    public static class JwtTokenHelper
    {
        public const string Issuer = "SGDE-API";
        public const string Audience = "SGDE-WebApp";
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(8);

        public static readonly byte[] SigningKeyBytes = ResolveSigningKey();

        private static byte[] ResolveSigningKey()
        {
            var configured = Environment.GetEnvironmentVariable("SGDE_JWT_SECRET");
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return Encoding.UTF8.GetBytes(configured);
            }

            // Sin SGDE_JWT_SECRET configurada: se genera una clave aleatoria valida
            // solo mientras el proceso siga vivo (todas las sesiones se invalidan si
            // la API se reinicia). Sirve para desarrollo local sin configuracion
            // adicional; en Azure hay que definir SGDE_JWT_SECRET como variable de
            // entorno del App Service para que las sesiones sobrevivan un restart.
            Console.WriteLine("[JwtTokenHelper] ADVERTENCIA: SGDE_JWT_SECRET no esta configurada. " +
                "Se genero una clave temporal solo para este proceso; las sesiones no " +
                "sobreviven a un reinicio de la API. Configurar SGDE_JWT_SECRET en produccion.");
            var randomKey = new byte[64];
            RandomNumberGenerator.Fill(randomKey);
            return randomKey;
        }

        // Genera el JWT que representa una sesion ya autenticada (login completo,
        // es decir despues de validar el OTP). Incluye el rol como claim estandar
        // (ClaimTypes.Role) para que [Authorize(Roles = "...")] funcione sin config
        // adicional.
        public static string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role ?? string.Empty),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.FirstLastName}".Trim())
            };

            var credentials = new SigningCredentials(new SymmetricSecurityKey(SigningKeyBytes), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(TokenLifetime),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
