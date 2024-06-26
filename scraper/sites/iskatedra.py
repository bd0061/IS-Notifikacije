import time
import requests
import logging
from bs4 import BeautifulSoup

import evars
from api.povuci_primaoce import povuci_primaoce
from mail.mailsender import posalji_mejl

def iskatedra_start():
    logging.basicConfig(level=logging.DEBUG, format='%(asctime)s - %(levelname)s - %(message)s')
    att_count = 0
    vidjeni_artikli = {}
    novi_artikli = []

    def odredi_kategorije(s):
        res = []
        sl = s.lower()


        if "програмски језици" in sl and "pj" not in res:
            res.append("pj")
        if "базе података 2" in sl or "бп2" in sl and "db2" not in res:
            res.append("db2")  
        if "базе података 3" in sl or "бп3" in sl and "db3" not in res:
            res.append("db3")
        #moramo da se postaramo da ispravno matchujemo predmet baze podataka kada su u obavestenju prisutni i baze podataka 2 ili 3
        if ("базе података" in sl or "бп" in sl) and (sl.count("базе података") > sl.count("базе података 2") + sl.count("базе података 3") or sl.count("бп") > sl.count("бп2") + sl.count("бп3")) and "db" not in res:
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
    if evars.TESTING_IS == "1":
        logging.critical(f"TEST MOD, SIMULIRAM FETCHOVANJE STRANICE CITANJEM IZ FAJLA {evars.TEST_FILE_IS}")
    while True:
        flag = 0
        novi_artikli =[]
        if evars.TESTING_IS != "1":
            try:
                page_html = requests.get("https://is.fon.bg.ac.rs")
            except Exception as e:
                logging.critical(f"Error pri fetchovanju: {e}, pokusavam ponovo za {evars.SLEEP_INTERVAL} {'minuta' if str(evars.SLEEP_INTERVAL)[-1] != '1' else 'minut'}.")
                time.sleep(evars.SLEEP_INTERVAL * 60)
                continue
            
            if page_html.status_code != 200:
                att_count += 1
                if att_count > evars.MAX_ATT_COUNT:
                    att_count = 0
                    logging.critical(f"Predjeno max pokusaja, obustavljam sa pokusajima, pokusavam ponovo za {evars.SLEEP_INTERVAL} {'minuta' if str(evars.SLEEP_INTERVAL)[-1] != '1' else 'minut'}.")
                    time.sleep(evars.SLEEP_INTERVAL * 60)
                    continue
                logging.error(f"Server nije odgovorio sa HTML-om (kod {page_html.status_code}) (da li je nedostupan? da li smo blokirani?), pokusavam fetch ponovo za {evars.RETRY_INTERVAL}s...(pokusaj {att_count}/{evars.MAX_ATT_COUNT})")
                time.sleep(evars.RETRY_INTERVAL)
                continue
            att_count = 0
            logging.info('Uspesno dobijen HTML odgovor.')
            
            articles = BeautifulSoup(page_html.text,"html.parser").find("main").find_all("article")
        else:
            articles = BeautifulSoup(open(evars.TEST_FILE_IS,"r",encoding="utf-8").read(),"html.parser").find("main").find_all("article")

        if not articles:
            logging.critical(f"Nepoznat error (main tag ne postoji? promenjena struktura sajta?), pokusavam ponovo za {evars.SLEEP_INTERVAL} {'minuta' if str(evars.SLEEP_INTERVAL)[-1] != '1' else 'minut'}.")
            time.sleep(evars.SLEEP_INTERVAL * 60)
            continue 

        if not vidjeni_artikli:
            logging.info(f"Prvi fetch, cekam {evars.SLEEP_INTERVAL} {'minuta' if str(evars.SLEEP_INTERVAL)[-1] != '1' else 'minut'}, pa ponovo trazim nove artikle.")
            time.sleep(evars.SLEEP_INTERVAL * 60)
            for article in articles:
                vidjeni_artikli[article['id']] = article
            continue 
        #npr
        # {"5646-id" : article_object1, "57545-id": article_object2}
        for article in articles:
            if not article['id'] in vidjeni_artikli:
                novi_artikli.append(article)
                vidjeni_artikli[article['id']] = article


        if not novi_artikli:
            logging.info(f"Nema novih artikala, cekam {evars.SLEEP_INTERVAL} {'minuta' if str(evars.SLEEP_INTERVAL)[-1] != '1' else 'minut'}, pa poredim ponovo.")
            time.sleep(evars.SLEEP_INTERVAL * 60) 
            continue        
        logging.info(f">>>>>>>>>>>>>>>>>>>>>>>NOVI {'ARTIKLI PRONADJENI' if len(novi_artikli) > 1 else 'ARTIKAL PRONADJEN'}, KRECEM POKUSAJ SLANJA MEJLA>>>>>>>>>>>>>>>>>>>>>>>")
        for article in novi_artikli:
            
            svi_linkovi = article.select("a")
            vest_header = svi_linkovi[0]
            
            if vest_header:
                vest_naslov = vest_header["title"]
            else:
                vest_naslov = "Nepoznat naziv"
            
            logging.info(f">>>>>OBRADJUJEM ARTIKAL \"{vest_naslov}\">>>>>")
            logging.info("Odredjujem kategorije artikla...")
            kategorije = odredi_kategorije(vest_naslov)
            if not kategorije:
                logging.error(f"Artikal \"{vest_naslov}\" ne pripada ni jednoj od poznatih kategorija, ignorisem...")
                continue
            body = article.prettify() + f"<br><p>Kategorije: {kategorije}</p>"
            
            logging.info(f"Artikal \"{vest_naslov}\" vezan je za sledece kategorije: {kategorije}")
            
            try:
                studenti_za_slanje = povuci_primaoce('http://mailbackend:5001/api/MejlPoSiframa',{'kursSifre': kategorije})
            except Exception as e:
                logging.critical(f"NEUSPESNO POVLACENJE STUDENATA ZA SLANJE ({e}), preskacem artikal \"{vest_naslov}\"")
                flag = 1
                continue
            if not (tmp:=[x['email'] for x in studenti_za_slanje['data']]):
                logging.warning(f"Nijedan student u bazi ne prima vesti za {'kategoriju' if len(kategorije) == 1 else 'kategorije'} {kategorije}, preskacem artikal \"{vest_naslov}\"...")
                continue #tehnicki nepotrebno
            else:
                logging.info(f"USPESNO POVUCENI STUDENTI ZA SLANJE ARTIKLA: {vest_naslov}: {tmp}")         
            
            for x in studenti_za_slanje['data']:
                try:
                    unsub_link = f"http://localhost:3000/unsubscribe?token={x['unsubtoken']}"
                    posalji_mejl([x['email']], 'Нова вест - ' + vest_naslov, body + f"<br><i>Ovaj mejl vam je poslat jer se nalazite na mejling listi za notifikacije. Da biste se odjavili kliknite <a href = \"{unsub_link}\">OVDE.</a></i>")
                    logging.info(f"KORISNIKU {x['email']} USPESNO POSLAT MEJL ZA ARTIKAL: \"{vest_naslov}\"")
                except Exception as e:
                    flag = 1
                    logging.critical(f"NEUSPESNO SLANJE MEJLA ZA ARTIKAL \"{vest_naslov}\" KORISNIKU {x['email']} ({e}), nastavljam dalje...") 
            

        if flag == 0:
            logging.info(f"Uspesno izvrsen odgovor, sledeci fetch za {evars.SLEEP_INTERVAL} {'minuta' if str(evars.SLEEP_INTERVAL)[-1] != '1' else 'minut'}.")
        elif flag == 1:
            logging.info(f"Neki delovi odgovora nisu uspesno izvrseni, sledeci fetch za {evars.SLEEP_INTERVAL} {'minuta' if str(evars.SLEEP_INTERVAL)[-1] != '1' else 'minut'}.")
        novi_artikli = []
        time.sleep(evars.SLEEP_INTERVAL * 60)