# MatalaA - Fullstack App

React + ASP.NET + SQL Server, running in Docker with hot reload.

## Prerequisites

- Docker & Docker Compose

## Quick Start

```bash
docker compose up --build
```

- **Frontend**: http://localhost:5173
- **Backend API**: http://localhost:5000/api/health
- **SQL Server**: localhost:1433 (sa / YourStr0ng!Pass)

The first startup takes a minute or two while SQL Server initializes and images are pulled.

## Hot Reload

- **Frontend**: Edit files in `frontend/src/` — Vite HMR picks up changes instantly.
- **Backend**: Edit files in `backend/Backend.Api/` — `dotnet watch` recompiles automatically.

## Stop

```bash
docker compose down
```

To also remove the database volume:

```bash
docker compose down -v
```
# matala
