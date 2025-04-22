using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NoiseMapGenerator))]
public class MapGenEditorButton : Editor
{
    public override void OnInspectorGUI()
    {
        NoiseMapGenerator noiseMapGenerator = (NoiseMapGenerator)target;
        
        //Draw Inspector and Auto update if changes are made.
        if (DrawDefaultInspector())
        {
            if (noiseMapGenerator.autoUpdate)
            {
                noiseMapGenerator.DrawMap();
            }
        }
        
        //Update when pressed.
        if (GUILayout.Button("Generate Noise Map"))
        {
            noiseMapGenerator.DrawMap();
        }
    }
}
