using UnityEngine;

public class ApplyMap : MonoBehaviour
{
    public Renderer targetRenderer;

    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        Texture2D texture = new Texture2D(width, height);
        
        Color[] colorMap = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                //Lerp between colors for each index in the colorMap;
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]); //Update for more colors
            }
        }
        //Set the pixel values and apply (CPU -> GPU copy).
        texture.SetPixels(colorMap);
        texture.Apply();
        
        //set the texture of the target (plane/chunk) to be that of the texture created using noise.
        targetRenderer.sharedMaterial.mainTexture = texture; //global change or texture.
        
        //targetRenderer.material.mainTexture = texture;    //local (single instance) change of texture.

        targetRenderer.transform.localScale = new Vector3(width, 1, height);
    }
}
