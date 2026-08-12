using CoreApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // OTP es infraestructura pre-autenticacion (se usa durante login, activacion y
    // recuperacion de contraseña, cuando el llamador todavia no tiene sesion) -- todo
    // el controller queda publico, igual que los endpoints equivalentes de Users.
    [AllowAnonymous]
    public class OtpsController : ControllerBase
    {
        [HttpPost]
        [Route("GenerateAndSend")]
        public ActionResult GenerateAndSend(string email, string userName, string purpose)
        {
            try
            {
                var om = new OtpManager();
                om.GenerateAndSendOtp(email, userName, purpose);
                AuditHelper.TryAudit(null, "Create", "Otp", null, $"OTP generado y enviado a {email} (motivo: {purpose})");
                return Ok("OTP enviado correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Validate")]
        public ActionResult Validate(string email, string tokenCode, string purpose)
        {
            try
            {
                var om = new OtpManager();
                om.ValidateOtp(email, tokenCode, purpose);
                AuditHelper.TryAudit(null, "Validate", "Otp", null, $"OTP validado correctamente para {email} (motivo: {purpose})");
                return Ok("OTP válido.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
