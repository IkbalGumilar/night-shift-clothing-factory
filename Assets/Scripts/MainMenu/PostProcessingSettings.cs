using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;


public class PostProcessingSettings : MonoBehaviour
{
    [Header("Post-Processing Volume")]
    [SerializeField] public Volume postProcessingVolume;

    [Header("UI Controls")]
    [SerializeField] private Toggle bloomToggle;
    [SerializeField] private Toggle motionBlurToggle;
    [SerializeField] private Toggle dofToggle;
    [SerializeField] private Toggle chromaticAberrationToggle;
    [SerializeField] private Toggle filmGrainToggle;

    private Dictionary<string, VolumeComponent> effects = new Dictionary<string, VolumeComponent>();

    private void Start()
    {
        if (postProcessingVolume == null || postProcessingVolume.profile == null)
        {
            Debug.LogWarning("Post-processing volume belum diatur!");
            return;
        }
        AddEffect<Bloom>("Bloom");
        AddEffect<MotionBlur>("MotionBlur");
        AddEffect<DepthOfField>("DOF");
        AddEffect<ChromaticAberration>("ChromaticAberration");
        AddEffect<FilmGrain>("FilmGrain");
        LoadSettings();
        bloomToggle.onValueChanged.AddListener(delegate { ApplySettings(); });
        motionBlurToggle.onValueChanged.AddListener(delegate { ApplySettings(); });
        dofToggle.onValueChanged.AddListener(delegate { ApplySettings(); });
        chromaticAberrationToggle.onValueChanged.AddListener(delegate { ApplySettings(); });
        filmGrainToggle.onValueChanged.AddListener(delegate { ApplySettings(); });
    }

    private void AddEffect<T>(string key) where T : VolumeComponent
    {
        if (postProcessingVolume.profile.TryGet(out T effect))
            effects[key] = effect;
    }

    public void ApplySettings()
    {
        SetEffect("Bloom", bloomToggle);
        SetEffect("MotionBlur", motionBlurToggle);
        SetEffect("DOF", dofToggle);
        SetEffect("ChromaticAberration", chromaticAberrationToggle);
        SetEffect("FilmGrain", filmGrainToggle);
        PlayerPrefs.Save();
    }

    private void SetEffect(string key, Toggle toggle)
    {
        if (effects.TryGetValue(key, out VolumeComponent effect))
            effect.active = toggle.isOn;

        PlayerPrefs.SetInt(key, toggle.isOn ? 1 : 0);
    }

    public void ResetToDefault()
    {
        string[] keys = { "Bloom", "MotionBlur", "DOF", "ChromaticAberration", "FilmGrain" };

        foreach (string key in keys)
            PlayerPrefs.DeleteKey(key);

        LoadSettings();
    }

    private void LoadSettings()
    {
        bloomToggle.isOn = GetToggleValue("Bloom");
        motionBlurToggle.isOn = GetToggleValue("MotionBlur");
        dofToggle.isOn = GetToggleValue("DOF");
        chromaticAberrationToggle.isOn = GetToggleValue("ChromaticAberration");
        filmGrainToggle.isOn = GetToggleValue("FilmGrain");

        ApplySettings();
    }

    private bool GetToggleValue(string key) => PlayerPrefs.GetInt(key, 1) == 1;
}
