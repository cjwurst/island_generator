using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class Helper
{
    public static int Bound(int n, int min, int max)
    {
        n = Mathf.Max(n, min);
        n = Mathf.Min(n, max);
        return n;
    }

    public static IInvertible Compose(List<IInvertible> commands) { return new InvertibleComposition(commands); }
    public static IInvertible Compose(params IInvertible[] commands) { return Compose(new List<IInvertible>(commands)); }

    public static bool FlagsIncludeAny(byte flags, byte test) { return (flags & test) != 0b_0000_0000; }
    public static bool FlagsIncludeAll(byte flags, byte test) { return (flags | test) == test; }

    class InvertibleComposition : IInvertible
    {
        List<IInvertible> commands;

        public InvertibleComposition(List<IInvertible> _commands)
        {
            commands = _commands;
        }

        public void Do()
        {
            for (int i = 0; i < commands.Count; i++)
                commands[i].Do();
        }

        public void Undo()
        {
            for (int i = commands.Count - 1; i >= 0; i--)
                commands[i].Undo();
        }
    }
}

public static class TextureHelper
{
    public static float[] OneChannelToArray(Texture2D oneChannelTexture)
    {
        Assert.AreEqual(TextureFormat.RFloat, oneChannelTexture.format);
        var size = new Vector2Int(oneChannelTexture.width, oneChannelTexture.height);
        var pixelArray = new float[size.x * size.y];
        for (var i = 0; i < size.x; i++)
            for (var j = 0; j < size.y; j++)
                pixelArray[i + j * size.x] = oneChannelTexture.GetPixel(i, j).r;
        return pixelArray;
    }

    public static Texture2D ArrayToOneChannel(float[] pixelArray, int width)
    {
        Assert.AreEqual(0, pixelArray.Length % width);
        var oneChannelTexture = new Texture2D(width, pixelArray.Length / width, TextureFormat.RFloat, false);
        oneChannelTexture.SetPixelData(pixelArray, 0);
        return oneChannelTexture;
    }

    public static Dictionary<Vector2Int, T> GetNearestNeighbors<T>(Texture2D texture, List<T> candidates, Func<T, Vector2> getPosition, out Dictionary<Vector2Int, float> distances)
        => GetNearestNeighbors<T>(new Vector2Int(texture.width, texture.height), candidates, getPosition, out distances);
    public static Dictionary<Vector2Int, T> GetNearestNeighbors<T>(Vector2Int dimensions, List<T> candidates, Func<T, Vector2> getPosition, out Dictionary<Vector2Int, float> distances)
    {
        var candidatePositionsData = new List<Vector2>();
        foreach (var candidate in candidates)
            candidatePositionsData.Add(getPosition.Invoke(candidate));
        var pixelCount = dimensions.x * dimensions.y;
        var resultData = new uint[pixelCount];
        for (var i = 0; i < pixelCount; i++)
            resultData[i] = 0u;

        var seedPositionsID = Shader.PropertyToID("candidatePositions");
        var resultID = Shader.PropertyToID("result");
        var distancesID = Shader.PropertyToID("distances");
        var resolutionWidthID = Shader.PropertyToID("resolutionWidth");

        var nnsLibrary = ComputeHelper.LoadLibrary("NNSLibrary");
        var kernelIndex = nnsLibrary.FindKernel("LinearNearestNeighborSearch");
        var candidatesBuffer = new ComputeBuffer(candidates.Count, 8);
        candidatesBuffer.SetData(candidatePositionsData);
        var resultBuffer = new ComputeBuffer(pixelCount, 4);
        resultBuffer.SetData(resultData);
        var distancesBuffer = new ComputeBuffer(pixelCount, 4);
        distancesBuffer.SetData(resultData);
        nnsLibrary.SetBuffer(kernelIndex, seedPositionsID, candidatesBuffer);
        nnsLibrary.SetBuffer(kernelIndex, resultID, resultBuffer);
        nnsLibrary.SetBuffer(kernelIndex, distancesID, distancesBuffer);
        nnsLibrary.SetInt(resolutionWidthID, dimensions.x);

        nnsLibrary.GetKernelThreadGroupSizes(kernelIndex, out var groupX, out var groupY, out _);
        var groupCountX = dimensions.x / (int)groupX;
        var groupCountY = dimensions.y / (int)groupY;
        nnsLibrary.Dispatch(kernelIndex, groupCountX, groupCountY, 1);
        var neighborIndices = new uint[pixelCount];
        resultBuffer.GetData(neighborIndices);
        var distanceData = new float[pixelCount];
        distancesBuffer.GetData(distanceData);

        candidatesBuffer.Release();
        resultBuffer.Release();
        distancesBuffer.Release();

        var nearestNeighbors = new Dictionary<Vector2Int, T>();
        distances = new Dictionary<Vector2Int, float>();
        for (var i = 0; i < dimensions.x; i++)
        {
            for (var j = 0; j < dimensions.y; j++)
            {
                var pixel = new Vector2Int(i, j);
                var pixelIndex = i + j * dimensions.x;
                nearestNeighbors.Add(pixel, candidates[(int)neighborIndices[pixelIndex]]);
                distances.Add(pixel, distanceData[pixelIndex]);
            }
        }
        return nearestNeighbors;
    }
}
