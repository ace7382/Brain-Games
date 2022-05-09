using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodeBreakerGuessListItemController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private TextMeshProUGUI    attemptNumText;
    [SerializeField] private GameObject         iconPanel;

    #endregion

    #region Public Functions

    public void Setup(GameObject prefab, int attemptNum, List<CodeBreakerChoicesInfo.CodebreakerChoices> choices, List<int> solutionIndicators)
    {
        attemptNumText.text = string.Format("#{0}", attemptNum <= 9999 ? attemptNum.ToString() : "X__X");

        Debug.Log(string.Format("Choices Count {0}", choices.Count));

        for (int i = 0; i < choices.Count; i++)
        {
            GameObject go = Instantiate(prefab, iconPanel.transform);
            go.transform.localScale = Vector3.one;

            CodeBreakerGuessIconController control = go.GetComponent<CodeBreakerGuessIconController>();

            switch (solutionIndicators[i])
            {
                case 0:
                    control.CorrectnessIndicator.color = Color.green;
                    break;
                case 1:
                    control.CorrectnessIndicator.color = Color.yellow;
                    break;
                default:
                    control.CorrectnessIndicator.color = Color.clear;
                    break;
            }

            Image pleaseWork = control.Icon;

            CodeBreakerChoicesInfo.GetChoiceImage(choices[i], ref pleaseWork);
        }
    }

    #endregion
}
