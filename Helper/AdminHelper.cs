using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Triton.BusinessOnline.Models;
using Triton.Model.TritonGroup.Custom;
using Triton.Model.TritonGroup.Tables;

namespace Triton.BusinessOnline.Helper
{
    public class AdminHelper
    {
        public ExternalUser AssignPropertiesEdit(ExternalUserModel model, int externalUserID)
        {
            var obj = new ExternalUser()
            {
                ExternalUserID = externalUserID,
                UserName = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PasswordHash = model.PasswordHash,
                SecurityStamp = model.SecurityStamp,
                PhoneNumber = model.PhoneNumber,
                PhoneNumberConfirmed = model.PhoneNumberConfirmed,
                Email = model.Email,
                EmailConfirmed = model.EmailConfirmed,
                LockoutEndDateUtc = model.LockoutEndDateUtc,
                LockoutEnabled = model.LockoutEnabled,
                AccessFailedCount = model.AccessFailedCount
            };

            return obj;
        }

        public ExternalUserMapModel AssignExternalUserMapModelProps(List<ExternalUserMap> model, int UserID)
        {
            var externalUserList = new List<ExternalUserMap>();
            var ExternalUserMapModel = new ExternalUserMapModel();

            foreach (var item in model)
            {
                var obj = new ExternalUserMap()
                {
                    ExternalUserID = item.ExternalUserID,
                    CustomerID = item.CustomerID,
                    UserTypeLCID = 298,
                    CreatedByUserID = UserID,
                    CreatedOn = DateTime.Now
                };
                externalUserList.Add(obj);
            }
            ExternalUserMapModel.ExternalUserMapList = externalUserList;
            return ExternalUserMapModel;
        }


        public ExternalUserMapModel AssignExternalUserMapModelPropsRegister(List<ExternalUserMap> model, int UserID, ExternalUser user)
        {
            var externalUserList = new List<ExternalUserMap>();
            var ExternalUserMapModel = new ExternalUserMapModel();

            foreach (var item in model)
            {
                var obj = new ExternalUserMap()
                {
                    ExternalUserID = item.ExternalUserID,
                    CustomerID = item.CustomerID,
                    UserTypeLCID = 298,
                    CreatedByUserID = UserID,
                    CreatedOn = DateTime.Now
                };
                externalUserList.Add(obj);
            }
            ExternalUserMapModel.ExternalUserMapList = externalUserList;
            ExternalUserMapModel.ExternalUser = user;
            return ExternalUserMapModel;
        }
    }
}
