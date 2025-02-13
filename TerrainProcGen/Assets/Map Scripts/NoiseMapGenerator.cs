using UnityEngine;

public class NoiseMapGenerator : MonoBehaviour
{
    public int noiseMapWidth;
    public int noiseMapHeight;
    public float noiseScale;
    public bool autoUpdate;

    public void GenerateMap()
    {
        float[,] noiseMap = CreateNoise.GenerateANoiseMap(noiseMapWidth, noiseMapHeight, noiseScale);

        ApplyMap chunk = FindFirstObjectByType<ApplyMap>();
        chunk.DrawNoiseMap(noiseMap);
    }
}
