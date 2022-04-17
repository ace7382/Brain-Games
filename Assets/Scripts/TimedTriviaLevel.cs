using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Trivia Question Set", menuName = "New Trivia Question Set", order = 52)]
public class TimedTriviaLevel : LevelBase
{
    //Objective 1 - completed
    //Objective 2 - under par time
    //Objective 3 - all questions correct

    [System.Serializable]
    public class TriviaQuestion
    {
        [System.Serializable]
        public class TriviaAnswer
        {
            public string  answerText;
            public bool    correct;
        }

        public string Question;
        public List<TriviaAnswer> Answers;
    }

    public int startTimeInSeconds;
    public int secondsLostForWrongAnswer;
    public int secondsGainedForCorrectAnswer;
    public int parTimeRemainingInSeconds;

    public List<TriviaQuestion> questions;
}
