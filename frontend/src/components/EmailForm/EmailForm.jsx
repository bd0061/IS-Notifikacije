import React from 'react';
import {Link} from "react-router-dom";
import './EmailForm.css';
import { CiMail } from "react-icons/ci";
import {useEffect, useState} from 'react';
import { getCourses, dodajMejl } from '../../api/apiService';
import  StatusPage  from '../StatusPage/StatusPage';

const formState = 
{
    FAIL: -1,
    LOADING: 0,
    NORMAL: 1,
    SENDING: 2,
    SUCCESS: 3
}

function isLocalStorageAvailable() {
    try {
        const testKey = 'test';
        localStorage.setItem(testKey, 'testValue');
        localStorage.removeItem(testKey);
        return true;
    } catch (e) {
        return false;
    }
}


export default function EmailForm()
{
    const [kursevi, setKursevi] = useState(() => {

        const d = isLocalStorageAvailable() ? localStorage.getItem('kursData') : null;
        return d ? JSON.parse(d) : [];
      });
    const [email,setEmail] = useState("");
    const [sifre, setSifre] = useState([]);//sifre kurseva koje je korisnik odabrao da prima notifikacije
    const [error,setError] = useState(null);
    const [status,setStatus] = useState(1);
    const [isScreenWidthLessThan, setIsScreenWidthLessThan] = useState(window.innerWidth < 520);

    const handleResize = () => {
        setIsScreenWidthLessThan(window.innerWidth < 520);
    };

    useEffect(() => {
        window.addEventListener('resize', handleResize);
        return () => {
            window.removeEventListener('resize', handleResize);
        };
    }, []);
    async function onSubmit(e)
    {
        const regex = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;

        e.preventDefault();
        if(email === "")
        {
            setError("Polje za mejl ne sme biti prazno.");
            return;
        } 
        else if(!regex.test(email) || email.length > 254)
        {
            setError("Unešena adresa nije validna imejl adresa.");
            return;
        }
        else if(sifre.length === 0)
        {
            setError("Morate izabrati barem jedan predmet za primanje notifikacija.");
            return;
        }
        try 
        {
            setStatus(formState.SENDING);
            const o = await dodajMejl(email,sifre);
            if(!o.ok)
            {
                const data = await o.json();
                setError(data.error.errorMessage);
                setStatus(formState.NORMAL);
                return;
            }
            
        }
        catch(Error)
        {
            setStatus(formState.NORMAL);
            setError("Neuspelo slanje podataka. Da li je server dostupan?");
            return;
        }
        setError("");
        setStatus(formState.SUCCESS);
    }
    function ucitajPonovo()
    {
        window.location.reload();
    }

    function handleChange(k)
    {
        if (!sifre.includes(k.sifra)) {
            setSifre([...sifre, k.sifra]);
          } else {
            setSifre(sifre.filter(s => s !== k.sifra));
          }
    }
    function PonudaKurseva()
    {        
        return(<>             
        <p className = "izbor">Izaberite kurseve:</p>
        {kursevi.map(k => <>
        <label key = {k.id} className="chk">
            <input checked={sifre.includes(k.sifra)} className = "inputchk" type = "checkbox" 
            onChange = {() => handleChange(k)}/>{(isScreenWidthLessThan &&  k.sifra.toUpperCase()) || k.punoIme}
            </label><br></br>
        </>)}
        </>);
    }


    useEffect(() => {
        async function dovuciKurseve()
        {
            try 
            {
                setStatus(formState.LOADING);
                const _kursevi = await getCourses();
                setKursevi(_kursevi);
                isLocalStorageAvailable() && localStorage.setItem("kursData",JSON.stringify(_kursevi));
                setError(null);
                setStatus(formState.NORMAL);
            }
            catch(Error)
            {
                setStatus(formState.FAIL);
            }

        }
        const d = isLocalStorageAvailable() ? localStorage.getItem("kursData") : null;
        if(!d)
        {
            dovuciKurseve();
        }
        else 
        {
            setKursevi(JSON.parse(d));
        }
    },[]);
    if(status === formState.LOADING)
    {
        return(<StatusPage heading = "Učitavanje u toku..." mesg = "Dobavljam kurseve..." spinner = {true}/>);
    }
    else if(status === formState.NORMAL)
    {
        return(
            <div className = 'wrapper'>
                <form onSubmit={onSubmit}>
                    <h1>Prijava</h1>
                    <p className = "mailtext">Unesite vaš studentski mejlooo:</p>
                    {error && <p className = "fade-in" style = {{color: "#B63A3A", paddingTop: "10px",textAlign: "center"}}><strong>{error}</strong></p>}
                    <div className="input-box">
                        <input value = {email} className = "mail-input" type="text" placeholder ="ime@imemejla.com" onChange = {e => setEmail(e.target.value)}/>
                        <CiMail className = "mail-icon" />
                    </div>
                    <PonudaKurseva/>
                    <div className = "button-box">
                        <button type="submit">Dodaj me</button>
                    </div>
                    <Link className = "kako" to ="/info"><i>Kako ovo funkcioniše?</i></Link>
                </form>
            </div>
        );
    }
    else if(status === formState.SENDING)
    {
        return(<StatusPage heading = "Podaci se šalju..." mesg = "Molimo sačekajte..." spinner = {true}/>);
    }
        
    else if(status === formState.SUCCESS)
    {
        return(<StatusPage heading = "Uspešno slanje!" mesg = {<><strong>Proverite Vaš mail inbox</strong> kako biste potvrdili primanje notifikacija.</>} hasButton = {true} btnText = "Nazad" fnc = {() => setStatus(formState.NORMAL)}/>); 
    }
    return(<StatusPage heading = "Neuspešno učitavanje" mesg = "Proverite Vašu konekciju ili dostupnost servera" hasButton = {true} btnText = "Učitaj ponovo" fnc = {ucitajPonovo}/>);
};