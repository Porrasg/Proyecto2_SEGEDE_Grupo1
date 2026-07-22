using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SEGEDE_Grupo1.WebApp.Pages;

// Nota arquitectónica: PageModel para Access Denied consumiendo la Web API REST.
public class AccessDeniedModel : PageModel
{
    // Método manejador que se ejecuta al recibir una petición HTTP GET para inicializar la vista y presentar los datos en pantalla.
    public void OnGet() { }
}
