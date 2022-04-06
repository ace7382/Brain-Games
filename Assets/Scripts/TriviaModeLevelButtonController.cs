using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Signals;

public class TriviaModeLevelButtonController : MonoBehaviour
{
    public bool locked;
    public TriviaSet triviaSet;

    public void Click(string levelName)
    {
        if (locked)
            return;

        object[] data = new object[2];

        data[0] = levelName;
        data[1] = triviaSet;

        Signal.Send("Trivia", "TriviaSetup", data);
    }
}
