# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run
dotnet run --project NukeSharp/NukeSharp.csproj

# Test (all)
dotnet test

# Test (single method)
dotnet test --filter "NukeSharpTests.ClassName.MethodName"

# Format (check)
dotnet csharpier check .

# Format (apply)
dotnet csharpier format .
```

CSharpier must be restored before first use: `dotnet tool restore`

## Architecture

This is a nuclear reactor pressure control **simulation** built on ASP.NET Core (.NET 8) with minimal APIs.

### Data flow

```
ReactorWorker (BackgroundService, 1s loop)
  → Reactor.Start() reads valve state
  → IPressureSensor.Update(isValveOpen) adjusts pressure value
  → PressureChanged event fires
  → ReactorSystem.HandlePressureChange() compares pressure vs thresholds
  → IValveControl.Open() or Close() if threshold crossed (2s async delay)
```

### Key components

**ReactorSystem** ([NukeSharp/ControlSystem/ReactorSystem.cs](NukeSharp/ControlSystem/ReactorSystem.cs)) — Core control logic. Subscribes to `PressureChanged` events and opens/closes the valve based on configurable thresholds (default: open at >0.71, close at <0.57). Thresholds use `volatile` fields for thread safety.

**IPressureSensor / PressureSensor / RandomizedPressureSensor** ([NukeSharp/Services/](NukeSharp/Services/)) — Two implementations: deterministic (pressure × 0.94 when valve open, × 1.03 when closed) and randomized (same plus an 8% spike with 5% probability). Selected via `appsettings.json` `"SensorType": "standard"` | `"randomized"`.

**ValveControl** ([NukeSharp/Services/ValveControl.cs](NukeSharp/Services/ValveControl.cs)) — Simulates a 2-second valve actuation delay using `Task.Run`. Uses a lock and `_isLocked` flag to prevent concurrent operations.

**PressureHistory** ([NukeSharp/PressureHistory.cs](NukeSharp/PressureHistory.cs)) — Thread-safe `ConcurrentQueue` capped at 100 readings.

**HTTP endpoints** ([NukeSharp/PressureEndpoints.cs](NukeSharp/PressureEndpoints.cs)):
- `GET /` — Serves static control panel UI
- `GET /pressure` — Current reading via Handlebars template
- `GET /gethistoricmeasurements` — Last 100 readings as JSON
- `POST /thresholds` — Update open/close thresholds (validates open > close)

### Testing

Tests use xUnit + Moq. All core services have interfaces (`IPressureSensor`, `IValveControl`, `IReactor`) specifically to support mocking in tests.
