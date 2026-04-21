# Temperature Statistics API

A .NET 10 Web API that reads historical temperature data from a CSV file, calculates min, max and average temperatures per city, and serves the results via a REST interface.

Calculations run once on startup and are cached in memory. Data can be refreshed at any time by replacing the file and calling the recalculate endpoint.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

---

## Running the app

```bash
dotnet run
```

The API will start on `http://localhost:5052`.  
Swagger UI is available at `http://localhost:5052/swagger`.

---

## Configuration

All configuration is in `appsettings.json`:

| Key | Default | Description |
|-----|---------|-------------|
| `DataFilePath` | `Data/measurements.csv` | Path to the CSV data file, relative to the project root |
| `ApiKey` | `my-secret-key-123` | Secret key required on every request |

---

## Data file format

The data file must be semicolon-separated:

```
1986-01-01T00:00;New York;-0.5
1986-07-15T00:00;New York;28.4
1986-01-01T00:00;Chicago;-14.5
```

| Column | Format | Example |
|--------|--------|---------|
| DateTime | `yyyy-MM-ddTHH:mm` | `1986-01-01T00:00` |
| City | String | `New York` |
| Temperature | Decimal (dot separator) | `-0.5` |

---

## Authentication

Every request requires an `X-Api-Key` header.

**Postman** — add a header:
```
Key:   X-Api-Key
Value: my-secret-key-123
```

**Swagger UI** — each endpoint has an `X-Api-Key` input field. Enter the key there before clicking Execute.

Requests without a valid key return `401 Unauthorized`.

---

## Endpoints

### Get all cities
```
GET /api/temperatures
```
Returns min, max and average temperature for every city in the dataset.

**Response:**
```json
[
  { "city": "New York", "min": -12.3, "max": 35.6, "avg": 10.45 },
  { "city": "Chicago",  "min": -18.9, "max": 33.4, "avg": 6.21  }
]
```

---

### Get a single city
```
GET /api/temperatures/{city}
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `city` | path | City name, e.g. `New York` |

Returns `404 Not Found` if the city does not exist in the dataset.

---

### Filter by average temperature
```
GET /api/temperatures/filter?comparison={gt|lt}&value={number}
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `comparison` | query | `gt` (greater than) or `lt` (less than) |
| `value` | query | Threshold temperature in Celsius |

**Examples:**
```
GET /api/temperatures/filter?comparison=gt&value=15
GET /api/temperatures/filter?comparison=lt&value=0
```

Returns `400 Bad Request` if `comparison` is not `gt` or `lt`.

---

### Recalculate
```
POST /api/temperatures/recalculate
```
Reloads the data file from disk and recalculates all statistics. Use this after replacing the data file with a new dataset.

---

## Updating the dataset

1. Replace the file at the path configured in `DataFilePath`
2. Call `POST /api/temperatures/recalculate`

All subsequent requests will return the new values.
