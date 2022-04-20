using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Doozy.Runtime.Signals;
using Doozy.Runtime.Reactor;

public class PathPuzzleController : MonoBehaviour
{
    public PathPuzzleLevel                  currentPPLevel;
    public int                              currentBoardNum;

    [Space]
    [Space]

    public GameObject                       gameBoard;
    public CountdownClockController         countdownClock;

    public List<PathPuzzleTileController>   tiles;

    private SignalReceiver                  pathpuzzle_tilerotated_receiver;
    private SignalStream                    pathpuzzle_tilerotated_stream;
    private SignalReceiver                  pathpuzzle_pathpuzzlesetup_receiver;
    private SignalStream                    pathpuzzle_pathpuzzlesetup_stream;

    private void Awake()
    {
        pathpuzzle_tilerotated_stream = SignalStream.Get("PathPuzzle", "TileRotated");
        pathpuzzle_pathpuzzlesetup_stream = SignalStream.Get("PathPuzzle", "PathPuzzleSetup");

        pathpuzzle_tilerotated_receiver = new SignalReceiver().SetOnSignalCallback(CheckTiles);
        pathpuzzle_pathpuzzlesetup_receiver = new SignalReceiver().SetOnSignalCallback(Setup);
    }

    private void OnEnable()
    {
        pathpuzzle_tilerotated_stream.ConnectReceiver(pathpuzzle_tilerotated_receiver);
        pathpuzzle_pathpuzzlesetup_stream.ConnectReceiver(pathpuzzle_pathpuzzlesetup_receiver);
    }

    private void OnDisable()
    {
        pathpuzzle_tilerotated_stream.DisconnectReceiver(pathpuzzle_tilerotated_receiver);
        pathpuzzle_pathpuzzlesetup_stream.DisconnectReceiver(pathpuzzle_pathpuzzlesetup_receiver);
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

        currentBoardNum = -1;

        countdownClock.SetupTimer(currentPPLevel.timeLimitInSeconds, currentPPLevel.parTimeInSeconds);
        countdownClock.StartTimer();

        LoadNextBoard();

        CheckTiles(null);
    }

    public void CheckTiles(Signal signal) //Signal isn't used
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
            Debug.Log("Puzzle Finished");
            LoadNextBoard();
        }
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

            CheckTiles(null);
        }
        else
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        Debug.Log("Game End");
    }
}
