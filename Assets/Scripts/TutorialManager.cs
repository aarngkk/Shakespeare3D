using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Steps (in order)")]
    public GameObject[] tutorialSteps;    // Array of UI panels for each tutorial step
    private int currentStep = 0;          // Index tracking current tutorial step
    public GameObject Script1ChoicePopUp; // Popup to show after tutorial completes

    public Button nextButton;  // Button to progress through tutorial steps
    public Button skipButton;  // Button to skip entire tutorial

    // Reference to script selection UI manager
    public ScriptSelectionManager scriptSelectionManager;

    // Static flag to track tutorial state globally
    public static bool tutorialActive;

    // Parent container for all tutorial UI elements
    public GameObject instructionsOverlay;

    // Time it takes curtains to draw
    [SerializeField] private float curtainDrawTime;
    [SerializeField] private GameObject tutorialButton;
    [SerializeField] private GameObject leftCurtains;
    [SerializeField] private GameObject rightCurtains;

    void Start()
    {
        if (leftCurtains != null && rightCurtains != null)
        {
            leftCurtains.SetActive(true);
            rightCurtains.SetActive(true);
        }

        if (PlayerPrefs.GetInt("TutorialCompleted") != 1)
        {
            tutorialActive = true;

            StartCoroutine(ShowFirstTimeTutorial());
        }
        else
        {
            StartCoroutine(ShowChoice1PopUp());
        }

    }

    public void ShowTutorial(int startStep)
    {
        currentStep = startStep;

        nextButton.onClick.RemoveAllListeners();
        skipButton.onClick.RemoveAllListeners();

        if (Script1ChoicePopUp != null)
        {
            Script1ChoicePopUp.SetActive(false);
        }

        instructionsOverlay.SetActive(true);

        // Hide all tutorial steps initially
        foreach (GameObject step in tutorialSteps)
            step.SetActive(false);

        // Activate first step if available
        if (tutorialSteps.Length > 0)
        {
            tutorialSteps[startStep].SetActive(true);
        }

        // Setup button click listeners
        nextButton.onClick.AddListener(OnNextClicked);
        skipButton.onClick.AddListener(OnSkipClicked);
    }

    // Handles progression to next tutorial step
    void OnNextClicked()
    {
        // Deactivate current step
        tutorialSteps[currentStep].SetActive(false);
        currentStep++;

        // Show next step if available
        if (currentStep < tutorialSteps.Length)
        {
            tutorialSteps[currentStep].SetActive(true);

            if (currentStep == 1)
            {
                EnableOutlines();
            }

            // Special handling for step 6 (script selection)
            if (currentStep == 6 && scriptSelectionManager != null)
            {
                scriptSelectionManager.OpenScriptSelection();
            }
            // Special handling for step 7 (script selection close)
            else if (currentStep == 7 && scriptSelectionManager != null)
            {
                scriptSelectionManager.CloseScriptSelection();
            }
        }
        else
        {
            // Complete tutorial if no steps remain
            EndTutorial();
        }

        Debug.Log("Current tutorial step: " + currentStep);
    }

    // Handles skip button click
    void OnSkipClicked()
    {
        scriptSelectionManager.CloseScriptSelection();
        EndTutorial();
    }

    // Cleans up tutorial UI and marks completion
    void EndTutorial()
    {
        // Hide all tutorial steps
        foreach (GameObject step in tutorialSteps)
            step.SetActive(false);

        // Hide main tutorial overlay
        if (instructionsOverlay != null)
        {
            instructionsOverlay.SetActive(false);
        }
        else
        {
            Debug.LogWarning("InstructionsOverlay is not assigned!");
        }

        // Update global tutorial state
        tutorialActive = false;

        // Show first choice popup
        Script1ChoicePopUp.SetActive(true);

        tutorialButton.SetActive(true);

        PlayerPrefs.SetInt("TutorialCompleted", 1);
    }

    private void EnableOutlines()
    {
        Outline bedOutline = GameObject.FindWithTag("Bed")?.GetComponent<Outline>();
        Outline dresserOutline = GameObject.FindWithTag("Dresser")?.GetComponent<Outline>();

        if (bedOutline != null) bedOutline.enabled = true;
        if (dresserOutline != null) dresserOutline.enabled = true;
    }

    IEnumerator ShowFirstTimeTutorial()
    {
        yield return new WaitForSeconds(curtainDrawTime);

        ShowTutorial(0);
    }

    IEnumerator ShowChoice1PopUp()
    {
        tutorialActive = true;

        yield return new WaitForSeconds(curtainDrawTime);

        EnableOutlines();
        Script1ChoicePopUp.SetActive(true);

        tutorialActive = false;

        tutorialButton.SetActive(true);
    }
}
