# CWM Student Math System

An automated arithmetic exam grading system built for a take-home assignment: teachers upload
XML exam documents (one student or a full mass batch), the system grades every task, and
students can review which of their tasks were correct.

**Stack:** .NET 8 · ASP.NET Core · Entity Framework Core · SQL Server · Blazor Server

## Documentation

| Doc | What's in it |
|---|---|
| [CLAUDE.md](CLAUDE.md) | The original assignment text, requirement-by-requirement traceability, every documented assumption, and the reasoning behind key decisions |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Hexagonal architecture overview, request-flow diagrams, and an extensibility guide (adding a new file format, a new operator, swapping the database) |
| [DEEP_DIVE.md](DEEP_DIVE.md) | File-by-file, method-by-method walkthrough of every project, with worked traces through the tokenizer/parser and EF Core internals |

## Solution layout

```
CWM.Domain                  entities + domain exceptions, zero dependencies
CWM.Application              ports (interfaces) + use cases -- the hexagon's core
CWM.Adapters.MathEngine       tokenizer -> parser -> evaluator, the independent arithmetic processor
CWM.Adapters.XmlParsing       deserializes the exam XML schema into Application's request shape
CWM.Adapters.Persistence      EF Core + SQL Server
CWM.Adapters.Api              ASP.NET Core Web API -- the one contract used by the UI and third parties alike
CWM.Adapters.Web              Blazor Server UI -- a plain HTTP client of the Api, no privileged access
CWM.Tests                     UnitTests/ ComponentTests/ IntegrationTests/
```

See [ARCHITECTURE.md](ARCHITECTURE.md) for why the project references are wired the way they are.

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB is fine for local development)

## Setup

1. Restore and build:
   ```
   dotnet restore
   dotnet build
   ```

2. Apply the EF Core migrations to create the schema:
   ```
   dotnet ef database update --project CWM.Adapters.Persistence --startup-project CWM.Adapters.Api
   ```
   The connection string is `CWM.Adapters.Api/appsettings.json`'s `ConnectionStrings:CwmDatabase`
   (defaults to a LocalDB instance).

3. Set an API key. `CWM.Adapters.Api/appsettings.json`'s `Auth:ApiKey` and
   `CWM.Adapters.Web/appsettings.json`'s `Api:ApiKey` must match — every request, from the UI
   or from a third party, goes through the same `X-Api-Key` gate.

## Running it

Two processes, in two terminals:

```
dotnet run --project CWM.Adapters.Api    # https://localhost:7180, Swagger at /swagger
dotnet run --project CWM.Adapters.Web    # https://localhost:7037
```

Sample exam files to upload are in [SampleXml/](SampleXml/), covering a single student, a
mass batch of multiple students, a mix of correct/incorrect answers, and malformed expressions
that should grade as "ungradable" without failing the request.

**Testing directly against the Api** (bypassing the Web UI): Swagger's "Authorize" button
accepts the same API key and attaches it to every "Try it out" call. Outside Swagger (curl,
Postman), set the `X-Api-Key` header manually:
```
curl -X POST https://localhost:7180/api/v1/exams/grade \
  -H "X-Api-Key: <your key>" \
  -F "file=@SampleXml/1-single-student.xml;type=text/xml"
```

## Testing

```
dotnet test
```

`UnitTests/` covers Domain, MathEngine, XmlParsing, and Application's use cases in isolation
(hand-written fakes, no mocking framework). `ComponentTests/` exercises the real HTTP pipeline
with the database mocked out. `IntegrationTests/` runs the full stack against a real (in-memory)
SQLite database — see [DEEP_DIVE.md](DEEP_DIVE.md#8--cwmtests) for why each tier uses the test
double it does.
