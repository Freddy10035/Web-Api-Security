using System.ComponentModel;
using System.Linq;
using System.Text;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;

namespace FreeWebApiSecurity.WebApi.BusinessObjects
{
    [MapInheritance(MapInheritanceType.ParentTable)]
    [DefaultProperty(nameof(UserName))]
    public class ApplicationUser : PermissionPolicyUser, ISecurityUserWithLoginInfo
    {
        public ApplicationUser(Session session) : base(session) { }

        [Browsable(false)]
        [Aggregated, Association("User-LoginInfo")]
        public XPCollection<ApplicationUserLoginInfo> LoginInfo
        {
            get { return GetCollection<ApplicationUserLoginInfo>(nameof(LoginInfo)); }
        }

        IEnumerable<ISecurityUserLoginInfo> IOAuthSecurityUser.UserLogins => LoginInfo.OfType<ISecurityUserLoginInfo>();

        ISecurityUserLoginInfo ISecurityUserWithLoginInfo.CreateUserLoginInfo(string loginProviderName, string providerUserKey)
        {
            ApplicationUserLoginInfo result = new ApplicationUserLoginInfo(Session);
            result.LoginProviderName = loginProviderName;
            result.ProviderUserKey = providerUserKey;
            result.User = this;
            return result;
        }
    }

    [Persistent("Post")]
    public class Post : BaseObject, IObjectSpaceLink
    {
        public Post(Session session) : base(session) { }

        [Persistent("PostId")]
        public int PostId { get; set; }

        [Size(100)]
        public string Title { get; set; }

        [Size(SizeAttribute.Unlimited)]
        public string Content { get; set; }

        //[Association("ApplicationUser-Post")]
        public ApplicationUser Author { get; set; }

        IObjectSpace IObjectSpaceLink.ObjectSpace { get; set; }


        public override void AfterConstruction()
        {
            base.AfterConstruction();

            if (Session.IsNewObject(this))
            {
                ApplicationUser currentUser = Session.GetObjectByKey<ApplicationUser>(SecuritySystem.CurrentUserId);
                if (currentUser != null)
                {
                    Author = currentUser;
                }
            }
        }


        protected override void OnSaving()
        {
            base.OnSaving();

            // perform custom logic before saving the object
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            // perform custom logic after loading the object
        }
    }
}

