using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using LSystemEngine;

public class OverworldEnvironment : MonoBehaviour
{
    ComputeShader imageProcessingLibrary;

    [SerializeField] Texture2DHolder textureHolder;

    [Space, Space]
    [SerializeField] Texture2D seedTexture;
    [SerializeField] int deadPixelCount = 0;
    const int SIZE = 256;

    [Space, Space]
    [SerializeField] Texture2D heatSampleTexture;
    [SerializeField, Range(0f, 1f)] float heatSamplePortion = 0.5f;
    [SerializeField] AnimationCurve heatHeightCurve;

    [Space, Space]
    [SerializeField] LSystem faultLSystem;

    [Space]
    [SerializeField, Range(0f, 0.1f)] float minimumIterationTime = 0.01f;           // per column

    [Space]
    [SerializeField, Range(0f, 1f)] float inlandFaultPortion = 0.2f;
    [SerializeField] int minPixelCount = 100;
    [SerializeField] int maxTries = 50;

    [Space, Space]
    [SerializeField, Range(2f, 15f)] float startingDensity = 3f;                    // per 100 sq pixels
    [SerializeField, Range(1f, 50f)] float densityCoefficient = 2f;                 // ""
    [SerializeField, Range(100f, 1000f)] float smoothingDensity = 300f;             // ""
    [SerializeField] int iterationCount = 1;

    [Space, Space]
    [SerializeField, Range(0f, 1f)] float cellSaturation = 0.5f;
    [SerializeField, Range(0f, 1f)] float cellValue = 0.5f;

    [Space, Space]
    [SerializeField] float sampleSize = 5f;
    [SerializeField] int octaveCount = 5;
    [SerializeField] float octaveWeightCoefficient = 0.5f;
    [SerializeField] float octaveFrequencyCoefficient = 2f;

    [Space, Space]
    [SerializeField, Range(0f, 1f)] float heightNoiseCoefficient = 0.5f;
    [SerializeField, Range(0f, 1f)] float edgeDistanceCoefficient = 0.5f;
    [SerializeField, Range(0f, 1f)] float faultDistanceCoefficient = 0.5f;
    [SerializeField, Range(0f, 1f)] float submergentCoastPortion = 0.5f;

    [Space]
    [SerializeField] AnimationCurve edgeHeightCurve;                        // distance from edge vs elevation
    [SerializeField] AnimationCurve edgeFaultCurve;                         // distance from edge vs fault weight
    [SerializeField] AnimationCurve edgeNoiseCurve;                         // distance from edge vs noise weight  

    [Space]
    [SerializeField] Color isoLineColor = Color.grey;
    [SerializeField] int isoLineCount = 10;
    [SerializeField] float isoLineThickness = 0.1f;

    [Space, Space]
    [SerializeField, Range(0f, 3f)] float maxBaseWindSpeed = 1f;

    [Space, Space]
    [SerializeField, Range(0f, 1f)] float humidityNoiseCoefficient = 0f;
    [SerializeField, Range(0f, 1f)] float heatNoiseCoefficient = 0f;
    [SerializeField, Range(0f, 2f)] float humidityPerlinCoefficient = 0f;
    [SerializeField, Range(0f, 2f)] float heatPerlinCoefficient = 0f;

    [Space]
    [SerializeField] Color mountainColor;
    [SerializeField] Color oceanColor;

    [Space]
    [SerializeField] Color tundraColor;
    [SerializeField] Color borealForestColor;
    [SerializeField] Color coldDesertColor;
    [SerializeField] Color subtropicalDesertColor;
    [SerializeField] Color seasonalForestColor;
    [SerializeField] Color savannaColor;
    [SerializeField] Color tropicalRainforestColor;
    Dictionary<Biome, Color> biomeColors = new Dictionary<Biome, Color>();

    readonly static Vector2Int[] adjacentPositions = new Vector2Int[]
    {
        new Vector2Int(-1, 1),  new Vector2Int(0, 1),  new Vector2Int(1, 1),
        new Vector2Int(-1, 0),                         new Vector2Int(1, 0),
        new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1)
    };
    readonly static Vector2Int[] adjacentPositionsInclusive = new Vector2Int[]
    {
        new Vector2Int(-1, 1),  new Vector2Int(0, 1),  new Vector2Int(1, 1),
        new Vector2Int(-1, 0),  new Vector2Int(0, 0),  new Vector2Int(1, 0),
        new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1)
    };

    readonly static float[] sobelX = new float[]
    {
        -1f, 0f, 1f,
        -2f, 0f, 2f,
        -1f, 0f, 1f
    };
    readonly static float[] sobelY = new float[]
    {
        -1f, -2f, -1f,
         0f,  0f,  0f,
         1f,  2f,  1f
    };

    void Start()
    {
        biomeColors.Add(Biome.Tundra, tundraColor);
        biomeColors.Add(Biome.BorealForest, borealForestColor);
        biomeColors.Add(Biome.ColdDesert, coldDesertColor);
        biomeColors.Add(Biome.SubtropicalDesert, subtropicalDesertColor);
        biomeColors.Add(Biome.SeasonalForest, seasonalForestColor);
        biomeColors.Add(Biome.Savanna, savannaColor);
        biomeColors.Add(Biome.TropicalRainforest, tropicalRainforestColor);

        imageProcessingLibrary = ComputeHelper.LoadLibrary("ImageProcessingLibrary");
    }

    void Update()
    {
        var hasTapped = false;
        foreach (var touch in Input.touches)
            if (touch.phase == TouchPhase.Began)
                hasTapped = true;

        if (Input.GetKeyDown(KeyCode.Space) || hasTapped)
        {
            StopAllCoroutines();
            StartCoroutine(Init());
        }
    }

    IEnumerator Init()
    {
        var landTexture = MakeFirstTexture();
        MakeLandTexture(landTexture);
        Apply(landTexture);

        yield return null;
        var faultTexture = new Texture2D(landTexture.width, landTexture.height, landTexture.format, false);
        MakeFaultTexture(faultTexture, landTexture, faultLSystem);
        Apply(faultTexture);

        var edgePixels = GetEdges(landTexture);

        yield return null;
        var noiseTexture = NoiseUtility.MakePerlinTexture(new Vector2Int(SIZE, SIZE), new Vector2(sampleSize, sampleSize), octaveCount, octaveWeightCoefficient, octaveFrequencyCoefficient);
        Apply(OneChannelToRainbow(noiseTexture), edgePixels);

        yield return null;
        var heightMap = new Texture2D(SIZE, SIZE, TextureFormat.RFloat, false);
        MakeHeightMap(landTexture, faultTexture, noiseTexture, heightMap);
        var heightGradient = OneChannelToGradient(heightMap);
        Apply(OneChannelToRainbow(heightMap), edgePixels);

        yield return null;
        var seaLevel = GetSeaLevel(heightMap, edgePixels, submergentCoastPortion);
        var marineTexture = new Texture2D(landTexture.width, landTexture.height, landTexture.format, false);
        MakeMarineTexture(landTexture, heightMap, seaLevel, edgePixels, marineTexture);
        Apply(marineTexture, edgePixels, marineTexture);

        yield return null;
        var heatTexture = new Texture2D(landTexture.width, landTexture.height, TextureFormat.RFloat, false);
        MakeHeatTexture(marineTexture, heightMap, seaLevel, heatTexture);
        Texture2D displayTexture = OneChannelToRainbow(heatTexture, new Vector2(0f, 0.75f));
        DrawIsoLines(displayTexture, heightMap, heightGradient);
        Apply(displayTexture, edgePixels);

        yield return null;
        var humidityTexture = new Texture2D(landTexture.width, landTexture.height, TextureFormat.RFloat, false);
        MakeHumidityTexture(marineTexture, heatTexture, humidityTexture);
        Apply(humidityTexture, edgePixels);

        yield return null;
        var heatNoiseTexture = NoiseUtility.MakePerlinTexture(new Vector2Int(SIZE, SIZE), new Vector2(sampleSize, sampleSize), octaveCount, octaveWeightCoefficient, octaveFrequencyCoefficient);
        var humidityNoiseTexture = NoiseUtility.MakePerlinTexture(new Vector2Int(SIZE, SIZE), new Vector2(sampleSize, sampleSize), octaveCount, octaveWeightCoefficient, octaveFrequencyCoefficient);
        var biomeTexture = new Texture2D(landTexture.width, landTexture.height, TextureFormat.RGB24, false);
        MakeBiomeTexture(marineTexture, heightMap, humidityTexture, heatTexture, heatNoiseTexture, humidityNoiseTexture, biomeTexture);
        Apply(biomeTexture);

        print("Done!");

        GetComponent<MapController>().Init(edgePixels, landTexture, noiseTexture, heightMap, marineTexture, heatTexture, humidityTexture, biomeTexture);
        yield break;
    }

    Texture2D MakeFirstTexture()
    {
        var texture = new Texture2D(SIZE, SIZE, TextureFormat.RGB24, false);
        texture.SetPixelData(seedTexture.GetRawTextureData(), 0);
        for (var i = 0; i < deadPixelCount; i++)
            texture.SetPixel(Random.Range(0, SIZE - 1), UnityEngine.Random.Range(0, SIZE - 1), Color.black);
        return texture;
    }

    void MakeLandTexture(Texture2D texture)
    {
        float _density = startingDensity;
        for (var i = 0; i < iterationCount; i++)
        {
            Iterate(_density);
            _density *= densityCoefficient;
        }

        if (smoothingDensity > _density / densityCoefficient)
            Iterate(smoothingDensity);

        for (var i = 0; i < SIZE; i++)
            for (var j = 0; j < SIZE; j++)
                if (texture.GetPixel(i, j) != Color.black)
                    texture.SetPixel(i, j, Color.white);

        void Iterate(float density)
        {
            // Calculate *seedCount* from *density*, and set *validPixels*.
            var validPixels = new HashSet<Vector2Int>();
            for (var i = 0; i < SIZE; i++)
                for (var j = 0; j < SIZE; j++)
                    if (texture.GetPixel(i, j) != Color.black)
                        validPixels.Add(new Vector2Int(i, j));
            var seedCount = Mathf.RoundToInt(density * validPixels.Count / 10000f);

            // generate seeds
            var seeds = new List<Seed>();
            for (var i = 0; i < seedCount; i++)
            {
                Seed seed = null;
                while (true)
                {
                    if (Seed.TryMakeRandom(SIZE, cellSaturation, cellValue, out var _seed, (s) => validPixels.Contains(s.position)))
                    {
                        seed = _seed;
                        break;
                    }
                }
                seeds.Add(seed);
            }

            var nearestSeeds = TextureHelper.GetNearestNeighbors(texture, seeds, (s) => s.position, out var _);
            var pixelMap = new Dictionary<Vector2Int, Seed>();
            var seedMap = new Dictionary<Seed, HashSet<Vector2Int>>();
            for (var i = 0; i < SIZE; i++)
            {
                for (var j = 0; j < SIZE; j++)
                {
                    var nearestSeed = nearestSeeds[new Vector2Int(i, j)];
                    texture.SetPixel(i, j, nearestSeed.color);

                    var pixel = new Vector2Int(i, j);
                    pixelMap.Add(pixel, nearestSeed);
                    if (!seedMap.TryGetValue(nearestSeed, out var pixelSet))
                    {
                        pixelSet = new HashSet<Vector2Int>();
                        seedMap.Add(nearestSeed, pixelSet);
                    }
                    pixelSet.Add(pixel);
                }
            }

            foreach (var seed in seeds)
                if (!ValidateCell(seed, seedMap, validPixels))
                    ClearCell(seed, seedMap);
        }

        bool ValidateCell(Seed seed, Dictionary<Seed, HashSet<Vector2Int>> seedMap, HashSet<Vector2Int> validPixels)
        {
            if (!seedMap.TryGetValue(seed, out var pixels)) return false;
            foreach (var pixel in pixels)
                if 
                (
                    !validPixels.Contains(pixel) 
                    || pixel.x == 0 
                    || pixel.y == 0 
                    || pixel.x == SIZE - 1 
                    || pixel.y == SIZE - 1
                )
                    return false;
            return true;
        }

        void ClearCell(Seed seed, Dictionary<Seed, HashSet<Vector2Int>> seedMap)
        {
            if (!seedMap.TryGetValue(seed, out var pixels)) return;
            foreach(var pixel in pixels)
                texture.SetPixel(pixel.x, pixel.y, Color.black);
        }
    }

    void MakeHeightMap(Texture2D landTexture, Texture2D faultTexture, Texture2D noiseTexture, Texture2D heightMap)
    {
        var edgePixels = new List<Vector2Int>(GetEdges(landTexture).ToArray());
        var faultPixels = new List<Vector2Int>(GetPixelsByColor(faultTexture, Color.black).ToArray());
        var edgeDistances = new float[SIZE * SIZE];                     // signed distances from the edge (negative if off shore)
        var faultDistances = new float[SIZE * SIZE];
        var noises = new float[SIZE * SIZE];
        var edgeBounds = new Vector2(float.PositiveInfinity, float.NegativeInfinity);
        var faultBounds = new Vector2(float.PositiveInfinity, float.NegativeInfinity);
        var noiseBounds = new Vector2(float.PositiveInfinity, float.NegativeInfinity);
        TextureHelper.GetNearestNeighbors(landTexture, edgePixels, (p) => p, out var rawEdgeDistances);
        TextureHelper.GetNearestNeighbors(landTexture, faultPixels, (p) => p, out var rawFaultDistances);
        for (var i = 0; i < SIZE; i++)
        {
            for (var j = 0; j < SIZE; j++)
            {
                var pixel = new Vector2Int(i, j);
                var pixelIndex = i + j * SIZE;
                var distanceFromEdge = rawEdgeDistances[pixel];
                var distanceFromFault = rawFaultDistances[pixel];
                var noise = noiseTexture.GetPixel(i, j).r;
                if (landTexture.GetPixel(i, j) == Color.black) distanceFromEdge *= -1f;
                edgeDistances[pixelIndex] = distanceFromEdge;
                faultDistances[pixelIndex] = -distanceFromFault;
                noises[pixelIndex] = noise;

                if (distanceFromEdge < edgeBounds.x) edgeBounds = new Vector2(distanceFromEdge, edgeBounds.y);
                if (distanceFromEdge > edgeBounds.y) edgeBounds = new Vector2(edgeBounds.x, distanceFromEdge);
                if (distanceFromFault < faultBounds.x) faultBounds = new Vector2(distanceFromFault, faultBounds.y);
                if (distanceFromFault > faultBounds.y) faultBounds = new Vector2(faultBounds.x, distanceFromFault);
                if (noise < noiseBounds.x) noiseBounds = new Vector2(noise, noiseBounds.y);
                if (noise > noiseBounds.y) noiseBounds = new Vector2(noiseBounds.x, noise);
            }
        }
        
        /*const int CURVE_SAMPLE_COUNT = 1000;
        var pixelCount = SIZE * SIZE;

        var edgeDistancesID = Shader.PropertyToID("edgeDistances");
        var edgeHeightCurveID = Shader.PropertyToID("edgeHeightCurve");
        var faultDistancesID = Shader.PropertyToID("faultDistances");
        var edgeFaultCurveID = Shader.PropertyToID("edgeFaultCurve");
        var noiseID = Shader.PropertyToID("noise");
        var edgeNoiseCurveID = Shader.PropertyToID("edgeNoiseCurve");
        var heightsID = Shader.PropertyToID("heights");

        var edgeBoundsID = Shader.PropertyToID("edgeBounds");
        var faultBoundsID = Shader.PropertyToID("faultBounds");
        var noiseBoundsID = Shader.PropertyToID("noiseBounds");
        var edgeDistanceCoefficientID = Shader.PropertyToID("edgeDistanceCoefficient");
        var faultDistanceCoefficientID = Shader.PropertyToID("faultDistanceCoefficient");
        var noiseDistanceCoefficientID = Shader.PropertyToID("noiseDistanceCoefficient");
        var resolutionWidthID = Shader.PropertyToID("resolutionWidth");

        var heightMapLibrary = ComputeHelper.LoadLibrary("HeightMapLibrary");
        var kernelIndex = heightMapLibrary.FindKernel("CombineCurves");
        var edgeDistancesBuffer = new ComputeBuffer(pixelCount, 4);
        edgeDistancesBuffer.SetData(edgeDistances);
        var edgeHeightCurveBuffer = new ComputeBuffer(CURVE_SAMPLE_COUNT, 4);
        edgeHeightCurveBuffer.SetData(edgeHeightCurve.Sample(CURVE_SAMPLE_COUNT));
        var faultDistancesBuffer = new ComputeBuffer(pixelCount, 4);
        faultDistancesBuffer.SetData(faultDistances);
        var edgeFaultCurveBuffer = new ComputeBuffer(CURVE_SAMPLE_COUNT, 4);
        edgeFaultCurveBuffer.SetData(edgeFaultCurve.Sample(CURVE_SAMPLE_COUNT));
        var noiseBuffer = new ComputeBuffer(pixelCount, 4);
        noiseBuffer.SetData(noises);
        var edgeNoiseCurveBuffer = new ComputeBuffer(CURVE_SAMPLE_COUNT, 4);
        edgeNoiseCurveBuffer.SetData(edgeNoiseCurve.Sample(CURVE_SAMPLE_COUNT));
        var heightsBuffer = new ComputeBuffer(pixelCount, 4);
        heightsBuffer.SetData(new float[pixelCount].Fill(0f));
        heightMapLibrary.SetBuffer(kernelIndex, edgeDistancesID, edgeDistancesBuffer);
        heightMapLibrary.SetBuffer(kernelIndex, edgeHeightCurveID, edgeHeightCurveBuffer);
        heightMapLibrary.SetBuffer(kernelIndex, faultDistancesID, faultDistancesBuffer);
        heightMapLibrary.SetBuffer(kernelIndex, edgeFaultCurveID, edgeFaultCurveBuffer);
        heightMapLibrary.SetBuffer(kernelIndex, noiseID, noiseBuffer);
        heightMapLibrary.SetBuffer(kernelIndex, edgeNoiseCurveID, edgeNoiseCurveBuffer);
        heightMapLibrary.SetBuffer(kernelIndex, heightsID, heightsBuffer);
        heightMapLibrary.SetFloats(edgeBoundsID, edgeBounds.x, edgeBounds.y);
        heightMapLibrary.SetFloats(faultBoundsID, faultBounds.x, faultBounds.y);
        heightMapLibrary.SetFloats(noiseBoundsID, noiseBounds.x, noiseBounds.y);
        heightMapLibrary.SetFloat(edgeDistanceCoefficientID, edgeDistanceCoefficient);
        heightMapLibrary.SetFloat(faultDistanceCoefficientID, faultDistanceCoefficient);
        heightMapLibrary.SetFloat(noiseDistanceCoefficientID, heightNoiseCoefficient);
        heightMapLibrary.SetInt(resolutionWidthID, SIZE);

        heightMapLibrary.GetKernelThreadGroupSizes(kernelIndex, out var groupX, out var groupY, out _);
        var groupCountX = SIZE / (int)groupX;
        var groupCountY = SIZE / (int)groupY;
        heightMapLibrary.Dispatch(kernelIndex, groupCountX, groupCountY, 1);
        var heights = new float[pixelCount];
        heightsBuffer.GetData(heights);

        edgeDistancesBuffer.Release();
        edgeHeightCurveBuffer.Release();
        faultDistancesBuffer.Release();
        edgeFaultCurveBuffer.Release();
        noiseBuffer.Release();
        edgeNoiseCurveBuffer.Release();
        heightsBuffer.Release();*/

        var pixelData = new float[edgeDistances.Length];
        var totalBounds = new Vector2(float.PositiveInfinity, float.NegativeInfinity);
        for (var i = 0; i < SIZE; i++)
        {
            for (var j = 0; j < SIZE; j++)
            {
                var index = i + j * SIZE;
                var targetBounds = new Vector2(-1f, 1f);
                var adjustedEdgeDistance = 0f;
                if (edgeDistances[index] > 0f)
                    adjustedEdgeDistance = Normalize(edgeDistances[index], new Vector2(0f, edgeBounds.y), new Vector2(0.5f, 1f));
                else
                    adjustedEdgeDistance = Normalize(edgeDistances[index], new Vector2(edgeBounds.x, 0f), new Vector2(0f, 0.5f));
                var adjustedFaultDistance = edgeFaultCurve.Evaluate(adjustedEdgeDistance) * Normalize(faultDistances[index], faultBounds, targetBounds);
                var adjustedNoise = edgeNoiseCurve.Evaluate(adjustedEdgeDistance) * Normalize(noiseTexture.GetPixel(i, j).r, noiseBounds, targetBounds);
                adjustedEdgeDistance = edgeHeightCurve.Evaluate(adjustedEdgeDistance);
                adjustedFaultDistance = Normalize(adjustedFaultDistance, targetBounds);
                adjustedNoise = Normalize(adjustedNoise, targetBounds);
                var pixelDatum = edgeDistanceCoefficient * adjustedEdgeDistance
                    + faultDistanceCoefficient * adjustedFaultDistance
                    + heightNoiseCoefficient * adjustedNoise;
                if (pixelDatum < totalBounds.x) totalBounds = new Vector2(pixelDatum, totalBounds.y);
                else if (pixelDatum > totalBounds.y) totalBounds = new Vector2(totalBounds.x, pixelDatum);
                pixelData[i + j * SIZE] = pixelDatum;
            }
        }
        /*var totalBounds = new Vector2(float.PositiveInfinity, float.NegativeInfinity);
        foreach(var height in heights)
        {
            if (height < totalBounds.x) totalBounds = new Vector2(height, totalBounds.y);
            if (height > totalBounds.y) totalBounds = new Vector2(totalBounds.x, height);
        }
        for (var i = 0; i < pixelCount; i++)
            heights[i] = Normalize(heights[i], totalBounds);*/

        heightMap.SetPixelData(pixelData, 0);
    }

    void MakeFaultTexture(Texture2D faultTexture, Texture2D landTexture, LSystem faultLSystem)
    {
        for (var tryCount = 0; tryCount < maxTries && !ValidateFault(); tryCount++)
        {
            for (var i = 0; i < faultTexture.width; i++)
                for (var j = 0; j < faultTexture.height; j++)
                    faultTexture.SetPixel(i, j, Color.white);

            var stringToDraw = faultLSystem.axiom;
            var origin = new Vector2Int(Random.Range(0, faultTexture.width), Random.Range(0, faultTexture.height));
            var firstAngle = Random.Range(0f, 360f);
            faultLSystem.Init(faultTexture, origin, firstAngle);
            faultLSystem.Reset();

            for (int i = 0; i < faultLSystem.iterationCount; i++)
            {
                stringToDraw = faultLSystem.IterateString(stringToDraw);
                faultLSystem.Reset();
            }
            faultLSystem.DrawString(stringToDraw, true);
            Apply(faultTexture);
        }

        bool ValidateFault()
        {
            var totalCount = 0;
            var inlandCount = 0;
            for (var i = 0; i < faultTexture.width; i++)
            {
                for (var j = 0; j < faultTexture.height; j++)
                {
                    if (faultTexture.GetPixel(i, j) != Color.black) continue;
                    if (landTexture.GetPixel(i, j) != Color.black) inlandCount++;
                    totalCount++;
                }
            }
            if (totalCount == 0) return false;
            return inlandCount / (float)totalCount > inlandFaultPortion && totalCount >= minPixelCount;
        }
    }

    void MakeMarineTexture(Texture2D landTexture, Texture2D heightMap, float seaLevel, HashSet<Vector2Int> edgePixels, Texture2D marineTexture)
    {
        var pixelsToTest = new Queue<Vector2Int>();         // pixels which may be below sea level with adjacent flooded pixels
        for (var i = 0; i < landTexture.width; i++)
            for (var j = 0; j < landTexture.height; j++)
                if (landTexture.GetPixel(i, j) == Color.black) pixelsToTest.Enqueue(new Vector2Int(i, j));

        var testedPixels = new HashSet<Vector2Int>();
        var k = 0;
        while(pixelsToTest.Count > 0)
        {
            var pixel = pixelsToTest.Dequeue();
            if (testedPixels.Contains(pixel)) continue;
            if (heightMap.GetPixel(pixel.x, pixel.y).r <= seaLevel)
            {
                marineTexture.SetPixel(pixel.x, pixel.y, Color.black);

                foreach (var position in adjacentPositions)
                {
                    var adjacentPixel = pixel + position;
                    if 
                    (
                        adjacentPixel.x <= SIZE - 1 
                        && adjacentPixel.x >= 0 
                        && adjacentPixel.y <= SIZE - 1 
                        && adjacentPixel.y >=0
                        && !testedPixels.Contains(adjacentPixel)
                        && !pixelsToTest.Contains(adjacentPixel)
                    )
                        pixelsToTest.Enqueue(adjacentPixel);
                }
            }
            testedPixels.Add(pixel);
        }
    }

    void MakeHeatTexture(Texture2D marineTexture, Texture2D heightMap, float seaLevel, Texture2D heatTexture)
    {
        var pixelData = new float[SIZE * SIZE];
        var origin = Random.Range(0, heatSampleTexture.height - SIZE);
        var seaLevelHeat = heatHeightCurve.Evaluate(seaLevel);
        for (var i = 0; i < SIZE; i++)
        {
            for (var j = 0; j < SIZE; j++)
            {
                bool isInland = marineTexture.GetPixel(i, j) != Color.black;
                float height;
                if (isInland) height = heightMap.GetPixel(i, j).r;
                else height = seaLevel;

                var datum = Normalize(heatSampleTexture.GetPixel(i, j + origin).r, new Vector2(0f, 1f), new Vector2(0f, heatSamplePortion));
                datum += Normalize(heatHeightCurve.Evaluate(height), new Vector2(seaLevelHeat, 1f), new Vector2(1f - heatSamplePortion, 1f));
                pixelData[i + j * SIZE] = datum;
            }
        }
        heatTexture.SetPixelData(pixelData, 0);
    }

    void MakeHumidityTexture(Texture2D marineTexture, Texture2D heatTexture, Texture2D humidityTexture)
    {
        var edgeDistances = new float[SIZE * SIZE];
        var edgeBounds = new Vector2(0f, float.NegativeInfinity);
        var edgePixels = GetEdges(marineTexture).ToList();
        TextureHelper.GetNearestNeighbors(marineTexture, edgePixels, (p) => p, out var rawEdgeDistances);
        for (var i = 0; i < SIZE; i++)
        {
            for (var j = 0; j < SIZE; j++)
            {
                var index = i + j * SIZE;
                var pixel = new Vector2Int(i, j);
                var distanceFromEdge = rawEdgeDistances[pixel];
                if (marineTexture.GetPixel(i, j) == Color.black) distanceFromEdge = 0f;
                edgeDistances[index] = distanceFromEdge;

                if (distanceFromEdge < edgeBounds.x) edgeBounds.x = distanceFromEdge;
                if (distanceFromEdge > edgeBounds.y) edgeBounds.y = distanceFromEdge;
            }
        }
        var pixelData = new float[SIZE * SIZE];
        for (var i = 0; i < SIZE; i++)
        {
            for (var j = 0; j < SIZE; j++)
            {
                var index = i + j * SIZE;
                var rawDatum = 1 - Normalize(edgeDistances[index], edgeBounds);
                pixelData[index] = Normalize(rawDatum, new Vector2(0f, 1f), new Vector2(0f, heatTexture.GetPixel(i, j).r));
            }
        }
        humidityTexture.SetPixelData(pixelData, 0);
    }

    void MakeBiomeTexture(Texture2D marineTexture, Texture2D heightTexture, Texture2D humidityTexture, Texture2D heatTexture, Texture2D heatNoiseTexture, Texture2D humiditynoiseTexture, Texture2D biomeTexture)
    {
        for (var i = 0; i < SIZE; i++)
        {
            for (var j = 0; j < SIZE; j++)
            {
                var heatNoise = Random.Range(-heatNoiseCoefficient, heatNoiseCoefficient) + heatPerlinCoefficient * (heatNoiseTexture.GetPixel(i, j).r - 0.5f);
                var humidityNoise = Random.Range(-humidityNoiseCoefficient, humidityNoiseCoefficient) + humidityPerlinCoefficient * (humiditynoiseTexture.GetPixel(i, j).r - 0.5f);
                var heat = heatTexture.GetPixel(i, j).r + heatNoise;
                var humidity = humidityTexture.GetPixel(i, j).r + humidityNoise;
                heat = Normalize(heat, new Vector2(0f, 1f + heatNoise));
                humidity = Normalize(humidity, new Vector2(0f, heat + humidityNoise), new Vector2(0f, heat));
                biomeTexture.SetPixel(i, j, biomeColors[BiomeUtility.GetBiome(humidity, heat)]);

                if (heightTexture.GetPixel(i, j).r > 0.995f)
                    biomeTexture.SetPixel(i, j, mountainColor);
                if (marineTexture.GetPixel(i, j) == Color.black)
                    biomeTexture.SetPixel(i, j, oceanColor);
            }
        }
    }

    Vector2[] MakeWindField(Vector2[] heightGradient)
    {
        var angle = Random.Range(0f, 2 * Mathf.PI);
        var magnitude = Random.Range(0f, maxBaseWindSpeed);
        var dominantWind = magnitude * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        var windField = new Vector2[heightGradient.Length];
        for (var i = 0; i < heightGradient.Length; i++)
            windField[i] = heightGradient[i] + dominantWind;
        return windField;
    }

    // uses the Sobel operator to approximate the gradient
    (Texture2D x, Texture2D y) OneChannelToGradient(Texture2D oneChannelTexture)
    {
        var pixelCount = oneChannelTexture.width * oneChannelTexture.height;
        var gradientX = MakePixelArray();
        var gradientY = MakePixelArray();
        /*for (var i = 0; i < oneChannelTexture.width; i++)
        {
            for (var j = 0; j < oneChannelTexture.height; j++)
            {
                gradientX[i + j * oneChannelTexture.width] = ApplyImageKernel(sobelX, new Vector2Int(i, j));
                gradientY[i + j * oneChannelTexture.width] = ApplyImageKernel(sobelY, new Vector2Int(i, j));
            }
        }*/
        DispatchShader(sobelX, gradientX);
        DispatchShader(sobelY, gradientY);
        return (ToTexture(gradientX), ToTexture(gradientY));

        Texture2D ToTexture(float[] f) => TextureHelper.ArrayToOneChannel(f, oneChannelTexture.width);

        float[] MakePixelArray() => new float[pixelCount].Fill(0f);

        /*float ApplyImageKernel(float[] imageKernel, Vector2Int id)
        {
            var resolution = new Vector2Int(oneChannelTexture.width, oneChannelTexture.height);
            float sum = 0;
            for (uint i = 0; i < 9; i++)
            {
                int neighborX = (id.x + adjacentPositionsInclusive[i].x + resolution.x) % resolution.x;
                int neighborY = (id.y + adjacentPositionsInclusive[i].y + resolution.y) % resolution.y;
                sum += oneChannelTexture.GetPixel(neighborX, neighborY).r * imageKernel[i];
            }
            return sum;
        }*/

        // HLSL
        /*float sum = 0;
        for (uint i = 0; i < 9; i++)
        {
            int neighborX = (int(id.x) + neighborDisplacements[i].x + resolution.x) % resolution.x;
            int neighborY = (int(id.y) + neighborDisplacements[i].y + resolution.y) % resolution.y;
            sum += inputData[neighborX + neighborY * resolution.x] * imageKernel[i];
        }
        result[id.x + id.y * resolution.x] = sum;*/

        void DispatchShader(float[] imageKernel, float[] result)
        {
            var inputDataID = Shader.PropertyToID("inputData");
            var imageKernelID = Shader.PropertyToID("imageKernel");
            var resultID = Shader.PropertyToID("result");
            var resolutionID = Shader.PropertyToID("resolution");

            var kernelIndex = imageProcessingLibrary.FindKernel("ApplyImageKernel");
            var inputDataBuffer = new ComputeBuffer(pixelCount, 4);
            inputDataBuffer.SetData(TextureHelper.OneChannelToArray(oneChannelTexture));
            var imageKernelBuffer = new ComputeBuffer(9, 4);
            imageKernelBuffer.SetData(imageKernel);
            var resultBuffer = new ComputeBuffer(pixelCount, 4);
            resultBuffer.SetData(result);
            imageProcessingLibrary.SetBuffer(kernelIndex, inputDataID, inputDataBuffer);
            imageProcessingLibrary.SetBuffer(kernelIndex, imageKernelID, imageKernelBuffer);
            imageProcessingLibrary.SetBuffer(kernelIndex, resultID, resultBuffer);
            imageProcessingLibrary.SetInts(resolutionID, oneChannelTexture.width, oneChannelTexture.height);

            imageProcessingLibrary.GetKernelThreadGroupSizes(kernelIndex, out var groupX, out var groupY, out _);
            var groupCountX = oneChannelTexture.width / (int)groupX;
            var groupCountY = oneChannelTexture.height / (int)groupY;
            imageProcessingLibrary.Dispatch(kernelIndex, groupCountX, groupCountY, 1);
            resultBuffer.GetData(result);

            inputDataBuffer.Release();
            imageKernelBuffer.Release();
            resultBuffer.Release();
        }
    }

    Texture2D OneChannelToRainbow(Texture2D oneChannelTexture, Vector2 bounds = default)
    {
        if (bounds == default) bounds = Vector2.up;
        var rainbowTexture = new Texture2D(oneChannelTexture.width, oneChannelTexture.height, TextureFormat.RGB24, false);
        for (var i = 0; i < oneChannelTexture.width; i++)
            for (var j = 0; j < oneChannelTexture.height; j++)
                rainbowTexture.SetPixel(i, j, Color.HSVToRGB(Normalize(oneChannelTexture.GetPixel(i, j).r, Vector2.up, bounds), cellSaturation, cellValue));
        return rainbowTexture;
    }

    float GetSeaLevel(Texture2D heightMap, HashSet<Vector2Int> edgePixels, float submergentCoastPortion)
    {
        List<Vector2Int> orderedEdgePixels = new List<Vector2Int>(edgePixels);
        orderedEdgePixels = orderedEdgePixels.OrderBy((p) => heightMap.GetPixel(p.x, p.y).r).ToList();
        var percentileIndex = Mathf.RoundToInt(submergentCoastPortion * orderedEdgePixels.Count) - 1;
        var seaLevelPixel = orderedEdgePixels[percentileIndex];
        return heightMap.GetPixel(seaLevelPixel.x, seaLevelPixel.y).r;
    }

    void Apply(Texture2D texture, HashSet<Vector2Int> edgePixels = null, Texture2D edgeMask = null)
    {
        texture.Apply();
        var textureToRender = new Texture2D(texture.width, texture.height, texture.format, false);
        textureToRender.SetPixelData(texture.GetRawTextureData(), 0);
        if (edgePixels != null)
            foreach (var pixel in edgePixels)
                textureToRender.SetPixel(pixel.x, pixel.y, GetEdgeColor(pixel));
        textureToRender.Apply();
        textureHolder.Push(textureToRender);

        Color GetEdgeColor(Vector2Int edgePixel)
        {
            var edgeColor = new Color(50f / 255f, 50f / 255f, 50f / 255f);
            if (edgeMask != null && edgeMask.GetPixel(edgePixel.x, edgePixel.y) != Color.black)
                edgeColor = new Color(150f / 255f, 150f / 255f, 150f / 255f);
            return edgeColor;
        }
    }

    void DrawIsoLines(Texture2D texture, Texture2D heightMap, (Texture2D x, Texture2D y) gradient)
    {
        var isoValues = new float[isoLineCount];
        var increment = 1f / (isoLineCount + 1f);
        for (var i = 0; i < isoLineCount; i++)
            isoValues[i] = (i + 1) * increment;

        for (var i = 0; i < SIZE; i++)
        {
            for (var j = 0; j < SIZE; j++)
            {
                for (var k = 0; k < isoLineCount; k++)
                {
                    var gradientX = gradient.x.GetPixel(i, j).r;
                    var gradientY = gradient.y.GetPixel(i, j).r;
                    var epsilon = isoLineThickness * Mathf.Sqrt(gradientX * gradientX + gradientY * gradientY);
                    if (Mathf.Abs(heightMap.GetPixel(i, j).r - isoValues[k]) < epsilon)
                    {
                        texture.SetPixel(i, j, Color.Lerp(texture.GetPixel(i, j), isoLineColor, isoLineColor.a));
                        continue;
                    }
                }
            }
        }
    }

    // returns each white pixel which is adjacent to a black pixel
    HashSet<Vector2Int> GetEdges(Texture2D texture)
    {
        var edgePixels = new HashSet<Vector2Int>();
        for (var i = 0; i < texture.width; i++)
        {
            for (var j = 0; j < texture.height; j++)
            {
                var pixel = new Vector2Int(i, j);
                if (texture.GetPixel(i, j) == Color.black) continue;
                foreach (var adjacentPosition in adjacentPositions)
                {
                    var adjacentPixel = adjacentPosition + pixel;
                    if (texture.GetPixel(adjacentPixel.x, adjacentPixel.y) == Color.black)
                        edgePixels.Add(adjacentPixel);
                }
            }
        }
        return edgePixels;
    }

    HashSet<Vector2Int> GetPixelsByColor(Texture2D texture, Color color)
    {
        var pixels = new HashSet<Vector2Int>();
        for (var i = 0; i < texture.width; i++)
            for (var j = 0; j < texture.height; j++)
                if (texture.GetPixel(i, j) == color)
                    pixels.Add(new Vector2Int(i, j));
        return pixels;
    }

    // returns a random pixel between *Vector2Int.zero* and *upperBounds* that fulfills *predicate*
    Vector2Int GetPixelSuchThat(Vector2Int upperBounds, Func<Vector2Int, bool> predicate)
    {
        const int MAX_TRIES = 100;
        Vector2Int candidate;
        var tries = 0;
        do candidate = new Vector2Int(Random.Range(0, upperBounds.x), Random.Range(0, upperBounds.y));
        while (++tries < MAX_TRIES && !predicate.Invoke(candidate));
        return candidate;
    }

    // takes a value within *bounds*, returns a value normalized within *targetBounds*
    float Normalize(float value, Vector2 bounds, Vector2 targetBounds = default)
    {
        if (targetBounds == default) targetBounds = Vector2.up;
        var normRange = bounds.y - bounds.x;
        var targetRange = targetBounds.y - targetBounds.x;
        var targetValue = (value - bounds.x) / normRange;
        targetValue *= targetRange;
        targetValue += targetBounds.x;
        return targetValue;
    }

    class Seed
    {
        const int MAX_TRY_COUNT = 20;

        public readonly Vector2Int position;

        public readonly Color color;

        public Seed(Vector2Int _position)
        {
            position = _position;
        }
        Seed(Vector2Int _position, float saturation, float value)
        {
            position = _position;

            color = Random.ColorHSV(0f, 1f, saturation, saturation, value, value, 1f, 1f);
        }

        public static bool TryMakeRandom(int boundSize, float _saturation, float _value, out Seed seed, Func<Seed, bool> predicate = null)
        {
            if (predicate == null) predicate = (_) => true;
            seed = MakeRandom(boundSize, _saturation, _value);
            var tryCount = 0;
            do
            {
                if (tryCount++ > MAX_TRY_COUNT) return false;
                MakeRandom(boundSize, _saturation, _value);
            }
            while (!predicate.Invoke(seed));
            return true;
        }
        static Seed MakeRandom(int boundSize, float _saturation, float _value)
        {
            var x = UnityEngine.Random.Range(0, boundSize + 1);
            var y = UnityEngine.Random.Range(0, boundSize + 1);
            return new Seed(new Vector2Int(x, y), _saturation, _value);
        }

        public float GetDistanceFrom(Vector2Int v)
        {
            return (position - v).magnitude;
        }
    }
}
