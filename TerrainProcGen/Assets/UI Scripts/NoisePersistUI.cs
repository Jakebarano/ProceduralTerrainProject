using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NoisePersistUI : MonoBehaviour
{
    [SerializeField] NoiseMapGenerator noiseMapGenerator;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI sText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider.value = noiseMapGenerator.noisePersistence;
        sText.text = slider.value.ToString("0.000");
        
        slider.onValueChanged.AddListener((v) =>
        {
            sText.text = v.ToString("0.000");
            noiseMapGenerator.noisePersistence = v;
        });  
    }

    // Update is called once per frame
    void Update()
    {
        if (noiseMapGenerator.autoUpdate)
        {
            noiseMapGenerator.GenerateMap();
        }
    }

}
