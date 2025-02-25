using UnityEngine;

public class CreateNoise
{
    public static float[,] GenerateANoiseMap(int noiseMapWidth, int noiseMapHeight, int seed, float noiseScale, int octaves, float persistence, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[noiseMapWidth, noiseMapHeight];
        
        System.Random random = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        
        int randMax = 100000;
        int randMin = -100000;
        
        for (int i = 0; i < octaves; i++)
        {
            float octaveOffsetX = random.Next(randMin, randMax)/100000f + offset.x;
            float octaveOffsetY = random.Next(randMin, randMax)/100000f + offset.y;
            octaveOffsets[i] = new Vector2(octaveOffsetX, octaveOffsetY);
        }
        
        //Clamp scale for 0 value (results in Integer Coordinates
        //(When using Perlin noise, integer coordinates ALWAYS provide the same value.)
        if (noiseScale <= 0)
        {
            noiseScale = 0.001f;
        }
        
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        
        float halfWidth = noiseMapWidth / 2f;
        float halfHeight = noiseMapHeight / 2f;
        
        
        //Index by Height position(y) first, THEN Width position (x)
        for (int y = 0; y < noiseMapHeight; y++)
        {
            for (int x = 0; x < noiseMapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                
                for (int i = 0; i < octaves; i++)
                {
                    //Set Sample values from width/height values.
                    float sampleX = (x - halfWidth) / noiseScale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / noiseScale * frequency + octaveOffsets[i].y;
                
                    //Generate Perlin Val using Mathf.PerlinNoise(), and assign within the 2D array.
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2-1;
                    noiseHeight += perlinValue * amplitude;
                    
                    amplitude *= persistence;  //Range: 0-1 decreasing, w/ each octave.
                    frequency *= lacunarity;   //Range: 1+, increases w/ each octave.
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }
        
        //Set up noiseMap values after normalizing the initial values.
        for (int y = 0; y < noiseMapHeight; y++)
        {
            for (int x = 0; x < noiseMapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}
