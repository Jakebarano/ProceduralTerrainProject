using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NoiseScaleUI : MonoBehaviour
{
    [SerializeField] NoiseMapGenerator noiseMapGenerator;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI sText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider.value = noiseMapGenerator.noiseScale;
        sText.text = slider.value.ToString("0.00");

        slider.onValueChanged.AddListener((v) =>
        {
            sText.text = v.ToString("0.00");
            noiseMapGenerator.noiseScale = v;

            //auto update w/out using Update()
            if (noiseMapGenerator.autoUpdate)
            {
                noiseMapGenerator.DrawMap();
            }
        });
    }
}
