using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OffsetSlidersUI : MonoBehaviour
{
    [SerializeField] NoiseMapGenerator noiseMapGenerator;
    [SerializeField] private Slider sliderX;
    [SerializeField] private TextMeshProUGUI sXText;
    [SerializeField] private Slider sliderY;
    [SerializeField] private TextMeshProUGUI sYText;
    void Start()
    {
        sliderX.value = noiseMapGenerator.offset.x;
        sXText.text = sliderX.value.ToString("0.00");
        
        sliderX.onValueChanged.AddListener((v) =>
        {
            sXText.text = v.ToString("0.00");
            noiseMapGenerator.offset.x = v;
        });  
        
        sliderY.value = noiseMapGenerator.offset.y;
        sYText.text = sliderY.value.ToString("0.00");
        
        sliderY.onValueChanged.AddListener((v) =>
        {
            sYText.text = v.ToString("0.00");
            noiseMapGenerator.offset.y = v;
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
