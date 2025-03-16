using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BiomeUtility
{
    static Node root;

    static BiomeUtility()
    {
        var rainforestNode = new Node(Biome.TropicalRainforest, 1f, 1f);
        var savannaNode = new Node(Biome.Savanna, 1f, 0.625f, rainforestNode);
        var subtropicalDesertNode = new Node(Biome.SubtropicalDesert, 1f, 0.25f, savannaNode);
        var seasonalForestNode = new Node(Biome.SeasonalForest, 0.75f, 0.625f, rainforestNode, subtropicalDesertNode);
        var coldDesertNode = new Node(Biome.ColdDesert, 0.75f, 0.25f, seasonalForestNode, subtropicalDesertNode); 
        var borealForestNode = new Node(Biome.BorealForest, 0.5f, 1f, null, coldDesertNode);
        root = new Node(Biome.Tundra, 0.25f, 1f, null, borealForestNode);
    }

    public static Biome GetBiome(float humidity, float temperature) => root.GetNodeFromPosition(humidity, temperature).biome;

    class Node
    {
        public readonly Biome biome;

        readonly float humidityBound;
        readonly float heatBound;

        readonly Node heatChild;
        readonly Node humidityChild;

        public Node(Biome _biome, float _heatBound, float _humidityBound, Node _humidityChild = null, Node _heatChild = null)
        {
            biome = _biome;

            humidityBound = _humidityBound;
            heatBound = _heatBound;

            heatChild = _heatChild;
            humidityChild = _humidityChild;
        }

        public Node GetNodeFromPosition(float humidity, float temperature)
        {
            if (temperature > heatBound && heatChild != null) return heatChild.GetNodeFromPosition(humidity, temperature);
            if (humidity > humidityBound && humidityChild != null) return humidityChild.GetNodeFromPosition(humidity, temperature);
            return this;
        }
    }
}

public enum Biome
{
    Tundra,
    BorealForest,
    ColdDesert,
    SubtropicalDesert,
    SeasonalForest,
    Savanna,
    TropicalRainforest
}
