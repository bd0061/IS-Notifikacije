namespace MailBackend.Models
{
    public class StudentKurs
    {
        public int StudentId { get; set; }
        public int KursId { get; set; }
        public Student Student { get; set; }
        public Kurs Kurs { get; set; }
    }
}
