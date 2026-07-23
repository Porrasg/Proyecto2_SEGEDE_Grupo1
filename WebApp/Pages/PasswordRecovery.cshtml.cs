using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SEGEDE_Grupo1.WebApp.Pages;

public class PasswordRecoveryModel : PageModel
{
    public void OnGet()
    {
        ViewData["HideSidebar"] = true;
    }
}