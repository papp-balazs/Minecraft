namespace Minecraft.OpenGL;

using Silk.NET.OpenGL;
using System.Runtime.Serialization;

/// <summary>
/// An exception that is thrown when an OpenGL error occurs.
/// </summary>
[Serializable]
public class GLException : Exception
{
    #region Public Properties

    /// <summary>
    /// The OpenGL error code that occured.
    /// </summary>
    public ErrorCode ErrorCode
    {
        get;
    }

    #endregion

    #region Constructors/Finalizer

    /// <summary>
    /// Create a new instance.
    /// </summary>
    /// <param name="errorCode">
    /// The OpenGL error code that occured.
    /// </param>
    public GLException(ErrorCode errorCode) :
        base($"An OpenGL error occured: {errorCode}") =>
        ErrorCode = errorCode;

    /// <summary>
    /// Create a new instance.
    /// </summary>
    /// <param name="errorCode">
    /// The OpenGL error code that occured.
    /// </param>
    /// <param name="message">
    /// A string describing the exception.
    /// </param>
    public GLException(ErrorCode errorCode, string message) :
        base(message) =>
        ErrorCode = errorCode;

    /// <summary>
    /// Create a new instance.
    /// </summary>
    /// <param name="errorCode">
    /// The OpenGL error code that occured.
    /// </param>
    /// <param name="message">
    /// A string describing the exception.
    /// </param>
    /// <param name="inner">
    /// The exception that caused this exception to be thrown.
    /// </param>
    public GLException(ErrorCode errorCode, string message, Exception inner) :
        base(message, inner) =>
        ErrorCode = errorCode;

    /// <summary>
    /// The serialization constructor.
    /// </summary>
    /// <param name="info">
    /// The serialization information.
    /// </param>
    /// <param name="context">
    /// The streaming context.
    /// </param>
    protected GLException(SerializationInfo info, StreamingContext context) :
        base(info, context) =>
        ErrorCode = (ErrorCode)info.GetInt32("ErrorCode");

    #endregion

    #region Public Methods

    /// <summary>
    /// Get the object data for serialization.
    /// </summary>
    /// <param name="info">
    /// The serialization information.
    /// </param>
    /// <param name="context">
    /// The streaming context.
    /// </param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("ErrorCode", (int)ErrorCode);
        base.GetObjectData(info, context);
    }

    #endregion
}