using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Signals;

public class TitleScreenController : MonoBehaviour
{
    public float timeBetweenFlashes = .1f;
    public List<RainbowFlash> flashers;

    private void Awake()
    {
        flashers = new List<RainbowFlash>(GetComponentsInChildren<RainbowFlash>());
    }

    public void StartFlash()
    {
        StartCoroutine(FlashEveryone());
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

        Signal.Send("GameManagement", "LeaveTitleScreen");
    }
}
