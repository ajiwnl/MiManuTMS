using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;

namespace TMS.Models
{
    [FirestoreData]

    public class mAuth
    {
        [FirestoreProperty]
        public string UserRole { get; set; }
        [FirestoreProperty]
        public string Username { get; set; }
        [FirestoreProperty]
        public string Email { get; set; }
        public string Password { get; set; }
        [FirestoreProperty]
        public string FirstName { get; set; }
        [FirestoreProperty]
        public string LastName { get; set; }
        [FirestoreProperty]
        public string UID { get; set; }

        [FirestoreProperty]
        public string UserImg { get; set; }

    }
}
