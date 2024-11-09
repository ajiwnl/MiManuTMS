using System.ComponentModel.DataAnnotations;

namespace TMS.ViewModels
{
    public class LoginViewModel
    {

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string EmailAdd { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
