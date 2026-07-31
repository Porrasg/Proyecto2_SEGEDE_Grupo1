using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SEGEDE_Grupo1.WebApp.Pages;

// Requiere sesión activa: el cambio de contraseña ya NO es una pantalla pública
// (para eso está RecoverPassword/ResetPassword). El guard real de sesión se
// aplica en el cliente (ChangePasswordViewController.js), igual que el resto
// de páginas autenticadas de este proyecto.
public class ChangePasswordModel : PageModel
{
    public void OnGet() { }
}