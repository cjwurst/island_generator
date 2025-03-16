using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public static class NoiseUtility
{
    const float originRadius = 10000f;

    public static Texture2D MakePerlinTexture
    (
        Vector2Int textureSize, 
        Vector2 sampleSize, 
        int octaveCount = 4, 
        float octaveWeightCoefficient = 0.5f, 
        float octaveFrequencyCoefficient = 2f
    )
    {
        // calculate a maximum output for normalization
        var maxPixelValue = 0.5f;
        for (var i = 0; i < octaveCount; i++)
            maxPixelValue += Mathf.Pow(octaveWeightCoefficient, i);

        var origin = new Vector2(Random.Range(-originRadius, originRadius), Random.Range(-originRadius, originRadius));
        var texture = new Texture2D(textureSize.x, textureSize.y, TextureFormat.RFloat, false);
        var xStep = sampleSize.x / textureSize.x;
        var yStep = sampleSize.y / textureSize.y;
        var pixelData = new float[textureSize.x * textureSize.y];
        for (var i = 0; i < textureSize.x; i ++)
        {
            for (var j = 0; j < textureSize.y; j ++)
            {
                var pixelValue = 0.5f;
                for (var k = 0; k < octaveCount; k++)
                {
                    var x = (origin.x + i * xStep) / Mathf.Pow(octaveFrequencyCoefficient, k);
                    var y = (origin.y + j * yStep) / Mathf.Pow(octaveFrequencyCoefficient, k);
                    pixelValue += 2f * Mathf.Pow(octaveWeightCoefficient, k) * (Mathf.PerlinNoise(x, y) - 0.5f);
                }
                pixelValue = (pixelValue - 0.5f) / maxPixelValue + 0.5f;        // normalize *pixelValue*
                pixelData[i + j * textureSize.x] = pixelValue;
                //if (!(pixelValue <= 1.1f && pixelValue >= -0.1f)) MonoBehaviour.print(pixelValue);
            }
        }

        texture.SetPixelData(pixelData, 0);
        texture.Apply();
        return texture;
    }
}
