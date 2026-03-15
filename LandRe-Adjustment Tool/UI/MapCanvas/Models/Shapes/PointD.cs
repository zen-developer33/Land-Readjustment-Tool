using System;

namespace Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes
{
    public struct PointD
    {
        public double X { get; set; }
        public double Y { get; set; }

        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static PointD operator +(PointD a, PointD b) => new PointD(a.X + b.X, a.Y + b.Y);
        public static PointD operator -(PointD a, PointD b) => new PointD(a.X - b.X, a.Y - b.Y);
        public static PointD operator *(PointD a, double d) => new PointD(a.X * d, a.Y * d);
        public static PointD operator /(PointD a, double d) => new PointD(a.X / d, a.Y / d);
        public override string ToString() => $"({X}, {Y})";
    }

    public struct RectangleD
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public RectangleD(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public double Left => X;
        public double Right => X + Width;
        public double Top => Y;
        public double Bottom => Y + Height;

        public bool Contains(PointD pt) => pt.X >= X && pt.X <= Right && pt.Y >= Y && pt.Y <= Bottom;
        public bool IntersectsWith(RectangleD other) => !(other.Left > Right || other.Right < Left || other.Top > Bottom || other.Bottom < Top);
        public override string ToString() => $"({X}, {Y}, {Width}, {Height})";
    }
}
