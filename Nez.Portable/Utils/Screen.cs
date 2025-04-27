using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez;

public static class Screen
{
    internal static GraphicsDeviceManager _graphicsManager;

    private static Vector2? _customSize;

    public static Point[] AvailableResolutions { get; set; } = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes
        .Select(x => new Point(x.Width, x.Height)).GroupBy(x => x).Select(x => x.First()).OrderBy(x => x.Y)
        .OrderBy(x => x.X).ToArray();

    /// <summary>
    ///     width of the GraphicsDevice back buffer
    /// </summary>
    /// <value>The width.</value>
    public static int BackbufferWidth
    {
        get => _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferWidth;
        set => _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferWidth = value;
    }

    /// <summary>
    ///     height of the GraphicsDevice back buffer
    /// </summary>
    /// <value>The height.</value>
    public static int BackbufferHeight
    {
        get => _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferHeight;
        set => _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferHeight = value;
    }

    public static int Width => (int)Size.X;
    public static int Height => (int)Size.Y;

    /// <summary>
    ///     gets the Screen's size as a Vector2
    /// </summary>
    /// <value>The screen size.</value>
    public static Vector2 Size => _customSize ?? new Vector2(PreferredBackBufferWidth, PreferredBackBufferHeight);

    /// <summary>
    ///     gets the Screen's center.null Note that this is the center of the backbuffer! If you are rendering to a smaller
    ///     RenderTarget
    ///     you will need to scale this value appropriately.
    /// </summary>
    /// <value>The center.</value>
    public static Vector2 Center => Size * 0.5f;

    public static int PreferredBackBufferWidth
    {
        get => _graphicsManager.PreferredBackBufferWidth;
        set => _graphicsManager.PreferredBackBufferWidth = value;
    }

    public static int PreferredBackBufferHeight
    {
        get => _graphicsManager.PreferredBackBufferHeight;
        set => _graphicsManager.PreferredBackBufferHeight = value;
    }


    public static int MonitorWidth => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;

    public static int MonitorHeight => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

    public static SurfaceFormat BackBufferFormat =>
        _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferFormat;

    public static SurfaceFormat PreferredBackBufferFormat
    {
        get => _graphicsManager.PreferredBackBufferFormat;
        set => _graphicsManager.PreferredBackBufferFormat = value;
    }

    public static bool SynchronizeWithVerticalRetrace
    {
        get => _graphicsManager.SynchronizeWithVerticalRetrace;
        set => _graphicsManager.SynchronizeWithVerticalRetrace = value;
    }

    // defaults to Depth24Stencil8
    public static DepthFormat PreferredDepthStencilFormat
    {
        get => _graphicsManager.PreferredDepthStencilFormat;
        set => _graphicsManager.PreferredDepthStencilFormat = value;
    }

    public static bool IsFullscreen
    {
        get => _graphicsManager.IsFullScreen;
        set => _graphicsManager.IsFullScreen = value;
    }

    public static DisplayOrientation SupportedOrientations
    {
        get => _graphicsManager.SupportedOrientations;
        set => _graphicsManager.SupportedOrientations = value;
    }

    public static bool EnableAA
    {
        get => _graphicsManager.PreferMultiSampling;
        set => _graphicsManager.PreferMultiSampling = value;
    }

    //MSAA currently not supported by MonoGame Desktop GL
    public static int AASamples { get; set; } = 0;

    internal static void Initialize(GraphicsDeviceManager graphicsManager)
    {
        _graphicsManager = graphicsManager;
    }

    /// <summary>
    ///     Overrides Screen.Size with an apparent value. Usefull when your scene size is different from your backbuffer size.
    /// </summary>
    /// <param name="size">The new apparent size.</param>
    public static void SetSizeOverride(Vector2? size)
    {
        _customSize = size;
    }

    public static void ApplyChanges()
    {
        _graphicsManager.ApplyChanges();
    }

    public static GraphicsDeviceManager GetManager()
    {
        return _graphicsManager;
    }

    /// <summary>
    ///     sets the preferredBackBuffer then applies the changes
    /// </summary>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    public static void SetSize(int width, int height)
    {
        PreferredBackBufferWidth = width;
        PreferredBackBufferHeight = height;
        ApplyChanges();
    }
}