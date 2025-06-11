using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameSettings : MonoBehaviour
{
    [Header("Mouse Settings")]
    public Slider sensitivitySlider;
    public Toggle invertXToggle;
    public Toggle invertYToggle;
    
    [Header("Graphics Settings")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public TMP_Dropdown qualityDropdown;
    
    private Resolution[] resolutions;

    void Start()
    {
        LoadSettings();
        SetupResolutionOptions();
        SetupQualityOptions();
    }

    void LoadSettings()
    {
        sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 15f);
        sensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);

        invertXToggle.isOn = PlayerPrefs.GetInt("InvertX", 0) == 1;
        invertYToggle.isOn = PlayerPrefs.GetInt("InvertY", 0) == 1;
        invertXToggle.onValueChanged.AddListener(UpdateInvertX);
        invertYToggle.onValueChanged.AddListener(UpdateInvertY);

        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = fullscreenToggle.isOn;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    void SetupResolutionOptions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int currentResolutionIndex = 0;
        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height + " @ " + resolutions[i].refreshRateRatio + "Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    void SetupQualityOptions()
    {
        qualityDropdown.ClearOptions();
        List<string> qualityLevels = new List<string>(QualitySettings.names);
        qualityDropdown.AddOptions(qualityLevels);
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.onValueChanged.AddListener(SetQuality);
    }

    public void UpdateSensitivity(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
    }

    public void UpdateInvertX(bool isOn)
    {
        PlayerPrefs.SetInt("InvertX", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void UpdateInvertY(bool isOn)
    {
        PlayerPrefs.SetInt("InvertY", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetResolution(int index)
    {
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);
        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("QualityLevel", index);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}
