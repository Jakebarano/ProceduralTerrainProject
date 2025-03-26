using System.Collections.Generic;
using UnityEngine;

public class EndlessTurrain : MonoBehaviour
{
    public const float maxViewDist = 300;
    public Transform viewerTransform;

    public static Vector2 viewerPos;

    private int aSingleChunkSize;
    private int chunksVisibleinDist;
    Dictionary<Vector2, TerrainChunk> TerrainChunksDict = new Dictionary<Vector2, TerrainChunk>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void UpdateVisibleChunks()
    {
        int currChunkCoordX = Mathf.RoundToInt(viewerPos.x/aSingleChunkSize);
        int currChunkCoordY = Mathf.RoundToInt(viewerPos.y/aSingleChunkSize);

        for (int yoffset = -chunksVisibleinDist; yoffset <= chunksVisibleinDist; yoffset++)
        {
            for (int xoffset = -chunksVisibleinDist; xoffset <= chunksVisibleinDist; xoffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currChunkCoordX + xoffset, currChunkCoordY + yoffset);

                if (TerrainChunksDict.ContainsKey(viewedChunkCoord))
                {
                    //WIP some stuff will go here - past Jake
                }
                else
                {
                    TerrainChunksDict.Add(viewedChunkCoord, TerrainChunksDict[viewedChunkCoord]);
                }
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        private const float DEFAULT_PLANE_SIZE = 10.0f; //default size of a plane primative.

        public TerrainChunk(Vector2 coord, int size)
        {
            position = coord * size;
            Vector3 posv3 = new Vector3(position.x, 0, position.y);

            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = posv3;
            meshObject.transform.localScale = Vector3.one * size / DEFAULT_PLANE_SIZE;
        }
        
        //TODO: more functions here // 9:29 time stamp.
    }
}
