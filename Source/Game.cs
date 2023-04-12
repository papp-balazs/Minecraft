namespace Minecraft;

using Minecraft.OpenGL;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SkiaSharp;
using System.Drawing;
using System.Reflection;

/// <summary>
/// The main game state/logic controller.
/// </summary>
public class Game : IDisposable
{
    #region Private Properties

    private uint vao;
    private OpenGL.Buffer? vbo;
    private OpenGL.Buffer? cbo;
    private OpenGL.Buffer? uvbo;
    private OpenGL.Buffer? ebo;
    private OpenGL.ShaderProgram? shader;
    private uint texture;

    #endregion

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

    #region Public Constructors/Finalizer

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
        graphicsOptions.API = ContextAPI.OpenGL;
        graphicsOptions.Version = new(4, 3);    
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
        MainWindow.MakeCurrent();

        vao = MainWindowGraphics.GenVertexArray();

        if (vao == 0)
        {
            throw new InvalidOperationException("Failed to generate vertrex array.");
        }

        MainWindowGraphics.BindVertexArray(vao);

        vbo = new OpenGL.Buffer(MainWindowGraphics, BufferTargetARB.ArrayBuffer);
        _ = vbo.Bind().UploadData(new float[]
        {
            -0.5f, -0.5f, 0.0f,
             0.5f, -0.5f, 0.0f,
            -0.5f,  0.5f, 0.0f,
             0.5f,  0.5f, 0.0f
        }, BufferUsageARB.StaticDraw);

        cbo = new OpenGL.Buffer(MainWindowGraphics, BufferTargetARB.ArrayBuffer);
        _ = cbo.Bind().UploadData(new float[]
        {
            1.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 1.0f
        }, BufferUsageARB.StaticDraw);

        uvbo = new OpenGL.Buffer(MainWindowGraphics, BufferTargetARB.ArrayBuffer);
        _ = uvbo.Bind().UploadData(new float[]
        {
            0.0f, 1.0f,
            1.0f, 1.0f,
            0.0f, 0.0f,
            1.0f, 0.0f
        }, BufferUsageARB.StaticDraw);

        ebo = new OpenGL.Buffer(MainWindowGraphics, BufferTargetARB.ElementArrayBuffer);
        _ = ebo.Bind().UploadData(new uint[]
        {
            0, 1, 2,
            1, 2, 3
        }, BufferUsageARB.StaticDraw);

        shader = OpenGL.ShaderProgram.FromSources(MainWindowGraphics, @"
#version 410 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aColor;
layout (location = 2) in vec2 aUV;

out vec3 vColor;
out vec2 vUV;

void main() {
    vColor = aColor;
    vUV = aUV;
    gl_Position = vec4(aPosition, 1.0);
}
", @"
#version 410 core

in vec3 vColor;
in vec2 vUV;

out vec4 fragColor;

uniform sampler2D uTexture;

void main() {
    vec4 tColor = texture(uTexture, vUV);
    vec4 fColor = vec4(vColor, 1.0);
    fragColor = tColor * fColor;
}
");

        var assemblyLocation = Path.GetDirectoryName(Path.GetFullPath(Assembly.GetExecutingAssembly().Location));

        if (assemblyLocation == null)
        {
            throw new InvalidOperationException("Cannot locate executing assembly.");
        }

        using var stream = File.OpenRead(Path.Combine(assemblyLocation, "Assets/texture.png"));
        var img = SKBitmap.Decode(stream);

        stream.Close();

        var imgData = img.Bytes;

        texture = MainWindowGraphics.GenTexture();
        MainWindowGraphics.ActiveTexture(TextureUnit.Texture0);
        MainWindowGraphics.BindTexture(TextureTarget.Texture2D, texture);
        MainWindowGraphics.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        MainWindowGraphics.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        MainWindowGraphics.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        MainWindowGraphics.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        MainWindowGraphics.TexImage2D<byte>(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)img.Width, (uint)img.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, imgData.AsSpan());
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
                MainWindow.IsClosing = true;
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

        MainWindow.MakeCurrent();

        MainWindowGraphics.Viewport(MainWindow.FramebufferSize);
        MainWindowGraphics.ClearColor(Color.CornflowerBlue);
        MainWindowGraphics.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        MainWindowGraphics.Enable(EnableCap.Blend);
        MainWindowGraphics.BlendEquation(BlendEquationModeEXT.FuncAdd);
        MainWindowGraphics.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        MainWindowGraphics.BindVertexArray(vao);
        _ = shader?.Activate();
        MainWindowGraphics.EnableVertexAttribArray(0);
        MainWindowGraphics.EnableVertexAttribArray(1);
        MainWindowGraphics.EnableVertexAttribArray(2);
        MainWindowGraphics.ActiveTexture(TextureUnit.Texture0);
        MainWindowGraphics.BindTexture(TextureTarget.Texture2D, texture);
        MainWindowGraphics.Uniform1(MainWindowGraphics.GetUniformLocation(shader?.ID ?? 0, "uTexture"), 0);

        MainWindowGraphics.VertexAttribFormat(0, 3, VertexAttribType.Float, false, 0);
        MainWindowGraphics.VertexAttribBinding(0, 0);
        MainWindowGraphics.BindVertexBuffer(0, vbo?.ID ?? 0, 0, 3 * sizeof(float));

        MainWindowGraphics.VertexAttribFormat(1, 3, VertexAttribType.Float, false, 0);
        MainWindowGraphics.VertexAttribBinding(1, 1);
        MainWindowGraphics.BindVertexBuffer(1, cbo?.ID ?? 0, 0, 3 * sizeof(float));

        MainWindowGraphics.VertexAttribFormat(2, 2, VertexAttribType.Float, false, 0);
        MainWindowGraphics.VertexAttribBinding(2, 2);
        MainWindowGraphics.BindVertexBuffer(2, uvbo?.ID ?? 0, 0, 2 * sizeof(float));

        _ = ebo?.Bind();
        MainWindowGraphics.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, ReadOnlySpan<uint>.Empty);

        MainWindowGraphics.DisableVertexAttribArray(2);
        MainWindowGraphics.DisableVertexAttribArray(1);
        MainWindowGraphics.DisableVertexAttribArray(0);
        _ = shader?.Deactivate();

        MainWindow.SwapBuffers();
    }

    /// <summary>
    /// The callback for when the main window is requesting to close.
    /// </summary>
    private void OnWindowClosing()
    {
        if (MainWindow == null)
        {
            return;
        }

        MainWindow.IsClosing = true;
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
            MainWindow = null;
        }

        IsDisposed = true;  
    }

    #endregion
}