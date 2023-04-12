namespace Minecraft.OpenGL;

using Silk.NET.OpenGL;

/// <summary>
/// An OpenGL shader.
/// </summary>
public class Shader : IDisposable
{
    #region Public Static Methods

    /// <summary>
    /// Create and compile a new instance from source code.
    /// </summary>
    /// <param name="context">
    /// The OpenGL context that will own the new instance.
    /// </param>
    /// <param name="type">
    /// The type of instance to create.
    /// </param>
    /// <param name="source">
    /// The source code to upload and compile with.
    /// </param>
    /// <returns>
    /// The new instance.
    /// </returns>
    public static Shader FromSource(GL context, ShaderType type, string source) =>
        new Shader(context, type).UploadSource(source).Compile();

    /// <summary>
    /// Create and compile a new instance from the source code in a file.
    /// </summary>
    /// <param name="context">
    /// The OpenGL context that will own the new instance.
    /// </param>
    /// <param name="type">
    /// The type of instance to create.
    /// </param>
    /// <param name="path">
    /// The path to the file containing the source code to upload and compile with.
    /// </param>
    /// <returns>
    /// The new instance.
    /// </returns>
    public static Shader FromSourceFile(GL context, ShaderType type, string path) =>
        FromSource(context, type, File.ReadAllText(path));

    #endregion

    #region Public Properties

    /// <summary>
    /// The OpenGL context that own this instance.
    /// </summary>
    public GL Context
    {
        get;
    }

    /// <summary>
    /// The OpenGL object ID.
    /// </summary>
    public uint ID
    {
        get;
        private set;
    }

    /// <summary>
    /// The type of this instance.
    /// </summary>
    public ShaderType Type
    {
        get;
    }

    /// <summary>
    /// The source code that has been uploaded to this instance.
    /// </summary>
    /// <seealso cref="UploadSource" />
    public string? Source
    {
        get;
        private set;
    }

    /// <summary>
    /// The information log from the last call to compile the instance.
    /// </summary>
    public string? InfoLog
    {
        get
        {
            if (IsDisposed || !IsValid)
            {
                return null;
            }

            Context.GetShaderInfoLog(ID, out var log);

            return log;
        }
    }

    /// <summary>
    /// Whether the instance has been disposed.
    /// </summary>
    public bool IsDisposed
    {
        get;
        private set;
    }

    /// <summary>
    /// Whether this instance wraps a valid OpenGL object.
    /// </summary>
    public bool IsValid =>
        ID != 0;

    /// <summary>
    /// Whether this instance has been compiled.
    /// </summary>
    /// <seealso cref="Compile" />
    /// <seealso cref="InfoLog" />
    public bool IsCompiled
    {
        get
        {
            if (IsDisposed || !IsValid)
            {
                return false;
            }

            Context.GetShader(ID, ShaderParameterName.CompileStatus, out var status);

            return status == 1;
        }
    }

    #endregion

    #region Constructors/Finalizer

    /// <summary>
    /// Create a new instance.
    /// </summary>
    /// <param name="context">
    /// The OpenGL context that will own this instance.
    /// </param>
    /// <param name="type">
    /// The type of instance to create.
    /// </param>
    /// <exception cref="GLException">
    /// Thrown if the OpenGL context returns an error.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the OpenGL context fails to create a native shader.
    /// </exception>
    public Shader(GL context, ShaderType type)
    {
        Context = context;
        Type = type;
        ID = Context.CreateShader(type);

        var err = (ErrorCode)Context.GetError();

        if (err != ErrorCode.NoError)
        {
            throw new GLException(err, "Failed to create an OpenGL shader.");
        }

        if (ID == 0)
        {
            throw new InvalidOperationException("Failed to create an OpenGL shader.");
        }
    }

    /// <summary>
    /// The finalizer.
    /// </summary>
    ~Shader() =>
        Dispose(false);

    #endregion

    #region Public Methods

    /// <summary>
    /// Upload source code to this instance.
    /// </summary>
    /// <param name="source">
    /// The source code to upload. Each element in the array will be uploaded as a separate line.
    /// </param>
    /// <returns>
    /// This instance for chaining.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if this instance has been disposed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this instance is invalid.
    /// </exception>
    /// <exception cref="GLException">
    /// Thrown if the OpenGL context reports an error.
    /// </exception>
    /// <seealso cref="Source" />
    /// <seealso cref="UploadSource(string[]) "/>
    public Shader UploadSource(string source)
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(Shader), "Cannot upload source code to a disposed OpenGL shader.");
        }

        if (!IsValid)
        {
            throw new InvalidOperationException("Cannot upload source code to an invalid OpenGL shader.");
        }

        Context.ShaderSource(ID, source);
        var err = (ErrorCode)Context.GetError();

        if (err != ErrorCode.NoError)
        {
            throw new GLException(err, "Failed to upload source code to an OpenGL shader.");
        }

        Source = source;

        return this;
    }

    /// <summary>
    /// Upload source code to this instance.
    /// </summary>
    /// <param name="source">
    /// The source code to upload. Each element in the array will be uploaded as a separate line.
    /// </param>
    /// <returns>
    /// This instance for chaining.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if this instance has been disposed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this instance is invalid.
    /// </exception>
    /// <exception cref="GLException">
    /// Thrown if the OpenGL context reports an error.
    /// </exception>
    /// <seealso cref="Source" />
    /// <seealso cref="UploadSource(string) "/>
    public Shader UploadSource(string[] sources)
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(Shader), "Cannot upload source code to a disposed OpenGL shader.");
        }

        if (!IsValid)
        {
            throw new InvalidOperationException("Cannot upload source code to an invalid OpenGL shader.");
        }

        var source = string.Join("\n", sources);

        Context.ShaderSource(ID, source);
        var err = (ErrorCode)Context.GetError();

        if (err != ErrorCode.NoError)
        {
            throw new GLException(err, "Failed to upload source code to an OpenGL shader.");
        }

        Source = source;

        return this;
    }

    /// <summary>
    /// Attempt to compile the instance.
    /// </summary>
    /// <returns>
    /// This instance for chaining.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if this instance has been disposed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this instance is invalid.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this instance has no source code.
    /// </exception>
    /// <exception cref="GLException">
    /// Thrown if the OpenGL context reports an error.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the OpenGL context fails to compile the shader.
    /// </exception>
    /// <seealso cref="IsCompiled" />
    /// <seealso cref="InfoLog" />
    public Shader Compile()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(Shader), "Cannot compile a disposed OpenGL shader.");
        }

        if (!IsValid)
        {
            throw new InvalidOperationException("Cannot compile an invalid OpenGL shader.");
        }

        if (Source == null)
        {
            throw new InvalidOperationException("Cannot compile an OpenGL shader without source code.");
        }

        Context.CompileShader(ID);
        var err = (ErrorCode)Context.GetError();

        if (err != ErrorCode.NoError)
        {
            throw new GLException(err, "Failed to compile an OpenGL shader.");
        }

        if (!IsCompiled)
        {
            var infoLog = InfoLog;

            if (string.IsNullOrWhiteSpace(infoLog))
            {
                throw new InvalidOperationException("Failed to compile an OpenGL shader.");
            }
            else
            {
                throw new InvalidOperationException($"Failed to compile an OpenGL shader: {infoLog}");
            }
        }

        return this;
    }

    /// <summary>
    /// Dispose of this instance.
    /// </summary>
    /// <seealso cref="IsDisposed" />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Dispose of this instance.
    /// </summary>
    /// <param name="managed">
    /// Whether this method is being called from managed or unmanaged code (e. g. the garbage collector).
    /// </param>
    /// <seealso cref="Dispose" />
    /// <seealso cref="IsDisposed" />
    private void Dispose(bool managed)
    {
        if (IsDisposed)
        {
            return;
        }

        if (managed && IsValid)
        {
            Context.DeleteShader(ID);
        }

        ID = 0;
        Source = null;
        IsDisposed = true;
    }

    #endregion
}