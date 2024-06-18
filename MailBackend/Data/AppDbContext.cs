using MailBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace MailBackend.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Student> Studenti { get; set; }
        public DbSet<Kurs> Kursevi { get; set; }
        public DbSet<StudentKurs> StudentKursevi { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>()
                .Property(s => s.RegisteredDate)
                .HasColumnType("timestamp with time zone");
            modelBuilder.Entity<Student>()
                .Property(s => s.SubTokenExpires)
                .HasColumnType("timestamp with time zone");
            modelBuilder.Entity<Student>()
                .Property(s => s.VerifiedDate)
                .HasColumnType("timestamp with time zone");
            modelBuilder.Entity<StudentKurs>()
                .HasKey(sc => new { sc.StudentId, sc.KursId });
            
            modelBuilder.Entity<StudentKurs>()
                .HasOne(sc => sc.Student)
                .WithMany(s => s.StudentKursevi)
                .HasForeignKey(sc => sc.StudentId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<StudentKurs>()
                .HasOne(sc => sc.Kurs)
                .WithMany(c => c.StudentKursevi)
                .HasForeignKey(sc => sc.KursId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Kurs>().HasData(
                new Kurs { Id = 1, Sifra = "db", PunoIme = "Baze podataka"},
                new Kurs { Id = 2, Sifra = "db2", PunoIme = "Baze podataka 2" },
                new Kurs { Id = 3, Sifra = "db3", PunoIme = "Baze podataka 3" },
                new Kurs { Id = 4, Sifra = "pj", PunoIme = "Programski jezici" },
                new Kurs { Id = 5, Sifra = "uis", PunoIme = "Uvod u informacione sisteme" },
                new Kurs { Id = 6, Sifra = "joris", PunoIme = "Jezici i okruzenja za razvoj IS" },
                new Kurs { Id = 7, Sifra = "mpp", PunoIme = "Modelovanje poslovnih procesa" },
                new Kurs { Id = 8, Sifra = "ailp", PunoIme = "Analiza i logicko projektovanje IS" },
                new Kurs { Id = 9, Sifra = "poslis", PunoIme = "Poslovni informacioni sistemi" },
                new Kurs { Id = 10, Sifra = "pis", PunoIme = "Projektovanje informacionih sistema" },
                new Kurs { Id = 11, Sifra = "spa", PunoIme = "Strukture podataka i algoritmi" },
                new Kurs { Id = 12, Sifra = "fpis", PunoIme = "Fizicko projektovanje informacionih sistema" },
                new Kurs { Id = 13, Sifra = "abp", PunoIme = "Administracija baze podataka" },
                new Kurs { Id = 14, Sifra = "isr", PunoIme = "Integrisana softverska resenja" },
                new Kurs { Id = 15, Sifra = "isuz", PunoIme = "Informacioni sistemi za upravljanje znanjem" },
                new Kurs { Id = 16, Sifra = "isitm", PunoIme = "ISiT Menadzment" },
                new Kurs { Id = 17, Sifra = "uris", PunoIme = "Upravljanje razvojem informacionih sistema" },
                new Kurs { Id = 18, Sifra = "prev", PunoIme = "Programski prevodioci" }
                );
        }
    }
}
