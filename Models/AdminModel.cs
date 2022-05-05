using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Triton.Model.TritonGroup.Custom;
using Triton.Model.TritonGroup.Tables;

namespace Triton.BusinessOnline.Models
{
    public class AdminModel
    {
        public ExternalUserModel ExternalUserModel { get; set; }
        public List<Triton.Model.TritonGroup.Tables.Roles> RoleList { get; set; }
        public List<Triton.Model.CRM.Tables.Customers> CustomerList { get; set; }
        public int CustomerID { get; set; }
        public List<Triton.Model.TritonGroup.Custom.ExternalUserMapModel> SelectedCustomers { get; set; }
        public int ExternalUserID { get; set; }
        public string CustomerText { get; set; }
        public List<ExternalUserMap> ExternalUserMapList { get; set; }
        public string EmailExistErrorMessage { get; set; }
    }
}
