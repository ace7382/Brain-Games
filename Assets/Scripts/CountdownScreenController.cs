using System.Collections;
using System;
using TMPro;
using UnityEngine;
using Doozy.Runtime.Signals;

public class CountdownScreenController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI t;
    
    //Called by the GameManagement.Countdown View's End Show Animation callback
    public void StartCountdown()
    {
        StartCoroutine(Countdown());
    }

    //Called by the GameManagement.Countdown View's End Hide Animation callback
    public void OnHidden()
    {
        t.text = "3";
    }

    private IEnumerator Countdown()
    {
        WaitForSeconds w = new WaitForSeconds(1);

        t.text = "3";
        AudioManager.instance.Play("Countdown Tick");

        yield return w;

        t.text = "2";
        AudioManager.instance.Play("Countdown Tick");

        yield return w;

        t.text = "1";
        AudioManager.instance.Play("Countdown Tick");

        yield return w;

        t.text = "Go!";
        AudioManager.instance.Play("Go");

        yield return w;

        Signal.Send("GameManagement", "CountdownEnded");
    }
}
