import requests
from evars import API_KEY

def povuci_primaoce(url,params):
    headers = {
        'Accept': 'text/plain',
        'X-API-KEY': API_KEY
    }
    response = requests.get(url, params=params, headers=headers, verify=False) 
    if response.status_code == 200:
        return response.json()
    raise BaseException(f"Los odgovor servera (status : {response.status_code})")