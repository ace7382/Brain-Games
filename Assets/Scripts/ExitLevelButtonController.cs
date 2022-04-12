using UnityEngine;
using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Containers;

public class ExitLevelButtonController : MonoBehaviour
{
    //Called by the Exit Button's OnClick
    public void ExitLevelSignal()
    {
        Signal.Send("QuitConfirmation", "Popup");
    }
}
