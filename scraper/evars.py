import os
try:
    RETRY_INTERVAL = int(os.getenv("RETRY_INTERVAL"))
except ValueError:
    RETRY_INTERVAL = 10
try:
    MAX_ATT_COUNT = int(os.getenv("MAX_ATT_COUNT"))
except ValueError:
    MAX_ATT_COUNT = 10
try:
    SLEEP_INTERVAL = float(os.getenv("SLEEP_INTERVAL"))
except ValueError:
    SLEEP_INTERVAL = 5
TESTING_IS = os.getenv("TESTING_IS")
TEST_FILE_IS = os.getenv("TEST_FILE_IS","tests/is/articles.html")
SMTP_SERVER = os.getenv("SMTP_SERVER")
if SMTP_SERVER is None:
    raise BaseException("FATAL: SMTP server nepoznat")

SMTP_PORT = int(os.getenv("SMTP_PORT",587))
MAIL_NAME = os.getenv("MAIL_NAME")
if MAIL_NAME is None:
    raise BaseException("FATAL: Ime mejla za slanje nepoznato")
APP_PASSWORD = os.getenv("APP_PASSWORD")
if APP_PASSWORD is None:
    raise BaseException("FATAL: Nepoznata sifra za autentikaciju mejl naloga")
API_KEY = os.getenv("API_KEY")
if API_KEY is None:
    raise BaseException("FATAL: Kljuc za autentikaciju endpointa nepoznat")