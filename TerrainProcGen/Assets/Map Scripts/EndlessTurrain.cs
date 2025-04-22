using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EndlessTurrain : MonoBehaviour
{
    public const float MAX_VIEW_DIST = 300.0f;
    //public int ChunksViewDistance = 4; //Use this to do view distance loading similar to LODs but with number of chunks (as a multiplier)
    //to load around the player at a given time.
    public Transform viewerTransform;

    public static Vector2 viewerPos;

    private int aSingleChunkSize;
    private int chunksVisibleinDist;
    Dictionary<Vector2, TerrainChunk> TerrainChunksDict = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> TerrainChunksVisibleSinceLastUpdate = new List<TerrainChunk>();
    void Start()
    {
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
                    TerrainChunksDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, aSingleChunkSize, transform));
                }
            }
        }
    }

    public class TerrainChunk
    {
        private const float DEFAULT_PLANE_SIZE = 10.0f; //default size of a plane primitive.
        GameObject meshObject;
        Vector2 position;
        Bounds chunkBounds;
        public TerrainChunk(Vector2 coord, int size, Transform parentTransform)
        {
            position = coord * size;
            Vector3 posV3 = new Vector3(position.x, 0, position.y);
            chunkBounds = new Bounds(position, (Vector2.one * size));
            
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = posV3;
            meshObject.transform.localScale = Vector3.one * size / DEFAULT_PLANE_SIZE;
            meshObject.transform.SetParent(parentTransform);
            SetVisible(false);
        }

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
