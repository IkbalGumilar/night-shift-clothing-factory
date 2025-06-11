using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SliderSensitivitas : MonoBehaviour
{
    [SerializeField] private Slider sliderSensitivitas;
    private float nilaiAsli;

    private void Start()
    {
        nilaiAsli = PlayerPrefs.GetFloat("SensitivitasMouse", 15f);
        sliderSensitivitas.value = nilaiAsli;
    }

    private void Update()
    {
        if (Gamepad.current != null)
        {
            sliderSensitivitas.value = nilaiAsli * 10f; // Jika pakai gamepad, x10
        }
        else
        {
            sliderSensitivitas.value = nilaiAsli; // Jika pakai mouse, tetap normal
        }
    }
}
