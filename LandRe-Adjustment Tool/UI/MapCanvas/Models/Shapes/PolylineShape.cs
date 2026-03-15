using System.Collections.Generic;
using System.Drawing;
using Land_Readjustment_Tool.DrawingCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Snapping;

namespace Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes
{
    public class PolylineShape : Shape, ISnapProvider
    {
        public List<PointD> Vertices { get; set; }
        public bool IsClosed { get; set; }

        public PolylineShape(IEnumerable<PointD> points, bool isClosed = false)
        {
            Vertices = new List<PointD>(points);
            IsClosed = isClosed;
        }

        public override RectangleD GetBoundingBox()
        {
            if (Vertices.Count == 0) return new RectangleD(0, 0, 0, 0);
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;
            foreach (var pt in Vertices)
            {
                minX = Math.Min(minX, pt.X);
                minY = Math.Min(minY, pt.Y);
                maxX = Math.Max(maxX, pt.X);
                maxY = Math.Max(maxY, pt.Y);
            }
            return new RectangleD(minX, minY, maxX - minX, maxY - minY);
        }

        public override void Draw(Graphics g, Func<PointD, PointD> worldToScreen, bool isPreview = false)
        {
            if (Vertices.Count < 2) return;
            try
            {
                // CRITICAL FIX: Keep in double precision and validate before converting
                var pointsD = Vertices.ConvertAll(pt => worldToScreen(pt));

                // Validate with double precision BEFORE converting to float
                foreach (var p in pointsD)
                {
                    if (double.IsNaN(p.X) || double.IsNaN(p.Y) ||
                        double.IsInfinity(p.X) || double.IsInfinity(p.Y))
                        return;
                }

                // PIXEL-PERFECT FIX: Round each vertex to nearest pixel for alignment
                var points = pointsD.ConvertAll(p => new PointF(
                    (float)Math.Round(p.X),
                    (float)Math.Round(p.Y)
                )).ToArray();

                // Use same lineweight as other shapes: 0.25f (AutoCAD default), 2f if selected
                float penWidth = IsSelected ? 2f : 0.25f;
                using (var pen = new Pen(isPreview ? Color.LightGray : (IsSelected ? Color.Yellow : BorderColor), penWidth))
                {
                    pen.DashStyle = isPreview ? System.Drawing.Drawing2D.DashStyle.Dash : System.Drawing.Drawing2D.DashStyle.Solid;
                    g.DrawLines(pen, points);
                }
                // No polygon fill, no closing segment unless IsClosed
                if (IsClosed && Vertices.Count > 2)
                {
                    using (var pen = new Pen(isPreview ? Color.LightGray : (IsSelected ? Color.Yellow : BorderColor), penWidth))
                    {
                        pen.DashStyle = isPreview ? System.Drawing.Drawing2D.DashStyle.Dash : System.Drawing.Drawing2D.DashStyle.Solid;
                        g.DrawLine(pen, points[points.Length - 1], points[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                // Optionally log or display error
                // System.Diagnostics.Debug.WriteLine($"PolylineShape.Draw error: {ex.Message}");
            }
        }

        public override IShape Clone()
        {
            return new PolylineShape(new List<PointD>(Vertices), IsClosed)
            {
                BorderColor = this.BorderColor,
                FillColor = this.FillColor,
                LayerName = this.LayerName
            };
        }

        public override bool ContainsPoint(PointD worldPoint, float tolerance)
        {
            // Simple hit test: check if close to any segment
            for (int i = 0; i < Vertices.Count - 1; i++)
            {
                if (PointToSegmentDistance(worldPoint, Vertices[i], Vertices[i + 1]) <= tolerance)
                    return true;
            }
            if (IsClosed && Vertices.Count > 2)
            {
                if (PointToSegmentDistance(worldPoint, Vertices[Vertices.Count - 1], Vertices[0]) <= tolerance)
                    return true;
            }
            return false;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            if (Vertices.Count == 0)
                yield break;
            // Endpoints
            foreach (var v in Vertices)
                yield return new SnapPoint(SnapType.Endpoint, v, this);
            // Midpoints of segments
            for (int i = 0; i < Vertices.Count - 1; i++)
            {
                var a = Vertices[i];
                var b = Vertices[i + 1];
                yield return new SnapPoint(SnapType.Midpoint, new PointD((a.X + b.X) / 2, (a.Y + b.Y) / 2), this);
            }
            if (IsClosed && Vertices.Count > 2)
            {
                var a = Vertices[Vertices.Count - 1];
                var b = Vertices[0];
                yield return new SnapPoint(SnapType.Midpoint, new PointD((a.X + b.X) / 2, (a.Y + b.Y) / 2), this);
            }
        }
    }
}