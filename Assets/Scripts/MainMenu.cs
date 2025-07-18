using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject quizMenu;
    [SerializeField] private GameObject quizButton;
    [SerializeField] private GameObject quizButtonGreyPanel;
    [SerializeField] private Color quizButtonGreyedOut;
    [SerializeField] private Color quizButtonTextGreyedOut;
    [SerializeField] private GameObject quizUnlockText;
    [SerializeField] private float quizUnlockTextDuration;

    private Coroutine quizUnlockCoroutine;

    private void Start()
    {
        Button button = quizButton.GetComponent<Button>();
        button.onClick.AddListener(ShowQuizUnlockText);

        if (PlayerPrefs.GetInt("QuizButtonEnabled") == 1)
        {
            
            quizButton.GetComponent<Image>().color = quizButtonGreyedOut;
            quizButton.GetComponentInChildren<TMP_Text>().color = quizButtonTextGreyedOut;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(ShowQuizMenu);
        }
    }

    private void OnEnable()
    {
        quizUnlockText.SetActive(false);
    }

    private void Awake()
    {
        if (PlayerPrefs.HasKey("GraphicsQuality"))
        {
            int savedQuality = PlayerPrefs.GetInt("GraphicsQuality");
            QualitySettings.SetQualityLevel(savedQuality, true);
            Debug.Log("Loaded saved graphics quality: " + QualitySettings.names[savedQuality]);
        }
    }

    // Loads the "Main Menu"
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    // Loads the "New Scene" menu
    public void CreateNewScene()
    {
        SceneManager.LoadScene("New Scene");
    }

    // Loads the "Quiz Scene" menu
    public void LoadQuizScene()
    {
        SceneManager.LoadScene("Quiz Scene");
    }

    // Handles quitting the application
    public void QuitApp()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }

    private void ShowQuizMenu()
    {
        this.gameObject.SetActive(false);
        quizMenu.SetActive(true);
    }

    public void ShowQuizUnlockText()
    {
        // Stop any running coroutine before starting a new one
        if (quizUnlockCoroutine != null)
        {
            StopCoroutine(quizUnlockCoroutine);
        }

        quizUnlockCoroutine = StartCoroutine(WaitForQuizUnlockText());
    }

    IEnumerator WaitForQuizUnlockText()
    {
        quizUnlockText.SetActive(true);

        yield return new WaitForSeconds(quizUnlockTextDuration);

        quizUnlockText.SetActive(false);
        quizUnlockCoroutine = null;
    }
}
