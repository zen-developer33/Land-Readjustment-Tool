using System.Drawing.Drawing2D;
using Land_Readjustment_Tool.Core.Entities.Roads;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
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

            using GraphicsPath path = ParcelPathBuilder.ToPath(road.Shape, worldToScreen);
            if (path.PointCount == 0)
            {
                return;
            }

            Color baseColor = FillColors.GetValueOrDefault(
                road.RoadType,
                Color.FromArgb(80, 160, 160, 175));
            using SolidBrush fill = new(
                isSelected ? Color.FromArgb(160, 255, 215, 0) : baseColor);
            graphics.FillPath(fill, path);

            using Pen outerPen = new(
                isSelected ? Color.DarkOrange : Color.DimGray,
                isSelected ? 2.5f : 1.5f);
            graphics.DrawPath(outerPen, path);

            if (showIslandOutlines && road.IsDonut)
            {
                using Pen islandPen = new(Color.ForestGreen, 1.0f)
                {
                    DashStyle = DashStyle.Dash
                };

                for (int i = 0; i < road.Shape.NumInteriorRings; i++)
                {
                    PointF[] points = road.Shape.GetInteriorRingN(i)
                        .Coordinates
                        .Select(worldToScreen)
                        .ToArray();
                    if (points.Length >= 3)
                    {
                        graphics.DrawPolygon(islandPen, points);
                    }
                }
            }

            DrawLabel(graphics, road, worldToScreen);
        }

        private static void DrawLabel(
            Graphics graphics,
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

            using Font font = new("Segoe UI", 7.5f, FontStyle.Regular);
            using SolidBrush brush = new(Color.Black);
            SizeF size = graphics.MeasureString(road.RoadName, font);
            graphics.DrawString(
                road.RoadName,
                font,
                brush,
                point.X - (size.Width / 2.0f),
                point.Y - (size.Height / 2.0f));
        }
    }
}
