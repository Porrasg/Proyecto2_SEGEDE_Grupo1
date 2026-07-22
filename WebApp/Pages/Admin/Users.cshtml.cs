using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SEGEDE_Grupo1.WebApp.Pages.Admin;

// Nota arquitectónica: PageModel para User Management consumiendo la Web API REST.
public class UsersModel : PageModel
{
    // Método manejador que se ejecuta al recibir una petición HTTP GET para inicializar la vista y presentar los datos en pantalla.
    public void OnGet() { }
}
