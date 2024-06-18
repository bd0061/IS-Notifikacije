import React, { useState, useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import { unsubscribe } from '../api/apiService';
import StatusPage from './StatusPage/StatusPage';

export default function ConfirmUnsubscribe() {
    const [heading, setHeading] = useState("");
    const [mesg, setMesg] = useState("");
    const [token, setToken] = useState("");
    const [loading, setLoading] = useState(true); 
    const location = useLocation();

    useEffect(() => {
        const params = new URLSearchParams(location.search);
        const tokenValue = params.get("token");

        setToken(tokenValue);

        if (!tokenValue) {
            setHeading("Neispravan link");
            setMesg("Molimo proverite da li ste ispravno kopirali link.");
            setLoading(false);
            return;
        }
    }, [location.search]);

    useEffect(() => {
        const unsub = async () => {
            try {
                const response = await unsubscribe(token);
                const data = await response.json();
                if (!response.ok) {
                    setHeading("Error pri odjavljivanju");
                    setMesg(data.error.errorMessage);
                } else {
                    setHeading("Uspeh");
                    setMesg(data.data);
                }
            } catch (error) {
                setHeading("Error pri konekciji");
                setMesg("Proverite Vašu konekciju i dostupnost servera.");
            } finally {
                setLoading(false);
            }
        };

        if (token) {
            unsub();
        }
    }, [token]);

    if (loading) {
        return <StatusPage heading="Odjavljujem..." mesg="Molimo sačekajte.." spinner={true} />;
    } else {
        return (
            <StatusPage 
                heading={heading} 
                mesg={mesg}  
            />
        );
    }
}
