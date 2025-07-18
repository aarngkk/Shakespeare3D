using UnityEngine;
using UnityEngine.UI;

public class PlaySceneButton : MonoBehaviour
{
    public Button playButton; // UI button to start the scene
    public CutsceneManager cutsceneManager; // Reference to cutscene controller
    private string currentSnapPoint; // Currently selected snap point

    public Outline bedOutline;      // Visual highlight for bed location
    public Outline dresserOutline;  // Visual highlight for dresser location

    void Start()
    {
        playButton.interactable = false; // Disable button at start
    }

    // Updates current snap point and button state
    public void SetCurrentSnapPoint(string snapPoint)
    {
        Debug.Log($"Received snap point: {snapPoint}");
        currentSnapPoint = snapPoint;

        // Enable play button only if a valid snap point is set AND the first cutscene has not started
        if (cutsceneManager != null && !cutsceneManager.IsCutscene1Started())
        {
            playButton.interactable = (snapPoint == "Bed" || snapPoint == "Dresser");
        }
        else
        {
            playButton.interactable = false; // Disable the button if the first cutscene has started
        }

        // Enable/disable outline based on whether a snap point is selected
        SetOutlineState(string.IsNullOrEmpty(snapPoint));
    }

    // Toggles outline visibility for bed and dresser
    private void SetOutlineState(bool enable)
    {
        if (bedOutline != null)
            bedOutline.enabled = enable;

        if (dresserOutline != null)
            dresserOutline.enabled = enable;
    }

    // Handles play button click event
    public void OnPlayButtonPressed()
    {
        if (!string.IsNullOrEmpty(currentSnapPoint) && cutsceneManager != null)
        {
            // Disable yellow outlines when the cutscene starts
            DisableYellowOutlines();

            // Play the corresponding Timeline cutscene before transitioning
            cutsceneManager.PlayCutscene(currentSnapPoint, RestoreWhiteOutlines);
        }
        else
        {
            Debug.LogWarning("No snap point selected or CutsceneManager is missing!");
        }
    }

    // Function to disable yellow outlines
    private void DisableYellowOutlines()
    {
        if (bedOutline != null && dresserOutline != null)
        {
            bedOutline.enabled = false;
            dresserOutline.enabled = false;
        }
    }

    // Function to restore white outlines after the cutscene ends
    private void RestoreWhiteOutlines()
    {
        if (bedOutline != null)
        {
            bedOutline.enabled = true;
            bedOutline.OutlineColor = Color.white;
        }

        if (dresserOutline != null)
        {
            dresserOutline.enabled = true;
            dresserOutline.OutlineColor = Color.white;
        }
    }

    // Loads the next scene
    private void LoadNextScene()
    {
        Debug.Log("Loading next scene...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Play Scene");
    }
}
