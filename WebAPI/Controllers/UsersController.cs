using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // Nunca devuelvo el hash de la contraseña en la API.
        // Lo limpio antes de responder para evitar mostrar esa información.
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

        // Agrega el JWT de sesion (login ya completo, post-OTP) al objeto de usuario
        // ya serializado, sin romper la forma plana que session.save() del frontend
        // ya espera (role/userId/email al nivel raiz, no anidados bajo "user").
        private static object WithToken(User user)
        {
            var node = System.Text.Json.JsonSerializer.SerializeToNode(user, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            })!.AsObject();
            node["token"] = JwtTokenHelper.GenerateToken(user);
            return node;
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

        // Datos para cambiar contraseña cuando el usuario ya tiene sesión abierta.
        public class ChangePasswordAuthenticatedRequest
        {
            public int UserId { get; set; }
            public string CurrentPassword { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public class UpdateProfileRequest
        {
            public int UserId { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string FirstLastName { get; set; } = string.Empty;
            public string? SecondLastName { get; set; }
            public string PhoneNumber { get; set; } = string.Empty;
        }

        public class EmailChangeRequest
        {
            public int UserId { get; set; }
            public string NewEmail { get; set; } = string.Empty;
        }

        public class ConfirmEmailChangeRequest : EmailChangeRequest
        {
            public string OtpCode { get; set; } = string.Empty;
        }

        public class ConfirmChangePasswordEmailRequest
        {
            public string Email { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        [HttpPut]
        [Route("Profile")]
        public ActionResult UpdateProfile(UpdateProfileRequest request)
        {
            try
            {
                var user = new UserManager().UpdateProfile(request.UserId, request.FirstName, request.FirstLastName, request.SecondLastName, request.PhoneNumber);
                AuditHelper.TryAudit(request.UserId, "Update", "Users", request.UserId, "Actualización de información de perfil");
                return Ok(Sanitize(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("Profile/RequestEmailChange")]
        public ActionResult RequestEmailChange(EmailChangeRequest request)
        {
            try
            {
                new UserManager().RequestEmailChange(request.UserId, request.NewEmail);
                return Ok(new { message = "Se envió un código de confirmación al correo nuevo." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("Profile/ConfirmEmailChange")]
        public ActionResult ConfirmEmailChange(ConfirmEmailChangeRequest request)
        {
            try
            {
                var user = new UserManager().ConfirmEmailChange(request.UserId, request.NewEmail, request.OtpCode);
                AuditHelper.TryAudit(request.UserId, "Update", "Users", request.UserId, "Cambio de correo electrónico confirmado");
                return Ok(Sanitize(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        public class UserActivationRequest
        {
            public string Email { get; set; } = string.Empty;
            public string TokenCode { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
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

        // Cambio de contraseña para un usuario YA logueado (pantalla /ChangePassword).
        // No usa OTP: la sesión activa + la contraseña actual ya verifican identidad.
        [HttpPost]
        [Route("ChangePassword")]
        public ActionResult ChangePassword(ChangePasswordAuthenticatedRequest request)
        {
            try
            {
                var um = new UserManager();
                um.ChangePasswordAuthenticated(request.UserId, request.CurrentPassword, request.NewPassword, request.ConfirmPassword);

                AuditHelper.TryAudit(request.UserId, "Update", "Users", request.UserId, "Cambio de contraseña (sesión activa)");

                return Ok(new { message = "Contraseña actualizada correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("ChangePasswordByEmail")]
        [AllowAnonymous]
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
        [AllowAnonymous]
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
                AuditHelper.TryAudit(null, "Create", "Users", user.Id, $"Autorregistro de comprador: {user.Email}");
                return Ok(new { message = "Comprador registrado con éxito. Active su cuenta con el código enviado a su correo." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("ConfirmChangePasswordByEmail")]
        [AllowAnonymous]
        public ActionResult ConfirmChangePasswordByEmail([FromBody] ConfirmChangePasswordEmailRequest request, [FromQuery] string tokenCode)
        {
            try
            {
                var um = new UserManager();
                um.ConfirmChangePassword(request.Email, tokenCode, request.NewPassword, request.ConfirmPassword);
                AuditHelper.TryAudit(null, "Update", "Users", null, $"Cambio de contraseña confirmado por correo: {request.Email}");
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

        // Genera una contraseña aleatoria que cumple los requisitos de complejidad,
        // usada cuando un administrador crea un usuario y no le asigna una contraseña
        // real: nadie (ni el admin) la conoce; el usuario define la suya propia al
        // activar la cuenta (ver UserManager.ActivateAccount).
        private static string GenerateRandomCompliantPassword()
        {
            var digits = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 999999);
            return $"Tmp{digits}!Ax";
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "Administrator")]
        public ActionResult Create(User user, [FromQuery] int? callerUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(user.Password))
                {
                    user.Password = GenerateRandomCompliantPassword();
                }

                var um = new UserManager();
                um.Create(user);

                try
                {
                    new AuditManager().Create(new Audit
                    {
                        UserId = callerUserId,
                        Action = "Create",
                        EntityName = "Users",
                        EntityId = user.Id,
                        Description = $"Usuario creado por administrador: {user.Email}"
                    });
                }
                catch { /* no bloquear la operación ya aplicada */ }

                return Ok(Sanitize(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Update")]
        [Authorize(Roles = "Administrator")]
        public ActionResult Update(User user, [FromQuery] int? callerUserId)
        {
            try
            {
                var um = new UserManager();
                um.Update(user);

                try
                {
                    new AuditManager().Create(new Audit
                    {
                        UserId = callerUserId,
                        Action = "Update",
                        EntityName = "Users",
                        EntityId = user.Id,
                        Description = $"Usuario actualizado: {user.Email}"
                    });
                }
                catch { /* no bloquear la operación ya aplicada */ }

                return Ok(Sanitize(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(User user, [FromQuery] int? callerUserId)
        {
            try
            {
                var um = new UserManager();
                um.Delete(user);

                try
                {
                    new AuditManager().Create(new Audit
                    {
                        UserId = callerUserId,
                        Action = "LogicalDelete",
                        EntityName = "Users",
                        EntityId = user.Id,
                        Description = $"Usuario desactivado: {user.Email}"
                    });
                }
                catch { /* no bloquear la operación ya aplicada */ }

                return Ok(Sanitize(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
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
                // El frontend lee este mensaje para mostrar el bloqueo o error real
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("ValidateLoginOtp")]
        [AllowAnonymous]
        public ActionResult ValidateLoginOtp(OtpRequest request)
        {
            try
            {
                var um = new UserManager();
                var user = um.ValidateLoginOtp(request.Email, request.OtpCode);
                return Ok(WithToken(Sanitize(user)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("ResetPassword")]
        [AllowAnonymous]
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
                AuditHelper.TryAudit(null, "Update", "Users", null, $"Contraseña restablecida vía recuperación: {request.Email}");
                return Ok("Contraseña restablecida correctamente.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("RecoverPassword")]
        [AllowAnonymous]
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
        [AllowAnonymous]
        public ActionResult ConfirmResetPassword(ResetPasswordRequest request)
        {
            try
            {
                var um = new UserManager();
                um.ConfirmResetPassword(request.Email, request.OtpCode, request.NewPassword, request.ConfirmPassword);
                AuditHelper.TryAudit(null, "Update", "Users", null, $"Contraseña restablecida vía recuperación: {request.Email}");
                return Ok("Contraseña restablecida correctamente.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("ActivateAccount")]
        [AllowAnonymous]
        public ActionResult ActivateAccount([FromBody] OtpRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.OtpCode))
                {
                    return BadRequest(new { message = "El correo electrónico y el código OTP son requeridos." });
                }

                var um = new UserManager();
                // Endpoint legado sin campos de contraseña; el flujo real de activación
                // es la acción Activate (abajo), que sí exige establecer la contraseña.
                um.ActivateAccount(request.Email?.Trim(), request.OtpCode?.Trim(), null, null);
                AuditHelper.TryAudit(null, "Update", "Users", null, $"Cuenta activada (endpoint legado): {request.Email}");
                return Ok(new { message = "Cuenta activada correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("Activate")]
        [AllowAnonymous]
        public ActionResult Activate([FromBody] UserActivationRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.TokenCode))
                {
                    return BadRequest(new { message = "El correo electrónico y el código OTP son requeridos." });
                }

                if (string.IsNullOrWhiteSpace(request.NewPassword) || string.IsNullOrWhiteSpace(request.ConfirmPassword))
                {
                    return BadRequest(new { message = "Debe establecer y confirmar su nueva contraseña." });
                }

                var um = new UserManager();
                um.ActivateAccount(request.Email?.Trim(), request.TokenCode?.Trim(), request.NewPassword, request.ConfirmPassword);
                AuditHelper.TryAudit(null, "Update", "Users", null, $"Cuenta activada: {request.Email}");
                return Ok(new { message = "Cuenta activada con éxito." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("ResendOtp")]
        [AllowAnonymous]
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
