using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldMap
{
    Dictionary<Vector2Int, MapPixel> indexedPixels;

    public Biome[] GetBiomeAt (params Vector2Int[] pixels)
    {
        var biomes = new Biome[pixels.Length];
        for(var i = 0; i < pixels.Length; i++)
            biomes[i] = indexedPixels[pixels[i]].Biome;
        return biomes;
    }
    public Biome GetBiomeAt(Vector2Int pixel) => GetBiomeAt(new Vector2Int[] { pixel })[0];

    struct MapPixel
    {
        public Biome Biome { get; }
    }
}
