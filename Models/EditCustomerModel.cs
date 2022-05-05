using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Triton.Model.TritonGroup.Custom;
using Triton.Model.TritonGroup.StoredProcs;
using Triton.Model.TritonOps.Tables;
using LookUpCodes = Triton.Model.TritonGroup.Tables.LookUpCodes;

namespace Triton.BusinessOnline.Models
{    public class AccountControllerBaseModel
    {
        public string SuccessMessage { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string PhoneNumber { get; set; }


        [Display(Name = "Customer")]
        public int SelectedID { get; set; }

        public List<LookUpCodes> LookUpCodesList { get; set; }

        public int SelectedLookupcodeID { get; set; }
        public int? UserID { get; set; }
       public UserEmployeeMap EmployeeMap { get; set; }
    }

    public class UserDetailsModel
    {
        public UserDetails UserDetails { get; set; }

        public int? NewCustomerID { get; set; }

        public int? SelectedID { get; set; }

        public int UserID { get; set; }

        public string Type { get; set; }
    }

    public class EditCustomerModel : AccountControllerBaseModel
    {
        public UserMapCustomerModels UserMapCustomerModels { get; set; }

        public int? SelectedID { get; set; }

        public int UserID { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }
        //public List<string> Roles { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public int UserID { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        //public string Code { get; set; }
    }
}
