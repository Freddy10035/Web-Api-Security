using System.Security.Claims;
using System.Security.Principal;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using FreeWebApiSecurity.WebApi.BusinessObjects;

namespace FreeWebApiSecurity.WebApi.Core
{

    public class CustomAuthenticationProvider : IAuthenticationProviderV2 {
        private readonly IPrincipalProvider principalProvider;

        public CustomAuthenticationProvider(IPrincipalProvider principalProvider) {
            this.principalProvider = principalProvider;
        }

        public object Authenticate(IObjectSpace objectSpace) {
            /// <summary>
            // When a user successfully logs in with an OAuth provider, you can get their unique user key.
            // The following code finds an ApplicationUser object associated with this key.
            // This code also creates a new ApplicationUser object for this key automatically.
            // For more information, see the following topic: https://docs.devexpress.com/eXpressAppFramework/402197
            // If this behavior meets your requirements, comment out the line below.
            /// </summary>
            return null;

            if(!CanHandlePrincipal(principalProvider.User)) {
                return null;
            }

            const bool autoCreateUser = true;

            ClaimsPrincipal claimsPrincipal = (ClaimsPrincipal)principalProvider.User;
            var userIdClaim = claimsPrincipal.FindFirst("sub") ?? claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Unknown user id");

            var providerUserKey = userIdClaim.Value;
            var loginProviderName = claimsPrincipal.Identity.AuthenticationType;
            var userName = claimsPrincipal.Identity.Name;

            var userLoginInfo = FindUserLoginInfo(objectSpace, loginProviderName, providerUserKey);
            if(userLoginInfo != null) {
                return userLoginInfo.User;
            }

            if(autoCreateUser) {
                return CreateApplicationUser(objectSpace, userName, loginProviderName, providerUserKey);
            }

            return null;
        }

        private bool CanHandlePrincipal(IPrincipal user) {
            return user.Identity.IsAuthenticated &&
                   user.Identity.AuthenticationType != SecurityDefaults.Issuer &&
                   user.Identity.AuthenticationType != SecurityDefaults.PasswordAuthentication &&
                   user.Identity.AuthenticationType != SecurityDefaults.WindowsAuthentication &&
                   !(user is WindowsPrincipal);
        }

        private object CreateApplicationUser(IObjectSpace objectSpace, string userName, string loginProviderName, string providerUserKey) {
            if(objectSpace.FirstOrDefault<ApplicationUser>(user => user.UserName == userName) != null) {
                throw new ArgumentException($"The username ('{userName}') was already registered within the system");
            }

            var user = objectSpace.CreateObject<ApplicationUser>();
            user.UserName = userName;
            user.SetPassword(Guid.NewGuid().ToString());
            user.Roles.Add(objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Default"));
            ((ISecurityUserWithLoginInfo)user).CreateUserLoginInfo(loginProviderName, providerUserKey);
            objectSpace.CommitChanges();
            return user;
        }

        private ISecurityUserLoginInfo FindUserLoginInfo(IObjectSpace objectSpace, string loginProviderName, string providerUserKey) {
            return objectSpace.FirstOrDefault<ApplicationUserLoginInfo>(userLoginInfo =>
                                userLoginInfo.LoginProviderName == loginProviderName &&
                                userLoginInfo.ProviderUserKey == providerUserKey);
        }
    }
}
