using UnityEngine;

public class CreateNoise
{
    public static float[,] GenerateANoiseMap(int noiseMapWidth, int noiseMapHeight, float noiseScale)
    {
        float[,] noiseMap = new float[noiseMapWidth, noiseMapHeight];
        
        
        //Clamp scale for 0 value (results in Integer Coordinates
        //(when using Perlin Noise, integer coordinates ALWAYS provide the same value.)
        if (noiseScale <= 0)
        {
            noiseScale = 0.001f;
        }
        //Index by Height position(y) first, THEN Width position (x)
        for (int y = 0; y < noiseMapHeight; y++)
        {
            for (int x = 0; x < noiseMapWidth; x++)
            {
                //set Sample values from width/height values.
                float sampleX = x / noiseScale;
                float sampleY = y / noiseScale;
                
                //Generate Perlin Val using Mathf.PerlinNoise(), and assign within the 2D array.
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseMap[x, y] = perlinValue;
            }
        }
        
        return noiseMap;
    }
}
