version: '3.8'

services:
  db:
    image: postgres:latest
    container_name: mi_postgres_auth
    restart: always
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: admin
      POSTGRES_DB: auth_db
    ports:
      - "5433:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data: