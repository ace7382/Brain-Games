using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Doozy.Runtime.Signals;
using Doozy.Runtime.Reactor;
using Doozy.Runtime.UIManager.Components;
using UnityEngine.UI;
using BizzyBeeGames;

public class TriviaModeController : MonoBehaviour
{
    [Header("Start Menu Variables")]
    public TextMeshProUGUI                                  title;
    public TextMeshProUGUI                                  numOfQuestions;
    public TextMeshProUGUI                                  difficultyText;
    public TextMeshProUGUI                                  startTimeText;
    public TextMeshProUGUI                                  timeModsText;
    public GameObject                                       completedDot;
    public GameObject                                       parTimeDot;
    public GameObject                                       allQuestionsDot;

    [Space]
    [Space]

    public TextMeshProUGUI                                  question;
    public TextMeshProUGUI                                  answer0;
    public TextMeshProUGUI                                  answer1;
    public TextMeshProUGUI                                  answer2;
    public TextMeshProUGUI                                  answer3;
    public TextMeshProUGUI                                  questionCount;

    public Font                                             responsePopupFont;
    public CountdownClockController                         countdownClock;

    [HideInInspector] public TimedTriviaLevel               currentTriviaSet;

    private SignalReceiver                                  trivia_triviasetup_receiver;
    private SignalStream                                    trivia_triviasetup_stream;
    private SignalReceiver                                  trivia_answerchosen_receiver;
    private SignalStream                                    trivia_answerchosen_stream;
    private SignalReceiver                                  quitconfirmation_exitlevel_receiver;
    private SignalStream                                    quitconfirmation_exitlevel_stream;
    private SignalReceiver                                  quitconfirmation_backtogame_receiver;
    private SignalStream                                    quitconfirmation_backtogame_stream;
    private SignalReceiver                                  quitconfirmation_popup_receiver;
    private SignalStream                                    quitconfirmation_popup_stream;

    private int                                             currentQuestionIndex;
    private List<TimedTriviaLevel.TriviaQuestion.TriviaAnswer>     currentAnswers = new List<TimedTriviaLevel.TriviaQuestion.TriviaAnswer>();
    private int                                             questionsAnsweredCorrectly = 0;

    private bool                                            won = false;

    private void Awake()
    {
        trivia_triviasetup_stream               = SignalStream.Get("Trivia", "TriviaSetup");
        trivia_answerchosen_stream              = SignalStream.Get("Trivia", "AnswerChosen");
        quitconfirmation_exitlevel_stream       = SignalStream.Get("QuitConfirmation", "ExitLevel");
        quitconfirmation_backtogame_stream      = SignalStream.Get("QuitConfirmation", "BackToGame");
        quitconfirmation_popup_stream           = SignalStream.Get("QuitConfirmation", "Popup");

        trivia_triviasetup_receiver             = new SignalReceiver().SetOnSignalCallback(SetUp);
        trivia_answerchosen_receiver            = new SignalReceiver().SetOnSignalCallback(AnswerChosen);
        quitconfirmation_exitlevel_receiver     = new SignalReceiver().SetOnSignalCallback(EndGameEarly);
        quitconfirmation_backtogame_receiver    = new SignalReceiver().SetOnSignalCallback(Unpause);
        quitconfirmation_popup_receiver         = new SignalReceiver().SetOnSignalCallback(Pause);
    }

    private void OnEnable()
    {
        trivia_triviasetup_stream.ConnectReceiver(trivia_triviasetup_receiver);
        trivia_answerchosen_stream.ConnectReceiver(trivia_answerchosen_receiver);
        quitconfirmation_exitlevel_stream.ConnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.ConnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.ConnectReceiver(quitconfirmation_popup_receiver);
    }

    private void OnDisable()
    {
        trivia_triviasetup_stream.DisconnectReceiver(trivia_triviasetup_receiver);
        trivia_answerchosen_stream.DisconnectReceiver(trivia_answerchosen_receiver);
        quitconfirmation_exitlevel_stream.DisconnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.DisconnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.DisconnectReceiver(quitconfirmation_popup_receiver);
    }

    //Called by the TriviaModePlay screen's OnHide callback
    public void OnHide()
    {
        quitconfirmation_exitlevel_stream.DisconnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.DisconnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.DisconnectReceiver(quitconfirmation_popup_receiver);
    }

    //Called by the TriviaModePlay screen's OnShow callback
    public void OnShow()
    {
        quitconfirmation_exitlevel_stream.ConnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.ConnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.ConnectReceiver(quitconfirmation_popup_receiver);
    }

    //Called by the View - Screen TriviaPlay's Show Animation Started callback
    public void StartGame()
    {
        countdownClock.StartTimer();
    }

    public void SetUp(Signal signal)
    {
        //Signal Data should be object[2]
        //  index 0 =>  int         - 0 == new level, 1 == replay current level, 2 == play next level
        //  index 1 =>  TriviaSet   - the triviaset for the current level (only needed if 0 above)

        object[] data = signal.GetValueUnsafe<object[]>();

        if ((int)data[0] == 0) //Level was defined by call
        {
            currentTriviaSet = (TimedTriviaLevel)data[1];
        }
        else if ((int)data[0] == 2) //Play Next Level
        {
            currentTriviaSet = (TimedTriviaLevel)currentTriviaSet.nextLevel;
        }
        //data[0] == 1 => Replay doesn't need to update the TriviaSet

        title.text              = currentTriviaSet.levelName;
        difficultyText.text     = currentTriviaSet.difficulty.ToString();
        numOfQuestions.text     = currentTriviaSet.questions.Count.ToString() + " Questions";
        timeModsText.text       = string.Format("Correct +{0}s  Wrong -{1}s", currentTriviaSet.secondsGainedForCorrectAnswer
                                    , currentTriviaSet.secondsLostForWrongAnswer);

        System.TimeSpan ts      = System.TimeSpan.FromSeconds(currentTriviaSet.startTimeInSeconds);
        startTimeText.text      = ts.Minutes + ":" + ts.Seconds.ToString("00");

        completedDot.SetActive(currentTriviaSet.objective1);
        parTimeDot.SetActive(currentTriviaSet.objective2);
        allQuestionsDot.SetActive(currentTriviaSet.objective3);

        answer0.transform.parent.gameObject.GetComponent<UIButton>().interactable = true;
        answer1.transform.parent.gameObject.GetComponent<UIButton>().interactable = true;
        answer2.transform.parent.gameObject.GetComponent<UIButton>().interactable = true;
        answer3.transform.parent.gameObject.GetComponent<UIButton>().interactable = true;

        currentQuestionIndex = -1;
        questionsAnsweredCorrectly = 0;

        countdownClock.SetupTimer(currentTriviaSet.startTimeInSeconds, currentTriviaSet.parTimeRemainingInSeconds);

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
            ResponsePopup(true, selection);

            //Getting the Last Question correct won't give you more time
            if (currentQuestionIndex < currentTriviaSet.questions.Count - 1)
            {
                countdownClock.AddTime(currentTriviaSet.secondsGainedForCorrectAnswer);
                TextPopup(string.Format("+{0}s", currentTriviaSet.secondsGainedForCorrectAnswer.ToString())
                    , countdownClock.timeDisplay.transform, Vector2.zero, Color.green);
            }
        }
        else
        {
            ResponsePopup(false, selection);
            
            //Getting the last question incorrect will make you lose time though, so you can lose on the last question
            countdownClock.SubtractTime(currentTriviaSet.secondsLostForWrongAnswer);
            TextPopup(string.Format("-{0}s", currentTriviaSet.secondsLostForWrongAnswer.ToString())
                , countdownClock.timeDisplay.transform, Vector2.zero, Color.red);
        }

        if (countdownClock.SecondsRemaining <= 0f)
        {
            countdownClock.SetTime(0f);
        }
        else
        {
            LoadNextQuestion();
        }
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

            currentAnswers  = new List<TimedTriviaLevel.TriviaQuestion.TriviaAnswer>(currentTriviaSet.questions[currentQuestionIndex].Answers);

            question.text   = currentTriviaSet.questions[currentQuestionIndex].Question;

            //Shuffle Answers around
            int n = currentAnswers.Count;
            System.Random rng = new System.Random();
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                TimedTriviaLevel.TriviaQuestion.TriviaAnswer value = currentAnswers[k];
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
            won = true;
            EndGame();
        }
    }

    //Invoked by the countdownClock's onOutOfTime event
    public void OutOfTime()
    {
        won = false;
        EndGame();
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

    private void Pause(Signal signal)
    {
        Debug.Log("Pause");
        countdownClock.Pause();
    }

    private void Unpause(Signal signal)
    {
        Debug.Log("Unpause");
        countdownClock.Unpause();
    }

    private void EndGameEarly(Signal signal)
    {
        Debug.Log("Exit Game");

        won                         = false;
        questionsAnsweredCorrectly  = 0;

        countdownClock.Pause();
        countdownClock.SetTime(0f);

        EndGame();
    }

    private void EndGame()
    {
        countdownClock.Pause();

        //Disable Buttons
        answer0.transform.parent.gameObject.GetComponent<UIButton>().interactable = false;
        answer1.transform.parent.gameObject.GetComponent<UIButton>().interactable = false;
        answer2.transform.parent.gameObject.GetComponent<UIButton>().interactable = false;
        answer3.transform.parent.gameObject.GetComponent<UIButton>().interactable = false;

        if (won)
        {
            if (!currentTriviaSet.objective1)
                currentTriviaSet.objective1 = true;
            
            if (currentTriviaSet.nextLevel != null && !currentTriviaSet.nextLevel.unlocked)
                currentTriviaSet.nextLevel.unlocked = true;

            if (!currentTriviaSet.objective2 && countdownClock.SecondsRemaining >= currentTriviaSet.parTimeRemainingInSeconds)
                currentTriviaSet.objective2 = true;

            if (!currentTriviaSet.objective3 && questionsAnsweredCorrectly == currentTriviaSet.questions.Count)
                currentTriviaSet.objective3 = true;
        }

        Invoke("GoToEndScreen", 2.5f);   
    }


    //Invoked by EndGame()
    private void GoToEndScreen()
    {
        object[] data   = new object[9];
        data[0]         = won;
        data[1]         = questionsAnsweredCorrectly;
        data[2]         = currentTriviaSet.questions.Count;
        data[3]         = countdownClock.SecondsRemaining;
        data[4]         = false;
        data[5]         = currentTriviaSet.objective1;
        data[6]         = currentTriviaSet.objective2;
        data[7]         = currentTriviaSet.objective3;
        data[8]         = currentTriviaSet.parTimeRemainingInSeconds;

        if (currentTriviaSet.nextLevel != null)
            data[4] = currentTriviaSet.nextLevel.unlocked;

        Signal.Send("Trivia", "EndGame", data);
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
