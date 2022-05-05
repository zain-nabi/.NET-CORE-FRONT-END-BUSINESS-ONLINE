using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Triton.Core;
using Triton.Interface.TritonGroup;
using Triton.Model.TritonGroup.Tables;

namespace Triton.BusinessOnline.Areas.Identity
{
    public class UserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ExternalUser>
    {
        private readonly IRole _roleService;
        private readonly IExternalUserMap _userMapService;
        private readonly IExternalUser _externalUser;
        private readonly IExternalUserRole _externalUserRole;

        public UserClaimsPrincipalFactory(UserManager<ExternalUser> userManager, IOptions<IdentityOptions> optionsAccessor, IRole role, IExternalUserMap userMapService, IExternalUser externalUser,
            IExternalUserRole externalUserRole) : base(userManager, optionsAccessor)
        {
            _roleService = role;
            _userMapService = userMapService;
            _externalUser = externalUser;
            _externalUserRole = externalUserRole;
        }
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ExternalUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            var UserID = await _externalUser.FindByNameAsync(user.UserName);
            int userID = UserID.ExternalUserID;
            identity.AddClaim(new Claim("UserID", $"{userID}"));
            identity.AddClaim(new Claim("FullName", $"{user.FirstName} {user.LastName}"));
            identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));

            // Get the user CustomerID
            var ucMapList = await _userMapService.GetUserCustomerMapModel(userID);

            if(ucMapList == null)
            {
                // Get the users Role information
                var roleList = await _externalUserRole.GetRolesByUserId(userID, StringHelpers.Database.TritonGroup);

                foreach (var item in roleList)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, item.RoleName));
                }
            }
            else
            {
                var customerIds = string.Join(", ", ucMapList.UserMap.Where(x => x.DeletedOn == null).Select(x => x.CustomerID));
                identity.AddClaim(new Claim("CustomerID", customerIds));

                // Get the users Role information
                var roleList = await _externalUserRole.GetRolesByUserId(userID, StringHelpers.Database.TritonGroup);

                foreach (var item in roleList)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, item.RoleName));
                }
            }



            // Add the users roles to the identity
            return identity;
        }
    }
}

