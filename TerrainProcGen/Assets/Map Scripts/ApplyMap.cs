using UnityEngine;

public class ApplyMap : MonoBehaviour
{
    public Renderer targetRenderer;
    public MeshFilter targetMeshFilter;
    public MeshRenderer targetMeshRenderer;

    public void DrawMapTexture(Texture2D texture)
    {
        //set the texture of the target (plane/chunk) to be that of the texture created using noise.
        targetRenderer.sharedMaterial.mainTexture = texture; //global change of texture across all instances.
        targetRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
    
    public void DrawMesh(MeshData tMeshData, Texture2D texture)
    {
        targetMeshFilter.sharedMesh = tMeshData.CreateMesh();
        targetMeshRenderer.sharedMaterial.mainTexture = texture;
    }
}

//targetRenderer.material.mainTexture = texture;    //local (single instance) change of texture.