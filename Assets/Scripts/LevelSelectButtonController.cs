using UnityEngine;
using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Components;

public class LevelSelectButtonController : MonoBehaviour
{
    public LevelBase                        level;

    [SerializeField] private GameObject     lockIcon;
    [SerializeField] private GameObject     levelNumber;
    [SerializeField] private UIButton       button;
    [SerializeField] private GameObject     objective1Dot;
    [SerializeField] private GameObject     objective2Dot;
    [SerializeField] private GameObject     objective3Dot;

    //Called By Level Buttons' PointerClick behavior
    public void Click()
    {
        if (!level.unlocked)
            return;

        GameManager.instance.SetLevel(level);

        int gameID = Helpful.GetGameID(level.GetType());

        AudioManager.instance.Play("Button Click");

        if (gameID >= 0)
            Signal.Send("GameManagement", "LoadLevelScene", gameID);
    }

    public void ShowLockedStatus()
    {
        if (level.unlocked)
        {
            lockIcon.SetActive(false);
            levelNumber.SetActive(true);
            button.interactable = true;

            objective1Dot.SetActive(level.objective1);
            objective2Dot.SetActive(level.objective2);
            objective3Dot.SetActive(level.objective3);
        }
        else
        {
            lockIcon.SetActive(true);
            levelNumber.SetActive(false);
            button.interactable = false;
        }
    }
}
