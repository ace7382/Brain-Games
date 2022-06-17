using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathPuzzleBoard
{
    public int              cellSize;
    public int              columns;
    public int              rows;
    public int              initialStartTileRotations;
    public int              initialFinishTileRotations;

    public List<GameObject> tiles;
}
