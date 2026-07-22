using CoreApp;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
                return Ok("OTP válido.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
