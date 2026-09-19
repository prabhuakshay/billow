# Billow

Simple billing software for Windows, with optional inventory management.

- **Billing** — create and manage bills for customers.
- **Inventory (optional)** — keep track of stock. Businesses that only need billing can leave it switched off.

> **Status:** early development. The app currently opens an empty window; features are being built.

## Not supported

Billow is built for small Indian retail businesses. It does **not** support:

- **E-invoicing**: generating an IRN and signed QR code through the Invoice Registration Portal. This is mandatory for businesses with annual turnover above ₹5 crore.
- **E-way bills**: needed when moving goods worth more than ₹50,000.

If your business needs either one, use a separate tool for it. See [ADR-0002](docs/adr/0002-no-e-invoicing-or-e-way-bills.md).

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

## Contributing

Git hooks are installed automatically the first time you build or restore (they live in `.husky/`). They:

- **Block commits on `main`.** Work on a branch and merge through a pull request.
- **Require [Conventional Commits](https://www.conventionalcommits.org) messages**, e.g. `feat(invoices): add PDF export` or `fix: correct tax rounding`. Types: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`, `revert`.
- **Check formatting and build the app** before each commit. The build runs the .NET code analyzers and the style rules in `.editorconfig`, with warnings treated as errors. Run `dotnet format` to fix formatting and most style issues automatically.

## Installing

Download `Billow-win-Setup.exe` from the [latest release](https://github.com/prabhuakshay/billow/releases/latest) and run it. No administrator rights or separate .NET install needed — it adds Start menu and desktop shortcuts.

## Where data is stored

All data is kept in a SQLite database at:

```
%LocalAppData%\BillowData\billow.db
```

It is separate from the program files, so updating, reinstalling or uninstalling Billow does **not** delete it. To back up, copy this file while Billow is closed.

## Building the installer

```
.\build-installer.ps1                  # version from Billow.csproj
.\build-installer.ps1 -Version 0.2.0   # or a specific version
```

The installer is written to `releases\Billow-win-Setup.exe`.

## Publishing a release

Push a version tag and GitHub Actions builds the installer and publishes a GitHub Release:

```
git tag v0.2.0
git push origin v0.2.0
```

Tags with a suffix, such as `v0.2.0-beta.1`, are published as pre-releases.

## Project layout

```
Billow.sln                  Solution file — open this in Visual Studio
global.json                 Pins the .NET SDK version
.editorconfig               Formatting, code style and analyzer rules
Directory.Build.props       Code quality settings for all projects
.husky/                     Git hooks
build-installer.ps1         Builds the installer
.github/workflows/          Release automation
src/Billow/                 The desktop app (WPF, .NET 10)
src/Billow/Data/            Database (SQLite via Entity Framework Core)
```
