using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDetails_StatDisplay_Controller : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private TextMeshProUGUI    statValueText;
    [SerializeField] private TextMeshProUGUI    statBarText;
    [SerializeField] private TextMeshProUGUI    statLabelText;
    [SerializeField] private RectTransform      statGrowthRateIndicatorContainer;
    [SerializeField] private Image              statBarMask;

    #endregion

    #region Private Variables

    private Helpful.StatTypes                   displayedStatType;

    #endregion

    #region Public Properties

    public Helpful.StatTypes DisplayedStatType
    {
        get { return displayedStatType; }
    }

    #endregion

    #region Public Functions

    public void Setup(Helpful.StatTypes statType, string statLabel, int statValue, int currentEXP, int nextLevelExp, int growthRateIndicatorNum)
    {
        displayedStatType       = statType;
        statLabelText.text      = statLabel;
        statValueText.text      = statValue.ToString();
        statBarText.text        = string.Format("{0} / {1}", currentEXP.ToString(), nextLevelExp.ToString());
        statBarMask.fillAmount  =  nextLevelExp == 0f ? 0f : (float)((float)currentEXP / (float)nextLevelExp);

        for (int i = 0; i < growthRateIndicatorNum; i++)
        {
            Instantiate(FindObjectOfType<CharacterDetailsScreenController>().GrowthRateIndicatorPrefab
                , statGrowthRateIndicatorContainer);
        }
    }

    public void UpdateDisplay(int statValue, int currentEXP, int nextLevelEXP, int growthRateIndicatorNum)
    {
        //TODO - make all fo these into progressors and have them fill up/empty out based on the amounts
        //      so that it's less jarring

        foreach (Transform child in statGrowthRateIndicatorContainer)
            Destroy(child.gameObject);

        Setup(displayedStatType, statLabelText.text, statValue, currentEXP, nextLevelEXP, growthRateIndicatorNum);
    }

    #endregion
}
