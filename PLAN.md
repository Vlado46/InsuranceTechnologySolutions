# Plan

How I am building the Claims API together with an AI assistant (Claude Code). The assignment is in
[docs/README.md](docs/README.md), the rules the assistant follows are in [CLAUDE.md](CLAUDE.md).

## Why this plan exists

My first attempt was generated in one go. It worked, all tests passed, but it was one big change that
I could not explain line by line and that nobody could review. So I threw it away and started from the
template again, this time one small step at a time: I decide the direction, the assistant proposes and
writes, I read the diff, and only then we commit.

The first attempt was still useful. It told me what the solution should look like and where the traps are.

## Target design

Four projects, with dependencies pointing inwards:

```text
Claims.Api ─────────► Claims.Application ──► Claims.Domain
   └─► Claims.Infrastructure ──┘
```

- MongoDB keeps covers and claims, SQL Server keeps the audit log. Both stay on EF Core, as in the template.
- Validation with FluentValidation, called explicitly from the services.
- Audit through an in-memory `Channel` and a `BackgroundService`. No Azure: it is optional in the
  assignment, and I would rather spend the time on the parts that are required.
- Deliberately left out: MediatR, CQRS, AutoMapper, generic repository, Unit of Work. The API is small
  and the assignment says more code is not better code.

## Steps

Each step is one commit, or a few small ones. Status: `todo`, `in progress`, `done`.

### 0. Project setup: `done`

- Add `CLAUDE.md`, this plan and the `next-step` skill.

### 1. Make the application start: `done`

The template does not start, even with Docker running.

- Failing first: run the existing test and read the error.
- Align the audit models with the migration. `ClaimId` and `HttpRequestType` are nullable in the model
  and required in the migration, so `Migrate()` throws `PendingModelChangesWarning`.
- Read connection strings from configuration and add `docker-compose.yml` for local databases.
- Move Testcontainers from `Program.cs` to the test project.
- Replace the vulnerable transitive `System.Security.Cryptography.Pkcs` package.
- Done when: the existing test passes and the API runs from Visual Studio.

### 2. Task 1: introduce layers, no behaviour change: `in progress`

Split into small commits, so that each move can be read on its own.

- 2a. Move the `[Bson]` attributes out of the entities and into the `DbContext`. Documents stay the same.
- 2b. Create `Claims.Domain` and `Claims.Infrastructure`, move the API and tests into `src/` and `tests/`.
  `ClaimsContext` leaves the controller file.
- 2c. Add `Claims.Application` with repository interfaces, and the repositories.
- 2d. Add services, request and response types. Controllers become thin.
- Done when: endpoints behave as before and controllers contain no logic.

### 3. Task 5: premium: `todo`

- Write tests for the desired formula first and see them fail.
- Move the formula to `PremiumCalculator` in the domain and fix it. Known bugs: days are counted up to
  three times because of missing `else`, day 366 is free, and a partial day counts as a whole one.
- Done when: tests cover the boundaries at days 30, 31, 180, 181, 365 and 366 for each kind of type.

### 4. Task 2: validation: `todo`

- Add validators for covers and claims with tests for each rule, using `TimeProvider`.
- Return failures as a 400 `ProblemDetails` response.
- Treat dates without an offset as UTC. Otherwise MongoDB stores them as local time, and on a UTC+1
  machine a cover moves to the previous day.
- Done when: every rule from the assignment has a passing and a failing test.

### 5. Task 3: audit without blocking the request: `todo`

- Add an `IAuditQueue` interface, a `Channel` implementation and a worker.
- Take the timestamp when the change happens, not when it is stored.
- Use `IDbContextFactory` in the worker: it is a singleton and `DbContext` is scoped.
- Empty the queue on shutdown, and make sure stopping twice is safe.
- Audit only after a successful change.
- Done when: an integration test sees the audit row, including after a shutdown.

### 6. Task 4: fill the test gaps: `todo`

- Unit tests for the services: what is stored and audited, and what is not when validation fails.
- Integration tests for the full API against real databases.
- Done when: the whole suite passes several runs in a row and the build has no warnings.

### 7. Documentation and pull request: `todo`

- `README.md`: how to run it, the structure, the assumptions and the known limitations.
- PR text answering the three questions from the assignment.

## Assumptions to confirm or challenge

- The end date is not charged: 1 to 31 January is 30 days.
- "Additional" discount means cumulative: 8% for yachts and 3% for others after day 180.
- One year is a calendar year, so a period over 29 February can take 366 days.
- A claim can be created from the first to the last day of the cover, both included.
- Damage must be above 0, which the assignment does not ask for.
- `/Covers/compute` follows the same rules as creating a cover.

## Decisions and corrections

The log of where I changed direction or corrected the assistant. Filled in as we go.

| Step | What came up | Decision | Why |
|---|---|---|---|
| Start | The first attempt was built in one go | Start again, step by step | I need to understand and explain every part |
| 1 | Audit models allowed nulls, the migration did not | Change the models, keep the migration | An audit row without an id or operation has no meaning, so the schema was right |
| 2 | Moving entities first would make `Claims.Domain` depend on MongoDB for one commit | Remove the `[Bson]` attributes before creating the projects, and create `Claims.Application` only when it gets code | The domain never depends on the database, and the history has no empty project |
