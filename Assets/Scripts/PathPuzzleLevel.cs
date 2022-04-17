using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Path Puzzle Level", menuName = "New Path Puzzle Level", order = 54)]
public class PathPuzzleLevel : ScriptableObject
{
    [System.Serializable]
    public class PathPuzzleBoard
    {
        public int                  columns;
        public int                  rows;

        public List<GameObject>     tiles;
    }

    public float                    timeLimitInSeconds;
    public List<PathPuzzleBoard>    boards;

}
