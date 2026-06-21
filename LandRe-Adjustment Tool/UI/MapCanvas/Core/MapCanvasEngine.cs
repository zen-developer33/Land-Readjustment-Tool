using System.Drawing;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.UI.MapCanvas.Core
{
    /// <summary>
    /// UI-independent viewport engine for GIS/CAD style map navigation.
    /// World coordinates use a Y-up coordinate system; screen coordinates use WinForms Y-down.
    /// </summary>
    public sealed class MapCanvasEngine
    {
        public const double MinZoom = 0.000001;
        // Maximum zoom-in is capped at map scale 1:0.001. ZoomScale is screen
        // pixels per world metre, and scale denominator = (96 DPI / 0.0254) / ZoomScale,
        // so denominator 0.001 corresponds to this ZoomScale. Do not allow more.
        public const double MaxZoom = 96.0 / 0.0254 / 0.001;
        // Maximum zoom-out is capped at map scale 1:25,000,000. Using the same
        // denominator relationship as above, that scale corresponds to this minimum
        // ZoomScale; the user cannot zoom out past it (the displayed scale stops here).
        public const double MaxScaleDenominator = 25_000_000.0;
        public const double MinScaleZoom = 96.0 / 0.0254 / MaxScaleDenominator;
        public const double ZoomStep = 1.4;
        private const double DefaultInitialCenterX = 245426.0206;
        private const double DefaultInitialCenterY = 3121303.7884;
        private const double DefaultInitialViewWidth = 8000.0;
        private const double DefaultInitialViewHeight = 6000.0;
        private const double GlobalProjectedExtent = 20037508.342789244;

        private Size _canvasSize;
        private double _zoomScale = 1.0;
        private PointD _viewOriginWorld = new(0.0, 0.0);

        private MapCanvasEngine(
            Size canvasSize,
            double zoomScale,
            PointD viewOriginWorld,
            RectangleD worldBounds)
        {
            _canvasSize = ClampSize(canvasSize);
            _zoomScale = zoomScale;
            _viewOriginWorld = viewOriginWorld;
            WorldBounds = worldBounds;
        }

        public MapCanvasEngine(Size canvasSize)
        {
            _canvasSize = ClampSize(canvasSize);
            WorldBounds = DefaultWorldBounds;
            SetView(
                DefaultInitialCenterX,
                DefaultInitialCenterY,
                DefaultInitialViewWidth,
                DefaultInitialViewHeight);
        }

        public static RectangleD DefaultWorldBounds => new(240000, 3060000, 300000, 120000);
        public static RectangleD GlobalWorldBounds => new(
            -GlobalProjectedExtent,
            -GlobalProjectedExtent,
            GlobalProjectedExtent * 2.0,
            GlobalProjectedExtent * 2.0);

        public Size CanvasSize => _canvasSize;

        public double ZoomScale => _zoomScale;

        public PointD ViewOriginWorld => _viewOriginWorld;

        public RectangleD WorldBounds { get; private set; } = DefaultWorldBounds;

        public MapCanvasEngine CreateSnapshot()
        {
            return new MapCanvasEngine(
                _canvasSize,
                _zoomScale,
                _viewOriginWorld,
                WorldBounds);
        }

        public void UpdateCanvasSize(Size canvasSize)
        {
            _canvasSize = ClampSize(canvasSize);
        }

        public void SetWorldBounds(RectangleD worldBounds)
        {
            if (worldBounds.Width <= 0 || worldBounds.Height <= 0)
            {
                return;
            }

            WorldBounds = worldBounds;
        }

        public PointD ScreenToWorld(Point screenPoint)
        {
            return ScreenToWorld(new PointD(screenPoint.X, screenPoint.Y));
        }

        public PointD ScreenToWorld(PointD screenPoint)
        {
            double worldX = _viewOriginWorld.X + screenPoint.X / _zoomScale;
            double worldY = _viewOriginWorld.Y + (_canvasSize.Height - screenPoint.Y) / _zoomScale;
            return new PointD(worldX, worldY);
        }

        public PointD WorldToScreen(PointD worldPoint)
        {
            double screenX = (worldPoint.X - _viewOriginWorld.X) * _zoomScale;
            double screenY = _canvasSize.Height - ((worldPoint.Y - _viewOriginWorld.Y) * _zoomScale);
            return new PointD(screenX, screenY);
        }

        public double ScreenToWorldDistance(double screenDistance)
        {
            return screenDistance / _zoomScale;
        }

        public RectangleD GetVisibleWorldBounds()
        {
            PointD topLeft = ScreenToWorld(new PointD(0, 0));
            PointD bottomRight = ScreenToWorld(new PointD(_canvasSize.Width, _canvasSize.Height));

            double left = Math.Min(topLeft.X, bottomRight.X);
            double right = Math.Max(topLeft.X, bottomRight.X);
            double bottom = Math.Min(topLeft.Y, bottomRight.Y);
            double top = Math.Max(topLeft.Y, bottomRight.Y);

            return new RectangleD(left, bottom, right - left, top - bottom);
        }

        public RectangleD GetClipWorldBounds(double worldMargin)
        {
            RectangleD visible = GetVisibleWorldBounds();
            double margin = double.IsFinite(worldMargin) && worldMargin > 0.0
                ? worldMargin
                : 0.0;

            return new RectangleD(
                visible.Left - margin,
                visible.Top - margin,
                visible.Width + margin * 2.0,
                visible.Height + margin * 2.0);
        }

        public void SetViewport(PointD centerWorld, double zoomScale)
        {
            if (!double.IsFinite(centerWorld.X) ||
                !double.IsFinite(centerWorld.Y) ||
                !double.IsFinite(zoomScale) ||
                zoomScale <= 0)
            {
                return;
            }

            _zoomScale = Math.Clamp(zoomScale, GetEffectiveMinZoom(), MaxZoom);

            _viewOriginWorld = new PointD(
                centerWorld.X - (_canvasSize.Width / 2.0) / _zoomScale,
                centerWorld.Y - (_canvasSize.Height / 2.0) / _zoomScale);
        }

        public void SnapViewOriginToPixelGrid()
        {
            if (_zoomScale <= 0 || !double.IsFinite(_zoomScale))
            {
                return;
            }

            double snappedX = Math.Round(_viewOriginWorld.X * _zoomScale) / _zoomScale;
            double snappedY = Math.Round(_viewOriginWorld.Y * _zoomScale) / _zoomScale;
            _viewOriginWorld = new PointD(snappedX, snappedY);
        }

        public void ZoomIn()
        {
            ZoomAtCenter(ZoomStep);
        }

        public void ZoomOut()
        {
            ZoomAtCenter(1.0 / ZoomStep);
        }

        public void ZoomAtCenter(double zoomFactor)
        {
            ZoomAtPoint(new Point(_canvasSize.Width / 2, _canvasSize.Height / 2), zoomFactor);
        }

        public void ZoomAtPoint(Point screenPoint, double zoomFactor)
        {
            if (zoomFactor <= 0)
            {
                return;
            }

            ZoomAtPointToScale(screenPoint, _zoomScale * zoomFactor);
        }

        public void ZoomAtPointToScale(Point screenPoint, double zoomScale)
        {
            if (zoomScale <= 0 || !double.IsFinite(zoomScale))
            {
                return;
            }

            PointD worldBeforeZoom = ScreenToWorld(screenPoint);
            double newZoom = Math.Clamp(zoomScale, GetEffectiveMinZoom(), MaxZoom);

            if (Math.Abs(newZoom - _zoomScale) < 0.000000001)
            {
                return;
            }

            _zoomScale = newZoom;

            _viewOriginWorld = new PointD(
                worldBeforeZoom.X - screenPoint.X / _zoomScale,
                worldBeforeZoom.Y - (_canvasSize.Height - screenPoint.Y) / _zoomScale);
        }

        public void ZoomToExtents()
        {
            ZoomToExtents(WorldBounds);
        }

        public void ZoomToExtents(RectangleD worldBounds, double padding = 0.9)
        {
            if (worldBounds.Width <= 0 || worldBounds.Height <= 0)
            {
                return;
            }

            if (worldBounds.Width < 10.0)
            {
                worldBounds.Width = 100.0;
            }

            if (worldBounds.Height < 10.0)
            {
                worldBounds.Height = 100.0;
            }

            double zoomX = _canvasSize.Width / worldBounds.Width;
            double zoomY = _canvasSize.Height / worldBounds.Height;
            _zoomScale = Math.Clamp(Math.Min(zoomX, zoomY) * padding, GetEffectiveMinZoom(), MaxZoom);

            double centerX = worldBounds.X + worldBounds.Width / 2.0;
            double centerY = worldBounds.Y + worldBounds.Height / 2.0;

            _viewOriginWorld = new PointD(
                centerX - (_canvasSize.Width / 2.0) / _zoomScale,
                centerY - (_canvasSize.Height / 2.0) / _zoomScale);
        }

        public void SetView(double worldCenterX, double worldCenterY, double worldWidth, double worldHeight)
        {
            if (worldWidth <= 0 || worldHeight <= 0)
            {
                return;
            }

            double zoomX = _canvasSize.Width / worldWidth;
            double zoomY = _canvasSize.Height / worldHeight;
            _zoomScale = Math.Clamp(Math.Min(zoomX, zoomY) * 0.9, GetEffectiveMinZoom(), MaxZoom);

            double screenCenterX = _canvasSize.Width / 2.0;
            double screenCenterY = _canvasSize.Height / 2.0;

            _viewOriginWorld = new PointD(
                worldCenterX - screenCenterX / _zoomScale,
                worldCenterY - ((_canvasSize.Height - screenCenterY) / _zoomScale));
        }

        /// <summary>
        /// Pans the current view by a screen-space delta.
        /// Positive <paramref name="screenDeltaX"/> moves the view right on screen,
        /// and positive <paramref name="screenDeltaY"/> moves the view down on screen.
        /// </summary>
        /// <param name="screenDeltaX">Horizontal pan amount in screen pixels.</param>
        /// <param name="screenDeltaY">Vertical pan amount in screen pixels.</param>
        public void PanByScreenDelta(double screenDeltaX, double screenDeltaY)
        {
            _viewOriginWorld = new PointD(
                _viewOriginWorld.X - screenDeltaX / _zoomScale,
                _viewOriginWorld.Y + screenDeltaY / _zoomScale);
        }

        /// <summary>
        /// Sets the view origin directly from a known pan-start world position
        /// and a total screen-pixel delta. This avoids sub-pixel drift from
        /// accumulating many incremental pan steps.
        /// </summary>
        public void SetViewOriginFromPanStart(
            PointD panStartWorldOrigin,
            double totalScreenDx,
            double totalScreenDy)
        {
            _viewOriginWorld = new PointD(
                panStartWorldOrigin.X - totalScreenDx / _zoomScale,
                panStartWorldOrigin.Y + totalScreenDy / _zoomScale);
        }

        public string GetZoomLabel()
        {
            return $"Zoom: {_zoomScale * 100.0:F1}%";
        }

        public string GetScaleLabel()
        {
            return $"Scale: 1:{(1.0 / _zoomScale):F2}";
        }

        /// <summary>
        /// Lower bound applied to every zoom operation. It is the more restrictive
        /// (larger) of the fit-to-world-bounds zoom and the 1:25,000,000 scale cap, so
        /// the user can never zoom out past that scale.
        /// </summary>
        private double GetEffectiveMinZoom()
        {
            return Math.Max(GetMinZoomForWorldBounds(), MinScaleZoom);
        }

        private double GetMinZoomForWorldBounds()
        {
            double zoomX = _canvasSize.Width / WorldBounds.Width;
            double zoomY = _canvasSize.Height / WorldBounds.Height;
            double worldZoomX = _canvasSize.Width / GlobalWorldBounds.Width;
            double worldZoomY = _canvasSize.Height / GlobalWorldBounds.Height;

            zoomX = Math.Min(zoomX, worldZoomX);
            zoomY = Math.Min(zoomY, worldZoomY);

            return Math.Min(zoomX, zoomY) * 0.9;
        }

        private static Size ClampSize(Size size)
        {
            return new Size(Math.Max(size.Width, 1), Math.Max(size.Height, 1));
        }
    }
}
