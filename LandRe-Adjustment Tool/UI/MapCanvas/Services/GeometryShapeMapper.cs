using System.Drawing;
using System.Reflection;
using System.Text.Json;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using NetTopologySuite.Geometries;
using NtsPoint = NetTopologySuite.Geometries.Point;

namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    /// <summary>
    /// Maps between runtime canvas shapes and persisted NetTopologySuite geometries.
    /// </summary>
    public static class GeometryShapeMapper
    {
        private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 0);

        public static CanvasObject ToCanvasObject(
            IShape shape,
            int layerId,
            CanvasObject? target = null)
        {
            ArgumentNullException.ThrowIfNull(shape);

            CanvasObject canvasObject = target ?? new CanvasObject();

            canvasObject.Id = shape.Id;
            canvasObject.CanvasLayerId = layerId;
            canvasObject.ObjectType = ResolveObjectType(shape);
            canvasObject.Shape = ToGeometry(shape);
            canvasObject.GeometryMetadataJson = CreateGeometryMetadataJson(shape);
            canvasObject.IsVisible = shape.IsVisible;
            canvasObject.BorderColorOverride = ResolveColorOverride(shape, "BorderColorOverride", target?.BorderColorOverride);
            canvasObject.FillColorOverride = ResolveColorOverride(shape, "FillColorOverride", target?.FillColorOverride);
            canvasObject.LabelText = ResolveLabelText(shape);
            canvasObject.ObjectDescription = GetPropertyAsString(shape, "Description");

            if (shape.Properties.TryGetValue("LineWeight", out object? lineWeightObj) &&
                TryToDouble(lineWeightObj, out double lineWeight))
            {
                canvasObject.LineWeightOverride = lineWeight;
            }

            if (shape.Properties.TryGetValue("LineStyle", out object? lineStyleObj))
            {
                canvasObject.LineStyleOverride = lineStyleObj?.ToString();
            }

            if (shape.Properties.TryGetValue("FillTransparency", out object? fillTransparencyObj) &&
                TryToInt(fillTransparencyObj, out int fillTransparency))
            {
                canvasObject.FillTransparencyOverride = fillTransparency;
            }

            if (shape.Properties.TryGetValue("BaselineParcelId", out object? baselineIdObj) &&
                TryToInt(baselineIdObj, out int baselineParcelId))
            {
                canvasObject.BaselineParcelId = baselineParcelId;
            }

            if (shape.Properties.TryGetValue("ReplottedParcelId", out object? replottedIdObj) &&
                TryToInt(replottedIdObj, out int replottedParcelId))
            {
                canvasObject.ReplottedParcelId = replottedParcelId;
            }

            if (shape.Properties.TryGetValue("RoadId", out object? roadIdObj) &&
                TryToInt(roadIdObj, out int roadId))
            {
                canvasObject.RoadId = roadId;
            }

            if (shape.Properties.TryGetValue("BlockId", out object? blockIdObj) &&
                TryToInt(blockIdObj, out int blockId))
            {
                canvasObject.BlockId = blockId;
            }

            return canvasObject;
        }

        public static IShape ToShape(CanvasObject canvasObject)
        {
            ArgumentNullException.ThrowIfNull(canvasObject);
            ArgumentNullException.ThrowIfNull(canvasObject.Shape);

            IShape shape = CreateShapeFromGeometry(
                canvasObject.ObjectType,
                canvasObject.Shape,
                canvasObject.LabelText,
                canvasObject.GeometryMetadataJson);

            SetShapeId(shape, canvasObject.Id);
            shape.LayerName = canvasObject.CanvasLayer?.Name ?? shape.LayerName;
            shape.IsVisible = canvasObject.IsVisible;

            if (!string.IsNullOrWhiteSpace(canvasObject.BorderColorOverride))
            {
                shape.BorderColor = ParseColorOrDefault(canvasObject.BorderColorOverride, shape.BorderColor);
                shape.Properties["BorderColorOverride"] = canvasObject.BorderColorOverride;
            }

            if (!string.IsNullOrWhiteSpace(canvasObject.FillColorOverride))
            {
                shape.FillColor = ParseColorOrDefault(canvasObject.FillColorOverride, shape.FillColor);
                shape.Properties["FillColorOverride"] = canvasObject.FillColorOverride;
            }

            if (!string.IsNullOrWhiteSpace(canvasObject.LabelText))
            {
                shape.Properties["LabelText"] = canvasObject.LabelText;
            }

            if (!string.IsNullOrWhiteSpace(canvasObject.ObjectDescription))
            {
                shape.Properties["Description"] = canvasObject.ObjectDescription;
            }

            if (canvasObject.LineWeightOverride.HasValue)
            {
                shape.Properties["LineWeight"] = canvasObject.LineWeightOverride.Value;
            }

            if (!string.IsNullOrWhiteSpace(canvasObject.LineStyleOverride))
            {
                shape.Properties["LineStyle"] = canvasObject.LineStyleOverride;
            }

            if (canvasObject.FillTransparencyOverride.HasValue)
            {
                shape.Properties["FillTransparency"] = canvasObject.FillTransparencyOverride.Value;
            }

            if (canvasObject.BaselineParcelId.HasValue)
            {
                shape.Properties["BaselineParcelId"] = canvasObject.BaselineParcelId.Value;
            }

            if (canvasObject.ReplottedParcelId.HasValue)
            {
                shape.Properties["ReplottedParcelId"] = canvasObject.ReplottedParcelId.Value;
            }

            if (canvasObject.RoadId.HasValue)
            {
                shape.Properties["RoadId"] = canvasObject.RoadId.Value;
            }

            if (canvasObject.BlockId.HasValue)
            {
                shape.Properties["BlockId"] = canvasObject.BlockId.Value;
            }

            return shape;
        }

        private static Geometry ToGeometry(IShape shape)
        {
            return shape switch
            {
                LineShape line => GeometryFactory.CreateLineString(
                    [
                        new Coordinate(line.Start.X, line.Start.Y),
                        new Coordinate(line.End.X, line.End.Y)
                    ]),
                PolylineShape polyline => CreateGeometryFromPolyline(polyline),
                RectangleShape rectangle => CreatePolygonFromRectangle(rectangle),
                CircleShape circle => CreatePolygonFromCircle(circle),
                ArcShape arc => CreateLineStringFromArc(arc),
                EllipseShape ellipse => CreatePolygonFromEllipse(ellipse),
                TextShape text => GeometryFactory.CreatePoint(
                    new Coordinate(text.Position.X, text.Position.Y)),
                _ => CreatePolygonFromBoundingBox(shape.GetBoundingBox())
            };
        }

        private static Geometry CreateGeometryFromPolyline(PolylineShape polyline)
        {
            if (polyline.Vertices.Count == 0)
            {
                return GeometryFactory.CreateGeometryCollection();
            }

            if (polyline.Vertices.Count == 1)
            {
                PointD v = polyline.Vertices[0];
                return GeometryFactory.CreatePoint(new Coordinate(v.X, v.Y));
            }

            List<Coordinate> coordinates = polyline.Vertices
                .Select(v => new Coordinate(v.X, v.Y))
                .ToList();

            if (polyline.IsClosed && coordinates.Count >= 3)
            {
                CloseRing(coordinates);
                return GeometryFactory.CreatePolygon(coordinates.ToArray());
            }

            return GeometryFactory.CreateLineString(coordinates.ToArray());
        }

        private static Polygon CreatePolygonFromRectangle(RectangleShape rectangle)
        {
            RectangleD bounds = rectangle.GetBoundingBox();
            List<Coordinate> ring =
            [
                new Coordinate(bounds.Left, bounds.Top),
                new Coordinate(bounds.Right, bounds.Top),
                new Coordinate(bounds.Right, bounds.Bottom),
                new Coordinate(bounds.Left, bounds.Bottom),
                new Coordinate(bounds.Left, bounds.Top)
            ];

            return GeometryFactory.CreatePolygon(ring.ToArray());
        }

        private static Polygon CreatePolygonFromCircle(CircleShape circle)
        {
            double radius = circle.GetRadius();
            Coordinate center = new(circle.Center.X, circle.Center.Y);
            return (Polygon)GeometryFactory.CreatePoint(center).Buffer(radius, quadrantSegments: 24);
        }

        private static LineString CreateLineStringFromArc(ArcShape arc)
        {
            Coordinate[] coordinates = arc.SamplePoints(96)
                .Select(point => new Coordinate(point.X, point.Y))
                .ToArray();
            return GeometryFactory.CreateLineString(coordinates);
        }

        private static Polygon CreatePolygonFromEllipse(EllipseShape ellipse)
        {
            RectangleD bounds = ellipse.GetBoundingBox();
            double centerX = bounds.Left + (bounds.Width / 2.0);
            double centerY = bounds.Top + (bounds.Height / 2.0);
            double radiusX = bounds.Width / 2.0;
            double radiusY = bounds.Height / 2.0;

            const int segments = 72;
            List<Coordinate> ring = new(segments + 1);

            for (int i = 0; i < segments; i++)
            {
                double theta = 2.0 * Math.PI * i / segments;
                double x = centerX + radiusX * Math.Cos(theta);
                double y = centerY + radiusY * Math.Sin(theta);
                ring.Add(new Coordinate(x, y));
            }

            ring.Add(new Coordinate(ring[0].X, ring[0].Y));
            return GeometryFactory.CreatePolygon(ring.ToArray());
        }

        private static Polygon CreatePolygonFromBoundingBox(RectangleD bounds)
        {
            List<Coordinate> ring =
            [
                new Coordinate(bounds.Left, bounds.Top),
                new Coordinate(bounds.Right, bounds.Top),
                new Coordinate(bounds.Right, bounds.Bottom),
                new Coordinate(bounds.Left, bounds.Bottom),
                new Coordinate(bounds.Left, bounds.Top)
            ];

            return GeometryFactory.CreatePolygon(ring.ToArray());
        }

        private static IShape CreateShapeFromGeometry(
            string objectType,
            Geometry geometry,
            string? labelText,
            string? metadataJson)
        {
            if (objectType.Equals("Circle", StringComparison.OrdinalIgnoreCase))
            {
                if (TryCreateCircleFromMetadata(metadataJson, out CircleShape? circle))
                {
                    return circle;
                }

                if (TryCreateCircleFromPolygon(geometry, out circle))
                {
                    return circle;
                }
            }

            if (objectType.Equals("Arc", StringComparison.OrdinalIgnoreCase) &&
                TryCreateArcFromMetadata(metadataJson, out ArcShape? arc))
            {
                return arc;
            }

            Geometry simplified = ReduceGeometry(geometry);

            if (objectType.Equals("Text", StringComparison.OrdinalIgnoreCase) &&
                simplified is NtsPoint textPoint)
            {
                return new TextShape(
                    new PointD(textPoint.X, textPoint.Y),
                    labelText ?? string.Empty);
            }

            return simplified switch
            {
                NtsPoint point => CreatePointShape(point),
                LineString line => CreateLineShape(objectType, line),
                Polygon polygon => CreatePolygonShape(polygon),
                _ => CreatePolygonFromEnvelopeShape(simplified.EnvelopeInternal)
            };
        }

        private static IShape CreatePointShape(NtsPoint point)
        {
            PolylineShape shape = new([new PointD(point.X, point.Y)], isClosed: false);
            shape.Properties["ObjectType"] = "Point";
            return shape;
        }

        private static IShape CreateLineShape(string objectType, LineString line)
        {
            Coordinate[] coordinates = line.Coordinates;

            if (coordinates.Length < 2)
            {
                return CreatePointShape(GeometryFactory.CreatePoint(coordinates.FirstOrDefault() ?? new Coordinate(0, 0)));
            }

            if (objectType.Equals("Line", StringComparison.OrdinalIgnoreCase) && coordinates.Length == 2)
            {
                return new LineShape(
                    new PointD(coordinates[0].X, coordinates[0].Y),
                    new PointD(coordinates[1].X, coordinates[1].Y));
            }

            List<PointD> vertices = coordinates
                .Select(c => new PointD(c.X, c.Y))
                .ToList();

            return new PolylineShape(vertices, isClosed: false);
        }

        private static IShape CreatePolygonShape(Polygon polygon)
        {
            Coordinate[] coordinates = polygon.ExteriorRing.Coordinates;
            List<PointD> vertices = coordinates
                .Select(c => new PointD(c.X, c.Y))
                .ToList();

            if (vertices.Count > 1 &&
                NearlyEqual(vertices[0].X, vertices[^1].X) &&
                NearlyEqual(vertices[0].Y, vertices[^1].Y))
            {
                vertices.RemoveAt(vertices.Count - 1);
            }

            return new PolylineShape(vertices, isClosed: true);
        }

        private static IShape CreatePolygonFromEnvelopeShape(Envelope envelope)
        {
            return new RectangleShape(
                new PointD(envelope.MinX, envelope.MinY),
                new PointD(envelope.MaxX, envelope.MaxY));
        }

        private static Geometry ReduceGeometry(Geometry geometry)
        {
            return geometry switch
            {
                MultiPolygon multiPolygon when multiPolygon.NumGeometries > 0 => multiPolygon
                    .Geometries
                    .OfType<Polygon>()
                    .OrderByDescending(poly => poly.Area)
                    .First(),
                MultiLineString multiLineString when multiLineString.NumGeometries > 0 => (LineString)multiLineString.GetGeometryN(0),
                GeometryCollection collection when collection.NumGeometries > 0 => ReduceGeometry(collection.GetGeometryN(0)),
                _ => geometry
            };
        }

        private static void CloseRing(List<Coordinate> coordinates)
        {
            Coordinate first = coordinates[0];
            Coordinate last = coordinates[^1];
            if (!NearlyEqual(first.X, last.X) || !NearlyEqual(first.Y, last.Y))
            {
                coordinates.Add(new Coordinate(first.X, first.Y));
            }
        }

        private static string ResolveObjectType(IShape shape)
        {
            if (shape.Properties.TryGetValue("ObjectType", out object? objectType) &&
                !string.IsNullOrWhiteSpace(objectType?.ToString()))
            {
                return objectType.ToString()!.Trim();
            }

            return shape switch
            {
                PolylineShape polyline when polyline.IsClosed => "Polygon",
                PolylineShape => "Polyline",
                LineShape => "Line",
                CircleShape => "Circle",
                RectangleShape => "Polygon",
                ArcShape => "Arc",
                EllipseShape => "Polygon",
                TextShape => "Text",
                _ => "Polyline"
            };
        }

        private static string? ResolveLabelText(IShape shape)
        {
            if (shape is TextShape textShape)
            {
                return textShape.Text;
            }

            if (shape.Properties.TryGetValue("LabelText", out object? value))
            {
                return value?.ToString();
            }

            return null;
        }

        private static string? GetPropertyAsString(IShape shape, string key)
        {
            if (shape.Properties.TryGetValue(key, out object? value))
            {
                return value?.ToString();
            }

            return null;
        }

        private static string? ColorToHtml(Color color)
        {
            return ColorTranslator.ToHtml(Color.FromArgb(255, color.R, color.G, color.B));
        }

        private static string? CreateGeometryMetadataJson(IShape shape)
        {
            try
            {
                return shape switch
                {
                    CircleShape circle => JsonSerializer.Serialize(new CurveMetadata(
                        "Circle",
                        circle.Center.X,
                        circle.Center.Y,
                        circle.RadiusPoint.X,
                        circle.RadiusPoint.Y,
                        circle.GetRadius(),
                        null,
                        null)),
                    ArcShape arc => JsonSerializer.Serialize(new CurveMetadata(
                        "Arc",
                        arc.Center.X,
                        arc.Center.Y,
                        null,
                        null,
                        arc.Radius,
                        arc.StartAngleRadians,
                        arc.SweepAngleRadians)),
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }

        private static bool TryCreateCircleFromMetadata(
            string? metadataJson,
            out CircleShape? circle)
        {
            circle = null;
            if (string.IsNullOrWhiteSpace(metadataJson))
            {
                return false;
            }

            try
            {
                CurveMetadata? metadata = JsonSerializer.Deserialize<CurveMetadata>(metadataJson);
                if (metadata == null ||
                    !metadata.ShapeType.Equals("Circle", StringComparison.OrdinalIgnoreCase) ||
                    !metadata.Radius.HasValue ||
                    metadata.Radius.Value <= 0.0)
                {
                    return false;
                }

                PointD center = new(metadata.CenterX, metadata.CenterY);
                PointD radiusPoint = metadata.RadiusPointX.HasValue && metadata.RadiusPointY.HasValue
                    ? new PointD(metadata.RadiusPointX.Value, metadata.RadiusPointY.Value)
                    : new PointD(metadata.CenterX + metadata.Radius.Value, metadata.CenterY);
                circle = new CircleShape(center, radiusPoint);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryCreateArcFromMetadata(
            string? metadataJson,
            out ArcShape? arc)
        {
            arc = null;
            if (string.IsNullOrWhiteSpace(metadataJson))
            {
                return false;
            }

            try
            {
                CurveMetadata? metadata = JsonSerializer.Deserialize<CurveMetadata>(metadataJson);
                if (metadata == null ||
                    !metadata.ShapeType.Equals("Arc", StringComparison.OrdinalIgnoreCase) ||
                    !metadata.Radius.HasValue ||
                    !metadata.StartAngleRadians.HasValue ||
                    !metadata.SweepAngleRadians.HasValue ||
                    metadata.Radius.Value <= 0.0)
                {
                    return false;
                }

                arc = new ArcShape(
                    new PointD(metadata.CenterX, metadata.CenterY),
                    metadata.Radius.Value,
                    metadata.StartAngleRadians.Value,
                    metadata.SweepAngleRadians.Value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryCreateCircleFromPolygon(
            Geometry geometry,
            out CircleShape? circle)
        {
            circle = null;
            Geometry simplified = ReduceGeometry(geometry);
            if (simplified is not Polygon polygon)
            {
                return false;
            }

            Envelope envelope = polygon.EnvelopeInternal;
            double width = envelope.Width;
            double height = envelope.Height;
            if (width <= 0.0 ||
                height <= 0.0 ||
                Math.Abs(width - height) > Math.Max(width, height) * 0.02)
            {
                return false;
            }

            double radius = (width + height) / 4.0;
            PointD center = new(
                (envelope.MinX + envelope.MaxX) / 2.0,
                (envelope.MinY + envelope.MaxY) / 2.0);
            circle = new CircleShape(center, new PointD(center.X + radius, center.Y));
            return true;
        }

        private sealed record CurveMetadata(
            string ShapeType,
            double CenterX,
            double CenterY,
            double? RadiusPointX,
            double? RadiusPointY,
            double? Radius,
            double? StartAngleRadians,
            double? SweepAngleRadians);

        private static string? ResolveColorOverride(
            IShape shape,
            string propertyKey,
            string? existingOverride)
        {
            if (!shape.Properties.TryGetValue(propertyKey, out object? value))
            {
                return existingOverride;
            }

            if (value is Color color)
            {
                return ColorToHtml(color);
            }

            string? html = value?.ToString();
            return string.IsNullOrWhiteSpace(html)
                ? null
                : html;
        }

        private static Color ParseColorOrDefault(string html, Color fallback)
        {
            try
            {
                return ColorTranslator.FromHtml(html);
            }
            catch
            {
                return fallback;
            }
        }

        private static void SetShapeId(IShape shape, Guid id)
        {
            PropertyInfo? idProperty = shape
                .GetType()
                .GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (idProperty == null)
            {
                return;
            }

            MethodInfo? setter = idProperty.GetSetMethod(nonPublic: true);
            setter?.Invoke(shape, [id]);
        }

        private static bool TryToDouble(object? value, out double result)
        {
            result = 0;
            return value != null && double.TryParse(value.ToString(), out result);
        }

        private static bool TryToInt(object? value, out int result)
        {
            result = 0;
            return value != null && int.TryParse(value.ToString(), out result);
        }

        private static bool NearlyEqual(double a, double b)
        {
            return Math.Abs(a - b) <= 0.0000001;
        }
    }
}
