### Books CRUD with AI-assisted Search

This solution contains:
- Backend: ASP.NET Core 8 Web API (`BooksApi/`) with EF Core (SQLite) providing CRUD endpoints for books and an AI-like search endpoint.
- Frontend: React + Vite + TypeScript (`frontend/`) providing a simple UI for managing books and performing AI-assisted search.

#### Backend (BooksApi)
- Requirements: .NET 8 SDK
- Config: `BooksApi/appsettings.json` (default SQLite: `books.db`).
- Run:
```
cd BooksApi
dotnet restore
dotnet run
```
Swagger UI: `https://localhost:7080/swagger` (or `http://localhost:5080/swagger`).

Endpoints:
- `GET /api/books?q=term&genre=...&page=1&pageSize=20`
- `GET /api/books/{id}`
- `POST /api/books`
- `PUT /api/books/{id}`
- `DELETE /api/books/{id}`
- `POST /api/ai/search` with body `{ "query": "natural language" }`

AI search note: Currently uses a keyword-based `SimpleBookSearchService`. You can later swap in an OpenAI-powered implementation via an environment variable if desired.

#### Frontend (React + Vite)
- Requirements: Node.js 18+
- Run:
```
cd frontend
npm install
npm run dev
```
The dev server runs at `http://localhost:5173` and is allowed by backend CORS.

#### Project structure
- `BooksApi/` — ASP.NET Core Web API (EF Core Sqlite, Swagger, CORS, seed data).
- `frontend/` — React UI (Vite + TS) with CRUD pages and AI search.
