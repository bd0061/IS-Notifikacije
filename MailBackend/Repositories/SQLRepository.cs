using MailBackend.Data;
using MailBackend.DTOs;
using MailBackend.Exceptions;
using MailBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Xml.Linq;
using MailBackend.Email;

namespace MailBackend.Repositories
{
    public class SQLRepository : IRepository
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<SQLRepository> _logger;
        public SQLRepository(AppDbContext context, IEmailService emailservice, ILogger<SQLRepository> logger)
        {
            _context = context;
            _emailService = emailservice;
            _logger = logger;
        }

        public async Task<List<KursDTO>> vratiKurseve()
        {
            return await _context.Kursevi.Select(k => new KursDTO { Id = k.Id, Sifra = k.Sifra, PunoIme = k.PunoIme})
                .ToListAsync();
        }
        public async Task<List<StudentDTO>> vratiMejloveStudenataKojiImajuSifre(List<string> kursSifre)
        {
            if(kursSifre == null || kursSifre.Count == 0)
            {
                throw new BadCoursesException("Loš zahtev.");
            }
            foreach (var s in kursSifre)
            {
                if (await _context.Kursevi.FirstOrDefaultAsync(ss => ss.Sifra == s) == null)
                {
                    throw new BadCoursesException("Loš zahtev.");
                }
            }
            var query = from student in _context.Studenti
                        join studentKurs in _context.StudentKursevi on student.Id equals studentKurs.StudentId
                        join kurs in _context.Kursevi on studentKurs.KursId equals kurs.Id
                        where kursSifre.Contains(kurs.Sifra)
                        select new StudentDTO { Email = student.Email, Unsubtoken = student.UnsubToken };
            return await query.ToListAsync();
        }

        public async Task dodajMejl(string Mejl, List<string> sifre)
        {
            string t = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
            _logger.Log(LogLevel.Information, $"Pristigao je novi zahtev za dodavanje (dodeljena interna sifra:{t})");
            if(sifre == null || sifre.Count == 0)
            {
                _logger.Log(LogLevel.Error, $"Zahtev odbijen, los niz kurseva(interna sifra: {t})");
                throw new BadCoursesException("Loš zahtev.");
            }
            if(string.IsNullOrEmpty(Mejl))
            {
                _logger.Log(LogLevel.Error, $"Zahtev odbijen, prazan mejl(interna sifra: {t})");
                throw new EmptyMailException("Polje za mejl ne sme biti prazno.");
            }

            try
            {
                var mailCheck = new MailAddress(Mejl);
            }
            catch (FormatException)
            {
                _logger.Log(LogLevel.Error, $"Zahtev odbijen, los mejl(interna sifra: {t})");
                throw new InvalidMailFormatException("Unešena adresa nije validna imejl adresa.");
            }

            if (!Mejl.EndsWith("@student.fon.bg.ac.rs"))
            {
                _logger.Log(LogLevel.Error, $"Zahtev odbijen, nije studentski mejl(interna sifra: {t})");
                throw new NotStudentMailException("Molimo vas unesite Vaš studentski mejl.");
            }
            foreach (string sifra in sifre)
            {
                if (!_context.Kursevi.Select(k => k.Sifra).Contains(sifra))
                {
                    _logger.Log(LogLevel.Error, $"Zahtev odbijen, nepostojeca sifra kursa (interna sifra: {t})");
                    throw new BadCoursesException("Loš zahtev.");
                }
                    
            }

            //========================================== KRAJ VALIDACIJE, POCETAK LOGIKE==========================================//
            Student noviStudent = null;
            var s = await _context.Studenti.FirstOrDefaultAsync(s => s.Email == Mejl);
            if (s == null)
            {
                noviStudent = new Student {
                    Email = Mejl,
                    SubToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)),
                    SubTokenExpires = DateTime.UtcNow.AddHours(2),
                    RegisteredDate = DateTime.UtcNow,
                    isVerified = false,
                    UnsubToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(256))
                };
                _context.Studenti.Add(noviStudent);
            }
            else if((s.SubToken != null && s.SubTokenExpires < DateTime.UtcNow) || s.SubToken == null)
            {
                //Ako je istekao trenutni token, ili ako je korisnik vec verifikovan pa mu je
                //token invalidiran a zeli da promeni izbor, generisi mu novi token, u suprotnom ostavljamo isti
                s.SubToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
                s.SubTokenExpires = DateTime.UtcNow.AddHours(2);
            }


            var punaImena = await _context.Kursevi.Where(k => sifre.Contains(k.Sifra)).Select(k => k.PunoIme).ToListAsync();
            string body = "<p>Poštovani,</p><p>Vaš je mejl je prijavljen za primanje notifikacija/promenu opcija za vesti o sledećim predmetima:</p><br>";
            body += "<ul>";
            foreach (var ime in punaImena)
            {
                body += $"<li>{ime}</li>";
            }
            body += "</ul><br>";
            string verifylink = $"http://localhost:3000/verify?token={(noviStudent != null ? noviStudent.SubToken : s.SubToken)}";
            foreach(var sfr in sifre)
            {
                verifylink += $"&sifre={sfr}";
            }
            body += $"<p>Vaš link za potvrdu se nalazi <a href = \"{verifylink}\" >OVDE.</a></p><p><i>Ukoliko se niste prijavili za ove notifikacije, obrišite ovaj mejl.</i></p>";

            try
            {
                _emailService.SendEmail(Mejl, "Potvrda o primanju notifikacija za IS", body);
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Zahtev odbijen, smtp server greska (limit dostignut?) : [ {ex.Message} ](interna sifra: {t})");
                throw new InvalidMailFormatException("Molimo pokušajte kasnije.");
            }
            
            
            if (s != default)
                _context.Studenti.Update(s);
            await _context.SaveChangesAsync();
            _logger.Log(LogLevel.Information, $"Zahtev prihvacen (interna sifra: {t} - dodeljen token: {(s != null ? s.SubToken : noviStudent.SubToken)})");
        }
        public async Task Verify(string token, List<string> sifre)
        {
            if(sifre == null || sifre.Count == 0 || sifre.Count > _context.Kursevi.Count())
            {
                _logger.Log(LogLevel.Error, $"Verifikacija odbijena, losi kursevi (token: {token})");
                throw new InvalidTokenException("Ovaj link je nevažeći. Probajte ponovo.");
            }
            var s = await _context.Studenti.FirstOrDefaultAsync(st => st.SubToken != null && st.SubToken.Equals(token));
            if (s == null || s.SubTokenExpires < DateTime.UtcNow)
            {
                _logger.Log(LogLevel.Error, $"Verifikacija odbijena, koriscen token {(s == null ? "ne postoji" : "je istekao")} (token: {token})");
                throw new InvalidTokenException("Ovaj link je nevažeći. Probajte ponovo.");
            }
            
            //Ne loopuj beznacajno jer ako nije verifikovan sigurno nema rekorda u StudentKurs
            if(s.isVerified)
            {
                _context.StudentKursevi.RemoveRange(_context.StudentKursevi.Where(sk => sk.StudentId == s.Id));
            }

            HashSet<string> seenElements = new HashSet<string>();
            foreach (string sif in sifre)
            {
                if (!seenElements.Add(sif.ToLower()))
                {
                    _logger.Log(LogLevel.Error, $"Verifikacija odbijena, duplikati kurseva (token: {token})");
                    throw new InvalidTokenException("Ovaj link je nevažeći. Probajte ponovo.");
                }
                var k = _context.Kursevi.FirstOrDefault(kurs => kurs.Sifra == sif.ToLower());
                if(k == null)
                {
                    _logger.Log(LogLevel.Error, $"Verifikacija odbijena, lose formiran niz kurseva?? (token: {token})");
                    throw new InvalidTokenException("Ovaj link je nevažeći. Probajte ponovo.");
                }
                await _context.StudentKursevi.AddAsync(new StudentKurs { StudentId = s.Id, KursId = k.Id });
            }
            

            if (!s.isVerified)
            {
                s.isVerified = true;
                s.VerifiedDate = DateTime.UtcNow;
				try
				{
					string unsublink = $"http://localhost:3000/unsubscribe?token={s.UnsubToken}";
					string text = "<p>Uspešno ste primljeni na listu za notifikacije za predmete katedre za IS</p>" +
					$"Ukoliko želite da poništite ovu odluku ili ukoliko se niste Vi prijavili, kliknite <a href = \"{unsublink}\">OVDE.</a></p>";

					
					_emailService.SendEmail(s.Email, "Dobrodošli na mailing listu!", text);
				}
				catch (Exception ex)
				{
					_logger.Log(LogLevel.Error, $"Neuspelo slanje mejla dobrodošlice. Smtp limit? (unsubtoken = {s.UnsubToken})");
				}
            }
			else 
			{
				try
				{
					string unsublink = $"http://localhost:3000/unsubscribe?token={s.UnsubToken}";
					string text = "<p>Poštovani,</p><p>Uspešno ste ažurirali vaše opcije za primanje notifikacija.</p>" +
					$"Ukoliko želite da se odjavite, kliknite <a href = \"{unsublink}\">OVDE.</a></p>";

					
					_emailService.SendEmail(s.Email, "Ažurirane opcije", text);
				}
				catch (Exception ex)
				{
					_logger.Log(LogLevel.Error, $"Neuspelo slanje mejla azuriranja. Smtp limit? (unsubtoken = {s.UnsubToken})");
				}	
				
			}
            _logger.Log(LogLevel.Information, $"Verifikacija uspesna (token: {token})");

            s.SubToken = null;
            s.SubTokenExpires = null;
            _context.Studenti.Update(s);
            await _context.SaveChangesAsync();  
        }

        public async Task Obrisi(string token)
        {
            var s = await _context.Studenti.FirstOrDefaultAsync(st => st.UnsubToken.Equals(token));
            if(s == null)
            {
                _logger.Log(LogLevel.Error, $"Brisanje odbijeno, neispravan link (unsubtoken: {token})");
                throw new InvalidTokenException("Ovaj link je nevažeći. Probajte ponovo.");
            }
            try
            {
                string text = "<p>Uspešno ste odjavljeni sa liste. Doviđenja!</p>";
                _emailService.SendEmail(s.Email, "Odjava sa liste za IS Notifikacije", text);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Neuspelo slanje mejla za unsub. Smtp limit? (unsubtoken = {s.UnsubToken})");
            }
            _logger.Log(LogLevel.Information, $"Brisanje uspesno (unsubtoken: {token})");
            _context.Studenti.Remove(s);
            await _context.SaveChangesAsync();
        }

    }
}
