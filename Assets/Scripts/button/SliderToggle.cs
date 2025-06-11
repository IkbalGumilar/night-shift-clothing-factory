using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderToggle : MonoBehaviour
{
    public Slider slider; // Referensi ke slider
    public Button toggleButton; // Tombol untuk mengubah nilai slider
    public TextMeshProUGUI statusText; // Menampilkan status ON/OFF (opsional)

    void Start()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleSlider);
        }

        UpdateStatus(slider.value);
    }

    void ToggleSlider()
    {
        if (slider != null)
        {
            slider.value = slider.value == 0 ? 1 : 0;
            UpdateStatus(slider.value);
        }
    }

    void UpdateStatus(float value)
    {
        if (statusText != null)
        {
            statusText.text = value == 1 ? "ON" : "OFF";
        }
    }
}
