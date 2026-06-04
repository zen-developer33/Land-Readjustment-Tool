using System.Text.Json;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.LandData;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Models.Import;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services.Import.Readers;
using Land_Readjustment_Tool.Services.Raster;
using Land_Readjustment_Tool.UI.Helpers;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using OSGeo.OSR;
using System.Drawing;

namespace Land_Readjustment_Tool.Services.Import
{
    public interface ICadastralImportService
    {
        CadastralFileInfo Inspect(string filePath);

        Task<CadastralImportResult> ImportAsync(
            ProjectSession session,
            string filePath,
            CadastralImportOptions options,
            IProgress<CadastralImportProgress>? progress = null,
            CancellationToken ct = default);
    }

    public sealed class CadastralImportService : ICadastralImportService
    {
        private readonly CadastralVectorReader _reader = new();
        private readonly IProjectRasterCrsResolver _projectCrsResolver;

        public CadastralImportService(IProjectRasterCrsResolver projectCrsResolver)
        {
            _projectCrsResolver = projectCrsResolver;
        }

        public CadastralFileInfo Inspect(string filePath)
        {
            return _reader.Inspect(filePath);
        }

        public async Task<CadastralImportResult> ImportAsync(
            ProjectSession session,
            string filePath,
            CadastralImportOptions options,
            IProgress<CadastralImportProgress>? progress = null,
            CancellationToken ct = default)
        {
            ReportProgress(progress, 3, "Reading cadastral source file...");
            List<CadastralRawParcel> rawParcels = _reader.Read(filePath, options);
            if (rawParcels.Count == 0)
            {
                ReportProgress(progress, 100, "No importable cadastral objects found.");
                return new CadastralImportResult(
                    false,
                    "No importable objects were found in the selected layer(s).",
                    0,
                    0,
                    0,
                    0,
                    0,
                    null,
                    null);
            }

            ReportProgress(progress, 15, $"Read {rawParcels.Count:N0} cadastral object(s). Resolving project CRS...");
            ProjectRasterCrsContext projectCrs =
                await _projectCrsResolver.ResolveAsync(session, ct);

            ReportProgress(progress, 25, "Transforming cadastral geometry to the project CRS...");
            List<CadastralRawParcel> parcels = NeedsTransform(
                    options.SourceCrsCode,
                    projectCrs.TargetSrsDefinition)
                ? TransformParcels(rawParcels, options.SourceCrsCode, projectCrs.TargetSrsDefinition)
                : rawParcels
                    .Select(parcel => parcel with { Geometry = parcel.Geometry.Copy() })
                    .ToList();

            ReportProgress(progress, 40, "Normalizing cadastral geometry for storage...");
            foreach (CadastralRawParcel parcel in parcels)
                NormalizeGeometryForCanvasDatabase(parcel.Geometry);

            int duplicateObjectsSkipped = 0;
            if (options.SkipDuplicateGeometries)
            {
                ReportProgress(progress, 52, "Removing duplicate cadastral geometries...");
                int originalCount = parcels.Count;
                parcels = RemoveDuplicateGeometries(parcels);
                duplicateObjectsSkipped = originalCount - parcels.Count;
            }

            ReportProgress(progress, 60, "Copying cadastral source file into the project folder...");
            string? copiedSourceFile = CopySourceFilesToProjectFolder(session, filePath);

            ReportProgress(progress, 66, "Preparing project database for cadastral objects...");
            AppDbContext context = session.GetDbContext();
            await context.Database.MigrateAsync(ct);

            ReportProgress(progress, 72, "Preparing cadastral map layers...");
            Dictionary<string, CanvasLayer> targetLayers = await GetOrCreateTargetLayersAsync(
                context,
                parcels,
                ct);

            ReportProgress(progress, 78, "Loading Original Parcel Records for automatic assignment...");
            Dictionary<string, BaselineParcel> baselineLookup = options.AutoAssignParcelRecords
                ? await LoadBaselineParcelLookupAsync(context, ct)
                : new Dictionary<string, BaselineParcel>(StringComparer.OrdinalIgnoreCase);

            ReportProgress(progress, 82, "Creating cadastral map objects...");
            DateTime now = DateTime.Now;
            List<CanvasObject> objects = [];
            for (int index = 0; index < parcels.Count; index++)
            {
                CadastralRawParcel parcel = parcels[index];
                string layerKey = GetTargetLayerKey(parcel);
                CanvasLayer layer = targetLayers[layerKey];
                double area = Math.Abs(parcel.Geometry.Area);
                bool isParcelPolygon = IsParcelPolygon(parcel);
                BaselineParcel? assignedParcel = isParcelPolygon
                    ? TryFindBaselineParcel(
                        baselineLookup,
                        parcel.MapSheetNo,
                        parcel.ParcelNo)
                    : null;

                CadastralCanvasMetadata metadata = new()
                {
                    SourceFormat = Path.GetExtension(filePath).TrimStart('.').ToUpperInvariant(),
                    SourceFileName = Path.GetFileName(filePath),
                    ProjectSourceFile = copiedSourceFile,
                    SourceLayer = parcel.SourceLayer,
                    MapSheetNo = parcel.MapSheetNo,
                    ParcelNo = parcel.ParcelNo,
                    CalculatedAreaSqm = area,
                    SourceHandle = parcel.SourceHandle,
                    MatchedText = parcel.MatchedText,
                    AttributesJson = parcel.Attributes.Count == 0
                        ? null
                        : JsonSerializer.Serialize(parcel.Attributes),
                    BaselineParcelId = assignedParcel?.Id,
                    FullUniqueParcelCode = assignedParcel?.FullUniqueParcelCode,
                    RecordAreaSqm = assignedParcel?.OriginalAreaSqm,
                    OwnerName = assignedParcel?.LandOwner?.FullName,
                    LandUse = assignedParcel?.LandUse,
                    AssignmentStatus = assignedParcel == null
                        ? "Unassigned"
                        : "AutoAssigned",
                    ImportedAt = now
                };

                CanvasObject canvasObject = new()
                {
                    CanvasLayerId = layer.Id,
                    CanvasLayer = layer,
                    ObjectType = parcel.ObjectType,
                    Shape = parcel.Geometry,
                    GeometryMetadataJson = JsonSerializer.Serialize(metadata),
                    LabelText = ResolveObjectLabelText(parcel),
                    BaselineParcelId = assignedParcel?.Id,
                    BorderColorOverride = IsAnnotationObject(parcel)
                        ? layer.LabelColor
                        : null,
                    ObjectDescription = BuildDescription(parcel),
                    IsVisible = true,
                    IsLocked = false,
                    SourceDxfHandle = parcel.SourceHandle,
                    CreatedDate = now,
                    LastModifiedDate = now
                };

                objects.Add(canvasObject);
                if (assignedParcel != null && assignedParcel.CanvasObjectId == null)
                    assignedParcel.CanvasObjectId = canvasObject.Id;

                if ((index + 1) % 250 == 0 || index == parcels.Count - 1)
                {
                    int percent = 82 + (int)Math.Round(((index + 1) / (double)parcels.Count) * 10.0);
                    ReportProgress(progress, percent, $"Creating cadastral map objects... {index + 1:N0}/{parcels.Count:N0}");
                }
            }

            ReportProgress(progress, 93, "Saving cadastral map objects to the project...");
            await context.CanvasObjects.AddRangeAsync(objects, ct);
            foreach (CanvasLayer layer in targetLayers.Values)
            {
                layer.SourceFile = copiedSourceFile ?? filePath;
                layer.ImportedDate = now;
                layer.LastModifiedDate = now;
            }

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                session.Logger.LogError("Cadastral map import save failed.", ex);
                ReportProgress(progress, 100, "Cadastral map import failed.");
                return new CadastralImportResult(
                    false,
                    $"Could not save cadastral map: {BuildExceptionMessage(ex)}",
                    0,
                    0,
                    0,
                    0,
                    0,
                    copiedSourceFile,
                    null);
            }

            ReportProgress(progress, 97, "Calculating cadastral map extent...");
            Envelope envelope = new();
            foreach (Geometry geometry in parcels.Select(parcel => parcel.Geometry))
                envelope.ExpandToInclude(geometry.EnvelopeInternal);

            int assigned = objects.Count(item => item.BaselineParcelId.HasValue);
            ReportProgress(progress, 100, $"Imported {objects.Count:N0} cadastral object(s).");
            return new CadastralImportResult(
                true,
                null,
                objects.Count,
                assigned,
                objects.Count - assigned,
                parcels.Count(item => !string.IsNullOrWhiteSpace(item.MatchedText)),
                duplicateObjectsSkipped,
                copiedSourceFile,
                envelope);
        }

        private static void ReportProgress(
            IProgress<CadastralImportProgress>? progress,
            int percent,
            string status)
        {
            progress?.Report(new CadastralImportProgress(
                Math.Clamp(percent, 0, 100),
                status));
        }

        private static List<CadastralRawParcel> RemoveDuplicateGeometries(
            IReadOnlyList<CadastralRawParcel> parcels)
        {
            HashSet<string> seen = new(StringComparer.Ordinal);
            List<CadastralRawParcel> unique = [];
            foreach (CadastralRawParcel parcel in parcels)
            {
                Geometry normalized = parcel.Geometry.Copy();
                normalized.Normalize();
                string key = $"{parcel.ObjectType}|{normalized.AsText()}";
                if (!seen.Add(key))
                    continue;

                unique.Add(parcel);
            }

            return unique;
        }

        private static string? CopySourceFilesToProjectFolder(
            ProjectSession session,
            string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                return null;

            string sourceFolder = Path.GetDirectoryName(sourcePath) ?? string.Empty;
            string extension = Path.GetExtension(sourcePath).ToLowerInvariant();
            string baseName = Path.GetFileNameWithoutExtension(sourcePath);
            string targetFolder = Path.Combine(session.ProjectFolderPath, "Imports", "Cadastral");
            Directory.CreateDirectory(targetFolder);

            string targetBaseName = GetAvailableImportBaseName(targetFolder, baseName, extension);
            string primaryRelativePath = Path.Combine("Imports", "Cadastral", targetBaseName + extension);

            foreach (string path in EnumerateSourceSidecarFiles(sourcePath))
            {
                string sidecarExtension = Path.GetExtension(path);
                string targetPath = Path.Combine(targetFolder, targetBaseName + sidecarExtension);
                File.Copy(path, targetPath, overwrite: true);
            }

            return primaryRelativePath.Replace(Path.DirectorySeparatorChar, '/');
        }

        private static IEnumerable<string> EnumerateSourceSidecarFiles(string sourcePath)
        {
            string extension = Path.GetExtension(sourcePath).ToLowerInvariant();
            if (extension != ".shp")
                return [sourcePath];

            string folder = Path.GetDirectoryName(sourcePath) ?? string.Empty;
            string baseName = Path.GetFileNameWithoutExtension(sourcePath);
            string[] sidecarExtensions =
            [
                ".shp", ".shx", ".dbf", ".prj", ".cpg", ".qix",
                ".sbn", ".sbx", ".xml", ".fix"
            ];

            return sidecarExtensions
                .Select(item => Path.Combine(folder, baseName + item))
                .Where(File.Exists)
                .ToList();
        }

        private static string GetAvailableImportBaseName(
            string targetFolder,
            string baseName,
            string primaryExtension)
        {
            string candidate = SanitizeLayerName(baseName) ?? "cadastral-source";
            string targetPath = Path.Combine(targetFolder, candidate + primaryExtension);
            if (!File.Exists(targetPath))
                return candidate;

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return $"{candidate}_{timestamp}";
        }

        private static async Task<Dictionary<string, CanvasLayer>> GetOrCreateTargetLayersAsync(
            AppDbContext context,
            IReadOnlyList<CadastralRawParcel> parcels,
            CancellationToken ct)
        {
            Dictionary<string, CanvasLayer> layers = new(StringComparer.OrdinalIgnoreCase);
            IReadOnlyList<TargetLayerSpec> specs = parcels
                .Select(parcel => GetTargetLayerSpec(parcel, parcels))
                .GroupBy(spec => spec.Key, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(spec => spec.Name)
                .ThenBy(spec => spec.LayerType)
                .ToList();

            int nextDisplayOrder =
                (await context.CanvasLayers
                    .Select(layer => (int?)layer.DisplayOrder)
                    .MaxAsync(ct) ?? -1) + 1;
            List<Color> existingLayerColors = await GetExistingLayerColorsAsync(context, ct);
            List<Color> newImportColors = [];

            foreach (TargetLayerSpec spec in specs)
            {
                CanvasLayer? layer = await context.CanvasLayers
                    .FirstOrDefaultAsync(
                        item => item.Name == spec.Name &&
                                item.LayerType == spec.LayerType,
                        ct);

                if (layer == null)
                {
                    Color layerColor = ChooseDistinctImportColor(
                        existingLayerColors,
                        newImportColors);
                    layer = CreateImportedCadastralLayer(
                        spec.Name,
                        spec.LayerType,
                        nextDisplayOrder++,
                        layerColor);
                    newImportColors.Add(layerColor);
                    existingLayerColors.Add(layerColor);
                    await context.CanvasLayers.AddAsync(layer, ct);
                    await context.SaveChangesAsync(ct);
                }
                else
                {
                    ApplyImportedCadastralLayerDefaults(layer);
                }

                layers[spec.Key] = layer;
            }

            return layers;
        }

        private static CanvasLayer CreateImportedCadastralLayer(
            string name,
            string layerType,
            int displayOrder,
            Color paletteColor)
        {
            DateTime now = DateTime.Now;
            CanvasLayer layer = new()
            {
                Name = name,
                LayerType = layerType,
                IsVisible = true,
                IsLocked = true,
                IsSelectable = true,
                IsPrintable = true,
                DisplayOrder = displayOrder,
                CreatedDate = now,
                LastModifiedDate = now,
                Description = "Imported cadastral map layer"
            };

            ApplyImportedCadastralLayerDefaults(layer, paletteColor);
            return layer;
        }

        private static void ApplyImportedCadastralLayerDefaults(CanvasLayer layer, Color? paletteColor = null)
        {
            layer.IsLocked = true;
            layer.IsSelectable = true;
            string? paletteFillColor = paletteColor.HasValue ? ToHtml(paletteColor.Value) : null;
            string? paletteStrokeColor = paletteColor.HasValue ? ToHtml(Darken(paletteColor.Value, 0.58f)) : null;

            if (CanvasLayerTreeService.IsAnnotationLayer(layer))
            {
                layer.BorderColor = string.IsNullOrWhiteSpace(layer.BorderColor)
                    ? paletteStrokeColor ?? "#111111"
                    : layer.BorderColor;
                layer.FillColor = null;
                layer.FillStyle = "None";
                layer.ShowFillTransparency = false;
                layer.FillTransparency = 50;
                layer.LineWeight = 0;
                layer.LabelColor = string.IsNullOrWhiteSpace(layer.LabelColor)
                    ? layer.BorderColor
                    : layer.LabelColor;
                layer.LabelFontName = string.IsNullOrWhiteSpace(layer.LabelFontName)
                    ? "Nirmala UI"
                    : layer.LabelFontName;
                layer.LabelFontSize = layer.LabelFontSize <= 0 ? 10.0 : layer.LabelFontSize;
                layer.TextAlignment = string.IsNullOrWhiteSpace(layer.TextAlignment)
                    ? "Center Middle"
                    : layer.TextAlignment;
                layer.PointSymbol = string.IsNullOrWhiteSpace(layer.PointSymbol)
                    ? "Dot"
                    : layer.PointSymbol;
                layer.PointSize = layer.PointSize <= 0 ? 1.0 : layer.PointSize;
                return;
            }

            layer.BorderColor = string.IsNullOrWhiteSpace(layer.BorderColor)
                ? paletteStrokeColor ?? ResolveDefaultBorderColor(layer.LayerType)
                : layer.BorderColor;
            layer.FillColor ??= CanvasLayerTreeService.IsPolygonLayer(layer)
                ? paletteFillColor ?? "#C8E8F4"
                : null;
            layer.ShowFillTransparency = CanvasLayerTreeService.IsPolygonLayer(layer) &&
                                         layer.ShowFillTransparency;
            layer.FillTransparency = layer.FillTransparency <= 0 ? 50 : layer.FillTransparency;
            layer.LineWeight = layer.LineWeight <= 0 ? 1.4 : layer.LineWeight;
            layer.LineStyle = string.IsNullOrWhiteSpace(layer.LineStyle)
                ? "Solid"
                : layer.LineStyle;
            layer.LineTypeScale = layer.LineTypeScale <= 0 ? 1.0 : layer.LineTypeScale;
            layer.FillStyle = string.IsNullOrWhiteSpace(layer.FillStyle)
                ? CanvasLayerTreeService.IsPolygonLayer(layer) ? "Solid" : "None"
                : layer.FillStyle;
            layer.LabelColor = string.IsNullOrWhiteSpace(layer.LabelColor)
                ? "#000000"
                : layer.LabelColor;
            layer.LabelFontName = string.IsNullOrWhiteSpace(layer.LabelFontName)
                ? "Nirmala UI"
                : layer.LabelFontName;
            if (layer.LabelFontSize <= 0 ||
                Math.Abs(layer.LabelFontSize - 2.0) < 0.0001)
            {
                layer.LabelFontSize = 1.0;
            }

            if (string.IsNullOrWhiteSpace(layer.TextAlignment) ||
                string.Equals(layer.TextAlignment.Trim(), "Left", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(layer.TextAlignment.Trim(), "Center", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(layer.TextAlignment.Trim(), "Right", StringComparison.OrdinalIgnoreCase))
            {
                layer.TextAlignment = "Center Middle";
            }

            layer.PointSymbol = string.IsNullOrWhiteSpace(layer.PointSymbol)
                ? "Dot"
                : layer.PointSymbol;
            layer.PointSize = layer.PointSize <= 0 ? 5.0 : layer.PointSize;
        }

        private static async Task<List<Color>> GetExistingLayerColorsAsync(
            AppDbContext context,
            CancellationToken ct)
        {
            var colorValues = await context.CanvasLayers
                .AsNoTracking()
                .Select(layer => new
                {
                    layer.FillColor,
                    layer.BorderColor
                })
                .ToListAsync(ct);

            return colorValues
                .SelectMany(layer => new[] { layer.FillColor, layer.BorderColor })
                .Select(TryParseHtmlColor)
                .Where(color => color.HasValue)
                .Select(color => NormalizeColor(color!.Value))
                .DistinctBy(color => color.ToArgb())
                .ToList();
        }

        private static Color ChooseDistinctImportColor(
            IReadOnlyList<Color> existingLayerColors,
            IReadOnlyList<Color> newImportColors)
        {
            Color[] palette = ColorDialogCustomColorsStore.GetLayerPaletteColors();
            if (palette.Length == 0)
            {
                palette =
                [
                    Color.FromArgb(142, 211, 230),
                    Color.FromArgb(246, 179, 182),
                    Color.FromArgb(207, 246, 194),
                    Color.FromArgb(246, 227, 180)
                ];
            }

            List<Color> candidates = palette
                .Select(NormalizeColor)
                .Where(IsUsableImportLayerColor)
                .DistinctBy(color => color.ToArgb())
                .ToList();
            if (candidates.Count == 0)
            {
                candidates = palette
                    .Select(NormalizeColor)
                    .DistinctBy(color => color.ToArgb())
                    .ToList();
            }

            List<Color> usedColors = existingLayerColors
                .Concat(newImportColors)
                .Select(NormalizeColor)
                .ToList();

            if (usedColors.Count == 0)
                return candidates[0];

            Color? unusedColor = candidates
                .Where(candidate => usedColors.All(used => ColorDistance(candidate, used) >= 75.0))
                .OrderByDescending(candidate => MinimumColorDistance(candidate, usedColors))
                .ThenBy(candidate => candidate.GetHue())
                .FirstOrDefault();

            if (unusedColor.HasValue && unusedColor.Value != Color.Empty)
                return unusedColor.Value;

            return candidates
                .OrderByDescending(candidate => MinimumColorDistance(candidate, usedColors))
                .ThenBy(candidate => candidate.GetHue())
                .First();
        }

        private static Color? TryParseHtmlColor(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            try
            {
                return NormalizeColor(ColorTranslator.FromHtml(value.Trim()));
            }
            catch
            {
                return null;
            }
        }

        private static Color NormalizeColor(Color color)
        {
            return Color.FromArgb(255, color.R, color.G, color.B);
        }

        private static bool IsUsableImportLayerColor(Color color)
        {
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
            double channelRange = Math.Max(color.R, Math.Max(color.G, color.B)) -
                                  Math.Min(color.R, Math.Min(color.G, color.B));

            return luminance is >= 0.35 and <= 0.93 &&
                   channelRange >= 24.0 &&
                   color.ToArgb() != Color.White.ToArgb();
        }

        private static double MinimumColorDistance(Color color, IReadOnlyList<Color> usedColors)
        {
            return usedColors.Count == 0
                ? double.MaxValue
                : usedColors.Min(used => ColorDistance(color, used));
        }

        private static double ColorDistance(Color left, Color right)
        {
            double rMean = (left.R + right.R) / 2.0;
            double r = left.R - right.R;
            double g = left.G - right.G;
            double b = left.B - right.B;

            return Math.Sqrt(
                (2.0 + rMean / 256.0) * r * r +
                4.0 * g * g +
                (2.0 + (255.0 - rMean) / 256.0) * b * b);
        }

        private static Color Darken(Color color, float factor)
        {
            factor = Math.Clamp(factor, 0.0f, 1.0f);
            return Color.FromArgb(
                255,
                (int)Math.Round(color.R * factor),
                (int)Math.Round(color.G * factor),
                (int)Math.Round(color.B * factor));
        }

        private static string ToHtml(Color color) =>
            $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        private static string GetTargetLayerKey(CadastralRawParcel parcel)
        {
            return GetTargetLayerSpec(parcel, [parcel]).Key;
        }

        private static TargetLayerSpec GetTargetLayerSpec(
            CadastralRawParcel parcel,
            IReadOnlyList<CadastralRawParcel> allParcels)
        {
            string baseName = SanitizeLayerName(parcel.CanvasLayerName)
                              ?? SanitizeLayerName(parcel.SourceLayer)
                              ?? "Unknown";
            string layerType = ResolveCanvasLayerType(parcel.ObjectType);
            IReadOnlySet<string> layerTypesForName = allParcels
                .Where(item =>
                    string.Equals(
                        SanitizeLayerName(item.CanvasLayerName) ??
                        SanitizeLayerName(item.SourceLayer) ??
                        "Unknown",
                        baseName,
                        StringComparison.OrdinalIgnoreCase))
                .Select(item => ResolveCanvasLayerType(item.ObjectType))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            string layerName = baseName;
            if (layerTypesForName.Count > 1 &&
                !string.Equals(layerType, GetPrimaryLayerType(layerTypesForName), StringComparison.OrdinalIgnoreCase))
            {
                layerName = $"{baseName} {GetLayerTypeSuffix(layerType)}";
            }

            return new TargetLayerSpec(
                $"{layerType}|{baseName}",
                layerName,
                layerType);
        }

        private static string ResolveCanvasLayerType(string objectType)
        {
            return objectType.Trim().ToLowerInvariant() switch
            {
                "text" => CanvasLayerTreeService.AnnotationLayerType,
                "point" => CanvasLayerTreeService.PointLayerType,
                "line" => CanvasLayerTreeService.PolylineLayerType,
                "polyline" => CanvasLayerTreeService.PolylineLayerType,
                "polygon" => CanvasLayerTreeService.PolygonLayerType,
                _ => CanvasLayerTreeService.PolylineLayerType
            };
        }

        private static string GetPrimaryLayerType(IReadOnlySet<string> layerTypes)
        {
            string[] priority =
            [
                CanvasLayerTreeService.PolygonLayerType,
                CanvasLayerTreeService.PolylineLayerType,
                CanvasLayerTreeService.PointLayerType,
                CanvasLayerTreeService.AnnotationLayerType
            ];

            return priority.First(type => layerTypes.Contains(type));
        }

        private static string GetLayerTypeSuffix(string layerType)
        {
            return layerType switch
            {
                CanvasLayerTreeService.AnnotationLayerType => "Annotation",
                CanvasLayerTreeService.PointLayerType => "Points",
                CanvasLayerTreeService.PolylineLayerType => "Lines",
                _ => layerType
            };
        }

        private static string ResolveDefaultBorderColor(string layerType)
        {
            return layerType switch
            {
                CanvasLayerTreeService.PointLayerType => "#1B5E20",
                CanvasLayerTreeService.PolylineLayerType => "#1976D2",
                CanvasLayerTreeService.AnnotationLayerType => "#111111",
                _ => "#8FCDE4"
            };
        }

        private static bool IsParcelPolygon(CadastralRawParcel parcel)
        {
            return string.Equals(parcel.ObjectType, "Polygon", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAnnotationObject(CadastralRawParcel parcel)
        {
            return string.Equals(parcel.ObjectType, "Text", StringComparison.OrdinalIgnoreCase);
        }

        private static string? ResolveObjectLabelText(CadastralRawParcel parcel)
        {
            return IsAnnotationObject(parcel)
                ? parcel.ParcelNo
                : parcel.ParcelNo;
        }

        private static async Task<Dictionary<string, BaselineParcel>> LoadBaselineParcelLookupAsync(
            AppDbContext context,
            CancellationToken ct)
        {
            List<BaselineParcel> parcels = await context.BaselineParcels
                .Include(parcel => parcel.LandOwner)
                .ToListAsync(ct);
            Dictionary<string, BaselineParcel> lookup = new(StringComparer.OrdinalIgnoreCase);
            foreach (BaselineParcel parcel in parcels)
            {
                if (!string.IsNullOrWhiteSpace(parcel.FullUniqueParcelCode))
                    lookup[parcel.FullUniqueParcelCode.Trim()] = parcel;

                lookup[BuildParcelCode(parcel.MapSheetNo, parcel.ParcelNo)] = parcel;
            }

            return lookup;
        }

        private static BaselineParcel? TryFindBaselineParcel(
            IReadOnlyDictionary<string, BaselineParcel> lookup,
            string? mapSheetNo,
            string? parcelNo)
        {
            if (string.IsNullOrWhiteSpace(mapSheetNo) ||
                string.IsNullOrWhiteSpace(parcelNo))
            {
                return null;
            }

            return lookup.TryGetValue(BuildParcelCode(mapSheetNo, parcelNo), out BaselineParcel? parcel)
                ? parcel
                : null;
        }

        private static string BuildParcelCode(string? mapSheetNo, string? parcelNo)
        {
            return $"{(mapSheetNo ?? string.Empty).Trim().ToUpperInvariant()}::{(parcelNo ?? string.Empty).Trim().ToUpperInvariant()}";
        }

        private static string? SanitizeLayerName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            char[] invalid = Path.GetInvalidFileNameChars();
            string cleaned = new(
                value.Trim()
                    .Select(ch => invalid.Contains(ch) ? '_' : ch)
                    .ToArray());

            return string.IsNullOrWhiteSpace(cleaned) ? null : cleaned;
        }

        private static string BuildDescription(CadastralRawParcel parcel)
        {
            if (IsAnnotationObject(parcel))
            {
                string text = string.IsNullOrWhiteSpace(parcel.ParcelNo)
                    ? "empty text"
                    : $"text '{parcel.ParcelNo}'";
                return $"Imported cadastral annotation, {text}";
            }

            string sheet = string.IsNullOrWhiteSpace(parcel.MapSheetNo)
                ? "unknown sheet"
                : parcel.MapSheetNo;
            string parcelNo = string.IsNullOrWhiteSpace(parcel.ParcelNo)
                ? "unassigned parcel"
                : $"parcel {parcel.ParcelNo}";
            return $"Original cadastral {parcelNo}, map sheet {sheet}";
        }

        private static bool NeedsTransform(string source, string target)
        {
            return !string.Equals(
                NormalizeDefinition(ProjectCrsWktBuilder.SanitizeForProj(source)),
                NormalizeDefinition(ProjectCrsWktBuilder.SanitizeForProj(target)),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeDefinition(string value)
        {
            return value.Trim().Replace("\r", string.Empty).Replace("\n", string.Empty);
        }

        private static List<CadastralRawParcel> TransformParcels(
            IReadOnlyList<CadastralRawParcel> sourceParcels,
            string sourceDefinition,
            string targetDefinition)
        {
            GdalBootstrapper.ConfigureAll();
            using SpatialReference sourceSrs = CreateSpatialReference(sourceDefinition);
            using SpatialReference targetSrs = CreateSpatialReference(targetDefinition);
            using CoordinateTransformation transformation = new(sourceSrs, targetSrs);

            List<CadastralRawParcel> transformed = [];
            foreach (CadastralRawParcel sourceParcel in sourceParcels)
            {
                Geometry copy = sourceParcel.Geometry.Copy();
                copy.Apply(new CoordinateTransformFilter(transformation));
                copy.GeometryChanged();
                Geometry? valid = ValidateImportedGeometry(sourceParcel.ObjectType, copy);
                if (valid != null)
                    transformed.Add(sourceParcel with { Geometry = valid });
            }

            return transformed;
        }

        private static Geometry? ValidateImportedGeometry(string objectType, Geometry geometry)
        {
            if (string.Equals(objectType, "Polygon", StringComparison.OrdinalIgnoreCase))
                return BoundaryGeometryReaderHelpers.ValidatePolygonalGeometry(geometry);

            return geometry.IsEmpty ? null : geometry;
        }

        private static SpatialReference CreateSpatialReference(string definition)
        {
            definition = ProjectCrsWktBuilder.SanitizeForProj(definition);
            SpatialReference spatialReference = new(string.Empty);
            spatialReference.SetAxisMappingStrategy(
                AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);

            if (spatialReference.SetFromUserInput(definition) != 0)
            {
                string wkt = definition;
                if (spatialReference.ImportFromWkt(ref wkt) != 0)
                {
                    spatialReference.Dispose();
                    throw new InvalidOperationException(
                        $"Could not parse CRS definition '{definition}'.");
                }
            }

            spatialReference.SetAxisMappingStrategy(
                AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
            return spatialReference;
        }

        private static void NormalizeGeometryForCanvasDatabase(Geometry geometry)
        {
            geometry.SRID = 0;
            for (int index = 0; index < geometry.NumGeometries; index++)
                geometry.GetGeometryN(index).SRID = 0;
        }

        private static string BuildExceptionMessage(Exception ex)
        {
            List<string> messages = [];
            Exception? current = ex;
            while (current != null)
            {
                if (!string.IsNullOrWhiteSpace(current.Message) &&
                    !messages.Contains(current.Message))
                {
                    messages.Add(current.Message);
                }

                current = current.InnerException;
            }

            return string.Join(" ", messages);
        }

        private sealed class CoordinateTransformFilter : ICoordinateSequenceFilter
        {
            private readonly CoordinateTransformation _transformation;

            public CoordinateTransformFilter(CoordinateTransformation transformation)
            {
                _transformation = transformation;
            }

            public bool Done => false;
            public bool GeometryChanged => true;

            public void Filter(CoordinateSequence seq, int i)
            {
                double[] point = [seq.GetX(i), seq.GetY(i), 0.0];
                _transformation.TransformPoint(point);

                if (!double.IsFinite(point[0]) || !double.IsFinite(point[1]))
                    throw new InvalidOperationException("A cadastral coordinate could not be transformed.");

                seq.SetX(i, point[0]);
                seq.SetY(i, point[1]);
            }
        }

        private sealed record TargetLayerSpec(
            string Key,
            string Name,
            string LayerType);
    }
}
