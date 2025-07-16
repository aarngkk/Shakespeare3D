using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerScript : MonoBehaviour
{
    public bool isCorrect = false;
    public QuizManager quizManager;
    public Color startColor;
    public Color startTextColor;
    public Color greyedOutColor;
    public Color greyedOutTextColor;

    private void Start()
    {
        startColor = GetComponent<Image>().color;

        var text = GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            startTextColor = text.color;
        }
    }

    public void Answer()
    {
        foreach (GameObject option in quizManager.options)
        {
            var answerScript = option.GetComponent<AnswerScript>();
            var image = option.GetComponent<Image>();
            var text = option.GetComponentInChildren<TMP_Text>();

            if (answerScript == null || image == null) continue;

            if (answerScript.isCorrect)
            {
                // Correct answer is green
                image.color = Color.green;
            }
            else if (option == this.gameObject && !isCorrect)
            {
                // Selected wrong answer is red
                image.color = Color.red;
            }
            else
            {
                // All other options greyed out
                image.color = greyedOutColor;
                text.color = greyedOutTextColor;
            }
        }

        if (isCorrect)
        {
            Debug.Log("Correct Answer");
            quizManager.Correct();
        }
        else
        {
            Debug.Log("Wrong Answer");
            quizManager.Wrong();
        }
    }
}
