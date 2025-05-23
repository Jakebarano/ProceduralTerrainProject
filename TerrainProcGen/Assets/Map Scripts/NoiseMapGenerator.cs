using System;
using System.Threading;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class NoiseMapGenerator : MonoBehaviour
{
    //Generation Mode
    public enum DrawMode {NoiseMap, ColorMap, Mesh, Falloff} //leave a third option as the voxel mode. //Voxel
    public DrawMode drawMode;
    public CreateNoise.NormalizationMode normalizationMode;
    public GameObject noiseMapChunk;    //Single Mesh Chunk for non-endless
    public GameObject noiseMap2D;       //Single 2D plane for Noise and color mode
    private bool isEndlessModeOn = false;
    public bool useFalloff = false;
    public GameObject viewer;
    [SerializeField] private Toggle Endlesstoggle;
    
    //Basic Params
    public const int mapChunkSize = 241;
    [Range(0, 6)]
    public int levelOfDetail;                              //Editor Specific LOD "Preview"
    
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
    private float[,] falloffMap;
    
    //Thread Queue + general params
    Queue<MapThreadInformation<DrawnMapData>> mapThreadQueue = new Queue<MapThreadInformation<DrawnMapData>>();
    Queue<MapThreadInformation<MeshData>> meshThreadQueue = new Queue<MapThreadInformation<MeshData>>();

    public void ToggleAutoUpdate()
    {
        autoUpdate = !autoUpdate;
    }

    public void ToggleEndlessModeOn()
    {
        isEndlessModeOn = !isEndlessModeOn;
        
        if (isEndlessModeOn == true)
        {
            noiseMapChunk.SetActive(false);
            GetComponent<EndlessTerrain>().enabled = true;
            GetComponent<EndlessTerrain>().UpdateVisibleChunks();
        }
        else if (isEndlessModeOn == false)
        {
            noiseMapChunk.SetActive(true);
            GetComponent<EndlessTerrain>().enabled = false;
            
            foreach(Transform child in this.transform)
            {
                child.gameObject.SetActive(false);
                GetComponent<EndlessTerrain>().ForceLastVisibleChunksDictReset();
            }

            viewer.transform.position = new Vector3(0, 100, 0);
        }
        
        Debug.Log(isEndlessModeOn.ToString());
    }
    
    public void SetDrawMode(int mode)
    {
        drawMode = (DrawMode)mode;
        Debug.Log(drawMode.ToString());

        if (isEndlessModeOn == false)
        {
            if (drawMode == DrawMode.ColorMap || drawMode == DrawMode.NoiseMap)
            {
                noiseMapChunk.SetActive(false);
                noiseMap2D.SetActive(true);
            }
            else if (drawMode == DrawMode.Mesh || drawMode == DrawMode.Falloff)
            {
                noiseMapChunk.SetActive(true);
                noiseMap2D.SetActive(false);
            }
        }
        else if (isEndlessModeOn)
        {
            if (drawMode == DrawMode.ColorMap || drawMode == DrawMode.NoiseMap)
            {
                noiseMapChunk.SetActive(false);
                noiseMap2D.SetActive(true);
            
                GetComponent<EndlessTerrain>().enabled = false;
            
                foreach(Transform child in this.transform)
                {
                    child.gameObject.SetActive(false);
                }
                GetComponent<EndlessTerrain>().ForceLastVisibleChunksDictReset();
                //TODO: Needs to update UI button state
                ToggleEndlessModeOn();
                Endlesstoggle.isOn = isEndlessModeOn;
            }
            if (drawMode == DrawMode.Mesh)
            {
                noiseMapChunk.SetActive(false);
                noiseMap2D.SetActive(false);
                
                GetComponent<EndlessTerrain>().enabled = true;
                GetComponent<EndlessTerrain>().UpdateVisibleChunks();
            }

            if (drawMode == DrawMode.Falloff)
            {
                noiseMapChunk.SetActive(true);
                noiseMap2D.SetActive(false);
            
                GetComponent<EndlessTerrain>().enabled = false;
            
                foreach(Transform child in this.transform)
                {
                    child.gameObject.SetActive(false);
                }
                GetComponent<EndlessTerrain>().ForceLastVisibleChunksDictReset();
                ToggleEndlessModeOn();
                Endlesstoggle.isOn = isEndlessModeOn;
            }
        }
    }

    //Multi-threading Logic
    
    //2D map Request + Thread
    public void RqstMapData( Vector2 center, Action<DrawnMapData> callback)
    {
        ThreadStart threadDelegate = delegate
        {
            NMapDataThread(center, callback);
        };
        
        new Thread(threadDelegate).Start();
    }

    void NMapDataThread(Vector2 center, Action<DrawnMapData> callback)
    {
        DrawnMapData mData = GenerateMapData(center);
        lock (mapThreadQueue)
        {
            mapThreadQueue.Enqueue(new MapThreadInformation<DrawnMapData>(callback, mData));
        }
    }
    
    //Mesh Request + Thread
    
    public void RqstMeshData(DrawnMapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadDelegate = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadDelegate).Start();
    }

    void MeshDataThread(DrawnMapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.noiseMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshThreadQueue)
        {
            meshThreadQueue.Enqueue(new MapThreadInformation<MeshData>(callback, meshData));
        }
    }
    
    //Map Data logic
    public void DrawMap()
    {
        DrawnMapData mapData = GenerateMapData(Vector2.zero);
        
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

        if (drawMode == DrawMode.Falloff)
        {
            chunk.DrawMapTexture(MapTextureGenerator.TextureFromHeightMap(FalloffEdgeGenerator.GenerateFOMap(mapChunkSize)));
        }
    }
    
    DrawnMapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = CreateNoise.GenerateANoiseMap(mapChunkSize, mapChunkSize,  mapSeed, noiseScale, numOctaves, noisePersistence, lacunarity, center + offset, normalizationMode, isEndlessModeOn);
        
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize]; //1D mapping for colors
        
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if (useFalloff)
                {
                    noiseMap[x,y] = Mathf.Clamp01(noiseMap[x,y] - falloffMap[x, y]);
                }
                
                //set the current heights
                float currentHeight = noiseMap[x, y];
                
                for (int i = 0; i < terrainRegions.Length; i++)
                {
                    if (currentHeight >= terrainRegions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = terrainRegions[i].tColor;
                    }
                    else
                    {
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

        falloffMap = FalloffEdgeGenerator.GenerateFOMap(mapChunkSize);
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
    
    
    void Update()
    {
        if (isEndlessModeOn)
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
    }

    void Awake()
    {
        falloffMap = FalloffEdgeGenerator.GenerateFOMap(mapChunkSize);
    }

    private void Start()
   {
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