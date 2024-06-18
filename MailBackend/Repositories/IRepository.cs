using MailBackend.DTOs;
using MailBackend.Models;

namespace MailBackend.Repositories
{
    public interface IRepository
    {
        Task<List<KursDTO>> vratiKurseve();
        Task dodajMejl(string Mejl, List<string> sifre);

        Task<List<StudentDTO>> vratiMejloveStudenataKojiImajuSifre(List<string> kursSifre);

        Task Verify(string token, List<string> sifre);

        Task Obrisi(string token);



    }
}
