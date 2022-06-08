using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Doozy.Runtime.UIManager.Components;
using Doozy.Runtime.Signals;

public class BattleGame_ColorDissonance : BattleGameControllerBase
{
    #region Classes

    [System.Serializable]
    private class WordColorCombo
    {
        [SerializeField] private string     word;
        [SerializeField] private Color      color;

        public string                       Word { get { return word; } }
        public Color                        Color { get { return color; } }
    }

    [System.Serializable]
    private class WordShapeCombo
    {
        [SerializeField] private string     word;
        [SerializeField] private Sprite     shape;

        public string                       Word { get { return word; } }
        public Sprite                       Shape { get { return shape; } }
    }

    #endregion

    #region Inspector Variables

    [SerializeField] private TextMeshProUGUI        topPanelText;
    [SerializeField] private TextMeshProUGUI        bottomPanelText;
    [SerializeField] private Image                  bottomPanelShapeImage;
    [SerializeField] private UIButton               doesMatchButton;        //TODO: Make these Button's Less Flat
    [SerializeField] private UIButton               doesNotMatchButton;     //      Right now they don't have the "edge" that most other buttons do

    [Space]

    [SerializeField] private List<WordColorCombo>   colorsAnswerKey;
    [SerializeField] private List<WordShapeCombo>   shapesAnswerKey;

    #endregion

    #region Private Variables

    private bool isShapeQuestion;

    #endregion

    #region Public Function

    public override void StartGame()
    {
        NextSet();
    }

    //Called by the True Button's OnClick Behavior
    public void CheckYes()
    {
        if (isShapeQuestion)
        {
            if (bottomPanelShapeImage.sprite == shapesAnswerKey.Find(x => x.Word == topPanelText.text).Shape)
            {
                Signal.Send("Battle", "CorrectResponse");

                AudioManager.instance.Play("Go");
            }
            else
            {
                Signal.Send("Battle", "IncorrectResponse");

                AudioManager.instance.Play("No");
            }
        }
        else
        {
            if (bottomPanelText.color == colorsAnswerKey.Find(x => x.Word == topPanelText.text).Color)
            {
                Signal.Send("Battle", "CorrectResponse");

                AudioManager.instance.Play("Go");
            }
            else
            {
                Signal.Send("Battle", "IncorrectResponse");

                AudioManager.instance.Play("No");
            }
        }

        NextSet();
    }

    //Called by the False Button's OnClick Behavior
    public void CheckNo()
    {
        if (isShapeQuestion)
        {
            if (bottomPanelShapeImage.sprite != shapesAnswerKey.Find(x => x.Word == topPanelText.text).Shape)
            {
                Signal.Send("Battle", "CorrectResponse");

                AudioManager.instance.Play("Go");
            }
            else
            {
                Signal.Send("Battle", "IncorrectResponse");

                AudioManager.instance.Play("No");
            }
        }
        else
        {
            if (bottomPanelText.color != colorsAnswerKey.Find(x => x.Word == topPanelText.text).Color)
            {
                Signal.Send("Battle", "CorrectResponse");

                AudioManager.instance.Play("Go");
            }
            else
            {
                Signal.Send("Battle", "IncorrectResponse");

                AudioManager.instance.Play("No");
            }
        }

        NextSet();
    }

    #endregion

    #region Private Functions

    private void NextSet()
    {
        if (Random.Range(0,2) > 0) //Color Question
        {
            bottomPanelShapeImage.gameObject.SetActive(false);
            bottomPanelText.gameObject.SetActive(true);

            isShapeQuestion             = false;

            int leftPanelColorIndex     = Random.Range(0, colorsAnswerKey.Count);
            int leftPanelWordIndex      = Random.Range(0, colorsAnswerKey.Count);

            int rightPanelColorIndex    = Random.Range(0, colorsAnswerKey.Count);
            int rightPanelWordIndex     = Random.Range(0, colorsAnswerKey.Count);

            topPanelText.color          = colorsAnswerKey[leftPanelColorIndex].Color;
            topPanelText.text           = colorsAnswerKey[leftPanelWordIndex].Word;

            bottomPanelText.color       = colorsAnswerKey[rightPanelColorIndex].Color;
            bottomPanelText.text        = colorsAnswerKey[rightPanelWordIndex].Word;
        }
        else //Shape
        {
            bottomPanelShapeImage.gameObject.SetActive(true);
            bottomPanelText.gameObject.SetActive(false);

            isShapeQuestion                 = true;

            int leftPanelColorIndex         = Random.Range(0, colorsAnswerKey.Count);
            int leftPanelWordIndex          = Random.Range(0, shapesAnswerKey.Count);

            int rightPanelShapeIndex        = Random.Range(0, shapesAnswerKey.Count);
            int rightPanelColorIndex        = Random.Range(0, colorsAnswerKey.Count);

            topPanelText.color              = colorsAnswerKey[leftPanelColorIndex].Color;
            topPanelText.text               = shapesAnswerKey[leftPanelWordIndex].Word;

            bottomPanelShapeImage.color     = colorsAnswerKey[rightPanelColorIndex].Color;
            bottomPanelShapeImage.sprite    = shapesAnswerKey[rightPanelShapeIndex].Shape;
        }
    }

    #endregion
}
