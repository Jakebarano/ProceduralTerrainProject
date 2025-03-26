using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButtonUI : MonoBehaviour
{
    [SerializeField] NoiseMapGenerator noiseMapGenerator;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI bText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            noiseMapGenerator.ToggleAutoUpdate();
            UpdateButtonText();
        });  
    }
    
    public void UpdateButtonText()
    {
        if (bText != null)
        {
            bText.text = noiseMapGenerator.autoUpdate ? "Auto Update - ON" : "Auto Update - OFF";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
