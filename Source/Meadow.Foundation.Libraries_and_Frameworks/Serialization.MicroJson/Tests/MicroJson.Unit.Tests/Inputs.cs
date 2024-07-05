using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Unit.Tests;

public static class Inputs
{
    public static string GetInputResource(string name)
    {
        var resName = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceNames()
            .Where(n => n.EndsWith(name))
            .FirstOrDefault();

        if (resName == null)
        {
            throw new Exception("Resource not found");
        }

        return new StreamReader(
            Assembly.GetExecutingAssembly().GetManifestResourceStream(resName))
            .ReadToEnd();
    }
}
