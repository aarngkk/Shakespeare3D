using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Defines subtitle data for a cutscene
[CreateAssetMenu(fileName = "SubtitleData", menuName ="Cutscenes/Subtitle Data", order = 1)]
public class SubtitleData : ScriptableObject
{
    [System.Serializable]
    public class SubtitleLine
    {
        public float timestamp; // Time in seconds to show this line
        [TextArea]
        public string text;     // Subtitle text
    }

    public List<SubtitleLine> lines = new List<SubtitleLine>(); // List of all subtitle lines
}
