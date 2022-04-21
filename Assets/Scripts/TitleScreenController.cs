using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Signals;

public class TitleScreenController : MonoBehaviour
{
    public float                timeBetweenFlashes = .1f;

    private List<RainbowFlash>  flashers;
    private IEnumerator         flashingCoroutine;

    private void Awake()
    {
        flashers = new List<RainbowFlash>(GetComponentsInChildren<RainbowFlash>());

        flashingCoroutine = null;
    }

    //Called By the Title Screen's Go To Main Button's Click (the invisible button)
    public void StartFlash()
    {
        if (flashingCoroutine == null)
        {
            flashingCoroutine = FlashEveryone();

            StartCoroutine(flashingCoroutine);
        }
        else
        {
            StopCoroutine(flashingCoroutine);

            flashingCoroutine = null;

            SendSignalToLeaveTitle();
        }
    }

    private IEnumerator FlashEveryone()
    {
        for (int i = 0; i < flashers.Count - 1; i++)
        {
            flashers[i].StartFlash();

            yield return new WaitForSeconds(timeBetweenFlashes);
        }

        yield return flashers[flashers.Count - 1].Flash();

        yield return new WaitForSeconds(.7f);

        SendSignalToLeaveTitle();
    }

    private void SendSignalToLeaveTitle()
    {
        Signal.Send("GameManagement", "LeaveTitleScreen");
    }
}
