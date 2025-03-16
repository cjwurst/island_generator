using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class ComputeHelper
{
    public static ComputeShader LoadLibrary(string s)
    {
        var library = Resources.Load<ComputeShader>(s);
        Assert.IsNotNull(library, $"{s} not found.");
        return library;
    }
}
