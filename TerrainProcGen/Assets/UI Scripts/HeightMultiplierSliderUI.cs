using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class HeightMultiplierSliderUI : MonoBehaviour
{
    [SerializeField] NoiseMapGenerator noiseMapGenerator;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI sText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider.value = noiseMapGenerator.meshHeightMultiplier;
        sText.text = slider.value.ToString("0");
        
        slider.onValueChanged.AddListener((v) =>
        {
            //string manipulation
            sText.text = v.ToString("0");
            noiseMapGenerator.meshHeightMultiplier = (int)v;
            
            //auto update w/out using Update()
            if (noiseMapGenerator.autoUpdate)
            {
                noiseMapGenerator.DrawMap();
            }
        });  
    }
}
