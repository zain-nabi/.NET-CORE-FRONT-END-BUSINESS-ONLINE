using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Triton.BusinessOnline.Utils
{
    public class StringHelper
    {
        public static class Types
        {
            public const string NoRecords = "No Records";
            public const string UpdateSuccess = "Update Success";
            public const string UpdateFailed = "Update Failed";
            public const string SaveSuccess = "Saved Successfully";
            public const string SaveFailed = "Save Failed";
            public const string DeleteSuccess = "Deleted Successfully";
            public const string DeleteFailed = "Delete Failed";
            public const string EmailSuccess = "Email sent successfully";
        }

        public static class Html
        {
            //HTML
            public const string Danger = "danger";
            public const string Dark = "dark";
            public const string Info = "info";
            public const string Light = "light";
            public const string Primary = "primary";
            public const string Secondary = "secondary";
            public const string Success = "success";
            public const string Warning = "warning";
            public const string FailedIcon = "fad fa-times-circle text-danger";
            public const string SuccessIcon = "fad fa-check-circle text-success";
            public const string UpdateRecordSuccessMessage = "The record was successfully updated";
            public const string DeleteRecordSuccessMessage = "The record was successfully deleted";
            public const string SaveRecordSuccessMessage = "The record was successfully saved";
            public const string UpdateRecordFailedMessage = "The record failed to update";
            public const string UpdateRecordSuccessTitle = "Update successful";
            public const string UpdateRecordFailedTitle = "Update failed";
            public const string SaveRecordSuccessTitle = "Save successful";
            public const string DeleteRecordSuccessTitle = "Delete successful";
            public const string UserNameErrorMessage = "Username already exists";
            public const string EmailAddressErrorMessage = "Email address already exists";
            public const string VehicleRegExistMessage = "Vehicle registration already exist";
        }
    }
}
