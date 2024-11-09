using System.ComponentModel.DataAnnotations;

namespace TMS.Models
{
    public class mUserProfile
    {
        public string Uid { get; set; }
        public string fName { get; set; } = null;

        public string lName { get; set; } = null;

        public string mName { get; set; } = null;

        public string profDisplay { get; set; } = "https://static.vecteezy.com/system/resources/thumbnails/009/292/244/small/default-avatar-icon-of-social-media-user-vector.jpg";

        public string userName { get; set; } = null;

        public string userType { get; set; } = null;
    }
}
