using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        [Route("RetrieveAll")]
        public ActionResult RetrieveAll()
        {
            try
            {
                var um = new UserManager();
                var lstResults = um.RetrieveAllUsers();
                return Ok(lstResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveById/{id}")]
        public ActionResult RetrieveById(int id)
        {
            try
            {
                var um = new UserManager();
                var user = um.RetrieveUserById(id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveByEmail/{email}")]
        public ActionResult RetrieveByEmail(string email)
        {
            try
            {
                var um = new UserManager();
                var user = um.RetrieveUserByEmail(email);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Create")]
        public ActionResult Create(User user)
        {
            try
            {
                var um = new UserManager();
                um.Create(user);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(User user)
        {
            try
            {
                var um = new UserManager();
                um.Update(user);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public ActionResult Delete(User user)
        {
            try
            {
                var um = new UserManager();
                um.Delete(user);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Login")]
        public ActionResult Login(string email, string password)
        {
            try
            {
                var um = new UserManager();
                var user = um.Login(email, password);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("ValidateLoginOtp")]
        public ActionResult ValidateLoginOtp(string email, string tokenCode)
        {
            try
            {
                var um = new UserManager();
                var user = um.ValidateLoginOtp(email, tokenCode);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("ChangePassword")]
        public ActionResult ChangePassword(int userId, string currentPassword, string newPassword, string confirmPassword)
        {
            try
            {
                var um = new UserManager();
                um.ChangePassword(userId, currentPassword, newPassword, confirmPassword);
                return Ok("Solicitud de cambio de contraseña enviada.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("ConfirmChangePassword")]
        public ActionResult ConfirmChangePassword(int userId, string tokenCode, string newPassword, string confirmPassword)
        {
            try
            {
                var um = new UserManager();
                um.ConfirmChangePassword(userId, tokenCode, newPassword, confirmPassword);
                return Ok("Contraseña actualizada correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("ResetPassword")]
        public ActionResult ResetPassword(string email)
        {
            try
            {
                var um = new UserManager();
                um.ResetPassword(email);
                return Ok("Solicitud de restablecimiento enviada.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("ConfirmResetPassword")]
        public ActionResult ConfirmResetPassword(string email, string tokenCode, string newPassword, string confirmPassword)
        {
            try
            {
                var um = new UserManager();
                um.ConfirmResetPassword(email, tokenCode, newPassword, confirmPassword);
                return Ok("Contraseña restablecida correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("ActivateAccount")]
        public ActionResult ActivateAccount(string email, string tokenCode)
        {
            try
            {
                var um = new UserManager();
                um.ActivateAccount(email, tokenCode);
                return Ok("Cuenta activada correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Variante de ActivateAccount con binding por JSON body (rama Harry) — se conserva junto a
        // ActivateAccount (query string) porque son dos formas de invocar el mismo UserManager.ActivateAccount.
        [HttpPost]
        [Route("Activate")]
        public ActionResult Activate(UserActivationRequest request)
        {
            try
            {
                var um = new UserManager();
                um.ActivateAccount(request.Email, request.TokenCode);
                return Ok(new { message = "Cuenta activada con éxito." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Mini-clase estructural para el mapeo limpio del JSON entrante de JavaScript
        public class UserActivationRequest
        {
            public string Email { get; set; } = string.Empty;
            public string TokenCode { get; set; } = string.Empty;
        }
    }
}
