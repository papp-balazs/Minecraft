namespace Minecraft.OpenGL;

using Silk.NET.OpenGL;

/// <summary>
/// An OpenGL shader program.
/// </summary>
public class ShaderProgram : IDisposable
{
    #region Public Static Methods

    /// <summary>
    /// Create a new instance from the given vertex and fragment shader sources.
    /// </summary>
    /// <param name="context">
    /// The OpenGL context that will own this instance.
    /// </param>
    /// <param name="vertex">
    /// The vertex shader source code.
    /// </param>
    /// <param name="fragment">
    /// The fragment shader source code.
    /// </param>
    /// <returns>
    /// The new instance.
    /// </returns>
    public static ShaderProgram FromSources(GL context, string vertex, string fragment) =>
        new ShaderProgram(context)
            .AttachShader(Shader.FromSource(context, ShaderType.VertexShader, vertex))
            .AttachShader(Shader.FromSource(context, ShaderType.FragmentShader, fragment))
            .Link();

    /// <summary>
    /// Create a new instance from the given vertex and fragment shader sources.
    /// </summary>
    /// <param name="context">
    /// The OpenGL context that will own this instance.
    /// </param>
    /// <param name="vertexPath">
    /// The path to the file containing the vertex shader source code.
    /// </param>
    /// <param name="fragmentPath">
    /// The path to the file containing the fragment shader source code.
    /// </param>
    /// <returns>
    /// The new instance.
    /// </returns>
    public static ShaderProgram FromFiles(GL context, string vertexPath, string fragmentPath) =>
        FromSources(context, File.ReadAllText(vertexPath), File.ReadAllText(fragmentPath));

    #endregion

    #region Private Fields

    /// <summary>
    /// A list of the shaders attached to this instance.
    /// </summary>
    private readonly IList<Shader> attachedShaders;

    #endregion

    #region Public Properties

    /// <summary>
    /// The OpenGL context that owns this instance.
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
    /// The information log from the last call to link this instance.
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
    /// Whether this instance has been disposed.
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
    /// Whether this instance has been linked.
    /// </summary>
    /// <seealso cref="Link" />
    /// <seealso cref="InfoLog" />
    public bool IsLinked
    {
        get
        {
            if (IsDisposed || !IsValid)
            {
                return false;
            }

            Context.GetProgram(ID, ProgramPropertyARB.LinkStatus, out var status);

            return status == 1;
        }
    }

    public bool IsActive
    {
        get
        {
            if (IsDisposed || !IsValid)
            {
                return false;
            }

            Context.GetInteger(GetPName.CurrentProgram, out var id);

            return id == ID;
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
    /// <exception cref="GLException">
    /// Thrown if the OpenGL context returns an error.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the OpenGL context fails to create a native shader program.
    /// </exception>
    public ShaderProgram(GL context)
    {
        Context = context;
        ID = Context.CreateProgram();

        var err = (ErrorCode)Context.GetError();

        if (err != ErrorCode.NoError)
        {
            throw new GLException(err, "Failed to create an OpenGL shader program.");
        }

        if (ID == 0)
        {
            throw new InvalidOperationException("Failed to create an OpenGL shader program.");
        }

        attachedShaders = new List<Shader>();
    }

    /// <summary>
    /// The finalizer.
    /// </summary>
    ~ShaderProgram() =>
        Dispose(false);

    #endregion

    #region Public Methods

    /// <summary>
    /// Check whether this instance has a given shader attached to it.
    /// </summary>
    /// <param name="shader">
    /// The shader to check for.
    /// </param>
    /// <returns>
    /// Whether this indtance has the given shader attached to it.
    /// </returns>
    public bool HasAttachedShader(Shader shader)
    {
        if (IsDisposed || !IsValid)
        {
            return false;
        }

        return attachedShaders.Contains(shader);
    }

    /// <summary>
    /// Attach a shader to this instance.
    /// </summary>
    /// <param name="shader">
    /// The shader to attach.
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
    /// <exception cref="InvalidOperationException">
    /// Thrown if the given shader is invalid.
    /// </exception>
    /// <exception cref="GLException">
    /// Thrown if the OpenGL context returns an error.
    /// </exception>
    public ShaderProgram AttachShader(Shader shader)
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(ShaderProgram), "Cannot attach an OpenGL shader to a disposed OpenGL shader program.");
        }

        if (!IsValid)
        {
            throw new InvalidOperationException("Cannot attach an OpenGL shader to an invalid OpenGL shader program.");
        }

        if (HasAttachedShader(shader))
        {
            return this;
        }

        if (!shader.IsValid)
        {
            throw new InvalidOperationException("Cannot attach an invalid OpenGL shader to an OpenGL shader program.");
        }

        Context.AttachShader(ID, shader.ID);
        var err = (ErrorCode)Context.GetError();

        if (err != ErrorCode.NoError)
        {
            throw new GLException(err, "Failed to attach an OpenGL shader to an OpenGL shader program.");
        }

        attachedShaders.Add(shader);

        return this;
    }

    /// <summary>
    /// Detach a shader to this instance.
    /// </summary>
    /// <param name="shader">
    /// The shader to detach.
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
    /// <exception cref="InvalidOperationException">
    /// Thrown if the given shader is invalid.
    /// </exception>
    /// <exception cref="GLException">
    /// Thrown if the OpenGL context returns an error.
    /// </exception>
    public ShaderProgram DetachShader(Shader shader)
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(ShaderProgram), "Cannot detach an OpenGL shader to a disposed OpenGL shader program.");
        }

        if (!IsValid)
        {
            throw new InvalidOperationException("Cannot detach an OpenGL shader to an invalid OpenGL shader program.");
        }

        if (!HasAttachedShader(shader))
        {
            return this;
        }

        if (!shader.IsValid)
        {
            throw new InvalidOperationException("Cannot detach an invalid OpenGL shader to an OpenGL shader program.");
        }

        Context.DetachShader(ID, shader.ID);
        var err = (ErrorCode)Context.GetError();

        if (err != ErrorCode.NoError)
        {
            throw new GLException(err, "Failed to detach an OpenGL shader to an OpenGL shader program.");
        }

        _ = attachedShaders.Remove(shader);

        return this;
    }

    /// <summary>
    /// Link the instance with the attached shaders.
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
    /// <exception cref="GLException">
    /// Thrown if the OpenGL context returns an error.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the OpenGL context fails to link the program.
    /// </exception>
    public ShaderProgram Link()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(ShaderProgram), "Cannot link a disposed OpenGL shader program.");
        }

        if (!IsValid)
        {
            throw new InvalidOperationException("Cannot link an invalid OpenGL shader program.");
        }

        Context.LinkProgram(ID);
        var err = (ErrorCode)Context.GetError();

        if (err != ErrorCode.NoError)
        {
            throw new GLException(err, "Failed to link an OpenGL shader program.");
        }

        if (!IsLinked)
        {
            var infoLog = InfoLog;

            if (string.IsNullOrWhiteSpace(infoLog))
            {
                throw new InvalidOperationException("Failed to link an OpenGL shader program.");
            }
            else
            {
                throw new InvalidOperationException($"Failed to link an OpenGL shader program: {infoLog}");
            }
        }

        return this;
    }

    /// <summary>
    /// Activate this instance for usage.
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
    /// Thrown if this instance is not linked.
    /// </exception>
    /// <exception cref="GLException">
    /// Thrown if the OpenGL context returns an error.
    /// </exception>
    public ShaderProgram Activate()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(ShaderProgram), "Cannot activate a disposed OpenGL shader program.");
        }

        if (!IsValid)
        {
            throw new InvalidOperationException("Cannot activate an invalid OpenGL shader program.");
        }

        if (!IsLinked)
        {
            throw new InvalidOperationException("Cannot activate an unlinked OpenGL shader program.");
        }

        if (IsActive)
        {
            return this;
        }

        Context.UseProgram(ID);
        var err = (ErrorCode)Context.GetError();

        if (err != ErrorCode.NoError)
        {
            throw new GLException(err, "Failed to activate an OpenGL shader program.");
        }

        return this;
    }

    /// <summary>
    /// Deactivate this instance for usage.
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
    /// Thrown if this instance is not linked.
    /// </exception>
    /// <exception cref="GLException">
    /// Thrown if the OpenGL context returns an error.
    /// </exception>
    public ShaderProgram Deactivate()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(ShaderProgram), "Cannot deactivate a disposed OpenGL shader program.");
        }

        if (!IsValid)
        {
            throw new InvalidOperationException("Cannot deactivate an invalid OpenGL shader program.");
        }

        if (!IsLinked)
        {
            throw new InvalidOperationException("Cannot deactivate an unlinked OpenGL shader program.");
        }

        if (!IsActive)
        {
            return this;
        }

        Context.UseProgram(0);
        var err = (ErrorCode)Context.GetError();

        if (err != ErrorCode.NoError)
        {
            throw new GLException(err, "Failed to deactivate an OpenGL shader program.");
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
    /// Whether this method is being called from managed code or from unmanaged code (e.g. the garbage collector).
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
            Context.DeleteProgram(ID);
            attachedShaders.Clear();
        }

        ID = 0;
        IsDisposed = true;
    }

    #endregion
}