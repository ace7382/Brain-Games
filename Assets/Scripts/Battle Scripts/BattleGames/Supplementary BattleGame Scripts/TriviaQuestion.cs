using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TriviaQuestion
{
    [System.Serializable]
    public class TriviaAnswer
    {
        public string           answerText;
        public bool             correct;
    }

    public string               Question;
    public List<TriviaAnswer>   Answers;
}
