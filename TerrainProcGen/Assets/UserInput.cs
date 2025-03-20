using UnityEngine;

public class UserInput : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; // Stop playing in the editor
        #else
                    Application.Quit(); // Quit the built application
        #endif
    }
}
