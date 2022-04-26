using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPageController : MonoBehaviour
{
    //Called by Tutorial Page's animation end's callback
    public void EnablePageTurnAgain()
    {
        Debug.Log(name + "'s show animation finished.");

        Signal.Send("TutorialScreen", "PageLoaded");
    }
}
