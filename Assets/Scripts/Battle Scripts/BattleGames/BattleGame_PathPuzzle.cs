using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleGame_PathPuzzle : BattleGameControllerBase
{
    #region Inspector Variables

    [SerializeField] private Transform  gameBoard;

    #endregion

    #region Private Variables

    private int                         currentBoardIndex;
    private List<PathPuzzleTile>        tiles;

    #endregion

    #region Signal Variables

    private SignalReceiver              pathpuzzle_tilerotated_receiver;
    private SignalStream                pathpuzzle_tilerotated_stream;

    #endregion

    #region Unity Functions

    protected override void Awake()
    {
        pathpuzzle_tilerotated_stream   = SignalStream.Get("PathPuzzle", "TileRotated");

        pathpuzzle_tilerotated_receiver = new SignalReceiver().SetOnSignalCallback(CheckTiles);
    }

    protected override void OnEnable()
    {
        pathpuzzle_tilerotated_stream.ConnectReceiver(pathpuzzle_tilerotated_receiver);
    }

    protected override void OnDisable()
    {
        pathpuzzle_tilerotated_stream.DisconnectReceiver(pathpuzzle_tilerotated_receiver);
    }

    private void OnDestroy()
    {
        OnDisable();
    }

    #endregion

    #region Public Functions

    public override void StartGame()
    {
        currentBoardIndex   = -1;
        tiles               = new List<PathPuzzleTile>();

        NextBoard();
    }

    public override void BoardReset()
    {
        throw new System.NotImplementedException();
    }

    public override string GetBattleGameName()
    {
        return Helpful.GetStringFromBattleGameType(Helpful.BattleGameTypes.PathPuzzle);
    }

    #endregion

    #region Private Functions

    private void NextBoard()
    {
        currentBoardIndex++;

        if (currentBoardIndex >= BattleManager.instance.CurrentEnemy.UnitInfo.PathPuzzleBoards.Count)
        {
            currentBoardIndex = 0;
        }

        PathPuzzleBoard currentBoard    = BattleManager.instance.CurrentEnemy.UnitInfo.PathPuzzleBoards[currentBoardIndex];

        foreach (Transform child in gameBoard)
            Destroy(child.gameObject);

        tiles.Clear();

        GridLayoutGroup glg     = gameBoard.GetComponent<GridLayoutGroup>();
        glg.constraintCount     = currentBoard.columns;
        glg.cellSize            = new Vector2(currentBoard.cellSize, currentBoard.cellSize);

        for (int i = 0; i < currentBoard.tiles.Count; i++)
        {
            GameObject go                                   = Instantiate(currentBoard.tiles[i]);

            go.transform.SetParent(gameBoard.transform);
            go.transform.localScale                         = Vector3.one;

            PathPuzzleTile control                          = go.GetComponent<PathPuzzleTile>();

            control.GridPosition                            = new Vector2(i % currentBoard.columns, i / currentBoard.columns);

            if (control.Start)
            {
                control.SetInitialRotation(currentBoard.initialStartTileRotations);
            }
            else if (control.Finish)
            {
                control.SetInitialRotation(currentBoard.initialFinishTileRotations);
            }
            else if (!control.Nonpath)
            {
                control.SetInitialRotation(Random.Range(1, 4));
            }

            tiles.Add(control);
        }

        CheckTiles(new Signal());
    }

    private void CheckTiles(Signal signal)
    {
        for (int i = 0; i < tiles.Count; i++)
            tiles[i].PartOfPath = false;

        PathPuzzleTile startTile                = tiles.Find(x => x.Start);
        PathPuzzleTile finishTile               = tiles.Find(x => x.Finish);

        HashSet<PathPuzzleTile> checkedSet      = new HashSet<PathPuzzleTile>();

        Stack<PathPuzzleTile> remainingStack    = new Stack<PathPuzzleTile>();

        checkedSet.Add(startTile);
        remainingStack.Push(startTile);

        while (remainingStack.Count > 0)
        {
            PathPuzzleTile tile                 = remainingStack.Pop();
            List<PathPuzzleTile> connections    = new List<PathPuzzleTile>();

            if (tile.North)
            {
                if (tiles.Exists(findTile => 
                    (findTile.GridPosition == new Vector2(tile.GridPosition.x, tile.GridPosition.y - 1)) 
                    && findTile.South))
                {
                    connections.Add(tiles.Find(findTile =>
                        (findTile.GridPosition == new Vector2(tile.GridPosition.x, tile.GridPosition.y - 1))
                        && findTile.South));
                }
            }

            if (tile.East)
            {
                if (tiles.Exists(findTile =>
                    (findTile.GridPosition == new Vector2(tile.GridPosition.x + 1, tile.GridPosition.y))
                    && findTile.West))
                {
                    connections.Add(tiles.Find(findTile =>
                        (findTile.GridPosition == new Vector2(tile.GridPosition.x + 1, tile.GridPosition.y))
                        && findTile.West));
                }
            }

            if (tile.South)
            {
                if (tiles.Exists(findTile =>
                    (findTile.GridPosition == new Vector2(tile.GridPosition.x, tile.GridPosition.y + 1))
                    && findTile.North))
                {
                    connections.Add(tiles.Find(findTile =>
                        (findTile.GridPosition == new Vector2(tile.GridPosition.x, tile.GridPosition.y + 1))
                        && findTile.North));
                }
            }

            if (tile.West)
            {
                if (tiles.Exists(findTile =>
                    (findTile.GridPosition == new Vector2(tile.GridPosition.x - 1, tile.GridPosition.y))
                    && findTile.East))
                {
                    connections.Add(tiles.Find(findTile =>
                        (findTile.GridPosition == new Vector2(tile.GridPosition.x - 1, tile.GridPosition.y))
                        && findTile.East));
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

        foreach (PathPuzzleTile t in checkedSet)
            t.PartOfPath = true;

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

                tiles.Find(x => !x.Start && !x.Finish && !x.Nonpath && x.PartOfPath).SetInitialRotation(1);
                CheckTiles(new Signal());
            }
            else
            {
                Signal.Send("Battle", "AbilityChargeGenerated", AbilityCharger.AbilityChargeActions.CORRECT_RESPONSE);
                NextBoard();
            }
        }
    }
    #endregion
}
