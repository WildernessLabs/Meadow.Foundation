using System;

namespace FTD2XX;

/// <summary>
/// Exceptions thrown by errors within the FTDI class.
/// </summary>
[Serializable]
public class FT_EXCEPTION : Exception
{
    /// <summary>
    /// 
    /// </summary>
    public FT_EXCEPTION() { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public FT_EXCEPTION(string message) : base(message) { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public FT_EXCEPTION(string message, Exception inner) : base(message, inner) { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected FT_EXCEPTION(
    System.Runtime.Serialization.SerializationInfo info,
    System.Runtime.Serialization.StreamingContext context)
        : base(info, context) { }
}