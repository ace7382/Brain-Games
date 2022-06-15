using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriviaAnswerButtonController : MonoBehaviour
{
    [SerializeField] private int answerNum;

    //Caled by the answer buttons' on click event
    public void SubmitAnswer()
    {
        Signal.Send("Trivia", "AnswerChosen", answerNum);

        AudioManager.instance.Play("Button Click");
    }
}
