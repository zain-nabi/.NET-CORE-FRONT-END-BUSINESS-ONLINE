using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Triton.Interface.TritonGroup;
using Triton.Model.TritonGroup.Tables;

namespace Triton.BusinessOnline.Areas.Identity
{
    public class UserStore : IUserStore<ExternalUser>, IUserEmailStore<ExternalUser>, IUserPasswordStore<ExternalUser>, IUserPhoneNumberStore<ExternalUser>, IUserTwoFactorStore<ExternalUser>, IUserAuthenticatorKeyStore<ExternalUser>, IUserRoleStore<ExternalUser>, IUserSecurityStampStore<ExternalUser>
    {
        private readonly IExternalUser _userService;
        private readonly IRole _roleService;

        public UserStore(IExternalUser userService, IRole roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        public void Dispose()
        {
            //Dispose();
            //throw new NotImplementedException();
        }

        public Task<string> GetUserIdAsync(ExternalUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.ExternalUserID.ToString());
        }

        public Task<string> GetUserNameAsync(ExternalUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetUserNameAsync(ExternalUser user, string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedUserNameAsync(ExternalUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedUserNameAsync(ExternalUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.FromResult(0);
        }

        public async Task<IdentityResult> CreateAsync(ExternalUser user, CancellationToken cancellationToken)
        {
            var result = await _userService.CreateAsync(user);
            return result.ExternalUserID > 0 ? IdentityResult.Success : IdentityResult.Failed();
        }

        public async Task<IdentityResult> UpdateAsync(ExternalUser user, CancellationToken cancellationToken)
        {
            //Update the user logic
            //var result = await UserService.PutUpdateAsync(user);
            //var result = await _userService.PutUpdateAsync(user);
            //return result ? IdentityResult.Success : IdentityResult.Failed();
            return await Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> DeleteAsync(ExternalUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ExternalUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var result = int.TryParse(userId, out _);
            return result ? _userService.FindByIdAsync(int.Parse(userId)) : null;
        }

        public async Task<ExternalUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var user = await _userService.FindByNameAsync(normalizedUserName);

            // User has been deleted or Lockout
            if (user == null) return null;

            if (user.LockoutEnabled)
                return null;
            user.Id = user.ExternalUserID.ToString();
            return user;
        }

        public Task SetEmailAsync(ExternalUser user, string email, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetEmailAsync(ExternalUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(ExternalUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(ExternalUser user, bool confirmed, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<ExternalUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userService.FindByNameAsync(normalizedEmail);
                user.Id = user.ExternalUserID.ToString();
                return user;
            }
            catch (Exception e) { Console.WriteLine(e); throw; }
        }

        public Task<string> GetNormalizedEmailAsync(ExternalUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedEmailAsync(ExternalUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.FromResult(0);
        }

        public Task SetPasswordHashAsync(ExternalUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(ExternalUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(ExternalUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public Task SetPhoneNumberAsync(ExternalUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPhoneNumberAsync(ExternalUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(ExternalUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetPhoneNumberConfirmedAsync(ExternalUser user, bool confirmed, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetTwoFactorEnabledAsync(ExternalUser user, bool enabled, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetTwoFactorEnabledAsync(ExternalUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task SetAuthenticatorKeyAsync(ExternalUser user, string key, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAuthenticatorKeyAsync(ExternalUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AddToRoleAsync(ExternalUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFromRoleAsync(ExternalUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<string>> GetRolesAsync(ExternalUser user, CancellationToken cancellationToken)
        {
            var result = await _roleService.GetRolesByUserId(user.ExternalUserID, "TritonGroup");
            var p = result.Select(x => x.RoleName).ToList();
            return p;
        }

        public Task<bool> IsInRoleAsync(ExternalUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<ExternalUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetSecurityStampAsync(ExternalUser user, string stamp, CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task<string> GetSecurityStampAsync(ExternalUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp ?? "AspNet.Identity.SecurityStamp");
        }
    }
}
