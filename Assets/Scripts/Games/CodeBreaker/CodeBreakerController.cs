using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Signals;
using UnityEngine.UI;

public class CodeBreakerController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private GameObject             codeBreakerGuessButtonPrefab;
    [SerializeField] private GameObject             codeBreakerChoiceButtonPrefab;
    [SerializeField] private GameObject             codeBreakerGuessTrackerPrefab;
    [SerializeField] private GameObject             codeBreakerGuessTrackerIconPrefab;

    [Space]
    [Space]

    [SerializeField] private GameObject             mainPanel;
    [SerializeField] private GameObject             rightPanelButtonPanel;
    [SerializeField] private GameObject             leftPanelListContainer;
    [SerializeField] private GameObject             selectionCursor;

    #endregion

    #region Private Variables

    private CodeBreakerLevel                        currentCodeBreakerLevel;

    private List<CodeBreakerGuessButtonController>  guessButtons;

    private int                                     selectedGuessSlotID;
    private int                                     attemptNumber;

    #endregion

    #region Signal Streams/Receivers

    private SignalReceiver                          gamemanagement_gamesetup_receiver;
    private SignalStream                            gamemanagement_gamesetup_stream;
    private SignalReceiver                          quitconfirmation_exitlevel_receiver;
    private SignalStream                            quitconfirmation_exitlevel_stream;
    private SignalReceiver                          quitconfirmation_backtogame_receiver;
    private SignalStream                            quitconfirmation_backtogame_stream;
    private SignalReceiver                          quitconfirmation_popup_receiver;
    private SignalStream                            quitconfirmation_popup_stream;

    private SignalReceiver                          codebreaker_guessbuttonclicked_receiver;
    private SignalStream                            codebreaker_guessbuttonclicked_stream;
    private SignalReceiver                          codebreaker_assignchoice_receiver;
    private SignalStream                            codebreaker_assignchoice_stream;

    #endregion


    #region Unity Functions
    private void Awake()
    {
        Canvas c            = GetComponentInParent<Canvas>();
        c.worldCamera       = Camera.main;
        c.sortingOrder      = UniversalInspectorVariables.instance.gameScreenOrderInLayer;

        gamemanagement_gamesetup_stream         = SignalStream.Get("GameManagement", "GameSetup");
        quitconfirmation_exitlevel_stream       = SignalStream.Get("QuitConfirmation", "ExitLevel");
        quitconfirmation_backtogame_stream      = SignalStream.Get("QuitConfirmation", "BackToGame");
        quitconfirmation_popup_stream           = SignalStream.Get("QuitConfirmation", "Popup");
        codebreaker_guessbuttonclicked_stream   = SignalStream.Get("CodeBreaker", "GuessButtonClicked");
        codebreaker_assignchoice_stream         = SignalStream.Get("CodeBreaker", "AssignChoice");

        gamemanagement_gamesetup_receiver       = new SignalReceiver().SetOnSignalCallback(Setup);
        quitconfirmation_exitlevel_receiver     = new SignalReceiver().SetOnSignalCallback(EndGameEarly);
        quitconfirmation_backtogame_receiver    = new SignalReceiver().SetOnSignalCallback(Unpause);
        quitconfirmation_popup_receiver         = new SignalReceiver().SetOnSignalCallback(Pause);
        codebreaker_guessbuttonclicked_receiver = new SignalReceiver().SetOnSignalCallback(GuessButtonClicked);
        codebreaker_assignchoice_receiver       = new SignalReceiver().SetOnSignalCallback(AssignGuess);
    }

    private void OnEnable()
    {
        gamemanagement_gamesetup_stream.ConnectReceiver(gamemanagement_gamesetup_receiver);
        quitconfirmation_exitlevel_stream.ConnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.ConnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.ConnectReceiver(quitconfirmation_popup_receiver);
        codebreaker_guessbuttonclicked_stream.ConnectReceiver(codebreaker_guessbuttonclicked_receiver);
        codebreaker_assignchoice_stream.ConnectReceiver(codebreaker_assignchoice_receiver);
    }

    private void OnDisable()
    {
        gamemanagement_gamesetup_stream.DisconnectReceiver(gamemanagement_gamesetup_receiver);
        quitconfirmation_exitlevel_stream.DisconnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.DisconnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.DisconnectReceiver(quitconfirmation_popup_receiver);
        codebreaker_guessbuttonclicked_stream.DisconnectReceiver(codebreaker_guessbuttonclicked_receiver);
        codebreaker_assignchoice_stream.DisconnectReceiver(codebreaker_assignchoice_receiver);
    }

    #endregion

    #region Public Functions

    //Called by the Gameplay View's Show Animation Finished callback
    public void StartGame()
    {
        selectionCursor.SetActive(true);
    }

    //Called by the Submit Button's OnClick Behavior
    public void SubmitGuess()
    {
        if (guessButtons.FindIndex(x => x.currentGuess == CodeBreakerChoicesInfo.CodebreakerChoices.NULL) >= 0)
            return;

        CreateAttemptTracker();

        ClearGuesses();
    }

    #endregion

    #region Private Functions

    private void Setup(Signal signal)
    {
        currentCodeBreakerLevel     = (CodeBreakerLevel)GameManager.instance.currentLevel;
        attemptNumber               = 0;

        SetupMainPanel();
        SetupRightPanel();
    }

    private void SetupMainPanel()
    {
        guessButtons                = new List<CodeBreakerGuessButtonController>();

        GridLayoutGroup layoutGroup = mainPanel.GetComponent<GridLayoutGroup>();

        switch(currentCodeBreakerLevel.solution.Count)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                Debug.Log("solution is too short");
                return;
            case 4:
            case 5:
            case 6:
            case 7:
                layoutGroup.cellSize    = new Vector2(175f, 175f);
                layoutGroup.spacing     = new Vector2(50f, 0f);
                ((RectTransform)selectionCursor.transform).sizeDelta = new Vector2(220f, 220f);
                break;
            case 8:
                layoutGroup.cellSize    = new Vector2(150f, 150f);
                layoutGroup.spacing     = new Vector2(40f, 0f);
                break;
        }

        for (int i = 0; i < currentCodeBreakerLevel.solution.Count; i++)
        {
            GameObject go                                   = Instantiate(codeBreakerGuessButtonPrefab, mainPanel.transform);
            CodeBreakerGuessButtonController buttonControl  = go.GetComponent<CodeBreakerGuessButtonController>();

            buttonControl.buttonID                          = i;
            buttonControl.Clear();

            guessButtons.Add(buttonControl);
        }

        Canvas.ForceUpdateCanvases();

        SelectGuessButton(0);
        selectionCursor.SetActive(false);
    }

    private void SetupRightPanel()
    {
        for (int i = 0; i < currentCodeBreakerLevel.possibleChoices.Count; i++)
        {
            GameObject go                               = Instantiate(codeBreakerChoiceButtonPrefab, rightPanelButtonPanel.transform);
            CodeBreakerChoiceSetButton buttonControl    = go.GetComponent<CodeBreakerChoiceSetButton>();

            buttonControl.Setup(currentCodeBreakerLevel.possibleChoices[i], i);
        }
    }

    private void GuessButtonClicked(Signal signal)
    {
        //Signal is int - the buttonID that was clicked

        int id = signal.GetValueUnsafe<int>();

        SelectGuessButton(id);
    }

    private void SelectGuessButton(int buttonID)
    {
        Transform buttonTrans               = guessButtons.Find(x => x.buttonID == buttonID).transform;

        selectionCursor.transform.position  = buttonTrans.position;

        selectedGuessSlotID                 = buttonID;
    }

    private void AssignGuess(Signal signal)
    {
        //Signal is int - the buttonID that was clicked

        int id = signal.GetValueUnsafe<int>();

        CodeBreakerGuessButtonController g = guessButtons.Find(x => x.buttonID == selectedGuessSlotID);

        g.SetIcon(currentCodeBreakerLevel.possibleChoices[id]);

        if (selectedGuessSlotID < guessButtons.Count - 1)
            SelectGuessButton(selectedGuessSlotID + 1);
    }

    private void CreateAttemptTracker()
    {
        attemptNumber++;

        GameObject go                               = Instantiate(codeBreakerGuessTrackerPrefab, leftPanelListContainer.transform);
        CodeBreakerGuessListItemController control  = go.GetComponent<CodeBreakerGuessListItemController>();

        List<CodeBreakerChoicesInfo.CodebreakerChoices> choices = new List<CodeBreakerChoicesInfo.CodebreakerChoices>();
        List<int> solutionIndicators                            = new List<int>();

        for (int i = 0; i < guessButtons.Count; i++)
        {
            choices.Add(guessButtons[i].currentGuess);

            int indicator;

            //If the icon is in the correct position
            if (choices[i] == currentCodeBreakerLevel.solution[i])
            {
                indicator = 0;
            }
            //if the icon is in the puzzle, but in the incorrect position
            else if (currentCodeBreakerLevel.solution.Contains(choices[i]))
            {
                indicator = 1;
            }
            //Not in the solution, or the exceptions to the above cases for icons in the solutions
            else
            {
                indicator = -1;
            }

            solutionIndicators.Add(indicator);
        }

        if (solutionIndicators.Contains(1))
        {
            List<int> indexesToCheck = new List<int>();

            for (int i = 0; i < solutionIndicators.Count; i++)
                if (solutionIndicators[i] != 0)
                    indexesToCheck.Add(i);
            
            for (int i = 0; i < solutionIndicators.Count; i++)
            {
                if (solutionIndicators[i] == 0)
                    continue;

                CodeBreakerChoicesInfo.CodebreakerChoices c = choices[i];
                
                for (int j = 0; j < indexesToCheck.Count; j++)
                {
                    if (i == indexesToCheck[j])
                        continue;

                    if (currentCodeBreakerLevel.solution[indexesToCheck[j]] == c)
                        goto nextSolutionSlot;
                }

                solutionIndicators[i] = -1;

                nextSolutionSlot:;
            } //TODO: only mark the 1st x of solutions as yellow, (don't mark 3 yellow if there's only 1 of the choice in the solution)
        }

        control.Setup(codeBreakerGuessTrackerIconPrefab, attemptNumber, choices, solutionIndicators);
    }

    private void ClearGuesses()
    {
        for (int i = 0; i < guessButtons.Count; i++)
        {
            guessButtons[i].Clear();
        }

        SelectGuessButton(0);
    }

    private void EndGameEarly(Signal signal)
    {

    }

    private void Pause(Signal signal)
    {

    }

    private void Unpause(Signal signal)
    {

    }

    #endregion
}
