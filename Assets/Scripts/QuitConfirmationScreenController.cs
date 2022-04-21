using UnityEngine;
using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Containers;

public class QuitConfirmationScreenController : MonoBehaviour
{
    //NOTES:    To add custom pause/exit behavior to certain games
    //          just add a stream listener for the popup/return to game streams

    private SignalReceiver  quitconfirmation_popup_receiver;
    private SignalStream    quitconfirmation_popup_stream;

    private void Awake()
    {
        quitconfirmation_popup_stream = SignalStream.Get("QuitConfirmation", "Popup");

        quitconfirmation_popup_receiver = new SignalReceiver().SetOnSignalCallback(ShowExitPopup);
    }

    private void OnEnable()
    {
        quitconfirmation_popup_stream.ConnectReceiver(quitconfirmation_popup_receiver);
    }

    private void OnDisable()
    {
        quitconfirmation_popup_stream.DisconnectReceiver(quitconfirmation_popup_receiver);
    }

    //Called by the QUitConfirmation Screen's Return to Game Button's OnClick
    public void ReturnToGame()
    {
        GetComponent<UIView>().Hide();

        Signal.Send("QuitConfirmation", "BackToGame");

        AudioManager.instance.Play("Button Click");
    }

    //Called by the Quit Confirmation Screen's Exit Button's OnClick
    //Each mode with a quit confirmation will handle ending the game by listening for this signal
    public void ExitLevel()
    {
        GetComponent<UIView>().Hide();

        Signal.Send("QuitConfirmation", "ExitLevel");

        AudioManager.instance.Play("Button Click");
    }

    private void ShowExitPopup(Signal signal)
    {
        GetComponent<UIView>().Show();
    }
}
