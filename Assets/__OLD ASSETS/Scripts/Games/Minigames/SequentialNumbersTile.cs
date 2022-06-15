using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SequentialNumbersTile : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Image              buttonBG;
    [SerializeField] private TextMeshProUGUI    numberDisplay;
    [SerializeField] private UIButton           uiButton;

    #endregion

    #region Private Variables

    private decimal                             number;
    private int                                 solutionIndex;

    #endregion

    #region Public Properties

    public decimal Number                       { get { return number; } }
    public int SolutionIndex                    { get { return solutionIndex; } }
    public bool Showing                         { get { return buttonBG.enabled; } }

    #endregion

    #region Public Functions

    public void Setup(decimal n, int sol)
    {
        number              = n;
        solutionIndex       = sol;
        numberDisplay.text  = number.ToString();
    }

    //Called by the tile button's OnClick behavior
    public void OnClick()
    {
        Signal.Send("SequentialNumbers", "TileClicked", this);
    }

    public void HideTile()
    {
        //Debug.Log("hiding tile : " + name + " value: " + number.ToString());

        buttonBG.enabled        = false;
        numberDisplay.enabled   = false;
        uiButton.enabled        = false;

        solutionIndex           = -1;

        Canvas.ForceUpdateCanvases();
    }

    public void ShowTile()
    {
        buttonBG.enabled        = true;
        numberDisplay.enabled   = true;
        uiButton.enabled        = true;
        uiButton.interactable   = true;

        transform.localScale    = Vector3.one;
        transform.eulerAngles   = Vector4.zero;

        Canvas.ForceUpdateCanvases();
    }

    #endregion
}
