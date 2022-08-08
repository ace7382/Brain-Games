using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Components;
using TMPro;
using UnityEngine;

public class MinigameSelectButtonController : MonoBehaviour
{
    public Minigame         minigame;

    [SerializeField] private GameObject     lockIcon;
    [SerializeField] private GameObject     minigameName;
    [SerializeField] private UIButton       button;

    //Called by the Minigame Button's OnClick behavior
    public void Click()
    {
        if (!minigame.unlocked)
            return;

        //GameManager.instance.SetMinigame(minigame);

        AudioManager.instance.Play("Button Click");

        Signal.Send("GameManagement", "LoadMinigameScene", minigame.minigameID);
    }

    public void ShowLockedStatus()
    {
        if (minigame.unlocked)
        {
            lockIcon.SetActive(false);
            minigameName.SetActive(true);
            minigameName.GetComponent<TextMeshProUGUI>().text = minigame.name;
            button.interactable = true;
        }
        else
        {
            lockIcon.SetActive(true);
            minigameName.SetActive(false);
            button.interactable = false;
        }
    }
}
