using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Triton.BusinessOnline.Models;
using Triton.Core;
using Triton.Interface.TritonGroup;
using Triton.Model.TritonGroup.Custom;
using Triton.Model.TritonGroup.Tables;
using Triton.Model.TritonGroup.ViewModel;
using Triton.Interface.CRM;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net;

namespace Triton.BusinessOnline.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<ExternalUser> _userManager;
        private readonly IUser _userService;
        private readonly IUserRole _userRoleService;
        private readonly IRole _roleService;
        private readonly IUserMap _userMapService;
        private readonly IExternalUser _externalUserService;
        private readonly IExternalUserRole _externalUserRole;
        private readonly ICustomers _customerService;
        private readonly IExternalUserMap _externalUserMap;


        public AdminController(UserManager<ExternalUser> userManager, IUser userService, IUserRole userRole, IRole role, IUserMap userMapService, IExternalUser externalUserService, IExternalUserRole externalUserRole, ICustomers customers
            , IExternalUserMap externalUserMap)
        {
            _userManager = userManager;
            _userService = userService;
            _userRoleService = userRole;
            _roleService = role;
            _userMapService = userMapService;
            _externalUserService = externalUserService;
            _externalUserRole = externalUserRole;
            _customerService = customers;
            _externalUserMap = externalUserMap;
        }

        public async Task<IActionResult> CreateUser()
        {
            return View(await GetRoles());
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ExternalUser
                {
                    UserName = model.User.Email,
                    Email = model.User.Email,
                    FirstName = model.User.FirstName,
                    LastName = model.User.LastName,
                    PhoneNumberConfirmed = true,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(user, model.User.PasswordHash);
                if (result.Succeeded)
                {
                    //Get the new created user
                    var userInfo = await _userService.FindByNameAsync(model.User.Email);

                    //Create the users roles
                    var userRole = new UserRoles
                    {
                        UserID = userInfo.UserID,
                        RoleID = model.SelectedRoleId,
                        CreatedByUserID = User.GetUserId(),
                        CreatedOn = DateTime.Now,
                        DeletedByUserID = null,
                        DeletedOn = null
                    };

                    try
                    {
                        var userRoleId = await _userRoleService.Post(userRole, StringHelpers.Database.TritonGroup);

                        if (userRoleId > 0)
                        {
                            ViewData["Message"] = HtmlHelpers.Success;
                        }
                    }
                    catch
                    {
                        ViewData["Message"] = HtmlHelpers.Failure;
                    }

                    //_logger.LogInformation("User created a new account with password.");

                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    //var callbackUrl = Url.Page(
                    //    "/Account/ConfirmEmail",
                    //    pageHandler: null,
                    //    values: new {area = "Identity", userId = user.Id, code = code},
                    //    protocol: Request.Scheme);

                    //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    //if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    //{
                    //    return RedirectToPage("RegisterConfirmation", new {email = model.User.Email});
                    //}

                    //await _signInManager.SignInAsync(user, isPersistent: false);

                    //return LocalRedirect(returnUrl);
                    //return Redirect("Account/Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                var roles = await GetRoles();
                model.Role = roles.Role;
                return View(model);
            }

            return View(model);
        }

        public async Task<IActionResult> ListUsers()
        {
            try
            {
                var model = await _externalUserService.GetUserWithRoles();


                return View(model);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<IActionResult> Delete(int userId, bool lockoutEnabled)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get the current user information
                    var model = await _userService.FindByIdAsync(userId);
                    model.LockoutEnabled = !lockoutEnabled;

                    var result = await _userService.PutUpdateAsync(model);
                    if (result)
                        ViewData["Message"] = HtmlHelpers.Update;

                    var returnModel = await _userService.GetUserWithRoles(userId, "1, 2");

                    return View("Edit", returnModel);
                }
                catch
                {
                    ViewData["Message"] = HtmlHelpers.Failure;
                }
            }
            else
            {
                ModelState.AddModelError("Error",
                    ModelState.Keys.SelectMany(key => ModelState[key].Errors).FirstOrDefault()?.ErrorMessage);
            }

            ViewData["Header"] = "User not <span class='font-weight-semi-bold'>found</span>";
            ViewData["Message"] = "Sorry we could not find the user details";
            return View("~/Views/Shared/Error.cshtml");
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int ExternalUserID)
        {
            var AdminModel = new AdminModel();
            AdminModel.ExternalUserModel = await _externalUserService.FindByExternalUserID(ExternalUserID);
            AdminModel.RoleList = await _externalUserRole.GetActiveUserRoles();
            AdminModel.CustomerList = await _customerService.GetAllActiveCustomers();
            AdminModel.ExternalUserID = ExternalUserID;
            AdminModel.SelectedCustomers = await _externalUserMap.GetUserMapCustomers(ExternalUserID);
            return View(AdminModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AdminModel model)
        {
            var AdminModel = new AdminModel();
            var AdminHelper = new Triton.BusinessOnline.Helper.AdminHelper();
            model.ExternalUserMapList =
                                JsonConvert.DeserializeObject<List<Triton.Model.TritonGroup.Tables.ExternalUserMap>>(WebUtility.UrlDecode(model.CustomerText) ?? string.Empty);
            var emailCheck = await _externalUserService.CheckIfEmailExist(model.ExternalUserModel.Email);
            var user = await _externalUserService.FindByExternalUserID(model.ExternalUserID);            


            if (user.Email == model.ExternalUserModel.Email)
            {

                if (user.Email != model.ExternalUserModel.Email)
                {
                    if (user.Email == model.ExternalUserModel.Email)
                    {
                        model.EmailExistErrorMessage = "";
                    }
                    if (user.Email != model.ExternalUserModel.Email)
                    {
                        model.EmailExistErrorMessage = "Please enter a unique email address";
                    }
                    if (emailCheck.ExternalUserID == 0)
                    {
                        await _externalUserMap.UpdateExternalUserMap(AdminHelper.AssignExternalUserMapModelProps(model.ExternalUserMapList, User.GetUserId()));
                        var ExternaluserRole = await _externalUserRole.GetUserRoleByID(model.ExternalUserID);
                        ExternaluserRole.RoleID = model.ExternalUserModel.RoleID;
                        await _externalUserRole.Put(ExternaluserRole, "TritonGroup");
                        await _externalUserService.PutUpdateAsync(AdminHelper.AssignPropertiesEdit(model.ExternalUserModel, model.ExternalUserID));
                        return RedirectToAction("ListUsers", "Admin");
                    }
                    else
                    {
                        AdminModel.ExternalUserModel = await _externalUserService.FindByExternalUserID(model.ExternalUserID);
                        AdminModel.RoleList = await _externalUserRole.GetActiveUserRoles();
                        AdminModel.CustomerList = await _customerService.GetAllActiveCustomers();
                        AdminModel.ExternalUserID = model.ExternalUserID;
                        AdminModel.SelectedCustomers = await _externalUserMap.GetUserMapCustomers(model.ExternalUserID);
                        AdminModel.EmailExistErrorMessage = "Please enter a unique email address";
                        return View(AdminModel);
                    }
                }
                var result = await _externalUserMap.UpdateExternalUserMap(AdminHelper.AssignExternalUserMapModelProps(model.ExternalUserMapList, User.GetUserId()));
                var userRole = await _externalUserRole.GetUserRoleByID(model.ExternalUserID);
                userRole.RoleID = model.ExternalUserModel.RoleID;
                await _externalUserRole.Put(userRole, "TritonGroup");
                await _externalUserService.PutUpdateAsync(AdminHelper.AssignPropertiesEdit(model.ExternalUserModel, model.ExternalUserID));
                return RedirectToAction("ListUsers", "Admin");
            }


            if (user.Email == model.ExternalUserModel.Email)
            {
                model.EmailExistErrorMessage = "";
            }
            if (user.Email != model.ExternalUserModel.Email)
            {
                model.EmailExistErrorMessage = "Please enter a unique email address";
            }
            if (emailCheck.ExternalUserID == 0)
            {
                await _externalUserMap.UpdateExternalUserMap(AdminHelper.AssignExternalUserMapModelProps(model.ExternalUserMapList, User.GetUserId()));
                var ExternaluserRole = await _externalUserRole.GetUserRoleByID(model.ExternalUserID);
                ExternaluserRole.RoleID = model.ExternalUserModel.RoleID;
                await _externalUserRole.Put(ExternaluserRole, "TritonGroup");
                await _externalUserService.PutUpdateAsync(AdminHelper.AssignPropertiesEdit(model.ExternalUserModel, model.ExternalUserID));
                return RedirectToAction("ListUsers", "Admin");
            }
            else
            {
                AdminModel.ExternalUserModel = await _externalUserService.FindByExternalUserID(model.ExternalUserID);
                AdminModel.RoleList = await _externalUserRole.GetActiveUserRoles();
                AdminModel.CustomerList = await _customerService.GetAllActiveCustomers();
                AdminModel.ExternalUserID = model.ExternalUserID;
                AdminModel.SelectedCustomers = await _externalUserMap.GetUserMapCustomers(model.ExternalUserID);
                AdminModel.EmailExistErrorMessage = "Please enter a unique email address";
                return View(AdminModel);
            }
        }

        private async Task GetUserRoles(UserWithRoles model)
        {
            const string roleIds = "1, 2";
            var roles = await _userService.GetUserWithRoles(model.Users.UserID, roleIds);

            model.Roles = roles.Roles;
        }


        [HttpGet]
        public async Task<IActionResult> AddCustomer(int userId, string name)
        {
            try
            {
                var model = await GetCustomerMap(userId);
                model.Name = name;
                return View(model);
            }
            catch (HttpRequestException e)
            {
                ModelState.AddModelError("error", e.Message);
                ViewData["Header"] = "User not <span class='font-weight-semi-bold'>found</span>";
                ViewData["Message"] = "Sorry we could not find the user details";
                return View("~/Views/Shared/Error.cshtml");
            }
        }


        public async Task<ActionResult> AddCustomer(EditCustomerModel model)
        {
            const int userTypeLcid = 299;
            var success = false;
            var customerId = model.SelectedID;

            if (ModelState.IsValid)
            {
                if (model.SelectedID == 0 || model.SelectedID == null)
                {
                    ModelState.AddModelError("Error", "InvalidSelection");
                }
                else
                {
                    //Link the User to the customer
                    var userMap = new UserMap
                    {
                        UserID = model.UserID,
                        CustomerID = customerId,
                        SupplierID = null,
                        UserTypeLCID = userTypeLcid,
                        SystemID = 20,
                        CreatedOn = DateTime.Now,
                        CreatedByUserID = User.GetUserId()
                    };
                    //var postResult =  PostAsJsonAsyncString("TritonGroupStoredProcs", "Post_proc_UserMap_Insert", userMap);
                    var postResult = await _userMapService.PostUserMapObject(userMap);

                    switch (postResult)
                    {
                        case "Duplicate Customer":
                            ModelState.AddModelError("Error", "Duplicate Customer");
                            break;
                        case "Duplicate Supplier":
                            ModelState.AddModelError("Error", "Duplicate Supplier");
                            break;
                        default:
                            success = true;
                            break;
                    }
                }
            }

            var userDetails = await GetCustomerMap(model.UserID);

            if (!success) return View(userDetails);

            userDetails.SuccessMessage = HtmlHelpers.Success;

            //return View(returnPath, userDetails);
            return RedirectToAction("AddCustomer", new { model.UserID, name = model.Name });
        }

        public async Task<ActionResult> DeleteLink(int userMapId, string actionName, string name)
        {
            // Get the current UserMap Record
            var model = await _userMapService.GetUserMap(userMapId);
            switch (actionName)
            {
                case "Delete":
                    model.DeletedByUserID = User.GetUserId();
                    model.DeletedOn = DateTime.Now;
                    break;
                default:
                    model.DeletedByUserID = null;
                    model.DeletedOn = null;
                    break;
            }

            // Update the record
            var result = await _userMapService.PutUserMap(model);

            var userDetails = await GetCustomerMap(model.UserID);
            if (!result) return View("AddCustomer", userDetails);

            userDetails.SuccessMessage = HtmlHelpers.Success;

            //return View(returnPath, userDetails);
            return RedirectToAction("AddCustomer", new { model.UserID, name });
        }

        private async Task<UserViewModel> GetRoles()
        {
            return new UserViewModel { Role = await _roleService.GetRolesByIds("1,2 ", StringHelpers.Database.TritonGroup) };
        }

        private async Task<EditCustomerModel> GetCustomerMap(int userId)
        {
            var umc = await _userMapService.GetUserCustomerMapModel(userId);
            var model = new EditCustomerModel();

            if (umc == null) return model;
            if (umc.UserMap != null && umc.UserMap.Count > 0)
            {
                model.UserID = umc.UserMap.Select(x => x.UserID).FirstOrDefault();
                model.UserMapCustomerModels = umc;
            }
            else
            {
                //Display an error
                ModelState.AddModelError("Error", "User not found");
            }

            return model;
        }
    }
}