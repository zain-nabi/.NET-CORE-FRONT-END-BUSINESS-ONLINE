using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Triton.Core;
using Triton.Model.TritonGroup.Tables;
using Triton.BusinessOnline.Utils;
using Triton.Interface.TritonGroup;
using System.Data.SqlTypes;

namespace Triton.BusinessOnline.Areas.Identity.Pages.Account.Manage
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<ExternalUser> _userManager;
        private readonly SignInManager<ExternalUser> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;
        private readonly IExternalUser _userService;

        public ChangePasswordModel(
            UserManager<ExternalUser> userManager,
            SignInManager<ExternalUser> signInManager,
            ILogger<ChangePasswordModel> logger,
            IExternalUser userService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _userService = userService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string OldPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToPage("./SetPassword");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                string messages = string.Join("; ", ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage));
                return Page();
            }

            //var user = await _userManager.GetUserAsync(User);
            var user = await _userManager.FindByNameAsync(User.GetUserName());
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            user.PasswordHash = Security.HashPassword(Input.NewPassword);
            DateTime lowDate = Convert.ToDateTime("1/1/1753");
            user.LockoutEndDateUtc = ((user.LockoutEndDateUtc == null) || (user.LockoutEndDateUtc <= lowDate)) ? (DateTime)SqlDateTime.MinValue : user.LockoutEndDateUtc;

            var result = await _userService.PutUpdateAsync(user);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Failed to update the password");
                return Page();
            }
            //var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            //if (!changePasswordResult.Succeeded)
            //{
            //    foreach (var error in changePasswordResult.Errors)
            //    {
            //        ModelState.AddModelError(string.Empty, error.Description);
            //    }
            //    return Page();
            //}

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");
            StatusMessage = "Your password has been changed.";

            return RedirectToPage();
        }
    }
}
