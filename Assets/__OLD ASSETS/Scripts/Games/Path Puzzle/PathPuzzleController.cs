using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Doozy.Runtime.Signals;
using Doozy.Runtime.Reactor;
using Doozy.Runtime.UIManager.Components;

public class PathPuzzleController : MonoBehaviour
{
    public PathPuzzleLevel                  currentPPLevel;
    public int                              currentBoardNum;
    public int                              gameLostCoroutineCounter = 0;

    [Space]
    [Space]

    public GameObject                       gameBoard;
    public CountdownClockController         countdownClock;
    public TextMeshProUGUI                  connectionCounterTitle;
    public TextMeshProUGUI                  connectionCounter;

    public List<PathPuzzleTileController_old>   tiles;

    private int                             pathPiecesConnected;
    private bool                            won;

    private SignalReceiver                  pathpuzzle_tilerotated_receiver;
    private SignalStream                    pathpuzzle_tilerotated_stream;
    private SignalReceiver                  gamemanagement_gamesetup_receiver;
    private SignalStream                    gamemanagement_gamesetup_stream;

    private SignalReceiver                  quitconfirmation_exitlevel_receiver;
    private SignalStream                    quitconfirmation_exitlevel_stream;
    private SignalReceiver                  quitconfirmation_backtogame_receiver;
    private SignalStream                    quitconfirmation_backtogame_stream;
    private SignalReceiver                  quitconfirmation_popup_receiver;
    private SignalStream                    quitconfirmation_popup_stream;

    private void Awake()
    {
        Canvas c        = GetComponentInParent<Canvas>();
        c.worldCamera   = Camera.main;
        c.sortingOrder  = UniversalInspectorVariables.instance.gameScreenOrderInLayer;

        pathpuzzle_tilerotated_stream           = SignalStream.Get("PathPuzzle", "TileRotated");
        gamemanagement_gamesetup_stream         = SignalStream.Get("GameManagement", "GameSetup");
        quitconfirmation_exitlevel_stream       = SignalStream.Get("QuitConfirmation", "ExitLevel");
        quitconfirmation_backtogame_stream      = SignalStream.Get("QuitConfirmation", "BackToGame");
        quitconfirmation_popup_stream           = SignalStream.Get("QuitConfirmation", "Popup");

        pathpuzzle_tilerotated_receiver         = new SignalReceiver().SetOnSignalCallback(CheckTiles);
        gamemanagement_gamesetup_receiver       = new SignalReceiver().SetOnSignalCallback(Setup);
        quitconfirmation_exitlevel_receiver     = new SignalReceiver().SetOnSignalCallback(EndGameEarly);
        quitconfirmation_backtogame_receiver    = new SignalReceiver().SetOnSignalCallback(Unpause);
        quitconfirmation_popup_receiver         = new SignalReceiver().SetOnSignalCallback(Pause);
    }

    private void OnEnable()
    {
        pathpuzzle_tilerotated_stream.ConnectReceiver(pathpuzzle_tilerotated_receiver);
        gamemanagement_gamesetup_stream.ConnectReceiver(gamemanagement_gamesetup_receiver);
        quitconfirmation_exitlevel_stream.ConnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.ConnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.ConnectReceiver(quitconfirmation_popup_receiver);
    }

    private void OnDisable()
    {
        pathpuzzle_tilerotated_stream.DisconnectReceiver(pathpuzzle_tilerotated_receiver);
        gamemanagement_gamesetup_stream.DisconnectReceiver(gamemanagement_gamesetup_receiver);
        quitconfirmation_exitlevel_stream.DisconnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.DisconnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.DisconnectReceiver(quitconfirmation_popup_receiver);
    }

    //Called by the PathPuzzlePlay screen's OnHide callback
    public void OnHide()
    {
        quitconfirmation_exitlevel_stream.DisconnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.DisconnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.DisconnectReceiver(quitconfirmation_popup_receiver);
    }

    //Called by the PathPuzzlePlay screen's OnShow callback
    public void OnShow()
    {
        quitconfirmation_exitlevel_stream.ConnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.ConnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.ConnectReceiver(quitconfirmation_popup_receiver);
    }

    public void Setup(Signal signal)
    {
        //currentPPLevel      = (PathPuzzleLevel)GameManager.instance.currentLevelOLD;

        currentBoardNum     = -1;
        won                 = false;
        pathPiecesConnected = 0;

        Signal.Send("GameManagement", "DisableExitLevelButton", true);

        SetConnectionCounter();

        countdownClock.SetupTimer(currentPPLevel.timeLimitInSeconds, currentPPLevel.parTimeInSeconds, true);

        LoadNextBoard();
    }

    //Called by the PathPuzzle view's OnShow animation complete callback
    public void StartGame()
    {
        countdownClock.StartTimer();
    }

    public void CheckTiles(Signal signal)
    {
        for (int i = 0; i < tiles.Count; i++)
            tiles[i].partOfPath = false;

        PathPuzzleTileController_old startTile = tiles.Find(x => x.start);
        PathPuzzleTileController_old finishTile = tiles.Find(x => x.finish);

        HashSet<PathPuzzleTileController_old> checkedSet = 
            new HashSet<PathPuzzleTileController_old>();

        Stack<PathPuzzleTileController_old> remainingStack =
            new Stack<PathPuzzleTileController_old>();

        checkedSet.Add(startTile);
        remainingStack.Push(startTile);

        while (remainingStack.Count > 0)
        {
            PathPuzzleTileController_old tile = remainingStack.Pop();
            List<PathPuzzleTileController_old> connections = new List<PathPuzzleTileController_old>();

            if (tile.north)
            {
                if (tiles.Exists(findTile => 
                    (findTile.gridPosition == new Vector2(tile.gridPosition.x, tile.gridPosition.y - 1)) 
                    && findTile.south))
                {
                    connections.Add(tiles.Find(findTile =>
                        (findTile.gridPosition == new Vector2(tile.gridPosition.x, tile.gridPosition.y - 1))
                        && findTile.south));
                }
            }

            if (tile.east)
            {
                if (tiles.Exists(findTile =>
                    (findTile.gridPosition == new Vector2(tile.gridPosition.x + 1, tile.gridPosition.y))
                    && findTile.west))
                {
                    connections.Add(tiles.Find(findTile =>
                        (findTile.gridPosition == new Vector2(tile.gridPosition.x + 1, tile.gridPosition.y))
                        && findTile.west));
                }
            }

            if (tile.south)
            {
                if (tiles.Exists(findTile =>
                    (findTile.gridPosition == new Vector2(tile.gridPosition.x, tile.gridPosition.y + 1))
                    && findTile.north))
                {
                    connections.Add(tiles.Find(findTile =>
                        (findTile.gridPosition == new Vector2(tile.gridPosition.x, tile.gridPosition.y + 1))
                        && findTile.north));
                }
            }

            if (tile.west)
            {
                if (tiles.Exists(findTile =>
                    (findTile.gridPosition == new Vector2(tile.gridPosition.x - 1, tile.gridPosition.y))
                    && findTile.east))
                {
                    connections.Add(tiles.Find(findTile =>
                        (findTile.gridPosition == new Vector2(tile.gridPosition.x - 1, tile.gridPosition.y))
                        && findTile.east));
                }
            }

            for (int i = 0; i < connections.Count; i++)
            {
                if (checkedSet.Add(connections[i]))
                {
                    remainingStack.Push(connections[i]);
                }
            }
        }

        foreach (PathPuzzleTileController_old t in checkedSet)
            t.partOfPath = true;

        for (int i = 0; i < tiles.Count; i++)
            tiles[i].MarkTileAsConnectedToStart();

        if (checkedSet.Contains(startTile) && checkedSet.Contains(finishTile))
        {
            if (signal.sourceGameObject == null) //if it's the setup phase
            {
                //TODO: This can potentially cause an infinite loop I think
                //      For instance, if there are 2 paths to the end and a T Tile spins between them?
                //      idk for sure but look into it bitch

                Debug.Log("Loaded solved, changing tile");

                tiles.Find(x => !x.start && !x.finish && !x.nonpath && x.partOfPath).SetInitialRotation(1);
                CheckTiles(new Signal());
            }
            else
            {
                pathPiecesConnected += checkedSet.Count;
                SetConnectionCounter();

                Helpful.TextPopup(string.Format("+{0}", checkedSet.Count.ToString())
                    , connectionCounter.transform
                    , Vector2.zero
                    , Color.green, UniversalInspectorVariables.instance.KGHappySolid
                    , 80);

                StartCoroutine(AnimateBoardEnding());
            }
        }
    }

    //Invoked by the countdownClock's onOutOfTime event
    public void OutOfTime()
    {
        won = false;

        StartCoroutine(GameLostAnimation());
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
        won = false;

        pathPiecesConnected = 0;
        SetConnectionCounter();

        countdownClock.Pause();
        countdownClock.SetTime(-1f);

        //AudioManager.instance.Play("Out of Time", .5f);

        //StartCoroutine(GameLostAnimation());
    }

    private IEnumerator AnimateBoardEnding()
    {
        countdownClock.Pause();
        FreezeButtons(false);

        for (int i = 0; i < tiles.Count; i++)
        {
            StartCoroutine(tiles[i].SpinShrink());
        }

        AudioManager.instance.Play("Go");

        while (tiles.FindIndex(x => x.endAnimation) >= 0)
            yield return null;

        LoadNextBoard();

        //countdownClock.Unpause();
    }

    private void LoadNextBoard()
    {
        currentBoardNum++;

        if (currentBoardNum < currentPPLevel.boards.Count)
        {
            //TODO: Pool these too
            foreach (Transform child in gameBoard.transform)
                Destroy(child.gameObject);

            tiles.Clear();

            GridLayoutGroup glg     = gameBoard.GetComponent<GridLayoutGroup>();
            glg.constraintCount     = currentPPLevel.boards[currentBoardNum].columns;
            glg.cellSize            = new Vector2(currentPPLevel.boards[currentBoardNum].cellSize
                                    , currentPPLevel.boards[currentBoardNum].cellSize);

            for (int i = 0; i < currentPPLevel.boards[currentBoardNum].tiles.Count; i++)
            {
                GameObject go = Instantiate(currentPPLevel.boards[currentBoardNum].tiles[i]);

                go.transform.SetParent(gameBoard.transform);
                go.transform.localScale = Vector3.one;

                PathPuzzleTileController_old control = go.GetComponent<PathPuzzleTileController_old>();

                control.gridPosition = new Vector2(i % currentPPLevel.boards[currentBoardNum].columns
                    , i / currentPPLevel.boards[currentBoardNum].columns);

                if (control.start)
                {
                    control.SetInitialRotation(currentPPLevel.boards[currentBoardNum].initialStartTileRotations);
                }
                else if (control.finish)
                {
                    control.SetInitialRotation(currentPPLevel.boards[currentBoardNum].initialFinishTileRotations);
                }
                else if (!control.nonpath)
                {
                    control.SetInitialRotation(Random.Range(1, 4));
                }

                tiles.Add(control);
            }

            CheckTiles(new Signal());

            if (currentBoardNum > 0) //Don't unpause on setup, but unpause after each new board is generated in game
                countdownClock.Unpause();
        }
        else
        {
            won = true;

            FreezeButtons(true);
            EndGame();
        }
    }

    private void FreezeButtons(bool includeExitButton)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].GetComponent<UIButton>().interactable = false;
        }

        if (includeExitButton)
            Signal.Send("GameManagement", "DisableExitLevelButton", false);
    }

    private void EndGame()
    {
        countdownClock.Pause();

        if (won)
        {
            if (!currentPPLevel.objective1)
                currentPPLevel.objective1 = true;

            if (currentPPLevel.levelsUnlockedByThisLevel != null)
            {
                for (int i = 0; i < currentPPLevel.levelsUnlockedByThisLevel.Count; i++)
                {
                    if (!currentPPLevel.levelsUnlockedByThisLevel[i].unlocked)
                    {
                        currentPPLevel.levelsUnlockedByThisLevel[i].unlocked = true;
                        //GameManager.instance.SetWorldMapUnlockLevels(currentPPLevel.levelsUnlockedByThisLevel[i]);
                    }
                }
            }
        }

        //Add one to seconds remaining because the clock display is actually showing a second behind
        if ((countdownClock.SecondsRemaining == 0 ? 0 : countdownClock.SecondsRemaining + 1) >= currentPPLevel.parTimeInSeconds 
            && !currentPPLevel.objective2)
            currentPPLevel.objective2 = true;

        if (pathPiecesConnected >= currentPPLevel.piecesConnectedGoal && !currentPPLevel.objective3)
            currentPPLevel.objective3 = true;

        StartCoroutine(ShrinkBoard());
    }
    
    private void GoToEndScreen()
    {
        LevelResultsData results    = new LevelResultsData();

        System.TimeSpan ts          = System.TimeSpan.FromSeconds(countdownClock.SecondsRemaining <= 0 ? 0 : countdownClock.SecondsRemaining + 1);

        results.successIndicator    = won;
        results.subtitleText        = string.Format("Time Remaining {0}:{1}\n{2} pieces traversed!", ts.Minutes.ToString(),
                                      ts.Seconds.ToString("00"), pathPiecesConnected.ToString());

        //GameManager.instance.SetLevelResults(results);

        Signal.Send("GameManagement", "LevelEnded", 0);
    }

    private void SetConnectionCounter()
    {
        connectionCounter.text = pathPiecesConnected.ToString();

        if (pathPiecesConnected >= currentPPLevel.piecesConnectedGoal)
        {
            connectionCounter.color         = Color.green;
            connectionCounterTitle.color    = Color.green;
        }
    }

    private IEnumerator GameLostAnimation()
    {
        FreezeButtons(true);

        for (int i = 0; i < tiles.Count; i++)
        {
            gameLostCoroutineCounter++;
            StartCoroutine(tiles[i].ShakeAndFall());
        }

        while (gameLostCoroutineCounter > 0)
            yield return null;

        EndGame();
    }

    private IEnumerator ShrinkBoard()
    {
        foreach (Image i in gameBoard.GetComponentsInChildren<Image>())
            if (i.gameObject != gameBoard)
                i.enabled = false;

        GridLayoutGroup g = gameBoard.GetComponent<GridLayoutGroup>();

        float startTime = Time.time;

        while (g.cellSize.x > 0)
        {
            float distCovered = (Time.time - startTime) * 10f;

            g.cellSize = new Vector2(g.cellSize.x - distCovered, g.cellSize.x - distCovered);

            yield return null;
        }

        g.cellSize = Vector2.zero;

        foreach (Transform child in gameBoard.transform)
            Destroy(child.gameObject);

        tiles.Clear();

        GoToEndScreen();
    }
}
