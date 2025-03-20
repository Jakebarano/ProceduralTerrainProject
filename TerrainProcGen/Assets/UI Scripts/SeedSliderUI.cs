using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SeedSliderUI : MonoBehaviour
{
    [SerializeField] NoiseMapGenerator noiseMapGenerator;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI sText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider.value = noiseMapGenerator.mapSeed;
        sText.text = slider.value.ToString("0");
        
        slider.onValueChanged.AddListener((v) =>
        {
            sText.text = v.ToString("0");
            noiseMapGenerator.mapSeed = (int)v;
        });  
    }

    void Update()
    {
        if (noiseMapGenerator.autoUpdate)
        {
            noiseMapGenerator.GenerateMap();
        }
    }
}
