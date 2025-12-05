//path src/PsGraphUtility/Auth/GraphAuthException.cs
using System;

namespace PsGraphUtility.Auth;

public class GraphAuthException : Exception
{
    public GraphAuthException(string message) : base(message) { }
    public GraphAuthException(string message, Exception inner) : base(message, inner) { }
}