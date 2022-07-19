using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberCardController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Image              characterImage;
    [SerializeField] private TextMeshProUGUI    characterName;
    [SerializeField] private TextMeshProUGUI    hpText;
    [SerializeField] private Image              hpBarFill;
    [SerializeField] private TextMeshProUGUI    expText;
    [SerializeField] private Image              expBarFill;
    [SerializeField] private TextMeshProUGUI    levelNumber;

    #endregion

    #region Private Variables

    private Unit                                unitToDisplay;

    #endregion

    #region Public Properties

    public Unit UnitDisplayed
    {
        get { return unitToDisplay; }
    }

    #endregion

    #region Public Functions

    public void Setup(Unit unit)
    {
        unitToDisplay           = unit;

        characterImage.sprite   = unitToDisplay.InBattleSprite;
        characterName.text      = unitToDisplay.Name;

        hpText.text             = string.Format("{0} / {1}", unitToDisplay.CurrentHP, unitToDisplay.MaxHP);
        hpBarFill.fillAmount    = unitToDisplay.CurrentHP / unitToDisplay.MaxHP;
    }

    //Called by OnClick Behavior
    public void LoadDetailsPage()
    {
        CharacterDetailsScreenController cd = FindObjectOfType<CharacterDetailsScreenController>();

        if (cd != null)
            cd.Setup(unitToDisplay);
        else
            Debug.Log("Character Detail Screen not found");
    }

    #endregion
}
