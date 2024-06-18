# IS Notifikacije

Ovaj program razvijen je je za finalni projekat predmeta Cloud Infrastruktura i Servisi na Fakultetu Organizacionih nauka kao demonstracija dokerizacije aplikacija.

## O aplikaciji
Aplikacija **Microsoft Teams** postala je standard za komunikaciju između nastavnog tima i studenata na FON-u. Međutim, neki predmeti i dalje svoja obaveštenja šalju pretežno preko sajtova katedre. Ovaj program pokušava da premosti taj jaz kod Katedre za Informacione Sisteme.

**IS Notifikacije** omogućava korisnicima da se prijave za primanje notifikacija za predmete čije se vesti kaće na sajtu Katedre za Informacione Sisteme(*is.fon.bg.ac.rs*).



![1718665856957blob](https://github.com/bd0061/IS-Notifikacije/assets/74324902/93198faf-fb50-4f92-99f2-bb70fba6a4cb)


## Implementacija
Program funkcioniše na sledeći način:
- Korisnici unesu svoj studentski mejl na stranici kao i predmete za koje žele da primaju notifikacije.
- Korisnici nakon toga potvrde ovaj mejl kroz verifikaciju koju sajt automatski sprovodi
- Nakon uspešne konfirmacije, korisnici su oficijalno na mejling listi.
- Poseban program periodično proverava sajt katedre, i kada primi vest, šalje mejl studentima koji su se opredelili da primaju mejlove za spomenuti predmet.

## Pokretanje 
Na githubu je predstavljena aplikacija za development mod, ali je deployment ovakve aplikacije vrlo jednostavan uz minimalne promene (promene url-ova i rutiranje preko HTTPS)
Pre nego što se pokrene compose fajl, neophodno je podesiti nekoliko fajlova. U tome dolazi u pomoć **env_scaffold.ps1** za Windows, odnosno **env_scaffold.sh** za Linux sisteme. Ova skripta bi trebalo da generiše skeleton konfiguracionih fajlova neophodnih za pokretanje aplikacije: **env** folder i **postgres-data** folder. 

## Environment
Postgres-data folder se koristi kao **bind-mount** za *PostgreSQL* bazu koju aplikacija koristi, dok se u env folderu nalaze sledeća tri fajla:
- *secrets.env* - Parametri vezani sa mejl nalog koji šalje notifikacije i api ključeve
   - APP_PASSWORD - *Password koji se koristi za autentikaciju mejla*
   - API_KEY - *Kljuc koji se koristi za pristupanje obezbeđenim endpointima*
   - SMTP_SERVER - *Adresa smtp servera koji mejl koristi za slanje notifikacija*
   - SMTP_PORT - *Port na kom sluša SMPT server*
- *postgres.env* - Baza
   - POSTGRES_USER - *Username korisnika koji pristupa bazi*
   - POSTGRES_PASSWORD - *Sifra korisnika koji pristupa bazi*
   - POSTGRES_HOST- *Adresa db servera ukoliko postoji, ukoliko ne koristi se container*
   - POSTGRES_PORT- *Port na kom sluša db server*
   - POSTGRES_DBNAME- *Ime baze*

- *scraper_config.env* - Konfiguracija za python webscraper koji proverava sajt
  -  RETRY_INTERVAL=10 - *Koliko dugo čekamo nakon manjeg neuspeha(sekunde)*
  -  MAX_ATT_COUNT=10 - *Koliko puta ponovo pokusavamo konekciju u kratkim intervalima nakon neuspeha pre dugog timeouta*
  -  SLEEP_INTERVAL=0.5 - *Koliko cesto proveravamo stanje sajta katedre(minuti)*
  -  TESTING=1 - *Ako je 1, umesto fetchovanja HTML-a program cita html iz prosleđenog fajla kao simulacija*
  -  TEST_FILE=tests/articles.html

## Dokerizacija
Nakon podešavanja ovih promenljivih, potrebno je samo pokrenuti `docker compose up` i aplikacija će servirati *frontend* na portu 3000(publishovan), *backend* na portu 5001(publishovan), i bazu na portu default 5432 (na kontejneru,bez publishovanja)

Aplikacija koristi neoptimizovan dev server za react frontend servis sa bind mountom, a backend je konfigurisan samo za HTTP u ovom izdanju (Potrebno je lično potpisivanje SSL sertifikata kako bi HTTPS radio u dokeru) Bind mount je, osim podataka za postgres bazu, takođe prisutan i u python scraperu.

# Napomena 
Web scraping nosi legalne i etičke implikacije. Ovaj program je isključivo demonstrativnog karaktera. Web scraping, u bilo kojoj meri, je **zabranjen** bez eksplicitne dozvole web administratora.

*Credits: Site favicon by AmruID*
