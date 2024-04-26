

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UNUMSelfPwdReset.Models
{
    public class UserProfileViewModel
    {
        public string Token { get; set; }
        public string Id { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Mail { get; set; }
        public string Department { get; set; }
        public string UserPrincipalName { get; set; }
        public string OnPremisesSamAccountName { get; set; }
        public DateTime LastSignInAt { get; set; }
        public DateTime? LastPasswordChangeDateTime { get; set; }
        public List<UserLoginClient> LoginClients { get; set; }

        public string lastSignInDateTimenew { get; set; }
    }




    public class UserLoginClient
    {
        public LoginClientType LoginType { get; set; }
        public string UserLoginId { get; set; }
        public DateTime? LastSignInAt { get; set; }
        public string LastSignInAtn { get; set; }
        public int? ExpireInDays { get; set; }
        public string Username { get; set; }
        public string Description { get; set; }

        public string OnPremisesSamAccountName { get; set; }
        public bool HasAccess { get; set; }


    }

    public enum LoginClientType
    {
        [Display(Name = "Lan Id")]
        [Description("LAN ID")]
        LAN ,
        RACF,
        ID,
        AzureAD

    }
    public class GenerateResponce
    {
        public string Message { get; set; }
        public string TempPassword { get; set; }

    }

    public class ResetPasswordRequest
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Username { get; set; }
        public LoginClientType AzureAD { get; set; }
        //public string OldPassword { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,22}$", ErrorMessage = "Password must be between 12 and 22 characters and contain one uppercase letter, one lowercase letter, one digit and one special character.")]
        [DataType(DataType.Password)]
        //[StringLength(255, ErrorMessage = "Must be between 5 and 255 characters", MinimumLength  = 5)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        //[StringLength(255, ErrorMessage = "Must be between 5 and 255 characters", MinimumLength = 5)]
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,22}$", ErrorMessage = "Password must be between 12 and 22 characters and contain one uppercase letter, one lowercase letter, one digit and one special character.")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Username { get; set; }
        public LoginClientType AzureAD { get; set; }

        [Required(ErrorMessage = "Old Password is required")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,22}$", ErrorMessage = "Password must be between 12 and 22 characters and contain one uppercase letter, one lowercase letter, one digit and one special character.")]
        [DataType(DataType.Password)]
        //[StringLength(255, ErrorMessage = "Must be between 5 and 255 characters", MinimumLength  = 5)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
    public class LastLoginViewModel
    {
        public string odatacontext { get; set; }
        public string displayName { get; set; }
        public string id { get; set; }
        public SigninActivityViewModel signInActivity { get; set; }
    }
    public class SigninActivityViewModel
    {
        public string lastSignInDateTime { get; set; }
        public string lastSignInRequestId { get; set; }
        public DateTime lastNonInteractiveSignInDateTime { get; set; }
        public string lastNonInteractiveSignInRequestId { get; set; }
    }
}
