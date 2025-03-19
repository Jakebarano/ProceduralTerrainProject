using UnityEngine;

public class MeshGenerator
{
    public static void GenerateTerrainMesh(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        MeshData 

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                MeshData.vertices[x + y * width] = ; //TODO: Work from this line.
            }
        }
    }
}

public class MeshData
{
    //Variables
    public Vector3[] vertices;
    public int[] triangles;
    int triangleIndex;
    
    //Functions
    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex+2] = c;
        triangleIndex += 3;
    }
}