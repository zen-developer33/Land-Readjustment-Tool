using System.Drawing;
using Land_Readjustment_Tool.Core.Entities.Roads;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Backends;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    /// <summary>
    /// Draws imported road parcel polygons, island outlines, and road-name labels
    /// on the map canvas.
    /// </summary>
    /// <remarks>
    /// The renderer keeps the existing WinForms <see cref="Graphics"/> entry
    /// point, but all drawing is routed through <see cref="IMapRenderSurface"/>
    /// so future SkiaSharp backends can reuse the same render flow.
    /// </remarks>
    public class RoadParcelRenderer
    {
        private static readonly Dictionary<RoadParcelType, Color> FillColors = new()
        {
            { RoadParcelType.StraightRoad, Color.FromArgb(80, 160, 160, 175) },
            { RoadParcelType.Roundabout, Color.FromArgb(100, 100, 140, 210) },
            { RoadParcelType.MedianRoad, Color.FromArgb(80, 120, 130, 195) },
            { RoadParcelType.CulDeSac, Color.FromArgb(90, 145, 110, 190) },
            { RoadParcelType.Junction, Color.FromArgb(90, 185, 130, 130) },
            { RoadParcelType.Highway, Color.FromArgb(80, 200, 160, 100) },
            { RoadParcelType.Unknown, Color.FromArgb(80, 160, 160, 175) }
        };

        private readonly IMapRenderSurfaceFactory _renderSurfaceFactory;

        /// <summary>
        /// Creates a road parcel renderer using the production map render
        /// surface factory.
        /// </summary>
        public RoadParcelRenderer()
            : this(MapRenderSurfaceFactory.Default)
        {
        }

        /// <summary>
        /// Creates a road parcel renderer with an injectable surface factory
        /// for tests and future backend selection.
        /// </summary>
        /// <param name="renderSurfaceFactory">
        /// Factory used to wrap the current paint target in a backend-neutral
        /// render surface.
        /// </param>
        public RoadParcelRenderer(IMapRenderSurfaceFactory renderSurfaceFactory)
        {
            _renderSurfaceFactory = renderSurfaceFactory
                ?? throw new ArgumentNullException(nameof(renderSurfaceFactory));
        }

        /// <summary>
        /// Draws one road parcel polygon and its optional label.
        /// </summary>
        /// <param name="graphics">Current WinForms paint target.</param>
        /// <param name="road">Road parcel entity to draw.</param>
        /// <param name="worldToScreen">Projection from world coordinates into screen pixels.</param>
        /// <param name="isSelected">Whether the road should use selected styling.</param>
        /// <param name="showIslandOutlines">Whether interior rings should be emphasized.</param>
        public void Draw(
            Graphics graphics,
            RoadParcel road,
            Func<Coordinate, PointF> worldToScreen,
            bool isSelected = false,
            bool showIslandOutlines = true)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            ArgumentNullException.ThrowIfNull(road);
            ArgumentNullException.ThrowIfNull(worldToScreen);

            using IMapRenderSurface surface = _renderSurfaceFactory.CreateForGraphics(
                graphics,
                Size.Round(graphics.VisibleClipBounds.Size),
                new MapRenderSurfaceOptions
                {
                    ApplyInitialQuality = false
                });

            using IMapPath path = CreatePolygonPath(surface, road.Shape, worldToScreen);
            if (path.PointCount == 0)
            {
                return;
            }

            Color baseColor = FillColors.GetValueOrDefault(
                road.RoadType,
                Color.FromArgb(80, 160, 160, 175));
            surface.FillPath(
                path,
                new FillStyle(isSelected ? Color.FromArgb(160, 255, 215, 0) : baseColor));
            surface.DrawPath(
                path,
                new StrokeStyle(
                    isSelected ? Color.DarkOrange : Color.DimGray,
                    isSelected ? 2.5f : 1.5f));

            if (showIslandOutlines && road.IsDonut)
            {
                DrawIslandOutlines(surface, road.Shape, worldToScreen);
            }

            DrawLabel(surface, road, worldToScreen);
        }

        /// <summary>
        /// Builds a backend-owned path from a road parcel polygon, including
        /// interior rings.
        /// </summary>
        private static IMapPath CreatePolygonPath(
            IMapRenderSurface surface,
            Polygon polygon,
            Func<Coordinate, PointF> worldToScreen)
        {
            IMapPathBuilder builder = surface.CreatePath(FillRule.Alternate);
            AddRing(builder, polygon.ExteriorRing, worldToScreen);

            for (int i = 0; i < polygon.NumInteriorRings; i++)
            {
                AddRing(builder, polygon.GetInteriorRingN(i), worldToScreen);
            }

            return builder.Build();
        }

        /// <summary>
        /// Adds one polygon ring to the active path builder after dropping
        /// non-finite screen points.
        /// </summary>
        private static void AddRing(
            IMapPathBuilder builder,
            LineString ring,
            Func<Coordinate, PointF> worldToScreen)
        {
            PointF[] points = ring.Coordinates
                .Select(worldToScreen)
                .Where(point => float.IsFinite(point.X) && float.IsFinite(point.Y))
                .ToArray();

            if (points.Length >= 3)
            {
                builder.AddPolygon(points);
            }
        }

        /// <summary>
        /// Draws dashed outlines around all interior rings of a donut road
        /// parcel.
        /// </summary>
        private static void DrawIslandOutlines(
            IMapRenderSurface surface,
            Polygon polygon,
            Func<Coordinate, PointF> worldToScreen)
        {
            StrokeStyle islandStroke = new(Color.ForestGreen, 1.0f, DashPatternKind.Dashed);
            for (int i = 0; i < polygon.NumInteriorRings; i++)
            {
                PointF[] points = polygon.GetInteriorRingN(i)
                    .Coordinates
                    .Select(worldToScreen)
                    .Where(point => float.IsFinite(point.X) && float.IsFinite(point.Y))
                    .ToArray();
                if (points.Length < 3)
                {
                    continue;
                }

                IMapPathBuilder builder = surface.CreatePath();
                builder.AddPolygon(points);
                using IMapPath islandPath = builder.Build();
                surface.DrawPath(islandPath, islandStroke);
            }
        }

        /// <summary>
        /// Draws the road-name label at the polygon point-on-surface location.
        /// </summary>
        private static void DrawLabel(
            IMapRenderSurface surface,
            RoadParcel road,
            Func<Coordinate, PointF> worldToScreen)
        {
            if (string.IsNullOrWhiteSpace(road.RoadName))
            {
                return;
            }

            Coordinate coordinate = road.Shape.PointOnSurface.Coordinate;
            PointF point = worldToScreen(coordinate);
            if (!float.IsFinite(point.X) || !float.IsFinite(point.Y))
            {
                return;
            }

            TextStyle textStyle = new(
                "Segoe UI",
                10.0f,
                Color.Black,
                Bold: false,
                HorizontalAlign: TextAlign.Near,
                VerticalAlign: TextAlign.Near);
            SizeF size = surface.MeasureText(road.RoadName, textStyle);
            RectangleF layout = new(
                point.X - (size.Width / 2.0f),
                point.Y - (size.Height / 2.0f),
                Math.Max(1.0f, size.Width),
                Math.Max(1.0f, size.Height));
            surface.DrawText(road.RoadName, layout, textStyle);
        }
    }
}
