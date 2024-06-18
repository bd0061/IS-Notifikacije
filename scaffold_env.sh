#!/bin/bash

mkdir -p env

cat <<EOF > env/secrets.env
MAIL_NAME=
APP_PASSWORD=
API_KEY=
SMTP_SERVER=
SMTP_PORT=
EOF

cat <<EOF > env/postgres.env
POSTGRES_USER=
POSTGRES_PASSWORD=
POSTGRES_PORT=
POSTGRES_HOST=
POSTGRES_DBNAME=
EOF

cat <<EOF > env/scraper_config.env
RETRY_INTERVAL=10
MAX_ATT_COUNT=10
SLEEP_INTERVAL=0.5
TESTING=1
TEST_FILE=tests/articles.html
EOF

mkdir -p postgres-data
