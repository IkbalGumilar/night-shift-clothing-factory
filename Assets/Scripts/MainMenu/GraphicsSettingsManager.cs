using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class GraphicsSettingsManager : MonoBehaviour
{
    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown graphicsDropdown;
    [SerializeField] private TMP_Dropdown shadowDropdown;
    [SerializeField] private TMP_Dropdown aaDropdown;
    [SerializeField] private TMP_Dropdown textureDropdown;
    [Header("Toggles")]
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle motionBlurToggle;
    [Header("Sliders")]
    [SerializeField] private Slider fovSlider;
    [Header("Post-Processing")]
    [SerializeField] private Volume postProcessingVolume;
    private MotionBlur motionBlur;
    private List<Resolution> filteredResolutions = new List<Resolution>();
    private void Start()
    {
        LoadResolutions();
        LoadGraphicsQuality();
        LoadShadows();
        LoadAntiAliasing();
        LoadTextureQuality();
        LoadSettings();
    }

    private void LoadResolutions()
    {
        resolutionDropdown.ClearOptions();
        filteredResolutions.Clear();
        List<string> options = new List<string>();
        string[] commonResolutions = { "1280x720", "1600x900", "1920x1080", "2560x1440", "3840x2160" };

        foreach (Resolution res in Screen.resolutions)
        {
            string resString = res.width + "x" + res.height;
            if (commonResolutions.Contains(resString) && !options.Contains(resString))
            {
                options.Add(resString);
                filteredResolutions.Add(res);
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", 2);
        resolutionDropdown.RefreshShownValue();
    }

    private void LoadGraphicsQuality()
    {
        graphicsDropdown.ClearOptions();
        graphicsDropdown.AddOptions(new List<string> { "Low", "Medium", "High", "Ultra" });
        graphicsDropdown.value = PlayerPrefs.GetInt("GraphicsQuality", 2);
        graphicsDropdown.RefreshShownValue();
    }

    private void LoadShadows()
    {
        shadowDropdown.ClearOptions();
        shadowDropdown.AddOptions(new List<string> { "No Shadows", "Soft Shadows", "Hard Shadows", "Ultra Shadows" });
        shadowDropdown.value = PlayerPrefs.GetInt("ShadowQuality", 1);
        shadowDropdown.RefreshShownValue();
    }

    private void LoadAntiAliasing()
    {
        aaDropdown.ClearOptions();
        aaDropdown.AddOptions(new List<string> { "Off", "FXAA", "MSAA 2x", "MSAA 4x", "TAA" });
        aaDropdown.value = PlayerPrefs.GetInt("AntiAliasing", 2);
        aaDropdown.RefreshShownValue();
    }

    private void LoadTextureQuality()
    {
        textureDropdown.ClearOptions();
        textureDropdown.AddOptions(new List<string> { "Ultra", "Hight", "Medium", "Low" });
        textureDropdown.value = PlayerPrefs.GetInt("TextureQuality", 0);
        textureDropdown.RefreshShownValue();
    }

    private void LoadSettings()
    {
        vSyncToggle.isOn = PlayerPrefs.GetInt("VSync", 1) == 1;
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        motionBlurToggle.isOn = PlayerPrefs.GetInt("MotionBlur", 1) == 1;
        fovSlider.value = PlayerPrefs.GetFloat("FOV", 75f);

        if (postProcessingVolume.profile.TryGet(out motionBlur))
            motionBlur.active = motionBlurToggle.isOn;
    }

    public void ApplySettings()
    {
        // Resolusi
        int resIndex = resolutionDropdown.value;
        Screen.SetResolution(filteredResolutions[resIndex].width, filteredResolutions[resIndex].height, fullscreenToggle.isOn);
        PlayerPrefs.SetInt("ResolutionIndex", resIndex);
        Debug.Log($"Resolution Set To: {filteredResolutions[resIndex].width}x{filteredResolutions[resIndex].height}");

        // Fullscreen
        Screen.fullScreenMode = fullscreenToggle.isOn ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        Debug.Log("Fullscreen Mode: " + Screen.fullScreenMode);

        // V-Sync
        QualitySettings.vSyncCount = vSyncToggle.isOn ? 1 : 0;
        PlayerPrefs.SetInt("VSync", vSyncToggle.isOn ? 1 : 0);
        Debug.Log("VSync: " + (vSyncToggle.isOn ? "Enabled" : "Disabled"));

        // Graphics Quality
        QualitySettings.SetQualityLevel(graphicsDropdown.value, true);
        PlayerPrefs.SetInt("GraphicsQuality", graphicsDropdown.value);
        Debug.Log("Graphics Quality Set To: " + graphicsDropdown.options[graphicsDropdown.value].text);

        // Shadow Quality
        UnityEngine.ShadowQuality[] shadowValues = { 
            UnityEngine.ShadowQuality.Disable, 
            UnityEngine.ShadowQuality.All, 
            UnityEngine.ShadowQuality.HardOnly, 
            UnityEngine.ShadowQuality.All 
        };

QualitySettings.shadows = shadowValues[shadowDropdown.value];

        // Anti-Aliasing
        int[] aaValues = { 0, 1, 2, 4, 8 };
        QualitySettings.antiAliasing = aaValues[aaDropdown.value];
        PlayerPrefs.SetInt("AntiAliasing", aaDropdown.value);
        Debug.Log("Anti-Aliasing Set To: " + aaDropdown.options[aaDropdown.value].text);
        // Texture Quality
        QualitySettings.globalTextureMipmapLimit = textureDropdown.value;
        PlayerPrefs.SetInt("TextureQuality", textureDropdown.value);
        Debug.Log("Texture Quality Set To: " + textureDropdown.options[textureDropdown.value].text);
        // Motion Blur
        if (motionBlur != null)
            motionBlur.active = motionBlurToggle.isOn;
        PlayerPrefs.SetInt("MotionBlur", motionBlurToggle.isOn ? 1 : 0);
        Debug.Log("Motion Blur: " + (motionBlurToggle.isOn ? "Enabled" : "Disabled"));
        // FOV
        Camera.main.fieldOfView = fovSlider.value;
        PlayerPrefs.SetFloat("FOV", fovSlider.value);
        Debug.Log("FOV Set To: " + fovSlider.value);
        PlayerPrefs.Save();
    }

    public void ResetToDefault()
    {
        PlayerPrefs.DeleteAll();
        LoadResolutions();
        LoadGraphicsQuality();
        LoadShadows();
        LoadAntiAliasing();
        LoadTextureQuality();
        LoadSettings();
        Debug.Log("Settings Reset to Default");
    }
}
