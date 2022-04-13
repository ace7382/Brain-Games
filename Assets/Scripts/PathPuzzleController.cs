using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathPuzzleController : MonoBehaviour
{
    public List<PathPuzzleTileController> tiles;

    public void CheckTiles()
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
    }
}
