New-Item -ItemType Directory -Name "env" 
@"
MAIL_NAME=
APP_PASSWORD=
API_KEY=
SMTP_SERVER=
SMTP_PORT=
"@ | Out-File -FilePath "env/secrets.env"

@"
POSTGRES_DB=
POSTGRES_USER=
POSTGRES_PASSWORD=
POSTGRES_PORT=
POSTGRES_HOST=
POSTGRES_DBNAME=
"@ | Out-File -FilePath "env/postgres.env"

@"
RETRY_INTERVAL=10
MAX_ATT_COUNT=10
SLEEP_INTERVAL=5
TESTING_IS=1
TEST_FILE_IS=tests/is/articles.html
"@ | Out-File -FilePath "env/scraper_config.env"
New-Item -ItemType Directory -Name "postgres-data" 