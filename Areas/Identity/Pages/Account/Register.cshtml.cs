using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Triton.BusinessOnline.Utils;
using Triton.Interface.TritonGroup;
using Triton.Model.TritonGroup.Tables;
using Triton.Core;
using Triton.Interface.CRM;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Net;
using Triton.Model.TritonGroup.Custom;

namespace Triton.BusinessOnline.Areas.Identity.Pages.Account
{
    [Authorize(Roles =  "Administrator")]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ExternalUser> _signInManager;
        private readonly UserManager<ExternalUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IExternalUser _externalUser;
        private readonly IExternalUserMap _userMapService;
        private readonly ICustomers _customerService;
        private readonly IExternalUserRole _externalUserRole;

        public RegisterModel(
            UserManager<ExternalUser> userManager,
            SignInManager<ExternalUser> signInManager,
            ILogger<RegisterModel> logger,
            IExternalUser externalUser, IExternalUserMap userMap, ICustomers customers, IExternalUserRole externalUserRole)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _externalUser = externalUser;
            _userMapService = userMap;
            _customerService = customers;
            _externalUserRole = externalUserRole;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }        
        public List<Triton.Model.CRM.Tables.Customers> CustomerList { get; set; }

        public List<Triton.Model.TritonGroup.Tables.Roles> RoleList { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public string EmailExist { get; set; }

        public class InputModel
        {
            //[Required]
            //[EmailAddress]
            //[Display(Name = "Email")]
            //public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string UserName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PasswordHash { get; set; }
            public string SecurityStamp { get; set; }
            public string PhoneNumber { get; set; }
            public bool PhoneNumberConfirmed { get; set; }
            public string Email { get; set; }
            public bool EmailConfirmed { get; set; }
            public DateTime LockoutEndDateUtc { get; set; }
            public bool LockoutEnabled { get; set; }
            public int AccessFailedCount { get; set; }
            public int CustomerID { get; set; }
            public int RoleID { get; set; }
            public string CustomerText { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            CustomerList = await _customerService.GetAllActiveCustomers();
            RoleList = await _externalUserRole.GetActiveUserRoles();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            var AdminModel = new Triton.BusinessOnline.Models.AdminModel();

            var AdminHelper = new Triton.BusinessOnline.Helper.AdminHelper();
            AdminModel.ExternalUserMapList =
                                JsonConvert.DeserializeObject<List<Triton.Model.TritonGroup.Tables.ExternalUserMap>>(WebUtility.UrlDecode(Input.CustomerText) ?? string.Empty);

            returnUrl = string.Format("ListUsers","Admin");            
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            var emailCheck = await _externalUser.CheckIfEmailExist(Input.Email);
            CustomerList = await _customerService.GetAllActiveCustomers();
            RoleList = await _externalUserRole.GetActiveUserRoles();
            if (emailCheck.ExternalUserID == 1)
            {
                EmailExist = "Please enter a unique email address";
            }
            else
            {
                var user = new ExternalUserMapModel();
                user.ExternalUser = new ExternalUser();
                user.ExternalUser.UserName = Input.Email;
                user.ExternalUser.Email = Input.Email;
                user.ExternalUser.EmailConfirmed = Input.EmailConfirmed;
                user.ExternalUser.PhoneNumber = Input.PhoneNumber;
                user.ExternalUser.PhoneNumberConfirmed = Input.PhoneNumberConfirmed;
                user.ExternalUser.SecurityStamp = null;
                user.ExternalUser.AccessFailedCount = 0;
                user.ExternalUser.LockoutEnabled = false;
                user.ExternalUser.LockoutEndDateUtc = null;
                user.ExternalUser.FirstName = Input.FirstName;
                user.ExternalUser.LastName = Input.LastName;
                user.ExternalUser.PasswordHash = Security.HashPassword(Input.Password);

                var result = "";
                if (AdminModel.ExternalUserMapList != null)
                {
                    result = await _userMapService.InsertExternalUserMap(AdminHelper.AssignExternalUserMapModelPropsRegister(AdminModel.ExternalUserMapList, User.GetUserId(), user.ExternalUser));

                }
                else
                {
                    var ExternalUserCustomerMapModel = new ExternalUserMapModel();
                    ExternalUserCustomerMapModel.ExternalUser = user.ExternalUser;
                    result = await _userMapService.InsertExternalUserMap(ExternalUserCustomerMapModel);
                }

                if (result != null)
                {
                    var externalUserRole = new ExternalUserRole
                    {
                        ExternalUserID = Convert.ToInt32(result),
                        RoleID = Input.RoleID,
                        CreatedOn = DateTime.Now,
                        CreatedByUserID = User.GetUserId(),
                        DeletedByUserID = null,
                        DeletedOn = null
                    };
                    await _externalUserRole.Post(externalUserRole, "TritonGroup");
                    _logger.LogInformation("User created a new account with password.");
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user.ExternalUser);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.ExternalUser.ExternalUserID, code = code },
                        protocol: Request.Scheme);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                    }
                    else
                    {
                        returnUrl = @Url.Action("ListUsers", "Admin");
                        //await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Message", "Home", new { type = Utils.StringHelper.Types.SaveSuccess, url = returnUrl });
                    }
                }
            }
            return Page();
        }

    }
}
