# Package Dependency Reference

Last updated: April 26, 2026

## Goal
Keep the application dependency set minimal, stable, and compatible with `net10.0-windows`.

## Active Core Packages (kept)
- `CsvHelper` (CSV handling)
- `DockPanelSuite` (WinForms docking UI)
- `ExcelDataReader`, `ExcelDataReader.DataSet` (Excel import)
- `Microsoft.Extensions.DependencyInjection` (startup composition root for services/forms)
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.EntityFrameworkCore.Sqlite.NetTopologySuite`
- `Microsoft.EntityFrameworkCore.Tools` (dev tooling/migrations)
- `netDxf` (DXF import/parsing)
- `NetTopologySuite` (geometry/spatial ops)
- `ProjNET` (projection/transformation support)
- `SkiaSharp` (rendering support)
- `SQLitePCLRaw.bundle_green`
- `SQLitePCLRaw.provider.sqlite3`
- `System.Data.SQLite`
- `System.Data.SQLite.Core`

## Packages Removed In Cleanup
- `Proj.NET` (legacy package; conflicted with `ProjNET`)
- `NetTopologySuite.IO` (legacy target, generated compatibility warnings)
- `NetTopologySuite.Features` (not used in current code)
- `PDFsharp` (not used in current code)
- `FastMember` (not used in current code)
- `ExcelNumberFormat` (not used in current code)
- `Microsoft.Bcl.AsyncInterfaces` (not required directly)
- `Microsoft.Extensions.DependencyInjection.Abstractions` (not required directly)
- `Microsoft.Extensions.Logging.Abstractions` (not required directly)

## Candidate Packages For Future Features
Add only when that feature starts implementation.

### Reporting / Print Layout / Export
- `PdfSharpCore` or `QuestPDF`
  - Use for production PDF layout exports (parcel sheets, map reports, certificates).

### GIS Data Interchange
- `NetTopologySuite.IO.GeoJSON` (latest compatible)
  - Use when GeoJSON import/export is implemented.
- `NetTopologySuite.IO.ShapeFile` (if shapefile I/O is needed)
  - Use for `.shp/.dbf/.shx` workflows.

### Structured Logging
- `Serilog`, `Serilog.Sinks.File`
  - Use when centralized diagnostics/audit logs are needed.

### High-Performance Bulk Mapping
- `FastMember`
  - Add only if import pipeline profiling shows reflection/mapping bottlenecks.

## DI Usage In Current App
- `Program.cs` builds the service container and resolves `frmMain`.
- `frmMain` receives `ProjectBackupService`, `ProjectSessionFactory`, `ProjectService`, and `IProjectScopedFactory` via constructor injection.
- `ProjectScopedFactory` centralizes creation of project-session-bound services (`ProjectInfoService`, `ProjectSettingsService`, `LandRecordsService`, `ImportPersistenceService`) so forms no longer compose these manually.
- This keeps startup/runtime wiring centralized and reduces repeated `new` calls across project workflows.

## Rules For Future Package Adds
1. Add package only with a specific feature ticket.
2. Prefer packages targeting modern .NET (`net8+`/`netstandard2.0+`).
3. Avoid parallel legacy/new package duplicates (example: `Proj.NET` vs `ProjNET`).
4. After adding, run:
   - `dotnet restore`
   - `dotnet build`
   - verify no new compatibility/conflict warnings.
