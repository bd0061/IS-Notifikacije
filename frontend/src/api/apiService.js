const baseURL = 'http://localhost:5001/api';
export async function getCourses()
{
    const odgovor = await fetch(baseURL + '/Kursevi');
    if(!odgovor.ok)
    {
        throw new Error("Neuspelo fetchovanje kurseva.");
    }
    return await odgovor.json();
}

export async function dodajMejl(mejl, sifre)
{
    const kys = {
        Mejl: mejl, 
        sifre: sifre
    };
    const odgovor = await fetch(baseURL + '/Mejl', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(kys)
    });
    return odgovor;
}

export async function verifyMail(token,sifre)
{
    const apiLink = `${baseURL}/verify?token=${token}${sifre.map(s => `&sifre=${s}`).join('')}`;
    const odgovor = await fetch(apiLink);
    return odgovor;
}
export async function unsubscribe(token)
{
    const apiLink = `${baseURL}/unsubscribe?token=${token}`;
    const odgovor = await fetch(apiLink, 
	{
		method: 'DELETE',
		headers: {
			'Content-Type': 'application/json'
		}
	}
    );
    return odgovor;
}
