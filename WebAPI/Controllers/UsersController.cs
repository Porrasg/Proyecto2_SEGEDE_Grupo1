using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // Nunca exponer el hash de la contraseña en las respuestas de la API:
        // se limpia el campo justo antes de serializar (defensa en profundidad
        // mientras no exista un DTO de respuesta dedicado sin ese campo).
        private static User Sanitize(User user)
        {
            if (user != null) user.Password = string.Empty;
            return user;
        }

        private static List<User> Sanitize(List<User> users)
        {
            users?.ForEach(u => u.Password = string.Empty);
            return users;
        }

        public class UserRegisterRequest
        {
            public string Identification { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string FirstLastName { get; set; } = string.Empty;
            public string? SecondLastName { get; set; }
            public string Phone { get; set; } = string.Empty;
            public DateTime BirthDate { get; set; }
            public string? PhotoUrl { get; set; }
            public string Password { get; set; } = string.Empty;
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class OtpRequest
        {
            public string Email { get; set; } = string.Empty;
            public string OtpCode { get; set; } = string.Empty;
        }

        public class ResetPasswordRequest
        {
            public string Email { get; set; } = string.Empty;
            public string OtpCode { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public class ChangePasswordEmailRequest
        {
            public string Email { get; set; } = string.Empty;
            public string CurrentPassword { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public class ConfirmChangePasswordEmailRequest
        {
            public string Email { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public class UserActivationRequest
        {
            public string Email { get; set; } = string.Empty;
            public string TokenCode { get; set; } = string.Empty;
        }

        [HttpGet]
        [Route("RetrieveAll")]
        public ActionResult RetrieveAll()
        {
            try
            {
                var um = new UserManager();
                var lstResults = um.RetrieveAllUsers();
                return Ok(Sanitize(lstResults));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("ChangePasswordByEmail")]
        public ActionResult ChangePasswordByEmail(ChangePasswordEmailRequest request)
        {
            try
            {
                var um = new UserManager();
                um.ChangePassword(request.Email, request.CurrentPassword, request.NewPassword, request.ConfirmPassword);
                return Ok(new { message = "Se ha enviado un código OTP a su correo." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("Register")]
        public ActionResult Register(UserRegisterRequest request)
        {
            try
            {
                var um = new UserManager();
                var user = new User
                {
                    Identification = request.Identification,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    FirstLastName = request.FirstLastName,
                    SecondLastName = request.SecondLastName,
                    PhoneNumber = request.Phone,
                    BirthDate = request.BirthDate,
                    ProfilePhoto = request.PhotoUrl,
                    Password = request.Password,
                    Role = "Distributor",
                    Status = "Pending"
                };

                user.Role = string.IsNullOrWhiteSpace(user.Role) ? "Distributor" : user.Role;
                user.Status = string.IsNullOrWhiteSpace(user.Status) ? "Pending" : user.Status;

                um.Create(user);
                return Ok(new { message = "Comprador registrado con éxito. Active su cuenta con el código enviado a su correo." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("ConfirmChangePasswordByEmail")]
        public ActionResult ConfirmChangePasswordByEmail([FromBody] ConfirmChangePasswordEmailRequest request, [FromQuery] string tokenCode)
        {
            try
            {
                var um = new UserManager();
                um.ConfirmChangePassword(request.Email, tokenCode, request.NewPassword, request.ConfirmPassword);
                return Ok(new { message = "Contraseña actualizada correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
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
                return Ok(Sanitize(user));
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
                return Ok(Sanitize(user));
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
                return Ok(Sanitize(user));
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
                return Ok(Sanitize(user));
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
                return Ok(Sanitize(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Login")]
        public ActionResult Login(LoginRequest request)
        {
            try
            {
                var um = new UserManager();
                var user = um.Login(request.Email, request.Password);
                return Ok(Sanitize(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("ValidateLoginOtp")]
        public ActionResult ValidateLoginOtp(OtpRequest request)
        {
            try
            {
                var um = new UserManager();
                var user = um.ValidateLoginOtp(request.Email, request.OtpCode);
                return Ok(Sanitize(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("ResetPassword")]
        public ActionResult ResetPassword(ResetPasswordRequest request)
        {
            try
            {
                var um = new UserManager();
                if (string.IsNullOrWhiteSpace(request.OtpCode))
                {
                    um.ResetPassword(request.Email);
                    return Ok("Solicitud de restablecimiento enviada.");
                }

                um.ConfirmResetPassword(request.Email, request.OtpCode, request.NewPassword, request.ConfirmPassword);
                return Ok("Contraseña restablecida correctamente.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("RecoverPassword")]
        public ActionResult RecoverPassword([FromBody] OtpRequest request)
        {
            try
            {
                var um = new UserManager();
                um.ResetPassword(request.Email);
                return Ok(new { message = "Código de recuperación enviado a su correo electrónico." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("ConfirmResetPassword")]
        public ActionResult ConfirmResetPassword(ResetPasswordRequest request)
        {
            try
            {
                var um = new UserManager();
                um.ConfirmResetPassword(request.Email, request.OtpCode, request.NewPassword, request.ConfirmPassword);
                return Ok("Contraseña restablecida correctamente.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("ActivateAccount")]
        public ActionResult ActivateAccount([FromBody] OtpRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.OtpCode))
                {
                    return BadRequest(new { message = "El correo electrónico y el código OTP son requeridos." });
                }

                var um = new UserManager();
                um.ActivateAccount(request.Email?.Trim(), request.OtpCode?.Trim());
                return Ok(new { message = "Cuenta activada correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("Activate")]
        public ActionResult Activate([FromBody] UserActivationRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.TokenCode))
                {
                    return BadRequest(new { message = "El correo electrónico y el código OTP son requeridos." });
                }

                var um = new UserManager();
                um.ActivateAccount(request.Email?.Trim(), request.TokenCode?.Trim());
                return Ok(new { message = "Cuenta activada con éxito." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("ResendOtp")]
        public ActionResult ResendOtp([FromBody] OtpRequest request, [FromQuery] string usageType)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                {
                    return StatusCode(400, new { message = "El correo electrónico es requerido." });
                }

                var purpose = usageType?.Trim().ToLowerInvariant() switch
                {
                    "activation" => "ACCOUNT_ACTIVATION",
                    "login" => "LOGIN",
                    "recover" => "RESET_PASSWORD",
                    "reset" => "RESET_PASSWORD",
                    _ => "ACCOUNT_ACTIVATION"
                };

                var um = new UserManager();
                var user = um.RetrieveUserByEmail(request.Email.Trim());

                var otpManager = new OtpManager();
                otpManager.GenerateAndSendOtp(user.Email, user.FirstName, purpose);

                return Ok(new { message = "Código reenviado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
