using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleGame_Trivia : BattleGameControllerBase
{
    #region Inspector Variables

    [SerializeField] private TextMeshProUGUI        question;
    [SerializeField] private TextMeshProUGUI        answer0;
    [SerializeField] private TextMeshProUGUI        answer1;
    [SerializeField] private TextMeshProUGUI        answer2;
    [SerializeField] private TextMeshProUGUI        answer3;

    #endregion

    #region Private Variables

    private int                                     currentQuestionIndex;
    private List<TriviaQuestion.TriviaAnswer>       currentAnswers;

    #endregion

    #region Signal Variables

    private SignalReceiver                          trivia_answerchosen_receiver;
    private SignalStream                            trivia_answerchosen_stream;

    #endregion

    #region Unity Functions

    protected override void Awake()
    {
        trivia_answerchosen_stream      = SignalStream.Get("Trivia", "AnswerChosen");

        trivia_answerchosen_receiver    = new SignalReceiver().SetOnSignalCallback(AnswerChosen);
    }

    protected override void OnEnable()
    {
        trivia_answerchosen_stream.ConnectReceiver(trivia_answerchosen_receiver);
    }

    protected override void OnDisable()
    {
        trivia_answerchosen_stream.DisconnectReceiver(trivia_answerchosen_receiver);
    }

    private void OnDestroy()
    {
        trivia_answerchosen_stream.DisconnectReceiver(trivia_answerchosen_receiver);
    }

    #endregion

    #region Public Functions

    public override void StartGame()
    {
        currentQuestionIndex = -1;

        NextQuestion();
    }

    public override void BoardReset()
    {
        NextQuestion();
    }

    public override string GetBattleGameName()
    {
        return Helpful.GetStringFromBattleGameType(Helpful.BattleGameTypes.Trivia);
    }

    #endregion

    #region Private Functions

    private void NextQuestion()
    {
        currentQuestionIndex++;
        
        if (currentQuestionIndex >= BattleManager.instance.CurrentEnemy.UnitInfo.TriviaQuestions.Count)
        {
            currentQuestionIndex = 0;
        }

        TriviaQuestion currentTriviaQuestion                = BattleManager.instance.CurrentEnemy.UnitInfo.TriviaQuestions[currentQuestionIndex];
        currentAnswers                                      = new List<TriviaQuestion.TriviaAnswer>(currentTriviaQuestion.Answers);

        question.text                                       = currentTriviaQuestion.Question;

        //Shuffle Answers
        int n                                               = currentAnswers.Count;
        System.Random rng                                   = new System.Random();
        while (n > 1)
        {
            n--;
            int k                                           = rng.Next(n + 1);
            TriviaQuestion.TriviaAnswer value               = currentAnswers[k];
            currentAnswers[k]                               = currentAnswers[n];
            currentAnswers[n]                               = value;
        }
        //

        answer0.text                                        = currentAnswers[0].answerText;
        answer1.text                                        = currentAnswers[1].answerText;
        answer2.text                                        = currentAnswers[2].answerText;
        answer3.text                                        = currentAnswers[3].answerText;
    }

    private void AnswerChosen(Signal signal)
    {
        //Signal Data should be int
        //  the answer number that was selected

        int selection   = signal.GetValueUnsafe<int>();
        object[] info   = new object[2];
        info[0]         = currentAnswers[selection].correct ? AbilityCharger.AbilityChargeActions.CORRECT_RESPONSE :
                            AbilityCharger.AbilityChargeActions.INCORRECT_RESPONSE;
        
        switch (selection)
        {
            case 0:
                info[1] = (Vector2)answer0.transform.position;
                break;
            case 1:
                info[1] = (Vector2)answer1.transform.position;
                break;
            case 2:
                info[1] = (Vector2)answer2.transform.position;
                break;
            case 3:
                info[1] = (Vector2)answer3.transform.position;
                break;
        }

        if (currentAnswers[selection].correct)
        {
            AudioManager.instance.Play("Go");

            Signal.Send("Battle", "AbilityChargeGenerated", info);

            NextQuestion();
        }
        else
        {
            AudioManager.instance.Play("No");

            Signal.Send("Battle", "AbilityChargeGenerated", info);
        }
    }

    #endregion
}
