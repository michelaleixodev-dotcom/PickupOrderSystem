.PHONY: db db-stop app app-stop up down logs

db:
	docker compose up -d db

db-stop:
	docker compose stop db

app:
	docker compose up -d --build api

app-stop:
	docker compose stop api

up:
	docker compose up -d --build

down:
	docker compose down

logs:
	docker compose logs -f
