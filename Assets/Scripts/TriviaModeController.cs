using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Doozy.Runtime.Signals;
using Doozy.Runtime.Reactor;
using UnityEngine.UI;
using BizzyBeeGames;
using Doozy.Runtime.UIManager.Components;

public class TriviaModeController : MonoBehaviour
{
    public TextMeshProUGUI                                  title;
    public TextMeshProUGUI                                  subtitle;
    public TextMeshProUGUI                                  numOfQuestions;

    public TextMeshProUGUI                                  question;
    public TextMeshProUGUI                                  answer0;
    public TextMeshProUGUI                                  answer1;
    public TextMeshProUGUI                                  answer2;
    public TextMeshProUGUI                                  answer3;
    public TextMeshProUGUI                                  questionCount;

    public Font                                             responsePopupFont;

    public Progressor                                       clockProgressor;
    public TextMeshProUGUI                                  timeDisplay;

    [HideInInspector] public TriviaSet                      currentTriviaSet;

    private SignalReceiver                                  trivia_triviasetup_receiver;
    private SignalStream                                    trivia_triviasetup_stream;
    private SignalReceiver                                  trivia_answerchosen_receiver;
    private SignalStream                                    trivia_answerchosen_stream;

    private int                                             currentQuestionIndex;
    private List<TriviaSet.TriviaQuestion.TriviaAnswer>     currentAnswers = new List<TriviaSet.TriviaQuestion.TriviaAnswer>();
    private int                                             questionsAnsweredCorrectly = 0;

    private float                                           secondsRemaining;
    private float                                           clockMaxSeconds = 60f;

    private bool                                            isPlaying = false;

    private void Awake()
    {
        trivia_triviasetup_stream = SignalStream.Get("Trivia", "TriviaSetup");
        trivia_answerchosen_stream = SignalStream.Get("Trivia", "AnswerChosen");

        trivia_triviasetup_receiver = new SignalReceiver().SetOnSignalCallback(SetUp);
        trivia_answerchosen_receiver = new SignalReceiver().SetOnSignalCallback(AnswerChosen);
    }

    private void OnEnable()
    {
        trivia_triviasetup_stream.ConnectReceiver(trivia_triviasetup_receiver);
        trivia_answerchosen_stream.ConnectReceiver(trivia_answerchosen_receiver);
    }

    private void OnDisable()
    {
        trivia_triviasetup_stream.DisconnectReceiver(trivia_triviasetup_receiver);
        trivia_answerchosen_stream.DisconnectReceiver(trivia_answerchosen_receiver);
    }

    private void Update()
    {
        if (isPlaying)
        {
            secondsRemaining = Mathf.Clamp(secondsRemaining - Time.deltaTime, 0f, float.MaxValue);

            float percentFill = Mathf.Clamp((secondsRemaining / clockMaxSeconds), 0f, float.MaxValue);

            clockProgressor.SetProgressAt(percentFill);

            UpdateTimerDisplay();

            if (secondsRemaining <= 0.0f)
                EndGame();
        }
    }

    public void StartGame()
    {
        isPlaying = true;
        secondsRemaining = 63f;
    }

    public void SetUp(Signal signal)
    {
        //Signal Data should be object[2]
        //  index 0 =>  Level name
        //  index 1 =>  TriviaSet

        object[] data       = signal.GetValueUnsafe<object[]>();
        currentTriviaSet    = (TriviaSet)data[1];

        title.text          = "Trivia " + data[0].ToString();
        subtitle.text       = "\"" + currentTriviaSet.setTitle + "\"";
        numOfQuestions.text = currentTriviaSet.questions.Count.ToString() +  " Questions";

        answer0.transform.parent.gameObject.GetComponent<UIButton>().interactable = true;
        answer1.transform.parent.gameObject.GetComponent<UIButton>().interactable = true;
        answer2.transform.parent.gameObject.GetComponent<UIButton>().interactable = true;
        answer3.transform.parent.gameObject.GetComponent<UIButton>().interactable = true;

        currentQuestionIndex = -1;
        questionsAnsweredCorrectly = 0;
        LoadNextQuestion();
    }

    public void AnswerChosen(Signal signal)
    {
        //Signal Data should be int
        //  the answer number that was selected

        int selection = signal.GetValueUnsafe<int>();

        if (currentAnswers[selection].correct)
        {
            questionsAnsweredCorrectly++;
            secondsRemaining += 5;

            ResponsePopup(true, selection);
            TextPopup("+5s", timeDisplay.transform, Vector2.zero, Color.green);
        }
        else
        {
            secondsRemaining -= 10;

            ResponsePopup(false, selection);
            TextPopup("-10s", timeDisplay.transform, Vector2.zero, Color.red);
        }

        LoadNextQuestion();
    }

    public void SubmitAnswer(int answerNum)
    {
        Signal.Send("Trivia", "AnswerChosen", answerNum);
    }

    public void LoadNextQuestion()
    {
        currentQuestionIndex++;

        if (currentQuestionIndex < currentTriviaSet.questions.Count)
        {
            questionCount.text = string.Format("Question\n{0} of {1}", currentQuestionIndex + 1, currentTriviaSet.questions.Count);

            currentAnswers  = new List<TriviaSet.TriviaQuestion.TriviaAnswer>(currentTriviaSet.questions[currentQuestionIndex].Answers);

            question.text   = currentTriviaSet.questions[currentQuestionIndex].Question;

            //Shuffle Answers around
            int n = currentAnswers.Count;
            System.Random rng = new System.Random();
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                TriviaSet.TriviaQuestion.TriviaAnswer value = currentAnswers[k];
                currentAnswers[k] = currentAnswers[n];
                currentAnswers[n] = value;
            }

            answer0.text    = currentAnswers[0].answerText;
            answer1.text    = currentAnswers[1].answerText;
            answer2.text    = currentAnswers[2].answerText;
            answer3.text    = currentAnswers[3].answerText;
        }
        else
        {
            EndGame();
        }
    }

    private void UpdateTimerDisplay()
    {
        System.TimeSpan ts = System.TimeSpan.FromSeconds(secondsRemaining);
        timeDisplay.text = ts.Minutes + ":" + ts.Seconds.ToString("00");
    }

    private void ResponsePopup(bool correct, int selection)
    {
        string t;
        Vector2 center;
        Transform tran;
        Color c;

        if (correct)
        {
            t = "Correct!";
            c= Color.green;
        }
        else
        {
            t = "Wrong";
            c = Color.red;
        }

        if (selection == 0)
        {
            tran = answer0.transform.parent;
        }
        else if (selection == 1)
        {
            tran = answer1.transform.parent;
        }
        else if (selection == 2)
        {
            tran = answer2.transform.parent;
        }
        else
        {
            tran = answer3.transform.parent;
        }

        center = new Vector2(0f, 40f);

        TextPopup(t, tran, center, c);
    }

    private void EndGame()
    {
        isPlaying = false;

        //Disable Buttons
        answer0.transform.parent.gameObject.GetComponent<UIButton>().interactable = false;
        answer1.transform.parent.gameObject.GetComponent<UIButton>().interactable = false;
        answer2.transform.parent.gameObject.GetComponent<UIButton>().interactable = false;
        answer3.transform.parent.gameObject.GetComponent<UIButton>().interactable = false;

        Invoke("GoToEndScreen", 2.5f);   
    }

    private void GoToEndScreen()
    {
        Signal.Send("Trivia", "EndGame");
    }

    private void TextPopup(string word, Transform par, Vector2 center, Color col)
    {
        GameObject floatingTextObject = new GameObject("found_word_floating_text", typeof(Shadow));
        RectTransform floatingTextRectT = floatingTextObject.AddComponent<RectTransform>();
        Text floatingText = floatingTextObject.AddComponent<Text>();
        floatingText.font = responsePopupFont;
        floatingText.fontSize = 80;

        floatingText.text = word;
        floatingText.color = col;

        floatingText.transform.SetParent(par, false);
        floatingTextRectT.anchoredPosition = center;
        floatingTextRectT.localScale = Vector3.one;
        floatingTextRectT.anchorMin = new Vector2(.5f, .5f);
        floatingTextRectT.anchorMax = new Vector2(.5f, .5f);
        floatingTextRectT.pivot = new Vector2(.5f, .5f);

        ContentSizeFitter csf = floatingTextObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        UIAnimation anim;
        anim = UIAnimation.PositionY(floatingText.rectTransform, center.y, center.y + 200f, .75f);
        anim.Play();
        anim = UIAnimation.Color(floatingText, new Color(floatingText.color.r, floatingText.color.g, floatingText.color.b, 0f), 1f);
        anim.OnAnimationFinished = (GameObject obj) => { GameObject.Destroy(obj); };
        anim.Play();
    }
}
