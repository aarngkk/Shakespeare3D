using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GraphicsSettingsMenu : MonoBehaviour
{
    public TMP_Dropdown qualityDropdown;

    void Start()
    {
        // Populate dropdown with QualitySettings levels
        qualityDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>(QualitySettings.names);
        qualityDropdown.AddOptions(options);

        // Load saved preference if it exists
        int savedQuality = PlayerPrefs.GetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
        qualityDropdown.value = savedQuality;
        qualityDropdown.RefreshShownValue();

        // Listen for changes
        qualityDropdown.onValueChanged.AddListener(SetQuality);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex, true);
        PlayerPrefs.SetInt("GraphicsQuality", qualityIndex);
        PlayerPrefs.Save();

        Debug.Log($"Graphics quality set to: {QualitySettings.names[qualityIndex]}");
    }
}
