using System.ComponentModel.DataAnnotations;

namespace TMS.ViewModels
{
    public class EditProfileViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserImg { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; } // Optional field for updating password
    }

}
