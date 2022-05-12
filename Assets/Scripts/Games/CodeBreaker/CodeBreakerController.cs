using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Doozy.Runtime.Signals;
using UnityEngine.UI;
using Doozy.Runtime.UIManager.Components;

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
    private bool                                    won;

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
        EnableScreenItems(true);

        currentCodeBreakerLevel     = (CodeBreakerLevel)GameManager.instance.currentLevel;
        attemptNumber               = 0;
        won                         = false;

        SetupMainPanel();
        SetupRightPanel();
        SetupLeftPanel();

        //This section is needed for some reason bc deactivating buttons on endgame and then activating at the beginning of this function
        //causes them to not be positioned correctly
        mainPanel.SetActive(false);
        Canvas.ForceUpdateCanvases();
        mainPanel.SetActive(true);
        Canvas.ForceUpdateCanvases();
        //
    }

    private void SetupMainPanel()
    {
        if (guessButtons == null)
            guessButtons            = new List<CodeBreakerGuessButtonController>();

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
                ((RectTransform)selectionCursor.transform).sizeDelta = new Vector2(195f, 195f);
                break;
        }

        if (mainPanel.transform.childCount == 0) //No buttons yet
        {
            for (int i = 0; i < currentCodeBreakerLevel.solution.Count; i++)
            {
                GameObject go                                   = Instantiate(codeBreakerGuessButtonPrefab, mainPanel.transform);
                CodeBreakerGuessButtonController buttonControl  = go.GetComponent<CodeBreakerGuessButtonController>();

                buttonControl.buttonID                          = i;
                buttonControl.Clear();

                guessButtons.Add(buttonControl);
            }
        }
        else if (mainPanel.transform.childCount > currentCodeBreakerLevel.solution.Count) //More children than needed for the solution
        {
            for (int i = currentCodeBreakerLevel.solution.Count; i < mainPanel.transform.childCount; i++)
            {
                Transform t = mainPanel.transform.GetChild(i);

                if (t.gameObject.activeSelf)
                {
                    guessButtons.Remove(t.GetComponent<CodeBreakerGuessButtonController>());
                    t.gameObject.SetActive(false);
                }
            }
        }
        else if (mainPanel.transform.childCount < currentCodeBreakerLevel.solution.Count) //Fewer children than are needed, but there are some existing children
        {
            //Check children to see if they're active. If they are, skip, if not activate and add to guess buttons.
            //if past the child count, make new ones
            guessButtons.Clear();

            for (int i = 0; i < currentCodeBreakerLevel.solution.Count; i++)
            {
                if (i < mainPanel.transform.childCount)
                {
                    GameObject go = mainPanel.transform.GetChild(i).gameObject;
                    CodeBreakerGuessButtonController buttonControl = go.GetComponent<CodeBreakerGuessButtonController>();

                    if (!go.activeSelf)
                        go.SetActive(true);

                    buttonControl.buttonID = i;
                    buttonControl.Clear();

                    guessButtons.Add(buttonControl);
                }
                else
                {
                    GameObject go = Instantiate(codeBreakerGuessButtonPrefab, mainPanel.transform);
                    CodeBreakerGuessButtonController buttonControl = go.GetComponent<CodeBreakerGuessButtonController>();

                    buttonControl.buttonID = i;
                    buttonControl.Clear();

                    guessButtons.Add(buttonControl);
                }
            }
        }
        else //Same number needed as existing. Don't need to do anything I think
        {

        }

        Canvas.ForceUpdateCanvases();

        SelectGuessButton(0);
        selectionCursor.SetActive(false);
    }

    private void SetupRightPanel()
    {
        foreach (Transform child in rightPanelButtonPanel.transform)
            Destroy(child.gameObject);

        for (int i = 0; i < currentCodeBreakerLevel.possibleChoices.Count; i++)
        {
            GameObject go                               = Instantiate(codeBreakerChoiceButtonPrefab, rightPanelButtonPanel.transform);
            CodeBreakerChoiceSetButton buttonControl    = go.GetComponent<CodeBreakerChoiceSetButton>();

            buttonControl.Setup(currentCodeBreakerLevel.possibleChoices[i], i);
        }
    }

    private void SetupLeftPanel()
    {
        foreach (Transform child in leftPanelListContainer.transform)
            Destroy(child.gameObject);
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
            Dictionary<CodeBreakerChoicesInfo.CodebreakerChoices, int> maxNumberOfWrongPlace = new Dictionary<CodeBreakerChoicesInfo.CodebreakerChoices, int>();

            for (int i = 0; i < currentCodeBreakerLevel.solution.Count; i++)
            {
                if (!maxNumberOfWrongPlace.ContainsKey(choices[i]))
                {
                    int numInSolution = currentCodeBreakerLevel.solution.Where(x => x == choices[i]).Count();

                    maxNumberOfWrongPlace.Add(choices[i], numInSolution);
                }
            }

            for (int i = 0; i < choices.Count; i++)
            {
                if (solutionIndicators[i] == 0)
                    maxNumberOfWrongPlace[choices[i]] -= 1;
            }

            for (int i = 0; i < choices.Count; i++)
            {
                if (solutionIndicators[i] != 1)
                    continue;

                if (maxNumberOfWrongPlace.ContainsKey(choices[i]))
                {
                    if (maxNumberOfWrongPlace[choices[i]] > 0)
                    {
                        maxNumberOfWrongPlace[choices[i]] -= 1;
                    }
                    else
                    {
                        solutionIndicators[i] = -1;
                    }    
                }
            }
        }

        control.Setup(codeBreakerGuessTrackerIconPrefab, attemptNumber, choices, solutionIndicators);

        RectTransform leftTran  = (RectTransform)leftPanelListContainer.transform;
        ScrollRect sc           = leftPanelListContainer.GetComponentInParent<ScrollRect>();

        if (leftTran.sizeDelta.y >= 0)
        {
            sc.enabled              = true;

            Canvas.ForceUpdateCanvases();

            leftTran.localPosition  = new Vector3(leftTran.localPosition.x
                , leftTran.sizeDelta.y + 50f //50 is the list item size.y
                , leftTran.localPosition.z);
        }
        else
            sc.enabled              = false;

        if (!solutionIndicators.Contains(1) && !solutionIndicators.Contains(-1))
        {
            AudioManager.instance.Play("Go");

            won = true;

            EndGame();
        }
        else
        {
            AudioManager.instance.Play("No");
        }

        //TODO: Add arrows/indicators showing list is scrollable. Make scrollable only when scroll is needed.
        //      Pull view to bottom of list when a new one is added
    }

    private void ClearGuesses()
    {
        for (int i = 0; i < guessButtons.Count; i++)
        {
            guessButtons[i].Clear();
        }

        SelectGuessButton(0);
    }

    private void EndGame()
    {
        EnableScreenItems(false);

        if (won)
        {
            if (!currentCodeBreakerLevel.objective1)
                currentCodeBreakerLevel.objective1 = true;

            if (currentCodeBreakerLevel.nextLevel != null && !currentCodeBreakerLevel.nextLevel.unlocked)
                currentCodeBreakerLevel.nextLevel.unlocked = true;
        }     

        if (attemptNumber <= currentCodeBreakerLevel.highNumberOfGuessesGoal)
        {
            if (!currentCodeBreakerLevel.objective2)
                currentCodeBreakerLevel.objective2 = true;

            if (attemptNumber <= currentCodeBreakerLevel.lowNumberOfGuessesGoal && !currentCodeBreakerLevel.objective3)
                currentCodeBreakerLevel.objective3 = true;
        }

        LevelResultsData results    = new LevelResultsData();
        results.successIndicator    = won;
        results.subtitleText        = string.Format("{0} attempts made!", attemptNumber.ToString());

        GameManager.instance.SetLevelResults(results);

        selectionCursor.SetActive(false);

        Invoke("EndSignal", 1.5f);
    }

    private void EnableScreenItems(bool settingOn)
    {
        foreach (UIButton butt in GetComponentsInChildren<UIButton>())
            butt.interactable = settingOn;

        ScrollRect sc   = leftPanelListContainer.GetComponentInParent<ScrollRect>();
        sc.enabled      = settingOn;
    }

    private void EndSignal()
    {
        Signal.Send("GameManagement", "LevelEnded", 0);
    }

    private void EndGameEarly(Signal signal)
    {
        EndGame();
    }

    private void Pause(Signal signal)
    {

    }

    private void Unpause(Signal signal)
    {

    }

    #endregion
}
