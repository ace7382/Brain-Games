using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialInfoPageController : MonoBehaviour
{
    //Called by Tutorial Page's animation end's callback
    public void EnablePageTurnAgain()
    {
        Signal.Send("TutorialScreen", "PageLoaded");
    }
}
