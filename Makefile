.PHONY: db db-stop app app-stop front front-install up down logs reset test

# ── Banco de dados ────────────────────────────────────────────────
db:
	docker compose up -d db

db-stop:
	docker compose stop db

# ── API (Docker) ──────────────────────────────────────────────────
app:
	docker compose up -d --build api

app-stop:
	docker compose stop api

# ── Frontend ──────────────────────────────────────────────────────
front-install:
	cd Front && npm install

front:
	cd Front && npm run dev

# ── Tudo junto ────────────────────────────────────────────────────
up:
	docker compose up -d --build
	cd Front && npm install && npm run dev

down:
	docker compose down

# ── Reset completo (apaga volume do banco) ────────────────────────
reset:
	docker compose down -v

# ── Testes unitários ─────────────────────────────────────────────
test:
	dotnet test src/Tests/Tests.csproj

# ── Logs da API ──────────────────────────────────────────────────
logs:
	docker compose logs -f api
