using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

public class SavedScenesMenu : MonoBehaviour
{
    public GameObject sceneButtonPrefab; //Prefab for each filename button
    public Transform contentParent; // Parent object to hold all instantiated buttons
    private VerticalLayoutGroup layoutGroup; // Layout group used to adjust spacing
    [SerializeField] private float layoutSpacing;
    [SerializeField] private ScrollRect scrollRect;

    public GameObject sceneOptionsPanel; // Panel shown when a scene is selected
    public TMP_Text sceneNameText, playConfirmButtonText, deleteCancelButtonText; // Text showing the selected scene name
    public Button playConfirmButton, deleteCancelButton; // UI Buttons for scene actions
    private string selectedSceneName; // Name of the scene currently selected

    private List<Button> allSceneButtons = new List<Button>(); // List to keep track of all scene buttons

    void Start()
    {
        // Get the VerticalLayoutGroup component from the content container
        layoutGroup = contentParent.GetComponent<VerticalLayoutGroup>(); 

        // Populate the list of saved scenes on startup
        PopulateSavedScenes();
        
        // Ensure the scene options panel is hidden initially
        sceneOptionsPanel.SetActive(false);
    }

    void OnEnable()
    {
        // When this menu is re-shown, always hide the scene options popup
        sceneOptionsPanel.SetActive(false);
    }

    // Creates buttons for all saved scenes
    public void PopulateSavedScenes()
    {
        // Clear existing buttons
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Get saved scene names from file system
        string[] savedScenes = GetAllSavedScenes();

        // Set spacing between buttons, if layout group is present
        if (layoutGroup != null)
        {
            layoutGroup.spacing = layoutSpacing; 
        }

        allSceneButtons.Clear(); // Reset button tracking list

        // Instantiate a button for each saved scene
        foreach (string sceneName in savedScenes)
        {
            GameObject newButton = Instantiate(sceneButtonPrefab, contentParent);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = sceneName;
           
            Button buttonComponent = newButton.GetComponent<Button>();
            allSceneButtons.Add(buttonComponent);

            // Add listener to show options when button is clicked
            string sceneNameCopy = sceneName;
            newButton.GetComponent<Button>().onClick.AddListener(() => ShowSceneOptions(sceneNameCopy));
        }

        // Force layout to rebuild so new buttons are properly arranged
         LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());

        // Reset scroll to top
        scrollRect.verticalNormalizedPosition = 1f;
    }

     // Displays text pop-up for the selected scene
    public void ShowSceneOptions(string sceneName) {
        selectedSceneName = sceneName;
        sceneNameText.text = "What would you like to do with \"" + sceneName + "\"?";
        
        Debug.Log("Opening pop-up for: " + sceneName);

        playConfirmButtonText.text = "Play";
        deleteCancelButtonText.text = "Delete";

        sceneOptionsPanel.SetActive(true);
        
        // Remove previous listeners to avoid stacking
        playConfirmButton.onClick.RemoveAllListeners();
        deleteCancelButton.onClick.RemoveAllListeners();

        // Assign new listeners for this selected scene
        playConfirmButton.onClick.AddListener(() => LoadReplayScene(sceneName));
        deleteCancelButton.onClick.AddListener(() => ShowDeleteConfirmation(sceneName));
    }
    
    // Loads the scene playback UI for the selected scene
    private void LoadReplayScene(string sceneName)
    {
        Debug.Log("Loading replay scene for: " + sceneName);

        // Pass the selected log file to the scene playback system
        SceneDataTransfer.Instance.SetLogFile(sceneName);

         // Load the main replay scene
        SceneManager.LoadScene("Play All Scenes (test)"); 
    }

    // Displays confirmation to delete save file
    private void ShowDeleteConfirmation(string sceneName)
    {
        selectedSceneName = sceneName;
        sceneNameText.text = "Are you sure you want to delete \"" + sceneName + "\"?";

        // Change button text for delete confirmation menu
        playConfirmButtonText.text = "Delete";
        deleteCancelButtonText.text = "Cancel";

        // Remove previous listeners to avoid stacking
        playConfirmButton.onClick.RemoveAllListeners();
        deleteCancelButton.onClick.RemoveAllListeners();

        // Assign new listeners for this selected scene
        playConfirmButton.onClick.AddListener(() => DeleteSave(sceneName));
        deleteCancelButton.onClick.AddListener(() => CloseSceneOptions());
    }

    // Deletes a saved scene file
    private void DeleteSave(string sceneName)
    {
        string savePath = Path.Combine(Application.persistentDataPath, "SavedScenes", sceneName + ".log");

        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Deleted save file: " + sceneName);
        }
        else
        {
            Debug.LogWarning($"Save file not found : {savePath}");
        }

        // Refresh the UI list
        PopulateSavedScenes();

        // Hide the options panel if it was open
        sceneOptionsPanel.SetActive(false);
    }


    // Hides the options panel and re-enables scene buttons
    public void CloseSceneOptions()
    {
        sceneOptionsPanel.SetActive(false);
        SetSceneButtonsInteractable(true);
    }

    // Enable or disable all scene buttons
    private void SetSceneButtonsInteractable(bool interactable)
    {
        foreach (Button btn in allSceneButtons)
        {
            btn.interactable = interactable;
        }
    }

    // Reads all saved scene logs from disk and returns their names
    private string[] GetAllSavedScenes()
    {
        string savePath = Application.persistentDataPath + "/SavedScenes/";

        // Return empty array if folder doesn't exist
        if (!Directory.Exists(savePath))
            return new string[0];

        // Get all .log files from the folder
        string[] files = Directory.GetFiles(savePath, "*.log");

        // Remove file extensions to get clean scene names
        for (int i = 0; i < files.Length; i++)
        {
            files[i] = Path.GetFileNameWithoutExtension(files[i]);
        }
        
        return files;
    }
}
