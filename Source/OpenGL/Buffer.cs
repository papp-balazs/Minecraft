namespace Minecraft.OpenGL;

using Silk.NET.OpenGL;
using System.Runtime.InteropServices;

/// <summary>
/// An OpenGL Buffer.
/// </summary>
public class Buffer : IDisposable
{
    #region Public Properties;

    /// <summary>
    /// The OpenGL context that owns the instance.
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
    /// The target this instance binds to.
    /// </summary>
    public BufferTargetARB Target
    {
        get;
    }

    /// <summary>
    /// Whether this instance has been disposed.
    /// </summary>
    /// <seealso cref="Dispose" />
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
    /// Whether this instance is bound to its target.
    /// </summary>
    /// <seealso cref="Bind" />
    /// <seealso cref="Unbind" />
    public bool IsBound
    {
        get
        {
            if (IsDisposed || !IsValid)
            {
                return false;
            }

            var pname = Target switch
            {
                BufferTargetARB.ArrayBuffer => GetPName.ArrayBufferBinding,
                BufferTargetARB.ElementArrayBuffer => GetPName.ElementArrayBufferBinding,
                _ => throw new NotSupportedException("Unsupported buffer target.")
            };

            return Context.GetInteger(pname) == ID;
        }
    }

    /// <summary>
    /// The amount of VRAM allocated to this instance, in bytes.
    /// </summary>
    /// <seealso cref="Allocate" />
    /// <seealso cref="UploadData" />
    public nuint? SizeBytes
    {
        get;
        private set;
    }

    /// <summary>
    /// The usage hint this instance was given when it was allocated.
    /// </summary>
    /// <seealso cref="Allocate" />
    /// <seealso cref="UploadData" />
    public BufferUsageARB UsageHint
    {
        get;
        private set;
    }

    #endregion

    #region Constructors/Finalizer

    /// <summary>
    /// Create a new instace.
    /// </summary>
    /// <param name="context">
    /// The OpenGL context that will own this instance.
    /// </param>
    /// <param name="target">
    /// The target this instance will bind to.
    /// </param>
    /// <exception cref="GLException">
    /// Thrown if the OpenGL context returns an error.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the OpenGL context fails to create a native buffer.
    /// </exception>
    public Buffer(GL context, BufferTargetARB target)
    {
        Context = context;
        Target = target;
        ID = Context.GenBuffer();

        var err = (ErrorCode)Context.GetError();

        if (err != ErrorCode.NoError)
        {
            throw new GLException(err, "Failed to create an OpenGL buffer.");
        }

        if (ID == 0)
        {
            throw new InvalidOperationException("Failed to create an OpenGL buffer.");
        }
    }

    /// <summary>
    /// The finalizer.
    /// </summary>
    ~Buffer() =>
        Dispose(false);

    #endregion

    #region Public Methods

    /// <summary>
    /// Bind this instance to its target.
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
    /// <seealso cref="IsBound" />
    public Buffer Bind()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(Buffer), "Cannot bind a disposed OpenGL buffer.");
        }

        if (!IsValid)
        {
            throw new InvalidOperationException("Cannot bind an invalid OpenGL buffer.");
        }

        if (IsBound)
        {
            return this;
        }

        Context.BindBuffer(Target, ID);
        var err = (ErrorCode)Context.GetError();

        if (err != ErrorCode.NoError)
        {
            throw new GLException(err, "Failed to bind an OpenGL buffer.");
        }

        return this;
    }

    /// <summary>
    /// Unbind this instance form its target.
    /// </summary>
    /// <returns>
    /// The instance for chaining.
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
    /// <seealso cref="IsBound" />
    public Buffer Unbind()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(Buffer), "Cannot unbind a disposed OpenGL buffer.");
        }

        if (!IsValid)
        {
            throw new InvalidOperationException("Cannot unbind an invalid OpenGL buffer.");
        }

        if (IsBound)
        {
            return this;
        }

        Context.BindBuffer(Target, 0);
        var err = (ErrorCode)Context.GetError();

        if (err != ErrorCode.NoError)
        {
            throw new GLException(err, "Failed to unbind an OpenGL buffer.");
        }

        return this;
    }

    /// <summary>
    /// Allocate space in VRAM for this instance.
    /// </summary>
    /// <param name="sizeBytes">
    /// The amount of VRAM to allocate, in bytes, for this instance.
    /// </param>
    /// <param name="usage">
    /// The usage hint to apply to this instance.
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
    /// Thrown if this instance is not bound to its target.
    /// </exception>
    /// <exception cref="GLException">
    /// Thrown if the OpenGL context returns an error.
    /// </exception>
    /// <seealso cref="SizeBytes" />
    /// <seealso cref="UsageHint" />
    public Buffer Allocate(nuint sizeBytes, BufferUsageARB usage)
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(Buffer), "Cannot allocate space for a disposed OpenGL buffer.");
        }

        if (!IsValid)
        {
            throw new InvalidOperationException("Cannot allocate space for an invalid OpenGL buffer.");
        }

        if (!IsBound)
        {
            throw new InvalidOperationException("Cannot allocate space for an unbound OpenGL buffer.");
        }

        Context.BufferData<byte>(Target, sizeBytes, Array.Empty<byte>(), usage);
        var err = (ErrorCode)Context.GetError();

        if (err != ErrorCode.NoError)
        {
            throw new GLException(err, "Failed to allocate space for an OpenGL buffer.");
        }

        SizeBytes = sizeBytes;
        UsageHint = usage;

        return this;
    }

    /// <summary>
    /// Upload data into this instance.
    /// </summary>
    /// <remarks>
    /// If space in VRAM has not been allocated for this instance it will be automatically allocated. If space has been allocated, but is not the same size as the data being uploaded it will be reallocated.
    /// </remarks>
    /// <typeparam name="T0">
    /// Tge type of the data to upload.
    /// </typeparam>
    /// <param name="data">
    /// The data to upload.
    /// </param>
    /// <param name="usage">
    /// The usage hint to apply to this instance.
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
    /// Thrown if this instance is not bound to its target.
    /// </exception>
    /// <exception cref="GLException">
    /// Thrown if the OpenGL context returns an error.
    /// </exception>
    /// <seealso cref="SizeBytes" />
    /// <seealso cref="UsageHint" />
    public Buffer UploadData<T0>(in T0[] data, BufferUsageARB usage) where T0 : unmanaged
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(Buffer), "Cannot upload data to a disposed OpenGL buffer.");
        }

        if (!IsValid)
        {
            throw new InvalidOperationException("Cannot upload data to an invalid OpenGL buffer.");
        }

        if (!IsBound)
        {
            throw new InvalidOperationException("Cannot upload data to an unbound OpenGL buffer.");
        }

        Context.BufferData<T0>(Target, (nuint)(data.Length * Marshal.SizeOf<T0>()), data, usage);
        var err = (ErrorCode)Context.GetError();

        if (err != ErrorCode.NoError)
        {
            throw new GLException(err, "Failed to upload data to an OpenGL buffer.");
        }

        SizeBytes = (nuint)(data.Length * Marshal.SizeOf<T0>());
        UsageHint = usage;

        return this;
    }

    /// <summary>
    /// Upload data into this instance.
    /// </summary>
    /// <remarks>
    /// If space in VRAM has not been allocated for this instance it will be automatically allocated. If space has been allocated, but is not the same size as the data being uploaded it will be reallocated.
    /// </remarks>
    /// <typeparam name="T0">
    /// Tge type of the data to upload.
    /// </typeparam>
    /// <param name="data">
    /// The data to upload.
    /// </param>
    /// <param name="usage">
    /// The usage hint to apply to this instance.
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
    /// Thrown if this instance is not bound to its target.
    /// </exception>
    /// <exception cref="GLException">
    /// Thrown if the OpenGL context returns an error.
    /// </exception>
    /// <seealso cref="SizeBytes" />
    /// <seealso cref="UsageHint" />
    public Buffer UploadData<T0>(ReadOnlySpan<T0> data, BufferUsageARB usage) where T0 : unmanaged
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(Buffer), "Cannot upload data to a disposed OpenGL buffer.");
        }

        if (!IsValid)
        {
            throw new InvalidOperationException("Cannot upload data to an invalid OpenGL buffer.");
        }

        if (!IsBound)
        {
            throw new InvalidOperationException("Cannot upload data to an unbound OpenGL buffer.");
        }

        Context.BufferData(Target, data, usage);
        var err = (ErrorCode)Context.GetError();

        if (err != ErrorCode.NoError)
        {
            throw new GLException(err, "Failed to upload data to an OpenGL buffer.");
        }

        SizeBytes = (nuint)(data.Length * Marshal.SizeOf<T0>());
        UsageHint = usage;

        return this;
    }

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
    /// </param>
    public void Dispose(bool managed)
    {
        if (IsDisposed)
        {
            return;
        }

        if (managed && IsValid)
        {
            Context.DeleteBuffer(ID);
        }

        ID = 0;
        SizeBytes = 0;
        UsageHint = 0;
        IsDisposed = true;
    }

    #endregion
}