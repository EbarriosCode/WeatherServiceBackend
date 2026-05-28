# 🌤️ Weather Service API

A REST API that retrieves weather data from [Open-Meteo](https://open-meteo.com/) and stores it in MongoDB as a cache. Built with **.NET 6** following **Clean Architecture**.

---

## Table of Contents

- [Architecture](#architecture)
- [Technologies](#technologies)
- [Prerequisites](#prerequisites)
- [Setting up the project locally](#setting-up-the-project-locally)
- [Database Initialization](#database-initialization)
- [Endpoints](#endpoints)
- [Use Cases](#use-cases)
- [Caching Strategy](#caching-strategy)
- [Monitoring](#monitoring)
- [Project structure](#project-structure)
- [Design decisions](#design-decisions)

---

## Architecture

The project follows **Clean Architecture** with four layers and the dependency rule strictly applied:

```
WeatherService.Domain          ← Entities, ValueObjects, contracts (no external dependencies)
WeatherService.Application     ← Use cases, business logic, DTOs
WeatherService.Infrastructure  ← MongoDB Implementation, HTTP clients to Open-Meteo Implementations, Custom HealthChecks
WeatherService.API             ← Controllers, Middleware, Program.cs
```

---

## Technologies

| Technology | Use |
|---|---|
| .NET 6 | Primary framework |
| MongoDB 6.0 | Storage and caching of weather data |
| Mongo Express | UI for viewing data in MongoDB |
| Docker Desktop | Containerization of the local environment |
| Open-Meteo API | External weather data provider and geocoding data provider (free) |
| Swagger / Swashbuckle | Interactive API documentation |
| xUnit + Moq | Unit tests |
| Testcontainers | Integration tests |

---

## Prerequisites

Before setting up the project, make sure you have the following installed:

- [Docker Desktop for Windows](https://www.docker.com/products/docker-desktop/) — version 4.x or higher
- [Visual Studio 2022-2026](https://visualstudio.microsoft.com/) with the **ASP.NET and web development** workload
- [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

> Verify that Docker Desktop is running before executing any commands or launching Visual Studio.

---

## Running the project locally

The local environment runs **3 containers** using Docker Compose:

| Container | Description | Port |
|---|---|---|
| `weather-service-backend` | The REST API | `80` / `443` |
| `mongodb-weather` | MongoDB database | `27017` |
| `mongo-express-weather` | UI for exploring MongoDB | `8081` |

The project can be started in two ways: from **Visual Studio** (recommended for development and debugging) or from the **command line**.

---

### Option A — Visual Studio (recommended)

This is the most convenient way to develop because it allows you to debug within Docker containers directly from the IDE.

1. Open `WeatherService.sln` in Visual Studio
2. In Solution Explorer, you’ll see the `docker-compose` project — this is the startup project
3. Verify that `docker-compose` is selected as the startup project (it should appear in bold). If it isn’t, right-click → **Set as Startup Project**
4. Press **F5** or the **▶ Docker Compose** button on the toolbar

Visual Studio builds the images, starts the 3 containers using the `DEV.env` file configured in the docker-compose project, and automatically attaches the debugger. You can set breakpoints and debug as if it were a normal local application.

> To stop, press **Shift + F5**. The containers stop automatically.

---

### Option B — Command Line

**Step 1 — Clone the repository**

```bash
git clone <repository-url>
cd WeatherService
```

**Step 2 — Check the DEV.env file**

The project uses a `DEV.env` file in the root directory for all configuration. Verify that it exists with the following content:

```env
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://+:443;http://+:80

MongoSettings__ConnectionString=mongodb://admin:password123@mongodb:27017
MongoSettings__DatabaseName=WeatherDb
MongoSettings__CollectionName=WeatherRecords

Cache__TtlHours=3

OpenMeteo__BaseUrl=https://api.open-meteo.com/v1/
OpenMeteo__GeoCodingBaseUrl=https://geocoding-api.open-meteo.com/v1/

MONGO_INITDB_ROOT_USERNAME=admin
MONGO_INITDB_ROOT_PASSWORD=password123

ME_CONFIG_MONGODB_SERVER=mongodb
ME_CONFIG_MONGODB_PORT=27017
ME_CONFIG_MONGODB_ENABLE_ADMIN=true
ME_CONFIG_MONGODB_ADMINUSERNAME=admin
ME_CONFIG_MONGODB_ADMINPASSWORD=password123

ME_CONFIG_BASICAUTH_USERNAME=admin
ME_CONFIG_BASICAUTH_PASSWORD=password123
```

**Step 3 — Start the containers**

```bash
docker-compose up --build
```

The first time, it downloads the base images, which may take a few minutes. Subsequent runs are instantaneous.

To run it in the background:

```bash
docker-compose up --build -d
```

**Step 4 — Verify that everything is running**

```bash
docker ps
```

You should see the 3 containers with status `Up`:

```
CONTAINER ID   IMAGE                  STATUS         PORTS
xxxxxxxxxxxx   weatherserviceapi      Up             0.0.0.0:5000->80/tcp
xxxxxxxxxxxx   mongo:6.0              Up             0.0.0.0:27017->27017/tcp
xxxxxxxxxxxx   mongo-express          Up             0.0.0.0:8081->8081/tcp
```

**Stop the environment**

```bash
docker-compose down
```

To stop and remove the data volumes:

```bash
docker-compose down -v
```

---

### Access the services

| Service | URL |
|---|---|
| Swagger UI (interactive documentation) | http://localhost:5000/swagger or https://localhost:5001/swagger |
| Health Check | http://localhost:5000/health or https://localhost:5001/health|
| Mongo Express (database explorer) | http://localhost:8081 |

> Mongo Express credentials: username `admin` / password `password123`

---

## Database initialization

The database is initialized automatically when the containers are started. The `init-db.js` file in the project root is mounted in the MongoDB container and executed by MongoDB on first startup:

```javascript
// init-db.js
db = db.getSiblingDB(‘WeatherDb’);
db.createCollection(‘WeatherRecords’);
```

This is configured in `docker-compose.yml` using the volume:

```yaml
volumes:
  - mongo_data:/data/db
  - ./init-db.js:/docker-entrypoint-initdb.d/init-db.js:ro
```

> MongoDB automatically runs all `.js` scripts it finds in `/docker-entrypoint-initdb.d/` when it starts up for the first time. If the `mongo_data` volume already exists, the script is not re-run. To force a re-initialization, run `docker-compose down -v` before restarting.

---

## Endpoints

### `GET /api/weather/by-coordinates`

Returns weather data for a geographic location.

**Query parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `latitude` | double | ✓ | Latitude (-90 to 90) |
| `longitude` | double | ✓ | Longitude (-180 to 180) |

**Successful response `200 OK`:**

```json
{
  "temperature": 22.5,
  "windSpeed": 14.2,
  "windDirection": 180.0,
  "sunrise": "2024-01-15T06:23:00",
  "latitude": 14.64,
  "longitude": -90.51,
  "fetchedAt": "2024-01-15T14:30:00Z",
  "fromCache": false
}
```

---

### `GET /api/weather/by-city`

Returns weather data for a city. Internally, it resolves the city name to coordinates using the Open-Meteo geocoding API and applies the same caching logic.

**Query parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `city` | string | ✓ | City name (e.g., “Guatemala City”) |

**Successful response `200 OK`:** same structure as the previous endpoint.

---

### Response codes

| Code | Description |
|---|---|
| `200 OK` | Data returned successfully |
| `400 Bad Request` | Invalid parameters (e.g., latitude out of range, empty city) |
| `404 Not Found` | City not found in the geocoding service |
| `502 Bad Gateway` | The external Open-Meteo service is unavailable |

---

## Examples of Use

### With Swagger UI

Go to http://localhost:5000/swagger, expand the desired endpoint, click **Try it out**, enter the parameters, and run the request.

### With curl

**By coordinates — Guatemala City:**
```bash
curl -X GET “http://localhost:5000/api/weather/by-coordinates?latitude=14.64&longitude=-90.51”
```

**By coordinates — Madrid:**
```bash
curl -X GET “http://localhost:5000/api/weather/by-coordinates?latitude=40.41&longitude=-3.70”
```

**By city:**
```bash
curl -X GET “http://localhost:5000/api/weather/by-city?city=Guatemala%20City”
```

**Verify cache behavior:**
```bash
# First call → fromCache: false (goes to Open-Meteo and saves to MongoDB)
curl -X GET “http://localhost:5000/api/weather/by-coordinates?latitude=14.64&longitude=-90.51”

# Second call → fromCache: true (data from MongoDB)
curl -X GET “http://localhost:5000/api/weather/by-coordinates?latitude=14.64&longitude=-90.51”
```

---

## Caching Strategy

The **cache-aside (lazy loading)** pattern is implemented using MongoDB as the storage backend:

```
Incoming request
      │
      ▼
Does it exist in MongoDB?
      │
   ┌──┴──┐
  NO     YES → Has the TTL expired?
   │              │
   │           ┌──┴──┐
   │          NO     YES
   │           │      │
   │     fromCache   External fetch
   │      = true    → MongoDB upsert
   │                → fromCache = false
   │
External fetch (Open-Meteo)
→ Save to MongoDB
→ fromCache = false
```

**TTL (Time To Live):** configurable in `DEV.env` with `Cache__TtlHours`. Default is 3 hours. Weather data is automatically refreshed upon expiration without the need for manual intervention.

**Coordinate accuracy:** latitude and longitude are rounded to 2 decimal places (~1 km accuracy) for the cache key. Coordinates that are very close to each other share the same record in MongoDB.

---

## Monitoring

### Health Check

```bash
curl https://localhost:5001/health
```

Response:

```json
{
  “status”: “Healthy”,
  “duration”: “00:00:00.1561”,
  “checks”: [
    {
      “name”: “mongodb”,
      “status”: “Healthy”,
      “description”: “MongoDB is reachable.”,
      “tags”: [“database”]
    },
    {
      “name”: “open-meteo-weather”,
      “status”: “Healthy”,
      “description”: “Open-Meteo weather API is reachable.”,
      “tags”: [“external-api”]
    },
    {
      “name”: “open-meteo-geocoding”,
      “status”: “Healthy”,
      “description”: “Open-Meteo geocoding API is reachable.”,
      “tags”: [“external-api”]
    }
  ]
}
```

## Logging

Logging is implemented at three strategic points, following the principle of logging where there is real observability value, not in every class.

**`Application/Services/WeatherServiceImp.cs`** — logs the result of each cache decision (hit, miss, expired), which allows us to measure the cache's effectiveness in production and determine whether the configured TTL is appropriate.

**`Infrastructure/ExternalClients/OpenMeteo/OpenMeteoWeatherClientImp.cs` and `Infrastructure/ExternalClients/OpenMeteo/OpenMeteoGeoCodingClientImp.cs`** — log the start and success of each call to the external provider to detect latency or failures in OpenMeteo without needing to review the code.

**`ExceptionHandlingMiddleware`** — distinguishes between `LogWarning` for expected client errors (400, 404) and `LogError` for actual system failures (500). This distinction prevents noise in production alerts — a 400 is a client error, not a system error.

---

## Explore data in Mongo Express

Navigate to http://localhost:8081, enter your credentials, and access the `WeatherDb` database → `WeatherRecords` collection to view all cached records.

---

## Project Structure

```
WeatherService/
├── DEV.env                                           ← Local environment variables
├── docker-compose.yml                                ← Orchestration of the 3 containers
├── init-db.js                                        ← MongoDB initialization script
├── README.md
│
├── src/
│   ├── WeatherService.Domain/                        ← No external dependencies
│   │   ├── Entities/
│   │   │   └── WeatherRecord.cs
│   │   ├── ValueObjects/
│   │   │   └── Coordinates.cs                        ← Validation and cache key
│   │   └── Interfaces/
│   │       ├── IWeatherRepository.cs
│   │       └── IWeatherExternalClient.cs
│   │
│   ├── WeatherService.Application/                   ← Business logic
│   │   ├── Services/
│   │   │   └── WeatherServiceImp.cs                     ← Cache-aside here
│   │   ├── Interfaces/
│   │   │   ├── IWeatherService.cs
│   │   │   └── IGeoCodingExternalClient.cs
│   │   ├── DTOs/
│   │   │   └── WeatherDto.cs
│   │   │── Settings/
│   │   │    └── CacheSettings.cs
│   │   └── DependencyContainer.cs
│   │
│   ├── WeatherService.Infrastructure/                ← External details
│   │   ├── Persistence/
│   │   │   ├── Documents/WeatherDocument.cs
│   │   │   ├── Mappers/WeatherMapper.cs
│   │   │   └── Repositories/WeatherMongoRepositoryImp.cs
│   │   ├── ExternalClients/OpenMeteo/
│   │   │   ├── OpenMeteoWeatherClientImp.cs
│   │   │   ├── OpenMeteoGeoCodingClientImp.cs
│   │   │   └── Responses/
│   │   ├── Configurations/
│   │   │   └── MongoSettings.cs
│   │   │   └── OpenMeteoSettings.cs
│   │   └── HealthChecks/
│   │       ├── MongoHealthCheck.cs
│   │       ├── OpenMeteoHealthCheck.cs
│   │       └── OpenMeteoGeoCodingHealthCheck.cs
│   │
│   └── WeatherService.API/                           ← Entry point
│       ├── Controllers/WeatherController.cs
│       ├── Middleware/ExceptionHandlingMiddleware.cs
│       └── Program.cs
│
└── tests/
    ├── WeatherService.UnitTests/                     ← Fast, no Docker, with mocks
    │   └── Application/
    │   │   └── Services/
    │   │       └── WeatherServiceImpTests.cs
    │   └── Domain/
    │       └── ValueObjects/
    │           └── CoordinatesTests.cs
    │
    └── WeatherService.IntegrationTests/              ← Slow, use Testcontainers + Docker
        └── Presentation/                             ← End-to-End API tests
            └── Controllers/
                │── WeatherControllerTests.cs
                └── Configurations/
                    │── IntegrationTestBase.cs        ← Testcontainers configuration
                    └── MongoDbContainerFixture.cs
```

---

## Testing

The project has two levels of testing.

**Unit tests** (`WeatherService.UnitTests`) — fast, no Docker, no external dependencies. They test the business logic of `WeatherServiceImp` in isolation using Moq to mock `IWeatherRepository`, `IWeatherExternalClient`, and `IGeoCodingExternalClient`. They cover the main cache-aside scenarios: cache hit, cache miss, expired cache, city-to-coordinates resolution, and input validations.

**Integration tests** (`WeatherService.IntegrationTests`) — run the actual API with `WebApplicationFactory` and a live MongoDB instance using Testcontainers. They test the full end-to-end flow: from the HTTP endpoint to MongoDB and the call to Open-Meteo. They require Docker Desktop to be running.

To run the tests:

```bash
# Unit tests only
dotnet test tests/WeatherService.UnitTests

# Integration tests only (requires Docker Desktop)
dotnet test tests/WeatherService.IntegrationTests

# All tests
dotnet test
```
---

## Design Decisions

**No CQRS or MediatR** — the scope of two read endpoints does not justify the indirection. A simple service is more readable and maintainable.

**Manual cache-aside with in-code TTL** — preferred over MongoDB’s native TTL because the upsert guarantees a single document per coordinate, making automatic deletion unnecessary. Additionally, the in-code TTL is testable and configurable without touching MongoDB indexes.

**ValueObject `Coordinates`** — validates ranges at the domain boundary and encapsulates rounding and cache key generation. No invalid values enter the system.

**IHttpClientFactory via typed clients** — Each external client (`OpenMeteoWeatherClientImp`, `OpenMeteoGeoCodingClientImp`) is registered as a typed `HttpClient`. This enables proper connection pooling and makes the dependency explicit.

**Unique index on `cacheKey`** — Enforced at the MongoDB level as a safety net, independent of application logic.

**`IMongoClient` as a Singleton** — The MongoDB driver internally manages a connection pool. Creating multiple instances would waste resources. The Singleton is the official recommendation of the .NET driver.

**Upsert in `SaveAsync`** — `ReplaceOneAsync` with `IsUpsert = true` handles both the initial insert and subsequent updates without conditional logic. The operation is idempotent and safe against concurrent calls thanks to the unique index on `cacheKey`.

**Automatic MongoDB initialization** — the `init-db.js` script, mounted as a volume, ensures that the database and collection exist at startup, without any additional manual steps.

**Global exception middleware** — maps domain exceptions to the correct HTTP codes in a single location. Controllers do not use try/catch blocks.
