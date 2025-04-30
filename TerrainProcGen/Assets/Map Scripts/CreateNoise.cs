using UnityEngine;

public class CreateNoise
{
    public enum NormalizationMode
    {
        Global,
        Local
    };
    
    public bool noiseIsEndless = false;
    
    public static float[,] GenerateANoiseMap(int noiseMapWidth, int noiseMapHeight, int seed, float noiseScale, int octaves, float persistence, float lacunarity, Vector2 offset, NormalizationMode normalizationMode, bool noisecalcMode)
    {
        float[,] noiseMap = new float[noiseMapWidth, noiseMapHeight];
        
        System.Random random = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        
        int randMax = 100000;
        int randMin = -100000;
        
        float amplitude = 1;
        float frequency = 1;
        float maxPossibleHeight = 0;
        
        for (int i = 0; i < octaves; i++)
        {
            float octaveOffsetX = random.Next(randMin, randMax)/100000f + offset.x;
            float octaveOffsetY = random.Next(randMin, randMax)/100000f - offset.y;
            octaveOffsets[i] = new Vector2(octaveOffsetX, octaveOffsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }
        
        //Clamp scale for 0 value (results in Integer Coordinates
        //(When using Perlin noise, integer coordinates ALWAYS provide the same value.)
        if (noiseScale <= 0)
        {
            noiseScale = 0.001f;
        }
        
        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;
        
        float halfWidth = noiseMapWidth / 2f;
        float halfHeight = noiseMapHeight / 2f;
        
        
        //Index by Height position(y) first, THEN Width position (x)
        for (int y = 0; y < noiseMapHeight; y++)
        {
            for (int x = 0; x < noiseMapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;
                
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX;
                    float sampleY;
                    //Set Sample values from width/height values.
                    if (noisecalcMode == true)
                    {
                        sampleX = (x - halfWidth + octaveOffsets[i].x) / noiseScale * frequency;
                        sampleY = (y - halfHeight + octaveOffsets[i].y) / noiseScale * frequency;
                    }
                    else
                    {
                        sampleX = (x - halfWidth ) / noiseScale * frequency + octaveOffsets[i].x;
                        sampleY = (y - halfHeight) / noiseScale * frequency + octaveOffsets[i].y;
                    }
                
                    //Generate Perlin Val using Mathf.PerlinNoise(), and assign within the 2D array.
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2-1;
                    noiseHeight += perlinValue * amplitude;
                    
                    amplitude *= persistence;  //Range: 0-1 decreasing, w/ each octave.
                    frequency *= lacunarity;   //Range: 1+, increases w/ each octave.
                }

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }
        
        //Set up noiseMap values after normalizing the initial values.
        for (int y = 0; y < noiseMapHeight; y++)
        {
            for (int x = 0; x < noiseMapWidth; x++)
            {
                if (normalizationMode == NormalizationMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
                else //Global Mode
                {
                    float normalizedHeight = (noiseMap[x, y] + 1)/(maxPossibleHeight);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }
        return noiseMap;
    }
}
