using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Reactor;
using TMPro;

public class aa_Test : MonoBehaviour
{
    public Progressor p;
    public TextMeshProUGUI timeDisplay;
    public float secondsRemaining = 75.0f;
    public float clockMaxSeconds = 60.0f;

    void Update()
    {
        secondsRemaining = Mathf.Clamp(secondsRemaining - Time.deltaTime, 0f, float.MaxValue);

        float percentFill = Mathf.Clamp((secondsRemaining / clockMaxSeconds), 0f, float.MaxValue) ;

        p.SetProgressAt(percentFill);

        UpdateTime();
    }

    private void UpdateTime()
    {
        System.TimeSpan ts = System.TimeSpan.FromSeconds(secondsRemaining);
        timeDisplay.text = ts.Minutes + ":" + ts.Seconds.ToString("00");
    }
}
