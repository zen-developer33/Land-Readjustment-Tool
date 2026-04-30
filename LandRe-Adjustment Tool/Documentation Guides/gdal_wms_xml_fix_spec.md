# Fix Specification — GDAL WMS XML / Non-Raster File Support
## File: `RasterImportService.cs`

---

## Root Cause

`Gdal.Open` is called on `.xml` files (GDAL WMS descriptors) and `.gdal-wms_*.xml`
files before GDAL's WMS/network drivers are registered.
`GdalConfiguration.ConfigureGdal()` only configures file-based drivers.
The WMS driver (`WMS`), WMTS driver (`WMTS`), and TMS driver (`TMS`) must be
explicitly enabled via `Gdal.SetConfigOption` and the driver registered before
`Gdal.Open` is called.

The second problem is that the service has no guard for file types it cannot
handle as importable rasters (`.xml`, `.gdal-wms_*.xml`, `.gdal-wmts_*.xml`).
When such a file is passed, the correct behaviour is to reject it with a clear
user-readable error **before** calling `Gdal.Open`, not let GDAL throw a raw
exception.

---

## Fix 1 — Register GDAL Network Drivers in `GdalConfiguration.ConfigureGdal()`

> **Note:** This fix is in `GdalConfiguration.cs`, not `RasterImportService.cs`.
> The Codex task must locate `GdalConfiguration.ConfigureGdal()` and add the
> lines below immediately after the existing driver registration block.

```csharp
// Add inside GdalConfiguration.ConfigureGdal(), after existing Gdal.AllRegister() call:
Gdal.SetConfigOption("GDAL_HTTP_UNSAFESSL", "YES");
Gdal.SetConfigOption("GDAL_DISABLE_READDIR_ON_OPEN", "EMPTY_DIR");
Gdal.SetConfigOption("CPL_VSIL_CURL_CACHE_SIZE", "128000000");
Gdal.SetConfigOption("GDAL_HTTP_MAX_RETRY", "3");
Gdal.SetConfigOption("GDAL_HTTP_RETRY_DELAY", "1");
```

These options allow the WMS/WMTS/TMS drivers (already built into GDAL) to
open network XML descriptors successfully. `AllRegister()` already registers
the WMS driver — the options above are what make it usable.

---

## Fix 2 — Add `IsNetworkServiceDescriptor` guard in `RasterImportService`

Add this private static method to `RasterImportService`:

```csharp
/// <summary>
/// Returns true when the file is a GDAL network-service XML descriptor
/// (WMS, WMTS, TMS) that cannot be imported as a project raster file.
/// </summary>
private static bool IsNetworkServiceDescriptor(string sourcePath)
{
    string ext = Path.GetExtension(sourcePath);

    // Plain .xml or well-known GDAL WMS/WMTS filename patterns
    if (string.Equals(ext, ".xml", StringComparison.OrdinalIgnoreCase))
        return true;

    string fileName = Path.GetFileName(sourcePath);
    return fileName.Contains(".gdal-wms",  StringComparison.OrdinalIgnoreCase) ||
           fileName.Contains(".gdal-wmts", StringComparison.OrdinalIgnoreCase) ||
           fileName.Contains(".gdal-tms",  StringComparison.OrdinalIgnoreCase);
}
```

---

## Fix 3 — Add early rejection in `ImportToProjectCrs`

In `ImportToProjectCrs`, immediately **after** the `File.Exists` check and
**before** `GdalConfiguration.ConfigureGdal()`, insert:

```csharp
if (IsNetworkServiceDescriptor(sourcePath))
    throw new NotSupportedException(
        $"'{Path.GetFileName(sourcePath)}' is a GDAL network-service descriptor " +
        $"(WMS/WMTS/TMS XML). Live tile service files cannot be imported as " +
        $"raster layers. Add them as an XYZ/WMS layer instead.");
```

---

## Fix 4 — Add the same guard in `ReadSourceMetadata`

In `ReadSourceMetadata`, immediately after the `File.Exists` check, insert:

```csharp
if (IsNetworkServiceDescriptor(sourcePath))
    throw new NotSupportedException(
        $"'{Path.GetFileName(sourcePath)}' is a GDAL network-service descriptor " +
        $"and does not contain raster metadata that can be read as a file.");
```

---

## Fix 5 — Add the same guard in `CreatePreviewImage`

In `CreatePreviewImage`, inside the `try` block after the `File.Exists` check,
insert:

```csharp
if (IsNetworkServiceDescriptor(sourcePath))
    return null;
```

---

## Fix 6 — Improve the GDAL exception message in `ImportToProjectCrs`

The raw GDAL `ApplicationException` message leaks a full file path and says
"not recognized as being in a supported file format" which is confusing to users.

Wrap the `Gdal.Open` call inside `ImportToProjectCrs` so unsupported formats
produce a readable message:

**Current code (line ~50):**
```csharp
using Dataset sourceDataset = Gdal.Open(sourcePath, Access.GA_ReadOnly)
    ?? throw new InvalidOperationException(
        "GDAL could not open the selected raster.");
```

**Replace with:**
```csharp
Dataset sourceDataset;
try
{
    sourceDataset = Gdal.Open(sourcePath, Access.GA_ReadOnly)
        ?? throw new InvalidOperationException(
            $"GDAL could not open '{Path.GetFileName(sourcePath)}'. " +
            $"The file format is not supported for raster import.");
}
catch (ApplicationException ex)
    when (ex.Message.Contains("not recognized as being in a supported file format",
              StringComparison.OrdinalIgnoreCase))
{
    throw new NotSupportedException(
        $"'{Path.GetFileName(sourcePath)}' is not a supported raster format. " +
        $"Supported formats include GeoTIFF, PNG, JPEG, and MBTiles. " +
        $"If this is a WMS/WMTS/TMS XML descriptor, add it as a tile service layer instead.",
        ex);
}
using (sourceDataset)
{
    // move all code that uses sourceDataset inside this using block
}
```

> **Important:** Because `using` with a declaration requires the variable to
> be declared in the same statement, restructure the remainder of the method
> body to sit inside the `using (sourceDataset) { ... }` block.
> All references to `sourceDataset` below the open call remain unchanged —
> only the block structure changes.

---

## Summary of Changes

| # | File | Location | Change |
|---|------|----------|--------|
| 1 | `GdalConfiguration.cs` | `ConfigureGdal()` after `AllRegister()` | Add 5 `SetConfigOption` calls for HTTP/WMS support |
| 2 | `RasterImportService.cs` | New private static method | Add `IsNetworkServiceDescriptor(string)` |
| 3 | `RasterImportService.cs` | `ImportToProjectCrs` — before `ConfigureGdal()` | Throw `NotSupportedException` for XML descriptors |
| 4 | `RasterImportService.cs` | `ReadSourceMetadata` — after `File.Exists` check | Same guard |
| 5 | `RasterImportService.cs` | `CreatePreviewImage` — after `File.Exists` check | Return `null` for XML descriptors |
| 6 | `RasterImportService.cs` | `ImportToProjectCrs` — `Gdal.Open` call | Catch raw GDAL `ApplicationException` and rethrow as `NotSupportedException` with user-readable message |

---

## Do NOT change

- Any tile rendering, layer drawing, cache, or viewport logic.
- `WarpToProjectCrs`, `CopyToProjectRaster`, `BuildRasterOverviews`, or any
  other import processing method.
- `MbTilesRenderLayer`, `RasterRenderLayer`, `RasterDeferredRenderer`, or any
  renderer file.
