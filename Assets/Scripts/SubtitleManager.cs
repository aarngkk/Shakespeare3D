using UnityEngine;
using UnityEngine.Playables;
using TMPro;

// Manages displaying subtitles during cutscenes
public class SubtitleManager : MonoBehaviour
{
    public TextMeshProUGUI subtitleText;  // UI text for subtitles
    public PlayableDirector director;     // Timeline director

    private SubtitleData currentSubtitleData;  // Current subtitle data
    private int currentLineIndex;              // Current line index

    // Starts subtitles with new data and director
    public void PlaySubtitles(SubtitleData data, PlayableDirector newDirector)
    {
        currentSubtitleData = data;
        director = newDirector;
        currentLineIndex = 0;
        subtitleText.text = "";
    }

    void Update()
    {
        // Only update if a cutscene is playing
        if (director == null || currentSubtitleData == null || director.state != PlayState.Playing)
            return;

        // Check if it's time to show the next line
        if (currentLineIndex < currentSubtitleData.lines.Count)
        {
            float time = (float)director.time;
            if (time >= currentSubtitleData.lines[currentLineIndex].timestamp)
            {
                subtitleText.text = currentSubtitleData.lines[currentLineIndex].text;
                currentLineIndex++;
            }
        }
    }

    // Clears the subtitle text
    public void ClearSubtitles()
    {
        subtitleText.text = "";
        currentSubtitleData = null;
    }
}
