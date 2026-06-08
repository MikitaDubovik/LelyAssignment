# Notes on my solution

A quick write-up of the choices that I've made.

## Early calls

- **Upgraded to .NET 10.** The brief allows a newer version, and it also
  just let me run the app :) last time I touched .NET 6 was around 2022.
- **Switched to `Microsoft.Data.SqlClient`**, the supported replacement for
  `System.Data.SqlClient`. Still raw SQL, no ORM.

## Double-milking + concurrency

The hard part was about the 6-hour rule holding up when 
several robots report on the same cow at once. A plain check-then-insert races.

So I do two checks: a cheap in-memory pre-check (`WasRecentlyMilked`) for the
common case, and then an authoritative DB re-check inside a per-animal
`SemaphoreSlim` before inserting. The lock is keyed per animal, so different cows
never block each other but the same cow is serialised and her check-then-insert
is atomic. Because that lock dictionary is shared, `MilkingService` is a
singleton.

For the expected failures (animal not found, recently milked, etc.) I return a
result type with a status enum rather than throwing - those are normal outcomes
the controller maps to 404/409/422. Exceptions are left for genuinely unexpected
errors, which a global handler catches.

## The notifier

`InMemoryRobotNotifier` starts empty, so right after a restart it would wrongly
say "not milked" and allow a double. I seed it lazily: the first
`WasRecentlyMilked` pulls recent events from the DB, guarded by a double-checked
lock so it happens once even under concurrent calls. Broadcast invokes each
subscriber in its own try/catch and logs failures (with handler name) so one bad
subscriber can't break the others.

## Refactoring

The big one: `DataService` was one class doing all data access. I split it into
four repositories behind interfaces, with a thin service layer on top so
controllers never touch repositories directly.

Smaller tidy-ups: parameterised all SQL (closing the injection hint), made the
data layer async end to end, added a global exception handler + data-annotation
validation, moved the 6-hour window into config, and switched POSTs to `201
Created`.

## Tests

Unit tests mock the repos/notifier - fast, no DB - and cover validation, the
not-found/inactive paths, both tiers of the double-milking check, and concurrency
(same cow - one succeeds; different cows - both succeed), plus the notifier's
subscribe/broadcast and hydration logic. Integration tests are a deliberately
small set against the real DB: happy-path persistence, double-milking end to end,
and the restart-then-hydrate case a mock can't prove. I also fixed the original
integration tests, which shared a static counter and had ordering dependencies -
each now creates its own data with a unique id.

## What I'd do with more time

- **A real (e.g. TCP) notifier** - the interface is there, so it slots in without
  touching the service.
- **Move the "recently milked" state out of process** (e.g. Redis) so it survives
  more than one replica behind a load balancer.
- **Move Repos to a separate project** (e.g. MilkingSystem.Database or MilkingSystem.Dal) so
it handles the responsibility of working with data instead of keeping it in MilkingSystem.Core

## Assumptions

- "Recently milked" is measured in UTC against the configured window; provided
  timestamps are converted to UTC.
- Zero/negative yield is invalid input, not a valid dry milking.
- Inactive robots can't record data (422) - an offline robot shouldn't be
  reporting.
