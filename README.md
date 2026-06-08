# Milking System Assessment

Technical assessment for C# Backend Developer candidates.

## Quick Start

1. **Start the database:**
   ```bash
   docker-compose up -d
   ```

2. **Wait for initialization** (about 30 seconds for database + migrations)

3. **Run the API:**
   ```bash
   cd MilkingSystem.Api
   dotnet run
   ```

4. **Run tests:**
   ```bash
   dotnet test
   ```

## Project Structure

```
MilkingSystemAssessment/
├── MilkingSystem.Api/          # ASP.NET Core Web API
│   ├── Controllers/            # API endpoints
│   └── Program.cs              # Application setup + Autofac
├── MilkingSystem.Core/         # Core library
│   ├── Models/                 # Domain models
│   ├── Services/               # Data access
│   └── Notifications/          # Robot notification system
├── MilkingSystem.Tests/        # xUnit tests
├── db/
│   ├── init/                   # Database creation script
│   └── migrations/             # Flyway migration scripts
├── docker-compose.yml          # Database setup
├── INSTRUCTIONS.md             # ⭐ START HERE - Candidate instructions
└── EVALUATION_RUBRIC.md        # For interviewers only
```

## For Candidates

**Read [INSTRUCTIONS.md](INSTRUCTIONS.md) for your task description and requirements.**

## For Interviewers

See [EVALUATION_RUBRIC.md](EVALUATION_RUBRIC.md) for scoring criteria (do not include in candidate package).

## Database

- **Server:** localhost:1433
- **Database:** MilkingSystem
- **User:** sa
- **Password:** MilkingSystem123!

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/animals | List all animals |
| GET | /api/animals/{id} | Get animal by ID |
| GET | /api/robots | List all robots |
| GET | /api/robots/active | List active robots |
| GET | /api/milkings/animal/{id} | Get milking events for animal |
| GET | /api/milkings/recent | Get recent milking events |
| GET | /api/weights/animal/{id} | Get weight measurements for animal |
| POST | /api/milkings | **TODO: Implement** |
| POST | /api/weights | **TODO: Implement** |

## Troubleshooting

### Database won't start
- Ensure Docker Desktop is running
- Check if port 1433 is available: `netstat -an | findstr 1433`
- View logs: `docker-compose logs sqlserver`

### Flyway migrations fail
- Ensure database is fully started before Flyway runs
- Check logs: `docker-compose logs flyway`
- Retry: `docker-compose up flyway`

### Reset everything
```bash
docker-compose down -v
docker-compose up -d
```
