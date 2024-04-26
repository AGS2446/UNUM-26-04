using Microsoft.Identity.Web;
using UNUMSelfPwdReset.Models;

namespace UNUMSelfPwdReset
{
    public class LoginsManager
    {
        private readonly IConfiguration _config;
        public LoginsManager(IConfiguration config)
        {
            _config = config;
        }
        #region Service for Append the USer Values 
        public async Task<List<UserLoginClient>> GetUserLogins(string userId, string Username, DateTime? pwdChangedOn, string lanID, string sss)
        {
            string days = _config.GetValue<string>("Localinstants:days");
            List<UserLoginClient> loginClients = new List<UserLoginClient>() {
                new UserLoginClient() {
                    LoginType = LoginClientType.LAN
                    , UserLoginId= userId
                    ,Username= Username
                    ,LastSignInAt=pwdChangedOn
                    , HasAccess= true,
                    LastSignInAtn=sss
                    , OnPremisesSamAccountName= lanID,
                    Description="ID used to login to the UNUM network.",
                     ExpireInDays= pwdChangedOn.HasValue ? Convert.ToInt16((pwdChangedOn.Value.AddDays(Convert.ToInt32(days)) - DateTime.Now).TotalDays ): null,
            }

            };

            return loginClients;
        }
        #endregion
    }
}
