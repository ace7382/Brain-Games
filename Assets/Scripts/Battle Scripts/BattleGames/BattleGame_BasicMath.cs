using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleGame_BasicMath : BattleGameControllerBase
{
    #region Inspector Variables

    [SerializeField] private TextMeshProUGUI    equationText;
    [SerializeField] private TextMeshProUGUI    solutionText;
    [SerializeField] private GameObject         negativeIndicator;

    #endregion

    #region Private Variables

    private string[]                            equationSymbols = new string[4] {"+", "-", "*", "÷" };
    private int                                 solution;
    private bool                                negativeOn;

    #endregion

    #region Private Properties

    private bool NegativeOn
    {
        get { return negativeOn; }
        set
        {
            if (negativeOn == value)
                return;

            negativeOn = value;
            negativeIndicator.SetActive(negativeOn);
        }
    }

    #endregion

    #region Public Functions

    public override void StartGame()
    {
        NextEquation();
    }

    public override void BoardReset()
    {
        NextEquation();
    }

    public override string GetBattleGameName()
    {
        return Helpful.GetStringFromBattleGameType(Helpful.BattleGameTypes.BasicMath);
    }

    //Called by the Number Buttons' OnClick behavior
    public void NumberButtonClicked(string numberClicked)
    {
        if (solutionText.text.Length >= 9)
            return;

        solutionText.text += numberClicked;
    }

    //Called by the Backspace Button's OnClick behavior
    public void Backspace()
    {
        if (string.IsNullOrEmpty(solutionText.text))
            return;

        solutionText.text = solutionText.text.Remove(solutionText.text.Length - 1);
    }

    //Called by the Submit Button's OnClick behavior
    public void Submit()
    {
        if (string.IsNullOrEmpty(solutionText.text))
            return;

        int currentSolutionTextAsInt = int.Parse(solutionText.text);

        if (NegativeOn)
            currentSolutionTextAsInt *= -1;

        Debug.Log(string.Format("Submitted: {0}. Solution is: {1}. Correct? {2}", currentSolutionTextAsInt
            , solution, solution == currentSolutionTextAsInt));

        if (currentSolutionTextAsInt == solution)
        {
            Signal.Send("Battle", "CorrectResponse");

            AudioManager.instance.Play("Go");

            NextEquation();
        }
        else
        {
            Signal.Send("Battle", "IncorrectResponse");

            AudioManager.instance.Play("No");

            ResetSolutionText();
        }
    }

    //Called by the Negative Button's OnClick behaviot\r
    public void ChangeNegative()
    {
        NegativeOn = !NegativeOn;
    }

    #endregion

    #region Private Functions

    private void NextEquation()
    {
        ResetSolutionText();

        int firstNum                        = -1;
        int secondNum                       = -1;

        string equationType                 = equationSymbols[Random.Range(0, equationSymbols.Length)];

        if (equationType.Equals("+")) //Addition
        {
            firstNum                        = Random.Range(0, 251);
            secondNum                       = Random.Range(0, 251);

            solution                        = firstNum + secondNum;
        }
        else if (equationType.Equals("-")) //Subtraction
        {
            firstNum                        = Random.Range(0, 251);
            secondNum                       = Random.Range(0, 251);

            solution                        = firstNum - secondNum;
        }
        else if (equationType.Equals("*")) //Multiplication
        {
            firstNum                        = Random.Range(0, 251);
            secondNum                       = Random.Range(0, 251);

            solution                        = firstNum * secondNum;
        }
        else if (equationType.Equals("÷")) //Division
        {
            firstNum                        = Random.Range(0, 251);
            List<int> secondNumberChoices   = new List<int>();

            for (int i = 1; i <= Mathf.Sqrt(firstNum); i++)
            {
                if (firstNum % i == 0)
                {
                    secondNumberChoices.Add(i);

                    if (firstNum / i == i)
                        continue;

                    secondNumberChoices.Add(firstNum / i);
                }
            }

            secondNum                       = secondNumberChoices[Random.Range(0, secondNumberChoices.Count)];

            solution                        = firstNum / secondNum;
        }

        equationText.text   = string.Format("{0} {1} {2} = ?", firstNum.ToString(), equationType, secondNum.ToString());
    }

    private void ResetSolutionText()
    {
        NegativeOn          = false;
        solutionText.text   = "";
    }

    #endregion
}
