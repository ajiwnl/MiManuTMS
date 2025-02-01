using System.ComponentModel.DataAnnotations;

namespace TMS.ViewModels
{
    public class EditProfileViewModel
    {
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required]
        public string CurrentEmail { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        public IFormFile UserImgFile { get; set; }

        public string UserImgUrl { get; set; }

    }
}
