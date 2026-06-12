# Pickup Order System

Sistema de gerenciamento de solicitações de coleta e entrega. Backend em .NET 8 (Minimal API) + PostgreSQL, frontend em React + TypeScript + Vite.

---

## Pré-requisitos

- [Docker](https://www.docker.com/) e Docker Compose
- [Node.js 18+](https://nodejs.org/) e npm
- make

---

## Configuração inicial

**1. Clone o repositório**
```bash
git clone https://github.com/michelaleixodev-dotcom/PickupOrderSystem.git
cd PickupOrderSystem
```

**2. Crie o arquivo `.env`** na raiz do projeto com base no exemplo:
```bash
cp .env.example .env
```

> Edite o `.env` e substitua `JWT_SECRET` por uma string aleatória de no mínimo 32 caracteres. As demais variáveis podem ser mantidas como estão.

---

## Executando

### Opção 1 — tudo de uma vez

```bash
make up
```

Sobe o banco + API via Docker e inicia o frontend em modo de desenvolvimento.

### Opção 2 — separado

```bash
make app          # sobe banco + API
make front        # inicia o frontend (primeira vez: make front-install antes)
```

---

## URLs

| Serviço  | URL                          |
|----------|------------------------------|
| Frontend | http://localhost:5173        |
| API      | http://localhost:8080        |
| Swagger  | http://localhost:8080/swagger |

---

## Usuários de teste

| Perfil      | E-mail                             | Senha     |
|-------------|------------------------------------|-----------|
| Colaborador | lucas.mendes@pickupsystem.com      | Senha@123 |
| Cliente     | contato@distribnoroeste.com.br     | Senha@123 |

---

## Outros comandos úteis

```bash
make logs    # acompanha logs da API em tempo real
make down    # para todos os containers
make reset   # para containers e apaga o volume do banco (reseta os dados)
```
