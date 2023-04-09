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
    /// The main game window.
    /// </summary>
    public IWindow? MainWindow
    {
        get;
        private set;
    }

    /// <summary>
    /// The main input context.
    /// </summary>
    public IInputContext? MainWindowInput
    {
        get;
        private set;
    }

    /// <summary>
    /// The main GPU context.
    /// </summary>
    public GL? MainWindowGraphics
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
    /// <exception cref="InvalidOperationException">
    /// Thrown if the instance is already initialized.
    /// </exception>
    public void Initialize()
    {
        if (MainWindow != null)
        {
            throw new InvalidOperationException("Instance is already initialized.");
        }

        var graphicsOptions = GraphicsAPI.Default;
        graphicsOptions.Version = new(4, 5);    
        graphicsOptions.Profile = ContextProfile.Core;
        graphicsOptions.Flags = ContextFlags.ForwardCompatible;

        var windowOptions = WindowOptions.Default;
        windowOptions.API = graphicsOptions;
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


        MainWindow = Window.Create(windowOptions);
        MainWindow.Load += OnWindowLoad;
        MainWindow.Update += OnWindowUpdate;
        MainWindow.Render += OnWindowRender;
        MainWindow.Closing += OnWindowClosing;
    }

    /// <summary>
    /// Run the game.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the main game window does not exist.
    /// </exception>
    public void Run()
    {
        if (MainWindow == null)
        {
            throw new InvalidOperationException("Instance is not initialized.");
        }

        MainWindow.Run();
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

    /// <summary>
    /// The callback for when the main window is loaded.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the main game window does not exist.
    /// </exception>
    private void OnWindowLoad()
    {
        if (MainWindow == null)
        {
            throw new InvalidOperationException("Main game window does not exist.");
        }

        MainWindowInput = MainWindow.CreateInput();
        MainWindowGraphics = MainWindow.CreateOpenGL();

        MainWindow.Center();
        MainWindow.IsVisible = true;
    }

    /// <summary>
    /// The callback for when the main window is updated.
    /// </summary>
    /// <param name="delta">
    /// The time, in fractional seconds since the last update.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the main game window or its input does not exist.
    /// </exception>
    private void OnWindowUpdate(double delta)
    {
        if (MainWindow == null)
        {
            throw new InvalidOperationException("Main game window does not exist.");
        }

        if (MainWindowInput == null)
        {
            throw new InvalidOperationException("Main game window input is null.");
        }

        foreach (var kb in MainWindowInput.Keyboards)
        {
            if (kb.IsKeyPressed(Key.Escape))
            {
                MainWindow.Close();
            }
        }
    }

    private void OnWindowRender(double delta)
    {
        if (MainWindow == null)
        {
            throw new InvalidOperationException("Main game window does not exist.");
        }

        if (MainWindowGraphics == null)
        {
            throw new InvalidOperationException("Main game window graphics is null.");
        }

        MainWindowGraphics.Viewport(MainWindow?.FramebufferSize ?? new(0, 0));
        MainWindowGraphics.ClearColor(Color.CornflowerBlue);
        MainWindowGraphics.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        MainWindow?.SwapBuffers();
    }

    /// <summary>
    /// The callback for when the main window is requesting to close.
    /// </summary>
    private void OnWindowClosing()
    {
        Dispose();
    }

    /// <summary>
    /// Dispose this instance and the resources its managing.
    /// </summary>
    /// <param name="managed">
    /// Whether this method is being called from managed code or from unmanaged code (e.g. the garbage collector).
    /// </param>
    private void Dispose(bool managed)
    {
        if (IsDisposed)
        {
            return;
        }

        if (managed)
        {
            MainWindowGraphics?.Dispose();
            MainWindowGraphics = null;
            MainWindowInput?.Dispose();
            MainWindowInput = null;
            MainWindow?.Dispose();
            MainWindow = null;
        }

        IsDisposed = true;  
    }

    #endregion
}