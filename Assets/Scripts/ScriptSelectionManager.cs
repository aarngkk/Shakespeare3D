using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ScriptSelectionManager : MonoBehaviour
{
    // UI Elements assigned via the Inspector:
    public GameObject scriptPanel;                 
    public TextMeshProUGUI scriptDetailText;         
    public Button nextButton;                        
    public Button prevButton;                        
    public Button backButton;                   
    public List<Button> otherButtons;           
    public ScrollRect scrollRect;            
    public Button saveButton;
    public Button undoButton;
    public Button redoButton;
    public Button scriptButton;
    public GameObject scriptChoicePopUp;

    public TMP_Text buttonText;
    public TMP_Text titleText;

    // Script management variables
    private List<string> scripts = new List<string>();
    private int currentIndex = 0;
    private float scrollSens = 3f;
    private bool scriptPopUpReopen = false;
    public string SelectedScript { get; private set; }
    public static bool IsPanelOpen { get; private set; }

    private void Start()
    {
        LoadScripts();
        UpdateScriptDetail();
        backButton.onClick.AddListener(CloseScriptSelection);

        if (scrollRect != null)
        {
            scrollRect.scrollSensitivity = scrollSens; 
        }

        // Initialize button states
        if (saveButton != null) saveButton.interactable = false;
        if (undoButton != null) undoButton.interactable = false;
        if (redoButton != null) redoButton.interactable = false;
        if (scriptButton != null) scriptButton.interactable = false;
    }

    // Loads script details from text files in the Resources folder
    private void LoadScripts()
    {
        for (int i = 1; i <= 8; i++)
        {
            string fileName = "ScriptDetail" + i;
            TextAsset textAsset = Resources.Load<TextAsset>(fileName);
            if (textAsset != null) scripts.Add(textAsset.text);
            else Debug.LogWarning("Could not load " + fileName + ".txt from Resources!");
        }
        Debug.Log("Loaded " + scripts.Count + " script details.");
    }

   
    // Updates the text element with the current script detail and resets the scroll position
    private void UpdateScriptDetail()
    {
        if (scriptDetailText == null)
        {
            Debug.LogError("scriptDetailText is not assigned in the Inspector!");
            return;
        }
        
        if (scripts.Count > 0)
        {
            scriptDetailText.text = scripts[currentIndex];
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }
        else
        {
            scriptDetailText.text = "No script details loaded.";
        }
    }


    private void Update()
    {
        // Only update if CutsceneManager is available and has a current cutscene type
        if (CutsceneManager.Instance == null || CutsceneManager.Instance.currentCutsceneType == null)
            return;

        string type = CutsceneManager.Instance.currentCutsceneType;

        // Map cutscene types to script indices
        switch (type)
        {
            case "Bed":
            case "Dresser":
                currentIndex = 0; // Script 1 (index 0)
                break;
            case "BedSad":
            case "DresserSad":
            case "BedAngry":
            case "DresserAngry":
                currentIndex = 1; // Script 2
                break;
            case "CapuletSympathetic":
            case "CapuletAnnoyed":
                currentIndex = 2; // Script 3
                break;
            case "CapuletEnraged":
            case "CapuletComposed":
                currentIndex = 3; // Script 4
                break;
            case "JulietKneelsCapuletCalm":
            case "JulietKneelsCapuletAngry":
            case "JulietStandsCapuletCalm":
            case "JulietStandsCapuletAngry":
                currentIndex = 4; // Script 5
                break;
            case "CapuletCalmsDown":
            case "CapuletRemainsAngry":
                currentIndex = 5; // Script 6
                break;
            case "BedDesperate":
            case "BedSorrowful":
            case "CarpetDesperate":
            case "CarpetSorrowful":
                currentIndex = 6; // Script 7
                break;   
            case "JulietSit":
            case "JulietWalk":
                currentIndex = 7; // Script 8
                break;            
            default:
                return;
        }
        UpdateTitle(currentIndex);
        UpdateScriptButtonLabel(currentIndex);
        if (currentIndex >= 0) {
            if (saveButton != null)
            {
                saveButton.interactable = true;
            }
            if (redoButton != null) 
            {
                redoButton.interactable = true;
            }
            if (scriptButton != null) 
            {
                scriptButton.interactable = true;
            }
        }
        if (currentIndex > 0) {
            if (undoButton != null) 
            {
                undoButton.interactable = true;
            }
        }
    }

    // Updates the script button label with current script number
    private void UpdateScriptButtonLabel(int currentIndex)
    {
        int scriptNumber = currentIndex + 1;
        buttonText.text = "Script " + scriptNumber;
    }

    // Updates the title text based on current script index
    private void UpdateTitle(int currentIndex)
    {
        switch(currentIndex)
        {
            case 0: titleText.text = "Lines 103-115";break;
            case 1: titleText.text = "Lines 116-125";break;
            case 2: titleText.text = "Lines 126-145";break;
            case 3: titleText.text = "Lines 146-157b";break;
            case 4: titleText.text = "Lines 158-175b";break;
            case 5: titleText.text = "Lines 176-196";break;
            case 6: titleText.text = "Lines 197-226";break;
            case 7: titleText.text = "Lines 227-243";break;
            default: return;            
        }
    }

    // Opens the pop-up panel and disables all other buttons
    public void OpenScriptSelection()
    {
        if (scriptChoicePopUp.activeSelf) scriptPopUpReopen = true;
        scriptChoicePopUp.SetActive(false);
        UpdateScriptDetail();
        scriptPanel.SetActive(true);
        IsPanelOpen = true;
        foreach (Button btn in otherButtons) btn.interactable = false;
    }

    // Closes the pop-up panel, re-enables other buttons, and updates the title display
    public void CloseScriptSelection()
    {
        scriptPanel.SetActive(false);
        IsPanelOpen = false; // Mark the panel as closed

        if (scriptPopUpReopen)
        {
            scriptChoicePopUp.SetActive(true);
            scriptPopUpReopen = false;
        }

        foreach (Button btn in otherButtons) btn.interactable = true;
        if (saveButton != null) saveButton.interactable = false;
        if (undoButton != null) undoButton.interactable = false;
        if (redoButton != null) redoButton.interactable = false;
        if (scriptButton != null) scriptButton.interactable = false;
    }

}
