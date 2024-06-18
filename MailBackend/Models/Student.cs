using System.ComponentModel.DataAnnotations.Schema;

namespace MailBackend.Models
{
    public class Student
    {
        public int Id { get; set; } 
        public string Email { get; set; }
        public string? SubToken { get; set; }
        public DateTime? SubTokenExpires { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public DateTime RegisteredDate { get; set; } 
        public string UnsubToken { get; set; }
        public bool isVerified { get; set; }
        public ICollection<StudentKurs> StudentKursevi { get; set; }
    }
}
