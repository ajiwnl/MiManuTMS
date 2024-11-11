using Google.Cloud.Firestore;

namespace TMS.Models
{
    [FirestoreData] // This attribute tells Firestore to serialize this class
    public class mUserProfile
    {
        [FirestoreProperty]
        public string Uid { get; set; }

        [FirestoreProperty]
        public string fName { get; set; } = null;

        [FirestoreProperty]
        public string lName { get; set; } = null;

        [FirestoreProperty]
        public string mName { get; set; } = null;

        [FirestoreProperty]
        public string emailAdd { get; set; }

        [FirestoreProperty]
        public string profDisplay { get; set; } = "https://static.vecteezy.com/system/resources/thumbnails/009/292/244/small/default-avatar-icon-of-social-media-user-vector.jpg";

        [FirestoreProperty]
        public string userName { get; set; } = null;

        [FirestoreProperty]
        public string userType { get; set; } = null;
    }
}
