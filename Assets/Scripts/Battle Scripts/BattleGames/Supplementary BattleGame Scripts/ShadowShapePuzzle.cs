using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Puzzle", menuName = "New Shadow Shape Puzzle", order = 21)]
public class ShadowShapePuzzle : ScriptableObject
{
    #region Classes

    [System.Serializable]
    public class ShadowShapePieceLayout
    {
        public GameObject   piece;
        public Vector3      rotation;
        public Vector3      position;
        public Vector3      scale;
    }

    #endregion

    #region Inspector Variables

    public List<ShadowShapePieceLayout> solutionPieces;
    public List<ShadowShapePieceLayout> extraPieces;

    #endregion
}
