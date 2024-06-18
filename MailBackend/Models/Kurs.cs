namespace MailBackend.Models
{
    public class Kurs
    {
        public int Id { get; set; }
        public string Sifra { get; set; }
        public string PunoIme { get; set; }

        public ICollection<StudentKurs> StudentKursevi { get; set; }
    }
}
