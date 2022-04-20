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

    [Space]
    [Space]

    public GameObject                       gameBoard;
    public CountdownClockController         countdownClock;
    public TextMeshProUGUI                  connectionCounterTitle;
    public TextMeshProUGUI                  connectionCounter;
    public Font                             popupFont;

    public List<PathPuzzleTileController>   tiles;

    private int                             pathPiecesConnected;
    private bool                            won;

    private SignalReceiver                  pathpuzzle_tilerotated_receiver;
    private SignalStream                    pathpuzzle_tilerotated_stream;
    private SignalReceiver                  pathpuzzle_pathpuzzlesetup_receiver;
    private SignalStream                    pathpuzzle_pathpuzzlesetup_stream;

    private SignalReceiver                  quitconfirmation_exitlevel_receiver;
    private SignalStream                    quitconfirmation_exitlevel_stream;
    private SignalReceiver                  quitconfirmation_backtogame_receiver;
    private SignalStream                    quitconfirmation_backtogame_stream;
    private SignalReceiver                  quitconfirmation_popup_receiver;
    private SignalStream                    quitconfirmation_popup_stream;

    private void Awake()
    {
        pathpuzzle_tilerotated_stream           = SignalStream.Get("PathPuzzle", "TileRotated");
        pathpuzzle_pathpuzzlesetup_stream       = SignalStream.Get("PathPuzzle", "PathPuzzleSetup");
        quitconfirmation_exitlevel_stream       = SignalStream.Get("QuitConfirmation", "ExitLevel");
        quitconfirmation_backtogame_stream      = SignalStream.Get("QuitConfirmation", "BackToGame");
        quitconfirmation_popup_stream           = SignalStream.Get("QuitConfirmation", "Popup");

        pathpuzzle_tilerotated_receiver         = new SignalReceiver().SetOnSignalCallback(CheckTiles);
        pathpuzzle_pathpuzzlesetup_receiver     = new SignalReceiver().SetOnSignalCallback(Setup);
        quitconfirmation_exitlevel_receiver     = new SignalReceiver().SetOnSignalCallback(EndGameEarly);
        quitconfirmation_backtogame_receiver    = new SignalReceiver().SetOnSignalCallback(Unpause);
        quitconfirmation_popup_receiver         = new SignalReceiver().SetOnSignalCallback(Pause);
    }

    private void OnEnable()
    {
        pathpuzzle_tilerotated_stream.ConnectReceiver(pathpuzzle_tilerotated_receiver);
        pathpuzzle_pathpuzzlesetup_stream.ConnectReceiver(pathpuzzle_pathpuzzlesetup_receiver);
        quitconfirmation_exitlevel_stream.ConnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.ConnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.ConnectReceiver(quitconfirmation_popup_receiver);
    }

    private void OnDisable()
    {
        pathpuzzle_tilerotated_stream.DisconnectReceiver(pathpuzzle_tilerotated_receiver);
        pathpuzzle_pathpuzzlesetup_stream.DisconnectReceiver(pathpuzzle_pathpuzzlesetup_receiver);
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
        //Signal Data should be object[2]
        //  index 0 =>  int                 - 0 == new level, 1 == replay current level, 2 == play next level
        //  index 1 =>  PathPuzzleLevel     - the info for the current level (only needed if 0 above)

        object[] data = signal.GetValueUnsafe<object[]>();

        if ((int)data[0] == 0) //Level was defined by call
        {
            currentPPLevel = (PathPuzzleLevel)data[1];
        }
        else if ((int)data[0] == 2) //Play Next Level
        {
            currentPPLevel = (PathPuzzleLevel)currentPPLevel.nextLevel;
        }

        currentBoardNum     = -1;
        won                 = false;
        pathPiecesConnected = 0;

        Signal.Send("GameManagement", "DisableExitLevelButton", true);

        SetConnectionCounter();

        countdownClock.SetupTimer(currentPPLevel.timeLimitInSeconds, currentPPLevel.parTimeInSeconds);
        countdownClock.StartTimer();

        LoadNextBoard();
    }

    public void CheckTiles(Signal signal)
    {
        for (int i = 0; i < tiles.Count; i++)
            tiles[i].partOfPath = false;

        PathPuzzleTileController startTile = tiles.Find(x => x.start);
        PathPuzzleTileController finishTile = tiles.Find(x => x.finish);

        HashSet<PathPuzzleTileController> checkedSet = 
            new HashSet<PathPuzzleTileController>();

        Stack<PathPuzzleTileController> remainingStack =
            new Stack<PathPuzzleTileController>();

        checkedSet.Add(startTile);
        remainingStack.Push(startTile);

        while (remainingStack.Count > 0)
        {
            PathPuzzleTileController tile = remainingStack.Pop();
            List<PathPuzzleTileController> connections = new List<PathPuzzleTileController>();

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

        foreach (PathPuzzleTileController t in checkedSet)
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
                Debug.Log("Puzzle Finished");

                pathPiecesConnected += checkedSet.Count;
                SetConnectionCounter();

                Helpful.TextPopup(string.Format("+{0}", checkedSet.Count.ToString())
                    , connectionCounter.transform
                    , Vector2.zero
                    , Color.green, popupFont);

                Debug.Log(pathPiecesConnected);

                StartCoroutine(AnimateBoardEnding());
            }
        }
    }

    //Invoked by the countdownClock's onOutOfTime event
    public void OutOfTime()
    {
        won = false;
        EndGame();
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
        countdownClock.SetTime(0f);

        EndGame();
    }

    private IEnumerator AnimateBoardEnding()
    {
        countdownClock.Pause();

        for (int i = 0; i < tiles.Count; i++)
        {
            StartCoroutine(tiles[i].SpinShrink());
        }

        while (tiles.FindIndex(x => x.endAnimation) >= 0)
            yield return null;

        countdownClock.Unpause();
        LoadNextBoard();
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

                PathPuzzleTileController control = go.GetComponent<PathPuzzleTileController>();

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
        }
        else
        {
            won = true;
            EndGame();
        }
    }

    private void EndGame()
    {
        countdownClock.Pause();

        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].GetComponent<UIButton>().interactable = false;
        }

        Signal.Send("GameManagement", "DisableExitLevelButton", false);

        if (won)
        {
            if (!currentPPLevel.objective1)
                currentPPLevel.objective1 = true;

            if (currentPPLevel.nextLevel != null && !currentPPLevel.nextLevel.unlocked)
                currentPPLevel.nextLevel.unlocked = true;
        }

        if (countdownClock.SecondsRemaining >= currentPPLevel.parTimeInSeconds && !currentPPLevel.objective2)
            currentPPLevel.objective2 = true;

        if (pathPiecesConnected >= currentPPLevel.piecesConnectedGoal && !currentPPLevel.objective3)
            currentPPLevel.objective3 = true;

        Invoke("GoToEndScreen", 2.5f);

        Debug.Log("Game End");
    }
    
    //Invoked by EndGame() funtion
    private void GoToEndScreen()
    {
        //Signal Data should be object[9]
        //  index 0 =>  LevelBase       - The level that was just completed/exited
        //  index 1 =>  bool            - true = success, false = exit early/fail
        //  index 2 =>  string          - subtitle text

        object[] data = new object[3];
        data[0] = currentPPLevel;
        data[1] = won;
        data[2] = string.Format("{0} pieces traversed!", pathPiecesConnected.ToString());

        Signal.Send("GameManagement", "LevelEnded", data);
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
}
