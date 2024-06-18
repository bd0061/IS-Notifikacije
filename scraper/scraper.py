import time
import requests
import logging
import smtplib
import os
import socket
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText
from bs4 import BeautifulSoup

def main():
    old_articles = None
    retry_interval = int(os.getenv("RETRY_INTERVAL"))
    att_count = 0
    max_att_count = int(os.getenv("MAX_ATT_COUNT"))
    sleep_interval = float(os.getenv("SLEEP_INTERVAL"))
    TESTING = os.getenv("TESTING")
    TEST_FILE = os.getenv("TEST_FILE")
    
    def povuci_studente(sifre):
        url = 'http://mailbackend:5001/api/MejlPoSiframa'
        params = {'kursSifre': sifre}
        headers = {
            'Accept': 'text/plain',
            'X-API-KEY': os.getenv("API_KEY")
        }
        response = requests.get(url, params=params, headers=headers, verify=False)
        if response.status_code == 200:
            return response.json()
        else:
            raise BaseException
    
    def posalji_mejl(primaoci, naslov, telo):
        server = smtplib.SMTP(os.getenv("SMTP_SERVER"), int(os.getenv("SMTP_PORT")))
        server.starttls() 
        server.login(os.getenv("MAIL_NAME"), os.getenv("APP_PASSWORD"))
        message = MIMEMultipart()
        message['From'] = os.getenv("MAIL_NAME")
        message['To'] = primaoci[0]
        message['Subject'] = naslov

        body = telo
        message.attach(MIMEText(body, 'html'))

        text = message.as_string()
        server.sendmail(os.getenv("MAIL_NAME"), primaoci, text)

        server.quit()

    def odredi_kategorije(s):
        res = []
        sl = s.lower()


        if "програмски језици" in sl and "pj" not in res:
            res.append("pj")
        if "базе података 2" in sl or "бп2" in sl and "bp2" not in res:
            res.append("db2")  
        if "базе података 3" in sl or "бп3" in sl and "bp3" not in res:
            res.append("db3")
        #moramo da se postaramo da ispravno matchujemo predmet baze podataka kada su u obavestenju prisutni i baze podataka 2 ili 3
        if ("базе података" in sl or "бп" in sl) and (sl.count("базе података") > sl.count("базе података 2") + sl.count("базе података 3") or sl.count("бп") > sl.count("бп2") + sl.count("бп3")) and "bp" not in res:
            res.append("db")
        if ("увод у информационе системе" in sl or "уис" in sl) and "uis" not in res:
            res.append("uis")
        if ("језици и окружења за развој ис" in sl or "јорис" in sl) and "joris" not in res:
            res.append("joris")      
        if ("моделовање пословних процеса" in sl or "мпп" in sl) and "mpp" not in res:
            res.append("mpp")
        if ("анализа и логичко пројектовање ис" in sl or "анализа и лп" in sl) and "ailp" not in res:
            res.append("ailp")     
        if ("пословни информациони системи" in sl or "послис" in sl) and "poslis" not in res:
            res.append("poslis")   
        if ("пројектовање информационих система" in sl or "пис" in sl) and "pis" not in res:
            res.append("pis")      
        if ("структуре података и алгоритми" in sl or "спа" in sl) and "spa" not in res:
            res.append("spa")   
        if ("физичко пројектовање ис" in sl or "фпис" in sl) and "fpis" not in res:
            res.append("fpis")  
        if ("администрација базе података" in sl) and "abp" not in res:
            res.append("abp") 
        if ("интегрисана софтверска решења" in sl or "иср" in sl) and "isr" not in res:
            res.append("isr") 
        if ("информациони системи за управљање знањем" in sl) and "isuz" not in res:
            res.append("isuz")         
        if ("исит менаџмент" in sl) and "isitm" not in res:
            res.append("isitm")  
        if ("управљање развојем информационих система" in sl or "урис" in sl) and "uris" not in res:
            res.append("uris")  
        if ("програмски преводиоци" in sl) and "prev" not in res:
            res.append("prev") 
        return res
    if TESTING == "1":
        logging.critical(f"TEST MOD, SIMULIRAM FETCHOVANJE STRANICE CITANJEM IZ FAJLA {TEST_FILE}")
    while True:
        flag = 0

        if TESTING != "1":
            try:
                page_html = requests.get("https://is.fon.bg.ac.rs")
            except:
                logging.critical(f"Nepoznat error pri fetchovanju, pokusavam ponovo za {sleep_interval} {'minuta' if str(sleep_interval)[-1] != '1' else 'minut'}.")
                time.sleep(sleep_interval * 60)
                continue
            
            if page_html.status_code != 200:
                att_count += 1
                if att_count > max_att_count:
                    att_count = 0
                    logging.critical(f"Predjeno max pokusaja, obustavljam sa pokusajima, pokusavam ponovo za {sleep_interval} {'minuta' if str(sleep_interval)[-1] != '1' else 'minut'}.")
                    time.sleep(sleep_interval * 60)
                    continue
                logging.error(f"Server nije odgovorio sa HTML-om (da li je nedostupan? da li smo blokirani?), pokusavam fetch ponovo za {retry_interval}s...(pokusaj {att_count}/{max_att_count})")
                time.sleep(retry_interval)
                continue
            att_count = 0
            logging.info('Uspesno dobijen HTML odgovor.')
            
            articles = BeautifulSoup(page_html.text,"html.parser").find("main").find_all("article")
        else:
            articles = BeautifulSoup(open(TEST_FILE,"r",encoding="utf-8").read(),"html.parser").find("main").find_all("article")
        
        if not articles:
            logging.critical(f"Nepoznat error (main tag ne postoji? promenjena struktura sajta?), pokusavam ponovo za {sleep_interval} {'minuta' if str(sleep_interval)[-1] != '1' else 'minut'}.")
            time.sleep(sleep_interval * 60)
            continue 
        
        if not old_articles:
            logging.info(f"Prvi fetch, cekam {sleep_interval} {'minuta' if str(sleep_interval)[-1] != '1' else 'minut'}, pa poredim za razlike.")
            time.sleep(sleep_interval * 60)
            old_articles = articles
            continue 
            
        if len(articles) < len(old_articles):
            logging.error(f"Novi artikli imaju manje sadrzaja od starih. Da li je neka vest obrisana? Cekam {sleep_interval} {'minuta' if str(sleep_interval)[-1] != '1' else 'minut'}, pa poredim za razlike.")
            time.sleep(sleep_interval * 60)
            old_articles = articles 
            continue
            
        #svaki artikal na sajtu je jedinstveno identifikovan svojim id atributom
        #iz tog razloga trazimo razliku izmedju starih i novih artikala po id vrednosti
        #pretvaranjem drugog niza u set dobijamo pretrazivanje u konstantnom vremenu, pa algoritam ima O(n) umesto O(n^2) vremensku kompleksnost
        #svaki objekat dobijen iz find_all je jedinstven tako da pretvaranjem u set ne dolazimo ni do kakvog rizika
        old_articles_comp = set([o['id'] for o in old_articles])
        diff_articles = [a for a in articles if a['id'] not in old_articles_comp]

        if not diff_articles:
            logging.info(f"Nema razlike od prethodnog fetcha, cekam {sleep_interval} {'minuta' if str(sleep_interval)[-1] != '1' else 'minut'}, pa poredim ponovo.")
            time.sleep(sleep_interval * 60) 
            continue        
        logging.info("RAZLIKA PRONADJENA, KRECEM POKUSAJ SLANJA MEJLA")
        for article in diff_articles:
            svi_linkovi = article.select("a")
            vest_header = svi_linkovi[0]
            
            if vest_header:
                vest_naslov = vest_header["title"]
                vest_link = vest_header["href"]
            else:
                vest_naziv = "Nepoznat naziv"
                vest_link = ""
            
            kategorije = odredi_kategorije(vest_naslov)
            
            #iz nekog razloga u nekim vestima je deo sadrzaja u <div> umesto obican paragraf <p> 
            sadrzaj = article.select("p, div:not([class]):not([id]):not([style]):not([name]):not([title]):not([role])")

            if sadrzaj:
                #poslednji p tag je dugme koje nam ne treba 
                sadrzaj.pop()
                if not sadrzaj:
                    sadrzaj_tekst = "Vest nema sadrzaj."
                else:
                    sadrzaj_tekst = "\n".join([s.text for s in sadrzaj])
            else:
                sadrzaj_tekst = "Nepoznat tekst"
                
            
            
            #body = vest_naslov + "\n\n"
            body = f"<p>CELU VEST POGLEDATI <a href = \"{vest_link}\">OVDE</a></p><br><p>{sadrzaj_tekst}</p>"
            
            
            if any([True if x["href"] != vest_link else False for x in svi_linkovi]):
                body += "<br><p>Linkovi spomenuti u vesti:</p>"
            
            for link in svi_linkovi:
                if link["href"] != vest_link:
                    l = link["href"]
                    body += f"<p>{l}</p>"
            body += f"Kategorije: {kategorije}<p>"
            print(f"TEST {kategorije}")
            try:
                studenti_za_slanje = povuci_studente(kategorije)
            except Exception as e:
                logging.critical(f"NEUSPESNO POVLACENJE STUDENATA ZA SLANJE, preskacem artikal {vest_naslov}")
                logging.critical(e) 
                flag = 1
                continue
            logging.info(f"USPESNO POVUCENI STUDENTI ZA SLANJE ARTIKLA {vest_naslov}: {studenti_za_slanje}")
                    
            
            for x in studenti_za_slanje['data']:
                try:
                    hn = socket.gethostbyname("frontend")
                    unsub_link = f"http://localhost:3000/unsubscribe?token={x['unsubtoken']}"
                    posalji_mejl([x['email']], vest_naslov, body + f"<br><i>Ovaj mejl vam je poslat jer se nalazite na mejling listi za notifikacije. Da biste se odjavili kliknite <a href = \"{unsub_link}\">OVDE.</a></i>")
                    logging.info(f"USPESNO POSLAT MEJL ZA ARTIKAL {vest_naslov} KORISNIKU {x['email']}")
                except Exception as e:
                    flag = 1
                    logging.critical(f"NEUSPESNO SLANJE MEJLA ZA ARTIKAL {vest_naslov} KORISNIKU {x['email']}, nastavljam dalje...") 
                    logging.critical(e) 
            



        old_articles = articles
        if flag == 0:
            logging.info(f"Uspesno izvrsen odgovor, sledeci fetch za {sleep_interval} {'minuta' if str(sleep_interval)[-1] != '1' else 'minut'}.")
        elif flag == 1:
            logging.info(f"Neki delovi odgovora nisu uspesno izvrseni, sledeci fetch za {sleep_interval} {'minuta' if str(sleep_interval)[-1] != '1' else 'minut'}.")
        time.sleep(sleep_interval * 60)
    

    
    

if __name__ == "__main__":
    logging.basicConfig(level=logging.DEBUG, format='%(asctime)s - %(levelname)s - %(message)s')
    main()
    
    
