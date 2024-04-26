using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UNUMSelfPwdReset.Models;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using UNUMSelfPwdReset.Utilities;
using Azure;
using UNUMSelfPwdReset.Managers;
using System.Security.Cryptography;
using NuGet.Common;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;

namespace UNUMSelfPwdReset.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly PasswordResetService _passwordResetService;
        private readonly AzureAdminActionManager _azureAdminActionManager;
        private readonly IConfiguration _config;

        private readonly LoginsManager _loginsManager;

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger,
            GraphServiceClient graphServiceClient, LoginsManager loginsManager
            , PasswordResetService passwordResetService, AzureAdminActionManager azureAdminActionManager
           , IConfiguration config)
        {
            _logger = logger;
            _graphServiceClient = graphServiceClient;
            _passwordResetService = passwordResetService;
            _loginsManager = loginsManager;
            _azureAdminActionManager = azureAdminActionManager;
            _config = config;
        }

        [AuthorizeForScopes(ScopeKeySection = "MicrosoftGraph:Scopes")]



        #region For My Accounts Page
        public async Task<IActionResult> Index()
        {
            var me = await _graphServiceClient.Me.Request().GetAsync();
            string accessToken = AdminToken.Token;
            
            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = await _azureAdminActionManager.GetAdminTokenForGraph();
            }
            else
            {
                var jwtToken = new JwtSecurityToken(accessToken);
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    accessToken = await _azureAdminActionManager.GetAdminTokenForGraph();
                }
            }
            
            // string accessToken = await _azureAdminActionManager.GetAdminTokenForGraph();
            var userInfo = CopyHandler.UserProperty(me);
            try
            {
                var Error = " User details by Admin";
                var user = await _graphServiceClient.Users[me.UserPrincipalName]
                   .Request()
                   .Select(x => new { x.LastPasswordChangeDateTime, x.OnPremisesSamAccountName, x.PasswordProfile })
                   .GetAsync();

                userInfo.LastPasswordChangeDateTime = user?.LastPasswordChangeDateTime?.DateTime;
                userInfo.OnPremisesSamAccountName = user?.OnPremisesSamAccountName;

                #region to get User Last Login date time
                Error = "call graph api for signInActivity";
                string lastSignInDateTime = user?.LastPasswordChangeDateTime?.DateTime.ToString();
                try
                {
                    dynamic result = await CallGraphApi(accessToken, me.Id);
                    LastLoginViewModel model = new LastLoginViewModel();
                    Error = "Deserialize the signInActivity model";
                    if (result != null)
                    {
                        model = JsonSerializer.Deserialize<LastLoginViewModel>(result);
                        if (model.signInActivity != null && model.signInActivity.lastSignInDateTime != null)
                        {
                            lastSignInDateTime = model.signInActivity.lastSignInDateTime;
                        }

                    }
                }
                catch (Exception ex)
                {

                    TempData.SetObjectAsJson("PopupViewModel", StaticMethods.CreatePopupModel("Home", ex.Message + "--" + Error));
                }
                // to convert the date time to local datetime
                //DateTime univDateTime = DateTime.Parse(model.signInActivity.lastSignInDateTime.ToString());
                //DateTime localDateTime = univDateTime.ToLocalTime();
                #endregion
                Error = "call login clients send the values";
                userInfo.LoginClients = await _loginsManager.GetUserLogins(userInfo?.Id, userInfo?.UserPrincipalName, Convert.ToDateTime(userInfo?.LastPasswordChangeDateTime), user?.OnPremisesSamAccountName, lastSignInDateTime);
                string strProfilePicBase64 = "";
                try
                {
                    var profilePic = await _graphServiceClient.Me.Photo.Content.Request().GetAsync();
                    using StreamReader? reader = profilePic is null ? null : new StreamReader(new CryptoStream(profilePic, new ToBase64Transform(), CryptoStreamMode.Read));
                    strProfilePicBase64 = reader is null ? null : await reader.ReadToEndAsync();
                }
                catch (Exception ex)
                {

                    strProfilePicBase64 = "";
                }
                if (userInfo.GivenName != null)
                {
                    HttpContext.Session.SetString("FirstName", userInfo.GivenName?.ToString());
                }
                if (strProfilePicBase64 != null)
                {
                    HttpContext.Session.SetString("Profilepic", strProfilePicBase64.ToString());
                }
                if (userInfo.Surname != null)
                {
                    HttpContext.Session.SetString("LastName", userInfo.Surname?.ToString());
                }
                if (user.PasswordProfile != null)
                {
                    if (user.PasswordProfile.ForceChangePasswordNextSignIn == true)
                    {
                        userInfo.LoginClients[0].ExpireInDays = 0;
                    }
                }
                int? days = userInfo.LoginClients[0].ExpireInDays;
                // return View(userInfo);
            }
            catch (Exception ex)
            {

                TempData.SetObjectAsJson("PopupViewModel", StaticMethods.CreatePopupModel("Home", ex.Message + "--" + Error));
            }
            return View(userInfo);
        }

        #endregion

        #region Generate Password
        [HttpGet]
        public async Task<IActionResult> GeneratePassword(string Id)
        {
            GenerateResponce model = new GenerateResponce();
            string Error = "";
            try
            {
                string clientId = Environment.GetEnvironmentVariable("ADMIN_USERNAME");
                string ADMIN_PWD = Environment.GetEnvironmentVariable("ADMIN_PWD");
                Error = " Admin token";
                string accessToken = AdminToken.Token;
                if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = await _azureAdminActionManager.GetAdminTokenForGraph();
                }
                else
                {
                    var jwtToken = new JwtSecurityToken(accessToken);
                    if (jwtToken.ValidTo < DateTime.UtcNow)
                    {
                        accessToken = await _azureAdminActionManager.GetAdminTokenForGraph();
                    }
                }
                // string token = await _azureAdminActionManager.GetAdminTokenForGraph();

                Error = " Password Generator";
                string tempPassword = GenerateRandomStrongPassword(12);
                //string tempPassword = "Ags@2023";
                ResetPasswordRequest temp = new ResetPasswordRequest();
                temp.Id = Id; temp.NewPassword = tempPassword;
                model.TempPassword = tempPassword;

                Error = " Reset Password";
                var response = await _passwordResetService.ResetUserPasswordAsync(accessToken, temp);
                //    var response = "true";
                if (response == "true")
                {

                    // TempData.SetObjectAsJson("PopupViewModel", StaticMethods.CreatePopupModel("Home", "Password Changed Successfully !"));
                    return View(model);
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", StaticMethods.CreatePopupModel("Home", response + " After Reset Password Method "));
                    return RedirectToAction("Index");
                }


            }
            catch (Exception ex)
            {
                TempData.SetObjectAsJson("PopupViewModel", StaticMethods.CreatePopupModel("Home", ex.Message + Error));
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region error Page
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion

        #region Passwod genarator
        static string alphaCaps = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        static string alphaLow = "abcdefghijklmnopqrstuvwxyz";
        static string numerics = "1234567890";
        static string special = "~!@#$%^&*()";//@#$&*~?
        string allChars = alphaCaps + alphaLow + numerics + special;
        Random r = new Random();

        public string GenerateRandomStrongPassword(int length = 12)
        {
            String generatedPassword = "";
            try
            {

                if (length < 4)
                    throw new Exception("Number of characters should be greater than 4.");
                int lowerpass, upperpass, numpass, specialchar;
                string posarray = "0123456789";
                if (length < posarray.Length)
                    posarray = posarray.Substring(0, length);
                lowerpass = getRandomPosition(ref posarray);
                upperpass = getRandomPosition(ref posarray);
                numpass = getRandomPosition(ref posarray);
                specialchar = getRandomPosition(ref posarray);


                for (int i = 0; i < length; i++)
                {
                    if (i == lowerpass)
                        generatedPassword += getRandomChar(alphaCaps);
                    else if (i == upperpass)
                        generatedPassword += getRandomChar(alphaLow);
                    else if (i == numpass)
                        generatedPassword += getRandomChar(numerics);
                    else if (i == specialchar)
                        generatedPassword += getRandomChar(special);
                    else
                        generatedPassword += getRandomChar(allChars);
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
            return generatedPassword;
        }

        public string getRandomChar(string fullString)
        {
            return fullString.ToCharArray()[(int)Math.Floor(r.NextDouble() * fullString.Length)].ToString();
        }

        public int getRandomPosition(ref string posArray)
        {
            int pos;
            string randomChar = posArray.ToCharArray()[(int)Math.Floor(r.NextDouble() * posArray.Length)].ToString();
            pos = int.Parse(randomChar);
            posArray = posArray.Replace(randomChar, "");
            return pos;
        }
        #endregion

        #region call graph API for Last Login Date time
        public async Task<string> CallGraphApi(string accessToken, string UserId)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = "https://graph.microsoft.com/v1.0/users/" + UserId + "?$select=signInActivity";
                    // Set the request headers, including the Authorization header with the access token
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    // Make the GET request to the Microsoft Graph API
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    // Check if the request was successful (status code 200-299)
                    if (response.IsSuccessStatusCode)
                    {
                        // Read and return the response content
                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        // If the request was not successful, throw an exception with the error message
                        // throw new HttpRequestException($"Graph API request failed with status code {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                        TempData.SetObjectAsJson("PopupViewModel", StaticMethods.CreatePopupModel("Home", response.StatusCode + await response.Content.ReadAsStringAsync()));
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                TempData.SetObjectAsJson("PopupViewModel", StaticMethods.CreatePopupModel("Home", ex.Message));
                return null;
            }
        }
        #endregion

    }
}