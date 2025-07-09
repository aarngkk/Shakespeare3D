using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Toggle subtitlesToggle;

    private void Start()
    {
        bool subtitlesOn = PlayerPrefs.GetInt("SubtitlesEnabled", 1) == 1;
        subtitlesToggle.isOn = subtitlesOn;

        ApplySubtitlesSetting(subtitlesOn);

        subtitlesToggle.onValueChanged.AddListener(ApplySubtitlesSetting);
    }

    void ApplySubtitlesSetting(bool isOn)
    {
        PlayerPrefs.SetInt("SubtitlesEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("Subtitles preference saved: " + isOn);
    }
}
