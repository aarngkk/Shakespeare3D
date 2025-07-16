using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    public List<QuizQuestion> questionsAndAnswers;
    public GameObject[] options;
    public int currentQuestionIndex;
    public TMP_Text questionText;
    public GameObject quizMenu;
    public GameObject gameOverMenu;
    public TMP_Text scoreText;
    public TMP_Text questionLabel;
    private int questionNumbering = 1;
    private int totalQuestions = 0;
    private int score = 0;
    [SerializeField] private float waitTime = 1f;

    private void Start()
    {
        totalQuestions = questionsAndAnswers.Count;
        quizMenu.SetActive(true);
        gameOverMenu.SetActive(false);
        GenerateQuestion();
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GameOver()
    {
        quizMenu.SetActive(false);
        gameOverMenu.SetActive(true);
        scoreText.text = "You got " + score + "/" + totalQuestions + " questions right!";
    }

    public void Correct()
    {
        score++;
        questionsAndAnswers.RemoveAt(currentQuestionIndex);
        StartCoroutine(WaitForNext());
    }

    public void Wrong()
    {
        questionsAndAnswers.RemoveAt(currentQuestionIndex);
        StartCoroutine(WaitForNext());
    }

    private void SetAnswer()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponent<Image>().color = options[i].GetComponent<AnswerScript>().startColor;

            var text = options[i].GetComponentInChildren<TMP_Text>();
            text.color = options[i].GetComponent<AnswerScript>().startTextColor;

            options[i].GetComponent<AnswerScript>().isCorrect = false;
            options[i].transform.GetChild(0).GetComponent<TMP_Text>().text = questionsAndAnswers[currentQuestionIndex].answers[i];

            if (questionsAndAnswers[currentQuestionIndex].correctAnswer == i+1)
            {
                options[i].GetComponent<AnswerScript>().isCorrect = true;
            }
        }
    }

    private void GenerateQuestion()
    {
        if (questionsAndAnswers.Count > 0)
        {
            currentQuestionIndex = Random.Range(0, questionsAndAnswers.Count);

            questionText.text = questionsAndAnswers[currentQuestionIndex].question;
            questionLabel.text = "Question " + questionNumbering;
            questionNumbering++;

            SetAnswer();
        }
        else
        {
            Debug.Log("Out of Questions.");
            GameOver();
        }
    }

    IEnumerator WaitForNext()
    {
        SetOptionsInteractable(false);

        yield return new WaitForSeconds(waitTime);
        GenerateQuestion();

        SetOptionsInteractable(true);
    }

    public void backToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void SetOptionsInteractable(bool state)
    {
        foreach (var option in options)
        {
            var button = option.GetComponent<Button>();
            if (button) button.interactable = state;
        }
    }
}
