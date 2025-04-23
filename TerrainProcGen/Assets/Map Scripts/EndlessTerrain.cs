using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    //Variables/Params for Endless Mode
    public const float MAX_VIEW_DIST = 300.0f;
    public Transform viewerTransform;
    public static Vector2 viewerPos;
    private int aSingleChunkSize;
    private int chunksVisibleinDist;
    
    //public int ChunksViewDistance = 4; //Use this to do view distance loading similar to LODs but with number of chunks (as a multiplier)
    //to load around the player at a given time.
    
    //Map material
    public Material meshMaterial;
    
    //Endless Mode Containers
    Dictionary<Vector2, TerrainChunk> TerrainChunksDict = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> TerrainChunksVisibleSinceLastUpdate = new List<TerrainChunk>();
    
    //Static Variables/Params
    static NoiseMapGenerator noiseMapGenerator;
    void Start()
    {
        noiseMapGenerator = FindFirstObjectByType<NoiseMapGenerator>();
        aSingleChunkSize = NoiseMapGenerator.mapChunkSize - 1;
        chunksVisibleinDist = Mathf.RoundToInt(MAX_VIEW_DIST / aSingleChunkSize);
    }

    void Update()
    {
        viewerPos = new Vector2(viewerTransform.position.x, viewerTransform.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        //Remove Chunks outside the user's view distance after each update so it is not overloaded.
        foreach (TerrainChunk chunk in TerrainChunksVisibleSinceLastUpdate)
        {
            chunk.SetVisible(false);
        }
        TerrainChunksVisibleSinceLastUpdate.Clear();
        
        //Load Chunks based on view distance of User. (View Distance should become a setting).
        int currChunkCoordX = Mathf.RoundToInt(viewerPos.x/aSingleChunkSize);
        int currChunkCoordY = Mathf.RoundToInt(viewerPos.y/aSingleChunkSize);

        for (int yoffset = -chunksVisibleinDist; yoffset <= chunksVisibleinDist; yoffset++)
        {
            for (int xoffset = -chunksVisibleinDist; xoffset <= chunksVisibleinDist; xoffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currChunkCoordX + xoffset, currChunkCoordY + yoffset);

                if (TerrainChunksDict.ContainsKey(viewedChunkCoord))
                {
                    TerrainChunksDict[viewedChunkCoord].UpdateChunk();
                    if (TerrainChunksDict[viewedChunkCoord].isVisible())
                    {
                        TerrainChunksVisibleSinceLastUpdate.Add(TerrainChunksDict[viewedChunkCoord]);
                    }
                }
                else
                {
                    TerrainChunksDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, aSingleChunkSize, transform, meshMaterial));
                }
            }
        }
    }

    public class TerrainChunk
    {
        private const float DEFAULT_PLANE_SIZE = 10.0f; //default size of a plane primitive.
        
        //Game Object +  Components
        GameObject meshObject;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        
        Vector2 position;
        Bounds chunkBounds;
        public TerrainChunk(Vector2 coord, int size, Transform parentTransform, Material material)
        {
            position = coord * size;
            Vector3 posV3 = new Vector3(position.x, 0, position.y);
            chunkBounds = new Bounds(position, (Vector2.one * size));

            meshObject = new GameObject("TerrainChunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;
            meshObject.transform.position = posV3;
            
            //meshObject.transform.localScale = Vector3.one * size / DEFAULT_PLANE_SIZE;  //TODO: Implement logic for if this is in non-mesh mode
            meshObject.transform.SetParent(parentTransform);
            SetVisible(false);
            
            //Send Map Data request
            noiseMapGenerator.RqstMapData(OnMapDataRecvd);
        }
        
        //Threading Functions for TerrainChunk Generation
        void OnMapDataRecvd(DrawnMapData mapData)
        {
            //Validation for Receiving Map Data
            print("Map data received");
            
            //Mesh based Logic
            noiseMapGenerator.RqstMeshData(mapData, OnMeshDataRecvd);
        }

        void OnMeshDataRecvd(MeshData meshData)
        {
            meshFilter.mesh = meshData.CreateMesh();
        }
        
        
        //General Functions for TerrainChunk Class
        public void UpdateChunk()
        {
            float viewerDistFromNearestEdge = Mathf.Sqrt(chunkBounds.SqrDistance(viewerPos));
            bool visibile = viewerDistFromNearestEdge <= MAX_VIEW_DIST;
            SetVisible(visibile);
        }
        
        //Visibility functions for all terrain chunks.
        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }
        public bool isVisible()
        {
            return meshObject.activeSelf;
        }
    }
}
