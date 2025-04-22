using System;
using UnityEngine;
using UnityEngine.Serialization;

public class NoiseMapGenerator : MonoBehaviour
{
    //Generation Mode
    public enum DrawMode {NoiseMap, ColorMap, Mesh, Voxel} //leave a third option as the voxel mode.
    public DrawMode drawMode;
    
    //Basic Params
    public const int mapChunkSize = 241;
    [Range(0, 6)]
    public int levelOfDetail;
    
    //TODO: set these up to be used later depending on mode.
    // public int noiseMapWidth = 25;
    // public int noiseMapHeight = 25;
    
    public float noiseScale = 0.03f;
    
    //Advanced Params
    public int mapSeed = 1;
    public int numOctaves = 4;
    [Range(0,1)] public float noisePersistence = 0.5f;
    public float lacunarity = 2.0f;
    
    public Vector2 offset = new Vector2(0.0f, 0.0f);
    
    [FormerlySerializedAs("MeshHeightMultiplier")] public float meshHeightMultiplier = 2f;
    public AnimationCurve meshHeightCurve;
    
    //Editor/GUI/UI params
    public bool autoUpdate  = true;
    public TerrainData[] terrainRegions;

    public void ToggleAutoUpdate()
    {
        autoUpdate = !autoUpdate;
    }

    public void DrawMap()
    {
        DrawnMapData mapData = GenerateMapData();
        
        ApplyMap chunk = FindFirstObjectByType<ApplyMap>();
        if (drawMode == DrawMode.NoiseMap)
        {
            chunk.DrawMapTexture(MapTextureGenerator.TextureFromHeightMap(mapData.noiseMap));
        }

        if (drawMode == DrawMode.ColorMap)
        {
            chunk.DrawMapTexture(MapTextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }

        if (drawMode == DrawMode.Mesh)
        {
            chunk.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), MapTextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }

        if (drawMode == DrawMode.Voxel)
        {
            //TODO: Setup Voxels and do a voxel mode.
        }
    }
    
    DrawnMapData GenerateMapData()
    {
        float[,] noiseMap = CreateNoise.GenerateANoiseMap(mapChunkSize, mapChunkSize,  mapSeed, noiseScale, numOctaves, noisePersistence, lacunarity, offset);
        
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize]; //1D mapping for colors
        
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                //set the current heights
                float currentHeight = noiseMap[x, y];
                
                for (int i = 0; i < terrainRegions.Length; i++)
                {
                    if (currentHeight < terrainRegions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = terrainRegions[i].tColor;
                        break;
                    }
                }
            }
        }
        return new DrawnMapData(noiseMap, colorMap);
    }
    
    //Min-Max params
    private float noiseScaleMin = 0.01f;
    private int octavesMinimum = 1;
    private int octavesMaximum = 28;
    private float lacunarityMinimum = 1f;
    
    //private int noiseMapMinAxisSize = 1;  //Depreciated w/mapsize checks (see below) that are obselete as well.
    private void OnValidate()
    {
        //Validate Param values, this is an easy approach.

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
                
        //Note: after the refactor for width/height these are not needed currently.
        // if (mapChunkSize < noiseMapMinAxisSize)
        // {
        //     mapChunkSize = noiseMapMinAxisSize;
        // }
        // if (mapChunkSize < noiseMapMinAxisSize)
        // {
        //     mapChunkSize = noiseMapMinAxisSize;
        // }
    }
    
    //Param Getter/Setter functions Here
    //TODO: figure out which ones to make private and make functions for getting/setting here.

    private void Start()
    {
        GenerateMapData();
    }
}

[Serializable]
public struct TerrainData
{
    public string name;
    public float height;
    public Color tColor;                                            //could change this to a texture?.
}

public struct DrawnMapData
{
    public float[,] noiseMap;                                       //height map param for perlin noise.
    public Color[] colorMap;

    public DrawnMapData(float[,] noiseMap, Color[] colorMap)
    {
        this.noiseMap = noiseMap;
        this.colorMap = colorMap;
    }
}