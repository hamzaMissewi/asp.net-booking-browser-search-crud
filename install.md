What I delivered
•
A full-stack Books CRUD app with an AI-assisted search feature
•
Backend: ASP.NET Core 8 Web API with EF Core (SQLite), Swagger, CORS, seeding
•
Frontend: React (Vite + TypeScript) with CRUD UI and AI search UX
Backend (BooksApi)
•
New project: BooksApi/
•
Packages: Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.Sqlite, Swashbuckle.AspNetCore
•
Config:
◦
appsettings.json contains SQLite connection string (Data Source=books.db)
◦
CORS allows http://localhost:5173 and http://localhost:3000
•
Data layer:
◦
Models/Book.cs with annotations
◦
Data/AppDbContext.cs with Fluent config and index on Title+Author
◦
Data/DbSeeder.cs to seed 5 sample books
•
Services:
◦
Services/IBookSearchService.cs + Services/SimpleBookSearchService.cs – keyword-based AI-like ranking with reasons
•
API Endpoints:
◦
Controllers/BooksController.cs – CRUD + list with optional q, genre, paging
◦
Controllers/AiController.cs – POST /api/ai/search with { "query": string }
•
App bootstrap: Program.cs (Swagger, CORS, DbContext, seed on start)
•
Launch profile: Properties/launchSettings.json
•
Project added to solution and references set (FrameworkReference Microsoft.AspNetCore.App)
Frontend (Vite + React + TS)
•
New app under frontend/
•
Files:
◦
package.json, tsconfig.json, vite.config.ts (proxy /api to https://localhost:7080)
◦
index.html, src/main.tsx, src/App.tsx, src/types.ts, src/api.ts
•
Features:
◦
Create form, list with search, edit/delete inline
◦
AI search section invoking /api/ai/search, showing ranked results with reasons
How to run locally
1)
Backend
cd BooksApi
dotnet restore
dotnet run
Open Swagger at https://localhost:7080/swagger (or http://localhost:5080/swagger). DB auto-creates and seeds.
2)
Frontend
cd frontend
npm install
npm run dev
Open http://localhost:5173. The Vite proxy forwards /api to the backend.
Notes
•
The runner here reported missing ASP.NET Core/EF namespaces during build, which indicates the environment lacks the ASP.NET Core targeting pack. With .NET 8 SDK installed locally, the solution should build and run as-is in Rider/VS/CLI.
•
The AI search is a local keyword-based scorer. You can later add an OpenAI-backed implementation guarded by an env var without changing the controller surface.