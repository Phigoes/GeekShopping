using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GeekShopping.IdentityServer.Pages.Account;

[SecurityHeaders]
[AllowAnonymous]
public class AccessDeniedModel : PageModel
{
    public async Task<IActionResult> OnGet(string returnUrl)
    {
        return Page();
    }
}