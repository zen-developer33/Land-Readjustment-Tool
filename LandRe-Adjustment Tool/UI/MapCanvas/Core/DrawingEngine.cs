using System.Drawing;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.UI.MapCanvas.Core
{
    /// <summary>
    /// FULLY CORRECTED DrawingEngine with proper coordinate transformations.
    /// </summary>
    public class DrawingEngine
    {
        // Viewport state
        private double _zoomScale = 1.0;
        // viewOffset is the WORLD COORDINATE of screen point (0, 0)
        private PointD _viewOffset = new PointD(0.0, 0.0);
        private Size _canvasSize;

        public float viewoffsetX {  get; set; }
        public float viewoffsetY { get; set; }
        public const double MIN_ZOOM = 0.0001; // Lower minimum zoom for large extents
        public const double MAX_ZOOM = 50000.0;
        public const double ZOOM_STEP = 1.4;

        public double ZoomScale => _zoomScale;
        public PointD ViewOffset => _viewOffset;
        public Size CanvasSize => _canvasSize;

        public DrawingEngine(Size canvasSize)
        {
            _canvasSize = canvasSize;
        }

        public void UpdateCanvasSize(Size newSize)
        {
            _canvasSize = newSize;
        }

        #region Coordinate Transformations - CORRECTED

        public PointD ScreenToWorld(Point screenPoint)
        {
            return ScreenToWorld(new PointD(screenPoint.X, screenPoint.Y));
        }

        public PointD ScreenToWorld(PointD screenPoint)
        {
            double screenX = screenPoint.X;
            double screenY = screenPoint.Y;
            double flippedY = _canvasSize.Height - screenY;
            double worldX = screenX / _zoomScale;
            double worldY = flippedY / _zoomScale;
            worldX += _viewOffset.X;
            worldY += _viewOffset.Y;
            return new PointD(worldX, worldY);
        }

        public PointD WorldToScreen(PointD worldPoint)
        {
            double relativeX = worldPoint.X - _viewOffset.X;
            double relativeY = worldPoint.Y - _viewOffset.Y;
            double screenX = relativeX * _zoomScale;
            double screenY = relativeY * _zoomScale;
            screenY = _canvasSize.Height - screenY;
            return new PointD(screenX, screenY);
        }

        public double ScreenToWorldDistance(double screenDistance)
        {
            return screenDistance / _zoomScale;
        }

        public double WorldToScreenDistance(double worldDistance)
        {
            return worldDistance * _zoomScale;
        }

        #endregion

        #region Viewport Queries - CORRECTED

        public RectangleD GetViewportBounds()
        {
            PointD topLeft = ScreenToWorld(new Point(0, 0));
            PointD topRight = ScreenToWorld(new Point(_canvasSize.Width, 0));
            PointD bottomLeft = ScreenToWorld(new Point(0, _canvasSize.Height));
            PointD bottomRight = ScreenToWorld(new Point(_canvasSize.Width, _canvasSize.Height));
            double minX = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
            double maxX = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));
            double minY = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
            double maxY = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));
            return new RectangleD(minX, minY, maxX - minX, maxY - minY);
        }

        public bool IsVisible(RectangleD worldBounds)
        {
            RectangleD viewport = GetViewportBounds();
            return viewport.IntersectsWith(worldBounds);
        }

        // Helper to get visible world rectangle from screen
        public RectangleD GetVisibleWorldRectangle()
        {
            PointD topLeftWorld = ScreenToWorld(new PointD(0, 0));
            PointD bottomRightWorld = ScreenToWorld(new PointD(CanvasSize.Width, CanvasSize.Height));

            double left = Math.Min(topLeftWorld.X, bottomRightWorld.X);
            double right = Math.Max(topLeftWorld.X, bottomRightWorld.X);
            double top = Math.Min(topLeftWorld.Y, bottomRightWorld.Y);
            double bottom = Math.Max(topLeftWorld.Y, bottomRightWorld.Y);

            return new RectangleD(
                left,
                top,
                right - left,
                bottom - top
            );
        }

        #endregion

        #region Zoom Operations - CORRECTED

        // Nepal UTM bounds (44N and 45N)
        // UTM 44N: Easting 240000 to 390000, Northing 3060000 to 3180000
        // UTM 45N: Easting 390000 to 540000, Northing 3060000 to 3180000
        // Combined world bounds: Easting 240000 to 540000, Northing 3060000 to 3180000
        private static readonly RectangleD NepalWorldBounds = new RectangleD(240000, 3060000, 300000, 120000);

        public RectangleD WorldBounds => NepalWorldBounds;

        // Helper to get min/max zoom for Nepal bounds
        public double GetMinZoomForWorldBounds()
        {
            double zoomX = _canvasSize.Width / NepalWorldBounds.Width;
            double zoomY = _canvasSize.Height / NepalWorldBounds.Height;
            return Math.Min(zoomX, zoomY) * 0.9;
        }
        public double GetMaxZoomForWorldBounds()
        {
            return MAX_ZOOM;
        }

        public void ZoomAtPoint(Point screenPoint, double zoomFactor)
        {
            double newZoomScale = _zoomScale * zoomFactor;
            double minZoom = GetMinZoomForWorldBounds();
            double maxZoom = GetMaxZoomForWorldBounds();
            newZoomScale = Math.Max(minZoom, Math.Min(maxZoom, newZoomScale));
            if (Math.Abs(newZoomScale - _zoomScale) < 0.0000001) return;
            PointD worldPoint = ScreenToWorld(screenPoint);
            _zoomScale = newZoomScale;
            double screenX = screenPoint.X;
            double screenY = _canvasSize.Height - screenPoint.Y;
            _viewOffset.X = worldPoint.X - (screenX / _zoomScale);
            _viewOffset.Y = worldPoint.Y - (screenY / _zoomScale);
        }

        public void ZoomAtCenter(double zoomFactor)
        {
            Point center = new Point(_canvasSize.Width / 2, _canvasSize.Height / 2);
            ZoomAtPoint(center, zoomFactor);
        }

        public void ZoomToExtents(RectangleD worldBounds, double padding = 0.9)
        {
            if (worldBounds.Width < 10.0) worldBounds.Width = 100.0;
            if (worldBounds.Height < 10.0) worldBounds.Height = 100.0;
            double zoomX = _canvasSize.Width / worldBounds.Width;
            double zoomY = _canvasSize.Height / worldBounds.Height;
            double minZoom = GetMinZoomForWorldBounds();
            double maxZoom = GetMaxZoomForWorldBounds();
            _zoomScale = Math.Min(zoomX, zoomY) * padding;
            _zoomScale = Math.Max(minZoom, Math.Min(maxZoom, _zoomScale));
            if (_zoomScale <= minZoom + 1e-12)
                _zoomScale = minZoom;
            double centerX = worldBounds.X + worldBounds.Width / 2.0;
            double centerY = worldBounds.Y + worldBounds.Height / 2.0;
            double screenCenterX = _canvasSize.Width / 2.0;
            double screenCenterY = _canvasSize.Height / 2.0;
            _viewOffset.X = centerX - (screenCenterX / _zoomScale);
            _viewOffset.Y = centerY - ((_canvasSize.Height - screenCenterY) / _zoomScale);
        }

        public void SetView(double worldCenterX, double worldCenterY, double worldWidth, double worldHeight)
        {
            double zoomX = _canvasSize.Width / worldWidth;
            double zoomY = _canvasSize.Height / worldHeight;
            double minZoom = GetMinZoomForWorldBounds();
            double maxZoom = GetMaxZoomForWorldBounds();
            _zoomScale = Math.Min(zoomX, zoomY) * 0.9;
            _zoomScale = Math.Max(minZoom, Math.Min(maxZoom, _zoomScale));
            double screenCenterX = _canvasSize.Width / 2.0;
            double screenCenterY = _canvasSize.Height / 2.0;
            _viewOffset.X = worldCenterX - (screenCenterX / _zoomScale);
            _viewOffset.Y = worldCenterY - ((_canvasSize.Height - screenCenterY) / _zoomScale);
        }

        #endregion

        #region Pan Operations - CORRECTED

        public void Pan(double screenDeltaX, double screenDeltaY)
        {
            double worldDeltaX = screenDeltaX / _zoomScale;
            double worldDeltaY = -screenDeltaY / _zoomScale;
            _viewOffset.X -= worldDeltaX;
            _viewOffset.Y -= worldDeltaY;
        }

        #endregion

        #region Utility

        public string GetScaleString()
        {
            return $"Scale: 1:{(1 / _zoomScale):F2}";
        }

        public string GetZoomPercentage()
        {
            return $"Zoom: {(_zoomScale * 100):F1}%";
        }

        #endregion
    }
}