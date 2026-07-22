using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
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
                // Si salta una validación en el manager, se envía el texto exacto del error al SweetAlert
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("Activate")]
        public ActionResult Activate(UserActivationRequest request)
        {
            try
            {
                var um = new UserManager();
                // Invoca la validación dura y activación en el CoreApp
                um.ActivateAccount(request.Email, request.TokenCode);

                // Retorna un estado exitoso para que JavaScript dispare el SweetAlert
                return Ok(new { message = "Cuenta activada con éxito." });
            }
            catch (Exception ex)
            {
                // Si el token expiró o es incorrecto, el mensaje de error viaja directo al SweetAlert rojo
                return StatusCode(500, ex.Message);
            }
        }

        // Mini-clase estructural para el mapeo limpio del JSON entrante de JavaScript
        public class UserActivationRequest
        {
            public string Email { get; set; }
            public string TokenCode { get; set; }
        }

    }
}
