using System.Runtime.InteropServices;
using SkiaSharp;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Skia
{
    /// <summary>
    /// Owns a per-thread WGL OpenGL context and the Skia GPU context built on
    /// top of it. One instance is created per calling thread on first use and
    /// kept alive for that thread's lifetime.
    /// </summary>
    internal sealed class SkiaGlContext : IDisposable
    {
        // ── Win32 types ──────────────────────────────────────────────────────────

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WNDCLASSEX
        {
            public uint cbSize;
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra, cbWndExtra;
            public IntPtr hInstance, hIcon, hCursor, hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)] public string? lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpszClassName;
            public IntPtr hIconSm;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PIXELFORMATDESCRIPTOR
        {
            public ushort nSize, nVersion;
            public uint dwFlags;
            public byte iPixelType, cColorBits;
            public byte cRedBits, cRedShift, cGreenBits, cGreenShift,
                         cBlueBits, cBlueShift, cAlphaBits, cAlphaShift;
            public byte cAccumBits, cAccumRedBits, cAccumGreenBits,
                         cAccumBlueBits, cAccumAlphaBits;
            public byte cDepthBits, cStencilBits, cAuxBuffers,
                         iLayerType, bReserved;
            public uint dwLayerMask, dwVisibleMask, dwDamageMask;
        }

        // ── Constants ────────────────────────────────────────────────────────────

        private const uint CS_OWNDC = 0x0020;
        private const uint WS_POPUP = 0x80000000u;
        private const uint PFD_DRAW_TO_WINDOW = 0x00000004;
        private const uint PFD_SUPPORT_OPENGL = 0x00000020;
        private const byte PFD_TYPE_RGBA = 0;
        private const string WndClass = "SkiaGpuOffscreenCtx";

        // ── P/Invoke ─────────────────────────────────────────────────────────────

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateWindowEx(uint exStyle, string cls, string name,
            uint style, int x, int y, int w, int h,
            IntPtr parent, IntPtr menu, IntPtr inst, IntPtr param);

        [DllImport("user32.dll")] private static extern bool DestroyWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")] private static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr GetModuleHandle(string? name);
        [DllImport("gdi32.dll")] private static extern int ChoosePixelFormat(IntPtr hdc, ref PIXELFORMATDESCRIPTOR pfd);
        [DllImport("gdi32.dll")] private static extern bool SetPixelFormat(IntPtr hdc, int fmt, ref PIXELFORMATDESCRIPTOR pfd);
        [DllImport("opengl32.dll")] private static extern IntPtr wglCreateContext(IntPtr hdc);
        [DllImport("opengl32.dll")] private static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);
        [DllImport("opengl32.dll")] private static extern bool wglDeleteContext(IntPtr hglrc);

        // ── Static state ─────────────────────────────────────────────────────────

        private static readonly object s_lock = new();
        private static readonly WndProcDelegate s_wndProc = (h, m, w, l) => DefWindowProc(h, m, w, l);
        private static readonly IntPtr s_wndProcPtr = Marshal.GetFunctionPointerForDelegate(s_wndProc);
        private static bool s_classRegistered;
        private static volatile int s_available; // 0=unknown, 1=yes, -1=no

        [ThreadStatic] private static SkiaGlContext? s_instance;

        // ── Instance state ───────────────────────────────────────────────────────

        private readonly IntPtr _hWnd;
        private readonly IntPtr _hDC;
        private readonly IntPtr _hRC;
        private bool _disposed;

        /// <summary>The Skia GPU context bound to this thread's GL context.</summary>
        public GRContext GrContext { get; }

        // ── Constructor ──────────────────────────────────────────────────────────

        private SkiaGlContext()
        {
            EnsureWindowClassRegistered();

            _hWnd = CreateWindowEx(0, WndClass, "", WS_POPUP, 0, 0, 1, 1,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if (_hWnd == IntPtr.Zero)
                throw new InvalidOperationException(
                    $"CreateWindowEx failed (error {Marshal.GetLastWin32Error()}).");

            _hDC = GetDC(_hWnd);
            if (_hDC == IntPtr.Zero)
            {
                DestroyWindow(_hWnd);
                throw new InvalidOperationException("GetDC failed.");
            }

            PIXELFORMATDESCRIPTOR pfd = new()
            {
                nSize = (ushort)Marshal.SizeOf<PIXELFORMATDESCRIPTOR>(),
                nVersion = 1,
                dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL,
                iPixelType = PFD_TYPE_RGBA,
                cColorBits = 32,
                cDepthBits = 24,
                cStencilBits = 8
            };

            int fmt = ChoosePixelFormat(_hDC, ref pfd);
            if (fmt == 0 || !SetPixelFormat(_hDC, fmt, ref pfd))
            {
                ReleaseDC(_hWnd, _hDC);
                DestroyWindow(_hWnd);
                throw new InvalidOperationException("SetPixelFormat failed.");
            }

            _hRC = wglCreateContext(_hDC);
            if (_hRC == IntPtr.Zero)
            {
                ReleaseDC(_hWnd, _hDC);
                DestroyWindow(_hWnd);
                throw new InvalidOperationException("wglCreateContext failed.");
            }

            if (!wglMakeCurrent(_hDC, _hRC))
            {
                wglDeleteContext(_hRC);
                ReleaseDC(_hWnd, _hDC);
                DestroyWindow(_hWnd);
                throw new InvalidOperationException("wglMakeCurrent failed.");
            }

            // Prefer ANGLE (Direct3D 11) so Skia targets the high-performance discrete
            // GPU selected by Windows rather than the iGPU that WGL defaults to on
            // multi-GPU laptops. Falls back to native WGL if ANGLE DLLs are absent.
            GRGlInterface? iface = GRGlInterface.CreateAngle()
                                ?? GRGlInterface.Create();
            GRContext? ctx = iface != null ? GRContext.CreateGl(iface) : null;
            if (ctx == null)
            {
                iface?.Dispose();
                wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
                wglDeleteContext(_hRC);
                ReleaseDC(_hWnd, _hDC);
                DestroyWindow(_hWnd);
                throw new InvalidOperationException(
                    "GRContext.CreateGl failed — driver may not support OpenGL or ANGLE.");
            }

            GrContext = ctx;
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// True when the GPU context can be created on this machine. Cached after
        /// the first probe — safe to call from any thread.
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
                if (s_available != 0)
                    return s_available > 0;

                lock (s_lock)
                {
                    if (s_available != 0)
                        return s_available > 0;

                    try
                    {
                        using SkiaGlContext probe = new();
                        s_available = 1;
                    }
                    catch
                    {
                        s_available = -1;
                    }
                }

                return s_available > 0;
            }
        }

        /// <summary>
        /// Returns the GL context for the calling thread, creating it on first call.
        /// The context is made current before returning so subsequent GL/Skia calls
        /// on this thread are routed to it.
        /// </summary>
        public static SkiaGlContext GetOrCreateForCurrentThread()
        {
            if (s_instance == null || s_instance._disposed)
            {
                s_instance = new SkiaGlContext();
            }
            else
            {
                wglMakeCurrent(s_instance._hDC, s_instance._hRC);
            }

            return s_instance;
        }

        // ── IDisposable ──────────────────────────────────────────────────────────

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            GrContext.Dispose();
            wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            wglDeleteContext(_hRC);
            ReleaseDC(_hWnd, _hDC);
            DestroyWindow(_hWnd);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static void EnsureWindowClassRegistered()
        {
            if (s_classRegistered)
                return;

            lock (s_lock)
            {
                if (s_classRegistered)
                    return;

                WNDCLASSEX wc = new()
                {
                    cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>(),
                    style = CS_OWNDC,
                    lpfnWndProc = s_wndProcPtr,
                    lpszClassName = WndClass,
                    hInstance = GetModuleHandle(null)
                };

                RegisterClassEx(ref wc); // failure means already registered — that is fine
                s_classRegistered = true;
            }
        }
    }
}
