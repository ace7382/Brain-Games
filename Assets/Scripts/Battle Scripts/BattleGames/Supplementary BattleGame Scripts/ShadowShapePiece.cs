using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShadowShapePiece : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Image      pieceImage;

    #endregion

    #region Public Variables

    public bool       isSolution = false;
    public int        id;

    #endregion
}
