using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;
using UNUMSelfPwdReset.Models;

namespace UNUMSelfPwdReset.Managers
{
    public class PasswordResetService
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IConfiguration _config;
        public PasswordResetService(ITokenAcquisition tokenAcquisition, IConfiguration config)
        {
            _tokenAcquisition = tokenAcquisition;
            _config = config;
        }
        #region Service for rest Password the User

        //  public async Task<string> ResetUserPasswordAsync(string token, ResetPasswordRequest resetPasswordRequest)
        public async Task<string> ResetUserPasswordAsync(string token, ResetPasswordRequest resetPasswordRequest)
        {
            try
            {
                string autoken = _config.GetValue<string>("Localinstants:autoken");
                var graphServiceClient = GetAdminGraphServiceClient(token);


                var result = await graphServiceClient.Users[resetPasswordRequest.Id].Authentication.Methods[autoken]
                    .ResetPassword(resetPasswordRequest.NewPassword)
                    .Request()
                    .PostAsync();

                return "true";
            }
            catch (ServiceException ex)
            {

                return ex.Message;

            }
        }
        #endregion

        #region Service For Admin token
        private GraphServiceClient GetAdminGraphServiceClient(string AdminToken)
        {
            string graphEndpoint = "https://graph.microsoft.com/v1.0";
            var graphClient = new GraphServiceClient(graphEndpoint, new DelegateAuthenticationProvider(async request =>
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);
            }));

            return graphClient;
        }
        #endregion

    }
}
