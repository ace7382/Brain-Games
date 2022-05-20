using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorDissonanceController : TimedMinigameController
{
    #region Classes

    [System.Serializable]
    private class WordColorCombo
    {
        [SerializeField] private string             word;
        [SerializeField] private Color              color;

        public string Word      { get { return word; } }
        public Color Color      { get { return color; } }
    }

    [System.Serializable]
    private class WordShapeCombo
    {
        [SerializeField] private string             word;
        [SerializeField] private Sprite             shape;

        public string Word      { get { return word; } }
        public Sprite Shape     { get { return shape; } }
    }

    #endregion

    #region Inspector Variables

    [SerializeField] private TextMeshProUGUI        leftPanelText;
    [SerializeField] private TextMeshProUGUI        rightPanelText;
    [SerializeField] private Image                  rightPanelShapeImage;
    [SerializeField] private UIButton               doesMatchButton;
    [SerializeField] private UIButton               doesNotMatchButton;

    [Space]

    [SerializeField] private List<WordColorCombo>   colorsAnswerKey;
    [SerializeField] private List<WordShapeCombo>   shapesAnswerKey;

    #endregion

    #region Private Variables

    private bool isShapeQuestion;

    #endregion

    #region Signal Variables

    //See base class for signal variables

    #endregion

    #region Unity Functions

    //See Base class for unity functions

    #endregion

    #region Public Functions

    ////Called by the GamePlay view's Shown callback
    //public void StartGame()

    ////Invoked by the Countdown Clock's OnOutOfTime Event
    //public void EndGame()

    //Called by the Does Match Button's OnClick Behavior
    public void CheckYes()
    {
        if (isShapeQuestion)
        {
            if (rightPanelShapeImage.sprite == shapesAnswerKey.Find(x => x.Word == leftPanelText.text).Shape)
            {
                correctResponses++;

                AudioManager.instance.Play("Go");
            }
            else
            {
                incorrectResponses++;

                AudioManager.instance.Play("No");
            }
        }
        else
        {
            if (rightPanelText.color == colorsAnswerKey.Find(x => x.Word == leftPanelText.text).Color)
            {
                correctResponses++;

                AudioManager.instance.Play("Go");
            }
            else
            {
                incorrectResponses++;

                AudioManager.instance.Play("No");
            }
        }

        NextSet();
    }

    //Called by the Does NOT Match Button's OnClick Behavior
    public void CheckNo()
    {
        if (isShapeQuestion)
        {
            if (rightPanelShapeImage.sprite != shapesAnswerKey.Find(x => x.Word == leftPanelText.text).Shape)
            {
                correctResponses++;

                AudioManager.instance.Play("Go");
            }
            else
            {
                incorrectResponses++;

                AudioManager.instance.Play("No");
            }
        }
        else
        {
            if (rightPanelText.color != colorsAnswerKey.Find(x => x.Word == leftPanelText.text).Color)
            {
                correctResponses++;

                AudioManager.instance.Play("Go");
            }
            else
            {
                incorrectResponses++;

                AudioManager.instance.Play("No");
            }
        }

        NextSet();
    }

    #endregion

    #region Override Funtions

    protected override void Setup(Signal signal)
    {
        base.Setup(signal);

        doesMatchButton.interactable    = true;
        doesNotMatchButton.interactable = true;

        isShapeQuestion                 = false;

        NextSet();
    }

    public override void EndGame()
    {
        doesMatchButton.interactable    = false;
        doesNotMatchButton.interactable = false;

        base.EndGame();
    }

    #endregion

    #region Private Functions

    private void NextSet()
    {
        if (Random.Range(0,2) > 0) //Color Question
        {
            rightPanelShapeImage.gameObject.SetActive(false);
            rightPanelText.gameObject.SetActive(true);

            isShapeQuestion             = false;

            int leftPanelColorIndex     = Random.Range(0, colorsAnswerKey.Count);
            int leftPanelWordIndex      = Random.Range(0, colorsAnswerKey.Count);

            int rightPanelColorIndex    = Random.Range(0, colorsAnswerKey.Count);
            int rightPanelWordIndex     = Random.Range(0, colorsAnswerKey.Count);

            leftPanelText.color         = colorsAnswerKey[leftPanelColorIndex].Color;
            leftPanelText.text          = colorsAnswerKey[leftPanelWordIndex].Word;

            rightPanelText.color        = colorsAnswerKey[rightPanelColorIndex].Color;
            rightPanelText.text         = colorsAnswerKey[rightPanelWordIndex].Word;
        }
        else //Shape
        {
            rightPanelShapeImage.gameObject.SetActive(true);
            rightPanelText.gameObject.SetActive(false);

            isShapeQuestion             = true;

            int leftPanelColorIndex     = Random.Range(0, colorsAnswerKey.Count);
            int leftPanelWordIndex      = Random.Range(0, shapesAnswerKey.Count);

            int rightPanelShapeIndex    = Random.Range(0, shapesAnswerKey.Count);
            int rightPanelColorIndex    = Random.Range(0, colorsAnswerKey.Count);

            leftPanelText.color         = colorsAnswerKey[leftPanelColorIndex].Color;
            leftPanelText.text          = shapesAnswerKey[leftPanelWordIndex].Word;

            rightPanelShapeImage.color  = colorsAnswerKey[rightPanelColorIndex].Color;
            rightPanelShapeImage.sprite = shapesAnswerKey[rightPanelShapeIndex].Shape;
        }
    }
    
    #endregion
}
