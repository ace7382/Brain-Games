using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Runtime.Signals;

public class PathPuzzleController : MonoBehaviour
{
    public PathPuzzleLevel                  currentPPLevel;
    public int                              currentBoardNum;
    public GameObject                       gameBoard;

    public List<PathPuzzleTileController>   tiles;

    private SignalReceiver                  pathpuzzle_tilerotated_receiver;
    private SignalStream                    pathpuzzle_tilerotated_stream;

    private void Awake()
    {
        pathpuzzle_tilerotated_stream = SignalStream.Get("PathPuzzle", "TileRotated");

        pathpuzzle_tilerotated_receiver = new SignalReceiver().SetOnSignalCallback(CheckTiles);
    }

    private void OnEnable()
    {
        pathpuzzle_tilerotated_stream.ConnectReceiver(pathpuzzle_tilerotated_receiver);
    }

    private void OnDisable()
    {
        pathpuzzle_tilerotated_stream.DisconnectReceiver(pathpuzzle_tilerotated_receiver);
    }

    public void Setup()
    {
        gameBoard.GetComponent<GridLayoutGroup>().constraintCount = currentPPLevel.boards[0].columns;

        for (int i = 0; i < currentPPLevel.boards[0].tiles.Count; i++)
        {
            GameObject go = Instantiate(currentPPLevel.boards[0].tiles[i]);
            
            go.transform.SetParent(gameBoard.transform);
            go.transform.localScale = Vector3.one;

            PathPuzzleTileController control = go.GetComponent<PathPuzzleTileController>();

            control.gridPosition = new Vector2(i % currentPPLevel.boards[0].columns, i / currentPPLevel.boards[0].columns);
            control.SetInitialRotation(Random.Range(0, 4));

            tiles.Add(control);
        }

        CheckTiles(null);
    }

    public void CheckTiles(Signal signal)
    {
        for (int i = 0; i < tiles.Count; i++)
            tiles[i].partOfPath = false;

        PathPuzzleTileController startTile = tiles.Find(x => x.start);

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
    }
}
