using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelObjectivesInfoPageController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI    objective1Text;
    [SerializeField] private TextMeshProUGUI    objective2Text;
    [SerializeField] private TextMeshProUGUI    objective3Text;
    [SerializeField] private GameObject         objective1CompleteMark;
    [SerializeField] private GameObject         objective2CompleteMark;
    [SerializeField] private GameObject         objective3CompleteMark;

    private void Start()
    {
        Setup();
    }

    //Called by the page's OnShow Callback
    public void Setup()
    {
        objective1Text.text = Helpful.GetLevelObjectiveTitles(GameManager.instance.currentLevelOLD, 1);
        objective2Text.text = Helpful.GetLevelObjectiveTitles(GameManager.instance.currentLevelOLD, 2);
        objective3Text.text = Helpful.GetLevelObjectiveTitles(GameManager.instance.currentLevelOLD, 3);

        objective1CompleteMark.GetComponent<Image>().enabled = GameManager.instance.currentLevelOLD.objective1;
        objective2CompleteMark.GetComponent<Image>().enabled = GameManager.instance.currentLevelOLD.objective2;
        objective3CompleteMark.GetComponent<Image>().enabled = GameManager.instance.currentLevelOLD.objective3;
    }
}
