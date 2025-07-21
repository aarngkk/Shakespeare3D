using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class VoiceRecorder : MonoBehaviour
{
    public Button recordButton;
    public Button stopButton;
    public Button playButton;

    private AudioSource audioSource;
    private AudioClip recordedClip;
    private string micDevice;
    private bool isRecording = false;
    [SerializeField] private int recordingDuration;

    private void Start()
    {
        foreach (var device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
        }

        audioSource = GetComponent<AudioSource>();
        micDevice = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;

        if (micDevice == null)
        {
            Debug.LogError("No microphone found!");
            recordButton.interactable = false;
            stopButton.interactable = false;
            playButton.interactable = false;
            return;
        }

        recordButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(StopRecording);
        playButton.onClick.AddListener(PlayRecording);

        stopButton.interactable = false;
        playButton.interactable = false;
    }

    public void StartRecording()
    {
        if (micDevice == null) return;

        Debug.Log("Recording started...");
        recordedClip = Microphone.Start(micDevice, false, recordingDuration, 44100);
        isRecording = true;

        recordButton.interactable = false;
        stopButton.interactable = true;
        playButton.interactable = false;
    }

    public void StopRecording()
    {
        if (!isRecording) return;

        Microphone.End(micDevice);
        SaveClip(recordedClip, "user_voice");
        isRecording = false;

        Debug.Log("Recording stopped.");

        stopButton.interactable = false;
        playButton.interactable = true;
        recordButton.interactable = true;
    }

    public void PlayRecording()
    {
        if (recordedClip != null)
        {
            audioSource.clip = recordedClip;
            audioSource.Play();
            Debug.Log("Playing recording...");
        }
    }

    void SaveClip(AudioClip clip, string filename)
    {
        var filepath = Path.Combine(Application.persistentDataPath, filename + ".wav");
        Directory.CreateDirectory(Path.GetDirectoryName(filepath));
        File.WriteAllBytes(filepath, WavUtility.FromAudioClip(clip));
        Debug.Log("Saved to: " + filepath);
    }
}
