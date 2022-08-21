using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.UIManager.Containers;
using UnityEngine.UI;

public class CharacterDetailsScreenController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private GameObject                     statDetailPrefab;
    [SerializeField] private RectTransform                  statDetailsListContainer;
    [SerializeField] private Image                          characterImage;
    [SerializeField] private GameObject                     growthRateIndicatorPrefab;

    #endregion

    #region Private Variables

    private Unit                                            displayedUnit;
    private List<CharacterDetails_StatDisplay_Controller>   statDisplays;

    #endregion

    #region Public Properties

    public GameObject                                       GrowthRateIndicatorPrefab { get { return growthRateIndicatorPrefab; } }
    #endregion

    #region Unity Functions

    private void Awake()
    {
        displayedUnit   = null;
        statDisplays    = new List<CharacterDetails_StatDisplay_Controller>();
    }

    #endregion

    #region Public Functions

    public void Setup(Unit u)
    {
        if (displayedUnit != null)
        {
            UpdateDetailScreen(u);
            GetComponent<UIView>().Show();
            return;
        }

        displayedUnit       = u;

        for (int i = 0; i < (int)Helpful.StatTypes.COUNT; i++)
        {
            GameObject go                                       = Instantiate(statDetailPrefab, statDetailsListContainer);
            go.transform.localPosition                          = Vector3.zero;
            go.transform.localScale                             = Vector3.one;

            CharacterDetails_StatDisplay_Controller controller  = go.GetComponent<CharacterDetails_StatDisplay_Controller>();

            Helpful.StatTypes curretStat                        = (Helpful.StatTypes)i;

            controller.Setup(
                curretStat
                , displayedUnit.GetStat(curretStat)
                , displayedUnit.GetExpForStat(curretStat)
                , displayedUnit.GetEXPNextLevelValue(curretStat)
                , displayedUnit.GetGrowthRate(curretStat)
            );

            statDisplays.Add(controller);
        }

        characterImage.sprite = displayedUnit.InBattleSprite;

        GetComponent<UIView>().Show();
    }

    //Called by the Next Button's OnClick Behavior
    public void Next()
    {
        InventoryScreenController inventoryScreen = FindObjectOfType<InventoryScreenController>();

        if (inventoryScreen == null)
            Debug.Log("Inventory Screen list not found");

        int currentUnitListIndex = inventoryScreen.PartyCards.FindIndex(x => x.UnitDisplayed == displayedUnit);

        if (currentUnitListIndex < 0)
            Debug.Log("Couldn't find Unit in Inventory Card list");

        currentUnitListIndex++;

        if (currentUnitListIndex >= inventoryScreen.PartyCards.Count)
            currentUnitListIndex = 0;

        UpdateDetailScreen(inventoryScreen.PartyCards[currentUnitListIndex].UnitDisplayed);
    }

    //Called by the Previous Button's OnClick Behavior
    public void Previous()
    {
        InventoryScreenController inventoryScreen = FindObjectOfType<InventoryScreenController>();

        if (inventoryScreen == null)
            Debug.Log("Inventory Screen list not found");

        int currentUnitListIndex = inventoryScreen.PartyCards.FindIndex(x => x.UnitDisplayed == displayedUnit);

        if (currentUnitListIndex < 0)
            Debug.Log("Couldn't find Unit in Inventory Card list");

        currentUnitListIndex--;

        if (currentUnitListIndex < 0)
            currentUnitListIndex = inventoryScreen.PartyCards.Count - 1;

        UpdateDetailScreen(inventoryScreen.PartyCards[currentUnitListIndex].UnitDisplayed);
    }

    #endregion

    #region

    private void UpdateDetailScreen(Unit u)
    {
        displayedUnit = u;

        for (int i = 0; i < statDisplays.Count; i++)
        {
            Helpful.StatTypes currentStat = statDisplays[i].DisplayedStatType;

            statDisplays[i].UpdateDisplay(
                displayedUnit.GetStat(currentStat)
                , displayedUnit.GetExpForStat(currentStat)
                , displayedUnit.GetEXPNextLevelValue(currentStat)
                , displayedUnit.GetGrowthRate(currentStat)
            );
        }

        characterImage.sprite = displayedUnit.InBattleSprite;
    }

    #endregion
}
