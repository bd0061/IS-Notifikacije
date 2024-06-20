import smtplib
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText
from evars import SMTP_SERVER, SMTP_PORT, MAIL_NAME,APP_PASSWORD

def posalji_mejl(primaoci, naslov, telo):
    server = smtplib.SMTP(SMTP_SERVER, SMTP_PORT)
    server.starttls() 
    server.login(MAIL_NAME, APP_PASSWORD)
    message = MIMEMultipart()
    message['From'] = MAIL_NAME
    message['To'] = primaoci[0]
    message['Subject'] = naslov

    body = telo
    message.attach(MIMEText(body, 'html'))

    text = message.as_string()
    server.sendmail(MAIL_NAME, primaoci, text)

    server.quit()