using System;
using UnityEngine;

public class NoiseMapGenerator : MonoBehaviour
{
    //Basic Params
    public int noiseMapWidth = 25;
    public int noiseMapHeight = 25;
    public float noiseScale = 0.03f;
    
    //Advanced Params
    public int mapSeed = 1;
    public int numOctaves = 4;
    [Range(0,1)] public float noisePersistence = 0.5f;
    public float lacunarity = 2.0f;
    public Vector2 offset = new Vector2(0.0f, 0.0f);
    
    //Editor+GUI params
    public bool autoUpdate  = true;
    public TerrainData[] terrainRegions;
    
    public void GenerateMap()
    {
        float[,] noiseMap = CreateNoise.GenerateANoiseMap(noiseMapWidth, noiseMapHeight,  mapSeed, noiseScale, numOctaves, noisePersistence, lacunarity, offset);

        for (int y = 0; y < noiseMapHeight; y++)
        {
            for (int x = 0; x < noiseMapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                
            }
        }
        ApplyMap chunk = FindFirstObjectByType<ApplyMap>();
        chunk.DrawNoiseMap(noiseMap);
    }
    
    //Min-Max params
    private int noiseMapMinAxisSize = 1;
    private float noiseScaleMin = 0.01f;
    private int octavesMinimum = 1;
    private int octavesMaximum = 28;
    private float lacunarityMinimum = 1f;
    private void OnValidate()
    {
        //Validate Param values, this is an easy approach.
        if (noiseMapWidth < noiseMapMinAxisSize)
        {
            noiseMapWidth = noiseMapMinAxisSize;
        }
        if (noiseMapHeight < noiseMapMinAxisSize)
        {
            noiseMapHeight = noiseMapMinAxisSize;
        }

        if (noiseScale <= 0)
        {
            noiseScale = noiseScaleMin;
        }

        if (numOctaves < octavesMinimum)
        {
            numOctaves = octavesMinimum;
        }

        if (numOctaves > octavesMaximum)
        {
            numOctaves = octavesMaximum;
        }

        if (lacunarity < 1)
        {
            lacunarity = lacunarityMinimum;
        }
    }
}

[System.Serializable]
public struct TerrainData
{
    public string name;
    public float height;
    public Color colour;  //could change this to a color.
}