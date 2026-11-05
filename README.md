# Claims API

Backend for insurance claims handling: create, read and delete covers and the claims against them.
The assignment is in [docs/README.md](docs/README.md), and [PLAN.md](PLAN.md) shows how it was built
step by step with an AI assistant.

## Running it

You need the .NET 10 SDK and Docker. Run these from the repository root:

```bash
docker compose up -d
dotnet run --project src/Claims.Api
```

Swagger opens at https://localhost:7052/swagger. Claims and covers go to MongoDB, the audit log to SQL Server.
The password in `docker-compose.yml` and `appsettings.Development.json` is only for these local containers.

## Running the tests

```bash
dotnet test --solution Claims.sln
```

The integration tests start their own MongoDB and SQL Server through Testcontainers, so Docker has to
be running. The unit tests need nothing:

```bash
dotnet test --project tests/Claims.UnitTests
```

## Structure

| Project | Responsibility | Depends on |
|---|---|---|
| `Claims.Domain` | Entities and the premium formula | nothing |
| `Claims.Application` | Services, validation, request and response types, repository and queue interfaces | Domain |
| `Claims.Infrastructure` | EF Core, MongoDB, SQL Server, the audit queue and worker | Application |
| `Claims.Api` | Controllers, validation errors as 400 responses, startup | Application, Infrastructure |

A request goes from the controller to a service, which validates it, saves through a repository and
queues an audit entry. A background worker writes the audit entries to SQL Server, so a request does
not wait for the audit database.

## Business rules and how they were read

- **Insured days** are counted by calendar date and the end date is not charged: 1 to 31 January is 30 days.
- **Premium discounts** are cumulative: from day 181 a yacht has 8% off (5% + 3%), other types 3% (2% + 1%).
- **One year** is a calendar year, so a period that includes 29 February can take 366 days.
- **A claim** can be created from the first to the last day of its cover, both included. The cover must
  exist, and the damage must be above 0 and at most 100.000.
- **Dates without an offset** are read as UTC, and "today" is the current UTC date.
- **`/Covers/compute`** follows the same rules as creating a cover, so it does not quote a cover that could not be bought.

## Known limitations

- The audit queue lives in memory. It is emptied on a normal shutdown, but a crash loses what is still
  queued. A failed write is logged and only retried for transient SQL errors. An outbox table or a
  message broker would fix this, and only the `IAuditQueue` implementation would change.
- The damage limit applies per claim, not per cover.
- Deleting a cover leaves its claims in place.
- Records cannot be updated, and the API has no authentication.
