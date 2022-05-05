using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Triton.BusinessOnline.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordConfirmation : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Email { get; set; }
        public void OnGet()
        {
            ViewData["passwordReset"] = $"{Email}";
        }
    }
}
