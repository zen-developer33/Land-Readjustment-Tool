using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.UI.MapCanvas.Core
{
    public class MapViewTransform
    {
        private double _scaleX;
        private double _scaleY;
        private double _offsetX;
        private double _offsetY;

        public void FitToExtent(Envelope worldExtent, Rectangle screenRect)
        {
            ArgumentNullException.ThrowIfNull(worldExtent);

            if (worldExtent.Width <= 0.0 || worldExtent.Height <= 0.0 ||
                screenRect.Width <= 0 || screenRect.Height <= 0)
            {
                _scaleX = 1.0;
                _scaleY = 1.0;
                _offsetX = screenRect.Left;
                _offsetY = screenRect.Top;
                return;
            }

            _scaleX = screenRect.Width / worldExtent.Width;
            _scaleY = screenRect.Height / worldExtent.Height;
            _offsetX = screenRect.Left - (worldExtent.MinX * _scaleX);
            _offsetY = screenRect.Top + (worldExtent.MaxY * _scaleY);
        }

        public PointF ToScreen(Coordinate world) =>
            new(
                (float)((world.X * _scaleX) + _offsetX),
                (float)((-world.Y * _scaleY) + _offsetY));

        public PointD ToScreenPointD(Coordinate world)
        {
            PointF point = ToScreen(world);
            return new PointD(point.X, point.Y);
        }

        public Coordinate ToWorld(PointF screen) =>
            new(
                (screen.X - _offsetX) / _scaleX,
                -((screen.Y - _offsetY) / _scaleY));
    }
}
