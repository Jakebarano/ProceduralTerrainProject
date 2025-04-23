using System;
using System.Threading;
using System.Collections.Generic;
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
    
    //Thread Queue + general params
    Queue<MapThreadInformation<DrawnMapData>> mapThreadQueue = new Queue<MapThreadInformation<DrawnMapData>>();
    Queue<MapThreadInformation<MeshData>> meshThreadQueue = new Queue<MapThreadInformation<MeshData>>();

    public void ToggleAutoUpdate()
    {
        autoUpdate = !autoUpdate;
    }

    //Multi-threading Logic
    
    //2D map Request + Thread
    public void RqstMapData(Action<DrawnMapData> callback)
    {
        ThreadStart threadDelegate = delegate
        {
            NMapDataThread(callback);
        };
        
        new Thread(threadDelegate).Start();
    }

    void NMapDataThread(Action<DrawnMapData> callback)
    {
        DrawnMapData mData = GenerateMapData();
        lock (mapThreadQueue)
        {
            mapThreadQueue.Enqueue(new MapThreadInformation<DrawnMapData>(callback, mData));
        }
    }
    
    //Mesh Request + Thread
    
    public void RqstMeshData(DrawnMapData mapData, Action<MeshData> callback)
    {
        ThreadStart threadDelegate = delegate
        {
            MeshDataThread(mapData, callback);
        };
        
        new Thread(threadDelegate).Start();
    }

    void MeshDataThread(DrawnMapData mapData, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail);
        lock (meshThreadQueue)
        {
            meshThreadQueue.Enqueue(new MapThreadInformation<MeshData>(callback, meshData));
        }
    }

    void Update()
    {
        if (mapThreadQueue.Count > 0)
        {
            for (int i = 0; i < mapThreadQueue.Count; i++)
            {
                MapThreadInformation<DrawnMapData> threadInfo = mapThreadQueue.Dequeue();
                threadInfo.callback(threadInfo.param);
            }
        }

        if (meshThreadQueue.Count > 0)
        {
            for (int i = 0; i < meshThreadQueue.Count; i++)
            {
                MapThreadInformation<MeshData> threadInfo = meshThreadQueue.Dequeue();
                threadInfo.callback(threadInfo.param);
            }
        }
    }
    
    //Map Data logic
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
    //private int noiseMapMinAxisSize = 1;  //Depreciated w/mapsize checks (see below) that are obsolete as well.
    
    // Editor Validation Logic
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
    
    //Thread Struct
    struct MapThreadInformation<T>
    {
        public readonly Action<T> callback;
        public readonly T param;

        public MapThreadInformation(Action<T> callback, T param)
        {
            this.callback = callback;
            this.param = param;
        }
    }
    
    
    //Param Getter/Setter functions Here
    //TODO: figure out which ones to make private and make functions for getting/setting here.

    private void Start()
    {
        GenerateMapData();
    }
}

//Data Structs
[Serializable]
public struct TerrainData
{
    public string name;
    public float height;
    public Color tColor;                                            //could change this to a texture?.
}

public struct DrawnMapData
{
    public readonly float[,] noiseMap;                                       //Height map param for perlin noise.
    public readonly Color[] colorMap;                                       //These params are set to "read only" since they are simple structs where data should only be read.

    public DrawnMapData(float[,] noiseMap, Color[] colorMap)
    {
        this.noiseMap = noiseMap;
        this.colorMap = colorMap;
    }
}