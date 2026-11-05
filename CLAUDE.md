# Claims API: working rules

Interview assignment: finish a .NET 10 backend for insurance claims. The task is in `docs/README.md`,
the step-by-step plan in `PLAN.md`. Reviewers read both the code and how it was built with AI, so the
history and these rules are part of the deliverable.

## How we work

The developer drives. Claude proposes and writes code, the developer has to be able to explain every line.

1. Work on one step of `PLAN.md` at a time. Do not start the next step or fix things that belong to a later one.
2. Before coding, say in a few sentences what will change and why. If there is a real choice, give a
   recommendation and wait for an answer.
3. Keep each change small enough to read in one sitting. Split a step when it grows.
4. After coding, build and run the tests, then show the diff and explain it: what changed, why, and what to look at.
5. Stop there. Never commit, push or open a PR until the developer says so for this change.
6. After a commit, update the step's status and decision notes in `PLAN.md`.

When the developer questions or rejects something, record the reason in `PLAN.md`. Those notes are the
most useful part of the file for reviewers.

## Language

- Chat with the developer: Macedonian.
- Code, comments, commit messages, `README.md` and `PLAN.md`: English.

## Architecture

| Project | Holds | May reference |
|---|---|---|
| `Claims.Domain` | Entities, premium formula | nothing |
| `Claims.Application` | Services, validators, DTOs, repository and queue interfaces | Domain |
| `Claims.Infrastructure` | EF Core, MongoDB, SQL Server, audit worker | Application |
| `Claims.Api` | Thin controllers, error handling, startup | Application, Infrastructure |

- Controllers call a service and turn the result into a response. No business logic and no EF Core.
- Domain classes stay free of `[Bson]` attributes. Mapping lives in the `DbContext`.
- Use specific repositories (`IClaimRepository`), not a generic one.
- Do not add MediatR, CQRS, AutoMapper or Unit of Work. Add an abstraction only when it solves a problem we have.
- Run validation explicitly in the service with FluentValidation, not through automatic pipeline validation.
- Use `TimeProvider` instead of `DateTime.Now` or `DateTime.UtcNow`.

## Code style

- Less code is better. No speculative options, helpers or layers.
- Comments explain why, not what. XML docs only where they add something, such as on controllers for Swagger.
- Match the surrounding code. Pass `CancellationToken` through async calls.
- Bug fixes in existing behaviour start with a failing test.

## Writing

Human readers, plain language, no filler. No marketing words and no claims that were not verified.

## Commits

One step, one purpose, and every commit builds with passing tests.

```text
<Short imperative summary>

Why the change is needed and any trade-off made. Mention what was
checked by hand if tests do not cover it.
```

Keep the `Co-Authored-By` line: it is open about AI use and we do not hide it.

## Commands

```bash
docker compose up -d                                # local MongoDB and SQL Server, run from the repo root
dotnet build Claims.sln
dotnet test --solution Claims.sln                   # integration tests need Docker running
dotnet test --project tests/Claims.UnitTests
```

`dotnet test` needs `--solution` or `--project`: `global.json` selects Microsoft.Testing.Platform.
