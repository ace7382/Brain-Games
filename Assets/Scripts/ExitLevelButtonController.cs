using UnityEngine;
using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Containers;
using Doozy.Runtime.UIManager.Components;

public class ExitLevelButtonController : MonoBehaviour
{
    private SignalReceiver  gamemanagement_disableexitlevelbutton_receiver;
    private SignalStream    gamemanagement_disableexitlevelbutton_stream;

    private void Awake()
    {
        gamemanagement_disableexitlevelbutton_stream    = SignalStream.Get("GameManagement", "DisableExitLevelButton");

        gamemanagement_disableexitlevelbutton_receiver  = new SignalReceiver().SetOnSignalCallback(SetInteractable);
    }

    private void OnEnable()
    {
        gamemanagement_disableexitlevelbutton_stream.ConnectReceiver(gamemanagement_disableexitlevelbutton_receiver);
    }

    private void OnDisable()
    {
        gamemanagement_disableexitlevelbutton_stream.DisconnectReceiver(gamemanagement_disableexitlevelbutton_receiver);
    }

    //Called by the Exit Button's OnClick
    public void ExitLevelSignal()
    {
        Signal.Send("QuitConfirmation", "Popup");
    }

    private void SetInteractable(Signal signal)
    {
        //Signal Data should be bool
        //  bool - Interactability of ExitLevel Button

        GetComponent<UIButton>().interactable = signal.GetValueUnsafe<bool>();
    }
}
