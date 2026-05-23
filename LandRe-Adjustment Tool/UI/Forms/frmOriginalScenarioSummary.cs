using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.LandData;
using Land_Readjustment_Tool.Core.Models.Import;
using Land_Readjustment_Tool.Data;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.UI.Forms
{
    public sealed class frmOriginalScenarioSummary : Form
    {
        private const double SqmPerRopani = 508.73704704;
        private static readonly JsonSerializerOptions MetadataJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly ProjectSession _session;
        private readonly string _projectFilePath;
        private readonly TabControl _tabs = new();
        private readonly FlowLayoutPanel _metricPanel = new();
        private readonly Label _subtitle = new();
        private readonly Button _btnRefresh = new();
        private readonly Button _btnExport = new();
        private readonly Button _btnClose = new();
        private SummaryBook _summaryBook = new();
        private int _sqmPrecision = 3;

        public frmOriginalScenarioSummary(ProjectSession session, string projectFilePath)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _projectFilePath = projectFilePath ?? string.Empty;
            InitializeUi();
            Shown += async (_, _) => await LoadSummaryAsync();
        }

        private void InitializeUi()
        {
            Text = "Original Scenario Summary";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(1120, 720);
            Size = new Size(1260, 780);
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.FromArgb(244, 247, 251);

            Panel header = new()
            {
                Dock = DockStyle.Top,
                Height = 112,
                BackColor = Color.FromArgb(31, 45, 64),
                Padding = new Padding(22, 14, 22, 12)
            };

            Label title = new()
            {
                Text = "Original Scenario Summary",
                Dock = DockStyle.Top,
                Height = 34,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            _subtitle.Text = "Project database summary, checks, and original land-use scenario.";
            _subtitle.Dock = DockStyle.Top;
            _subtitle.Height = 24;
            _subtitle.ForeColor = Color.FromArgb(210, 222, 238);
            _subtitle.TextAlign = ContentAlignment.MiddleLeft;

            FlowLayoutPanel actions = new()
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };

            ConfigureButton(_btnClose, "Close", Color.FromArgb(82, 96, 113));
            ConfigureButton(_btnExport, "Export XLS", Color.FromArgb(32, 128, 92));
            ConfigureButton(_btnRefresh, "Refresh", Color.FromArgb(56, 108, 176));
            _btnClose.Click += (_, _) => Close();
            _btnRefresh.Click += async (_, _) => await LoadSummaryAsync();
            _btnExport.Click += (_, _) => ExportSummaryToExcel();
            actions.Controls.AddRange([_btnClose, _btnExport, _btnRefresh]);

            header.Controls.Add(actions);
            header.Controls.Add(_subtitle);
            header.Controls.Add(title);

            _metricPanel.Dock = DockStyle.Top;
            _metricPanel.Height = 118;
            _metricPanel.Padding = new Padding(14, 12, 14, 6);
            _metricPanel.BackColor = Color.FromArgb(244, 247, 251);
            _metricPanel.WrapContents = false;
            _metricPanel.AutoScroll = true;

            _tabs.Dock = DockStyle.Fill;
            _tabs.Padding = new Point(16, 7);
            _tabs.Appearance = TabAppearance.Normal;

            Controls.Add(_tabs);
            Controls.Add(_metricPanel);
            Controls.Add(header);
        }

        private static void ConfigureButton(Button button, string text, Color backColor)
        {
            button.Text = text;
            button.Width = 112;
            button.Height = 31;
            button.Margin = new Padding(8, 2, 0, 2);
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = backColor;
            button.ForeColor = Color.White;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }

        private async Task LoadSummaryAsync()
        {
            Cursor previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            _btnRefresh.Enabled = false;
            _btnExport.Enabled = false;

            try
            {
                _summaryBook = await BuildSummaryBookAsync();
                RenderSummaryBook();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not generate original scenario summary.\n\n{ex.Message}",
                    "Original Scenario Summary",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _btnRefresh.Enabled = true;
                _btnExport.Enabled = _summaryBook.Sheets.Count > 0;
                Cursor = previousCursor;
            }
        }

        private async Task<SummaryBook> BuildSummaryBookAsync()
        {
            AppDbContext context = _session.GetDbContext();

            var precisionSetting = await context.ProjectSettings
                .AsNoTracking()
                .Select(ps => (int?)ps.AreaSqmDecimalPlaces)
                .FirstOrDefaultAsync();
            _sqmPrecision = precisionSetting ?? 3;

            List<BaselineParcel> parcels = await context.BaselineParcels
                .AsNoTracking()
                .Include(parcel => parcel.LandOwner)
                .Include(parcel => parcel.CoOwners)
                    .ThenInclude(coOwner => coOwner.LandOwner)
                .Include(parcel => parcel.MalpotReference)
                .Include(parcel => parcel.ParcelFrontages)
                    .ThenInclude(frontage => frontage.Road)
                .Include(parcel => parcel.ParcelContributionSummary)
                .ToListAsync();

            List<LandOwner> owners = await context.LandOwners
                .AsNoTracking()
                .Include(owner => owner.BaselineParcels)
                .Include(owner => owner.BaselineCoOwnerships)
                .ToListAsync();

            List<CanvasObject> canvasObjects = await context.CanvasObjects
                .AsNoTracking()
                .Include(item => item.CanvasLayer)
                .ToListAsync();

            List<CadastralMapParcel> mapParcels = canvasObjects
                .Select(CreateCadastralMapParcel)
                .Where(item => item != null)
                .Cast<CadastralMapParcel>()
                .ToList();

            double recordArea = parcels.Sum(parcel => parcel.OriginalAreaSqm);
            double mapArea = mapParcels.Sum(parcel => parcel.AreaSqm);
            double boundaryArea = canvasObjects
                .Where(IsProjectBoundaryObject)
                .Sum(item => Math.Abs(item.Shape?.Area ?? 0.0));
            double scenarioBaseArea = boundaryArea > 0 ? boundaryArea : mapArea > 0 ? mapArea : recordArea;

            SummaryBook book = new()
            {
                ProjectName = Path.GetFileNameWithoutExtension(_projectFilePath),
                GeneratedAt = DateTime.Now,
                RecordParcelCount = parcels.Count,
                MapParcelCount = mapParcels.Count,
                BoundaryAreaSqm = boundaryArea,
                RecordAreaSqm = recordArea,
                MapAreaSqm = mapArea
            };

            AddOverview(book, parcels, owners, mapParcels, recordArea, mapArea, boundaryArea, scenarioBaseArea);
            AddAreaClassification(book, parcels);
            AddLandUseScenario(book, parcels, mapParcels, recordArea, mapArea, boundaryArea, scenarioBaseArea);
            AddOwnershipSummary(book, parcels, owners);
            AddLocationSummary(book, parcels);
            AddAssignmentAndSpatialSummary(book, parcels, mapParcels, recordArea, mapArea, boundaryArea);
            AddDataQualitySummary(book, parcels, owners, mapParcels);
            AddFrontageSummary(book, parcels);
            AddTenancySummary(book, parcels);
            AddContributionReadinessSummary(book, parcels);
            AddSourceAndLayerSummary(book, canvasObjects, mapParcels);

            return book;
        }

        private void AddOverview(
            SummaryBook book,
            List<BaselineParcel> parcels,
            List<LandOwner> owners,
            List<CadastralMapParcel> mapParcels,
            double recordArea,
            double mapArea,
            double boundaryArea,
            double scenarioBaseArea)
        {
            int assignedMapParcels = mapParcels.Count(parcel => parcel.BaselineParcelId.HasValue);
            int jointParcels = parcels.Count(parcel => parcel.CoOwners.Count > 0);
            int tenantParcels = parcels.Count(parcel => parcel.HasTenant);
            double discrepancy = scenarioBaseArea - recordArea;

            book.Metrics.AddRange(
            [
                new("Record Parcels", parcels.Count.ToString("N0"), "Original land records"),
                new("Map Parcels", mapParcels.Count.ToString("N0"), "Cadastral parcel polygons"),
                new("Boundary Area", FormatAreaCompact(boundaryArea), "BO / project boundary"),
                new("Record Area", FormatAreaCompact(recordArea), "Total private records"),
                new("Discrepancy", FormatAreaCompact(discrepancy), "Boundary/base minus record area", Math.Abs(discrepancy) < 0.01 ? GoodColor : WarnColor),
                new("Owners", owners.Count.ToString("N0"), "Unique owner records"),
                new("Assigned", FormatPercent(Percent(assignedMapParcels, mapParcels.Count)), "Map-to-record assignment", assignedMapParcels == mapParcels.Count ? GoodColor : InfoColor),
                new("Joint Parcels", jointParcels.ToString("N0"), "Parcels with co-owners"),
                new("Tenant Parcels", tenantParcels.ToString("N0"), "Parcels with tenant flag")
            ]);

            SummarySheet sheet = new("Dashboard", "High-level project indicators and reconciliation checks.");
            sheet.Columns.AddRange(DefaultColumns("Summary", "Value", "Remarks"));
            AddRow(sheet, "Project", book.ProjectName, _projectFilePath);
            AddRow(sheet, "Generated At", book.GeneratedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), string.Empty);
            AddRow(sheet, "Record Parcel Count", parcels.Count, "Baseline parcel records");
            AddRow(sheet, "Map Parcel Count", mapParcels.Count, "Cadastral parcel polygons");
            AddRow(sheet, "Assigned Map Parcels", assignedMapParcels, "Canvas parcels linked to records");
            AddRow(sheet, "Unassigned Map Parcels", mapParcels.Count - assignedMapParcels, "Need assignment review");
            AddRow(sheet, "Record Area", recordArea, FormatAreaLong(recordArea));
            AddRow(sheet, "Map Parcel Area", mapArea, FormatAreaLong(mapArea));
            AddRow(sheet, "Boundary Area", boundaryArea, FormatAreaLong(boundaryArea));
            AddRow(sheet, "Boundary vs Record Discrepancy", discrepancy, Math.Abs(discrepancy) < 0.01 ? "Check OK" : "Review boundary/map/records");
            AddRow(sheet, "Owners", owners.Count, "Unique land owner rows");
            AddRow(sheet, "Joint Ownership Parcels", jointParcels, "Records with co-owners");
            AddRow(sheet, "Tenant Parcels", tenantParcels, "Records marked with tenant");

            sheet.Charts.Add(new ChartModel(
                "Area Reconciliation",
                "Compare the three main area sources.",
                ChartKind.Bar,
                [
                    new("Record", recordArea, AccentBlue),
                    new("Map", mapArea, AccentTeal),
                    new("Boundary", boundaryArea, AccentRose)
                ]));

            sheet.Charts.Add(new ChartModel(
                "Assignment Progress",
                "Assigned vs pending cadastral map parcels.",
                ChartKind.Donut,
                [
                    new("Assigned", assignedMapParcels, GoodColor),
                    new("Unassigned", Math.Max(0, mapParcels.Count - assignedMapParcels), WarnColor)
                ]));

            book.Sheets.Add(sheet);
        }

        private void AddAreaClassification(SummaryBook book, List<BaselineParcel> parcels)
        {
            var bands = new[]
            {
                new AreaBand("< 2.5 aana", 0, AanaToSqm(2.5)),
                new AreaBand("2.5 aana - 4 aana", AanaToSqm(2.5), AanaToSqm(4)),
                new AreaBand("4 aana - 8 aana", AanaToSqm(4), AanaToSqm(8)),
                new AreaBand("8 aana - 1 Ropani", AanaToSqm(8), RopaniToSqm(1)),
                new AreaBand("1 ropani - 2 ropani", RopaniToSqm(1), RopaniToSqm(2)),
                new AreaBand("2 ropani - 3 ropani", RopaniToSqm(2), RopaniToSqm(3)),
                new AreaBand("3 ropani - 5 Ropani", RopaniToSqm(3), RopaniToSqm(5)),
                new AreaBand("5 ropani - 10 Ropani", RopaniToSqm(5), RopaniToSqm(10)),
                new AreaBand("10 ropani - 20 Ropani", RopaniToSqm(10), RopaniToSqm(20)),
                new AreaBand(">20 Ropani", RopaniToSqm(20), double.MaxValue)
            };

            SummarySheet sheet = new("Area Classification", "Parcel count and area by commonly used Nepali area bands.");
            sheet.Columns.AddRange(DefaultColumns("S.N.", "Area Category", "Parcel Count", "Area (sq.m)", "Area (Ropani)", "Remarks"));

            int serial = 1;
            foreach (AreaBand band in bands)
            {
                var group = parcels
                    .Where(parcel => parcel.OriginalAreaSqm >= band.MinSqm &&
                                     (band.MaxSqm == double.MaxValue || parcel.OriginalAreaSqm < band.MaxSqm))
                    .ToList();
                double area = group.Sum(parcel => parcel.OriginalAreaSqm);
                AddRow(sheet, serial++, band.Name, group.Count, area, ToRopani(area), string.Empty);
            }

            AddTotalRow(sheet, string.Empty, "Total", parcels.Count, parcels.Sum(parcel => parcel.OriginalAreaSqm), ToRopani(parcels.Sum(parcel => parcel.OriginalAreaSqm)), string.Empty);

            sheet.Charts.Add(new ChartModel(
                "Parcel Count by Area",
                "Distribution of original parcel sizes.",
                ChartKind.Bar,
                bands.Select(band =>
                {
                    int count = parcels.Count(parcel => parcel.OriginalAreaSqm >= band.MinSqm &&
                                                        (band.MaxSqm == double.MaxValue || parcel.OriginalAreaSqm < band.MaxSqm));
                    return new ChartSegment(band.Name, count, PickColor(sheet.Charts.Count + count));
                }).ToList()));

            book.Sheets.Add(sheet);
        }

        private void AddLandUseScenario(
            SummaryBook book,
            List<BaselineParcel> parcels,
            List<CadastralMapParcel> mapParcels,
            double recordArea,
            double mapArea,
            double boundaryArea,
            double scenarioBaseArea)
        {
            var classes = new[]
            {
                new { Name = "Total Private Plots (Land Record)", Predicate = (Func<BaselineParcel, bool>)(parcel => ClassifyLandUse(parcel) == LandUseClass.Private) },
                new { Name = "Total Public Land (Govt.)", Predicate = (Func<BaselineParcel, bool>)(parcel => ClassifyLandUse(parcel) == LandUseClass.Public) },
                new { Name = "Total Existing Road (Govt.)", Predicate = (Func<BaselineParcel, bool>)(parcel => ClassifyLandUse(parcel) == LandUseClass.Road) },
                new { Name = "Total Guthi Land", Predicate = (Func<BaselineParcel, bool>)(parcel => ClassifyLandUse(parcel) == LandUseClass.Guthi) }
            };

            SummarySheet sheet = new("Original Land Use Scenario", "Original scenario based on land-use and ownership classification.");
            sheet.Columns.AddRange(DefaultColumns("S.N.", "Description (Land Use)", "Area (sq.m)", "Area (Ropani)", "Percentage (%)", "Remarks"));

            int serial = 1;
            double classifiedArea = 0;
            foreach (var item in classes)
            {
                double area = parcels.Where(item.Predicate).Sum(parcel => parcel.OriginalAreaSqm);
                classifiedArea += area;
                AddRow(sheet, serial++, item.Name, area, ToRopani(area), Percent(area, scenarioBaseArea), string.Empty);
            }

            double discrepancy = scenarioBaseArea - classifiedArea;
            AddRow(sheet, serial++, "Discrepancy", discrepancy, ToRopani(discrepancy), Percent(discrepancy, scenarioBaseArea), Math.Abs(discrepancy) < 0.01 ? "Check OK" : "Review");
            AddTotalRow(sheet, string.Empty, "Total Land Area (BO area)", scenarioBaseArea, ToRopani(scenarioBaseArea), Percent(scenarioBaseArea, scenarioBaseArea), boundaryArea > 0 ? "Boundary area" : mapArea > 0 ? "Map parcel area" : "Record area");

            sheet.Charts.Add(new ChartModel(
                "Original Land Use",
                "Area share by scenario classification.",
                ChartKind.Donut,
                sheet.Rows
                    .Where(row => !row.IsTotal && Convert.ToDouble(row.Values[2], CultureInfo.InvariantCulture) > 0)
                    .Select((row, index) => new ChartSegment(row.Values[1]?.ToString() ?? string.Empty, Convert.ToDouble(row.Values[2], CultureInfo.InvariantCulture), PickColor(index)))
                    .ToList()));

            sheet.Charts.Add(new ChartModel(
                "Record / Map / Boundary",
                "Useful area sources for discrepancy review.",
                ChartKind.Bar,
                [
                    new("Land Record", recordArea, AccentBlue),
                    new("Cadastral Map", mapParcels.Sum(parcel => parcel.AreaSqm), AccentTeal),
                    new("Boundary", boundaryArea, AccentRose)
                ]));

            book.Sheets.Add(sheet);
        }

        private void AddOwnershipSummary(SummaryBook book, List<BaselineParcel> parcels, List<LandOwner> owners)
        {
            SummarySheet sheet = new("Ownership", "Owner, parcel holding, joint ownership, and contact completeness.");
            sheet.Columns.AddRange(DefaultColumns("S.N.", "Ownership Summary", "Count", "Area (sq.m)", "Area (Ropani)", "Remarks"));

            int singleOwnerParcels = parcels.Count(parcel => parcel.CoOwners.Count == 0);
            int jointOwnerParcels = parcels.Count(parcel => parcel.CoOwners.Count > 0);
            int ownersWithMultipleParcels = owners.Count(owner => owner.BaselineParcels.Count + owner.BaselineCoOwnerships.Count > 1);
            int missingCitizenship = owners.Count(owner => string.IsNullOrWhiteSpace(owner.CitizenshipNumber));
            int missingContact = owners.Count(owner => string.IsNullOrWhiteSpace(owner.ContactNumber) && string.IsNullOrWhiteSpace(owner.Email));
            int manualReviewOwners = owners.Count(owner => owner.NeedsManualReview);

            AddRow(sheet, 1, "Total Owners", owners.Count, parcels.Sum(parcel => parcel.OriginalAreaSqm), ToRopani(parcels.Sum(parcel => parcel.OriginalAreaSqm)), "Primary owners");
            AddRow(sheet, 2, "Single-owner Parcels", singleOwnerParcels, parcels.Where(parcel => parcel.CoOwners.Count == 0).Sum(parcel => parcel.OriginalAreaSqm), ToRopani(parcels.Where(parcel => parcel.CoOwners.Count == 0).Sum(parcel => parcel.OriginalAreaSqm)), string.Empty);
            AddRow(sheet, 3, "Joint Ownership Parcels", jointOwnerParcels, parcels.Where(parcel => parcel.CoOwners.Count > 0).Sum(parcel => parcel.OriginalAreaSqm), ToRopani(parcels.Where(parcel => parcel.CoOwners.Count > 0).Sum(parcel => parcel.OriginalAreaSqm)), "Has co-owners");
            AddRow(sheet, 4, "Owners With Multiple Parcels", ownersWithMultipleParcels, 0, 0, "Primary or co-owner holdings");
            AddRow(sheet, 5, "Owners Missing Citizenship", missingCitizenship, 0, 0, "Data quality");
            AddRow(sheet, 6, "Owners Missing Contact", missingContact, 0, 0, "Data quality");
            AddRow(sheet, 7, "Owners Needing Manual Review", manualReviewOwners, 0, 0, "Deduplication review");

            sheet.Charts.Add(new ChartModel(
                "Ownership Mix",
                "Single vs joint ownership parcels.",
                ChartKind.Donut,
                [
                    new("Single", singleOwnerParcels, AccentBlue),
                    new("Joint", jointOwnerParcels, AccentRose)
                ]));

            sheet.Charts.Add(CreateTopOwnersChart(parcels));
            book.Sheets.Add(sheet);
        }

        private void AddLocationSummary(SummaryBook book, List<BaselineParcel> parcels)
        {
            SummarySheet sheet = new("Location", "Province, district, municipality, ward, and map-sheet distribution.");
            sheet.Columns.AddRange(DefaultColumns("S.N.", "Category", "Name", "Parcel Count", "Area (sq.m)", "Area (Ropani)", "Percentage (%)"));

            int serial = 1;
            AddGroupedRows(sheet, ref serial, "Province", parcels.GroupBy(parcel => BlankAsUnknown(parcel.Province)));
            AddGroupedRows(sheet, ref serial, "District", parcels.GroupBy(parcel => BlankAsUnknown(parcel.District)));
            AddGroupedRows(sheet, ref serial, "Municipality", parcels.GroupBy(parcel => BlankAsUnknown(parcel.Municipality)));
            AddGroupedRows(sheet, ref serial, "Ward", parcels.GroupBy(parcel => BlankAsUnknown(parcel.WardNo)));
            AddGroupedRows(sheet, ref serial, "Map Sheet", parcels.GroupBy(parcel => BlankAsUnknown(parcel.MapSheetNo)));

            sheet.Charts.Add(CreateGroupedCountChart("Parcels by Ward", parcels.GroupBy(parcel => BlankAsUnknown(parcel.WardNo))));
            sheet.Charts.Add(CreateGroupedCountChart("Parcels by Map Sheet", parcels.GroupBy(parcel => BlankAsUnknown(parcel.MapSheetNo))));
            book.Sheets.Add(sheet);
        }

        private void AddAssignmentAndSpatialSummary(
            SummaryBook book,
            List<BaselineParcel> parcels,
            List<CadastralMapParcel> mapParcels,
            double recordArea,
            double mapArea,
            double boundaryArea)
        {
            int assigned = mapParcels.Count(parcel => parcel.BaselineParcelId.HasValue);
            int unassigned = mapParcels.Count - assigned;
            HashSet<int> assignedRecordIds = mapParcels
                .Where(parcel => parcel.BaselineParcelId.HasValue)
                .Select(parcel => parcel.BaselineParcelId!.Value)
                .ToHashSet();
            int recordWithoutMap = parcels.Count(parcel => !assignedRecordIds.Contains(parcel.Id));

            SummarySheet sheet = new("Assignment & Spatial", "Cadastral map assignment, parcel counts, area discrepancy, and boundary checks.");
            sheet.Columns.AddRange(DefaultColumns("S.N.", "Check", "Record Value", "Map/Boundary Value", "Difference", "Remarks"));
            AddRow(sheet, 1, "Parcel Count", parcels.Count, mapParcels.Count, mapParcels.Count - parcels.Count, "Map parcels minus record parcels");
            AddRow(sheet, 2, "Assigned Map Parcels", assigned, mapParcels.Count, Percent(assigned, mapParcels.Count), "Assignment progress %");
            AddRow(sheet, 3, "Unassigned Map Parcels", unassigned, mapParcels.Count, Percent(unassigned, mapParcels.Count), unassigned == 0 ? "Check OK" : "Assign pending");
            AddRow(sheet, 4, "Record Parcels Without Map Link", recordWithoutMap, parcels.Count, Percent(recordWithoutMap, parcels.Count), recordWithoutMap == 0 ? "Check OK" : "Spatial link pending");
            AddRow(sheet, 5, "Record Area vs Map Area", recordArea, mapArea, mapArea - recordArea, "sq.m difference");
            AddRow(sheet, 6, "Boundary Area vs Record Area", recordArea, boundaryArea, boundaryArea - recordArea, boundaryArea > 0 ? "Uses BO boundary" : "No boundary area");
            AddRow(sheet, 7, "Boundary Area vs Map Area", mapArea, boundaryArea, boundaryArea - mapArea, boundaryArea > 0 ? "Map fit inside BO review" : "No boundary area");

            sheet.Charts.Add(new ChartModel(
                "Spatial Assignment",
                "Map parcel assignment status.",
                ChartKind.Donut,
                [
                    new("Assigned", assigned, GoodColor),
                    new("Unassigned", unassigned, WarnColor)
                ]));
            sheet.Charts.Add(new ChartModel(
                "Area Sources",
                "Record, map and boundary area.",
                ChartKind.Bar,
                [
                    new("Record", recordArea, AccentBlue),
                    new("Map", mapArea, AccentTeal),
                    new("Boundary", boundaryArea, AccentRose)
                ]));
            book.Sheets.Add(sheet);
        }

        private void AddDataQualitySummary(
            SummaryBook book,
            List<BaselineParcel> parcels,
            List<LandOwner> owners,
            List<CadastralMapParcel> mapParcels)
        {
            SummarySheet sheet = new("Data Quality", "Completeness checks that usually affect calculation, assignment, and reporting.");
            sheet.Columns.AddRange(DefaultColumns("S.N.", "Quality Check", "Issue Count", "Affected Area (sq.m)", "Priority", "Remarks"));

            int serial = 1;
            AddQualityRow(sheet, serial++, "Missing Parcel No.", parcels.Where(parcel => string.IsNullOrWhiteSpace(parcel.ParcelNo)).ToList(), "High", "Required for matching");
            AddQualityRow(sheet, serial++, "Missing Map Sheet No.", parcels.Where(parcel => string.IsNullOrWhiteSpace(parcel.MapSheetNo)).ToList(), "High", "Required for matching");
            AddQualityRow(sheet, serial++, "Zero / Negative Area", parcels.Where(parcel => parcel.OriginalAreaSqm <= 0).ToList(), "High", "Area must be valid");
            AddQualityRow(sheet, serial++, "Missing Land Use", parcels.Where(parcel => string.IsNullOrWhiteSpace(parcel.LandUse)).ToList(), "Medium", "Affects scenario summary");
            AddQualityRow(sheet, serial++, "Missing Owner Citizenship", parcels.Where(parcel => string.IsNullOrWhiteSpace(parcel.LandOwner?.CitizenshipNumber)).ToList(), "Medium", "Owner identity completeness");
            AddQualityRow(sheet, serial++, "Missing Owner Contact", parcels.Where(parcel => string.IsNullOrWhiteSpace(parcel.LandOwner?.ContactNumber) && string.IsNullOrWhiteSpace(parcel.LandOwner?.Email)).ToList(), "Low", "Communication readiness");
            AddQualityRow(sheet, serial++, "Missing Location Fields", parcels.Where(parcel => string.IsNullOrWhiteSpace(parcel.District) || string.IsNullOrWhiteSpace(parcel.Municipality) || string.IsNullOrWhiteSpace(parcel.WardNo)).ToList(), "Low", "Report completeness");
            AddRow(sheet, serial++, "Unassigned Map Parcels", mapParcels.Count(parcel => !parcel.BaselineParcelId.HasValue), 0, "High", "Cadastral object not linked to parcel record");
            AddRow(sheet, serial++, "Owners Needing Manual Review", owners.Count(owner => owner.NeedsManualReview), 0, "Medium", "Deduplication review flag");

            sheet.Charts.Add(new ChartModel(
                "Quality Issues",
                "Issue counts by check.",
                ChartKind.Bar,
                sheet.Rows
                    .Where(row => Convert.ToDouble(row.Values[2], CultureInfo.InvariantCulture) > 0)
                    .Select((row, index) => new ChartSegment(row.Values[1]?.ToString() ?? string.Empty, Convert.ToDouble(row.Values[2], CultureInfo.InvariantCulture), PickColor(index)))
                    .ToList()));
            book.Sheets.Add(sheet);
        }

        private void AddFrontageSummary(SummaryBook book, List<BaselineParcel> parcels)
        {
            var frontages = parcels.SelectMany(parcel => parcel.ParcelFrontages.Select(frontage => new { parcel, frontage })).ToList();
            SummarySheet sheet = new("Road Frontage", "Frontage availability, road-facing parcels, direction, and frontage length.");
            sheet.Columns.AddRange(DefaultColumns("S.N.", "Road / Direction", "Parcel Count", "Frontage Length (m)", "Area (sq.m)", "Remarks"));

            int serial = 1;
            AddRow(sheet, serial++, "Parcels With Frontage", parcels.Count(parcel => parcel.ParcelFrontages.Count > 0), frontages.Sum(item => item.frontage.FrontageLength ?? 0), parcels.Where(parcel => parcel.ParcelFrontages.Count > 0).Sum(parcel => parcel.OriginalAreaSqm), string.Empty);
            AddRow(sheet, serial++, "Parcels Without Frontage", parcels.Count(parcel => parcel.ParcelFrontages.Count == 0), 0, parcels.Where(parcel => parcel.ParcelFrontages.Count == 0).Sum(parcel => parcel.OriginalAreaSqm), "May need frontage assignment");

            foreach (var group in frontages.GroupBy(item => BlankAsUnknown(item.frontage.Road?.RoadName)).OrderByDescending(group => group.Count()).ThenBy(group => group.Key))
            {
                AddRow(sheet, serial++, group.Key, group.Select(item => item.parcel.Id).Distinct().Count(), group.Sum(item => item.frontage.FrontageLength ?? 0), group.Select(item => item.parcel).DistinctBy(parcel => parcel.Id).Sum(parcel => parcel.OriginalAreaSqm), "By road");
            }

            foreach (var group in frontages.GroupBy(item => BlankAsUnknown(item.frontage.FacingDirection)).OrderBy(group => group.Key))
            {
                AddRow(sheet, serial++, $"Facing {group.Key}", group.Select(item => item.parcel.Id).Distinct().Count(), group.Sum(item => item.frontage.FrontageLength ?? 0), group.Select(item => item.parcel).DistinctBy(parcel => parcel.Id).Sum(parcel => parcel.OriginalAreaSqm), "By direction");
            }

            sheet.Charts.Add(new ChartModel(
                "Frontage Coverage",
                "Parcels with and without road frontage.",
                ChartKind.Donut,
                [
                    new("With Frontage", parcels.Count(parcel => parcel.ParcelFrontages.Count > 0), GoodColor),
                    new("Without Frontage", parcels.Count(parcel => parcel.ParcelFrontages.Count == 0), WarnColor)
                ]));
            sheet.Charts.Add(CreateGroupedCountChart("Parcels by Road", frontages.GroupBy(item => BlankAsUnknown(item.frontage.Road?.RoadName)), item => item.parcel.Id));
            book.Sheets.Add(sheet);
        }

        private void AddTenancySummary(SummaryBook book, List<BaselineParcel> parcels)
        {
            SummarySheet sheet = new("Tenancy", "Tenant presence and tenant-name completeness.");
            sheet.Columns.AddRange(DefaultColumns("S.N.", "Tenancy Summary", "Parcel Count", "Area (sq.m)", "Area (Ropani)", "Remarks"));
            var tenantParcels = parcels.Where(parcel => parcel.HasTenant).ToList();
            var noTenantParcels = parcels.Where(parcel => !parcel.HasTenant).ToList();
            AddRow(sheet, 1, "Has Tenant", tenantParcels.Count, tenantParcels.Sum(parcel => parcel.OriginalAreaSqm), ToRopani(tenantParcels.Sum(parcel => parcel.OriginalAreaSqm)), string.Empty);
            AddRow(sheet, 2, "No Tenant", noTenantParcels.Count, noTenantParcels.Sum(parcel => parcel.OriginalAreaSqm), ToRopani(noTenantParcels.Sum(parcel => parcel.OriginalAreaSqm)), string.Empty);
            AddRow(sheet, 3, "Tenant Flag But Missing Name", tenantParcels.Count(parcel => string.IsNullOrWhiteSpace(parcel.TenantName)), tenantParcels.Where(parcel => string.IsNullOrWhiteSpace(parcel.TenantName)).Sum(parcel => parcel.OriginalAreaSqm), ToRopani(tenantParcels.Where(parcel => string.IsNullOrWhiteSpace(parcel.TenantName)).Sum(parcel => parcel.OriginalAreaSqm)), "Data cleanup");
            sheet.Charts.Add(new ChartModel(
                "Tenancy",
                "Tenant flag distribution.",
                ChartKind.Donut,
                [
                    new("Tenant", tenantParcels.Count, AccentRose),
                    new("No Tenant", noTenantParcels.Count, AccentBlue)
                ]));
            book.Sheets.Add(sheet);
        }

        private void AddContributionReadinessSummary(SummaryBook book, List<BaselineParcel> parcels)
        {
            SummarySheet sheet = new("Contribution Readiness", "Original records prepared for later contribution and replotting calculations.");
            sheet.Columns.AddRange(DefaultColumns("S.N.", "Contribution Summary", "Parcel Count", "Area (sq.m)", "Area (Ropani)", "Remarks"));
            var withEffective = parcels.Where(parcel => parcel.EffectiveAreaSqm.HasValue).ToList();
            var withoutEffective = parcels.Where(parcel => !parcel.EffectiveAreaSqm.HasValue).ToList();
            var withSummary = parcels.Where(parcel => parcel.ParcelContributionSummary != null).ToList();
            var finalized = parcels.Where(parcel => parcel.ParcelContributionSummary?.IsFinalized == true).ToList();
            AddRow(sheet, 1, "Effective Area Available", withEffective.Count, withEffective.Sum(parcel => parcel.EffectiveAreaSqm ?? 0), ToRopani(withEffective.Sum(parcel => parcel.EffectiveAreaSqm ?? 0)), string.Empty);
            AddRow(sheet, 2, "Effective Area Missing", withoutEffective.Count, withoutEffective.Sum(parcel => parcel.OriginalAreaSqm), ToRopani(withoutEffective.Sum(parcel => parcel.OriginalAreaSqm)), "Calculation pending");
            AddRow(sheet, 3, "Contribution Calculated", withSummary.Count, withSummary.Sum(parcel => parcel.ParcelContributionSummary?.TotalContributionSqm ?? 0), ToRopani(withSummary.Sum(parcel => parcel.ParcelContributionSummary?.TotalContributionSqm ?? 0)), string.Empty);
            AddRow(sheet, 4, "Contribution Finalized", finalized.Count, finalized.Sum(parcel => parcel.ParcelContributionSummary?.NetReturnableAreaSqm ?? 0), ToRopani(finalized.Sum(parcel => parcel.ParcelContributionSummary?.NetReturnableAreaSqm ?? 0)), string.Empty);
            AddRow(sheet, 5, "Manual Effective Area", parcels.Count(parcel => parcel.IsEffectiveAreaManual), parcels.Where(parcel => parcel.IsEffectiveAreaManual).Sum(parcel => parcel.EffectiveAreaSqm ?? parcel.OriginalAreaSqm), ToRopani(parcels.Where(parcel => parcel.IsEffectiveAreaManual).Sum(parcel => parcel.EffectiveAreaSqm ?? parcel.OriginalAreaSqm)), "Manual override");
            sheet.Charts.Add(new ChartModel(
                "Contribution Readiness",
                "Calculation state by parcel count.",
                ChartKind.Bar,
                [
                    new("Effective Area", withEffective.Count, AccentBlue),
                    new("Missing Effective", withoutEffective.Count, WarnColor),
                    new("Calculated", withSummary.Count, AccentTeal),
                    new("Finalized", finalized.Count, GoodColor)
                ]));
            book.Sheets.Add(sheet);
        }

        private void AddSourceAndLayerSummary(SummaryBook book, List<CanvasObject> canvasObjects, List<CadastralMapParcel> mapParcels)
        {
            SummarySheet sheet = new("Source & Layers", "Canvas object counts by layer, source, and assignment status.");
            sheet.Columns.AddRange(DefaultColumns("S.N.", "Layer / Source", "Object Count", "Parcel Count", "Area (sq.m)", "Remarks"));

            int serial = 1;
            foreach (var group in canvasObjects.GroupBy(item => item.CanvasLayer?.Name ?? "Unknown Layer").OrderByDescending(group => group.Count()))
            {
                double area = group.Sum(item => Math.Abs(item.Shape?.Area ?? 0.0));
                int parcelCount = group.Count(item => IsCadastralParcelObject(item));
                AddRow(sheet, serial++, group.Key, group.Count(), parcelCount, area, group.FirstOrDefault()?.CanvasLayer?.LayerType ?? string.Empty);
            }

            foreach (var group in mapParcels.GroupBy(parcel => BlankAsUnknown(parcel.SourceFileName)).OrderByDescending(group => group.Count()))
            {
                AddRow(sheet, serial++, $"Source: {group.Key}", group.Count(), group.Count(), group.Sum(parcel => parcel.AreaSqm), "Imported cadastral parcels");
            }

            sheet.Charts.Add(CreateGroupedCountChart("Objects by Layer", canvasObjects.GroupBy(item => item.CanvasLayer?.Name ?? "Unknown Layer")));
            sheet.Charts.Add(CreateGroupedCountChart("Cadastral Parcels by Source", mapParcels.GroupBy(parcel => BlankAsUnknown(parcel.SourceFileName))));
            book.Sheets.Add(sheet);
        }

        private void RenderSummaryBook()
        {
            _subtitle.Text = $"{_summaryBook.ProjectName} | Generated {_summaryBook.GeneratedAt:yyyy-MM-dd HH:mm}";
            RenderMetrics();

            _tabs.SuspendLayout();
            try
            {
                _tabs.TabPages.Clear();
                foreach (SummarySheet sheet in _summaryBook.Sheets)
                {
                    _tabs.TabPages.Add(CreateTabPage(sheet));
                }
            }
            finally
            {
                _tabs.ResumeLayout();
            }
        }

        private void RenderMetrics()
        {
            _metricPanel.SuspendLayout();
            try
            {
                _metricPanel.Controls.Clear();
                foreach (MetricCard metric in _summaryBook.Metrics)
                {
                    _metricPanel.Controls.Add(CreateMetricCard(metric));
                }
            }
            finally
            {
                _metricPanel.ResumeLayout();
            }
        }

        private Control CreateMetricCard(MetricCard metric)
        {
            Panel card = new()
            {
                Width = 178,
                Height = 88,
                Margin = new Padding(7, 0, 7, 8),
                BackColor = Color.White,
                Padding = new Padding(12, 9, 12, 8)
            };
            card.Paint += (_, e) =>
            {
                using Pen pen = new(Color.FromArgb(222, 229, 238));
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                using Brush accent = new SolidBrush(metric.AccentColor);
                e.Graphics.FillRectangle(accent, 0, 0, 4, card.Height);
            };

            Label value = new()
            {
                Text = metric.Value,
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(29, 43, 61)
            };
            Label label = new()
            {
                Text = metric.Label,
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(67, 79, 96)
            };
            Label hint = new()
            {
                Text = metric.Hint,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(105, 116, 130),
                AutoEllipsis = true
            };
            card.Controls.Add(hint);
            card.Controls.Add(label);
            card.Controls.Add(value);
            return card;
        }

        private TabPage CreateTabPage(SummarySheet sheet)
        {
            TabPage page = new(sheet.Title)
            {
                BackColor = Color.FromArgb(244, 247, 251),
                Padding = new Padding(12)
            };

            SplitContainer split = new()
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                FixedPanel = FixedPanel.Panel2,
                SplitterDistance = Math.Max(300, Height - 360)
            };

            Panel top = new()
            {
                Dock = DockStyle.Top,
                Height = 42,
                Padding = new Padding(2, 0, 2, 8),
                BackColor = page.BackColor
            };

            Label description = new()
            {
                Text = sheet.Description,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(83, 96, 113),
                TextAlign = ContentAlignment.MiddleLeft
            };
            top.Controls.Add(description);

            DataGridView grid = CreateGrid(sheet);
            Panel gridPanel = new() { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            gridPanel.Controls.Add(grid);
            split.Panel1.Controls.Add(gridPanel);
            split.Panel1.Controls.Add(top);

            FlowLayoutPanel charts = new()
            {
                Dock = DockStyle.Fill,
                BackColor = page.BackColor,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 10, 0, 0)
            };
            foreach (ChartModel chart in sheet.Charts.Where(chart => chart.Segments.Any(segment => Math.Abs(segment.Value) > 0.0001)))
            {
                charts.Controls.Add(new SummaryChartPanel(chart)
                {
                    Width = 370,
                    Height = 210,
                    Margin = new Padding(0, 0, 12, 0)
                });
            }

            if (charts.Controls.Count == 0)
            {
                charts.Controls.Add(new Label
                {
                    Text = "No chart data available for this summary.",
                    ForeColor = Color.FromArgb(100, 110, 124),
                    AutoSize = false,
                    Width = 420,
                    Height = 40,
                    TextAlign = ContentAlignment.MiddleLeft
                });
            }

            split.Panel2.Controls.Add(charts);
            page.Controls.Add(split);
            return page;
        }

        private static DataGridView CreateGrid(SummarySheet sheet)
        {
            DataGridView grid = new()
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(229, 234, 241),
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                ColumnHeadersHeight = 34
            };

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 65, 91);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 235, 255);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(28, 42, 60);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 252, 255);

            foreach (SummaryColumn column in sheet.Columns)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = column.Name,
                    HeaderText = column.Header,
                    Width = column.Width,
                    AutoSizeMode = column.Fill ? DataGridViewAutoSizeColumnMode.Fill : DataGridViewAutoSizeColumnMode.None,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = { Alignment = column.Alignment }
                });
            }

            foreach (SummaryRow row in sheet.Rows)
            {
                int rowIndex = grid.Rows.Add(row.Values.Select(FormatCellValue).ToArray());
                DataGridViewRow gridRow = grid.Rows[rowIndex];
                if (row.IsTotal)
                {
                    gridRow.DefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
                    gridRow.DefaultCellStyle.BackColor = Color.FromArgb(235, 244, 239);
                    gridRow.DefaultCellStyle.ForeColor = Color.FromArgb(24, 89, 63);
                }
            }

            grid.ClearSelection();
            return grid;
        }

        private void ExportSummaryToExcel()
        {
            using SaveFileDialog dialog = new()
            {
                Title = "Export Original Scenario Summary",
                FileName = $"Original Scenario Summary - {_summaryBook.ProjectName}.xls",
                Filter = "Excel Workbook (*.xls)|*.xls",
                RestoreDirectory = true,
                OverwritePrompt = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                File.WriteAllText(dialog.FileName, BuildExcelHtml(_summaryBook), Encoding.UTF8);
                MessageBox.Show(
                    "Summary exported successfully. Excel can open this .xls workbook.",
                    "Export Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not export the summary.\n\n{ex.Message}",
                    "Export Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static string BuildExcelHtml(SummaryBook book)
        {
            StringBuilder html = new();
            html.AppendLine("<html><head><meta charset=\"utf-8\">");
            html.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;} table{border-collapse:collapse;margin-bottom:22px;} th{background:#2d415b;color:white;font-weight:700;} th,td{border:1px solid #9aa7b5;padding:5px 8px;} .title{font-size:18px;font-weight:700;color:#1f2d40;} .total{font-weight:700;background:#ebf4ef;color:#18593f;} .sheet{font-size:15px;font-weight:700;margin-top:18px;color:#1f2d40;}</style>");
            html.AppendLine("</head><body>");
            html.AppendLine($"<div class=\"title\">{Html(book.ProjectName)} - Original Scenario Summary</div>");
            html.AppendLine($"<div>Generated: {Html(book.GeneratedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture))}</div>");

            foreach (SummarySheet sheet in book.Sheets)
            {
                html.AppendLine($"<div class=\"sheet\">{Html(sheet.Title)}</div>");
                html.AppendLine($"<div>{Html(sheet.Description)}</div>");
                html.AppendLine("<table>");
                html.AppendLine("<tr>");
                foreach (SummaryColumn column in sheet.Columns)
                    html.AppendLine($"<th>{Html(column.Header)}</th>");
                html.AppendLine("</tr>");

                foreach (SummaryRow row in sheet.Rows)
                {
                    html.AppendLine(row.IsTotal ? "<tr class=\"total\">" : "<tr>");
                    foreach (object? value in row.Values)
                        html.AppendLine($"<td>{Html(FormatCellValue(value))}</td>");
                    html.AppendLine("</tr>");
                }

                html.AppendLine("</table>");
            }

            html.AppendLine("</body></html>");
            return html.ToString();
        }

        private static string Html(string? value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }

        private static CadastralMapParcel? CreateCadastralMapParcel(CanvasObject canvasObject)
        {
            CadastralCanvasMetadata? metadata = ReadMetadata(canvasObject.GeometryMetadataJson);
            if (metadata == null && !IsCadastralParcelObject(canvasObject))
                return null;

            return new CadastralMapParcel(
                canvasObject.Id,
                canvasObject.BaselineParcelId ?? metadata?.BaselineParcelId,
                FirstNonEmpty(metadata?.MapSheetNo, string.Empty),
                FirstNonEmpty(metadata?.ParcelNo, string.Empty),
                metadata?.AssignmentStatus ?? (canvasObject.BaselineParcelId.HasValue ? "Assigned" : "Unassigned"),
                Math.Abs(canvasObject.Shape?.Area ?? metadata?.CalculatedAreaSqm ?? 0.0),
                metadata?.SourceFileName ?? string.Empty,
                metadata?.SourceLayer ?? canvasObject.CanvasLayer?.Name ?? string.Empty);
        }

        private static CadastralCanvasMetadata? ReadMetadata(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                CadastralCanvasMetadata? metadata = JsonSerializer.Deserialize<CadastralCanvasMetadata>(json, MetadataJsonOptions);
                return string.Equals(metadata?.Kind, CadastralCanvasMetadata.MetadataKind, StringComparison.OrdinalIgnoreCase)
                    ? metadata
                    : null;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsCadastralParcelObject(CanvasObject item)
        {
            return string.Equals(item.ObjectType, "Polygon", StringComparison.OrdinalIgnoreCase) &&
                   !string.IsNullOrWhiteSpace(item.GeometryMetadataJson) &&
                   item.GeometryMetadataJson.Contains(CadastralCanvasMetadata.MetadataKind, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsProjectBoundaryObject(CanvasObject item)
        {
            return string.Equals(item.CanvasLayer?.LayerType, "ProjectBoundary", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(item.CanvasLayer?.Name, "Project Boundary", StringComparison.OrdinalIgnoreCase);
        }

        private static void AddGroupedRows(
            SummarySheet sheet,
            ref int serial,
            string category,
            IEnumerable<IGrouping<string, BaselineParcel>> groups)
        {
            double totalArea = groups.SelectMany(group => group).Sum(parcel => parcel.OriginalAreaSqm);
            foreach (var group in groups.OrderByDescending(group => group.Count()).ThenBy(group => group.Key))
            {
                double area = group.Sum(parcel => parcel.OriginalAreaSqm);
                AddRow(sheet, serial++, category, group.Key, group.Count(), area, ToRopani(area), Percent(area, totalArea));
            }
        }

        private static void AddQualityRow(SummarySheet sheet, int serial, string label, List<BaselineParcel> affectedParcels, string priority, string remarks)
        {
            AddRow(sheet, serial, label, affectedParcels.Count, affectedParcels.Sum(parcel => parcel.OriginalAreaSqm), priority, remarks);
        }

        private static ChartModel CreateTopOwnersChart(List<BaselineParcel> parcels)
        {
            var segments = parcels
                .GroupBy(parcel => BlankAsUnknown(parcel.LandOwner?.FullName))
                .Select(group => new { Owner = group.Key, Area = group.Sum(parcel => parcel.OriginalAreaSqm) })
                .OrderByDescending(item => item.Area)
                .Take(8)
                .Select((item, index) => new ChartSegment(item.Owner, item.Area, PickColor(index)))
                .ToList();

            return new ChartModel("Top Owners by Area", "Largest original holdings.", ChartKind.Bar, segments);
        }

        private static ChartModel CreateGroupedCountChart<T>(string title, IEnumerable<IGrouping<string, T>> groups)
        {
            List<ChartSegment> segments = groups
                .Select(group => new { group.Key, Count = group.Count() })
                .OrderByDescending(item => item.Count)
                .ThenBy(item => item.Key)
                .Take(10)
                .Select((item, index) => new ChartSegment(item.Key, item.Count, PickColor(index)))
                .ToList();

            return new ChartModel(title, "Top groups by count.", ChartKind.Bar, segments);
        }

        private static ChartModel CreateGroupedCountChart<T, TKey>(string title, IEnumerable<IGrouping<string, T>> groups, Func<T, TKey> distinctKey)
        {
            List<ChartSegment> segments = groups
                .Select(group => new
                {
                    group.Key,
                    Count = group.Select(distinctKey).Distinct().Count()
                })
                .OrderByDescending(item => item.Count)
                .ThenBy(item => item.Key)
                .Take(10)
                .Select((item, index) => new ChartSegment(item.Key, item.Count, PickColor(index)))
                .ToList();

            return new ChartModel(title, "Top groups by parcel/object count.", ChartKind.Bar, segments);
        }

        private static LandUseClass ClassifyLandUse(BaselineParcel parcel)
        {
            string text = $"{parcel.LandUse} {parcel.LandOwnershipType} {parcel.Remarks}".ToLowerInvariant();
            if (ContainsAny(text, "guthi", "guthi land"))
                return LandUseClass.Guthi;
            if (ContainsAny(text, "road", "sadak", "right of way", "row"))
                return LandUseClass.Road;
            if (ContainsAny(text, "public", "government", "govt", "sarkari", "school", "temple", "open space", "river", "canal"))
                return LandUseClass.Public;
            return LandUseClass.Private;
        }

        private static bool ContainsAny(string text, params string[] needles)
        {
            return needles.Any(needle => text.Contains(needle, StringComparison.OrdinalIgnoreCase));
        }

        private static SummaryColumn[] DefaultColumns(params string[] headers)
        {
            return headers.Select((header, index) => new SummaryColumn(
                $"Column{index}",
                header,
                index == 1 ? 260 : 128,
                index == headers.Length - 1,
                IsNumericHeader(header) ? DataGridViewContentAlignment.MiddleRight : DataGridViewContentAlignment.MiddleLeft)).ToArray();
        }

        private static bool IsNumericHeader(string header)
        {
            return header.Contains("Count", StringComparison.OrdinalIgnoreCase) ||
                   header.Contains("Area", StringComparison.OrdinalIgnoreCase) ||
                   header.Contains("Value", StringComparison.OrdinalIgnoreCase) ||
                   header.Contains("Percentage", StringComparison.OrdinalIgnoreCase) ||
                   header.Contains("Difference", StringComparison.OrdinalIgnoreCase);
        }

        private static void AddRow(SummarySheet sheet, params object?[] values)
        {
            sheet.Rows.Add(new SummaryRow(values.ToList(), false));
        }

        private static void AddTotalRow(SummarySheet sheet, params object?[] values)
        {
            object?[] normalized = new object?[sheet.Columns.Count];
            for (int i = 0; i < values.Length && i < normalized.Length; i++)
                normalized[i] = values[i];
            sheet.Rows.Add(new SummaryRow(normalized.ToList(), true));
        }

        private static string FormatCellValue(object? value)
        {
            return value switch
            {
                null => string.Empty,
                double d => d.ToString("N2", CultureInfo.InvariantCulture),
                float f => f.ToString("N2", CultureInfo.InvariantCulture),
                decimal m => m.ToString("N2", CultureInfo.InvariantCulture),
                int i => i.ToString("N0", CultureInfo.InvariantCulture),
                long l => l.ToString("N0", CultureInfo.InvariantCulture),
                _ => value.ToString() ?? string.Empty
            };
        }

        private string FormatAreaCompact(double areaSqm)
        {
            return $"{areaSqm.ToString($"F{_sqmPrecision}", CultureInfo.InvariantCulture)} sq.m";
        }

        private string FormatAreaLong(double areaSqm)
        {
            return $"{areaSqm.ToString($"F{_sqmPrecision}", CultureInfo.InvariantCulture)} sq.m / {ToRopani(areaSqm):N2} ropani";
        }

        private static string FormatPercent(double percent)
        {
            return $"{percent:0.##}%";
        }

        private static double Percent(double value, double total)
        {
            return Math.Abs(total) < 0.000001 ? 0 : value / total * 100.0;
        }

        private static double ToRopani(double sqm)
        {
            return sqm / SqmPerRopani;
        }

        private static double RopaniToSqm(double ropani)
        {
            return ropani * SqmPerRopani;
        }

        private static double AanaToSqm(double aana)
        {
            return RopaniToSqm(aana / 16.0);
        }

        private static string BlankAsUnknown(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Unknown" : value.Trim();
        }

        private static string FirstNonEmpty(params string?[] values)
        {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
        }

        private static Color PickColor(int index)
        {
            Color[] colors =
            [
                AccentBlue,
                AccentTeal,
                AccentRose,
                Color.FromArgb(132, 95, 180),
                Color.FromArgb(224, 151, 65),
                Color.FromArgb(74, 151, 112),
                Color.FromArgb(87, 115, 185),
                Color.FromArgb(188, 90, 92),
                Color.FromArgb(93, 130, 145),
                Color.FromArgb(115, 101, 88)
            ];
            return colors[Math.Abs(index) % colors.Length];
        }

        private static readonly Color AccentBlue = Color.FromArgb(62, 117, 185);
        private static readonly Color AccentTeal = Color.FromArgb(38, 151, 139);
        private static readonly Color AccentRose = Color.FromArgb(205, 99, 108);
        private static readonly Color GoodColor = Color.FromArgb(45, 145, 95);
        private static readonly Color WarnColor = Color.FromArgb(221, 143, 55);
        private static readonly Color InfoColor = Color.FromArgb(72, 130, 185);

        private sealed record AreaBand(string Name, double MinSqm, double MaxSqm);
        private sealed record MetricCard(string Label, string Value, string Hint, Color? Accent = null)
        {
            public Color AccentColor => Accent ?? AccentBlue;
        }

        private sealed class SummaryBook
        {
            public string ProjectName { get; set; } = "Project";
            public DateTime GeneratedAt { get; set; } = DateTime.Now;
            public int RecordParcelCount { get; set; }
            public int MapParcelCount { get; set; }
            public double BoundaryAreaSqm { get; set; }
            public double RecordAreaSqm { get; set; }
            public double MapAreaSqm { get; set; }
            public List<MetricCard> Metrics { get; } = [];
            public List<SummarySheet> Sheets { get; } = [];
        }

        private sealed class SummarySheet(string title, string description)
        {
            public string Title { get; } = title;
            public string Description { get; } = description;
            public List<SummaryColumn> Columns { get; } = [];
            public List<SummaryRow> Rows { get; } = [];
            public List<ChartModel> Charts { get; } = [];
        }

        private sealed record SummaryColumn(
            string Name,
            string Header,
            int Width,
            bool Fill,
            DataGridViewContentAlignment Alignment);

        private sealed record SummaryRow(List<object?> Values, bool IsTotal);
        private sealed record CadastralMapParcel(
            Guid CanvasObjectId,
            int? BaselineParcelId,
            string MapSheetNo,
            string ParcelNo,
            string AssignmentStatus,
            double AreaSqm,
            string SourceFileName,
            string SourceLayer);

        private sealed record ChartModel(string Title, string Subtitle, ChartKind Kind, List<ChartSegment> Segments);
        private sealed record ChartSegment(string Label, double Value, Color Color);
        private enum ChartKind { Bar, Donut }
        private enum LandUseClass { Private, Public, Road, Guthi }

        private sealed class SummaryChartPanel : Panel
        {
            private readonly ChartModel _chart;

            public SummaryChartPanel(ChartModel chart)
            {
                _chart = chart;
                BackColor = Color.White;
                DoubleBuffered = true;
                Padding = new Padding(12);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Graphics graphics = e.Graphics;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using Pen border = new(Color.FromArgb(222, 229, 238));
                graphics.DrawRectangle(border, 0, 0, Width - 1, Height - 1);

                using Font titleFont = new("Segoe UI Semibold", 10F, FontStyle.Bold);
                using Font smallFont = new("Segoe UI", 8F);
                using Brush titleBrush = new SolidBrush(Color.FromArgb(31, 45, 64));
                using Brush subBrush = new SolidBrush(Color.FromArgb(92, 105, 122));
                graphics.DrawString(_chart.Title, titleFont, titleBrush, 14, 10);
                graphics.DrawString(_chart.Subtitle, smallFont, subBrush, 14, 32);

                Rectangle chartArea = new(14, 56, Width - 28, Height - 70);
                if (_chart.Kind == ChartKind.Donut)
                    DrawDonut(graphics, chartArea, smallFont);
                else
                    DrawBars(graphics, chartArea, smallFont);
            }

            private void DrawDonut(Graphics graphics, Rectangle area, Font font)
            {
                double total = _chart.Segments.Sum(segment => Math.Max(0, segment.Value));
                if (total <= 0) return;

                Rectangle pie = new(area.Left, area.Top + 6, Math.Min(116, area.Height - 12), Math.Min(116, area.Height - 12));
                float start = -90;
                foreach (ChartSegment segment in _chart.Segments)
                {
                    float sweep = (float)(Math.Max(0, segment.Value) / total * 360.0);
                    using Brush brush = new SolidBrush(segment.Color);
                    graphics.FillPie(brush, pie, start, sweep);
                    start += sweep;
                }

                Rectangle inner = Rectangle.Inflate(pie, -28, -28);
                using Brush white = new SolidBrush(Color.White);
                graphics.FillEllipse(white, inner);
                using Font centerFont = new("Segoe UI Semibold", 9F, FontStyle.Bold);
                string totalText = total.ToString("N0", CultureInfo.InvariantCulture);
                SizeF totalSize = graphics.MeasureString(totalText, centerFont);
                using Brush textBrush = new SolidBrush(Color.FromArgb(31, 45, 64));
                graphics.DrawString(totalText, centerFont, textBrush, inner.Left + (inner.Width - totalSize.Width) / 2, inner.Top + (inner.Height - totalSize.Height) / 2);

                int x = pie.Right + 18;
                int y = area.Top + 8;
                foreach (ChartSegment segment in _chart.Segments.Take(8))
                {
                    using Brush brush = new SolidBrush(segment.Color);
                    graphics.FillRectangle(brush, x, y + 4, 10, 10);
                    graphics.DrawString($"{segment.Label}: {segment.Value:N0}", font, textBrush, x + 16, y);
                    y += 19;
                }
            }

            private void DrawBars(Graphics graphics, Rectangle area, Font font)
            {
                var segments = _chart.Segments.Where(segment => segment.Value > 0).Take(10).ToList();
                if (segments.Count == 0) return;

                double max = segments.Max(segment => segment.Value);
                int rowHeight = Math.Max(16, area.Height / segments.Count);
                using Brush textBrush = new SolidBrush(Color.FromArgb(31, 45, 64));
                for (int i = 0; i < segments.Count; i++)
                {
                    ChartSegment segment = segments[i];
                    int y = area.Top + i * rowHeight;
                    string label = segment.Label.Length > 23 ? segment.Label[..22] + "..." : segment.Label;
                    graphics.DrawString(label, font, textBrush, area.Left, y);
                    int barLeft = area.Left + 145;
                    int barWidth = (int)Math.Round((area.Width - 210) * segment.Value / max);
                    Rectangle bar = new(barLeft, y + 3, Math.Max(2, barWidth), Math.Max(8, rowHeight - 8));
                    using Brush brush = new SolidBrush(segment.Color);
                    graphics.FillRectangle(brush, bar);
                    graphics.DrawString(segment.Value.ToString("N0", CultureInfo.InvariantCulture), font, textBrush, bar.Right + 6, y);
                }
            }
        }
    }
}
