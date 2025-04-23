using UnityEngine;

public class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail)
    {
        //Create and Animation Curve to seperate curves for each mesh instance in Threading
        AnimationCurve iHeightCurve = new AnimationCurve(heightCurve.keys);
        
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2.0f;
        float topLeftZ = (height - 1) / 2.0f;

        int SimplificationIncrement = (levelOfDetail == 0)?1:levelOfDetail * 2;
        int verticesPerLine = (width-1) / SimplificationIncrement + 1;
        
        MeshData terrainMeshData = new MeshData (verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        for (int y = 0; y < height; y+= SimplificationIncrement)
        {
            for (int x = 0; x < width; x+= SimplificationIncrement)
            {
                terrainMeshData.vertices[vertexIndex] = new Vector3 (topLeftX + x, iHeightCurve.Evaluate(heightMap[x, y])  * heightMultiplier, topLeftZ - y);
                terrainMeshData.uvs[vertexIndex] = new Vector2 (x / (float)width, y / (float)height);

                if (x < width - 1 && y < height - 1)
                {
                    terrainMeshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    terrainMeshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }
        return terrainMeshData;
    }
}

public class MeshData
{
    //Variables
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    
    int triangleIndex;
    
    //Functions
    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex+2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
    
}