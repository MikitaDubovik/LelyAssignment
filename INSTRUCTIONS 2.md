# Technical Assessment: C# Backend Developer (Mid-Level)

## Time Limit
**Maximum 3 hours** — You don't need to use all of it. Focus on demonstrating quality over quantity.

## Business Context

You're working on a livestock management system that collects data from milking robots on a dairy farm. The system follows the [ICAR Animal Data Exchange](https://github.com/adewg/ICAR) standard for animal data management.

### The Problem

Our milking robots are large stationary machines placed in the barn. A cow walks into a robot, and the robot milks her autonomously while providing feed. After milking, the cow leaves the robot.

The robots communicate with our backend system via a low-level TCP socket protocol. When a robot completes milking a cow, it sends a report to our system with the milking details (yield, duration, etc.).

**Critical Business Requirement: A cow must not be milked more than once within a 6-hour window.**

Here's the challenge: cows love the feed they receive during milking. After being milked by Robot A, a hungry cow might walk over to Robot B hoping for more food. Robot B must know that this cow was recently milked and should **send her away without milking**.

Our system must:
1. Record milking events from robots
2. Broadcast notifications to all other robots when a milking is completed
3. Ensure robots can quickly check if a cow was recently milked

**Concurrency considerations:**
- Multiple robots send messages about **different cows** at the same time
- The system must remain responsive and consistent under load

## Technical Stack

- **.NET 6** (you may upgrade to a newer version if desired)
- **ASP.NET Core Web API**
- **Autofac** for dependency injection
- **Raw SQL** (System.Data.SqlClient) — we prefer candidates demonstrate SQL skills without ORM abstractions
- **xUnit** for testing
- **MSSQL Express 2019** (provided via Docker)

## Your Tasks

### 1. Implement the Milking Endpoint

Complete the `POST /api/milkings` endpoint in `MilkingsController.cs`:

- Accept milking data: `animalId`, `robotId`, `milkYieldLiters`, `duration` (optional), `timestamp` (optional)
- Validate that the animal and robot exist
- **Prevent double-milking**: Reject requests if the animal was milked within the last 6 hours
- Save the milking event to the database
- Notify other robots using `IRobotNotifier`
- **Handle concurrent requests**: The system receives messages from multiple robots about different cows simultaneously — ensure thread-safe processing

### 2. Implement the Weight Endpoint

Complete the `POST /api/weights` endpoint in `WeightsController.cs`:

- Accept weight data: `animalId`, `robotId`, `weightKg`, `timestamp` (optional)
- Validate that the animal and robot exist
- Save the weight measurement
- (No double-weighing protection needed)

### 3. Complete the IRobotNotifier Implementation

The `InMemoryRobotNotifier` class has stub methods. Implement:

- `NotifyMilkingCompleted()` — Broadcast to all subscribers so other robots know not to milk this cow
- `Subscribe()` — Allow robots to subscribe to notifications
- `WasRecentlyMilked()` — Quick check if an animal was milked within the protection window (robots call this before starting a milking)
- Ensure **thread-safety** — multiple messages from different robots arrive concurrently

### 4. Register Dependencies

The `IRobotNotifier` registration is missing in `Program.cs`. Add the proper Autofac registration.

### 5. Write Tests

Demonstrate the correctness of your solution with unit tests:

- Test the double-milking prevention logic
- Test concurrent message processing from multiple robots
- Test the notification mechanism
- Test edge cases (invalid animal, invalid robot, cow already recently milked, etc.)

### 6. Improve Legacy Code (Campsite Rule)

The codebase contains some legacy patterns. If you encounter code that could be improved, feel free to refactor it. This is optional but encouraged — "leave the campsite cleaner than you found it."

Examples of issues you might find:
- Service locator anti-pattern
- SQL injection vulnerabilities
- Inconsistent error handling
- Missing abstractions
- Test isolation issues

## Getting Started

### Prerequisites
- Docker Desktop
- .NET 6 SDK (or newer)
- Your preferred IDE (Visual Studio, Rider, VS Code)

### Setup

1. Start the database:
   ```bash
   docker-compose up -d
   ```
   Wait for the database to be ready and migrations to complete.

2. Build and run the API:
   ```bash
   cd MilkingSystem.Api
   dotnet run
   ```

3. The API will be available at `http://localhost:xxxx` (check console output for exact port)

### Existing Data

The database is seeded with:
- 5 animals (Bella, Daisy, Rosie, Buttercup, Clover)
- 4 robots (Robot-A1, Robot-A2, Robot-B1, Robot-B2 — B2 is inactive)
- Some historical milking events and weight measurements

### Running Tests

```bash
dotnet test
```

Note: Integration tests require the database to be running.

## Deliverables

1. **Working code** — Your solution should compile and run
2. **Tests** — Demonstrate correctness with automated tests
3. **Any refactoring** you chose to do (optional)

## Submission

Please submit on the agreed date:
- A written motivation of your choosen solution and improvements
- A ZIP file of your solution, OR
- A link to a GitHub repository

## Interview Discussion

Be prepared to discuss:
- The trade-offs you made
- How you handled concurrency
- What you would improve with more time
- Any design decisions and their rationale
- Legacy code issues you found (and fixed, or chose not to fix, and why)

## Questions?

If anything is unclear, make reasonable assumptions and document them. We'll discuss your assumptions during the interview.

---

Good luck! We're looking forward to seeing your solution.
