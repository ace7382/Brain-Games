using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Path Puzzle Level", menuName = "New Path Puzzle Level", order = 54)]
public class PathPuzzleLevel : LevelBase
{
    [System.Serializable]
    public class PathPuzzleBoard
    {
        public int                  cellSize;
        public int                  columns;
        public int                  rows;

        public int                  initialStartTileRotations;
        public int                  initialFinishTileRotations;

        public List<GameObject>     tiles;
    }

    public float                    timeLimitInSeconds;
    public float                    parTimeInSeconds;
    public List<PathPuzzleBoard>    boards;

}
