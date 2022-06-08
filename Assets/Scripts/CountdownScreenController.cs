using System.Collections;
using System;
using TMPro;
using UnityEngine;
using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Containers;

public class CountdownScreenController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI t;

    private void Start()
    {
        Canvas c        = GetComponentInParent<Canvas>();
        Debug.Log(c.gameObject.name);
        c.worldCamera   = Camera.main;
        c.sortingOrder  = UniversalInspectorVariables.instance.popupScreenOrderInLayer;
    }

    //Called by the GameManagement.Countdown View's End Show Animation callback
    public void StartCountdown()
    {
        t.text = "3";
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        WaitForSeconds w    = new WaitForSeconds(1);

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

        Signal.Send("Battle", "CountdownEnded");
    }
}
