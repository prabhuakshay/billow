# Billow

Simple billing software for Windows, with optional inventory management.

- **Billing** — create and manage bills for customers.
- **Inventory (optional)** — keep track of stock. Businesses that only need billing can leave it switched off.

> **Status:** early development. The app currently opens an empty window; features are being built.

## Requirements

- Windows 10 or 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2026](https://visualstudio.microsoft.com/) with the **.NET desktop development** workload (recommended)

## Running the app

**In Visual Studio:** open `Billow.sln` and press **▶ Billow** (or F5).

**From the command line:**

```
dotnet run --project src/Billow
```

## Project layout

```
Billow.sln          Solution file — open this in Visual Studio
global.json         Pins the .NET SDK version
src/Billow/         The desktop app (WPF, .NET 10)
```
