using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using TMPro;

public class CutsceneManager : MonoBehaviour
{
    private string SavePath => Application.persistentDataPath + "/SavedScenes/"; // Path for saving user scene logs
    public string logFileName; // Filename used when saving choice logs
    private bool isPaused = false;// Tracks if the current cutscene is paused

    // Playback control buttons
    public Button playButton;
    public Button pauseButton;

    [Header("Subtitles")]
    public GameObject subtitlesPanel;
    private bool subtitlesEnabled;
    public SubtitleManager subtitleManager;
    [System.Serializable]
    public class CutsceneSubtitleMapping
    {
        public string cutsceneType;
        public SubtitleData subtitleData;
    }

    public CutsceneSubtitleMapping[] subtitleMappings;


    // State flags for specific cutscene triggers
    private bool isCutscene1Started = false;
    public bool IsCutscene1Started() => isCutscene1Started; // Accessor for cutscene 1 state

    private bool isCutscene7Started = false; // New flag to track Cutscene7 state
    public bool IsCutscene7Started() => isCutscene7Started; // Public getter for Cutscene 7 state

    public GameObject Script1ChoicePopUp; // UI popup shown after script 1

    // Game objects used for player interaction zones
    [Header("Objects to Disable")]
    public GameObject bedZone;
    public GameObject dresserZone;
    public GameObject carpetObject;

    // Cameras used for scene transitions
    [Header("Cameras")]
    public GameObject mainCamera;
    public GameObject stageCamera;

    // Cutscene Timeline references for script 1
    [Header("Script 1")]
    public PlayableDirector bedCutscene;
    public PlayableDirector dresserCutscene;

    public GameObject choicePopupPanel; // First choice (Sad/Angry)

    public Button sadButton;
    public Button angryButton;

    // Cutscene Timeline references for script 2
    [Header("Script 2")]
    public PlayableDirector bedSadCutscene;
    public PlayableDirector bedAngryCutscene;
    public PlayableDirector dresserSadCutscene;
    public PlayableDirector dresserAngryCutscene;

    public GameObject capuletChoicePopupPanel; // Second choice (Sympathetic/Annoyed)

    public Button sympatheticButton;
    public Button annoyedButton;

    // Cutscene Timeline references for script 3
    [Header("Script 3")]
    public PlayableDirector capuletSympatheticCutscene;
    public PlayableDirector capuletAnnoyedCutscene;

    public GameObject capuletFinalChoicePopupPanel; // Third choice (Enraged/Composed)

    public Button enragedButton;
    public Button composedButton;

    // Cutscene Timeline references for script 4
    [Header("Script 4")]
    public PlayableDirector capuletEnragedCutscene;
    public PlayableDirector capuletComposedCutscene;

    [SerializeField] private GameObject julietKneelingChoicePopupPanel; // Juliet Kneeling Choice Popup
    [SerializeField] private Button kneelButton;
    [SerializeField] private Button standButton;

    // Cutscene Timeline references for script 5
    [Header("Script 5")]
    public PlayableDirector julietKneelsCapuletCalm;
    public PlayableDirector julietKneelsCapuletAngry;
    public PlayableDirector julietStandsCapuletCalm;
    public PlayableDirector julietStandsCapuletAngry;

    [SerializeField] private GameObject capuletThirdChoicePopupPanel; //Fifth choice (Calm/Angry)
    [SerializeField] private Button calmDownButton;
    [SerializeField] private Button remainAngryButton;

    // Cutscene Timeline references for script 6
    [Header("Script 6")]
    public PlayableDirector capuletCalmsDownCutscene;
    public PlayableDirector capuletRemainsAngryCutscene;

    public GameObject script7PositionPanel; // Placement prompt UI
    public GameObject script7ChoicePopupPanel; // 6th choice: Desperate/Sorrowful
    public Button desperateButton;
    public Button sorrowfulButton;

    // Cutscene Timeline references for script 7
    [Header("Script 7")]
    public PlayableDirector bedDesperateCutscene;
    public PlayableDirector bedSorrowfulCutscene;
    public PlayableDirector carpetDesperateCutscene;
    public PlayableDirector carpetSorrowfulCutscene;

    public GameObject JulietSitWalkChoicePopupPanel; // Final Juliet choice (Sit/Walk)
    public Button sitButton;
    public Button walkButton;

    [Header("Script 8")]
    public PlayableDirector julietSitCutscene;
    public PlayableDirector julietWalkCutscene;

    public PlayableDirector currentCutscene; // Reference to currently playing cutscene
    private List<string> choicesMade = new List<string>(); // Log of user choices
    private Stack<string> choiceHistory = new Stack<string>(); // Stores past choices

    public bool isReplayMode = false; // Enables replaying cutscenes from logs
    private bool isCutscene6Finished = false;
    public bool IsCutscene6Finished() => isCutscene6Finished;

    public GameObject FinishedPopUp; // End-of-scene UI popup

    public static CutsceneManager Instance { get; private set; } // Singleton access
    public string currentCutsceneType { get; private set; } // Identifier for current cutscene

    void Start()
    {
        // Set initial button visibility
        playButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(true);

        // Attach play/pause events
        playButton.onClick.AddListener(PlayCutscene);
        pauseButton.onClick.AddListener(PauseCutscene);
    }

    public void PlayCutscene(string cutsceneType, System.Action onCutsceneEnd = null)
    {
        Debug.Log($"Attempting to play cutscene: {cutsceneType}");

        // Restore animations to active state
        RestoreCharacterAnimation();
        currentCutsceneType = cutsceneType;

        // Pre-cutscene setup for Script 1 (Bed/Dresser selection)
        if (cutsceneType == "Bed" || cutsceneType == "Dresser")
        {
            isCutscene1Started = true;

            // Clear snap points and reset character positions
            DragCharacter[] dragCharacters = FindObjectsOfType<DragCharacter>();
            foreach (DragCharacter character in dragCharacters)
            {
                character.ClearSnapPoints();
            }

            // Disable the DresserZone game object
            if (dresserZone != null)
            {
                dresserZone.SetActive(false);
                Debug.Log("DresserZone GameObject disabled.");
            }

            // Hide script 1 popup if active
            if (Script1ChoicePopUp != null) Script1ChoicePopUp.SetActive(false);
        }

        // Setup for Script 7 emotion choices (Bed/Carpet with Desperate/Sorrowful)
        if (cutsceneType == "BedDesperate" || cutsceneType == "BedSorrowful" ||
            cutsceneType == "CarpetDesperate" || cutsceneType == "CarpetSorrowful")
        {

            // Clear snap points and reset character positions
            DragCharacter[] dragCharacters = FindObjectsOfType<DragCharacter>();
            foreach (DragCharacter character in dragCharacters)
            {
                character.ClearSnapPoints();
            }

            // Disable Bed and carpet zone areas
            if (bedZone != null)
            {
                bedZone.SetActive(false);
                Debug.Log("BedZone GameObject disabled.");
            }
            if (carpetObject != null)
            {
                carpetObject.SetActive(false);
                Debug.Log("CarpetZone GameObject disabled.");
            }
        }

        // Maps cutscene string to corresponding PlayableDirector
        switch (cutsceneType)
        {
            case "Bed": currentCutscene = bedCutscene; break;
            case "Dresser": currentCutscene = dresserCutscene; break;
            case "BedSad": currentCutscene = bedSadCutscene; break;
            case "BedAngry": currentCutscene = bedAngryCutscene; break;
            case "DresserSad": currentCutscene = dresserSadCutscene; break;
            case "DresserAngry": currentCutscene = dresserAngryCutscene; break;
            case "CapuletSympathetic": currentCutscene = capuletSympatheticCutscene; break;
            case "CapuletAnnoyed": currentCutscene = capuletAnnoyedCutscene; break;
            case "CapuletEnraged": currentCutscene = capuletEnragedCutscene; break;
            case "CapuletComposed": currentCutscene = capuletComposedCutscene; break;
            case "JulietKneelsCapuletCalm": currentCutscene = julietKneelsCapuletCalm; break;
            case "JulietKneelsCapuletAngry": currentCutscene = julietKneelsCapuletAngry; break;
            case "JulietStandsCapuletCalm": currentCutscene = julietStandsCapuletCalm; break;
            case "JulietStandsCapuletAngry": currentCutscene = julietStandsCapuletAngry; break;
            case "CapuletCalmsDown": currentCutscene = capuletCalmsDownCutscene; break;
            case "CapuletRemainsAngry": currentCutscene = capuletRemainsAngryCutscene; break;
            case "BedDesperate": currentCutscene = bedDesperateCutscene; break;
            case "BedSorrowful": currentCutscene = bedSorrowfulCutscene; break;
            case "CarpetDesperate": currentCutscene = carpetDesperateCutscene; break;
            case "CarpetSorrowful": currentCutscene = carpetSorrowfulCutscene; break;
            case "JulietSit": currentCutscene = julietSitCutscene; break;
            case "JulietWalk": currentCutscene = julietWalkCutscene; break;

            default:
                Debug.LogWarning("Invalid cutscene type!");
                onCutsceneEnd?.Invoke();
                return;
        }

        Debug.Log($"Playing cutscene: {currentCutscene.name}");

        // Look for matching subtitle data
        SubtitleData selectedSubtitleData = null;
        foreach (var mapping in subtitleMappings)
        {
            if (mapping.cutsceneType == cutsceneType)
            {
                selectedSubtitleData = mapping.subtitleData;
                break;
            }
        }

        // Start the subtitles if found
        if (subtitleManager != null)
        {
            if (selectedSubtitleData != null && currentCutscene != null)
            {
                subtitleManager.PlaySubtitles(selectedSubtitleData, currentCutscene);
                Debug.Log($"Subtitles started for cutscene: {cutsceneType}");
            }
            else
            {
                subtitleManager.ClearSubtitles();
                Debug.Log($"No subtitles found for cutscene: {cutsceneType}");
            }
        }

        // Track user choice and attach event handler (unless in replay mode)
        if (!isReplayMode)
        {
            choiceHistory.Push(currentCutscene.name);
            choicesMade.Add(cutsceneType);
            Autosave();
            currentCutscene.stopped += OnCutsceneFinished;
        }

        // In replay mode, attach temporary handler for callback after cutscene ends
        if (isReplayMode)
        {
            currentCutscene.stopped += (PlayableDirector director) =>
            {
                Debug.Log("Cutscene finished playing.");
                director.stopped -= OnCutsceneFinished;
                FreezeCharacterPose();
                onCutsceneEnd?.Invoke();
            };
        }

        // Start the cutscene playback
        currentCutscene.Play();

        // Enable subtitles panel if assigned
        if (subtitlesPanel != null && subtitlesEnabled)
        {
            subtitlesPanel.SetActive(true);
        }

        // Update character interactivity based on state
        UpdateAllCharactersDraggableState();
    }


    private void RestoreCharacterAnimation()
    {
        // Re-enables character animations after being previously frozen
        Animator julietAnimator = GameObject.Find("Juliet")?.GetComponent<Animator>();
        Animator ladyCapuletAnimator = GameObject.Find("Lady Capulet")?.GetComponent<Animator>();

        if (julietAnimator != null) julietAnimator.enabled = true;
        if (ladyCapuletAnimator != null) ladyCapuletAnimator.enabled = true;
    }

    private void OnCutsceneFinished(PlayableDirector director)
    {
        // Hide subtitles panel when cutscene finishes
        if (subtitlesPanel != null)
        {
            subtitlesPanel.SetActive(false);
        }

        Debug.Log("Cutscene finished playing.");
        director.stopped -= OnCutsceneFinished; // Remove listener to avoid duplicate triggers

        FreezeCharacterPose(); // Stops character animations to preserve their pose

        // Skip user interactions if in replay mode
        if (isReplayMode)
        {
            Debug.Log("Replay mode active, continuing to next cutscene.");
            return;
        }

        // Determine which popup to show based on the finished cutscene
        if (currentCutscene == bedCutscene || currentCutscene == dresserCutscene)
        {
            ShowChoicePopup(); // Sad/Angry choice
        }
        else if (currentCutscene == bedSadCutscene || currentCutscene == bedAngryCutscene ||
                 currentCutscene == dresserSadCutscene || currentCutscene == dresserAngryCutscene)
        {
            ShowCapuletChoicePopup();  // Sympathetic/Annoyed
        }
        else if (currentCutscene == capuletSympatheticCutscene || currentCutscene == capuletAnnoyedCutscene)
        {
            ShowCapuletFinalChoicePopup(); // Enraged/Composed
        }
        else if (currentCutscene == capuletEnragedCutscene || currentCutscene == capuletComposedCutscene)
        {
            ShowJulietKneelingChoicePopup(); // Juliet Kneel/Stand
        }
        else if (currentCutscene == julietKneelsCapuletCalm || currentCutscene == julietKneelsCapuletAngry ||
                 currentCutscene == julietStandsCapuletCalm || currentCutscene == julietStandsCapuletAngry)
        {
            ShowCapuletThirdChoicePopup(); // Capulet Calm/Remain Angry
        }
        else if (currentCutscene == capuletCalmsDownCutscene || currentCutscene == capuletRemainsAngryCutscene)
        {
            // Prepare for Script 7 interaction after Cutscene 6
            isCutscene6Finished = true;
            Debug.Log("Cutscene 6 finished. Ready for script 7 choices.");

            if (script7PositionPanel != null) script7PositionPanel.SetActive(true);

            if (carpetObject != null)
            {
                carpetObject.SetActive(true);
                Debug.Log("Carpet GameObject enabled.");
            }

            if (mainCamera != null) mainCamera.SetActive(true);
            if (stageCamera != null) stageCamera.SetActive(false);

            // Enable the bed outline immediately after Cutscene6 finishes
            Outline bedOutline = GameObject.FindWithTag("Bed")?.GetComponent<Outline>();
            if (bedOutline != null)
            {
                bedOutline.enabled = true;
                bedOutline.OutlineColor = Color.white; // Set the outline color to white
                Debug.Log("Bed outline enabled after Cutscene6.");
            }

            UpdateAllCharactersDraggableState();
        }
        else if (currentCutscene == bedDesperateCutscene || currentCutscene == bedSorrowfulCutscene ||
                 currentCutscene == carpetDesperateCutscene || currentCutscene == carpetSorrowfulCutscene)
        {
            ShowScript8ChoicePopup(); // Sit/Walk popup for Juliet
        }
        else if (currentCutscene == julietSitCutscene || currentCutscene == julietWalkCutscene)
        {
            ShowFinishPopUp(); // Final completion popup
        }
    }

    // Disables character animations to preserve their final pose
    private void FreezeCharacterPose()
    {
        Animator julietAnimator = GameObject.Find("Juliet")?.GetComponent<Animator>();
        Animator ladyCapuletAnimator = GameObject.Find("Lady Capulet")?.GetComponent<Animator>();

        if (julietAnimator != null) julietAnimator.enabled = false;
        if (ladyCapuletAnimator != null) ladyCapuletAnimator.enabled = false;
    }

    // Show script 2 choice popup (Bed/Dresser variations)
    private void ShowChoicePopup()
    {
        if (choicePopupPanel != null)
        {
            choicePopupPanel.SetActive(true);

            sadButton.onClick.RemoveAllListeners();
            angryButton.onClick.RemoveAllListeners();

            if (currentCutscene == dresserCutscene)
            {
                // If previous cutscene was dresser, play dresser variations
                sadButton.onClick.AddListener(() => PlayNextCutscene("DresserSad"));
                angryButton.onClick.AddListener(() => PlayNextCutscene("DresserAngry"));
            }
            else
            {
                // Otherwise, assume it was the bed cutscene
                sadButton.onClick.AddListener(() => PlayNextCutscene("BedSad"));
                angryButton.onClick.AddListener(() => PlayNextCutscene("BedAngry"));
            }
        }
        else
        {
            Debug.LogError("Choice popup panel is not assigned!");
        }
    }

    // Show script 3 choice popup
    private void ShowCapuletChoicePopup()
    {
        if (capuletChoicePopupPanel != null)
        {
            capuletChoicePopupPanel.SetActive(true);

            sympatheticButton.onClick.RemoveAllListeners();
            annoyedButton.onClick.RemoveAllListeners();

            sympatheticButton.onClick.AddListener(() => PlayNextCutscene("CapuletSympathetic"));
            annoyedButton.onClick.AddListener(() => PlayNextCutscene("CapuletAnnoyed"));
        }
        else
        {
            Debug.LogError("Capulet choice popup panel is not assigned!");
        }
    }

    // Show script 4 choice popup
    private void ShowCapuletFinalChoicePopup()
    {
        if (capuletFinalChoicePopupPanel != null)
        {
            capuletFinalChoicePopupPanel.SetActive(true);

            enragedButton.onClick.RemoveAllListeners();
            composedButton.onClick.RemoveAllListeners();

            enragedButton.onClick.AddListener(() => PlayNextCutscene("CapuletEnraged"));
            composedButton.onClick.AddListener(() => PlayNextCutscene("CapuletComposed"));
        }
        else
        {
            Debug.LogError("Capulet final choice popup panel is not assigned!");
        }
    }

    // Show script 5 choice popup
    private void ShowJulietKneelingChoicePopup()
    {
        if (julietKneelingChoicePopupPanel != null)
        {
            julietKneelingChoicePopupPanel.SetActive(true);

            kneelButton.onClick.RemoveAllListeners();
            standButton.onClick.RemoveAllListeners();

            // Determine the previous Capulet choice
            bool capuletWasComposed = choicesMade.Contains("CapuletComposed");

            kneelButton.onClick.AddListener(() =>
            {
                string nextCutscene = capuletWasComposed ? "JulietKneelsCapuletCalm" : "JulietKneelsCapuletAngry";
                PlayNextCutscene(nextCutscene);
            });

            standButton.onClick.AddListener(() =>
            {
                string nextCutscene = capuletWasComposed ? "JulietStandsCapuletCalm" : "JulietStandsCapuletAngry";
                PlayNextCutscene(nextCutscene);
            });
        }
        else
        {
            Debug.LogError("Juliet kneeling choice popup panel is not assigned!");
        }
    }

    // Show script 6 choice popup
    private void ShowCapuletThirdChoicePopup()
    {
        if (capuletThirdChoicePopupPanel != null)
        {
            capuletThirdChoicePopupPanel.SetActive(true);

            calmDownButton.onClick.RemoveAllListeners();
            remainAngryButton.onClick.RemoveAllListeners();

            calmDownButton.onClick.AddListener(() => PlayNextCutscene("CapuletCalmsDown"));
            remainAngryButton.onClick.AddListener(() => PlayNextCutscene("CapuletRemainsAngry"));
        }
        else
        {
            Debug.LogError("Capulet third choice popup panel is not assigned!");
        }
    }

    // Show script 7 choice popup
    public void ShowScript7ChoicePopup(string zone)
    {
        if (script7PositionPanel != null) script7PositionPanel.SetActive(false);
        if (script7ChoicePopupPanel != null)
        {
            script7ChoicePopupPanel.SetActive(true);

            desperateButton.onClick.RemoveAllListeners();
            sorrowfulButton.onClick.RemoveAllListeners();

            desperateButton.onClick.AddListener(() => PlayScript7Cutscene(zone, "Desperate"));
            sorrowfulButton.onClick.AddListener(() => PlayScript7Cutscene(zone, "Sorrowful"));
        }
        else
        {
            Debug.LogError("Script 7 choice popup panel is not assigned!");
        }
    }

    // Show script 8 choice popup
    private void ShowScript8ChoicePopup()
    {
        if (JulietSitWalkChoicePopupPanel != null)
        {
            JulietSitWalkChoicePopupPanel.SetActive(true);

            sitButton.onClick.RemoveAllListeners();
            walkButton.onClick.RemoveAllListeners();

            sitButton.onClick.AddListener(() => PlayNextCutscene("JulietSit"));
            walkButton.onClick.AddListener(() => PlayNextCutscene("JulietWalk"));
        }
        else
        {
            Debug.LogError("Script 8 choice popup panel is not assigned!");
        }
    }

    // Show whole scene completed popup
    private void ShowFinishPopUp()
    {
        if (FinishedPopUp != null)
        {
            FinishedPopUp.SetActive(true);
        }
        else
        {
            Debug.LogError("Finished Pop Up Not Assigned!!");
        }
    }

    // Play next cutscene and hide all popups
    private void PlayNextCutscene(string nextCutsceneType)
    {
        choicePopupPanel?.SetActive(false);
        capuletChoicePopupPanel?.SetActive(false);
        capuletFinalChoicePopupPanel?.SetActive(false);
        julietKneelingChoicePopupPanel?.SetActive(false);
        capuletThirdChoicePopupPanel?.SetActive(false);
        JulietSitWalkChoicePopupPanel?.SetActive(false);

        PlayCutscene(nextCutsceneType);
    }

    // Save player choices to log file
    public void LogChoicesToFile()
    {
        if (choicesMade.Count == 0)
        {
            Debug.LogWarning("No choices have been made yet. Nothing to save.");
            return;
        }
        if (string.IsNullOrWhiteSpace(logFileName))
        {
            logFileName = "default_log"; // Fallback filename
        }
        try
        {
            string filePath = SavePath + logFileName + ".log";
            File.WriteAllLines(filePath, choicesMade); // Save choices as lines in a file
            Debug.Log($"Choices logged to: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to write log file: {e.Message}");
        }
    }

    // Undo the last player choice
    public void UndoChoice()
    {
        if (choiceHistory.Count < 2) // Ensure there is a previous choice to revert to
        {
            Debug.LogWarning("No previous choice to undo!");
            return;
        }

        if (currentCutscene != null && currentCutscene.state == PlayState.Playing)
        {
            currentCutscene.stopped -= OnCutsceneFinished; // Removes the listener
            currentCutscene.Stop();
            Debug.Log("Current cutscene stopped.");
        }

        choiceHistory.Pop(); // Remove the latest choice
        string previousChoice = choiceHistory.Peek(); // Get the choice before it
        choicesMade.RemoveAt(choicesMade.Count - 1);
        Debug.Log($"Undoing choice, returning to: {previousChoice}");

        // Reset the state of isCutscene6Finished and isCutscene7Started
        isCutscene6Finished = false;
        isCutscene7Started = false; // Reset the Cutscene7 flag
        Debug.Log("Cutscene7 state reset. Bed outline and snap points re-enabled.");

        // Re-enable the Carpet GameObject
        if (carpetObject != null)
        {
            carpetObject.SetActive(true);
            Debug.Log("Carpet GameObject re-enabled.");
        }

        // Re-enable the BedZone GameObject
        if (bedZone != null)
        {
            bedZone.SetActive(true);
            Debug.Log("BedZone GameObject re-enabled.");
        }

        // Get outline references
        Outline bedOutline = GameObject.FindWithTag("Bed")?.GetComponent<Outline>();
        Outline carpetOutline = GameObject.FindWithTag("CarpetZone")?.GetComponent<Outline>();
        Outline dresserOutline = GameObject.FindWithTag("Dresser")?.GetComponent<Outline>();

        // Only enable outlines if we're returning to a state where they should be visible
        bool shouldShowOutlines = !IsCutscene1Started(); // Only show at game start or after cutscene 6

        if (bedOutline != null)
        {
            bedOutline.enabled = shouldShowOutlines;
            bedOutline.OutlineColor = shouldShowOutlines ? Color.white : Color.clear;
            Debug.Log($"Bed outline {(shouldShowOutlines ? "enabled" : "disabled")}");
        }

        if (carpetOutline != null)
        {
            carpetOutline.enabled = shouldShowOutlines;
            carpetOutline.OutlineColor = shouldShowOutlines ? Color.white : Color.clear;
            Debug.Log($"Carpet outline {(shouldShowOutlines ? "enabled" : "disabled")}");
        }

        if (dresserOutline != null)
        {
            dresserOutline.enabled = shouldShowOutlines;
            dresserOutline.OutlineColor = shouldShowOutlines ? Color.white : Color.clear;
            Debug.Log($"Dresser outline {(shouldShowOutlines ? "enabled" : "disabled")}");
        }

        // Reset the characters' positions to their original positions
        DragCharacter[] dragCharacters = FindObjectsOfType<DragCharacter>();
        foreach (DragCharacter character in dragCharacters)
        {
            character.ResetPosition();
        }

        // Play the previous cutscene
        PlayableDirector previousDirector = GameObject.Find(previousChoice)?.GetComponent<PlayableDirector>();
        if (previousDirector != null)
        {
            currentCutscene = previousDirector;
            currentCutscene.stopped += OnCutsceneFinished; // Reattach listener
            OnCutsceneFinished(currentCutscene); // Call the existing logic to show the choice panel
        }
        else
        {
            Debug.LogError("Could not find PlayableDirector for previous choice: " + previousChoice);
        }

        UpdateAllCharactersDraggableState();
    }

    // Skip current cutscene
    public void SkipCutscene()
    {
        if (currentCutscene == null || currentCutscene.state != PlayState.Playing)
        {
            Debug.LogWarning("No cutscene is currently playing to skip!");
            return;
        }

        Debug.Log($"Skipping cutscene: {currentCutscene.name}");
        currentCutscene.stopped -= OnCutsceneFinished;
        currentCutscene.Stop();
        OnCutsceneFinished(currentCutscene);
    }

    // Play script 7 cutscene based on zone and emotion
    private void PlayScript7Cutscene(string zone, string emotion)
    {
        // Hide the Script7ChoicePopup panel
        script7ChoicePopupPanel.SetActive(false);

        // Disable the carpet GameObject
        if (carpetObject != null)
        {
            carpetObject.SetActive(false);
            Debug.Log("Carpet GameObject disabled.");
        }

        // Disable the BedZone GameObject
        if (bedZone != null)
        {
            bedZone.SetActive(false);
            Debug.Log("BedZone GameObject disabled.");
        }

        // Disable the Bed outline component
        Outline bedOutline = GameObject.FindWithTag("Bed")?.GetComponent<Outline>();
        if (bedOutline != null)
        {
            bedOutline.enabled = false;
            Debug.Log("Bed outline disabled.");
        }

        // Set the flag to indicate that Cutscene7 has started
        isCutscene7Started = true;
        Debug.Log("Cutscene7 started. Bed outline and snap points disabled.");

        // Play the selected cutscene
        string cutsceneType = $"{zone}{emotion}";
        PlayCutscene(cutsceneType);
    }

    // Hide script 7 popup
    public void HideScript7ChoicePopup()
    {
        if (script7ChoicePopupPanel != null)
        {
            script7ChoicePopupPanel.SetActive(false);
        }
    }

    // Hide finished popup
    public void HideFinishPopup()
    {
        if (FinishedPopUp != null)
        {
            FinishedPopUp.SetActive(false);
            Debug.Log("Finished popup hidden from SceneSaverUI.");
        }
    }

    // Singleton setup
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        ApplySavedSubtitleSetting();
    }

    // Play currently paused cutscene
    public void PlayCutscene()
    {
        if (currentCutscene != null && isPaused)
        {
            currentCutscene.Play();
            isPaused = false;
            UpdateButtonVisibility();
        }
    }

    // Pause current cutscene
    public void PauseCutscene()
    {
        if (currentCutscene != null && !isPaused)
        {
            currentCutscene.Pause();
            isPaused = true;
            UpdateButtonVisibility();
        }
    }

    // Update play/pause button visibility
    private void UpdateButtonVisibility()
    {
        playButton.gameObject.SetActive(isPaused);
        pauseButton.gameObject.SetActive(!isPaused);
    }

    // Update draggable state of all characters
    private void UpdateAllCharactersDraggableState()
    {
        if (CharacterManager.Instance != null)
            CharacterManager.Instance.UpdateDraggableState();
    }

    // Resume cutscene and update buttons
    public void ResumeAndUpdateButtons()
    {
        isPaused = false;
        UpdateButtonVisibility();
    }

    private void ApplySavedSubtitleSetting()
    {
        subtitlesEnabled = PlayerPrefs.GetInt("SubtitlesEnabled", 1) == 1;

        Debug.Log("Subtitles setting loaded: " + subtitlesEnabled);
    }

    // Autosave everytime a user choice is made
    private void Autosave()
    {
        if (choicesMade.Count == 0)
        {
            Debug.LogWarning("No choices to autosave.");
            return;
        }

        string autosaveFilePath = SavePath + "Autosave.log";
        try
        {
            File.WriteAllLines(autosaveFilePath, choicesMade);
            Debug.Log($"Autosaved to: {autosaveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to write autosave file: {e.Message}");
        }
    }
}
