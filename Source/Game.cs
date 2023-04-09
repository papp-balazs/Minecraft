namespace Minecraft;

using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Drawing;

/// <summary>
/// The main game state/logic controller.
/// </summary>
public sealed class Game : IDisposable
{
    #region Public Properties

    /// <summary>
    /// The main game window
    /// </summary>
    public IWindow? Window
    {
        get;
        private set;
    }

    /// <summary>
    /// The main input context.
    /// </summary>
    public IInputContext? Input
    {
        get;
        private set;
    }

    /// <summary>
    /// The main GPU context.
    /// </summary>
    public GL? GraphicsContext
    {
        get;
        private set;
    }

    /// <summary>
    /// Whether this instance has been disposed.
    /// </summary>
    public bool IsDisposed
    {
        get;
        private set;
    }

    #endregion

    #region Public Constructors/Finalizers

    /// <summary>
    /// Create a new instance.
    /// </summary>
    public Game()
    { }

    /// <summary>
    /// The finalizer.
    /// </summary>
    ~Game()
    {
        Dispose(false);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initialize the game.
    /// </summary>
    public void Init()
    {
        var glOptions = GraphicsAPI.Default;
        glOptions.Version = new(4, 5);
        glOptions.Profile = ContextProfile.Core;
        glOptions.Flags = ContextFlags.ForwardCompatible;

        var windowOptions = WindowOptions.Default;
        windowOptions.API = glOptions;
        windowOptions.Size = new(1280, 720);
        windowOptions.Title = "Minecraft";
        windowOptions.WindowClass = "Minecraft";
        windowOptions.IsVisible = false;
        windowOptions.WindowState = WindowState.Normal;
        windowOptions.WindowBorder = WindowBorder.Resizable;
        windowOptions.VSync = false;
        windowOptions.Samples = 0;
        windowOptions.ShouldSwapAutomatically = false;
        windowOptions.FramesPerSecond = 60;
        windowOptions.UpdatesPerSecond = 60;


        Window = Silk.NET.Windowing.Window.Create(windowOptions);
        Window.Load += OnWindowLoad;
        Window.Update += OnWindowUpdate;
        Window.Render += OnWindowRender;
        Window.Closing += OnWindowClosing;
    }

    /// <summary>
    /// Run the game.
    /// </summary>
    public void Run()
    {

        Window?.Run();
    }

    /// <summary>
    /// Dispose this instance and the resources its managing.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Private Methods

    private void OnWindowLoad()
    {
        if (Window == null)
        {
            throw new InvalidOperationException("Main game window does not exist.");
        }

        Input = Window.CreateInput();
        GraphicsContext = Window.CreateOpenGL();

        Window.Center();
        Window.IsVisible = true;
    }

    private void OnWindowUpdate(double delta)
    {
        var kbs = Input?.Keyboards ?? Array.Empty<IKeyboard>();

        foreach (var kb in kbs)
        {
            if (kb.IsKeyPressed(Key.Escape))
            {
                Window?.Close();
            }
        }
    }

    private void OnWindowRender(double delta)
    {
        var fbSize = Window?.FramebufferSize;

        if (fbSize != null)
        {
            GraphicsContext?.Viewport(fbSize.Value);
        }

        GraphicsContext?.ClearColor(Color.CornflowerBlue);
        GraphicsContext?.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        Window?.SwapBuffers();
    }

    private void OnWindowClosing()
    {
        GraphicsContext?.Dispose();
        GraphicsContext = null;
        Input?.Dispose();
        Input = null;
    }

    /// <summary>
    /// Dispose this instance and the resources its managing.
    /// </summary>
    /// <param name="managed">
    /// Whether this method is being called from managed code or from unmanaged code (e.g. the garbage collector).
    /// </param>
    private void Dispose(bool managed)
    {
        if (IsDisposed) return;
        if (managed)
        {
            Window?.Dispose();
            Window = null;
        }

        IsDisposed = true;
    }

    #endregion
}