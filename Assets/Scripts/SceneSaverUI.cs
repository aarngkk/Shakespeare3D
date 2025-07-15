using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;


public class SceneSaverUI : MonoBehaviour
{
    public ScriptSelectionManager scriptSelectionManager; // Reference to manager that handles script selections
    private CutsceneManager cutsceneManager; // Reference to CutsceneManager
    public TMP_InputField sceneNameInputField; // Input field for naming the saved scene
    public GameObject savePanel; // Save scene UI panel
    public GameObject leavePanel; // Panel shown when trying to leave without saving
    public GameObject overwriteConfirmPanel;// Panel shown if scene name already exists
    public TextMeshProUGUI errorMessageText; // Displays validation or error messages
    public List<Button> mainScreenButtons; // Buttons that should be disabled during modal panels
    public Button saveButton; // Button to trigger scene saving
    public Button undoButton; // Undo button to restore interactability after modal
    public bool sceneIsSaved = false; // Flag to track if scene was saved
    private bool wasUndoInteractable = false; // To restore undo button state
    private string pendingOverwriteSceneName = "";// Stores the scene name awaiting overwrite confirmation

    private void Start()
    {
        // Locate the CutsceneManager in the scene
        cutsceneManager = FindObjectOfType<CutsceneManager>();
    }

    public void OpenSavePanel()
    {
        // Save current undo button state
        if (undoButton != null)
        {
            wasUndoInteractable = undoButton.interactable;
        }
         // Pause cutscene if it's currently playing
        if (cutsceneManager != null && cutsceneManager.currentCutscene != null &&
            cutsceneManager.currentCutscene.state == PlayState.Playing)
        {
            cutsceneManager.currentCutscene.Pause();
            Debug.Log("Cutscene paused on save panel open.");
        }

        // Disable main screen buttons to prevent interaction
        if (mainScreenButtons != null)
        {
            foreach (Button btn in mainScreenButtons)
            {
                btn.interactable = false;
            }
        }

        // Clear previous input and show the save panel
        if (savePanel != null)
        {
            if (errorMessageText != null)
                errorMessageText.text = "";
            sceneNameInputField.text = "";

            savePanel.SetActive(true);
        }
    }


    public void CloseSavePanel()
    {
        // Hide the save panel
        if (savePanel != null)
        {
            savePanel.SetActive(false);
        }

        // Resume the cutscene if it was paused
        if (cutsceneManager != null && cutsceneManager.currentCutscene != null &&
            cutsceneManager.currentCutscene.state == PlayState.Paused)
        {
            cutsceneManager.currentCutscene.Resume();
            Debug.Log("Cutscene resumed after closing save panel.");
        }
        
        // Re-enable all main screen buttons
        if (mainScreenButtons != null)
        {
            foreach (Button btn in mainScreenButtons)
            {
                btn.interactable = true;
            }
        }

        // Restore previous undo button state
        if (undoButton != null) undoButton.interactable = wasUndoInteractable;
    }


     public void OnSaveButtonClicked()
    {

        string sceneName = sceneNameInputField.text;

        // Validate input
        if (string.IsNullOrEmpty(sceneName))
        {
            if (errorMessageText != null)
                errorMessageText.text = "Please enter a scene name before saving.";
            return;
        }

        // Hide the finished popup if it's open
        if (cutsceneManager != null && cutsceneManager.FinishedPopUp != null && cutsceneManager.FinishedPopUp.activeSelf)
        {
            cutsceneManager.HideFinishPopup();
        }

        string filePath = System.IO.Path.Combine(Application.persistentDataPath + "/SavedScenes/", sceneName + ".log");

        // If the file already exists, ask for overwrite confirmation
        if (System.IO.File.Exists(filePath))
        {
            pendingOverwriteSceneName = sceneName;
            if (overwriteConfirmPanel != null)
                overwriteConfirmPanel.SetActive(true);
            return;
        }

        // Save the scene directly if no conflict
        SaveScene(sceneName);
    }
    public void OnConfirmOverwrite()
    {
        // Proceed with overwrite
        if (!string.IsNullOrEmpty(pendingOverwriteSceneName))
        {
            SaveScene(pendingOverwriteSceneName);
            pendingOverwriteSceneName = "";
        }

        // Close the overwrite panel
        if (overwriteConfirmPanel != null)
            overwriteConfirmPanel.SetActive(false);
    }

    public void OnCancelOverwrite()
    {
        // Cancel overwrite attempt
        pendingOverwriteSceneName = "";
        if (overwriteConfirmPanel != null)
            overwriteConfirmPanel.SetActive(false);
    }

    private void SaveScene(string sceneName)
    {
        // Ensure directory exists
        string directoryPath = System.IO.Path.Combine(Application.persistentDataPath, "SavedScenes");
        if (!System.IO.Directory.Exists(directoryPath))
        {
            System.IO.Directory.CreateDirectory(directoryPath);
        }

        // Save the cutscene log
        if (cutsceneManager != null)
        {
            cutsceneManager.logFileName = sceneName;
            cutsceneManager.LogChoicesToFile();
        }
        else
        {
            Debug.LogError("CutsceneManager not found! Choices were not logged.");
        }

        sceneIsSaved = true;
        CloseSavePanel();

        SceneManager.LoadScene("Main Menu"); // Return to main menu
    }

    public void OnBackButtonPressed()
    {
        // Go to main menu
        SceneManager.LoadScene("Main Menu");
    }

    public void OnLeaveButtonClicked()
    {
        // User confirms they want to leave without saving
        SceneManager.LoadScene("Main Menu");
    }

    
    public void OpenLeavePanel()
    {
        // Save undo button state
        if (undoButton != null)
        {
            wasUndoInteractable = undoButton.interactable;
        }

        // Pause the cutscene
        if (cutsceneManager != null && cutsceneManager.currentCutscene != null &&
            cutsceneManager.currentCutscene.state == PlayState.Playing)
        {
            cutsceneManager.currentCutscene.Pause();
            Debug.Log("Cutscene paused on leave panel open.");
        }
        // Disable all main screen buttons
        if (mainScreenButtons != null)
        {
            foreach (Button btn in mainScreenButtons)
            {
                btn.interactable = false;
            }
        }

        // Show leave confirmation panel
        if (leavePanel != null)
        {
            leavePanel.SetActive(true);
        }
    }


    public void CloseLeavePanel()
    {
        // Hide the leave confirmation panel
        if (leavePanel != null)
        {
            leavePanel.SetActive(false);
        }

        // Resume the cutscene if it was paused
        if (cutsceneManager != null && cutsceneManager.currentCutscene != null &&
            cutsceneManager.currentCutscene.state == PlayState.Paused)
        {
            cutsceneManager.currentCutscene.Resume();
            Debug.Log("Cutscene resumed after closing save panel.");
        }
        
        // Re-enable all main screen buttons
        if (mainScreenButtons != null)
        {
            foreach (Button btn in mainScreenButtons)
            {
                btn.interactable = true;
            }
        }

        // Restore undo button
        if (undoButton != null)
        {
            undoButton.interactable = wasUndoInteractable;
        }
    }

    // User chooses to stay on current screen
    public void OnStayButtonClicked()
    {
        CloseLeavePanel();
    }

     // Cancel save operation
    public void OnCancelButtonClicked()
    {
        CloseSavePanel();
    }

     // Discard current changes and start a new scene
    public void OnDiscardAllButtonClicked()
    {
        SceneManager.LoadScene("New Scene");
    }
}
