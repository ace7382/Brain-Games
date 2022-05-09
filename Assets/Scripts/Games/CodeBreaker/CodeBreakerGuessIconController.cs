using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CodeBreakerGuessIconController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Image correctnessIndicator;
    [SerializeField] private Image icon;

    #endregion

    #region Public Properties

    public Image CorrectnessIndicator   { get { return correctnessIndicator; } }
    public Image Icon                   { get { return icon; } }

    #endregion
}
