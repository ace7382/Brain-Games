using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseButtonController : MonoBehaviour
{
    //Called by the Pause Button and Unpause Button's OnClick Behaviors
    public void OnClick(bool isPausing)
    {
        Signal.Send("Battle", isPausing ? "Pause" : "Unpause");
    }
}
