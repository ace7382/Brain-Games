using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Doozy.Runtime.Signals;
using Doozy.Runtime.Reactor;
using Doozy.Runtime.UIManager.Components;
using UnityEngine.UI;
using BizzyBeeGames;

public class TimedTriviaController : MonoBehaviour
{
    //SPEED MATH is a variant of the standard TIMED TRIVIA

    public TextMeshProUGUI                                      question;
    public TextMeshProUGUI                                      answer0;
    public TextMeshProUGUI                                      answer1;
    public TextMeshProUGUI                                      answer2;
    public TextMeshProUGUI                                      answer3;
    public TextMeshProUGUI                                      questionCount;

    //public Font                                                 responsePopupFont;
    public CountdownClockController                             countdownClock;

    private TimedTriviaLevel                                     currentTimedTriviaLevel;

    private SignalReceiver                                      gamemanagement_gamesetup_receiver;
    private SignalStream                                        gamemanagement_gamesetup_stream;
    private SignalReceiver                                      trivia_answerchosen_receiver;
    private SignalStream                                        trivia_answerchosen_stream;
    private SignalReceiver                                      quitconfirmation_exitlevel_receiver;
    private SignalStream                                        quitconfirmation_exitlevel_stream;
    private SignalReceiver                                      quitconfirmation_backtogame_receiver;
    private SignalStream                                        quitconfirmation_backtogame_stream;
    private SignalReceiver                                      quitconfirmation_popup_receiver;
    private SignalStream                                        quitconfirmation_popup_stream;

    private int                                                 currentQuestionIndex;
    private List<TimedTriviaLevel.TriviaQuestion.TriviaAnswer>  currentAnswers = new List<TimedTriviaLevel.TriviaQuestion.TriviaAnswer>();
    
    private int                                                 questionsAnsweredCorrectly = 0;
    private bool                                                won = false;

    private void Awake()
    {
        gamemanagement_gamesetup_stream         = SignalStream.Get("GameManagement", "GameSetup");
        trivia_answerchosen_stream              = SignalStream.Get("Trivia", "AnswerChosen");
        quitconfirmation_exitlevel_stream       = SignalStream.Get("QuitConfirmation", "ExitLevel");
        quitconfirmation_backtogame_stream      = SignalStream.Get("QuitConfirmation", "BackToGame");
        quitconfirmation_popup_stream           = SignalStream.Get("QuitConfirmation", "Popup");

        gamemanagement_gamesetup_receiver       = new SignalReceiver().SetOnSignalCallback(SetUp);
        trivia_answerchosen_receiver            = new SignalReceiver().SetOnSignalCallback(AnswerChosen);
        quitconfirmation_exitlevel_receiver     = new SignalReceiver().SetOnSignalCallback(EndGameEarly);
        quitconfirmation_backtogame_receiver    = new SignalReceiver().SetOnSignalCallback(Unpause);
        quitconfirmation_popup_receiver         = new SignalReceiver().SetOnSignalCallback(Pause);
    }

    private void OnEnable()
    {
        gamemanagement_gamesetup_stream.ConnectReceiver(gamemanagement_gamesetup_receiver);
        trivia_answerchosen_stream.ConnectReceiver(trivia_answerchosen_receiver);
        quitconfirmation_exitlevel_stream.ConnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.ConnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.ConnectReceiver(quitconfirmation_popup_receiver);
    }

    private void OnDisable()
    {
        gamemanagement_gamesetup_stream.DisconnectReceiver(gamemanagement_gamesetup_receiver);
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

    //Called by the View - Screen TriviaPlay's Show Animation Finished callback
    public void StartGame()
    {
        countdownClock.StartTimer();
    }

    public void SetUp(Signal signal)
    {
        currentTimedTriviaLevel = (TimedTriviaLevel)GameManager.instance.currentLevel;
        
        if (answer0 != null) { answer0.transform.parent.gameObject.GetComponent<UIButton>().interactable = true; }
        if (answer1 != null) { answer1.transform.parent.gameObject.GetComponent<UIButton>().interactable = true; }
        if (answer2 != null) { answer2.transform.parent.gameObject.GetComponent<UIButton>().interactable = true; }
        if (answer3 != null) { answer3.transform.parent.gameObject.GetComponent<UIButton>().interactable = true; }
        
        Signal.Send("GameManagement", "DisableExitLevelButton", true);
        
        currentQuestionIndex = -1;
        questionsAnsweredCorrectly = 0;
        
        countdownClock.SetupTimer(currentTimedTriviaLevel.startTimeInSeconds, currentTimedTriviaLevel.parTimeRemainingInSeconds, true);
        
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
            if (currentQuestionIndex < currentTimedTriviaLevel.questions.Count - 1)
            {
                countdownClock.AddTime(currentTimedTriviaLevel.secondsGainedForCorrectAnswer);
                Helpful.TextPopup(string.Format("+{0}s", currentTimedTriviaLevel.secondsGainedForCorrectAnswer.ToString())
                    , countdownClock.timeDisplay.transform, Vector2.zero, Color.green
                    , UniversalInspectorVariables.instance.KGHappy, 80);
            }
        }
        else
        {
            ResponsePopup(false, selection);
            
            //Getting the last question incorrect will make you lose time though, so you can lose on the last question
            countdownClock.SubtractTime(currentTimedTriviaLevel.secondsLostForWrongAnswer);
            Helpful.TextPopup(string.Format("-{0}s", currentTimedTriviaLevel.secondsLostForWrongAnswer.ToString())
                , countdownClock.timeDisplay.transform, Vector2.zero, Color.red
                , UniversalInspectorVariables.instance.KGHappy, 80);
        }

        if (countdownClock.SecondsRemaining > 0)
            LoadNextQuestion();
    }

    //Caled by the answer buttons' on click event
    public void SubmitAnswer(int answerNum)
    {
        Signal.Send("Trivia", "AnswerChosen", answerNum);

        AudioManager.instance.Play("Button Click");
    }

    public void LoadNextQuestion()
    {
        currentQuestionIndex++;

        if (currentQuestionIndex < currentTimedTriviaLevel.questions.Count)
        {
            questionCount.text  = string.Format("Question\n{0} of {1}", currentQuestionIndex + 1, currentTimedTriviaLevel.questions.Count);

            currentAnswers      = new List<TimedTriviaLevel.TriviaQuestion.TriviaAnswer>(currentTimedTriviaLevel.questions[currentQuestionIndex].Answers);

            question.text       = currentTimedTriviaLevel.questions[currentQuestionIndex].Question;

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

            if (answer0 != null) {answer0.text    = currentAnswers[0].answerText; }
            if (answer1 != null) {answer1.text    = currentAnswers[1].answerText; }
            if (answer2 != null) {answer2.text    = currentAnswers[2].answerText; }
            if (answer3 != null) {answer3.text    = currentAnswers[3].answerText; }
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

        Helpful.TextPopup(t, tran, center, c, UniversalInspectorVariables.instance.KGHappy, 80);
    }

    private void Pause(Signal signal)
    {
        countdownClock.Pause();
    }

    private void Unpause(Signal signal)
    {
        countdownClock.Unpause();
    }

    private void EndGameEarly(Signal signal)
    {
        won                         = false;
        questionsAnsweredCorrectly  = 0;

        countdownClock.Pause(); //TODO: Don't think I need this call
        countdownClock.SetTime(-1f); //To make the game end at the same time the timer visually ends, we feed it a negative value
    }

    private void EndGame()
    {
        countdownClock.Pause();

        //Disable Buttons
        if (answer0 != null) { answer0.transform.parent.gameObject.GetComponent<UIButton>().interactable = false; }
        if (answer1 != null) { answer1.transform.parent.gameObject.GetComponent<UIButton>().interactable = false; }
        if (answer2 != null) { answer2.transform.parent.gameObject.GetComponent<UIButton>().interactable = false; }
        if (answer3 != null) { answer3.transform.parent.gameObject.GetComponent<UIButton>().interactable = false; }

        Signal.Send("GameManagement", "DisableExitLevelButton", false);

        if (won)
        {
            if (!currentTimedTriviaLevel.objective1)
                currentTimedTriviaLevel.objective1 = true;
            
            if (currentTimedTriviaLevel.nextLevel != null && !currentTimedTriviaLevel.nextLevel.unlocked)
                currentTimedTriviaLevel.nextLevel.unlocked = true;

            if (!currentTimedTriviaLevel.objective2 && (countdownClock.SecondsRemaining == 0 ? 0 : countdownClock.SecondsRemaining + 1) >= currentTimedTriviaLevel.parTimeRemainingInSeconds)
                currentTimedTriviaLevel.objective2 = true;

            if (!currentTimedTriviaLevel.objective3 && questionsAnsweredCorrectly == currentTimedTriviaLevel.questions.Count)
                currentTimedTriviaLevel.objective3 = true;
        }

        Invoke("GoToEndScreen", 2f);   
    }


    //Invoked by EndGame()
    private void GoToEndScreen()
    {
        LevelResultsData results    = new LevelResultsData();

        System.TimeSpan ts          = System.TimeSpan.FromSeconds(countdownClock.SecondsRemaining <= 0 ? 0 : countdownClock.SecondsRemaining + 1);
        
        results.successIndicator    = won;
        results.subtitleText        = string.Format("Time Remaining {0}:{1}\n{2}/{3} Questions Correct", ts.Minutes.ToString(),
                                      ts.Seconds.ToString("00"), questionsAnsweredCorrectly.ToString()
                                      , currentTimedTriviaLevel.questions.Count.ToString());

        GameManager.instance.SetLevelResults(results);

        Signal.Send("GameManagement", "LevelEnded", 0);
    }
}
