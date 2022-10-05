using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseButtonController : MonoBehaviour
{
    //Called by the Pause Button's OnClick Behavior
    public void OnClick()
    {
        Signal.Send("Battle", "Pause");
    }
}
