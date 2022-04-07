using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Trivia Question Set", menuName = "New Trivia Question Set", order = 52)]
public class TriviaSet : ScriptableObject
{
    public enum DifficultyLevel
    {
        VeryEasy = 5,
        Easy = 10,
        Medium = 15,
        Hard = 20,
        VeryHard = 25,
        Insane = 30        
    };

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

    public string setTitle;
    public DifficultyLevel difficulty;

    public int startTimeInSeconds;
    public int secondsLostForWrongAnswer;
    public int secondsGainedForCorrectAnswer;
    public int parTimeRemainingInSeconds;

    public List<TriviaQuestion> questions;

    public bool unlocked;
    public TriviaSet nextTriviaSet;

    [Header("Level Goals")]
    public bool completed;
    public bool allQuestionsCorrect;
    public bool underParTime;
}
