using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Trivia Question Set", menuName = "New Trivia Question Set", order = 52)]
public class TriviaSet : ScriptableObject
{
    [System.Serializable]
    public class TriviaQuestion
    {
        [System.Serializable]
        public class TriviaAnswer
        {
            public string  answerText;
            public bool    correct;

            public override string ToString()
            {
                return answerText + " || Correct? " + correct.ToString();
            }
        }

        public string Question;
        public List<TriviaAnswer> Answers;

        public override string ToString()
        {
            string ret = string.Empty;

            foreach (TriviaAnswer a in Answers)
            {
                ret += a.ToString() + "\n";
            }

            return ret;
        }
    }

    public string setTitle;
    public List<TriviaQuestion> questions;
}
