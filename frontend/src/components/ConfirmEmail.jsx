import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { verifyMail } from '../api/apiService';
import StatusPage from './StatusPage/StatusPage';

export default function ConfirmEmail() {
    const [sifre, setSifre] = useState([]);
    const [heading, setHeading] = useState("");
    const [mesg, setMesg] = useState("");
    const [token, setToken] = useState("");
    const [loading, setLoading] = useState(true);
    const location = useLocation();
    const navigate = useNavigate();

    useEffect(() => {
        const params = new URLSearchParams(location.search);
        const sifreArray = params.getAll("sifre");
        const tokenValue = params.get("token");

        setSifre(sifreArray);
        setToken(tokenValue);

        if (!tokenValue || sifreArray.length === 0) {
            setHeading("Neispravan link");
            setMesg("Molimo proverite da li ste ispravno kopirali link.");
            setLoading(false);
            return;
        }
    }, [location.search]);

    useEffect(() => {
        const verify = async () => {
            try {
                const response = await verifyMail(token, sifre);
                const data = await response.json();
                if (!response.ok) {
                    setHeading("Error pri konfirmaciji");
                    setMesg(data.error.errorMessage);
                } else {
                    setHeading("Uspešna konfirmacija!");
                    setMesg(data.data);
                }
            } catch (error) {
                setHeading("Error pri konekciji");
                setMesg("Proverite Vašu konekciju i dostupnost servera.");
            } finally {
                setLoading(false);
            }
        };

        if (token && sifre.length > 0) {
            verify();
        }
    }, [token, sifre]);

    if (loading) {
        return <StatusPage heading="Verifikacija u procesu..." mesg="Molimo sačekajte.." spinner={true} />;
    } else {
        return (
            <StatusPage 
                heading={heading} 
                mesg={mesg} 
                hasButton={true} 
                btnText="Nazad na formu" 
                fnc={() => navigate("/prijava")} 
            />
        );
    }
}
