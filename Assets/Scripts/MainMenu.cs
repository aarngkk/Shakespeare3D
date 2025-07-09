using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Awake()
    {
        if (PlayerPrefs.HasKey("GraphicsQuality"))
        {
            int savedQuality = PlayerPrefs.GetInt("GraphicsQuality");
            QualitySettings.SetQualityLevel(savedQuality, true);
            Debug.Log("Loaded saved graphics quality: " + QualitySettings.names[savedQuality]);
        }
    }

    // Loads the "New Scene" menu
    public void CreateNewScene()
    {
        SceneManager.LoadScene("New Scene");
    }

    // Handles quitting the application
    public void QuitApp()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }

}
