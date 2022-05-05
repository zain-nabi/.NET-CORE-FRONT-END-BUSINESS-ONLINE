using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Triton.BusinessOnline.Utils;
using Triton.Model.TritonGroup.Tables;
using Triton.Core;

namespace Triton.BusinessOnline.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ExternalUser> _userManager;
        //private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;
        private static IWebHostEnvironment _webHostEnvironment;

        public ForgotPasswordModel(UserManager<ExternalUser> userManager, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            //_emailSender = emailSender;
            _config = configuration;
            _webHostEnvironment= webHostEnvironment;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                var email = new Email(_config.GetSection("SMTP").GetSection("ip").Value, _config.GetSection("SMTP").GetSection("port").Value);
                //await email.SendEmail(Input.Email, "Reset Password", $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");


                var template = new Dictionary<string, string>
                {
                    { "callbackUrl", callbackUrl}
                };

                var emailBody = EmailHelper.ResetPasswordEmail(template, "Html/ResetPassword.html", _webHostEnvironment);
                await email.SendEmail(Input.Email, "Reset Password", emailBody);
                //await _emailSender.SendEmailAsync(
                //    Input.Email,
                //    "Reset Password",
                //    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToPage("./ForgotPasswordConfirmation",new { Email = user.Email});
            }

            return Page();
        }
    }
}
