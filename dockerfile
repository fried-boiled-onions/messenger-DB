from postgres:15

env POSTGRES_DB=messenger-db
env POSTGRES_USER=postgres
env POSTGRES_PASSWORD=postgres

copy ./init.sql /docker-entrypoint-initdb.d/init.sql

expose 5432